#!/bin/bash

# az --version

storageAccount='strsample'
storageAccountFunction='strfunctionmm01'
resourceGroup='eval.table'
tableName='Sample'
functionName='SampleFunctionMM01'

resourceGroupExists=$(az group list --query "[?name=='$resourceGroup'].name" -o tsv)
if [ -z "$resourceGroupExists"  ];
then
    echo 'No ResourceGroup'
    az group create --location westeurope --name $resourceGroup
else 
    echo 'ResourceGroup exists'
fi

storageAccountFunctionExists=$(az storage account list --resource-group $resourceGroup --query "[?name=='$storageAccountFunction'].name" -o tsv)
if [ -z "$storageAccountFunctionExists"  ];
then
    echo 'No StorageAccount Function'
    az storage account create --name $storageAccountFunction --resource-group $resourceGroup --location westeurope --sku Standard_LRS
else 
    echo 'Storage Function account exists'
fi

storageAccountExists=$(az storage account list --resource-group $resourceGroup --query "[?name=='$storageAccount'].name" -o tsv)
if [ -z "$storageAccountExists"  ];
then
    echo 'No StorageAccount'
    az storage account create --name $storageAccount --resource-group $resourceGroup --location westeurope --sku Standard_LRS
    az storage table create --name $tableName --account-name $storageAccount
else 
    echo 'Storage account exists'
fi

functionExists=$(az functionapp list --resource-group $resourceGroup --query "[?name=='$functionName'].name" -o tsv)
if [ -z "$functionExists"  ];
then
    echo 'No Function'
    az appservice plan create --resource-group $resourceGroup --name 'samplePlan' 
    az functionapp create --name $functionName --resource-group $resourceGroup --storage-account $storageAccountFunction --os-type Linux --functions-version 4 --plan 'samplePlan'

    az functionapp identity assign --name $functionName --resource-group $resourceGroup
    functionManagedId=$(az functionapp identity show --name $functionName --resource-group $resourceGroup --query principalId -o tsv)
    
    # Storage Table
    storageAccountId=$(az storage account list --resource-group $resourceGroup --query "[?name=='$storageAccount'].id" -o tsv)
    az role assignment create --role "Storage Table Data Contributor" --scope $storageAccountId --assignee-principal-type ServicePrincipal --assignee-object-id $functionManagedId

    #AzureWebJobsStorage
    storageAccountFunctionId=$(az storage account list --resource-group $resourceGroup --query "[?name=='$storageAccountFunction'].id" -o tsv)
    az role assignment create --role "Storage Blob Data Contributor" --scope $storageAccountFunctionId --assignee-principal-type ServicePrincipal --assignee-object-id $functionManagedId
else 
    echo 'Function exists'
fi

tableEndpoint="https://$storageAccount.table.core.windows.net"
az functionapp config appsettings set --name $functionName --resource-group $resourceGroup --settings SampleTableConnection__endpoint=$tableEndpoint

#AzureWebJobsStorage
az functionapp config appsettings delete --name $functionName --resource-group $resourceGroup --setting-names "AzureWebJobsStorage"
az functionapp config appsettings set --name $functionName --resource-group $resourceGroup --settings AzureWebJobsStorage__accountName=${storageAccountFunction}

cd SampleTableService
func azure functionapp publish $functionName
cd ..