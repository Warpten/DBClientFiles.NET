using System;
using System.Reflection;
using System.Reflection.Emit;
using DBClientFiles.NET.Attributes;

namespace DBClientFiles.NET.AutoMapper
{
    internal sealed class FieldGenerator : MemberGenerator
    {
        public FieldGenerator(TypeGenerator parent, string fieldName, Type fieldType) : base(parent, fieldName, fieldType)
        {
        }

        public override void Generate(TypeBuilder builder)
        {
            var fieldBuilder = builder.DefineField(Name, Type, FieldAttributes.Public);

            if (Type.IsArray)
            {
                fieldBuilder.SetCustomAttribute(new CustomAttributeBuilder(
                    typeof(CardinalityAttribute).GetConstructor(Type.EmptyTypes),
                    new object[0],
                    new[] { typeof(CardinalityAttribute).GetProperty("SizeConst") },
                    new object[] { Cardinality }));
            }

            if (IsIndex)
            {
                fieldBuilder.SetCustomAttribute(
                    new CustomAttributeBuilder(typeof(IndexAttribute).GetConstructor(Type.EmptyTypes),
                        new object[0]));
            }
        }
    }
}