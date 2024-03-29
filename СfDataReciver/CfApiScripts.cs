﻿using Common.Models;
using HtmlAgilityPack;
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
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace СfDataReciver
{
    public class CfApiScripts
    {
        public IMongoDatabase Db { get; set; }
        const string codeforcesApiUrl = "https://codeforces.com/api/";
        public IMongoCollection<BsonDocument> ContestStatus
        {
            get
            {
                return Db.GetCollection<BsonDocument>("ContestStatus");
            }
        }
        /// <summary>
        /// Название коллекции в MongoDb, куда записывать данные полученные по Api
        /// </summary>
        private string WriteCollection { get; set; } = "ContestStatus";
        /// <summary>
        /// Список контестов, по которым получена информация. Формируется из лог файла, куда записываются Id контеста, по завершению стягивания информации
        /// </summary>
        public List<long> LoadedContestList { get; private set; }
        public List<ProxyInfo> ProxyInfoList { get; set; } 
        int currentProxyIndex = 0;
        public ProxyInfo CurrentProxy => ProxyInfoList[currentProxyIndex];
        public CfApiScripts()
        {
            string connectionString = "mongodb://localhost:27017";
            MongoClient dbClient = new(connectionString);
            Db = dbClient.GetDatabase("Test");
            LoadedContestList = UsedIdList();
            globalClient = new HttpClient(new HttpClientHandler()
            {
                UseCookies = true,
                AllowAutoRedirect = false
            });
        }
        public CfApiScripts(string writeCollection) : this()
        {
            WriteCollection = writeCollection;
        }
        private List<long> UsedIdList(string filePath = null)
        {
            filePath ??= Path.Combine(Environment.CurrentDirectory, "UsedContests.txt");
            List<long> usedIdList = new();
            using (FileStream fstream = File.OpenRead(filePath))
            {
                // выделяем массив для считывания данных из файла
                byte[] buffer = new byte[fstream.Length];
                // считываем данные
                fstream.Read(buffer, 0, buffer.Length);
                // декодируем байты в строку
                string textFromFile = Encoding.Default.GetString(buffer);
                usedIdList = textFromFile.Split('\n').Where(l => Regex.IsMatch(l, @"(\d+)")).Select(l => Convert.ToInt64(Regex.Match(l, @"(\d+)").Value)).ToList();
                Console.WriteLine($"Контесты с информацией: \n{textFromFile}");
            }
            return usedIdList;
        }
        public List<long> UpdateUsedIdList(string filePath, List<long> ContestList)
        {
            List<long> usedIdList = new();
            using (FileStream fstream = File.OpenRead(filePath))
            {
                // выделяем массив для считывания данных из файла
                byte[] buffer = new byte[fstream.Length];
                fstream.Read(buffer, 0, buffer.Length);
                string textFromFile = Encoding.Default.GetString(buffer);
                usedIdList = textFromFile.Split('\n').Where(l => Regex.IsMatch(l, @"(\d+)")).Select(l => Convert.ToInt64(Regex.Match(l, @"(\d+)").Value)).ToList();
                Console.WriteLine($"Контесты с информацией: \n{textFromFile}");
            }
            ContestList = usedIdList;
            return usedIdList;
        }
        List<WebProxy?> proxyList => new List<WebProxy?>()
        {
            new WebProxy() {Address = new Uri(@"http://220.179.219.46:8089")},
            new WebProxy() {Address = new Uri(@"http://218.64.139.73:7890")},
            new WebProxy() {Address = new Uri(@"http://183.172.235.227:7891")},
            null
        };
        static Random rnd = new Random();

        public async Task<JObject> GetJsonRequest(string url)
        {
            JObject getRequest()
            {
                HttpClient client = GetClient(rnd.Next()%ProxyInfoList.Count);
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
        async void RefreshUsersCollection()
        {
            Db.DropCollection("Users");
            var collection = Db.GetCollection<BsonDocument>("Users");
            var result = await GetJsonRequest(codeforcesApiUrl + "user.ratedList?activeOnly=false&includeRetired=true");
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
        async void RefreshProblemsArchive()
        {
            Db.DropCollection("Archive");
            var collection = Db.GetCollection<BsonDocument>("Archive");
            var result = await GetJsonRequest(codeforcesApiUrl + "problemset.problems?lang=ru");
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
        async void RefreshContestArchive()
        {
            Db.DropCollection("ContestList");
            var collection = Db.GetCollection<BsonDocument>("ContestList");
            var result = await GetJsonRequest(codeforcesApiUrl + "contest.list?gym=false");
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
        public async Task<List<BsonDocument>> GetContestSolutionList(long contestId, int startIndex, int count)
        {
            List<BsonDocument> contestSolutionList = new();
            var result = await GetJsonRequest(codeforcesApiUrl + $"contest.status?contestId={contestId}&from={startIndex}&count={count}&lang=ru");
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
        public async Task RefreshContestSolutionList()
        {
            var contestList = Db.GetCollection<BsonDocument>("ContestList");
            var contestStatusCollection = Db.GetCollection<BsonDocument>("ContestStatus");
            var usedList = UsedIdList();
            //Parallel.ForEach<BsonDocument>(contestList.Find(new BsonDocument() { { "phase", "FINISHED" } }).ToList(), WriteSolutionCollection);
            foreach (var contest in contestList.Find(new BsonDocument() { { "phase", "FINISHED" } }).ToList())
            {
                if (usedList.Contains(contest["id"].AsInt64)) continue;
                WriteSolutionCollection(contest["id"].AsInt64);
            }
        }
        public async Task WriteSolutionCollection(long contestId, int batchSize = 1500)
        {
            int startIndex = 1;
            var contestStatusCollection = Db.GetCollection<BsonDocument>(WriteCollection);
            bool breakCondition = true;
            List<BsonDocument> currentSolutionList = new();
            do
            {
                currentSolutionList = await GetContestSolutionList(contestId, startIndex, batchSize);
                try
                {
                    await contestStatusCollection.InsertManyAsync(currentSolutionList, new InsertManyOptions() { IsOrdered = false });
                }
                catch(MongoBulkWriteException)
                {

                }
                breakCondition = currentSolutionList.Count != batchSize;
                startIndex+= batchSize;
            } while (!breakCondition);
            Console.WriteLine($"В контесте {contestId} добавлено {startIndex + currentSolutionList.Count} записей о попытках");
            using (StreamWriter writer = new StreamWriter(@"C:\Users\skld0\source\repos\AnalData\СfDataReciver\UsedContests.txt", true))
            {
                await writer.WriteLineAsync($"В контесте {contestId} добавлено {startIndex + currentSolutionList.Count} записей о попытках");
            }
        }
        HttpClient globalClient { get; set; }
        
        private void ChangeClient()
        {
            currentProxyIndex++;
            currentProxyIndex = currentProxyIndex % ProxyInfoList.Count;
            globalClient = GetClient(currentProxyIndex);
            
        }
        public HttpClient GetClient(int proxyInfoIndex)
        {
            var proxyInfo = ProxyInfoList[proxyInfoIndex];
            HttpClient httpClient = new();
            if (string.IsNullOrEmpty(proxyInfo.IpPort))
            {
                httpClient = new HttpClient(new HttpClientHandler()
                {
                    AllowAutoRedirect = false,
                    UseProxy = true,
                });
            }
            else
            {
                IWebProxy proxy = new WebProxy(proxyInfo.IpPort)
                {
                    Credentials = new NetworkCredential(proxyInfo.UserName, proxyInfo.Password)
                };
                var handler = new HttpClientHandler()
                {
                    AllowAutoRedirect = false,
                    UseCookies = true,
                    Proxy = proxy,
                };
                httpClient = new HttpClient(handler);
            }
            return httpClient;
        }
        public string GetSolutionCodeText(string contestId, string solutionId)
        {
            string url = $"https://codeforces.com/contest/{contestId}/submission/{solutionId}";
            
            async Task<string> getRequest()
            {
                HttpRequestMessage httpRequestMessage = new HttpRequestMessage(HttpMethod.Get, url);
                httpRequestMessage.Headers.Add("cookie", ProxyInfoList[currentProxyIndex].Cookie);
                var responce = globalClient.Send(httpRequestMessage);
                if(responce.StatusCode == HttpStatusCode.Redirect)
                {
                    ChangeClient();
                    Thread.Sleep(TimeSpan.FromSeconds(rnd.Next()%4));
                }
                var responceContent = await responce.Content.ReadAsStringAsync();
                return responceContent;
            }
            
            bool succesed = false;
            while (!succesed)
            {

                try
                {
                    var documentText = getRequest();
                    HtmlDocument document = new();
                    document.LoadHtml(documentText.Result);

                    var result = document.DocumentNode.SelectSingleNode("//*[@id='program-source-text']")?.InnerText;
                    if(result == null)
                    {
                        continue;
                    }
                    return result;
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    Thread.Sleep(TimeSpan.FromSeconds(0.5));
                    continue;
                }
            }
            throw new Exception("Не удалось получить код решения");
        }
    }
}
