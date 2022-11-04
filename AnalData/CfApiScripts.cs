using MongoDB.Bson;
using MongoDB.Bson.IO;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;
using MongoDB.Driver;
using Newtonsoft.Json.Bson;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace AnalData
{
    public class CfApiScripts
    {
        IMongoDatabase Db;
        const string codeforcesApiUrl = "https://codeforces.com/api/";
        List<long> usedIdList = new();

        public CfApiScripts()
        {
            string connectionString = "mongodb://localhost:27017";
            MongoClient dbClient = new(connectionString);
            Db = dbClient.GetDatabase("Test");
            using (FileStream fstream = File.OpenRead(@"C:\Users\skld0\source\repos\AnalData\AnalData\UsedContests.txt"))
            {
                // выделяем массив для считывания данных из файла
                byte[] buffer = new byte[fstream.Length];
                // считываем данные
                fstream.Read(buffer, 0, buffer.Length);
                // декодируем байты в строку
                string textFromFile = Encoding.Default.GetString(buffer);
                usedIdList = textFromFile.Split('\n').Select(l => Convert.ToInt64(Regex.Match(l, @"(\d+)").Value)).ToList();
                Console.WriteLine($"Текст из файла: {textFromFile}");
            }
        }
        JObject GetJsonRequest(string url, string contestId = null)
        {
            JObject getRequest()
            {
                HttpClient client = new HttpClient();
                var responce = client.GetAsync(url).Result;
                var responceResult = responce.Content.ReadAsStringAsync().Result;
                return Newtonsoft.Json.JsonConvert.DeserializeObject<JObject>(responceResult);
            }
            bool succesed = false;
            JObject result = new JObject();
            while (!succesed)
            {

                try
                {
                    result = getRequest();
                    succesed = result != null;
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    Thread.Sleep(TimeSpan.FromSeconds(0.5));
                    continue;
                }
            }

            return result;
        }


        BsonDocument FromJsonToBson(JToken json)
        {
            using var stream = new MemoryStream(); // Or open a FileStream if you prefer
            using (var writer = new BsonDataWriter(stream) { CloseOutput = false })
            {
                json.WriteTo(writer);
            }
            // Reset the stream position to 0 if you are going to immediately re-read it.
            stream.Position = 0;
            BsonDocument bsonData;
            using (var reader = new BsonBinaryReader(stream))
            {
                var context = BsonDeserializationContext.CreateRoot(reader);
                bsonData = BsonDocumentSerializer.Instance.Deserialize(context);
            }
            var newJson = bsonData.ToJson();
            return bsonData;
        }
        /// <summary>
        /// Обновить коллекцию пользвателей
        /// </summary>
        void RefreshUsersCollection()
        {
            Db.DropCollection("Users");
            var collection = Db.GetCollection<BsonDocument>("Users");
            var result = GetJsonRequest(codeforcesApiUrl + "user.ratedList?activeOnly=false&includeRetired=true");
            foreach (var item in result.SelectToken("result").Children())
            {
                BsonDocument bsonElements = FromJsonToBson(item);
                bsonElements["_id"] = bsonElements["handle"];
                collection.InsertOne(bsonElements);
            }
            Console.WriteLine($"{result.SelectToken("result").Children().Count()} documents inserted");
        }
        /// <summary>
        /// Обновить коллекцию архива задач
        /// </summary>
        void RefreshProblemsArchive()
        {
            Db.DropCollection("Archive");
            var collection = Db.GetCollection<BsonDocument>("Archive");
            var result = GetJsonRequest(codeforcesApiUrl + "problemset.problems?lang=ru");
            foreach (var item in result.SelectToken("$..problems").Children())
            {
                BsonDocument bson = FromJsonToBson(item);
                bson["_id"] = new BsonString(bson["contestId"].AsNullableInt64 + bson["index"].AsString);
                collection.InsertOne(bson);
                Console.WriteLine($"{bson["contestId"].AsNullableInt64} {bson["index"].AsString} inserted");
            }
        }

        /// <summary>
        /// Обновить коллекцию контестов
        /// </summary>
        void RefreshContestArchive()
        {
            Db.DropCollection("ContestList");
            var collection = Db.GetCollection<BsonDocument>("ContestList");
            var result = GetJsonRequest(codeforcesApiUrl + "contest.list?gym=false");
            foreach (var item in result.SelectToken("result").Children())
            {
                BsonDocument bson = FromJsonToBson(item);
                bson["_id"] = bson["id"];
                collection.InsertOne(bson);
                Console.WriteLine($"{bson["id"]} added");
            }
        }

        /// <summary>
        /// Получить список попыток по контесту
        /// </summary>
        List<BsonDocument> GetContestSolutionList(long contestId, int startIndex, int count)
        {
            List<BsonDocument> contestSolutionList = new();
            var result = GetJsonRequest(codeforcesApiUrl + $"contest.status?contestId={contestId}&from={startIndex}&count={count}&lang=ru", contestId: contestId.ToString());
            if (result.ContainsKey("comment") && result["comment"].ToString() == $"contestId: Contest with id {contestId} not found")
            {
                return contestSolutionList;
            }
            foreach (var item in result.SelectToken("result").Children())
            {
                BsonDocument bson = FromJsonToBson(item);
                bson["_id"] = bson["id"];
                contestSolutionList.Add(bson);
            }
            return contestSolutionList;
        }
        /// <summary>
        /// Обновить коллекцию попыток
        /// </summary>
        async Task RefreshContestSolutionList()
        {
            //db.DropCollection("ContestStatus");
            var contestList = Db.GetCollection<BsonDocument>("ContestList");
            var contestStatusCollection = Db.GetCollection<BsonDocument>("ContestStatus");
            //Parallel.ForEach<BsonDocument>(contestList.Find(new BsonDocument() { { "phase", "FINISHED" } }).ToList(), WriteSolutionCollection);
            foreach (var contest in contestList.Find(new BsonDocument() { { "phase", "FINISHED" } }).ToList())
            {
                if (usedIdList.Contains(contest["id"].AsInt64)) continue;
                WriteSolutionCollection(contest);
            }
        }
        async void WriteSolutionCollection(BsonDocument contestDocument)
        {
            int startIndex = 1;
            int step = 500;
            var contestStatusCollection = Db.GetCollection<BsonDocument>("ContestStatus");
            bool breakCondition = true;
            List<BsonDocument> currentSolutionList = new();
            do
            {
                currentSolutionList = GetContestSolutionList(contestDocument["id"].AsInt64, startIndex, step);
                contestStatusCollection.InsertManyAsync(currentSolutionList, new InsertManyOptions() { IsOrdered = false });
                breakCondition = currentSolutionList.Count != step;
                startIndex+= step;
            } while (!breakCondition);
            Console.WriteLine($"В контесте {contestDocument["id"].AsInt64} добавлено {startIndex + currentSolutionList.Count} записей о попытках");
            using (StreamWriter writer = new StreamWriter(@"C:\Users\skld0\source\repos\AnalData\AnalData\UsedContests.txt", true))
            {
                await writer.WriteLineAsync($"\nВ контесте {contestDocument["id"].AsInt64} добавлено {startIndex + currentSolutionList.Count} записей о попытках");
            }

        }
    }
}
