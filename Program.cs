using Azure.Identity;
using Microsoft.Azure.Cosmos;

// Set up authentication and authorization with Azure managed identity
var credential = new DefaultAzureCredential(new DefaultAzureCredentialOptions());
var options = new CosmosClientOptions
{
    SerializerOptions = new CosmosSerializationOptions
    {
        PropertyNamingPolicy = CosmosPropertyNamingPolicy.CamelCase,
        IgnoreNullValues = true
    }
};

// Set up the Cosmos client
var endpointUri = "https://emea-servicedata-logic-qa.documents.azure.com:443/";
var client = new CosmosClient(endpointUri, credential, options);

// Set up the container
var databaseName = "DATABASE_NAME";
var containerName = "CONTAINER_NAME";  
var database = client.GetDatabase(databaseName);
var container = database.GetContainer(containerName);

// Delete a record
var id = "DOCUMENT_ID";
var partitionKey = new PartitionKey("DOCUMENT_PARTITION_KEY");
var result = await container.DeleteItemAsync<dynamic>(id, partitionKey);

// Check the status of the operation
if (result.StatusCode == System.Net.HttpStatusCode.NoContent)
{
    Console.WriteLine("Record deleted successfully");
}
else
{
    Console.WriteLine($"Failed to delete record. Status code: {result.StatusCode}");
}