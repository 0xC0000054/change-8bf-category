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
using System.Text;

namespace ChangeFilterCategory.Interop
{
    /// <summary>
    /// Encapsulates a ShellLink shortcut file
    /// </summary>
    internal sealed class ShellLink : IDisposable
    {
        private NativeInterfaces.IShellLinkW shellLink;
        private bool disposed;

        /// <summary>
        /// Initializes a new instance of the <see cref="ShellLink"/> class.
        /// </summary>
        public ShellLink()
        {
            shellLink = new NativeInterfaces.NativeShellLink();
            disposed = false;
        }

        /// <summary>
        /// Loads a shortcut from a file.
        /// </summary>
        /// <param name="linkPath">The shortcut to load.</param>
        public bool Load(string linkPath)
        {
            return ((NativeInterfaces.IPersistFile)shellLink).Load(linkPath, NativeConstants.STGM_READ) == NativeConstants.S_OK;
        }

        /// <summary>
        /// Gets the target path of the shortcut.
        /// </summary>
        public string Path
        {
            get
            {
                StringBuilder sb = new StringBuilder(NativeConstants.MAX_PATH);

                if (shellLink.GetPath(sb, sb.MaxCapacity, IntPtr.Zero, 0U) != NativeConstants.S_OK)
                {
                    return string.Empty;
                }

                return sb.ToString();
            }
        }

        /// <summary>
        /// Releases unmanaged resources and performs other cleanup operations before the
        /// <see cref="ShellLink"/> is reclaimed by garbage collection.
        /// </summary>
        ~ShellLink()
        {
            Dispose(false);
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            if (!disposed)
            {
                disposed = true;

                if (disposing)
                {
                    if (shellLink != null)
                    {
                        Marshal.ReleaseComObject(shellLink);
                        shellLink = null;
                    }
                }
            }
        }
    }
}
