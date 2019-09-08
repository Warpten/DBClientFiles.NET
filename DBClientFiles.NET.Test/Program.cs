using DBClientFiles.NET.Collections.Generic;
using DBClientFiles.NET.Types.WDB2;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace DBClientFiles.NET.Runner
{
    public static class Program
    {
        public static void Main(string[] args)
        {
            using (var fs = File.OpenRead(@"D:\Games\World of Warcraft 4.3.4 - Akama\dbc\Item-sparse.db2"))
            {
                var collection = new StorageEnumerable<ItemSparse>(StorageOptions.Default, fs).GetEnumerator();
                ROOT_INSPECT(collection);
            }
        }

        private static void ROOT_INSPECT<T>(IEnumerator<T> enumerator)
        {
            enumerator.MoveNext();
            InspectObject(enumerator.Current);
        }

        private static void InspectObject(object obj)
        {
            PrintValue("model", obj);
        }

        private static void PrintValue(string prefix, object value)
        {
            if (value is string || value.GetType().IsPrimitive)
            {
                if (value is string)
                    Console.WriteLine($@"{prefix.PadRight(50, ' ')}""{value}""");
                else if (value is int || value is uint)
                    Console.WriteLine($"{prefix.PadRight(50, ' ')}{value} (0x{value:X8})");
                else
                    Console.WriteLine($"{prefix.PadRight(50, ' ')}{value}");
            }
            else
            {
                var props = value.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance);
                foreach (var prop in props)
                {
                    var propValue = prop.GetValue(value);
                    if (propValue == null)
                    {
                        Console.WriteLine($"{prefix}.{prop.Name}] = null");

                        continue;
                    }

                    if (propValue.GetType().IsArray)
                    {
                        var valArray = (Array)propValue;
                        for (var i = 0; i < valArray.Length; ++i)
                            PrintValue($"{prefix}.{prop.Name}[{i}]", valArray.GetValue(i));
                    }
                    else
                    {
                        PrintValue($"{prefix}.{prop.Name}", propValue);
                    }
                }
            }
        }

    }
}
