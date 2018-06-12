using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using DBClientFiles.NET.Mapper.Definitions;
using DBClientFiles.NET.Mapper.Mapping;
using DBClientFiles.NET.Mapper.UI.Forms;

namespace DBClientFiles.NET.Mapper
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            var definitions = FindArgument(args, "--defs", "-d") ?? Properties.Settings.Default.DefinitionRoot;
            if (args == null || args.Length == 0)
            {
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                Application.Run(new MainForm());
            }
            else if (Array.IndexOf(args, "--batch") != -1)
            {
                var sourceFiles = Directory.GetFiles(@"./source/");
                var targetFiles = Directory.GetFiles(@"./target/");

                foreach (var sourceFile in sourceFiles)
                {
                    var fileName = Path.GetFileName(sourceFile);
                    var targetFile = targetFiles.First(f => f == fileName);

                    var resolver = MapFiles(sourceFile, targetFile, definitions);
                    DefinitionFactory.Save(fileName, resolver.Type);
                }
            }
            else
            {
                var sourceFile = FindArgument(args, "--source", "-s");
                var targetFile = FindArgument(args, "--target", "-t");

                var resolver = MapFiles(sourceFile, targetFile, definitions);
                if (resolver == null)
                    Console.WriteLine("Unable to map files");


            }
        }

        private static MappingResolver MapFiles(string source, string target, string definitionFolder)
        {
            var definitionName = Path.GetFileNameWithoutExtension(source);

            var definitionFile = Path.Combine(definitionFolder, definitionName + ".dbd");
            var definitionStore = DefinitionFactory.Open(definitionFile);

            var sourceStream = File.OpenRead(source);
            var targetStream = File.OpenRead(target);

            var sourceAnalyzer = AnalyzerFactory.Create(sourceStream);
            var targetAnalyzer = AnalyzerFactory.Create(targetStream);

            var sourceType = definitionStore[sourceAnalyzer.LayoutHash];
            var targetType = definitionStore[targetAnalyzer.LayoutHash];

            if (sourceType == null)
            {
                sourceStream.Dispose();
                targetStream.Dispose();
                return null;
            }

            sourceAnalyzer = AnalyzerFactory.Create(sourceType, sourceStream);
            if (targetType != null)
                targetAnalyzer = AnalyzerFactory.Create(targetType, targetStream);

            return new MappingResolver(sourceAnalyzer, targetAnalyzer);
        }

        private static string FindArgument(string[] args, string key, string shorthand = null)
        {
            if (args == null)
                return null;

            var ofs = Array.IndexOf(args, key);
            if (shorthand != null && ofs == -1)
                ofs = Array.IndexOf(args, shorthand);

            if (ofs == -1 || ofs + 1 >= args.Length)
                return null;

            return args[ofs + 1];
        }
    }
}
