﻿/////////////////////////////////////////////////////////////////////////////////
//
// Change 8bf category
//
// This software is provided under the MIT License:
//   Copyright (C) 2016-2019 Nicholas Hayes
//
// See LICENSE.txt for complete licensing and attribution information.
//
/////////////////////////////////////////////////////////////////////////////////

using ChangeFilterCategory.Interop;
using ChangeFilterCategory.Properties;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Runtime.InteropServices;
using System.Security.Permissions;

namespace ChangeFilterCategory
{
	internal sealed class FileEnumerator : IEnumerator<string>
	{
		private sealed class SearchData
		{
			public readonly string path;
			public readonly bool isShortcut;

			/// <summary>
			/// Initializes a new instance of the <see cref="SearchData"/> class.
			/// </summary>
			/// <param name="path">The path.</param>
			/// <param name="isShortcut"><c>true</c> if the path is the target of a shortcut; otherwise, <c>false</c>.</param>
			/// <exception cref="ArgumentNullException"><paramref name="path"/> is null.</exception>
			public SearchData(string path, bool isShortcut)
			{
				if (path == null)
				{
					throw new ArgumentNullException(nameof(path));
				}

				this.path = path;
				this.isShortcut = isShortcut;
			}

			/// <summary>
			/// Initializes a new instance of the <see cref="SearchData"/> class with a sub directory of the parent <c>SearchData</c>.
			/// </summary>
			/// <param name="parent">The SearchData containing the current path.</param>
			/// <param name="subDirectoryName">The name of the sub directory within the path of the parent SearchData.</param>
			/// <exception cref="ArgumentNullException">
			/// <paramref name="parent"/> is null.
			/// or
			/// <paramref name="subDirectoryName"/> is null.
			/// </exception>
			public SearchData(SearchData parent, string subDirectoryName)
			{
				if (parent == null)
				{
					throw new ArgumentNullException(nameof(parent));
				}
				if (subDirectoryName == null)
				{
					throw new ArgumentNullException(nameof(subDirectoryName));
				}

				this.path = Path.Combine(parent.path, subDirectoryName);
				this.isShortcut = parent.isShortcut;
			}
		}

		private static readonly bool IsWindows7OrLater = CheckIsWindows7OrLater();

		private static bool CheckIsWindows7OrLater()
		{
			OperatingSystem os = Environment.OSVersion;

			return os.Platform == PlatformID.Win32NT && ((os.Version.Major == 6 && os.Version.Minor >= 1) || os.Version.Major > 6);
		}

		private const int STATE_INIT = 0;
		private const int STATE_FIND_FILES = 1;
		private const int STATE_FINISH = 2;

		private readonly NativeEnums.FindExInfoLevel infoLevel;
		private readonly NativeEnums.FindExAdditionalFlags additionalFlags;
		private readonly string fileExtension;
		private readonly SearchOption searchOption;
		private readonly bool dereferenceLinks;
		private readonly uint oldErrorMode;

		private int state;
		private bool disposed;
		private SafeFindHandle handle;
		private ShellLink shellLink;
		private Queue<SearchData> searchDirectories;
		private SearchData searchData;
		private string current;
		private bool needsPathDiscoveryDemand;
		private string shellLinkTarget;

		/// <summary>
		/// Initializes a new instance of the <see cref="FileEnumerator"/> class.
		/// </summary>
		/// <param name="path">The directory to search.</param>
		/// <param name="fileExtension">The file extension to search for.</param>
		/// <param name="searchOption">
		/// One of the <see cref="SearchOption"/> values that specifies whether the search operation should include only the current directory
		/// or should include all subdirectories.
		/// </param>
		/// <param name="dereferenceLinks">If set to <c>true</c> search the target of shortcuts.</param>
		/// <exception cref="ArgumentNullException">
		/// <paramref name="path"/> in null.
		/// -or-
		/// <paramref name="fileExtension"/> is null.
		/// </exception>
		/// <exception cref="ArgumentException">
		/// <paramref name="path"/> is a 0 length string, or contains only white-space, or contains one or more invalid characters.
		/// </exception>
		/// <exception cref="ArgumentOutOfRangeException"><paramref name="searchOption"/> is not a valid <see cref="SearchOption"/> value.</exception>
		/// <exception cref="DirectoryNotFoundException">The directory specified by <paramref name="path"/> does not exist.</exception>
		/// <exception cref="IOException"><paramref name="path"/> is a file.</exception>
		/// <exception cref="PathTooLongException">The specified path, file name, or combined exceed the system-defined maximum length.</exception>
		/// <exception cref="UnauthorizedAccessException">The caller does not have the required permission.</exception>
		/// <exception cref="System.Security.SecurityException">The caller does not have the required permission.</exception>
		public FileEnumerator(string path, string fileExtension, SearchOption searchOption, bool dereferenceLinks)
		{
			if (path == null)
			{
				throw new ArgumentNullException(nameof(path));
			}

			if (fileExtension == null)
			{
				throw new ArgumentNullException(nameof(fileExtension));
			}

			if (searchOption < SearchOption.TopDirectoryOnly || searchOption > SearchOption.AllDirectories)
			{
				throw new ArgumentOutOfRangeException(nameof(searchOption));
			}

			string fullPath = Path.GetFullPath(path);
			string demandPath = GetPermissionPath(fullPath, false);
			new FileIOPermission(FileIOPermissionAccess.PathDiscovery, demandPath).Demand();
			this.needsPathDiscoveryDemand = false;

			this.fileExtension = fileExtension;
			this.searchOption = searchOption;
			this.searchDirectories = new Queue<SearchData>();
			this.searchData = new SearchData(fullPath, false);

			if (dereferenceLinks)
			{
				this.shellLink = new ShellLink();
				this.dereferenceLinks = true;
			}
			else
			{
				this.shellLink = null;
				this.dereferenceLinks = false;
			}
			this.shellLinkTarget = null;
			if (IsWindows7OrLater)
			{
				// Suppress the querying of short filenames and use a larger buffer on Windows 7 and later.
				this.infoLevel = NativeEnums.FindExInfoLevel.Basic;
				this.additionalFlags = NativeEnums.FindExAdditionalFlags.LargeFetch;
			}
			else
			{
				this.infoLevel = NativeEnums.FindExInfoLevel.Standard;
				this.additionalFlags = NativeEnums.FindExAdditionalFlags.None;
			}
			this.oldErrorMode = SetErrorModeWrapper(NativeConstants.SEM_FAILCRITICALERRORS);
			this.state = -1;
			this.current = null;
			this.disposed = false;
			Init();
		}

		/// <summary>
		/// Gets the demand path for the FileIOPermission.
		/// </summary>
		/// <param name="path">The path.</param>
		/// <param name="includeSubDirectories">if set to <c>true</c> include the sub directories of <paramref name="path"/>.</param>
		/// <returns></returns>
		private static string GetPermissionPath(string path, bool includeSubDirectories)
		{
			char end = path[path.Length - 1];

			if (!includeSubDirectories)
			{
				if (end == Path.DirectorySeparatorChar || end == Path.AltDirectorySeparatorChar)
				{
					return path + ".";
				}

				return path + Path.DirectorySeparatorChar + "."; // Demand permission for the current directory only
			}

			if (end == Path.DirectorySeparatorChar || end == Path.AltDirectorySeparatorChar)
			{
				return path;
			}

			return path + Path.DirectorySeparatorChar; // Demand permission for the current directory and all subdirectories.
		}

		/// <summary>
		/// Performs a FileIOPermission demand for PathDiscovery on the specified directory.
		/// </summary>
		/// <param name="directory">The path.</param>
		/// <exception cref="SecurityException">The caller does not have the required permission.</exception>
		private static void DoDemand(string directory)
		{
			string demandPath = GetPermissionPath(directory, false);
			new FileIOPermission(FileIOPermissionAccess.PathDiscovery, demandPath).Demand();
		}

		private static string GetWin32ErrorMessage(int error)
		{
			return new Win32Exception(error).Message;
		}

		private static int MakeHRFromWin32Error(int error)
		{
			return unchecked(((int)0x80070000) | (error & 0xffff));
		}

		private static uint SetErrorModeWrapper(uint newMode)
		{
			uint oldMode;

			if (IsWindows7OrLater)
			{
				UnsafeNativeMethods.SetThreadErrorMode(newMode, out oldMode);
			}
			else
			{
				oldMode = UnsafeNativeMethods.SetErrorMode(newMode);
			}

			return oldMode;
		}

		private bool FileMatchesFilter(string file)
		{
			return file.EndsWith(this.fileExtension, StringComparison.OrdinalIgnoreCase);
		}

		/// <summary>
		/// Throws an exception for the specified Win32 error code.
		/// </summary>
		/// <param name="win32Error">The Win32 error code.</param>
		private void HandleError(int win32Error)
		{
			Dispose();
			switch (win32Error)
			{
				case NativeConstants.ERROR_PATH_NOT_FOUND:
					throw new DirectoryNotFoundException(string.Format(CultureInfo.CurrentCulture, Resources.DirectoryNotFoundWithPath, this.searchData.path));
				case NativeConstants.ERROR_ACCESS_DENIED:
					throw new UnauthorizedAccessException(string.Format(CultureInfo.CurrentCulture, Resources.AccessDeniedWithPath, this.searchData.path));
				case NativeConstants.ERROR_DIRECTORY:
				default:
					throw new IOException(GetWin32ErrorMessage(win32Error), MakeHRFromWin32Error(win32Error));
			}
		}

		/// <summary>
		/// Initializes this instance.
		/// </summary>
		private void Init()
		{
			WIN32_FIND_DATAW findData = new WIN32_FIND_DATAW();
			string searchPath = Path.Combine(this.searchData.path, "*");
			this.handle = UnsafeNativeMethods.FindFirstFileExW(
				searchPath,
				this.infoLevel,
				findData,
				NativeEnums.FindExSearchOp.NameMatch,
				IntPtr.Zero,
				this.additionalFlags);

			if (this.handle.IsInvalid)
			{
				int error = Marshal.GetLastWin32Error();

				if (error != NativeConstants.ERROR_FILE_NOT_FOUND && error != NativeConstants.ERROR_NO_MORE_FILES)
				{
					HandleError(error);
				}
				else
				{
					// If no matching files are found exit when MoveNext is called.
					// This may happen for an empty root directory.
					this.state = STATE_FINISH;
				}
			}
			else
			{
				this.state = STATE_INIT;
				if (FirstFileIncluded(findData))
				{
					this.current = CreateFilePath(findData);
				}
			}
		}

		/// <summary>
		/// Resolves the shortcut target.
		/// </summary>
		/// <param name="path">The shortcut target to resolve.</param>
		/// <param name="isDirectory">set to <c>true</c> if the target is a directory.</param>
		/// <returns>The target of the shortcut; or null if the target does not exist.</returns>
		private static string ResolveShortcutTarget(string path, out bool isDirectory)
		{
			isDirectory = false;

			if (!string.IsNullOrEmpty(path))
			{
				uint attributes = UnsafeNativeMethods.GetFileAttributesW(path);
				if (attributes != NativeConstants.INVALID_FILE_ATTRIBUTES)
				{
					isDirectory = (attributes & NativeConstants.FILE_ATTRIBUTE_DIRECTORY) == NativeConstants.FILE_ATTRIBUTE_DIRECTORY;
					return path;
				}
			}

			return null;
		}

		private bool FirstFileIncluded(WIN32_FIND_DATAW findData)
		{
			if ((findData.dwFileAttributes & NativeConstants.FILE_ATTRIBUTE_DIRECTORY) == NativeConstants.FILE_ATTRIBUTE_DIRECTORY)
			{
				if (this.searchOption == SearchOption.AllDirectories && !findData.cFileName.Equals(".") && !findData.cFileName.Equals(".."))
				{
					this.searchDirectories.Enqueue(new SearchData(this.searchData, findData.cFileName));
				}
			}
			else
			{
				return IsFileIncluded(findData);
			}

			return false;
		}

		private bool IsFileIncluded(WIN32_FIND_DATAW findData)
		{
			if (findData.cFileName.EndsWith(".lnk", StringComparison.OrdinalIgnoreCase) && this.dereferenceLinks)
			{
				// Do not search shortcuts recursively.
				if (!this.searchData.isShortcut && this.shellLink.Load(Path.Combine(this.searchData.path, findData.cFileName)))
				{
					bool isDirectory;
					string target = ResolveShortcutTarget(this.shellLink.Path, out isDirectory);

					if (!string.IsNullOrEmpty(target))
					{
						if (isDirectory)
						{
							// If the shortcut target is a directory, add it to the search list.
							this.searchDirectories.Enqueue(new SearchData(target, true));
						}
						else if (FileMatchesFilter(target))
						{
							this.shellLinkTarget = target;
							return true;
						}
					}
				}
			}
			else if (FileMatchesFilter(findData.cFileName))
			{
				if (this.needsPathDiscoveryDemand)
				{
					DoDemand(this.searchData.path);
					this.needsPathDiscoveryDemand = false;
				}
				this.shellLinkTarget = null;

				return true;
			}

			return false;
		}

		private string CreateFilePath(WIN32_FIND_DATAW findData)
		{
			if (this.shellLinkTarget != null)
			{
				return this.shellLinkTarget;
			}
			else
			{
				return Path.Combine(this.searchData.path, findData.cFileName);
			}
		}

		/// <summary>
		/// Gets the element in the collection at the current position of the enumerator.
		/// </summary>
		public string Current
		{
			get
			{
				return this.current;
			}
		}

		/// <summary>
		/// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
		/// </summary>
		public void Dispose()
		{
			if (!this.disposed)
			{
				this.disposed = true;

				if (this.handle != null)
				{
					this.handle.Dispose();
					this.handle = null;
				}

				if (this.shellLink != null)
				{
					this.shellLink.Dispose();
					this.shellLink = null;
				}
				this.current = null;
				this.state = -1;
				SetErrorModeWrapper(this.oldErrorMode);
			}
		}

		/// <summary>
		/// Gets the element in the collection at the current position of the enumerator.
		/// </summary>
		object IEnumerator.Current
		{
			get
			{
				if (this.current == null)
				{
					throw new InvalidOperationException();
				}

				return this.current;
			}
		}

		/// <summary>
		/// Advances the enumerator to the next element of the collection.
		/// </summary>
		/// <returns>
		/// true if the enumerator was successfully advanced to the next element; false if the enumerator has passed the end of the collection.
		/// </returns>
		public bool MoveNext()
		{
			WIN32_FIND_DATAW findData = new WIN32_FIND_DATAW();

			switch (this.state)
			{
				case STATE_INIT:
					this.state = STATE_FIND_FILES;

					if (this.current != null)
					{
						return true;
					}
					else
					{
						goto case STATE_FIND_FILES;
					}
				case STATE_FIND_FILES:
					do
					{
						if (this.handle == null)
						{
							this.searchData = this.searchDirectories.Dequeue();

							string demandPath = GetPermissionPath(this.searchData.path, false);
							new FileIOPermission(FileIOPermissionAccess.PathDiscovery, demandPath).Demand();
							string searchPath = Path.Combine(this.searchData.path, "*");
							this.handle = UnsafeNativeMethods.FindFirstFileExW(
								searchPath,
								this.infoLevel,
								findData,
								NativeEnums.FindExSearchOp.NameMatch,
								IntPtr.Zero,
								this.additionalFlags);

							if (this.handle.IsInvalid)
							{
								this.handle.Dispose();
								this.handle = null;

								if (this.searchDirectories.Count > 0)
								{
									continue;
								}
								else
								{
									this.state = STATE_FINISH;
									goto case STATE_FINISH;
								}
							}
							this.needsPathDiscoveryDemand = true;
							if (FirstFileIncluded(findData))
							{
								this.current = CreateFilePath(findData);
								return true;
							}
						}

						while (UnsafeNativeMethods.FindNextFileW(this.handle, findData))
						{
							if ((findData.dwFileAttributes & NativeConstants.FILE_ATTRIBUTE_DIRECTORY) == 0)
							{
								if (IsFileIncluded(findData))
								{
									this.current = CreateFilePath(findData);
									return true;
								}
							}
							else if (this.searchOption == SearchOption.AllDirectories && !findData.cFileName.Equals(".") && !findData.cFileName.Equals(".."))
							{
								this.searchDirectories.Enqueue(new SearchData(this.searchData, findData.cFileName));
							}
						}

						this.handle.Dispose();
						this.handle = null;

					} while (this.searchDirectories.Count > 0);

					this.state = STATE_FINISH;
					goto case STATE_FINISH;
				case STATE_FINISH:
					Dispose();
					break;
			}

			return false;
		}

		/// <summary>
		/// Sets the enumerator to its initial position, which is before the first element in the collection.
		/// </summary>
		void IEnumerator.Reset()
		{
			throw new NotSupportedException();
		}
	}
}
