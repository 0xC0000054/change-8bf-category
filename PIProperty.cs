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
            vendorID = *(uint*)ptr;
            propertyKey = *(uint*)(ptr + 4);
            propertyID = *(int*)(ptr + 8);
            propertyDataLength = *(int*)(ptr + 12);
            if (propertyDataLength > 0)
            {
                propertyData = new byte[propertyDataLength];
                Marshal.Copy((IntPtr)(ptr + 16), propertyData, 0, propertyDataLength);
            }
            else
            {
                propertyData = null;
            }
            propertyDataPaddingLength = (4 - propertyDataLength) & 3;
            totalLength = PropertyHeaderLength + propertyDataLength + propertyDataPaddingLength;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PIProperty"/> class, with the specified key and value.
        /// </summary>
        /// <param name="key">The property key.</param>
        /// <param name="value">The byte array containing the property data.</param>
        public PIProperty(uint key, byte[] value)
        {
            vendorID = PIPLConstants.VendorIDPhotoshop;
            propertyKey = key;
            propertyID = 0;
            if (value != null && value.Length > 0)
            {
                // The Photoshop SDK states that the property data length does not include the padding bytes
                // required to achieve four byte alignment, but some hosts depend on it being included.
                propertyDataLength = (value.Length + 4) & ~3;
                propertyData = (byte[])value.Clone();
                propertyDataPaddingLength = propertyDataLength - value.Length;
            }
            else
            {
                propertyDataLength = 0;
                propertyData = null;
                propertyDataPaddingLength = 0;
            }
            totalLength = PropertyHeaderLength + propertyDataLength;
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
                return propertyKey;
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
                return totalLength;
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
            return propertyData;
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

            stream.WriteUInt32(vendorID);
            stream.WriteUInt32(propertyKey);
            stream.WriteInt32(propertyID);
            stream.WriteInt32(propertyDataLength);

            if (propertyData != null)
            {
                stream.Write(propertyData, 0, propertyData.Length);
                if (propertyDataPaddingLength > 0)
                {
                    byte[] padding = new byte[propertyDataPaddingLength];
                    stream.Write(padding, 0, propertyDataPaddingLength);
                }
            }
        }
    }
}
