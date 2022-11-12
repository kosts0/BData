


using System.Diagnostics;

long Kb = 1024;
long Mb = Kb*Kb;
/// <summary>
/// Путь к исполняемому файлу процесса
/// </summary>
string processPath = Environment.GetCommandLineArgs().Count() > 1 ? Environment.GetCommandLineArgs()[1] : @"C:\Users\skld0\source\repos\AnalData\CfDataMetering\bin\Debug\net6.0\CfDataMetering.exe";
/// <summary>
/// Шаг батча
/// </summary>
int step = Environment.GetCommandLineArgs().Count() > 2 ? Convert.ToInt32(Environment.GetCommandLineArgs()[2]) : 200;
/// <summary>
/// Стартовый размер батча
/// </summary>
int startBatchSize = Environment.GetCommandLineArgs().Count() > 3 ? Convert.ToInt32(Environment.GetCommandLineArgs()[3]) : 200;
/// <summary>
/// Граница измерений батча
/// </summary>
int batchSizeLimit = Environment.GetCommandLineArgs().Count() > 4 ? Convert.ToInt32(Environment.GetCommandLineArgs()[4]) : 10000;
/// <summary>
/// Аргументы процесса
/// </summary>
string commandLineArgs = Environment.GetCommandLineArgs().Count() > 5 ? Environment.GetCommandLineArgs()[5].Trim('"') : "500000";

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
    process = Process.Start(processPath, currentBatchSize.ToString());
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

using(StreamWriter writer = new StreamWriter(@"C:\Users\skld0\source\repos\AnalData\ProcessMetering\BatchCfLog.txt", true))
{
    foreach(var batch in batchDictionary)
    {
        writer.WriteLine(batch.Value);
    }
}