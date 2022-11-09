using MongoDB.Driver;
using Nest;

namespace Common
{
    /// <summary>
    /// Обертка над коннектом к монге и elasticsearch
    /// </summary>
    public class MongoElasticConector
    {
        private static IMongoDatabase mongoDb;
        private static ElasticClient client;
        public MongoElasticConector Init(IMongoDatabase db)
        {
            mongoDb = db;
            var settings = new ConnectionSettings(new Uri("http://localhost:9200"))
                .CertificateFingerprint("94:75:CE:4F:EB:05:32:83:40:B8:18:BB:79:01:7B:E0:F0:B6:C3:01:57:DB:4D:F5:D8:B8:A6:BA:BD:6D:C5:C4")
                .EnableApiVersioningHeader()
                .DisableDirectStreaming(true);
            client = new ElasticClient(settings);
            return this;
        }
        public MongoElasticConector Init()
        {
            string connectionString = "mongodb://localhost:27017";
            MongoClient dbClient = new(connectionString);
            mongoDb = dbClient.GetDatabase("Test");
            var settings = new ConnectionSettings(new Uri("http://localhost:9200"))
                .CertificateFingerprint("94:75:CE:4F:EB:05:32:83:40:B8:18:BB:79:01:7B:E0:F0:B6:C3:01:57:DB:4D:F5:D8:B8:A6:BA:BD:6D:C5:C4")
                .EnableApiVersioningHeader()
                .DisableDirectStreaming(true);
            client = new ElasticClient(settings);
            return this;
        }
        /// <summary>
        /// Клиент кластера elasticsearch
        /// </summary>
        public ElasticClient ElasticClient
        {
            get
            {
                if (client == null)
                {
                    Init();
                }
                return client;
            }
        }
        /// <summary>
        /// Коннект к монге
        /// </summary>
        public IMongoDatabase MongoDb
        {
            get
            {
                if (mongoDb == null)
                {
                    Init();
                }
                return mongoDb;
            }
        }
    }
}