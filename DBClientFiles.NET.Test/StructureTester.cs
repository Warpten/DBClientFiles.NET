using DBClientFiles.NET.Collections;
using DBClientFiles.NET.Collections.Generic;
using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;

namespace DBClientFiles.NET.Test
{
    public class StructureTester
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
                    Console.WriteLine($"{memberInfo.Name}: {{");
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

    public class StructureTester<TValue> where TValue : class, new()
    {
        public void InspectInstance(TValue instance)
        {
            foreach (var memberInfo in typeof(TValue).GetProperties(BindingFlags.Public | BindingFlags.Instance))
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
                    Console.WriteLine($"{memberInfo.Name}: {{");
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

        public static TimeSpan AccumulateList(Stream dataStream, StorageOptions options) => Accumulate<StorageList<TValue>>(dataStream, options);
        public static TimeSpan AccumulateList(Stream dataStream) => Accumulate<StorageList<TValue>>(dataStream, StorageOptions.Default);

        public static TimeSpan Accumulate<TStorage>(Stream dataStream) where TStorage : StorageBase<TValue>
            => Accumulate<TStorage>(dataStream, StorageOptions.Default);

        public static TimeSpan Accumulate<TStorage>(Stream dataStream, StorageOptions options) where TStorage : StorageBase<TValue>
        {
            var timer = Stopwatch.StartNew();
            typeof(TStorage).CreateInstance(dataStream, options);
            timer.Stop();

            return timer.Elapsed - TypeExtensions.LambdaGenerationTime;
        }

        public static double Benchmark<TStorage>(Stream dataStream, int iterationCount = 100) where TStorage : StorageBase<TValue>
            => Benchmark<TStorage>(dataStream, StorageOptions.Default, iterationCount);

        public static double Benchmark<TStorage>(Stream dataStream, StorageOptions options, int iterationCount = 100) where TStorage : StorageBase<TValue>
        {
            var averageDuration = Accumulate<TStorage>(dataStream, StorageOptions.Default);
            var lambdaGenerationTime = TypeExtensions.LambdaGenerationTime;
            for (var i = 1; i < iterationCount; ++i)
                averageDuration += Accumulate<TStorage>(dataStream, options);
            
            return (averageDuration - lambdaGenerationTime).TotalMilliseconds / iterationCount;
        }

        public static double Benchmark<TStorage>(Stream dataStream, out TimeSpan lambdaGenerationTime, int iterationCount = 100) where TStorage : StorageBase<TValue>
            => Benchmark<TStorage>(dataStream, out lambdaGenerationTime, StorageOptions.Default, iterationCount);

        public static double Benchmark<TStorage>(Stream dataStream, out TimeSpan lambdaGenerationTime, StorageOptions options, int iterationCount = 100) where TStorage : StorageBase<TValue>
        {
            var averageDuration = Accumulate<TStorage>(dataStream, StorageOptions.Default);

            lambdaGenerationTime = TypeExtensions.LambdaGenerationTime;
            for (var i = 1; i < iterationCount; ++i)
                averageDuration += Accumulate<TStorage>(dataStream, options);

            return (averageDuration - lambdaGenerationTime).TotalMilliseconds / iterationCount;
        }
    }
}
