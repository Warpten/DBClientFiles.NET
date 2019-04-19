using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;
using DBClientFiles.NET.Benchmark.Attributes;

namespace DBClientFiles.NET.Benchmark
{
    [NetCoreJob]
    public class SlimMutexTest
    {
        [Params(4, 8)]
        public int Parallelism;

        [Benchmark(Baseline = true)]
        public void ConcurrentDictionaryTest()
        {
            var dict = new ConcurrentDictionary<int, int>();

            Action<object> thr = (object o) => {
                var key = (int) o;
                dict.TryAdd(key, key * 2 + 1);
            };

            var tasks = new Task[Parallelism];
            for (int i = 0; i < Parallelism; ++i)
                tasks[i] = Task.Factory.StartNew(thr, i);

            Task.WaitAll(tasks);
        }

        [Benchmark]
        public void SlimMutexTestBench()
        {
            var dict = new Dictionary<int, int>();
            
            var mtx = new SemaphoreSlim(1);
            Action<object> thr = (object o) => {
                var key = (int) o;
                mtx.Wait();
                dict.Add(key, key * 2 + 1);
                mtx.Release();
            };

            var tasks = new Task[Parallelism];
            for (int i = 0; i < Parallelism; ++i)
                tasks[i] = Task.Factory.StartNew(thr, i);

            Task.WaitAll(tasks);
        }
    }
}
