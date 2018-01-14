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

using Microsoft.Win32.SafeHandles;
using System.Security.Permissions;

// The following code is quoted from Mike Stall's blog
// Type-safe Managed wrappers for kernel32!GetProcAddress
// http://blogs.msdn.com/b/jmstall/archive/2007/01/06/typesafe-getprocaddress.aspx

namespace ChangeFilterCategory.Interop
{
	[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.UnmanagedCode)]
	internal sealed class SafeLibraryHandle : SafeHandleZeroOrMinusOneIsInvalid
	{
		/// <summary>
		/// Create safe library handle
		/// </summary>
		private SafeLibraryHandle() : base(true)
		{
		}

		/// <summary>
		/// Release handle
		/// </summary>
		protected override bool ReleaseHandle()
		{
			return UnsafeNativeMethods.FreeLibrary(handle);
		}
	}
}


