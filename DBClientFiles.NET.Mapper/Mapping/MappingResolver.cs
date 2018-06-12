using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using DBClientFiles.NET.Attributes;
using DBClientFiles.NET.Collections.Generic;
using DBClientFiles.NET.Definitions;
using DBClientFiles.NET.Definitions.Attributes;
using DBClientFiles.NET.Internals;
using DBClientFiles.NET.Mapper.Generator;
using DBClientFiles.NET.Mapper.Utils;

namespace DBClientFiles.NET.Mapper.Mapping
{
    public class MappingResolver : Dictionary<MemberInfo /* to */, MappingResolver.ResolvedMapping>
    {
        public class ResolvedMapping 
        {
            public MemberInfo From { get; set; }
            public List<MemberInfo> Candidates { get; } = new List<MemberInfo>();
        }

        public Type Type { get; }

        public MappingResolver(FileAnalyzer source, FileAnalyzer target)
        {
            #region setup default source if none - should never be true if you want relevant names

            if (source.RecordType == null)
            {
                var sourceTypeGenerator = CreateTypeFromAnalyzer(source, "SourceType");
                
                source.Stream.Position = 0;
                source = new FileAnalyzer(sourceTypeGenerator.Generate(), source.Stream, source.Options);
                source.Analyze();

                if (AdjustStringMembers(sourceTypeGenerator, source))
                {
                    source.Stream.Position = 0;
                    source = new FileAnalyzer(sourceTypeGenerator.Generate(), source.Stream, source.Options);
                    source.Analyze();
                }
            }
            #endregion

            #region Setup dummy target structure
            if (target.RecordType == null)
            {
                var targetTypeGenerator = CreateTypeFromAnalyzer(target, "TargetType");

                target.Stream.Position = 0;
                target = new FileAnalyzer(targetTypeGenerator.Generate(), target.Stream, target.Options);
                target.Analyze();

                if (AdjustStringMembers(targetTypeGenerator, target))
                {
                    target.Stream.Position = 0;
                    target = new FileAnalyzer(targetTypeGenerator.Generate(), target.Stream, target.Options);
                    target.Analyze();
                }
            }
            #endregion

            if (source.IndexColumn != target.IndexColumn)
                Console.WriteLine("Index column moved!");

            // Construct containers for each node
            var sourceList = CreateStore(source);
            var targetList = CreateStore(target);

            var mappingStore = new Dictionary<MemberInfo, List<MemberInfo>>();
            #region mapping
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

                    var itr = 0;
                    while (mappingList.Count != 0 && itr < mappingList.Count)
                    {
                        if (mappingList.Count == 1)
                            break;

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

                        if (!valueMatch)
                            mappingList.Remove(targetMemberInfo);
                        else
                            ++itr;

                        if (mappingList.Count == 1)
                        {
                            break;
                        }
                    }
                }
            }
            #endregion
            
            // Create mappings
            foreach (var t in target.Members.Members.OrderBy(m => m.MemberInfo.GetCustomAttribute<OrderAttribute>().Order))
                this[t.MemberInfo] = new ResolvedMapping();
            
            // Fill mappings
            foreach (var mapInfo in mappingStore)
            {
                foreach (var targetNode in mapInfo.Value)
                {
                    if (!TryGetValue(targetNode, out var map))
                        map = this[targetNode] = new ResolvedMapping();

                    map.Candidates.Add(mapInfo.Key);
                }
            }

            // if only one candidate is found, map the match
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

            // Generate resolved type and expose it
            var typeGen = new TypeGenerator();
            foreach (var kv in this)
            {
                if (kv.Value.From != null)
                {
                    typeGen.CreateProperty(kv.Value.From.Name,
                        kv.Key.GetMemberType(),
                        kv.Value.From.GetCustomAttribute<CardinalityAttribute>()?.SizeConst ?? 1,
                        kv.Key.IsDefined(typeof(IndexAttribute), false));
                }
                else if (kv.Value.Candidates.Count > 1)
                {
                    typeGen.CreateProperty("unverified_" + kv.Key.GetCustomAttribute<OrderAttribute>().Order,
                        kv.Key.GetMemberType(),
                        kv.Key.GetCustomAttribute<CardinalityAttribute>()?.SizeConst ?? 1,
                        kv.Key.IsDefined(typeof(IndexAttribute), false));
                }
            }
            
            foreach (var attr in target.RecordType.GetCustomAttributes<LayoutAttribute>())
                typeGen.AddAttribute(attr.GetType().GetConstructor(new[] { typeof(uint) }), new object[] { attr.LayoutHash });

            foreach (var attr in target.RecordType.GetCustomAttributes<BuildAttribute>())
                typeGen.AddAttribute(attr.GetType().GetConstructor(new[] { typeof(BuildInfo) } ), new object[] { attr.Build });

            foreach (var attr in target.RecordType.GetCustomAttributes<BuildRangeAttribute>())
                typeGen.AddAttribute(attr.GetType().GetConstructor(new[] { typeof(BuildInfo), typeof(BuildInfo) }), new object[] { attr.From, attr.To });

            Type = typeGen.Generate();
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
