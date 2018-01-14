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

using ChangeFilterCategory.Interop;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Runtime.InteropServices;

namespace ChangeFilterCategory
{
    internal static class PIPLWriter
    {
        /// <summary>
        /// Updates the plug-in with the changes to the PiPL resource.
        /// </summary>
        /// <param name="data">The <see cref="PluginData"/> to write.</param>
        /// <exception cref="ArgumentNullException"><paramref name="data"/> is null.</exception>
        /// <exception cref="OutOfMemoryException">Insufficient memory to create the new PiPL resource.</exception>
        /// <exception cref="Win32Exception">An error occurred updating the resource.</exception>
        internal static void UpdatePIPLResource(PluginData data)
        {
            if (data == null)
            {
                throw new ArgumentNullException(nameof(data));
            }

            if (data.Dirty)
            {
                IntPtr hUpdateResource = UnsafeNativeMethods.BeginUpdateResourceW(data.Path, false);
                if (hUpdateResource == IntPtr.Zero)
                {
                    throw new Win32Exception();
                }

                try
                {
                    UpdateResource(hUpdateResource, data);
                }
                catch (Exception)
                {
                    UnsafeNativeMethods.EndUpdateResourceW(hUpdateResource, true);
                    throw;
                }

                if (!UnsafeNativeMethods.EndUpdateResourceW(hUpdateResource, false))
                {
                    throw new Win32Exception();
                }
                data.Dirty = false;
            }
        }

        /// <summary>
        /// Updates the PiPL resource in the file.
        /// </summary>
        /// <param name="hUpdateResource">The resource handle opened by BeginUpdateResource.</param>
        /// <param name="data">The data to write.</param>
        /// <exception cref="ArgumentNullException"><paramref name="data"/> is null.</exception>
        /// <exception cref="OutOfMemoryException">Insufficient memory to create the new PiPL resource.</exception>
        /// <exception cref="Win32Exception">An error occurred updating the resource.</exception>
        private static void UpdateResource(IntPtr hUpdateResource, PluginData data)
        {
            if (data == null)
            {
                throw new ArgumentNullException(nameof(data));
            }

            long resourceSize = GetPIPLResourceSize(data);
            IntPtr resourceData = Marshal.AllocHGlobal(new IntPtr(resourceSize));

            try
            {
                WriteNewPIPLResource(data.Properties, resourceData, resourceSize);

                if (data.ResourceName.IsString)
                {
                    if (!UnsafeNativeMethods.UpdateResourceW(hUpdateResource, "PIPL", data.ResourceName.Name, data.ResourceLanguage, resourceData, (uint)resourceSize))
                    {
                        throw new Win32Exception();
                    }
                }
                else
                {
                    if (!UnsafeNativeMethods.UpdateResourceW(hUpdateResource, "PIPL", (IntPtr)data.ResourceName.Ordinal, data.ResourceLanguage, resourceData, (uint)resourceSize))
                    {
                        throw new Win32Exception();
                    }
                }
            }
            finally
            {
                Marshal.FreeHGlobal(resourceData);
            }
        }

        /// <summary>
        /// Gets the size in bytes of the PiPL resource.
        /// </summary>
        /// <param name="data">The <see cref="PluginData"/> containing the collection of PiPL resources.</param>
        /// <returns>
        /// The size in bytes of the PiPL resource.
        /// </returns>
        /// <exception cref="ArgumentNullException"><paramref name="data"/> is null.</exception>
        private static long GetPIPLResourceSize(PluginData data)
        {
            if (data == null)
            {
                throw new ArgumentNullException(nameof(data));
            }

            long size = PIPLConstants.ResourceHeaderSize;

            ReadOnlyCollection<PIProperty> properties = data.Properties;
            for (int i = 0; i < properties.Count; i++)
            {
                size += properties[i].TotalLength;
            }

            return size;
        }

        /// <summary>
        /// Writes the new PiPL resource to the specified unmanaged memory buffer.
        /// </summary>
        /// <param name="properties">The collection of PiPL resources to write.</param>
        /// <param name="buffer">The unmanaged memory buffer to write to.</param>
        /// <param name="bufferLength">The length of the unmanaged memory buffer.</param>
        /// <exception cref="ArgumentNullException"><paramref name="properties"/> is null.</exception>
        private static void WriteNewPIPLResource(ReadOnlyCollection<PIProperty> properties, IntPtr buffer, long bufferLength)
        {
            if (properties == null)
            {
                throw new ArgumentNullException(nameof(properties));
            }

            using (UnmanagedMemoryStreamEx stream = new UnmanagedMemoryStreamEx(buffer, bufferLength, FileAccess.Write))
            {
                // Write the PIPL resource header.
                stream.WriteInt16(PIPLConstants.ResourceSignature);
                stream.WriteInt32(PIPLConstants.ResourceVersion);
                stream.WriteInt32(properties.Count);

                // Write the PIPL resources.
                for (int i = 0; i < properties.Count; i++)
                {
                    properties[i].Write(stream);
                }
            }
        }
    }
}
