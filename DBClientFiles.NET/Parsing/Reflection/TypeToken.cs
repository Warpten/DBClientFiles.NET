using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using DBClientFiles.NET.Utils.Extensions;

namespace DBClientFiles.NET.Parsing.Reflection
{
    /// <summary>
    /// A representation of a type.
    /// </summary>
    internal class TypeToken : IEquatable<Type>
    {
        /// <summary>
        /// The underlying CLR <see cref="Type"/> representation of this type.
        /// </summary>
        internal Type Type { get; }

        private Dictionary<Type, TypeToken> _declaredTypes;
        private List<MemberToken> _members;

        public IEnumerable<TypeToken> DeclaredTypes => _declaredTypes.Values;
        public IList<MemberToken> Members => _members;

        public IEnumerable<MemberToken> Fields
            => Members.Where(m => m.MemberType == TypeTokenType.Field);

        public IEnumerable<MemberToken> Properties
            => Members.Where(m => m.MemberType == TypeTokenType.Property);

        public TypeToken(Type type)
        {
            _declaredTypes = new Dictionary<Type, TypeToken>();
            
            _members = new List<MemberToken>();

            Type = type;

            // We pretend a string or a primitive type have no properties or fields.
            // In truth they do but for our purposes they should be treated as "pure" types.
            if (type.IsPrimitive || type == typeof(string))
                return;

            if (!type.IsArray)
            {
                var fields = type.GetFields(BindingFlags.Public | BindingFlags.Instance);
                for (int i = 0; i < fields.Length; ++i)
                {
                    ref readonly var fieldInfo = ref fields[i];
                    GetChildToken(fieldInfo.FieldType);

                    var fieldType = new FieldToken(this, fieldInfo, i);
                    _members.Add(fieldType);
                }

                var properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);
                for (int i = 0; i < properties.Length; i++)
                {
                    ref readonly var propInfo = ref properties[i];
                    GetChildToken(propInfo.PropertyType);

                    _members.Add(new PropertyToken(this, propInfo, i));
                }
            }
        }

        public MemberToken FindChild(MemberInfo reflectionInfo)
        {
            foreach (var child in Members)
                if (child.MemberInfo == reflectionInfo)
                    return child;

            return null;
        }

        public T GetAttribute<T>() where T : Attribute
        {
            return Type.GetCustomAttribute<T>();
        }

        public TypeToken GetChildToken(Type type)
        {
            if (_declaredTypes.TryGetValue(type, out var typeInfo))
                return typeInfo;

            _declaredTypes[type] = new TypeToken(type);
            return _declaredTypes[type];
        }

        public TypeToken GetElementTypeToken()
        {
            if (!Type.IsArray)
                throw new InvalidOperationException("not an array");

            return GetChildToken(Type.GetElementType());
        }

        public bool HasChild(IMemberToken child)
        {
            return _members.Any(m => m == child);
        }

        public override string ToString()
        {
            return Type.ToString();
        }

        public (MemberToken memberToken, Expression memberAccess) MakeMemberAccess(ref int index, Expression accessExpression, TypeTokenType type)
        {
            foreach (var memberInfo in _members)
            {
                if (memberInfo.MemberType != type)
                    continue;

                Expression memberAccessExpr = memberInfo.MakeAccess(accessExpression);

                if (memberInfo.TypeToken.IsArray)
                {
                    // If the member is an array, we need t build an intermediate array access expression somehow
                    var elementTypeToken = memberInfo.TypeToken.GetElementTypeToken();

                    for (var i = 0; i < memberInfo.Cardinality; ++i)
                    {
                        Expression arrayAccessExpr = Expression.ArrayAccess(memberAccessExpr, Expression.Constant(i));
                        
                        var token = elementTypeToken.MakeMemberAccess(ref index, arrayAccessExpr, type);

                        if (token.memberAccess != null)
                            return token;
                    }

                    continue;
                }
                else if (memberInfo.TypeToken.Members.Any(m => m.MemberType == type))
                {
                    var childToken = memberInfo.TypeToken.MakeMemberAccess(ref index, memberAccessExpr, type);
                    if (childToken.memberAccess != null)
                        return childToken;
                
                    continue;
                }

                if (index == 0)
                {
                    accessExpression = memberInfo.MakeAccess(accessExpression);
                    return (memberInfo, accessExpression);
                }

                index -= 1;
            }

            return default;
        }

        public bool IsArray => Type.IsArray;
        public bool IsClass => Type.IsClass;

        public bool IsPrimitive => Type.IsPrimitive;

        public string Name => Type.Name;

        public bool HasDefaultConstructor => Type.HasDefaultConstructor();

        #region IEquatable<Type>
        public bool Equals(Type other) => Type == other;
        #endregion

        public static bool operator ==(TypeToken typeToken, Type type) => typeToken.Type == type;
        public static bool operator !=(TypeToken typeToken, Type type) => typeToken.Type != type;

        // Sigh.
        public static bool operator ==(Type type, TypeToken typeToken) => typeToken.Type == type;
        public static bool operator !=(Type type, TypeToken typeToken) => typeToken.Type != type;

        public override bool Equals(object obj)
        {
            if (obj is TypeToken typeToken)
                return typeToken.Type == Type;

            if (obj is Type type)
                return Type == type;

            return false;
        }

        public override int GetHashCode()
        {
            return Type.GetHashCode();
        }

        #region Expression helpers
        /// <summary>
        /// Returns an expression representing instanciation of an array where the element type is the current object, of given bounds.
        /// </summary>
        /// <param name="bounds"></param>
        /// <returns></returns>
        public Expression NewArrayBounds(params Expression[] bounds) => Expression.NewArrayBounds(Type, bounds);

        // Helper for above
        public Expression NewArrayBounds(int bound) => Expression.NewArrayBounds(Type, Expression.Constant(bound));

        public Expression NewExpression() => Expression.New(Type);

        #endregion

        #region MethodInfo helpers
        public MethodInfo MakeGenericMethod(MethodInfo methodInfo) => methodInfo.MakeGenericMethod(Type);
        #endregion
    }
}
