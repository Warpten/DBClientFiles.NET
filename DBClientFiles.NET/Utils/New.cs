using System;
using System.Linq;
using Expr = System.Linq.Expressions.Expression;
using System.Runtime.Serialization;
using System.Runtime.CompilerServices;

namespace DBClientFiles.NET.Utils
{
    public static class New<T>
    {
        public static readonly Func<T> Instance = Creator();

        private static Func<T> Creator()
        {
            var t = typeof(T);
            if (t == typeof(string)) // TODO: Can't we just return a function manually ???
                return Expr.Lambda<Func<T>>(Expr.Constant(string.Empty)).Compile();

            if (t.HasDefaultConstructor())
                return Expr.Lambda<Func<T>>(Expr.New(t)).Compile();

            return () => (T)FormatterServices.GetUninitializedObject(t);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Expr Expression() => New.Expression(typeof(T));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Expr Expression(params Expr[] arguments) => New.Expression(typeof(T), arguments);
    }

    public static class New
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Expr Expression(Type instanceType) => Expr.New(instanceType);
        
        public static Expr Expression(Type instanceType, params Expr[] arguments)
        {
            // If a constructor is found with the provided parameters, use it.
            // Otherwise, well, fuck.
            var constructorInfo = instanceType.GetConstructor(arguments.Select(argument => argument.Type).ToArray());
            if (constructorInfo != null)
                return Expr.New(constructorInfo, arguments);

            if (instanceType.IsValueType)
                return Expr.Default(instanceType);

            // Use default parameterless constructor.
            return Expression(instanceType);
        }
    }
}
