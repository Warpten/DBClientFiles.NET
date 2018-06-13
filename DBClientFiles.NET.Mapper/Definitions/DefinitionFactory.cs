using System;
using System.IO;
using DBClientFiles.NET.Definitions.Parsers;

namespace DBClientFiles.NET.Mapper.Definitions
{
    internal static class DefinitionFactory
    {
        public static DBD Open(string definitionName, string rootFolder = null)
        {
            var completePath = Path.Combine(rootFolder ?? Properties.Settings.Default.DefinitionRoot, definitionName + ".dbd");

            using (var fs = File.OpenRead(completePath))
                return new DBD(definitionName, fs);
        }

        public static void Save(string definitionName, Type newDefinition)
        {
            var completePath = Path.Combine(Properties.Settings.Default.DefinitionRoot, definitionName + ".dbd");
            
            DBD definition;
            using (var fs = new FileStream(completePath, FileMode.Open))
            {
                definition = new DBD(definitionName, fs);
                definition.AddType(newDefinition);
            }

            using (var fs = new FileStream(completePath, FileMode.Truncate))
                definition.Save(fs);
        }
    }
}
