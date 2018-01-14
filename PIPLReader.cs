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

using ChangeFilterCategory.Interop;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace ChangeFilterCategory
{
    internal static class PIPLReader
    {
        private sealed class QueryPlugin
        {
            public readonly string path;
            public List<PluginData> plugins;

            public QueryPlugin(string path)
            {
                this.path = path;
                this.plugins = new List<PluginData>();
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage(
            "Microsoft.Reliability",
            "CA2001:AvoidCallingProblematicMethods",
            MessageId = "System.Runtime.InteropServices.SafeHandle.DangerousGetHandle",
            Justification = "Required as a SafeHandle cannot be used for methods that pass the underlying handle to a managed callback.")]
        internal static IEnumerable<PluginData> LoadPluginsFromFile(string path)
        {
            List<PluginData> pluginData = new List<PluginData>();

            using (SafeLibraryHandle dll = UnsafeNativeMethods.LoadLibraryExW(path, IntPtr.Zero, NativeConstants.LOAD_LIBRARY_AS_DATAFILE))
            {
                if (!dll.IsInvalid)
                {
                    QueryPlugin query = new QueryPlugin(path);

                    GCHandle handle = GCHandle.Alloc(query, GCHandleType.Normal);
                    bool needsRelease = false;
                    System.Runtime.CompilerServices.RuntimeHelpers.PrepareConstrainedRegions();
                    try
                    {
                        dll.DangerousAddRef(ref needsRelease);
                        IntPtr callback = GCHandle.ToIntPtr(handle);
                        if (UnsafeNativeMethods.EnumResourceNamesW(dll.DangerousGetHandle(), "PIPl", EnumPIPL, callback))
                        {
                            query = (QueryPlugin)GCHandle.FromIntPtr(callback).Target;

                            pluginData.AddRange(query.plugins);
                        }
                    }
                    finally
                    {
                        if (handle.IsAllocated)
                        {
                            handle.Free();
                        }

                        if (needsRelease)
                        {
                            dll.DangerousRelease();
                        }
                    }

                }
            }

            return pluginData;
        }

        private static unsafe bool EnumPIPL(IntPtr hModule, IntPtr lpszType, IntPtr lpszName, IntPtr lParam)
        {
            GCHandle handle = GCHandle.FromIntPtr(lParam);
            QueryPlugin data = (QueryPlugin)handle.Target;

            IntPtr hRes = UnsafeNativeMethods.FindResourceW(hModule, lpszName, lpszType);
            if (hRes == IntPtr.Zero)
            {
#if DEBUG
                System.Diagnostics.Debug.WriteLine(string.Format(System.Globalization.CultureInfo.CurrentCulture, "FindResource failed for PiPL in {0}", data.path));
#endif
                return true;
            }

            IntPtr loadRes = UnsafeNativeMethods.LoadResource(hModule, hRes);
            if (loadRes == IntPtr.Zero)
            {
#if DEBUG
                System.Diagnostics.Debug.WriteLine(string.Format(System.Globalization.CultureInfo.CurrentCulture, "LoadResource failed for PiPL in {0}", data.path));
#endif
                return true;
            }

            IntPtr lockRes = UnsafeNativeMethods.LockResource(loadRes);
            if (lockRes == IntPtr.Zero)
            {
#if DEBUG
                System.Diagnostics.Debug.WriteLine(string.Format(System.Globalization.CultureInfo.CurrentCulture, "LockResource failed for PiPL in {0}", data.path));
#endif

                return true;
            }

            short signature = Marshal.ReadInt16(lockRes);

            if (signature == PIPLConstants.ResourceSignature)
            {
                int version = Marshal.ReadInt32(lockRes, 2);

                if (version == PIPLConstants.ResourceVersion)
                {
                    int count = Marshal.ReadInt32(lockRes, 6);

                    if (count > 0)
                    {
                        byte* propPtr = (byte*)lockRes.ToPointer() + PIPLConstants.ResourceHeaderSize;

                        PIProperty[] properties = new PIProperty[count];
                        for (int i = 0; i < count; i++)
                        {
                            PIProperty property = new PIProperty(propPtr);
                            if (property.Key == PIPropertyID.PIKindProperty)
                            {
                                const uint FilterKind = 0x3842464d; // 8BFM

                                if (BitConverter.ToUInt32(property.GetPropertyDataReadOnly(), 0) != FilterKind)
                                {
                                    // Skip any PiPL resources that are not filters.
                                    return true;
                                }
                            }

                            properties[i] = property;
                            propPtr += property.TotalLength;
                        }

                        // Get the language that the PiPL resource is stored in.
                        ushort language = 0;

                        UnsafeNativeMethods.EnumResourceLanguagesW(hModule, lpszType, lpszName, EnumResourceLanguages, new IntPtr(&language));

                        data.plugins.Add(new PluginData(data.path, GetPIPLResourceName(lpszName), language, properties));
                        handle.Target = data;
                    }
                }
            }

            return true;
        }

        private static unsafe bool EnumResourceLanguages(IntPtr hModule, IntPtr lpszType, IntPtr lpszName, ushort wLangId, IntPtr lParam)
        {
            ushort* ptr = (ushort*)lParam.ToPointer();
            *ptr = wLangId;

            // Return false as there should only be one PiPL resource for a given resource id.
            return false;
        }

        private static PIPLResourceName GetPIPLResourceName(IntPtr lpszName)
        {
            // A Win32 resource name can be specified as an ordinal or a string.
            if (IsIntResource(lpszName))
            {
                return new PIPLResourceName(lpszName.ToInt32());
            }
            else
            {
                return new PIPLResourceName(Marshal.PtrToStringUni(lpszName));
            }
        }

        private static bool IsIntResource(IntPtr ptr)
        {
            const long IntResourceMask = unchecked((long)0xFFFFFFFFFFFF0000);

            return ((ptr.ToInt64() & IntResourceMask) == 0);
        }
    }
}
