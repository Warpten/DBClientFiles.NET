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
        private struct ResolvedMapping
        {
            public MemberInfo From { get; set; }
            public MemberInfo To { get; set; }
        }

        private readonly List<ResolvedMapping> _resolvedMappings = new List<ResolvedMapping>();
        private List<ExtendedMemberInfo> _availableTargetPool = new List<ExtendedMemberInfo>();
        private List<ExtendedMemberInfo> _availableSourcePool = new List<ExtendedMemberInfo>();
        private AssemblyBuilder _assemblyBuilder;
        private ModuleBuilder _module;

        private readonly Dictionary<long, string> _sourceStrings = new Dictionary<long, string>();
        private readonly Dictionary<long, string> _targetStrings = new Dictionary<long, string>();

        public MappingResolver(FileAnalyzer source, FileAnalyzer target)
        {
            var assemblyName = new AssemblyName { Name = "TemporaryAssembly" };
            _assemblyBuilder = Thread.GetDomain().DefineDynamicAssembly(assemblyName, AssemblyBuilderAccess.Run);
            _module = _assemblyBuilder.DefineDynamicModule("TemporaryModule");

            if (source.RecordType == null)
            {
                //todo : should throw here, anchor needs to exist
                source.Stream.Position = 0;
                source = new FileAnalyzer(CreateTypeFromAnalyzer(source, "SourceType"), source.Stream, source.Options);
                source.Analyze();
            }

            if (target.RecordType == null)
            {
                target.Stream.Position = 0;
                target = new FileAnalyzer(CreateTypeFromAnalyzer(target, "TargetType"), target.Stream, target.Options);
                target.Analyze();
            }

            if (source.IndexColumn != target.IndexColumn)
                Console.WriteLine("Index column moved!");

            _availableSourcePool.AddRange(source.Members.Members);
            _availableTargetPool.AddRange(target.Members.Members);

            // Do a simple pass where we map by array sizes only.
            // We have to look for members with unique sizes
            
            //var availableArraySourcePool = _availableSourcePool.UniqueBy(n => n.Cardinality).ToArray();
            //var availableArrayTargetPool = _availableTargetPool.UniqueBy(n => n.Cardinality).ToArray();
            //if (availableArraySourcePool.Length != 0 && availableArrayTargetPool.Length != 0)
            //{
            //    foreach (var availableArraySource in availableArraySourcePool)
            //    {
            //        foreach (var availableArrayTarget in availableArrayTargetPool)
            //        {
            //            if (availableArrayTarget.Cardinality != availableArraySource.Cardinality)
            //                continue;

            //            _resolvedMappings.Add(new ResolvedMapping
            //            {
            //                From = availableArraySource.MemberInfo,
            //                To = availableArrayTarget.MemberInfo
            //            });
            //            break;
            //        }
            //    }

            //    // Prune out used nodes from each pool
            //    foreach (var resolvedMapping in _resolvedMappings)
            //    {
            //        _availableSourcePool.Remove(resolvedMapping.From);
            //        _availableTargetPool.Remove(resolvedMapping.To);
            //    }
            //}

            // This time, we are gonna need to enumerate records and compare members one-by-one.
            // Given a set of values for a SOURCE column, we will iterate the entirety of the TARGET's columns, and build a list 
            
            // Construct containers for each node
            var sourceList = CreateStore(source.RecordType, source.Members.IndexMember.Type, source.Options, source.Stream);
            var targetList = CreateStore(target.RecordType, target.Members.IndexMember.Type, target.Options, target.Stream);
            var sourceStorage = (IStorage)sourceList;
            var targetStorage = (IStorage)targetList;

            var mappingStore = new Dictionary<MemberInfo, List<MemberInfo>>();

            foreach (var sourceKey in sourceList.Keys)
            {
                if (!targetList.Contains(sourceKey))
                    continue;

                var sourceElement = sourceList[sourceKey];
                object targetElement;
                try {
                    targetElement = targetList[sourceKey];
                } catch { // Silence the exception (which means item does not belong to collection)
                    continue;
                }

                foreach (var sourceExtendedMemberInfo in source.Members.Members)
                {
                    var sourceMemberInfo = sourceExtendedMemberInfo.MemberInfo;
                    var sourceMemberValue = (sourceMemberInfo as PropertyInfo)?.GetValue(sourceElement);
                    if (sourceMemberValue == null)
                        continue;

                    Console.WriteLine($"[*] source.{sourceMemberInfo.Name} = {sourceMemberValue}");

                    if (!mappingStore.TryGetValue(sourceMemberInfo, out var mappingList))
                    {
                        // At first, pretend everything matches
                        mappingStore[sourceMemberInfo] = mappingList = new List<MemberInfo>();
                        mappingList.AddRange(target.Members.Members.Select(m => m.MemberInfo));
                    }

                    var itr = 0;
                    while (mappingList.Count != 0 && itr < mappingList.Count)
                    {
                        var targetMemberInfo = mappingList[itr] as PropertyInfo;
                        if (targetMemberInfo == null)
                            throw new InvalidOperationException("Unreachable");

                        var targetMemberValue = targetMemberInfo.GetValue(targetElement);
                        
                        Console.WriteLine($"     Testing against target.{targetMemberInfo.Name} ({targetMemberValue})");
                        
                        var isSourceStringRef = sourceExtendedMemberInfo.Type == typeof(int);
                        var isTargetStringRef = targetMemberValue is int;

                        //! TODO: For WDC2 offsets read as int are now relative to its start.
                        //! TODO: This is gross.
                        if (isSourceStringRef)
                        {
                            var stringTableOffset = (int)sourceMemberValue;

                            if (source.Signature == Signatures.WDC2)
                            {
                                using (var binaryReader = new BinaryReader(source.Stream, Encoding.UTF8, true))
                                {
                                    source.Stream.Position = stringTableOffset;
                                    sourceMemberValue = binaryReader.ReadCString();
                                }
                            }
                            else
                            {
                                if (sourceStorage.StringTable.ContainsKey(stringTableOffset))
                                    sourceMemberValue = sourceStorage.StringTable[stringTableOffset];
                            }
                        }

                        if (isTargetStringRef)
                        {
                            var stringTableOffset = (int)targetMemberValue;
                            if (target.Signature == Signatures.WDC2)
                            {
                                using (var binaryReader = new BinaryReader(target.Stream, Encoding.UTF8, true))
                                {
                                    target.Stream.Position = stringTableOffset;
                                    targetMemberValue = binaryReader.ReadCString();
                                }
                            }
                            else
                            {
                                if (sourceStorage.StringTable.ContainsKey(stringTableOffset))
                                    targetMemberValue = sourceStorage.StringTable[stringTableOffset];
                            }
                        }

                        if (!targetMemberValue.Equals(sourceMemberValue))
                        {
                            mappingList.Remove(targetMemberInfo);
                            itr = 0;
                        }
                        else ++itr;
                    }
                }
            }

            for (var i = 0; i < mappingStore.Count;)
            {
                var possibleMappings = mappingStore.ElementAt(i);

                if (possibleMappings.Value.Count == 1)
                {
                    _resolvedMappings.Add(new ResolvedMapping
                    {
                        From = possibleMappings.Key,
                        To = possibleMappings.Value[0]
                    });

                    mappingStore.Remove(possibleMappings.Key);

                    i = 0;
                }
                else
                    ++i;
            }
        }

        private Type CreateTypeFromAnalyzer(FileAnalyzer target, string name = null)
        {
            if (target.Members.FileMembers.Count == 0)
                throw new InvalidOperationException();

            var randomTypeName = Path.GetRandomFileName().GetHashCode().ToString();

            var typeBuilder = _module.DefineType(name ?? $"GeneratedType_{randomTypeName}");

            var fileMemberIndex = 0;
            foreach (var fileMemberInfo in target.Members.FileMembers)
            {
                if (fileMemberIndex == target.Members.IndexColumn && target.Members.HasIndexTable && target.Signature != Signatures.WDC2)
                {
                    DefineProperty(typeBuilder, typeof(int), "ID").SetCustomAttribute(new CustomAttributeBuilder(typeof(IndexAttribute).GetConstructor(Type.EmptyTypes), new object[0]));
                    ++fileMemberIndex;
                }

                var isIndexInlinedMember = fileMemberIndex == target.Members.IndexColumn && !target.Members.HasIndexTable;

                var propertyType = typeof(int);
                if (fileMemberInfo.ByteSize == 8 || (fileMemberInfo.BitSize > 32 && fileMemberInfo.BitSize <= 64))
                    propertyType = typeof(long);
                if (fileMemberInfo.ByteSize == 2 || (fileMemberInfo.BitSize > 8 && fileMemberInfo.BitSize <= 16))
                    propertyType = typeof(short);
                else if (fileMemberInfo.ByteSize == 1 || fileMemberInfo.BitSize <= 8)
                    propertyType = typeof(byte);

                if (fileMemberInfo.Cardinality > 1)
                    propertyType = propertyType.MakeArrayType(fileMemberInfo.Cardinality);

                var propBuilder = DefineProperty(typeBuilder, propertyType, isIndexInlinedMember ? "ID" : $"UnkMember{fileMemberIndex}");

                if (fileMemberInfo.Cardinality > 1)
                {
                    propBuilder.SetCustomAttribute(new CustomAttributeBuilder(
                        typeof(CardinalityAttribute).GetConstructor(Type.EmptyTypes),
                        new object[0],
                        new [] { typeof(CardinalityAttribute).GetProperty("SizeConst") },
                        new object[] { fileMemberInfo.Cardinality }));
                }

                if (isIndexInlinedMember)
                {
                    propBuilder.SetCustomAttribute(new CustomAttributeBuilder(typeof(IndexAttribute).GetConstructor(Type.EmptyTypes), new object[0]));
                }

                ++fileMemberIndex;
            }

            return typeBuilder.CreateType();
        }

        private IDictionary CreateStore(Type instanceType, Type keyType, StorageOptions options, Stream inputStream)
        {
            inputStream.Position = 0;

            var dictionaryType = typeof(StorageDictionary<,>).MakeGenericType(keyType, instanceType);

            var instance = Activator.CreateInstance(dictionaryType, inputStream, options);
            return (IDictionary) instance;
        }

        private PropertyBuilder DefineProperty(TypeBuilder typeBuilder, Type propType, string propName)
        {
            var fieldBuilder = typeBuilder.DefineField($"{propName}_backingField", propType, FieldAttributes.Private | FieldAttributes.SpecialName);

            var propBuilder = typeBuilder.DefineProperty(propName, PropertyAttributes.None, propType, Type.EmptyTypes);
            var getSetAttr = MethodAttributes.Public | MethodAttributes.SpecialName | MethodAttributes.HideBySig;

            var getBuilder = typeBuilder.DefineMethod($"get_{propName}", getSetAttr, propType, Type.EmptyTypes);
            var getGenerator = getBuilder.GetILGenerator();

            getGenerator.Emit(OpCodes.Ldarg_0);
            getGenerator.Emit(OpCodes.Ldfld, fieldBuilder);
            getGenerator.Emit(OpCodes.Ret);

            var setBuilder = typeBuilder.DefineMethod($"set_{propName}", getSetAttr, null, new[] { propType });
            var setGenerator = setBuilder.GetILGenerator();

            setGenerator.Emit(OpCodes.Ldarg_0);
            setGenerator.Emit(OpCodes.Ldarg_1);
            setGenerator.Emit(OpCodes.Stfld, fieldBuilder);
            setGenerator.Emit(OpCodes.Ret);

            propBuilder.SetSetMethod(setBuilder);
            propBuilder.SetGetMethod(getBuilder);

            return propBuilder;
        }
    }
}
