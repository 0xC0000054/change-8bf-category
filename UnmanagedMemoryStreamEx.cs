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
using System.IO;

namespace ChangeFilterCategory
{
    internal sealed class UnmanagedMemoryStreamEx : UnmanagedMemoryStream
    {
        private byte[] buffer;

        /// <summary>
        /// Initializes a new instance of the <see cref="UnmanagedMemoryStreamEx"/> class.
        /// </summary>
        /// <param name="address">The base address of the unmanaged memory buffer.</param>
        /// <param name="length">The length of the unmanaged memory buffer.</param>
        /// <param name="access">The access of the unmanaged memory buffer.</param>
        public unsafe UnmanagedMemoryStreamEx(IntPtr address, long length, FileAccess access) :
            base((byte*)address.ToPointer(), length, length, access)
        {
            this.buffer = new byte[4];
        }

        /// <summary>
        /// Writes a signed 16 bit integer to the stream in little endian byte order.
        /// </summary>
        /// <param name="value">The value to write.</param>
        public void WriteInt16(short value)
        {
            this.buffer[0] = (byte)value;
            this.buffer[1] = (byte)(value >> 8);
            Write(this.buffer, 0, 2);
        }

        /// <summary>
        /// Writes a signed 32 bit integer to the stream in little endian byte order.
        /// </summary>
        /// <param name="value">The value to write.</param>
        public void WriteInt32(int value)
        {
            this.buffer[0] = (byte)value;
            this.buffer[1] = (byte)(value >> 8);
            this.buffer[2] = (byte)(value >> 16);
            this.buffer[3] = (byte)(value >> 24);
            Write(this.buffer, 0, 4);
        }

        /// <summary>
        /// Writes an unsigned 32 bit integer to the stream in little endian byte order.
        /// </summary>
        /// <param name="value">The value to write.</param>
        public void WriteUInt32(uint value)
        {
            this.buffer[0] = (byte)value;
            this.buffer[1] = (byte)(value >> 8);
            this.buffer[2] = (byte)(value >> 16);
            this.buffer[3] = (byte)(value >> 24);
            Write(this.buffer, 0, 4);
        }
    }
}
