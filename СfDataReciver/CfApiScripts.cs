using Common.Models;
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
        public List<ProxyInfo> ProxyInfoList { get; set; } = new List<ProxyInfo>()
        {
            new ProxyInfo()
            {
                IpPort = null,
                UserName = null,
                Password = null,
                Cookie = "__utmz=71512449.1666293093.1.1.utmcsr=(direct)|utmccn=(direct)|utmcmd=(none); X-User-Sha1=58c631f6b588d4c6ed918439c9f721cc9b4332c2; nocturne.language=ru; 70a7c28f3de=l8xak2mi47iwh6nlc0; RCPC=5fcb02770a13f0f39d02fa4a3ecf9398; JSESSIONID=1E100893D0FE81FEF76C0677457120D9-n1; 39ce7=CFXHg0aa; __utmc=71512449; __utma=71512449.365517011.1666293093.1668875452.1668884849.24; __utmb=71512449.9.10.1668884849",
            },
            new ProxyInfo()
            {
                IpPort = "5.101.64.106:19991",
                UserName = "67953366a8",
                Password = "73686a7a76",
                Cookie = "__utma=71512449.1471895420.1609518929.1614685249.1668891604.7; __utmz=71512449.1668891604.7.1.utmcsr=(direct)|utmccn=(direct)|utmcmd=(none); __utmt=1; RCPC=f77e01c986005bea445df6ec346432de; JSESSIONID=750660FFA529B53FDE3C55958B8F0869-n1; 39ce7=CFfGT0kS; __utmc=71512449; evercookie_png=tyulwrll6dotowgdg0; evercookie_etag=tyulwrll6dotowgdg0; evercookie_cache=tyulwrll6dotowgdg0; 70a7c28f3de=tyulwrll6dotowgdg0; __utmb=71512449.11.10.1668891604",
            },
            new ProxyInfo()
            {
                IpPort = "192.162.57.22:16175",
                UserName = "7d937ff993",
                Password = "673487e947",
                Cookie = "__utma=71512449.1471895420.1609518929.1614685249.1668891604.7; __utmz=71512449.1668891604.7.1.utmcsr=(direct)|utmccn=(direct)|utmcmd=(none); 70a7c28f3de=tyulwrll6dotowgdg0; RCPC=40eff7e6bfa7a79e2cad3e7862a9ec7d; JSESSIONID=CA4F101EDB9A59B126044F960B6B47D0-n1; 39ce7=CFggkdjj; __utmc=71512449; __utmt=1; __utmb=71512449.13.10.1668891604",
            },
            new ProxyInfo()
            {
                IpPort = "185.80.150.96:35790",
                UserName = "6a65e3d096",
                Password = "a745357ed0",
                Cookie = "__utma=71512449.1471895420.1609518929.1614685249.1668891604.7; __utmz=71512449.1668891604.7.1.utmcsr=(direct)|utmccn=(direct)|utmcmd=(none); 70a7c28f3de=tyulwrll6dotowgdg0; RCPC=9f1a5c762a0d7cb934e998baa78099b9; JSESSIONID=881E9719AE4E253D1238F566AE2266FF-n1; 39ce7=CFtNSkn5; __utmc=71512449; __utmt=1; __utmb=71512449.16.10.1668891604",
            },
            new ProxyInfo()
            {
                IpPort = "192.162.58.190:36496",
                UserName = "099e47360a",
                Password = "7e4e6909a4",
                Cookie = "__utma=71512449.1471895420.1609518929.1614685249.1668891604.7; __utmz=71512449.1668891604.7.1.utmcsr=(direct)|utmccn=(direct)|utmcmd=(none); 70a7c28f3de=tyulwrll6dotowgdg0; __utmt=1; RCPC=89a8c084572f40f050a297243c882153; JSESSIONID=BC931A390CDA750CEDD7062C677249EB-n1; 39ce7=CFMN2WzO; __utmc=71512449; __utmb=71512449.18.10.1668891604",
            }
        };
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
        private List<long> UsedIdList(string filePath = @"C:\Users\skld0\source\repos\AnalData\СfDataReciver\UsedContests.txt")
        {
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

        public JObject GetJsonRequest(string url)
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
        public List<BsonDocument> GetContestSolutionList(long contestId, int startIndex, int count)
        {
            List<BsonDocument> contestSolutionList = new();
            var result = GetJsonRequest(codeforcesApiUrl + $"contest.status?contestId={contestId}&from={startIndex}&count={count}&lang=ru");
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
                currentSolutionList = GetContestSolutionList(contestId, startIndex, batchSize);
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
            if (proxyInfo.IpPort == null)
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
                    Thread.Sleep(TimeSpan.FromSeconds(rnd.Next()%10 + 10));
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
