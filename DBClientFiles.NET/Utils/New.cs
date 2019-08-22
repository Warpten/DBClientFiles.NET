using System;
using System.Linq;
using Expr = System.Linq.Expressions.Expression;
using System.Runtime.Serialization;
using System.Runtime.CompilerServices;
using DBClientFiles.NET.Utils.Extensions;
using System.Reflection;

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

    // Genericity hell, here we go
    public static class New<T, P1>
    {
        public static readonly Func<P1, T> Instance = Creator();

        private static Func<P1, T> Creator()
        {
            var t = typeof(T);
            foreach (var ctor in t.GetConstructors(BindingFlags.Public | BindingFlags.Instance))
            {
                var parameters = ctor.GetParameters();
                if (parameters.Length == 1)
                {
                    if (parameters[0].ParameterType.IsAssignableFrom(typeof(P1)))
                    {
                        var parameter = Expr.Parameter(typeof(P1));
                        return Expr.Lambda<Func<P1, T>>(Expr.New(ctor, parameter), parameter).Compile();
                    }
                }
            }

            throw new InvalidOperationException("no ctor matching types");
        }
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
