using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Threading;

namespace DBClientFiles.NET.Mapper.Generator
{
    internal sealed class TypeGenerator
    {
        private AssemblyBuilder _assemblyBuilder;
        private ModuleBuilder _module;

        private string _typeName;

        private List<MemberGenerator> _members = new List<MemberGenerator>();
        private List<CustomAttributeBuilder> _attrs = new List<CustomAttributeBuilder>();
        private int _iteration = 0;

        public Type Type { get; private set; }

        public TypeGenerator(string name = null)
        {
            var assemblyName = new AssemblyName { Name = "TemporaryAssembly" };
            _assemblyBuilder = Thread.GetDomain().DefineDynamicAssembly(assemblyName, AssemblyBuilderAccess.Run);
            _module = _assemblyBuilder.DefineDynamicModule("TemporaryModule");

            _typeName = name ?? $"GeneratedType_{Path.GetRandomFileName().GetHashCode()}";
        }

        public void AddAttribute(ConstructorInfo ctor, object[] @params)
        {
            _attrs.Add(new CustomAttributeBuilder(ctor, @params));
        }

        public void AddAttribute(ConstructorInfo ctor, object[] @params, PropertyInfo[] props, object[] propValues)
        {
            _attrs.Add(new CustomAttributeBuilder(ctor, @params, props, propValues));
        }

        public Type Generate()
        {
            var typeBuilder = _module.DefineType(_typeName + "_" + (_iteration++));

            foreach (var memberInfo in _members.OrderBy(s => s.Index))
                memberInfo.Generate(typeBuilder);

            foreach (var attr in _attrs)
                typeBuilder.SetCustomAttribute(attr);

            return Type = typeBuilder.CreateType();
        }

        public MemberGenerator GetMember(int memberIndex)
        {
            return _members.First(s => s.Index == memberIndex);
        }

        public T GetMember<T>(int memberIndex) where T : MemberGenerator
        {
            return GetMember(memberIndex) as T;
        }

        public FieldGenerator CreateField(string name, Type type, int cardinality, bool isIndex)
        {
            var instance = new FieldGenerator(this, name, type, _members.Count);
            instance.Cardinality = cardinality;
            instance.IsIndex = isIndex;

            _members.Add(instance);

            return instance;
        }

        public PropertyGenerator CreateProperty(string name, Type type, int cardinality, bool isIndex)
        {
            var instance = new PropertyGenerator(this, name, type, _members.Count);
            instance.Cardinality = cardinality;
            instance.IsIndex = isIndex;

            _members.Add(instance);

            return instance;
        }
    }
}
