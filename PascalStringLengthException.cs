/////////////////////////////////////////////////////////////////////////////////
//
// Change 8bf category
//
// This software is provided under the MIT License:
//   Copyright (C) 2016-2019 Nicholas Hayes
//
// See LICENSE.txt for complete licensing and attribution information.
//
/////////////////////////////////////////////////////////////////////////////////

using System;
using System.Runtime.Serialization;

namespace ChangeFilterCategory
{
    [Serializable]
    internal sealed class PascalStringLengthException : ArgumentException
    {
        public PascalStringLengthException(string message) : base(message)
        {
        }

        private PascalStringLengthException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}
