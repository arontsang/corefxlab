// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text.Utf8.Resources;

// TODO: Make this struct serializable.

namespace System.Text
{
    /// <summary>
    /// Represents a Unicode scalar value.
    /// </summary>
    /// <remarks>
    /// A Unicode scalar value is an unsigned integer in the range U+0000..U+D7FF, inclusive;
    /// or within the range U+E000..U+10FFFF, inclusive.
    /// </remarks>
    public readonly struct UnicodeScalar : IComparable<UnicodeScalar>, IEquatable<UnicodeScalar>
    {
        private readonly uint _value;

        /// <summary>
        /// Creates a <see cref="UnicodeScalar"/> from the provided UTF-16 code unit.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">
        /// If <paramref name="ch"/> represents a UTF-16 surrogate code point
        /// U+D800..U+DFFF, inclusive.
        /// </exception>
        public UnicodeScalar(char ch)
            : this(ch, false)
        {
            if (UnicodeHelpers.IsSurrogateCodePoint(_value))
            {
                throw new ArgumentOutOfRangeException(
                    message: Strings.Argument_NotValidUnicodeScalar,
                    paramName: nameof(ch));
            }
        }

        /// <summary>
        /// Creates a <see cref="UnicodeScalar"/> from the provided Unicode scalar value.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">
        /// If <paramref name="scalarValue"/> does not represent a value Unicode scalar value.
        /// </exception>
        public UnicodeScalar(int scalarValue)
            : this((uint)scalarValue)
        {
        }

        /// <summary>
        /// Creates a <see cref="UnicodeScalar"/> from the provided Unicode scalar value.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">
        /// If <paramref name="scalarValue"/> does not represent a value Unicode scalar value.
        /// </exception>
        public UnicodeScalar(uint scalarValue)
            : this(scalarValue, false)
        {
            if (!UnicodeHelpers.IsValidUnicodeScalar(_value))
            {
                throw new ArgumentOutOfRangeException(
                    message: Strings.Argument_NotValidUnicodeScalar,
                    paramName: nameof(scalarValue));
            }
        }

        // non-validating ctor
        private UnicodeScalar(uint scalarValue, bool ignored)
        {
            _value = scalarValue;
        }

        /// <summary>
        /// Compares two <see cref="UnicodeScalar"/> instances for equality.
        /// </summary>
        public static bool operator ==(UnicodeScalar a, UnicodeScalar b) => (a._value == b._value);

        /// <summary>
        /// Compares two <see cref="UnicodeScalar"/> instances for inequality.
        /// </summary>
        public static bool operator !=(UnicodeScalar a, UnicodeScalar b) => (a._value != b._value);

        /// <summary>
        /// Returns true iff this scalar value is ASCII ([ U+0000..U+007F ])
        /// and therefore representable by a single UTF-8 code unit.
        /// </summary>
        public bool IsAscii => (_value < 0x80U);

        /// <summary>
        /// Returns true iff this scalar value is within the BMP ([ U+0000..U+FFFF ])
        /// and therefore representable by a single UTF-16 code unit.
        /// </summary>
        public bool IsBmp => (_value < 10000U);

        /// <summary>
        /// A <see cref="UnicodeScalar"/> instance that represents the Unicode replacement character U+FFFD.
        /// </summary>
        public static UnicodeScalar ReplacementChar => DangerousCreateWithoutValidation(0xFFFD);

        /// <summary>
        /// Returns the length in code units (<see cref="Char"/>) of the
        /// UTF-16 sequence required to represent this scalar value.
        /// </summary>
        /// <remarks>
        /// The return value will be 1 or 2.
        /// </remarks>
        public int Utf16SequenceLength
        {
            get
            {
                // If _value <  0x10000, returns (-1) + 2 = 1
                // If _value >= 0x10000, returns   0  + 2 = 2
                return (((int)_value - 0x10000) >> 31) + 2;
            }
        }

        /// <summary>
        /// Returns the length in code units (<see cref="Utf8Char"/>) of the
        /// UTF-8 sequence required to represent this scalar value.
        /// </summary>
        /// <remarks>
        /// The return value will be 1 through 4, inclusive.
        /// </remarks>
        public int Utf8SequenceLength => throw null;

        /// <summary>
        /// Returns the Unicode scalar value as an unsigned integer.
        /// </summary>
        public uint Value => _value;

        /// <summary>
        /// Compares this <see cref="UnicodeScalar"/> instance to another <see cref="UnicodeScalar"/> instance.
        /// </summary>
        public int CompareTo(UnicodeScalar other) => this._value.CompareTo(other._value);

        /// <summary>
        /// Creates a <see cref="UnicodeScalar"/> from the provided Unicode scalar value
        /// without validating the input value for well-formedness.
        /// </summary>
        /// <remarks>
        /// The caller is expected to have validated that <paramref name="scalarValue"/> is
        /// a valid value. The behavior of this type is undefined if the input value is invalid.
        /// </remarks>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public static UnicodeScalar DangerousCreateWithoutValidation(uint scalarValue) => new UnicodeScalar(scalarValue, false);

        /// <summary>
        /// Returns true iff this <see cref="UnicodeScalar"/> instance is equal to the provided object.
        /// </summary>
        public override bool Equals(object obj)
        {
            return (obj is UnicodeScalar) && Equals((UnicodeScalar)obj);
        }

        /// <summary>
        /// Returns true iff this <see cref="UnicodeScalar"/> instance is equal to the provided <see cref="UnicodeScalar"/> instance.
        /// </summary>
        public bool Equals(UnicodeScalar other) => this._value.Equals(other._value);

        /// <summary>
        /// Returns a hash code for this <see cref="UnicodeScalar"/> instance suitable for use in a dictionary.
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode() => _value.GetHashCode();

        /// <summary>
        /// Returns a <see cref="String"/> representation of this <see cref="UnicodeScalar"/> instance.
        /// </summary>
        public override string ToString()
        {
            Span<char> chars = stackalloc char[2]; // worst case
            return new String(chars.Slice(0, ToUtf16(chars)));
        }

        /// <summary>
        /// Writes this scalar value as a UTF-16 sequence to the output buffer, returning
        /// the number of code units (<see cref="Char"/>) written.
        /// </summary>
        /// <exception cref="ArgumentException">
        /// Thrown if <paramref name="output"/> is too short to contain the output.
        /// The required length can be queried ahead of time via the <see cref="Utf16SequenceLength"/> property.
        /// </exception>
        public int ToUtf16(Span<char> output) => throw null;

        /// <summary>
        /// Writes this scalar value as a UTF-8 sequence to the output buffer, returning
        /// the number of code units (<see cref="Utf8Char"/>) written.
        /// </summary>
        /// <exception cref="ArgumentException">
        /// Thrown if <paramref name="output"/> is too short to contain the output.
        /// The required length can be queried ahead of time via the <see cref="Utf8SequenceLength"/> property.
        /// </exception>
        public int ToUtf8(Span<Utf8Char> output) => throw null;

        /// <summary>
        /// Returns a <see cref="Utf8String"/> representation of this <see cref="UnicodeScalar"/> instance.
        /// </summary>
        public Utf8String ToUtf8String()
        {
            Span<Utf8Char> utf8Chars = stackalloc Utf8Char[4]; // worst case
            return new Utf8String(utf8Chars.Slice(0, ToUtf8(utf8Chars)));
        }
    }
}
