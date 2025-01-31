using System.Linq;
using System;
using System.Collections.Generic;
namespace HanDebugger.Extensions
{
    public static class StringExtensions
    {
        public static IEnumerable<string> SplitInParts(this string s, Int32 partLength)
        {
            if (s == null)
                throw new ArgumentNullException(nameof(s));
            if (partLength <= 0)
                throw new ArgumentException("Part length has to be positive.", nameof(partLength));

            for (var i = 0; i < s.Length; i += partLength)
            {
                // yield return s.AsMemory().Slice(i, Math.Min(partLength, s.Length - i));
                yield return s.Substring(i, Math.Min(partLength, s.Length - i));
            }
        }
    }
}