using DBClientFiles.NET.Collections;
using DBClientFiles.NET.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace DBClientFiles.NET.Internals.Serializers
{
    internal static class SerializationUtils
    {
        public static bool GenerateTypeReader<T>(StorageOptions options, Expression destinationExpr, params ParameterExpression[] parameterExprs) 
            => GenerateTypeReader(typeof(T), options, destinationExpr, parameterExprs);

        public static bool GenerateTypeReader(Type type, StorageOptions options, Expression destinationExpr, params ParameterExpression[] parameterExprs)
        {
            var instanciationExpr = GetInstanciationExpression(type, parameterExprs);
            var isParameterlessConstruction = instanciationExpr.Arguments.Count == 0;

            var bodyList = new List<Expression>() {
                Expression.Assign(destinationExpr, instanciationExpr)
            };

            var memberIndex = 0;
            foreach (var memberInfo in type.GetMembers(BindingFlags.Public | BindingFlags.Instance))
            {
                // Don't try to deserialize members that are not what the user asks for.
                if (memberInfo.MemberType != options.MemberType)
                    continue;

                // Don't try to deserialize properties without a setter.
                if (memberInfo is PropertyInfo propInfo)
                    if (propInfo.GetSetMethod() == null)
                        continue;

                var extendedMemberInfo = ExtendedMemberInfo.Initialize(memberInfo, memberIndex++);
                var memberType = extendedMemberInfo.Type;

                // Arrays with more than 1 rank are not supported (well, they could be, but whatever)
                if (memberType.GetArrayRank() > 1)
                    throw new InvalidOperationException();

                if (memberType.IsArray)
                    memberType = memberType.GetElementType();

                // Multidimentional arrays are not supported - and check array rank again
                if (memberType.IsArray || memberType.GetArrayRank() > 1)
                    throw new InvalidOperationException();

                //
            }

            return false;
        }

        public static NewExpression GetInstanciationExpression(Type typeInfo, params ParameterExpression[] parameterExprs)
        {
            var constructorInfo = typeInfo.GetConstructor(parameterExprs.Select(p => p.Type).ToArray());
            if (constructorInfo != null)
                return Expression.New(constructorInfo, parameterExprs);

            return Expression.New(typeInfo);
        }
    }
}
