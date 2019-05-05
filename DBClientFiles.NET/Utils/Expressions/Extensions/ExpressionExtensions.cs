using System.Linq.Expressions;

namespace DBClientFiles.NET.Utils.Expressions.Extensions
{
    internal static class ExpressionExtensions
    {
        public static string AsString(this Expression expression)
        {
            return new ExpressionStringBuilder(expression).ToString();
        }
    }
}
