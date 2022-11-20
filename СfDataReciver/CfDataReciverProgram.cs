using СfDataReciver;


if (Environment.GetCommandLineArgs().Count() > 3)
{
    Console.WriteLine($"Started api download with batch {Convert.ToInt32(Environment.GetCommandLineArgs()[3])}");
    var reciver = new CfApiScripts(Environment.GetCommandLineArgs()[2]);
    await reciver.WriteSolutionCollection(Convert.ToInt64(Environment.GetCommandLineArgs()[1]), batchSize: Convert.ToInt32(Environment.GetCommandLineArgs()[3]));
}
else
{
    Console.WriteLine("NoArgs");
    await new CfApiScripts().RefreshContestSolutionList();
}