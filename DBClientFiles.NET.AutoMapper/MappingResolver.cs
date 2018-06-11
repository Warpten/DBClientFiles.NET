using DBClientFiles.NET.Collections.Generic;
using DBClientFiles.NET.Definitions;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using DBClientFiles.NET.Definitions.Attributes;
using DBClientFiles.NET.Internals;

namespace DBClientFiles.NET.AutoMapper
{
    public unsafe class MappingResolver : Dictionary<MemberInfo /* to */, MappingResolver.ResolvedMapping>
    {
        public class ResolvedMapping 
        {
            public MemberInfo From { get; set; }
            public List<MemberInfo> Candidates { get; } = new List<MemberInfo>();
        }

        private TypeGenerator _sourceTypeGenerator;
        private TypeGenerator _targetTypeGenerator;

        public MappingResolver(FileAnalyzer source, FileAnalyzer target)
        {
            if (source.RecordType == null)
            {
                _sourceTypeGenerator = CreateTypeFromAnalyzer(source, "SourceType");
                
                source.Stream.Position = 0;
                source = new FileAnalyzer(_sourceTypeGenerator.Generate(), source.Stream, source.Options);
                source.Analyze();

                if (AdjustStringMembers(_sourceTypeGenerator, source))
                {
                    source.Stream.Position = 0;
                    source = new FileAnalyzer(_sourceTypeGenerator.Generate(), source.Stream, source.Options);
                    source.Analyze();
                }
            }

            if (target.RecordType == null)
            {
                _targetTypeGenerator = CreateTypeFromAnalyzer(target, "TargetType");

                target.Stream.Position = 0;
                target = new FileAnalyzer(_targetTypeGenerator.Generate(), target.Stream, target.Options);
                target.Analyze();

                if (AdjustStringMembers(_targetTypeGenerator, target))
                {
                    target.Stream.Position = 0;
                    target = new FileAnalyzer(_targetTypeGenerator.Generate(), target.Stream, target.Options);
                    target.Analyze();
                }
            }

            if (source.IndexColumn != target.IndexColumn)
                Console.WriteLine("Index column moved!");

            // Construct containers for each node
            var sourceList = CreateStore(source);
            var targetList = CreateStore(target);

            var mappingStore = new Dictionary<MemberInfo, List<MemberInfo>>();

            foreach (var sourceKey in sourceList.Keys)
            {
                if (!targetList.Contains(sourceKey))
                    continue;

                var sourceElement = sourceList[sourceKey];
                object targetElement;
                try
                {
                    targetElement = targetList[sourceKey];
                }
                catch
                {
                    // Silence the exception (which means item does not belong to collection)
                    continue;
                }

                foreach (var sourceExtendedMemberInfo in source.Members.Members)
                {
                    if (this.Any(m => m.Value.From == sourceExtendedMemberInfo.MemberInfo))
                        break;

                    var sourceMemberInfo = sourceExtendedMemberInfo.MemberInfo;
                    var sourceMemberValue = BoxToIntOrSelf((sourceMemberInfo as PropertyInfo)?.GetValue(sourceElement));
                    if (sourceMemberValue == null)
                        continue;

                    if (!mappingStore.TryGetValue(sourceMemberInfo, out var mappingList))
                    {
                        // At first, pretend everything matches
                        mappingStore[sourceMemberInfo] = mappingList = new List<MemberInfo>();
                        mappingList.AddRange(target.Members.Members.Select(m => m.MemberInfo));
                    }
                    else if (mappingList.Count == 1)
                        continue;

                    Console.WriteLine($"[#{sourceKey}] source.{sourceMemberInfo.Name} = {sourceMemberValue}");

                    var itr = 0;
                    while (mappingList.Count != 0 && itr < mappingList.Count)
                    {
                        if (mappingList.Count == 1)
                        {
                            Console.WriteLine($"     [RESOLVED] target.{mappingList[0].Name}");
                            break;
                        }

                        var targetMemberInfo = mappingList[itr] as PropertyInfo;
                        if (targetMemberInfo == null)
                            throw new InvalidOperationException("Unreachable");

                        var targetMemberValue = BoxToIntOrSelf(targetMemberInfo.GetValue(targetElement));

                        bool valueMatch = sourceMemberValue.Equals(targetMemberValue);
                        if (sourceMemberValue is Array arrSource && targetMemberValue is Array arrTarget)
                        {
                            valueMatch = true;
                            for (var i = 0; i < arrSource.Length && i < arrTarget.Length && valueMatch; ++i)
                            {
                                valueMatch = BoxToIntOrSelf(arrSource.GetValue(i)).Equals(BoxToIntOrSelf(arrTarget.GetValue(i)));
                            }
                        }

                        Console.WriteLine($"     [#{sourceKey}] Testing against target.{targetMemberInfo.Name} ({targetMemberValue}) [{(!valueMatch ? "MISMATCH" : "MATCHES")}]");

                        if (!valueMatch)
                            mappingList.Remove(targetMemberInfo);
                        else
                            ++itr;

                        if (mappingList.Count == 1)
                        {
                            Console.WriteLine($"     [RESOLVED] target.{mappingList[0].Name}");
                            break;
                        }
                    }
                }
            }

            // invert the map
            var iterationCount = mappingStore.Count;
            foreach (var mapInfo in mappingStore)
            {
                foreach (var targetNode in mapInfo.Value)
                {
                    if (!TryGetValue(targetNode, out var map))
                        map = this[targetNode] = new ResolvedMapping();

                    map.Candidates.Add(mapInfo.Key);
                }
            }

            // if only one candidate is found, map it
            foreach (var t in this)
            {
                if (t.Value.Candidates.Count == 1)
                    t.Value.From = t.Value.Candidates[0];
            }

            // Sort the set by key, and assing it back to us
            var sortedSet = this.OrderBy(kv => kv.Key.GetCustomAttribute<OrderAttribute>().Order).ToArray();
            Clear();
            foreach (var kv in sortedSet)
                Add(kv.Key, kv.Value);
        }

        private IDictionary CreateStore(FileAnalyzer source)
        {
            var dictType = typeof(StorageDictionary<,>).MakeGenericType(source.Members.IndexMember.Type, source.RecordType);

            source.Stream.Position = 0;
            return (IDictionary) Activator.CreateInstance(dictType, source.Stream, source.Options);
        }

        private bool AdjustStringMembers(TypeGenerator generator, FileAnalyzer analyzer)
        {
            analyzer.Stream.Position = 0;

            var enumerableType = typeof(StorageEnumerable<>).MakeGenericType(analyzer.RecordType);
            var enumerable = (IEnumerable) Activator.CreateInstance(enumerableType, analyzer.Stream, analyzer.Options);

            var isValidString = new bool[analyzer.Members.FileMembers.Count];
            for (var itr = 0; itr < isValidString.Length; ++itr)
                isValidString[itr] = false;
            
            foreach (var node in enumerable)
            {
                var memberIndex = 0;
                foreach (var exMemberInfo in analyzer.Members.Members)
                {
                    if (memberIndex == analyzer.Members.IndexColumn)
                        continue;

                    var memberInfo = (PropertyInfo)exMemberInfo.MemberInfo;
                    var memberValue = memberInfo.GetValue(node);
                    
                    if (exMemberInfo.Type == typeof(string) && exMemberInfo.MappedTo.BitSize > 16)
                        isValidString[memberIndex] = memberValue != null;
                    //{
                    //    if (analyzer.Signature == Signatures.WDC2)
                    //        intValue = (int)(recordOffset + memberOffsetInRecord / 8 + intValue);

                    //    if (((IStorage) enumerable).StringTable.ContainsKey(intValue))
                    //    {
                    //        var memberStringValue = ((IStorage) enumerable).StringTable[intValue];

                    //        // String table returns null if not in table (empty is a valid string!)
                    //        if (exMemberInfo.MappedTo.BitSize > 16)
                    //            isValidString[memberIndex] &= memberStringValue != null;
                    //    }
                    //    else
                    //        isValidString[memberIndex] = false;
                    //}

                    ++memberIndex;
                }
            }

            for (var itr = 0; itr < isValidString.Length; ++itr)
                if (!isValidString[itr])
                    generator.GetMember(itr).Type = typeof(int);

            if (isValidString.Any())
                generator.Generate();

            return isValidString.Any();
        }

        private TypeGenerator CreateTypeFromAnalyzer(FileAnalyzer source, string name)
        {
            var typeGen = new TypeGenerator(name);
            
            foreach (var memberInfo in source.Members.FileMembers)
            {
                var fieldName = $"UnkMember_{memberInfo.Index}";
                var fieldType = typeof(string);
                if (memberInfo.Index == source.Members.IndexColumn)
                {
                    fieldType = typeof(int);
                }

                switch (memberInfo.CompressionType)
                {
                    case MemberCompressionType.BitpackedPalletArrayData:
                    case MemberCompressionType.BitpackedPalletData:
                    case MemberCompressionType.CommonData:
                        fieldType = typeof(int);
                        break;
                }
                
                // We don't really care if we are an int or whatever, because bit sizes automatically make us properly deserialize.
                if (memberInfo.Cardinality > 1)
                    fieldType = fieldType.MakeArrayType();

                typeGen.CreateProperty(fieldName, fieldType, memberInfo.Cardinality, memberInfo.Index == source.Members.IndexColumn);
            }

            return typeGen;
        }

        public static object BoxToIntOrSelf(object t)
        {
            if (t is ushort sourceUshort)
                return (int)new Value16 { UInt16 = sourceUshort }.Int16;

            if (t is short sourceShort)
                return (int) sourceShort;

            if (t is byte sourceByte)
                return (int)new Value8 { UInt8 = sourceByte }.Int8;

            if (t is sbyte sourceSByte)
                return (int) sourceSByte;

            if (t is float f)
                return new Value32 { Single = f }.Int32;

            return t;
        }
    }
}
