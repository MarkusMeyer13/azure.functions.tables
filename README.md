# Evaluation of WebJobs Table extension and Azure Function identity connections

## Table extensions

Restore the nuget package:

```bash
dotnet add package Microsoft.Azure.WebJobs.Extensions.Tables
```

[nuget.org: Microsoft.Azure.WebJobs.Extensions.Tables](https://www.nuget.org/packages/Microsoft.Azure.WebJobs.Extensions.Tables)

Information and samples about the tables extension:

[https://devblogs.microsoft.com/azure-sdk/whats-new-in-the-azure-functions-tables-extension-for-net-beta/](https://devblogs.microsoft.com/azure-sdk/whats-new-in-the-azure-functions-tables-extension-for-net-beta/)

## Connections based on Identity

[Tutorial: Create a function app that connects to Azure services using identities instead of secrets](https://docs.microsoft.com/en-us/azure/azure-functions/functions-identity-based-connections-tutorial)

## Code in the repository

- Azure Function for adding and deleting table data with ```TableClient```.
- Unit tests for adding and deleting table data with ```TableClient```.
- Deployment of Azure Function and resources for using managed identity. 

## More links

[Quickstart: Create a C# function in Azure from the command line](https://docs.microsoft.com/en-us/azure/azure-functions/create-first-function-cli-csharp?tabs=azure-cli%2Cin-process)