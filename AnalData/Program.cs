using AnalData;
using Common;
using System.Net;
using СfDataReciver;

/// <summary>
/// Исполняемый скрипт
/// </summary>
async Task MainScriptAsync()
{
    List<int> ForList = Enumerable.Range(1, 1000).ToList();
    Parallel.ForEach(ForList, intVar => {
    new CfApiScripts().GetJsonRequest(url: "https://codeforces.com/api/contest.status?contestId=1736&from=1&count=100");
        Console.WriteLine($"{intVar} successed");
        });
}
 await MainScriptAsync();