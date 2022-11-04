using AnalData;
using Elasticsearch.Net;
using MongoDB.Bson;
using MongoDB.Bson.IO;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;
using MongoDB.Driver;
using Nest;
using Newtonsoft.Json;
using Newtonsoft.Json.Bson;
using Newtonsoft.Json.Linq;
using System.Net;
using System.Text;
using Nest;
using AnalData.Models;
using System.Diagnostics;

/// <summary>
/// Исполняемый скрипт
/// </summary>
void MainScriptAsync()
{
    //MongoElasticConector mongoElasticConector = new();
    //mongoElasticConector.CopySubmissionsAsync();
    long mB = 1024;
    long kB = mB*mB;
    List<string> initList = new List<string>();
    for (int i = 0; i < 100; i++)
    {
        initList.Add("1234567890");
    }
    Process process = Process.GetCurrentProcess();
        string r;
    List<string> list = new List<string>();
    do
    {
        list.AddRange(initList);
        initList.AddRange(initList);
        process.Refresh();
        Console.WriteLine(Process.GetCurrentProcess().PagedSystemMemorySize64/kB);
        Console.WriteLine(Process.GetCurrentProcess().PeakPagedMemorySize64/kB);
        Console.WriteLine(Process.GetCurrentProcess().PrivateMemorySize64/kB);
        Console.WriteLine("GC GetTotalAllocatedMemory: {0}", GC.GetTotalAllocatedBytes()/kB);
        r = Console.ReadLine();
        if(r == "1")
        {
            initList = new List<string>();
            list = new List<string>();
            GC.Collect();
            for (int i = 0; i<100; i++)
            {
                initList.Add("1234567890");
            }
        }
    } while (r != "0");
}
 MainScriptAsync();