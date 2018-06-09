using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DBClientFiles.NET.AutoMapper
{
    class Program
    {
        public static void Main(string[] args)
        {
            var sourceArg = Array.IndexOf(args, "--source");
            var targetArg = Array.IndexOf(args, "--target");
            if (sourceArg == -1 || targetArg == -1)
                return;

            using (var sourceFile = File.OpenRead(args[sourceArg + 1]))
            using (var targetFile = File.OpenRead(args[targetArg + 1]))
            {
                var sourceAnalyzer = AnalyzerFactory.Create(sourceFile);
                var targetAnalyzer = AnalyzerFactory.Create(targetFile);
                
                var mappingResolver = new MappingResolver(sourceAnalyzer, targetAnalyzer);
                Console.WriteLine("Mapping done");
            }
            
        }
    }
}
