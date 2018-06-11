using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DBClientFiles.NET.Definitions.Parsers;

namespace DBClientFiles.NET.Mapper.Definitions
{
    internal static class DefinitionFactory
    {
        public static DBD Open(string definitionName)
        {
            var completePath = Path.Combine(Properties.Settings.Default.DefinitionRoot, definitionName + ".dbd");

            using (var fs = File.OpenRead(completePath))
                return new DBD(definitionName, fs);
        }
    }
}
