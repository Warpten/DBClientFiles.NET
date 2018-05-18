using DBClientFiles.NET.Collections.Generic;
using DBClientFiles.NET.Data.WDBC;
using DBClientFiles.NET.Test;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace DBClientFiles.NET.ConsoleTests
{
    class Program
    {
        static void Main(string[] args)
        {
            TestStructuresInNamespace("DBClientFiles.NET.Data.WDBC");

            TestStructuresInNamespace("DBClientFiles.NET.Data.WDB2");

            Console.ReadKey();
        }

        private struct PerformanceNode
        {
            public IList Container;
            public TimeSpan AverageTime;
        }

        private static Dictionary<Type, PerformanceNode> _dataStores = new Dictionary<Type, PerformanceNode>();

        private static MethodInfo methodInfo = typeof(Program).GetMethod("BenchmarkStructure", BindingFlags.NonPublic | BindingFlags.Static);

        // Forces the corresponding assembly to be referenced.
        private volatile AchievementEntry entry;

        private static void TestStructuresInNamespace(string @namespace)
        {
            var fileType = @namespace.Split('.').Last();

            var types = Assembly.GetExecutingAssembly().GetReferencedAssemblies().Where(a => a.Name.Contains("DBClientFiles")).Select(a => Assembly.Load(a)).SelectMany(a => a.GetTypes());

            foreach (var typeInfo in types.Where(t => t.Namespace == @namespace))
            {
                var genericMethodInfo = methodInfo.MakeGenericMethod(typeInfo);
                var resourcePath = $@"D:\Repositories\DBFilesClient.NET\Tests\{fileType}\Files\{typeInfo.Name.Replace("Entry", "")}.dbc";
                genericMethodInfo.Invoke(null, new object[] { resourcePath });
            }

            Console.WriteLine($"{fileType}                                    Total Average       Total Average per record");
            Console.WriteLine( "==========================================================================================");
            foreach (var kv in _dataStores)
                Console.WriteLine($@"{kv.Key.Name.PadRight(40)}{kv.Value.AverageTime.ToString().PadRight(20)}{kv.Value.AverageTime.TotalMilliseconds / kv.Value.Container.Count:F5} ms");

            Console.WriteLine();
        }

        private static void BenchmarkStructure<TValue>(string resourcePath) where TValue : class, new()
        {
            using (var fs = File.OpenRead(File.Exists(resourcePath) ? resourcePath : resourcePath.Replace(".dbc", ".db2")))
            {
                var structureTester = new StructureTester<TValue>();
                var averageTime = structureTester.Benchmark<StorageList<TValue>>(out var dataStore, fs, 100);
                _dataStores[typeof(TValue)] = new PerformanceNode()
                {
                    Container = dataStore,
                    AverageTime = averageTime
                };
            }
        }
    }
}
