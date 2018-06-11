using System;
using System.Reflection;
using System.Reflection.Emit;
using DBClientFiles.NET.Attributes;
using DBClientFiles.NET.Definitions.Attributes;

namespace DBClientFiles.NET.AutoMapper
{
    internal sealed class FieldGenerator : MemberGenerator
    {
        public FieldGenerator(TypeGenerator parent, string fieldName, Type fieldType, int index) : base(parent, fieldName, fieldType, index)
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

            fieldBuilder.SetCustomAttribute(
                new CustomAttributeBuilder(typeof(OrderAttribute).GetConstructor(new[] { typeof(int) }),
                new object[] { Index }));
        }
    }
}