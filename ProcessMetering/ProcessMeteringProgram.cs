


using System.Diagnostics;

long Kb = 1024;
long Mb = Kb*Kb;

int step = 200;
int startBatchSize = 200;
int batchSizeLimit = 10000;
string processPath = @"C:\Users\skld0\source\repos\AnalData\MongoElasticDataConnector\bin\Release\net6.0\MongoElasticDataConnector.exe";

List<int> batchSizeList = new List<int>();

for(int currentBatchSize = startBatchSize; currentBatchSize <= batchSizeLimit; currentBatchSize += step)
{
    batchSizeList.Add(currentBatchSize);
}
Dictionary<int, string> batchDictionary = new();
foreach(var currentBatchSize in batchSizeList)
{
    long maxMemoryUsage = 0;
    double spendTime = 0;
    DateTime startTime = DateTime.Now;
    DateTime endTime = DateTime.Now;
    int exitCode = 0;
    Process process = new();
    process = Process.Start(processPath, currentBatchSize.ToString() + " 100000");
    process.EnableRaisingEvents = true;
    process.Exited += new EventHandler(ProcessExited);

    DateTime LogTime = DateTime.Now;
    while (!process.HasExited)
    {
        maxMemoryUsage = Math.Max(maxMemoryUsage, process.PeakPagedMemorySize64/Mb);
        process.Refresh();
        if((DateTime.Now - LogTime).TotalSeconds > 10)
        {
            Console.WriteLine($"Process {currentBatchSize}, peak value {maxMemoryUsage}, time spend {(DateTime.Now - startTime).TotalSeconds} seconds");
            LogTime = DateTime.Now;
        }
        
    }
    Console.WriteLine($"{currentBatchSize},{spendTime},{maxMemoryUsage},{startTime},{exitCode}");
    batchDictionary.Add(currentBatchSize, $"{currentBatchSize},{spendTime},{maxMemoryUsage},{startTime},{exitCode}");
    void ProcessExited(object sender, System.EventArgs e)
    {
        endTime = DateTime.Now;
        spendTime = Math.Round((endTime - startTime).TotalMilliseconds);
        exitCode = process.ExitCode;
    }
};

using(StreamWriter writer = new StreamWriter(@"C:\Users\skld0\source\repos\AnalData\ProcessMetering\BatchRecordLog.txt", true))
{
    foreach(var batch in batchDictionary)
    {
        writer.WriteLine(batch.Value);
    }
}