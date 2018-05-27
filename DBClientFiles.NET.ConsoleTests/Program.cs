using DBClientFiles.NET.Collections.Generic;
using DBClientFiles.NET.Data;
using DBClientFiles.NET.Data.WDBC;
using DBClientFiles.NET.Test;
using System;
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
            TestStructuresInNamespace("DBClientFiles.NET.Data.WDC1");

            Console.ReadKey();
        }

        private static Dictionary<Type, BenchmarkResult> _dataStores = new Dictionary<Type, BenchmarkResult>();

        private static MethodInfo methodInfo = typeof(Program).GetMethod("BenchmarkStructure", BindingFlags.NonPublic | BindingFlags.Static);

        // Forces the corresponding assembly to be referenced.
        private volatile AchievementEntry entry;

        private static void TestStructuresInNamespace(string @namespace)
        {
            _dataStores.Clear();

            var fileType = @namespace.Split('.').Last();

            var types = Assembly.GetExecutingAssembly().GetReferencedAssemblies()
                .Where(a => a.Name.Contains("DBClientFiles"))
                .Select(a => Assembly.Load(a))
                .SelectMany(a => a.GetTypes());

            foreach (var typeInfo in types.Where(t => t.Namespace == @namespace))
            {
                var genericMethodInfo = methodInfo.MakeGenericMethod(typeInfo);
                var fileName = typeInfo.Name.Replace("Entry", "");
                var nameAttr = typeInfo.GetCustomAttribute<DBFileNameAttribute>();
                if (nameAttr != null)
                {
                    fileName = nameAttr.Name + "." + (nameAttr.Extension == FileExtension.DB2 ? "db2" : "dbc");
                }
                else
                {
                    fileName += ".dbc";
                }

                var resourcePath = $@"D:\Repositories\DBFilesClient.NET\Tests\{fileType}\Files\{fileName}";
                genericMethodInfo.Invoke(null, new object[] { resourcePath });
            }

            Console.WriteLine(BenchmarkResult.Header);
            Console.WriteLine(BenchmarkResult.HeaderSep);
            foreach (var kv in _dataStores)
            {
                Console.WriteLine(kv.Value.ToString());
                using (var writer  = new StreamWriter($"./perf_{kv.Value.RecordType.Name.Replace("Entry","").ToLower()}.csv"))
                    kv.Value.WriteCSV(writer);
            }

            Console.WriteLine();

        }

        private static void BenchmarkStructure<TValue>(string resourcePath) where TValue : class, new()
        {
            var correctedPath = File.Exists(resourcePath) ? resourcePath : resourcePath.Replace(".dbc", ".db2");

            using (var fs = File.OpenRead(correctedPath))
            using (var ms = new MemoryStream())
            {
                fs.CopyTo(ms);
                ms.Position = 0;

                var structureTester = new StructureTester<TValue>();
                var benchmarkResult = structureTester.Benchmark<StorageList<TValue>>(out var dataStore, ms, 250);
                _dataStores[typeof(TValue)] = benchmarkResult;
            }
        }
    }
}
