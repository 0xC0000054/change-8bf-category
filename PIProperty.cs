/////////////////////////////////////////////////////////////////////////////////
//
// Change 8bf category
//
// This software is provided under the MIT License:
//   Copyright (C) 2016-2017 Nicholas Hayes
// 
// See LICENSE.txt for complete licensing and attribution information.
//
/////////////////////////////////////////////////////////////////////////////////

using System;
using System.Runtime.InteropServices;

namespace ChangeFilterCategory
{
    internal sealed class PIProperty
    {
        private const int PropertyHeaderLength = 16;

        private readonly uint vendorID;
        private readonly uint propertyKey;
        private readonly int propertyID;
        private readonly int propertyDataLength;
        private readonly byte[] propertyData;
        private readonly int propertyDataPaddingLength;
        private readonly int totalLength;

        /// <summary>
        /// Initializes a new instance of the <see cref="PIProperty"/> class from the specified pointer.
        /// </summary>
        /// <param name="ptr">The pointer to the start of the property header.</param>
        public unsafe PIProperty(byte* ptr)
        {
            this.vendorID = *(uint*)ptr;
            this.propertyKey = *(uint*)(ptr + 4);
            this.propertyID = *(int*)(ptr + 8);
            this.propertyDataLength = *(int*)(ptr + 12);
            if (this.propertyDataLength > 0)
            {
                this.propertyData = new byte[this.propertyDataLength];
                Marshal.Copy((IntPtr)(ptr + 16), this.propertyData, 0, this.propertyDataLength);
            }
            else
            {
                this.propertyData = null;
            }
            this.propertyDataPaddingLength = (4 - this.propertyDataLength) & 3;
            this.totalLength = PropertyHeaderLength + this.propertyDataLength + this.propertyDataPaddingLength;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PIProperty"/> class, with the specified key and value.
        /// </summary>
        /// <param name="key">The property key.</param>
        /// <param name="value">The byte array containing the property data.</param>
        public PIProperty(uint key, byte[] value)
        {
            this.vendorID = PIPLConstants.VendorIDPhotoshop;
            this.propertyKey = key;
            this.propertyID = 0;
            if (value != null)
            {
                // The Photoshop SDK states that the property data length does not include the padding bytes
                // required to achieve four byte alignment, but some hosts depend on it being included.
                this.propertyDataLength = (value.Length + 3) & ~3;
                this.propertyData = (byte[])value.Clone();
                this.propertyDataPaddingLength = this.propertyDataLength - value.Length;
            }
            else
            {
                this.propertyDataLength = 0;
                this.propertyData = null;
                this.propertyDataPaddingLength = 0;
            }
            this.totalLength = PropertyHeaderLength + this.propertyDataLength;
        }

        /// <summary>
        /// Gets the key identifying the type of this property.
        /// </summary>
        /// <value>
        /// The key identifying the property type.
        /// </value>
        public uint Key
        {
            get
            {
                return this.propertyKey;
            }
        }

        /// <summary>
        /// Gets the total length of the property including the header, property data and any padding.
        /// </summary>
        /// <value>
        /// The total length of the property.
        /// </value>
        public int TotalLength
        {
            get
            {
                return this.totalLength;
            }
        }

        /// <summary>
        /// Gets the property data without cloning the array.
        /// </summary>
        /// <returns>
        /// The property data as a read only byte array.
        /// </returns>
        public byte[] GetPropertyDataReadOnly()
        {
            return this.propertyData;
        }

        /// <summary>
        /// Writes the <see cref="PIProperty"/> to the specified stream.
        /// </summary>
        /// <param name="stream">The stream.</param>
        /// <exception cref="ArgumentNullException"><paramref name="stream"/> is null.</exception>
        public void Write(UnmanagedMemoryStreamEx stream)
        {
            if (stream == null)
            {
                throw new ArgumentNullException(nameof(stream));
            }

            stream.WriteUInt32(this.vendorID);
            stream.WriteUInt32(this.propertyKey);
            stream.WriteInt32(this.propertyID);
            stream.WriteInt32(this.propertyDataLength);

            if (this.propertyData != null)
            {
                stream.Write(this.propertyData, 0, this.propertyData.Length);
                if (this.propertyDataPaddingLength > 0)
                {
                    byte[] padding = new byte[this.propertyDataPaddingLength];
                    stream.Write(padding, 0, this.propertyDataPaddingLength);
                }
            }
        }
    }
}
