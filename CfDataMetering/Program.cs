



using СfDataReciver;
int batchSize = Convert.ToInt32(Environment.GetCommandLineArgs()[1]);
var reciver = new CfApiScripts("TestCfMetering");
reciver.Db.DropCollection("TestCfMetering");
reciver.Db.CreateCollection("TestCfMetering");
await reciver.WriteSolutionCollection(1726, batchSize: batchSize);