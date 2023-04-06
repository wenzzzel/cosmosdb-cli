using Azure.Identity;
using cosmosdb_cli.Models;
using Microsoft.Azure.Cosmos;

/*********
* SETUP *
*********/
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
var endpointUri = "ENPOINT_URI";
var client = new CosmosClient(endpointUri, credential, options);

// Set up the container
var databaseName = "DATABASE_NAME";
var containerName = "CONTAINER_NAME";
var database = client.GetDatabase(databaseName);
var container = database.GetContainer(containerName);


/************************
 * DELETE SINGLE RECORD *
 ************************/
// TODO: control with parameters if below method is run or not
// Delete a record
var deleteResult = await DeleteItemAsync(
    container,
    "DRU2711~1400001542",
    "YV1BZ8256C1135673");


/*********************************************
 * DELETE MANY RECORDS BASED ON QUERY RESULT *
 *********************************************/
// TODO: Control with parameters if below method is run or not
// Fetch selection with sql query
var queryResult = await GetItemsBySQLAsync<SampleModel>(
    "SELECT * FROM c where 1 = 1",
    client,
    database,
    container);

var datas = new List<(string id, string partitionKey)>();

foreach (var item in queryResult)
    datas.Add((item.Id, item.PartitionKey));

// Delete all fetched records
await DeleteItemsAsync(
    container,
    datas);
async Task<IEnumerable<T>> GetItemsBySQLAsync<T>(string sqlQuery, CosmosClient client, Database database, Container container)
{
    var query = new QueryDefinition(sqlQuery);

    var requestOptions = new QueryRequestOptions()
    {
        MaxItemCount = -1,
        ResponseContinuationTokenLimitInKb = 3
    };

    FeedIterator<T> resultSet = client.GetContainer(database.Id, container.Id)
        .GetItemQueryIterator<T>(query, requestOptions: requestOptions);

    List<T> items = new List<T>();
    while (resultSet.HasMoreResults)
    {
        items.AddRange(await resultSet.ReadNextAsync());
    }
    return items;
}

async Task<ItemResponse<dynamic>> DeleteItemAsync(Container container, string id, string partitionKey)
{
    var result = await container.DeleteItemAsync<dynamic>(id, new PartitionKey(partitionKey));
    Console.WriteLine($"Deleted item with id: {id} and partitionKey {partitionKey}");
    return result;
}

async Task DeleteItemsAsync(Container container, List<(string id, string partitionKey)> items)
{
    foreach (var (id, partitionKey) in items)
        await DeleteItemAsync(container, id, partitionKey);

    return;
}