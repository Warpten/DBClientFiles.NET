using DBClientFiles.NET.Attributes;
using DBClientFiles.NET.Collections;
using DBClientFiles.NET.Collections.Generic;
using DBClientFiles.NET.Definitions;
using DBClientFiles.NET.Internals.Binding;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading;
using DBClientFiles.NET.AutoMapper.Utils;
using DBClientFiles.NET.Internals;

namespace DBClientFiles.NET.AutoMapper
{
    public class MappingResolver
    {
        private class ResolvedMapping
        {
            public MemberInfo From { get; set; }
            public MemberInfo To { get; set; }
            public List<MemberInfo> Candidates { get; } = new List<MemberInfo>();
        }

        private readonly List<ResolvedMapping> _resolvedMappings = new List<ResolvedMapping>();
        private List<ExtendedMemberInfo> _availableTargetPool = new List<ExtendedMemberInfo>();
        private List<ExtendedMemberInfo> _availableSourcePool = new List<ExtendedMemberInfo>();

        public int Count => _resolvedMappings.Count;

        private TypeGenerator _sourceTypeGenerator;
        private TypeGenerator _targetTypeGenerator;

        public MappingResolver(FileAnalyzer source, FileAnalyzer target)
        {
            if (source.RecordType == null)
            {
                _sourceTypeGenerator = CreateTypeFromAnalyzer(source, "SourceType");

                //todo : should throw here, anchor needs to exist
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

            _availableSourcePool.AddRange(source.Members.Members);
            _availableTargetPool.AddRange(target.Members.Members);

            // Do a simple pass where we map by array sizes only.
            // We have to look for members with unique sizes

            var availableArraySourcePool = _availableSourcePool.UniqueBy(n => n.Cardinality).ToArray();
            var availableArrayTargetPool = _availableTargetPool.UniqueBy(n => n.Cardinality).ToArray();
            if (availableArraySourcePool.Length != 0 && availableArrayTargetPool.Length != 0)
            {
                foreach (var availableArraySource in availableArraySourcePool)
                {
                    foreach (var availableArrayTarget in availableArrayTargetPool)
                    {
                        if (availableArrayTarget.Cardinality != availableArraySource.Cardinality)
                            continue;

                        _resolvedMappings.Add(new ResolvedMapping
                        {
                            From = availableArraySource.MemberInfo,
                            To = availableArrayTarget.MemberInfo
                        });
                        break;
                    }
                }

                // Prune out used nodes from each pool
                foreach (var resolvedMapping in _resolvedMappings)
                {
                    _availableSourcePool.RemoveWhere(m => m.MemberInfo == resolvedMapping.From);
                    _availableTargetPool.RemoveWhere(m => m.MemberInfo == resolvedMapping.To);
                }
            }

            // This time, we are gonna need to enumerate records and compare members one-by-one.
            // Given a set of values for a SOURCE column, we will iterate the entirety of the TARGET's columns, and build a list 

            // Construct containers for each node
            var sourceList = CreateStore(source);
            var targetList = CreateStore(target);
            var sourceStorage = (IStorage) sourceList;
            var targetStorage = (IStorage) targetList;

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
                    if (_resolvedMappings.Any(m => m.From == sourceExtendedMemberInfo.MemberInfo))
                        break;

                    var sourceMemberInfo = sourceExtendedMemberInfo.MemberInfo;
                    var sourceMemberValue = (sourceMemberInfo as PropertyInfo)?.GetValue(sourceElement);
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
                            Console.WriteLine("     [RESOLVED]");
                            break;
                        }

                        var targetMemberInfo = mappingList[itr] as PropertyInfo;
                        if (targetMemberInfo == null)
                            throw new InvalidOperationException("Unreachable");

                        var targetMemberValue = targetMemberInfo.GetValue(targetElement);

                        bool valueMatch = sourceMemberValue.Equals(targetMemberValue);

                        Console.WriteLine(
                            $"     [#{sourceKey}] Testing against target.{targetMemberInfo.Name} ({targetMemberValue}) [{(!valueMatch ? "MISMATCH" : "MATCHES")}]");

                        if (!valueMatch)
                            mappingList.Remove(targetMemberInfo);
                        else
                            ++itr;
                    }
                }
            }

            // Map all the members we are certain about first.
            // We do this a couple of times. This should be recursive but cba.
            var iterationCount = mappingStore.Count;
            while (iterationCount != 0)
            {
                var uniqueMatches = mappingStore.Where(m => m.Value.Count == 1).ToArray(); // Gay way to copy from source
                foreach (var uniqueMatch in uniqueMatches)
                {
                    if (_resolvedMappings.All(m => m.From != uniqueMatch.Key))
                    {
                        _resolvedMappings.Add(new ResolvedMapping
                        {
                            From = uniqueMatch.Key,
                            To = uniqueMatch.Value[0]
                        });

                        // Remove ourselves from the unresolved pool
                        mappingStore.Remove(uniqueMatch.Key);
                    }
                }

                --iterationCount;
            }

            // The remains get pooled
            foreach (var sourceExtendedMemberInfo in source.Members.Members)
            {
                var sourceMemberInfo = sourceExtendedMemberInfo.MemberInfo;
                if (!mappingStore.TryGetValue(sourceMemberInfo, out var mappingList))
                    continue;

                _resolvedMappings.First(f => f.From == sourceMemberInfo).Candidates.AddRange(mappingList);
            }
        }

        public string this[int index]
        {
            get
            {
                var node = _resolvedMappings[index];
                if (node.Candidates.Count > 1)
                    return $"source.{node.From.Name} = Either({{ {string.Join(", ", node.Candidates)} }})";
                else if (node.Candidates.Count == 0 && node.To == null)
                    return $"source.{node.From.Name} = ????";
                else
                    return $"source.{node.From.Name} = target.{node.To.Name}";
            }
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
                var fieldName = $"UnkMember{memberInfo.Index}";
                var fieldType = typeof(string);
                if (memberInfo.Index == source.Members.IndexColumn)
                {
                    fieldName = "ID";
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

                typeGen.CreateProperty(fieldName, fieldType, memberInfo.Cardinality, fieldName == "ID");
            }

            return typeGen;
        }
        
        
    }
}
