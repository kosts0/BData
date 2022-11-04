using AnalData.Models;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;
using Nest;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AnalData
{
    public class MongoElasticConector
    {
        IMongoDatabase Db;
        ElasticClient client;
        public MongoElasticConector(IMongoDatabase db)
        {
            Db = db;
            var settings = new ConnectionSettings(new Uri("http://localhost:9200"))
                .CertificateFingerprint("94:75:CE:4F:EB:05:32:83:40:B8:18:BB:79:01:7B:E0:F0:B6:C3:01:57:DB:4D:F5:D8:B8:A6:BA:BD:6D:C5:C4")
                .EnableApiVersioningHeader()
                .DisableDirectStreaming(true);
            client = new ElasticClient(settings);
        }
        public MongoElasticConector()
        {
            string connectionString = "mongodb://localhost:27017";
            MongoClient dbClient = new(connectionString);
            Db = dbClient.GetDatabase("Test");
            var settings = new ConnectionSettings(new Uri("http://localhost:9200"))
                .CertificateFingerprint("94:75:CE:4F:EB:05:32:83:40:B8:18:BB:79:01:7B:E0:F0:B6:C3:01:57:DB:4D:F5:D8:B8:A6:BA:BD:6D:C5:C4")
                .EnableApiVersioningHeader()
                .DisableDirectStreaming(true);
            client = new ElasticClient(settings);
        }


        //public IMongoClient<BsonDocument> MongoCollection(string collectionName) => Db.GetCollection<BsonDocument>("")
        public void  CopySubmissionsAsync()
        {
            var submissionCollection = Db.GetCollection<BsonDocument>("ContestStatus");  

            FindOptions<BsonDocument> findOptions = new FindOptions<BsonDocument>() { ShowRecordId=false };
            client.Indices.Delete("submission_index");
            var response2 = client.Indices.Create("submission_index_timeCheck",
                            index => index.Map<Submission>(
                                x => x.AutoMap()));
            using (var cursor = submissionCollection.FindAsync(new BsonDocument() { { "author.participantType", "CONTESTANT" } }).Result)
            {
                while (cursor.MoveNext())
                {
                    var subm = cursor.Current.ToList();
                    List<Submission> summ2 = subm.Select(s => new Submission(s)).ToList();
                    var response = client.IndexMany<Submission>(summ2, "submission_index");
                    if (!response.IsValid)
                    {
                        throw new Exception(response.DebugInformation);
                    }
                }
            }
            
        }
    }
}
