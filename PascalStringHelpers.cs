/////////////////////////////////////////////////////////////////////////////////
//
// Change 8bf category
//
// This software is provided under the MIT License:
//   Copyright (C) 2016-2018 Nicholas Hayes
//
// See LICENSE.txt for complete licensing and attribution information.
//
/////////////////////////////////////////////////////////////////////////////////

using System;
using System.Text;

namespace ChangeFilterCategory
{
    internal static class PascalStringHelpers
    {
        private const int MaxPascalStringLength = 255;

        /// <summary>
        /// Creates a <see cref="string"/> from the Pascal string in the specified byte array.
        /// </summary>
        /// <param name="pascalString">The byte array containing the Pascal string.</param>
        /// <returns>
        /// A new string form the <paramref name="pascalString"/> byte array.
        /// </returns>
        /// <exception cref="ArgumentNullException"><paramref name="pascalString"/> is null.</exception>
        internal static string ConvertToString(byte[] pascalString)
        {
            if (pascalString == null)
            {
                throw new ArgumentNullException(nameof(pascalString));
            }

            if (pascalString.Length == 0)
            {
                return string.Empty;
            }

            return Encoding.ASCII.GetString(pascalString, 1, pascalString[0]);
        }

        /// <summary>
        /// Creates a Pascal string from the specified <see cref="string"/>.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns>
        /// A byte array containing the resulting Pascal string.
        /// </returns>
        /// <exception cref="ArgumentNullException"><paramref name="value"/> is null.</exception>
        /// <exception cref="PascalStringLengthException"><paramref name="value"/> exceeds the maximum length for a Pascal string.</exception>
        internal static byte[] CreateFromString(string value)
        {
            if (value == null)
            {
                throw new ArgumentNullException(nameof(value));
            }

            int byteCount = Encoding.ASCII.GetByteCount(value);

            if (byteCount < 0 || byteCount > MaxPascalStringLength)
            {
                throw new PascalStringLengthException("The string must be <= 255 ASCII characters to encode as a Pascal string.");
            }

            byte[] pascalString = new byte[1 + byteCount];
            pascalString[0] = (byte)byteCount;

            Encoding.ASCII.GetBytes(value, 0, value.Length, pascalString, 1);

            return pascalString;
        }

        /// <summary>
        /// Determines whether the specified value has a valid length to be encoded as a Pascal string.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns>
        /// <c>true</c> if the specified value can be encoded as a Pascal string; otherwise, <c>false</c>.
        /// </returns>
        /// <exception cref="ArgumentNullException"><paramref name="value"/> is null.</exception>
        internal static bool IsLengthValid(string value)
        {
            if (value == null)
            {
                throw new ArgumentNullException(nameof(value));
            }

            int byteCount = Encoding.ASCII.GetByteCount(value);

            return (byteCount >= 0 && byteCount <= MaxPascalStringLength);
        }
    }
}
