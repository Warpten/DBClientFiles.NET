using DBClientFiles.NET.Collections.Generic;
using DBClientFiles.NET.Data;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using DBClientFiles.NET.Attributes;
using AchievementEntry = DBClientFiles.NET.Data.WDBC.AchievementEntry;

namespace DBClientFiles.NET.ConsoleTests
{
    class Dummy
    {
        [Index]
        public int ID { get; set; }
    }

    class Program
    {
        static void Main(string[] args)
        {
            if (args.Contains("--preJT"))
                PrepareLibrary();

            var idxCount = Array.IndexOf(args, "--count");
            var iterationCount = 1;
            if (idxCount != -1 && args.Length >= idxCount + 1)
                int.TryParse(args[idxCount + 1], out iterationCount);

            using (var fs =
                File.OpenRead(
                    @"C:\Users\Vincent Piquet\source\repos\DBClientFiles.NET\DBClientFiles.NET.Benchmark\Files\WDC2\Achievement.db2")
            )
            {
                var store = new StorageDictionary<int, Data.WDC2.AchievementEntry>(fs);
                StructureTester.InspectInstance(store.First().Value);
                return;
            }


            //using (var fs = File.OpenRead(@"C:\Users\Vincent Piquet\source\repos\DBClientFiles.NET\DBClientFiles.NET.Benchmark\bin\Release\net472\Data\WDC1\ItemSearchName.db2"))
            //{
            //    // var sl = new StorageList<Data.WDC1.SpellEffectEntry>(fs);
            //    var d = new StorageDictionary<int, Data.WDC1.ItemSearchNameEntry>(fs);
            //}

            TestStructuresInNamespace("DBClientFiles.NET.Data.WDBC", iterationCount);
            TestStructuresInNamespace("DBClientFiles.NET.Data.WDB2", iterationCount);
            TestStructuresInNamespace("DBClientFiles.NET.Data.WDC1", iterationCount);
            TestStructuresInNamespace("DBClientFiles.NET.Data.WDC2", iterationCount);

            Console.WriteLine("Press a key to exit");
            Console.ReadKey();
        }

        private static void PrepareLibrary()
        {
            var types = Assembly.GetExecutingAssembly().GetReferencedAssemblies()
                .Where(a => a.Name == "DBClientFiles.NET")
                .Select(Assembly.Load)
                .SelectMany(a => a.GetTypes());

            foreach (var type in types)
            {
                foreach (var method in type.GetMethods(BindingFlags.DeclaredOnly |
                                                       BindingFlags.NonPublic |
                                                       BindingFlags.Public | BindingFlags.Instance |
                                                       BindingFlags.Static))
                {
                    RuntimeHelpers.PrepareMethod(method.MethodHandle);
                }
            }
        }

        private static Dictionary<Type, BenchmarkResult> _dataStores = new Dictionary<Type, BenchmarkResult>();

        private static MethodInfo methodInfo = typeof(Program).GetMethod("BenchmarkStructure", BindingFlags.NonPublic | BindingFlags.Static);
        // Forces the corresponding assembly to be referenced.
#pragma warning disable 169
        private volatile AchievementEntry entry;
#pragma warning restore 169

        private static Stream OpenFile(string resourceName)
        {
            return File.OpenRead($@"C:\Users\Vincent Piquet\source\repos\DBClientFiles.NET\DBClientFiles.NET.Benchmark\Files\{resourceName}");
        }


        private static void TestStructuresInNamespace(string @namespace, int count = 1)
        {
            var fileType = @namespace.Split('.').Last();

            var types = Assembly.GetExecutingAssembly().GetReferencedAssemblies()
                .Where(a => a.Name.Contains("DBClientFiles"))
                .Select(Assembly.Load)
                .SelectMany(a => a.GetTypes());

            Console.WriteLine(BenchmarkResult.Header);
            Console.WriteLine(BenchmarkResult.HeaderSep);
            foreach (var typeInfo in types.Where(t => t.Namespace == @namespace))
            {
                if (!typeInfo.IsDefined(typeof(DBFileNameAttribute), false))
                    continue;

                var genericMethodInfo = methodInfo.MakeGenericMethod(typeInfo);
                var nameAttr = typeInfo.GetCustomAttribute<DBFileNameAttribute>();
                var fileName = nameAttr.Name + "." + (nameAttr.Extension == FileExtension.DB2 ? "db2" : "dbc");
                genericMethodInfo.Invoke(null, new object[] {fileName, count});
            }

            foreach (var kv in _dataStores)
            {
                Console.WriteLine(kv.Value.ToString());
                using (var writer  = new StreamWriter($"./perf_{kv.Value.RecordType.Name.Replace("Entry","").ToLower()}_{kv.Value.Signature}.csv"))
                    kv.Value.WriteCSV(writer);
            }

            Console.WriteLine();
        }

        private static void BenchmarkStructure<TValue>(string resourcePath, int count) where TValue : class, new()
        {
            var correctedPath = File.Exists(resourcePath) ? resourcePath : resourcePath.Replace(".dbc", ".db2");

            using (var fs = OpenFile(resourcePath))
            using (var ms = new MemoryStream((int)fs.Length))
            {
                fs.CopyTo(ms);
                ms.Position = 0;
                var benchmarkResult = StructureTester<TValue>.Benchmark<StorageList<TValue>>(out var dataStore, ms, count);

                Console.WriteLine(benchmarkResult.ToString());
#if NETCOREAPP2_1
                using (var writer = new StreamWriter($"./perf_{benchmarkResult.RecordType.Name.Replace("Entry", "").ToLower()}_{benchmarkResult.Signature}_netcore21.csv"))
#else
                using (var writer = new StreamWriter($"./perf_{benchmarkResult.RecordType.Name.Replace("Entry", "").ToLower()}_{benchmarkResult.Signature}_netframework472.csv"))
#endif
                    benchmarkResult.WriteCSV(writer);
            }
        }
    }
}
