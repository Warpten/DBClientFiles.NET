using DBClientFiles.NET.Collections;
using DBClientFiles.NET.Collections.Generic;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace DBClientFiles.NET.ConsoleTests
{
    public static class StructureTester
    {
        public static void InspectInstance(object instance)
        {
            foreach (var memberInfo in instance.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance))
            {
                if (!memberInfo.PropertyType.IsArray)
                {
                    if (memberInfo.PropertyType == typeof(string))
                        Console.WriteLine($@"{memberInfo.Name}: ""{memberInfo.GetValue(instance)}""");
                    else
                        Console.WriteLine($@"{memberInfo.Name}: {memberInfo.GetValue(instance)}");
                }
                else
                {
                    var value = (Array)memberInfo.GetValue(instance);
                    Console.WriteLine($"{memberInfo.Name}: [{value.Length}] {{");
                    if (memberInfo.PropertyType == typeof(string[]))
                    {
                        for (var i = 0; i < value.Length; ++i)
                            if (!string.IsNullOrEmpty((string)value.GetValue(i)))
                                Console.WriteLine($@"{i.ToString().PadLeft(5)}: ""{value.GetValue(i)}""");
                    }
                    else
                        for (var i = 0; i < value.Length; ++i)
                            Console.WriteLine($@"{i.ToString().PadLeft(5)}: {value.GetValue(i)}");

                    Console.WriteLine("}");
                }
            }
        }
    }

    public static class StructureTester<TValue> where TValue : class, new()
    {
        public static void InspectInstance(TValue instance) => StructureTester.InspectInstance(instance);

        public static TimeSpan AccumulateList(Stream dataStream, StorageOptions options) => Accumulate<StorageList<TValue>>(dataStream, options);
        public static TimeSpan AccumulateList(Stream dataStream) => AccumulateList(dataStream, StorageOptions.Default);

        public static TimeSpan Accumulate<TStorage>(Stream dataStream, StorageOptions options) where TStorage : IStorage
            => Accumulate<TStorage>(out _, dataStream, options);

        public static TimeSpan Accumulate<TStorage>(out TStorage instance, Stream dataStream, StorageOptions options)
            where TStorage : IStorage
        {
            dataStream.Position = 0;

            var timer = new Stopwatch();
            {
                timer.Start();
                instance = (TStorage)typeof(TStorage).CreateInstance(dataStream, options);
                timer.Stop();
            }

            return timer.Elapsed - TypeExtensions.LambdaGenerationTime;
        }

        public static BenchmarkResult Benchmark<TStorage>(out TStorage instance, Stream dataStream, int iterationCount = 100) where TStorage : IStorage
            => Benchmark(out instance, dataStream, StorageOptions.Default, iterationCount);

        public static BenchmarkResult Benchmark<TStorage>(out TStorage instance, Stream dataStream, StorageOptions options, int iterationCount = 100) where TStorage : IStorage
        {
            if (iterationCount == 0)
                throw new ArgumentOutOfRangeException(nameof(iterationCount));

            var benchmarkResult = new BenchmarkResult();
            benchmarkResult.RecordType = typeof(TValue);

            // Stupid workaround for the compiler not picking up assignment of instance in the loop
            benchmarkResult.TotalTimes.Add(Accumulate(out instance, dataStream, options));
            GC.Collect();

            for (var i = 1; i < iterationCount; ++i)
            {
                benchmarkResult.TotalTimes.Add(Accumulate<TStorage>(dataStream, options));
                GC.Collect();
                dataStream.Position = 0;
            }

            benchmarkResult.Container = (IList)instance;

            return benchmarkResult;
        }

        public static BenchmarkResult Benchmark<TStorage>(Stream dataStream, int iterationCount = 100) where TStorage : IStorage
            => Benchmark<TStorage>(dataStream, StorageOptions.Default, iterationCount);

        public static BenchmarkResult Benchmark<TStorage>(Stream dataStream, StorageOptions options, int iterationCount = 100) where TStorage : IStorage
            => Benchmark<TStorage>(out _, dataStream, options, iterationCount);
    }

    public class BenchmarkResult
    {
        public BenchmarkResult()
        {
            TotalTimes = new List<TimeSpan>();

            RecordType = typeof(object);
        }

        public List<TimeSpan> TotalTimes { get; }
        public TimeSpan BestTime => TotalTimes.Min();
        public TimeSpan WorstTime => TotalTimes.Max();
        public TimeSpan AverageTime => new TimeSpan(Convert.ToInt64(TotalTimes.Average(t => t.Ticks)));
        public Type RecordType { get; set; }
        public Signatures Signature => ((IStorage)Container).Signature;
        public IList Container { get; set; }

        public override string ToString()
        {
            var stringBuilder = new StringBuilder();
            stringBuilder.Append("| ");
            stringBuilder.Append((RecordType.Name + " [" + Container.Count + " entries - (" + Signature + ")]").PadRight(65));

            stringBuilder.Append(" | ");
            stringBuilder.Append(string.Format("{0:s\\.ffffff} ({1:s\\.ffffff})", 
                AverageTime, 
                TimeSpan.FromTicks(AverageTime.Ticks / Container.Count)).PadRight(20));
            stringBuilder.Append(" | ");
            stringBuilder.Append(string.Format("{0:s\\.ffffff} ({1:s\\.ffffff})",
                BestTime,
                TimeSpan.FromTicks(BestTime.Ticks / Container.Count)).PadRight(20));
            stringBuilder.Append(" | ");
            stringBuilder.Append(string.Format("{0:s\\.ffffff} ({1:s\\.ffffff})",
                WorstTime,
                TimeSpan.FromTicks(WorstTime.Ticks / Container.Count)).PadRight(20));
            stringBuilder.Append(" |");
            return stringBuilder.ToString();
        }

        public void WriteCSV(StreamWriter writer)
        {
            for (var i = 0; i < TotalTimes.Count; ++i)
                writer.WriteLine("{0},{1},{2}",
                    i,
                    TotalTimes[i].TotalMilliseconds,
                    TotalTimes[i].TotalMilliseconds / Container.Count);
            writer.WriteLine();
        }

        public static string Header
        {
            get
            {
                var stringBuilder = new StringBuilder();
                stringBuilder.Append("| File name".PadRight(67) + " | ");
                stringBuilder.Append("Avg (s)".PadRight(20) + " | ");
                stringBuilder.Append("Best (s)".PadRight(20) + " | ");
                stringBuilder.Append("Worst (s)".PadRight(20) + " | ");
                return stringBuilder.ToString();
            }
        }

        public static string HeaderSep
        {
            get
            {
                var stringBuilder = new StringBuilder();
                stringBuilder.Append("| ");
                stringBuilder.Append(new string('-', 65));
                stringBuilder.Append(" | ");
                stringBuilder.Append(new string('-', 20));
                stringBuilder.Append(" | ");
                stringBuilder.Append(new string('-', 20));
                stringBuilder.Append(" | ");
                stringBuilder.Append(new string('-', 20));
                stringBuilder.Append(" |");
                return stringBuilder.ToString();
            }
        }
    }
}
