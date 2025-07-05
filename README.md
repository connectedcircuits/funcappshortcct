# FuncTriggerManager - Azure Function App Circuit Breaker Service

## Overview

FuncTriggerManager is an Azure Function App that provides a circuit breaker pattern implementation for other Azure Function Apps. It allows you to programmatically enable or disable function triggers, which is especially useful when downstream services are unavailable or experiencing issues.

## Purpose

The primary purpose of this service is to temporarily disable Azure Function App triggers when:

- A downstream service is unavailable
- Rate limits have been exceeded
- System maintenance is being performed
- You need to prevent processing of new events during incident recovery

After a specified period, the function automatically re-enables the trigger, resuming normal operation. Or if no reenablement period has been given, the function app trigger will be permittly disabled.

## How It Works

1. The service listens to a storage queue for messages containing function management instructions
2. When a message is received, it:
   - Parses the function details and action required (enable/disable)
   - Uses the Azure Resource Manager APIs to modify the function app settings
   - If disabling a function, it can automatically re-enable it after a specified period

## Message Structure

Messages sent to the queue should follow this structure:
```json
{
  "FunctionAppName": "YourFunctionAppName",
  "FunctionName": "SpecificFunctionName",
  "RessourceGroupName": "YourResourceGroupName",
  "DisableFunction": true,
  "DisablePeriodMinutes": 30
}
```

## Prerequisites

The function app's managed identity requires:
- IAM Reader permissions on the resource group
- IAM Website Contributor permissions on the target function apps

## Configuration Settings

Required application settings:
- `StorageConnection`: Connection string to the Azure Storage Account
- `QueueName`: Name of the queue to monitor
- `AzureSubscriptionId`: ID of the Azure subscription containing the function apps to control
