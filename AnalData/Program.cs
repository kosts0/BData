using Common;

/// <summary>
/// Исполняемый скрипт
/// </summary>
async Task MainScriptAsync()
{
    MongoElasticConector mongoElasticConector = new();
    await mongoElasticConector.CopySubmissionsAsync();
}
 await MainScriptAsync();