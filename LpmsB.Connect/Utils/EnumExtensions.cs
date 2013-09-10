using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LpmsB.Utils
{
    internal static class EnumExtensions
    {
        public static bool IsSet<T>(this T value, T flag)
            where T : struct
        {
            var type = typeof(T);

            if (!type.IsEnum)
                throw new InvalidOperationException();

            var v = ((IConvertible)value).ToUInt64(null);
            var f = ((IConvertible)flag).ToUInt64(null);

            return (v & f) == f;
        }
    }
}
