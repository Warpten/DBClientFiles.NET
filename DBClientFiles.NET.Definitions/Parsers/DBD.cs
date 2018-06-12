using DBClientFiles.NET.Attributes;
using DBClientFiles.NET.Definitions.Attributes;
using DBClientFiles.NET.Definitions.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
// ReSharper disable AssignNullToNotNullAttribute

namespace DBClientFiles.NET.Definitions.Parsers
{
    public sealed class DBD : StreamReader
    {
        private class ColumnDefinition
        {
            public Type Type;
            public string Name;
            public ForeignKeyInfo? ForeignKeyInfo;
            public bool Verified;
            public string Comments;
        }

        private class ColumnImplementation
        {
            public string Comment;
            public ColumnDefinition Definition;
            public int? Size;
            public int? Cardinality;
            public List<string> Annotations = new List<string>();
        }

        private readonly List<ColumnDefinition> _columnDefinitions = new List<ColumnDefinition>();
        private readonly List<uint> _layoutAttributes = new List<uint>();
        private readonly List<string> _buildAttributes = new List<string>();
        private readonly List<string> _buildRangeAttributes = new List<string>();
        private string _comment;

        private int _memberIndex;

        private readonly ModuleBuilder _module;
        private readonly string _fileName;

        private struct ForeignKeyInfo
        {
            public string Type;
            public string Name;
        }

        private readonly List<Type> _createdTypes = new List<Type>();

        public Type this[uint layoutHash]
        {
            get
            {
                foreach (var t in _createdTypes)
                    if (t.GetCustomAttributes<LayoutAttribute>().Any(attr => attr.LayoutHash == layoutHash))
                        return t;

                return null;
            }
        }

        public bool ContainsKey(uint layoutHash)
        {
            foreach (var t in _createdTypes)
                if (t.GetCustomAttributes<LayoutAttribute>().Any(attr => attr.LayoutHash == layoutHash))
                    return true;

            return false;
        }

        public DBD(string fileName, Stream fileStream) : base(fileStream, Encoding.UTF8)
        {
            _fileName = fileName;

            var assemblyName = new AssemblyName { Name = "TemporaryAssembly" };
            _assemblyBuilder = Thread.GetDomain().DefineDynamicAssembly(assemblyName, AssemblyBuilderAccess.Run);
            _module = _assemblyBuilder.DefineDynamicModule("TemporaryModule");
            
            string line;

            while ((line = ReadLine()) != null)
            {
                if (line.StartsWith("COLUMNS"))
                    ReadColumnDefinitions();
                else if (line.StartsWith("LAYOUT"))
                    ReadLayoutDefinition(line);
                else if (line.StartsWith("BUILD"))
                    ReadBuildDefinition(line);
                else if (line.StartsWith("COMMENT"))
                    ReadCommentDefinition(line);
                else 
                    ProduceType(line);
            }
        }

        private static readonly Regex _colUsageRegex = new Regex(@"^(?:\$(?<annotations>[a-z,]+)\$)?(?<name>[a-z0-9_]+)(?:<(?<size>u?[0-9]+)>)?(?:\[(?<array>[0-9]+)\])?(?: \/\/ (?<comments>.+))?$", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        private static readonly Regex _colRegex = new Regex(@"^(?<type>.+)(?:<(?<fk_type>.+)::(?<fk_field>.+)>)? (?<name>[a-z0-9_]+)(?<verified>\?)?(?: \/\/ (?<comments>.+))?$", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        private readonly AssemblyBuilder _assemblyBuilder;

        private void ReadColumnDefinitions()
        {
            string line;

            while ((line = ReadLine()) != null)
            {
                if (string.IsNullOrEmpty(line))
                    return;

                var tokens = _colRegex.Match(line);
                if (!tokens.Success)
                    throw new InvalidOperationException($"Unable to parse column definition '{line}'");

                var columnType = tokens.Groups["type"];
                var foreignKey = tokens.Groups["fk_type"];
                var foreignField = tokens.Groups["fk_field"];
                var columnName = tokens.Groups["name"];
                var comments = tokens.Groups["comments"];
                var verified = tokens.Groups["verified"];

                var columnDefinition = new ColumnDefinition {
                    Name = columnName.Value,
                    Type = columnType.Value.ToType()
                };

                if (foreignField.Success && foreignKey.Success)
                {
                    columnDefinition.ForeignKeyInfo = new ForeignKeyInfo()
                    {
                        Name = foreignField.Value,
                        Type = foreignKey.Value
                    };
                }
                if (comments.Success)
                    columnDefinition.Comments = comments.Value;

                columnDefinition.Verified = !verified.Success;
                
                _columnDefinitions.Add(columnDefinition);
            }
        }

        private void ReadLayoutDefinition(string line)
        {
            var tokens = line.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries).Skip(1).Select(s => Convert.ToUInt32(s, 16));
            foreach (var token in tokens)
                _layoutAttributes.Add(token);
        }

        private void ReadBuildDefinition(string line)
        {
            var tokenSource = line.Substring("BUILD ".Length);
            if (tokenSource.Contains('-'))
                _buildRangeAttributes.Add(tokenSource.Trim());
            else
            {
                var individual = tokenSource.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                foreach (var i in individual)
                    _buildAttributes.Add(i.Trim());
            }
        }

        private void ReadCommentDefinition(string line)
        {
            _comment = line.Substring("COMMENT ".Length);
        }

        private void ProduceType(string line)
        {
            var columns = new List<ColumnImplementation>();
            _memberIndex = 0;

            while (line != null)
            {
                if (string.IsNullOrEmpty(line))
                    break;

                var impl = new ColumnImplementation();

                var regexMatch = _colUsageRegex.Match(line);
                impl.Definition = _columnDefinitions.First(d => d.Name == regexMatch.Groups["name"].Value);

                if (regexMatch.Groups["size"].Success)
                    impl.Size = int.Parse(regexMatch.Groups["size"].Value);

                if (regexMatch.Groups["array"].Success)
                    impl.Cardinality = int.Parse(regexMatch.Groups["array"].Value);

                if (regexMatch.Groups["comments"].Success)
                    impl.Comment = regexMatch.Groups["comments"].Value;

                if (regexMatch.Groups["annotations"].Success)
                    impl.Annotations = regexMatch.Groups["annotations"].Value.Split(new[] {','}, StringSplitOptions.RemoveEmptyEntries).Select(v => v.Trim()).ToList();

                columns.Add(impl);

                line = ReadLine();
            }

            var type = _module.DefineType($"{_fileName}_{Math.Abs(Path.GetRandomFileName().GetHashCode())}");
            foreach (var layoutAttr in _layoutAttributes)
            {
                var attrBuilder = new CustomAttributeBuilder(typeof(LayoutAttribute).GetConstructor(new[] { typeof(uint) }), new object[] { layoutAttr });
                type.SetCustomAttribute(attrBuilder);
            }

            foreach (var buildAttr in _buildAttributes)
            {
                var attrBuilder = new CustomAttributeBuilder(typeof(BuildAttribute).GetConstructor(new[] { typeof(string) }), new object[] { buildAttr });
                type.SetCustomAttribute(attrBuilder);
            }

            foreach (var buildAttr in _buildRangeAttributes)
            {
                var attrBuilder = new CustomAttributeBuilder(typeof(BuildRangeAttribute).GetConstructor(new[] { typeof(string) }), new object[] { buildAttr });
                type.SetCustomAttribute(attrBuilder);
            }

            if (!string.IsNullOrEmpty(_comment))
            {
                var attrBuilder = new CustomAttributeBuilder(typeof(CommentAttribute).GetConstructor(new[] { typeof(string) }), new object[] { _comment });
                type.SetCustomAttribute(attrBuilder);
            }

            foreach (var colInfo in columns)
                GenerateMember(type, colInfo);

            _createdTypes.Add(type.CreateType());

            // Type produced, cleanup
            _layoutAttributes.Clear();
            _buildAttributes.Clear();
            _buildRangeAttributes.Clear();
            _comment = null;
        }

        private void GenerateMember(TypeBuilder typeBuilder, ColumnImplementation columnInfo)
        {
            var nodeType = columnInfo.Definition.Type;
            if (columnInfo.Size.HasValue)
                nodeType = nodeType.AdjustBitCount(columnInfo.Size.Value);

            if (columnInfo.Cardinality.HasValue && columnInfo.Cardinality.Value > 1)
                nodeType = nodeType.MakeArrayType();

            var backingField = typeBuilder.DefineField(columnInfo.Definition.Name + "_backingField", nodeType,
                FieldAttributes.Private | FieldAttributes.SpecialName);
            var memberBuilder = typeBuilder.DefineProperty(columnInfo.Definition.Name, PropertyAttributes.None, nodeType, Type.EmptyTypes);

            var getSetAttr = MethodAttributes.Public | MethodAttributes.SpecialName | MethodAttributes.HideBySig;

            var getBuilder = typeBuilder.DefineMethod($"get_{columnInfo.Definition.Name}", getSetAttr, nodeType, Type.EmptyTypes);
            var getGenerator = getBuilder.GetILGenerator();

            getGenerator.Emit(OpCodes.Ldarg_0);
            getGenerator.Emit(OpCodes.Ldfld, backingField);
            getGenerator.Emit(OpCodes.Ret);

            var setBuilder = typeBuilder.DefineMethod($"set_{columnInfo.Definition.Name}", getSetAttr, null, new[] { nodeType });
            var setGenerator = setBuilder.GetILGenerator();

            setGenerator.Emit(OpCodes.Ldarg_0);
            setGenerator.Emit(OpCodes.Ldarg_1);
            setGenerator.Emit(OpCodes.Stfld, backingField);
            setGenerator.Emit(OpCodes.Ret);

            memberBuilder.SetSetMethod(setBuilder);
            memberBuilder.SetGetMethod(getBuilder);

            CustomAttributeBuilder arraySize = null;
            if (columnInfo.Cardinality.HasValue)
                arraySize = new CustomAttributeBuilder(
                    typeof(CardinalityAttribute).GetConstructor(Type.EmptyTypes),
                    new object[] { }, 
                    new [] { typeof(CardinalityAttribute).GetProperty("SizeConst") },
                    new object[] { columnInfo.Cardinality.Value });

            if (arraySize != null)
                memberBuilder.SetCustomAttribute(arraySize);

            if (columnInfo.Annotations.Contains("id"))
                memberBuilder.SetCustomAttribute(new CustomAttributeBuilder(
                    typeof(IndexAttribute).GetConstructor(Type.EmptyTypes),
                    new object[] { }
                ));

            if (!string.IsNullOrEmpty(columnInfo.Comment))
            {
                memberBuilder.SetCustomAttribute(new CustomAttributeBuilder(
                    typeof(CommentAttribute).GetConstructor(new[] {typeof(string)}),
                    new object[] {columnInfo.Comment}));
            }

            if (!string.IsNullOrEmpty(columnInfo.Definition.Comments))
            {
                memberBuilder.SetCustomAttribute(new CustomAttributeBuilder(
                    typeof(CommentAttribute).GetConstructor(new[] { typeof(string) }),
                    new object[] { columnInfo.Definition.Comments }));
            }

            // info about relationship isnt needed for DBCF.NET
            // same for fks

            if (!columnInfo.Definition.Verified)
            {
                memberBuilder.SetCustomAttribute(new CustomAttributeBuilder(
                    typeof(UnverifiedAttribute).GetConstructor(Type.EmptyTypes),
                    new object[0]));
            }

            memberBuilder.SetCustomAttribute(
                new CustomAttributeBuilder(typeof(OrderAttribute).GetConstructor(new[] { typeof(int) }),
                new object[] { _memberIndex++ }));
        }

        public void Save(string fileName)
        {
            if (File.Exists(fileName))
                File.Delete(fileName);

            _assemblyBuilder.Save(fileName);
        }
    }
}
