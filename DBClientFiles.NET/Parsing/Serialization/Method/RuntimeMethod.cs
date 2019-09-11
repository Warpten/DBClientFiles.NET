using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace DBClientFiles.NET.Parsing.Serialization.Method
{
    internal abstract class RuntimeMethod<T> where T : Delegate
    {
        protected RuntimeMethod()
        {
            _methodGenerator = new Lazy<T>(() => Expression.Lambda<T>(CreateBody(), EnumerateParameters()).Compile());
        }

        private Lazy<T> _methodGenerator;

        public T Method => _methodGenerator.Value;

        protected abstract IEnumerable<ParameterExpression> EnumerateParameters();

        protected abstract Expression CreateBody();
    }
}
