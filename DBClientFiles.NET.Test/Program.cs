using DBClientFiles.NET.Attributes;
using DBClientFiles.NET.Collections.Generic;
using DBClientFiles.NET.Types.WDBC;
using System;
using System.IO;
using System.Linq;
using System.Reflection;

namespace DBClientFiles.NET.Runner
{
    class Program
    {
        static unsafe void Main(string[] args)
        {
            using (var fs = File.OpenRead(@"D:\World of Warcraft 3.3.5\dbc\Achievement.dbc"))
            {
                var collection = new StorageList<Achievement>(StorageOptions.Default, fs);
                for (var i = 0; i < 10; ++i)
                    InspectObject(collection[i]);
            }

            Console.ReadKey();
        }

        private static void InspectObject(object obj)
        {
            PrintValue("model", obj);
        }

        private static void PrintValue(string prefix, object value)
        {
            if (value.GetType() == typeof(string) || value.GetType().IsPrimitive)
            {
                if (value.GetType() == typeof(string))
                    Console.WriteLine($@"{prefix.PadRight(50, ' ')}""{value}""");
                else if (value.GetType() == typeof(int) || value.GetType() == typeof(uint))
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
