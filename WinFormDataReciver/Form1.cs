using Common;
using Common.Models;
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
        }
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
                        this.CodeParseTextBox.Text = $"{item["id"].AsInt64}\n {solutionCode}";
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

        private async void UploadUsersToEs_Click(object sender, EventArgs e)
        {
            try
            {
                UploadUsersToEs.Enabled = false;
                var UsersCollection = MongoElasticConector.Init().MongoDb.GetCollection<User>("Users");
                string indexName = "user_index";
                MongoElasticConector.ElasticClient.Indices.Delete(indexName);
                MongoElasticConector.ElasticClient.Indices.Create(indexName, index => index.Map<User>(x => x.AutoMap()));
                using (var cursor = await UsersCollection.FindAsync<User>(new BsonDocument()))
                {
                    while (cursor.MoveNext())
                    {
                        await MongoElasticConector.ElasticClient.IndexManyAsync<User>(cursor.Current, indexName);
                    }
                }
            }
            catch (Exception ex)
            {
                UploadUsersToEs.Enabled = true;
                MessageBox.Show("Загрузка пользователей в Es " + ex.Message);
            }
            UploadUsersToEs.Enabled = true;
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
    }
}