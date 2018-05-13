using DBClientFiles.NET.Collections;
using DBClientFiles.NET.Collections.Generic;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace DBClientFiles.NET.Test
{
    public class StorageTester<TKey, TValue>
    {
        private List<TimeSpan> _dictionaryTimes = new List<TimeSpan>();
        private List<TimeSpan> _listTimes = new List<TimeSpan>();

        public TimeSpan AverageDictionaryTime => new TimeSpan((long)_dictionaryTimes.Average(t => t.Ticks));
        public TimeSpan AverageListTime => new TimeSpan((long)_listTimes.Average(t => t.Ticks));

        public TimeSpan MaxDictionaryTime => _dictionaryTimes.Max();
        public TimeSpan MaxListTime => _listTimes.Max();

        public TimeSpan MinDictionaryTime => _dictionaryTimes.Min();
        public TimeSpan MinListTime => _listTimes.Min();

        public void TestStorages(string fileName, int iterationCount = 100)
        {
            using (var fs = File.OpenRead(fileName))
                TestStorages(fs, iterationCount);
        }

        public void TestStorages(Stream fs, int iterationCount = 100)
        {
            for (var i = 0; i < iterationCount; ++i)
            {
                var sw = new Stopwatch();

                var storeType = typeof(StorageList<>).MakeGenericType(typeof(TValue));

                sw.Start();
                var instance = Activator.CreateInstance(storeType, fs, StorageOptions.Default);
                sw.Stop();

                _listTimes.Add(sw.Elapsed);

                fs.Position = 0;
            }
        }
    }
}
