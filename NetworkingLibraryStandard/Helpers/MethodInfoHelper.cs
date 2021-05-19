using System;
using System.Linq.Expressions;
using System.Reflection;

namespace Jaika1.Networking.Helpers
{
    public static class MethodInfoHelper
    {
        public static MethodInfo GetMethodInfo<T>(Expression<Action<T>> expression)
        {
            var member = expression.Body as MethodCallExpression;

            if (member != null)
                return member.Method;

            NetBase.WriteDebug("Expression provided to MethodInfoHelper.GetMethodInfo<T> is not a method!", true);
            return null; // Will never be hit, but is done to make C# happy that it's getting a return type.
        }
    }
}
