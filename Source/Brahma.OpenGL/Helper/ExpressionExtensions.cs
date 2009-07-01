using System.Linq.Expressions;

namespace Brahma.OpenGL.Helper
{
    public static class ExpressionExtensions
    {
        public static bool IsOutputCoordAccess(this MemberExpression expression)
        {
            return ((expression.Member.Name == "Current") ||
                    (expression.Member.Name == "CurrentX") ||
                    (expression.Member.Name == "CurrentY")) &&
                   (expression.Member.DeclaringType == typeof(output));
        }
    }
}
