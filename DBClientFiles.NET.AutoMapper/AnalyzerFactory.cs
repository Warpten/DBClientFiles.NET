using System;
using System.IO;
using DBClientFiles.NET.Collections;
using DBClientFiles.NET.Definitions;

namespace DBClientFiles.NET.AutoMapper
{
    public static class AnalyzerFactory
    {
        public static FileAnalyzer Create(Type recordType, Stream fileStream)
        {
            var options = new StorageOptions
            {
                LoadMask = LoadMask.Records | LoadMask.StringTable,
                CopyToMemory = false,
                InternStrings = true,
                MemberType = System.Reflection.MemberTypes.Property,
                OverrideSignedChecks = true
            };

            fileStream.Position = 0;
            var instance = new FileAnalyzer(recordType, fileStream, options);
            instance.Analyze();
            return instance;
        }

        public static FileAnalyzer Create(Stream fileStream)
        {
            return Create(null, fileStream);
        }
    }
}
