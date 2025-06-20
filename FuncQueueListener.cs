using System;
using System.Text;
using System.Text.Json;
using Azure.Storage.Queues;
using Azure.Storage.Queues.Models;
using FuncAppShortCircuitSvc.Models;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace FuncAppShortCircuitSvc
{
    public class FuncQueueListener
    {
        private readonly ILogger<FuncQueueListener> _logger;
        private readonly IConfiguration _configuration;

        public FuncQueueListener(ILogger<FuncQueueListener> logger, IConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;
        }

        [Function(nameof(FuncQueueListener))]
        public async Task Run(
        [QueueTrigger("%QueueName%", Connection = "StorageConnection")] QueueMessage queueMessage)
        {
            _logger.LogInformation($"C# Queue trigger function processed: {queueMessage.MessageText}");

            // Get the connection string from configuration
            string? connectionString = _configuration.GetValue<string>("StorageConnection");
            if (string.IsNullOrEmpty(connectionString))
            {
                _logger.LogError("StorageConnection is not configured or is null/empty.");
                throw new InvalidOperationException("StorageConnection configuration is missing.");
            }
            string? subscriptionId = _configuration.GetValue<string>("Azure:SubscriptionId");
            string? queueName = _configuration.GetValue<string>("QueueName");

            // Access the message conten
            string messageContent = queueMessage.MessageText;
            var shortCircuitMsg = new ShortCircuitMsg();
            _logger.LogInformation($"Message content: {messageContent}");
            try
            {
                shortCircuitMsg = JsonConvert.DeserializeObject<ShortCircuitMsg>(messageContent);
                _logger.LogInformation($"Deserialized message for function: {shortCircuitMsg!.FunctionAppName}/{shortCircuitMsg!.FunctionName}");

            }
            catch (Newtonsoft.Json.JsonException ex)
            {
                _logger.LogError(ex, "Failed to deserialize message content.");
                throw;
            }

            // Process the message to either disable or enable the function app.
            try
            {
                // Get the function app details
                var functionApp = await Utils.WebAppConfigurator.GetFunctionAsync(subscriptionId!, shortCircuitMsg.RessourceGroupName,
                    shortCircuitMsg.FunctionAppName, _logger);

                // set the function status based on the message
                await Utils.WebAppConfigurator.FunctionConditionAsync(functionApp, shortCircuitMsg.FunctionName,
                    shortCircuitMsg.DisableFunction, _logger);

                // If disable function is true, republish the message to the queue if the disable period is greater than 0
                if (shortCircuitMsg.DisableFunction && shortCircuitMsg.DisablePeriodMinutes > 0)
                {
                    _logger.LogInformation($"Function {shortCircuitMsg.FunctionName} is disabled for {shortCircuitMsg.DisablePeriodMinutes} minutes");
                    // Set the visibility timeout to the specified period
                    TimeSpan visibilityTimeout = TimeSpan.FromMinutes(shortCircuitMsg.DisablePeriodMinutes);
                    shortCircuitMsg.DisableFunction = false;

                    // Create a queue client
                    QueueClient queueClient = new QueueClient(connectionString, queueName);
                    await queueClient.CreateIfNotExistsAsync();

                    // Send the message with a custom visibility timeout
                    var messageOut = JsonConvert.SerializeObject(shortCircuitMsg);
                    string base64Message = Convert.ToBase64String(Encoding.UTF8.GetBytes(messageOut));
                    await queueClient.SendMessageAsync(base64Message, visibilityTimeout);
                    _logger.LogInformation($"Message republished to output queue with visibility timeout of {visibilityTimeout}");

                }
                else
                {
                    _logger.LogInformation($"Function {shortCircuitMsg.FunctionName} is enabled");
                }

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to process the message");
                throw;
            }

        }
    }
}
