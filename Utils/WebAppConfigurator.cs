using Azure.Identity;
using Azure.ResourceManager;
using Azure.ResourceManager.AppService;
using Azure.ResourceManager.Resources;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FuncAppShortCircuitSvc.Utils
{

    /// <summary>
    /// This requires IAM reader permissions on the resource group and IAM Website contributor permissions on the 
    /// function app for this function app's managed identity.
    /// </summary>
    internal static class WebAppConfigurator
    {
        internal static async Task<WebSiteResource> GetFunctionAsync(string subscriptionId, string resourceGroupName, string functionAppName, ILogger logger)
        {
            logger.LogInformation($"Getting function app {functionAppName} in resource group {resourceGroupName} under subscription {subscriptionId}");
            var client = new ArmClient(new DefaultAzureCredential());

            try
            {
                logger.LogDebug("Creating subscription resource identifier...");
                var subscription = client.GetSubscriptionResource(
                    SubscriptionResource.CreateResourceIdentifier(subscriptionId));

                logger.LogDebug("Fetching resource group...");
                var resourceGroup = await subscription.GetResourceGroupAsync(resourceGroupName);
                if (resourceGroup == null || resourceGroup.Value == null)
                {
                    logger?.LogError($"Resource group '{resourceGroupName}' not found");
                    throw new InvalidOperationException($"Resource group '{resourceGroupName}' not found");
                }

                logger.LogDebug("Fetching function app...");
                var functionApp = await resourceGroup.Value.GetWebSiteAsync(functionAppName);
                if (functionApp == null || functionApp.Value == null)
                {
                    logger?.LogError($"Function app '{functionAppName}' not found");
                    throw new InvalidOperationException($"Function app '{functionAppName}' not found");
                }

                logger.LogInformation("Successfully retrieved function app");
                return functionApp;
            }
            catch (Exception ex) when (ex is not InvalidOperationException)
            {
                logger?.LogError(ex, $"Unexpected error getting app {functionAppName}");
                throw;
            }
        }


        internal static async Task FunctionConditionAsync(WebSiteResource webApp, string functionName, bool disableFunction, ILogger logger)
        {
            logger?.LogInformation($"Setting function {functionName} monitor to {disableFunction}");
            try
            {
                // Get existing app settings
                var config = await webApp.GetApplicationSettingsAsync();
                var appSettings = config.Value;

                if( disableFunction)
                {
                    // Disable the function
                    appSettings.Properties[$"AzureWebJobs.{functionName}.Disabled"] = "true";
                }
                else
                {
                    // Enable the function
                    appSettings.Properties[$"AzureWebJobs.{functionName}.Disabled"] = "false";
                }
                // Apply the changes
                await webApp.UpdateApplicationSettingsAsync(appSettings);
            }
            catch (Exception ex)
            {
                logger?.LogError(ex, $"Error disabling function {functionName}");
                throw;
            }
        }



    }
}
