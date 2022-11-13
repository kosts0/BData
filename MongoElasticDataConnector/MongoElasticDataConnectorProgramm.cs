using Common;
using Common.Models;
using MongoDB.Bson;
using MongoDB.Driver;
using Nest;

int? batchSize, limit = null;
if(Environment.GetCommandLineArgs().Count() > 2)
{
    batchSize = Convert.ToInt32(Environment.GetCommandLineArgs()[1]);
    limit = Convert.ToInt32(Environment.GetCommandLineArgs()[2]);
}else if(Environment.GetCommandLineArgs().Count() > 1)
{
    batchSize=Convert.ToInt32(Environment.GetCommandLineArgs()[1]);
    Console.WriteLine("No limit argument");
}
else
{
    batchSize = 500;
    Console.WriteLine($"No batchSize Argument, defaultValue is {batchSize}");
    Console.WriteLine("No limit argument");
}


Console.WriteLine($"Convert with batchSize {batchSize} and limit {(limit == null ? "null" : limit)} started");
var connector = new MongoElasticConector();
var submissionCollection = connector.MongoDb.GetCollection<BsonDocument>("ContestStatus");

FindOptions<BsonDocument> findOptions = new FindOptions<BsonDocument>() { ShowRecordId=false };

if (Environment.GetCommandLineArgs().Count() > 1)
{
    batchSize = Convert.ToInt32(Environment.GetCommandLineArgs()[1]);
}
string indexName = $"submission_index_master";
connector.ElasticClient.Indices.Delete(indexName);
var response2 = connector.ElasticClient.Indices.Create(indexName,
                    index => index.Map<SubmissionWithUsers>(
                        x => x.AutoMap()));
using (var cursor = await submissionCollection.FindAsync(new BsonDocument() { { "author.participantType", "CONTESTANT" } }))
{
    while (cursor.MoveNext())
    {
        var user = await 
        List<SubmissionWithUsers> subm = cursor.Current.Select(s => new SubmissionWithUsers(s) { User = connector}).ToList();
        var response = connector.ElasticClient.IndexMany(subm, indexName);
        if (!response.IsValid)
        {
            throw new Exception(response.DebugInformation);
        }
    }
}