using System;
using System.Reflection;
using System.Reflection.Emit;
using DBClientFiles.NET.Attributes;
using DBClientFiles.NET.Definitions.Attributes;

namespace DBClientFiles.NET.Mapper.Generator
{
    internal sealed class PropertyGenerator : MemberGenerator
    {
        public PropertyGenerator(TypeGenerator parent, string fieldName, Type fieldType, int index) : base(parent, fieldName, fieldType, index)
        {
        }

        public override void Generate(TypeBuilder builder)
        {
            var fieldBuilder = builder.DefineField(Name + "_backingField", Type, FieldAttributes.Private | FieldAttributes.SpecialName);
            var propBuilder = builder.DefineProperty(Name, PropertyAttributes.None, Type, Type.EmptyTypes);

            var getSetAttr = MethodAttributes.Public | MethodAttributes.SpecialName | MethodAttributes.HideBySig;

            var getBuilder = builder.DefineMethod($"get_{Name}", getSetAttr, Type, Type.EmptyTypes);
            var getGenerator = getBuilder.GetILGenerator();

            getGenerator.Emit(OpCodes.Ldarg_0);
            getGenerator.Emit(OpCodes.Ldfld, fieldBuilder);
            getGenerator.Emit(OpCodes.Ret);

            var setBuilder = builder.DefineMethod($"set_{Name}", getSetAttr, null, new[] { Type });
            var setGenerator = setBuilder.GetILGenerator();

            setGenerator.Emit(OpCodes.Ldarg_0);
            setGenerator.Emit(OpCodes.Ldarg_1);
            setGenerator.Emit(OpCodes.Stfld, fieldBuilder);
            setGenerator.Emit(OpCodes.Ret);

            propBuilder.SetSetMethod(setBuilder);
            propBuilder.SetGetMethod(getBuilder);

            if (Type.IsArray)
            {
                propBuilder.SetCustomAttribute(new CustomAttributeBuilder(
                    typeof(CardinalityAttribute).GetConstructor(Type.EmptyTypes),
                    new object[0],
                    new[] { typeof(CardinalityAttribute).GetProperty("SizeConst") },
                    new object[] { Cardinality }));
            }

            if (IsIndex)
            {
                propBuilder.SetCustomAttribute(
                    new CustomAttributeBuilder(typeof(IndexAttribute).GetConstructor(Type.EmptyTypes),
                    new object[0]));
            }

            propBuilder.SetCustomAttribute(
                new CustomAttributeBuilder(typeof(OrderAttribute).GetConstructor(new[] { typeof(int) }),
                new object[] { Index }));
        }
    }
}