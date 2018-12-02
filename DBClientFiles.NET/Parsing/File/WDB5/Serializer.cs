using DBClientFiles.NET.Parsing.Binding;
using DBClientFiles.NET.Parsing.File.Records;
using DBClientFiles.NET.Parsing.File.Segments;
using DBClientFiles.NET.Parsing.File.Segments.Handlers;
using DBClientFiles.NET.Parsing.Reflection;
using DBClientFiles.NET.Parsing.Serialization;
using DBClientFiles.NET.Utils;
using System.Linq.Expressions;

namespace DBClientFiles.NET.Parsing.File.WDB5
{
    internal sealed class Serializer<T> : StructuredSerializer<T>
    {
        private TypeMapper _mapper;

        public Serializer() : base()
        {

        }

        public override void Initialize(IBinaryStorageFile parser)
        {
            _mapper = new TypeMapper(parser.Type);
            var recordBlock = parser.FindBlockHandler<FieldInfoHandler<MemberMetadata>>(BlockIdentifier.FieldInfo);

            _mapper.Resolve(parser.Options.MemberType, recordBlock);
        }

        public int GetElementBitCount(Member memberInfo)
        {
            return (int)(_mapper.Map[memberInfo].Size);
        }

        /// <summary>
        /// WDB5 deserilization is trivial. The only packing is done over 24 bits.
        /// </summary>
        /// <param name="memberAccess"></param>
        /// <param name="memberInfo"></param>
        /// <param name="recordReader"></param>
        /// <returns></returns>
        /// <remarks>
        /// <para>
        /// For array types:
        /// <list type="bullet">
        ///     <item>For arrays of primitives or strings</item>
        ///     <description>
        ///     We chain the call to <see cref="IRecordReader.ReadArray{T}(int, int)"/> or <see cref="IRecordReader.ReadStringArray(int, int)"/>
        ///     if the type is either primitive, or a string.
        ///     </description>
        ///
        ///     <item>For arrays of value or reference types.</item>
        ///     <description>
        ///     We just create the array itself, and move on (<c>new T[...]</c>).
        ///     </description>
        /// </list>
        /// </para>
        /// <para>
        /// For regular types, this is all of the above, except value or references are a no-op, and methods become
        /// <see cref="IRecordReader.Read{T}(int)"/> and <see cref="IRecordReader.ReadString(int)"/>, respectively.
        /// </para>
        /// </remarks>
        public override Expression VisitNode(Expression memberAccess, Member memberInfo, Expression recordReader)
        {
            var bitCount = GetElementBitCount(memberInfo);

            if (memberInfo.Type.Type.IsArray)
            {
                var elementType = memberInfo.Type.Type.GetElementType();
                if (elementType.IsPrimitive)
                {
                    bool isPacked = bitCount == UnsafeCache.BitSizeOf(memberInfo.Type.Type.GetElementType());

                    if (isPacked)
                    {
                        // ReadArray<T>(cardinalityCount, bitCount);
                        return Expression.Call(recordReader,
                            _IRecordReader.ReadArrayPacked.MakeGenericMethod(elementType),
                            Expression.Constant(memberInfo.Cardinality),
                            Expression.Constant(bitCount));
                    }

                    // ReadArray<T>(cardinalityCount);
                    return Expression.Call(recordReader,
                        _IRecordReader.ReadArrayPacked.MakeGenericMethod(elementType),
                        Expression.Constant(memberInfo.Cardinality));
                }
                else if (elementType == typeof(string))
                {
                    if (bitCount == 32)
                        // ReadStringArray(cardinalityCount)
                        return Expression.Call(recordReader,
                            _IRecordReader.ReadStringArrayPacked,
                            Expression.Constant(memberInfo.Cardinality));

                    // ReadStringArray(cardinalityCount, bitCount)
                    return Expression.Call(recordReader,
                        _IRecordReader.ReadStringArrayPacked,
                        Expression.Constant(memberInfo.Cardinality),
                        Expression.Constant(bitCount));
                }

                return null;
            }

            if (memberInfo.Type.Type.IsPrimitive)
            {
                bool isPacked = bitCount == UnsafeCache.BitSizeOf(memberInfo.Type.Type.GetElementType());

                if (!isPacked)
                    // Read<T>();
                    return Expression.Call(recordReader,
                        _IRecordReader.ReadPacked.MakeGenericMethod(memberInfo.Type.Type));

                // Read<T>(bitCount);
                return Expression.Call(recordReader,
                    _IRecordReader.ReadPacked.MakeGenericMethod(memberInfo.Type.Type),
                    Expression.Constant(bitCount));
            }
            else if (memberInfo.Type.Type == typeof(string))
            {
                if (bitCount == 32)
                    // ReadString()
                    return Expression.Call(recordReader,
                        _IRecordReader.ReadStringPacked,
                        Expression.Constant(memberInfo.Cardinality));

                // ReadString(bitCount);
                return Expression.Call(recordReader,
                    _IRecordReader.ReadStringPacked,
                    Expression.Constant(bitCount));
            }

            return null;
        }
    }
}
