using AnalData;
using Common;
using System.Net;
using СfDataReciver;

/// <summary>
/// Исполняемый скрипт
/// </summary>
async Task MainScriptAsync()
{
    var connString = System.Environment.GetEnvironmentVariable("MONGODB_CONNSTRING");
    Console.WriteLine(connString);
}
 await MainScriptAsync();