using Common;
using Common.Models;
using Microsoft.Extensions.Configuration;
using MongoDB.Bson;
using MongoDB.Driver;
using Nest;
using System.Collections.Generic;
using СfDataReciver;

namespace WinFormDataReciver
{
    public partial class Form1 : Form
    {

        public CfApiScripts CfApiScripts { get; set; }
        public MongoElasticConector MongoElasticConector { get; set; }
        public Form1()
        {
            CfApiScripts = new();
            MongoElasticConector = new();
            InitializeComponent();
            LoadedContestList = CfApiScripts.UpdateUsedIdList(Path.Combine(Environment.CurrentDirectory, "UsedContests.txt"), LoadedContestList);
            CfApiScripts.ProxyInfoList = Program.Configuration.GetSection("ProxyInfoList").Get<List<ProxyInfo>>();
        }
        public List<long> LoadedContestList { get; private set; } = new();
        public List<long> InLoadContestList { get; private set; } = new();
        private async Task GetSolutionCode()
        {
            var ContestStatusDb = MongoElasticConector.MongoDb.GetCollection<BsonDocument>("ContestStatus");

            var filter = Builders<BsonDocument>.Filter.Exists("SolutionCode", false);
            filter &= Builders<BsonDocument>.Filter.Eq("author.participantType", "CONTESTANT");
            using (var cursor = await ContestStatusDb.FindAsync(filter))
            {
                while (cursor.MoveNext())
                {
                    foreach (var item in cursor.Current)
                    {
                        var solutionCode = CfApiScripts.GetSolutionCodeText(item["contestId"].AsInt64.ToString(), item["_id"].AsInt64.ToString()) ?? "";
                        var updateSettings = new BsonDocument("$set", new BsonDocument("SolutionCode", solutionCode));
                        await ContestStatusDb.UpdateOneAsync(new BsonDocument() { { "_id", item["_id"].AsInt64 } }, updateSettings);
                        this.CodeParseTextBox.Text = $"Proxy: {CfApiScripts.CurrentProxy.IpPort ?? "null"}"+ Environment.NewLine + $"{item["id"].AsInt64}\n" + $"{solutionCode}";
                    }
                }
            }
        }
        private async void GetParsedSolution_Click(object sender, EventArgs e)
        {
            try
            {
                GetParsedSolution.Enabled = false;
                await GetSolutionCode();
            }
            catch (Exception ex)
            {
                GetParsedSolution.Enabled = true;
                MessageBox.Show("Получение кода решения " + ex.Message);
            }
            GetParsedSolution.Enabled = true;
        }

        private async void LoadToEsDataButton_Click(object sender, EventArgs e)
        {
            try
            {
                LoadToEsDataButton.Enabled = false;
                string indexName = $"submission_index_master_with_users";
                var submissionCollection = MongoElasticConector.MongoDb.GetCollection<BsonDocument>("ContestStatus");
                var userCollestion = MongoElasticConector.MongoDb.GetCollection<User>("Users");
                Dictionary<string, User> userDictionary = new();
                MongoElasticConector.ElasticClient.Indices.Delete(indexName);
                var response2 = MongoElasticConector.ElasticClient.Indices.Create(indexName,
                                    index => index.Map<SubmissionWithUsers>(
                                        x => x.AutoMap()));
                foreach (var user in userCollestion.AsQueryable())
                {
                    userDictionary[user.handle] = user;
                }
                long loadedCount = 0;
                using (var cursor = await submissionCollection.FindAsync(new BsonDocument() { { "author.participantType", "CONTESTANT" } }))
                {
                    while (cursor.MoveNext())
                    {
                        var inserItems = cursor.Current.Where(u => userDictionary.ContainsKey(u["author"].AsBsonDocument["members"].AsBsonArray[0].AsBsonDocument["handle"].AsString)).Select(u => new SubmissionWithUsers(u) { User = userDictionary[u["author"].AsBsonDocument["members"].AsBsonArray[0].AsBsonDocument["handle"].AsString] });
                        var response = await MongoElasticConector.ElasticClient.IndexManyAsync<SubmissionWithUsers>(inserItems, indexName);
                        if (!response.IsValid)
                        {
                            throw new Exception(response.DebugInformation);
                        }
                        loadedCount += cursor.Current.Count();
                        textBox1.Text = $"{loadedCount}";
                    }
                }
            }
            catch (Exception ex)
            {
                LoadToEsDataButton.Enabled = true;
                MessageBox.Show("Загрузка данных в Es " + ex.Message);
            }
            LoadToEsDataButton.Enabled = true;
        }

        private async void UploadSolutionCodeButton_Click(object sender, EventArgs e)
        {
            var submissionCollection = MongoElasticConector.MongoDb.GetCollection<BsonDocument>("ContestStatus");
            Dictionary<string, User> userDictionary = new();
            var userCollestion = MongoElasticConector.MongoDb.GetCollection<User>("Users");
            foreach (var user in userCollestion.AsQueryable())
            {
                userDictionary[user.handle] = user;
            }
            string indexName = "submission_index_master_with_users";
            using (var cursor = await submissionCollection.FindAsync(Builders<BsonDocument>.Filter.Exists("SolutionCode")))
            {
                while (cursor.MoveNext())
                {
                    foreach(var item in cursor.Current)
                    {
                        var userHandle = item["author"].AsBsonDocument["members"].AsBsonArray[0].AsBsonDocument["handle"].AsString;
                        var user = userDictionary.ContainsKey(userHandle) ? userDictionary[userHandle] : null;
                        var res = await this.MongoElasticConector.ElasticClient.SearchAsync<SubmissionWithUsers>( s=> s.Index(indexName).Query(q => q.Term("_id", item["id"].AsInt64.ToString())));
                        if (res.Total > 0)
                        {
                            var response = await this.MongoElasticConector.ElasticClient.UpdateAsync<SubmissionWithUsers>(item["id"].AsInt64.ToString(), u => u.Index("submission_index_master_with_users").Doc(new SubmissionWithUsers(item) { User = user }));
                            if (!response.IsValid)
                            {
                                throw new Exception(response.DebugInformation);
                            }
                        }
                        else
                        {
                            var response = await this.MongoElasticConector.ElasticClient.IndexAsync<SubmissionWithUsers>(new SubmissionWithUsers(item) { User = user }, u => u.Index("submission_index_master_with_users"));
                            if (!response.IsValid)
                            {
                                throw new Exception(response.DebugInformation);
                            }
                        }
                                                                                             
                    }
                }
            }
        }

        private async void StartApiCfThread(Button button, TextBox textBox)
        {
            button.Enabled = false;
            int startIndex = 1;
            var contestStatusCollection = CfApiScripts.ContestStatus;
            bool breakCondition = true;
            int batchSize = 1500;
            long contestId = getContestId;
            InLoadContestList.Add(contestId);
            List<BsonDocument> currentSolutionList = new();
            do
            {
                currentSolutionList = await CfApiScripts.GetContestSolutionList(contestId, startIndex, batchSize);
                try
                {
                    if (currentSolutionList.Count == 0) continue;
                    await contestStatusCollection.InsertManyAsync(currentSolutionList, new InsertManyOptions() { IsOrdered = false });
                }
                catch (MongoBulkWriteException)
                {

                }
                breakCondition = currentSolutionList.Count != batchSize;
                startIndex+= batchSize;
                textBox.Text = $"ContestId: {contestId}; count: {startIndex}";
            } while (!breakCondition);
            using (StreamWriter writer = new StreamWriter(Path.Combine(Environment.CurrentDirectory, "UsedContests.txt"), true))
            {
                await writer.WriteLineAsync($"В контесте {contestId} добавлено {startIndex + currentSolutionList.Count} записей о попытках");
            }
            InLoadContestList.Remove(contestId);
            LoadedContestList.Add(contestId);
            LoadCfData.Enabled = true;
        }

        private async void LoadCfData_Click(object sender, EventArgs e)
        {
            StartApiCfThread(LoadCfData, CfFirstThread);
        }
        long getContestId
        {
            get
            {
                if(InLoadContestList.Count == 0)
                {
                    return LoadedContestList.Min() - 1;
                }
                return Math.Min(LoadedContestList.Min(), InLoadContestList.Min()) - 1;
            }
        }
        private int currentCfSubmissionThreadCount = 1;
        private async void AddContestReciverThreadButton_Click(object sender, EventArgs e)
        {

            Button threadButton = new Button()
            {
                Size = LoadCfData.Size,
                Text = $"Submission Api (поток {currentCfSubmissionThreadCount + 1})",
            };
            TextBox textBox = new TextBox()
            {
                Size = CfFirstThread.Size,
                Location = new(CfFirstThread.Location.X, CfFirstThread.Location.Y + currentCfSubmissionThreadCount*LoadCfData.Size.Height + 10),
                ReadOnly = true,
            };
            threadButton.Click += (e, s) => StartApiCfThread(threadButton, textBox);
            var initLocation = LoadCfData.Location;
            var diffSize = initLocation.Y + currentCfSubmissionThreadCount*LoadCfData.Size.Height + 10;
            threadButton.Location = new Point(initLocation.X, initLocation.Y + currentCfSubmissionThreadCount*LoadCfData.Size.Height + 10);
            this.Controls.Add(threadButton);
            this.Controls.Add(textBox);
            AddContestReciverThreadButton.Location = new Point(AddContestReciverThreadButton.Location.X, AddContestReciverThreadButton.Location.Y + threadButton.Size.Height + 10);
            currentCfSubmissionThreadCount++;
            Refresh();
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            var docPath = Environment.CurrentDirectory;
            string text = "";
            DirectoryInfo directoryInfo = new DirectoryInfo(docPath);
            using (StreamReader reader = new StreamReader(Path.Combine(Environment.CurrentDirectory, "UsedContests.txt")))
            {
                text = reader.ReadToEnd();
            }
            while (directoryInfo.Name != "WinFormDataReciver")
            {
                directoryInfo = directoryInfo.Parent;
            }
            using(StreamWriter writer = new StreamWriter(Path.Combine(directoryInfo.FullName, "UsedContests.txt")))
            {
                writer.Write(text);
            }
        }
    }
}