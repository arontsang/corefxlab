// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace System.Text
{
    internal static class UnicodeHelpers
    {
        /// <summary>
        /// Returns <see langword="true"/> iff <paramref name="value"/> is between
        /// <paramref name="lowerBound"/> and <paramref name="upperBound"/>, inclusive.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsInRangeInclusive(uint value, uint lowerBound, uint upperBound) => ((value - lowerBound) <= (upperBound - lowerBound));

        /// <summary>
        /// Returns <see langword="true"/> iff <paramref name="value"/> is a UTF-16 surrogate code point,
        /// i.e., is in [ U+D800..U+DFFF ], inclusive.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsSurrogateCodePoint(uint value) => IsInRangeInclusive(value, 0xD800U, 0xDFFFU);

        /// <summary>
        /// Returns <see langword="true"/> iff <paramref name="value"/> is a valid Unicode scalar
        /// value, i.e., is in [ U+0000..U+D7FF ], inclusive; or [ U+E000..U+10FFFF ], inclusive.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsValidUnicodeScalar(uint value) => IsInRangeInclusive(value & 0xD800U, 0x800U, 0x10FFFFU);
    }
}
