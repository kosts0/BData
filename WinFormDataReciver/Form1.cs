using Common;
using Common.Models;
using MongoDB.Bson;
using MongoDB.Driver;
using Nest;
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
        private async Task GetSolutionsCode()
        {
            
        }
        private async void GetParsedSolution_Click(object sender, EventArgs e)
        {
            try
            {
                GetParsedSolution.Enabled = false;
                await CfApiScripts.GetSolutionCode();
            }
            catch (Exception ex)
            {
                GetParsedSolution.Enabled = true;
                MessageBox.Show("Получение кода решения " + ex.Message);
            }
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
        }

        private async void LoadToEsDataButton_Click(object sender, EventArgs e)
        {
            try
            {
                LoadToEsDataButton.Enabled = false;
                string indexName = $"submission_index_master_with_users";
                var submissionCollection = MongoElasticConector.MongoDb.GetCollection<BsonDocument>("ContestStatus");
                var userCollestion = MongoElasticConector.MongoDb.GetCollection<User>("Users");
                MongoElasticConector.ElasticClient.Indices.Delete(indexName);
                var response2 = MongoElasticConector.ElasticClient.Indices.Create(indexName,
                                    index => index.Map<SubmissionWithUsers>(
                                        x => x.AutoMap()));
                using (var cursor = await submissionCollection.FindAsync(new BsonDocument() { { "author.participantType", "CONTESTANT" } }))
                {
                    while (cursor.MoveNext())
                    {
                        foreach(var item in cursor.Current)
                        {
                            SubmissionWithUsers submissionWithUsers = new SubmissionWithUsers(item);
                            submissionWithUsers.User = await (await userCollestion.FindAsync(Builders<User>.Filter.Eq(u => u.handle, submissionWithUsers.AuthorHandle))).FirstOrDefaultAsync();
                            var response = await MongoElasticConector.ElasticClient.IndexAsync<SubmissionWithUsers>(submissionWithUsers, i => i.Index(indexName));
                            if (!response.IsValid)
                            {
                                throw new Exception(response.DebugInformation);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                LoadToEsDataButton.Enabled = true;
                MessageBox.Show("Загрузка данных в Es " + ex.Message);
            }
        }
        
    }
}