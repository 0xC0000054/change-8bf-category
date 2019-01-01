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
    internal static class NativeInterfaces
    {
        [ComImport]
        [Guid(NativeConstants.IID_IShellLinkW)]
        [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        internal interface IShellLinkW
        {
            [PreserveSig]
            int GetPath([Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder pszFile, int cchMaxPath, IntPtr pfd, uint fFlags);

            [PreserveSig]
            int GetIDList(out IntPtr ppidl);

            [PreserveSig]
            int SetIDList(IntPtr pidl);

            [PreserveSig]
            int GetDescription([Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder pszName, int cchMaxName);

            [PreserveSig]
            int SetDescription([MarshalAs(UnmanagedType.LPWStr)] string pszName);

            [PreserveSig]
            int GetWorkingDirectory([Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder pszDir, int cchMaxPath);

            [PreserveSig]
            int SetWorkingDirectory([MarshalAs(UnmanagedType.LPWStr)] string pszDir);

            [PreserveSig]
            int GetArguments([Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder pszArgs, int cchMaxPath);

            [PreserveSig]
            int SetArguments([MarshalAs(UnmanagedType.LPWStr)] string pszArgs);

            [PreserveSig]
            int GetHotkey(out short pwHotkey);

            [PreserveSig]
            int SetHotkey(short wHotkey);

            [PreserveSig]
            int GetShowCmd(out int piShowCmd);

            [PreserveSig]
            int SetShowCmd(int iShowCmd);

            [PreserveSig]
            int GetIconLocation([Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder pszIconPath, int cchIconPath, out int piIcon);

            [PreserveSig]
            int SetIconLocation([MarshalAs(UnmanagedType.LPWStr)] string pszIconPath, int iIcon);

            [PreserveSig]
            int SetRelativePath([MarshalAs(UnmanagedType.LPWStr)] string pszPathRel, int dwReserved);

            [PreserveSig]
            int Resolve(IntPtr hwnd, uint fFlags);

            [PreserveSig]
            int SetPath([MarshalAs(UnmanagedType.LPWStr)] string pszFile);
        }

        [ComImport]
        [Guid(NativeConstants.IID_IPersist)]
        [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        internal interface IPersist
        {
            [PreserveSig]
            int GetClassID(out Guid pClassID);
        }

        [ComImport]
        [Guid(NativeConstants.IID_IPersistFile)]
        [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        internal interface IPersistFile : IPersist
        {
            [PreserveSig]
            new int GetClassID(out Guid pClassID);

            [PreserveSig]
            int IsDirty();

            [PreserveSig]
            int Load([MarshalAs(UnmanagedType.LPWStr)] string pszFileName, uint dwMode);

            [PreserveSig]
            int Save([MarshalAs(UnmanagedType.LPWStr)] string pszFileName, [MarshalAs(UnmanagedType.Bool)] bool fRemember);

            [PreserveSig]
            int SaveCompleted([MarshalAs(UnmanagedType.LPWStr)] string pszFileName);

            [PreserveSig]
            int GetCurFile([MarshalAs(UnmanagedType.LPWStr)] string ppszFileName);
        }

        [ComImport]
        [Guid(NativeConstants.IID_IModalWindow)]
        [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        internal interface IModalWindow
        {
            [PreserveSig]
            int Show(IntPtr parent);
        }

        [ComImport]
        [Guid(NativeConstants.IID_IFileDialog)]
        [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        internal interface IFileDialog : IModalWindow
        {
            // Defined on IModalWindow - repeated here due to requirements of COM interop layer
            // --------------------------------------------------------------------------------
            [PreserveSig]
            new int Show(IntPtr parent);

            // IFileDialog-Specific interface members
            // --------------------------------------------------------------------------------

            void SetFileTypes(
                uint cFileTypes,
                [MarshalAs(UnmanagedType.LPArray)] NativeStructs.COMDLG_FILTERSPEC[] rgFilterSpec);

            void SetFileTypeIndex(uint iFileType);

            void GetFileTypeIndex(out uint piFileType);

            void Advise([MarshalAs(UnmanagedType.Interface)] IFileDialogEvents pfde, out uint pdwCookie);

            void Unadvise(uint dwCookie);

            void SetOptions(NativeEnums.FOS fos);

            void GetOptions(out NativeEnums.FOS pfos);

            void SetDefaultFolder([MarshalAs(UnmanagedType.Interface)] IShellItem psi);

            void SetFolder([MarshalAs(UnmanagedType.Interface)] IShellItem psi);

            void GetFolder([MarshalAs(UnmanagedType.Interface)] out IShellItem ppsi);

            void GetCurrentSelection([MarshalAs(UnmanagedType.Interface)] out IShellItem ppsi);

            void SetFileName([MarshalAs(UnmanagedType.LPWStr)] string pszName);

            void GetFileName([MarshalAs(UnmanagedType.LPWStr)] out string pszName);

            void SetTitle([MarshalAs(UnmanagedType.LPWStr)] string pszTitle);

            void SetOkButtonLabel([MarshalAs(UnmanagedType.LPWStr)] string pszText);

            void SetFileNameLabel([MarshalAs(UnmanagedType.LPWStr)] string pszLabel);

            void GetResult([MarshalAs(UnmanagedType.Interface)] out IShellItem ppsi);

            void AddPlace([MarshalAs(UnmanagedType.Interface)] IShellItem psi, NativeEnums.FDAP fdap);

            void SetDefaultExtension([MarshalAs(UnmanagedType.LPWStr)] string pszDefaultExtension);

            void Close([MarshalAs(UnmanagedType.Error)] int hr);

            void SetClientGuid(ref Guid guid);

            void ClearClientData();

            // Not supported:  IShellItemFilter is not defined, converting to IntPtr

            void SetFilter([MarshalAs(UnmanagedType.Interface)] IntPtr pFilter);
        }

        [ComImport]
        [Guid(NativeConstants.IID_IFileOpenDialog)]
        [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        internal interface IFileOpenDialog : IFileDialog
        {
            // Defined on IModalWindow - repeated here due to requirements of COM interop layer
            // --------------------------------------------------------------------------------
            [PreserveSig]
            new int Show(IntPtr parent);

            // Defined on IFileDialog - repeated here due to requirements of COM interop layer

            new void SetFileTypes(
                uint cFileTypes,
                [MarshalAs(UnmanagedType.LPArray)] NativeStructs.COMDLG_FILTERSPEC[] rgFilterSpec);

            new void SetFileTypeIndex(uint iFileType);

            new void GetFileTypeIndex(out uint piFileType);

            new void Advise([MarshalAs(UnmanagedType.Interface)] IFileDialogEvents pfde, out uint pdwCookie);

            new void Unadvise(uint dwCookie);

            new void SetOptions(NativeEnums.FOS fos);

            new void GetOptions(out NativeEnums.FOS pfos);

            new void SetDefaultFolder([MarshalAs(UnmanagedType.Interface)] IShellItem psi);

            new void SetFolder([MarshalAs(UnmanagedType.Interface)] IShellItem psi);

            new void GetFolder([MarshalAs(UnmanagedType.Interface)] out IShellItem ppsi);

            new void GetCurrentSelection([MarshalAs(UnmanagedType.Interface)] out IShellItem ppsi);

            new void SetFileName([MarshalAs(UnmanagedType.LPWStr)] string pszName);

            new void GetFileName([MarshalAs(UnmanagedType.LPWStr)] out string pszName);

            new void SetTitle([MarshalAs(UnmanagedType.LPWStr)] string pszTitle);

            new void SetOkButtonLabel([MarshalAs(UnmanagedType.LPWStr)] string pszText);

            new void SetFileNameLabel([MarshalAs(UnmanagedType.LPWStr)] string pszLabel);

            new void GetResult([MarshalAs(UnmanagedType.Interface)] out IShellItem ppsi);

            new void AddPlace([MarshalAs(UnmanagedType.Interface)] IShellItem psi, NativeEnums.FDAP fdap);

            new void SetDefaultExtension([MarshalAs(UnmanagedType.LPWStr)] string pszDefaultExtension);

            new void Close([MarshalAs(UnmanagedType.Error)] int hr);

            new void SetClientGuid(ref Guid guid);

            new void ClearClientData();

            // Not supported:  IShellItemFilter is not defined, converting to IntPtr

            new void SetFilter([MarshalAs(UnmanagedType.Interface)] IntPtr pFilter);

            // Defined by IFileOpenDialog
            // ---------------------------------------------------------------------------------

            void GetResults([MarshalAs(UnmanagedType.Interface)] out IShellItemArray ppenum);

            void GetSelectedItems([MarshalAs(UnmanagedType.Interface)] out IShellItemArray ppsai);
        }

        [ComImport]
        [Guid(NativeConstants.IID_IFileDialogEvents)]
        [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        internal interface IFileDialogEvents
        {

            // NOTE: some of these callbacks are cancelable - returning S_FALSE means that
            // the dialog should not proceed (e.g. with closing, changing folder); to
            // support this, we need to use the PreserveSig attribute to enable us to return
            // the proper HRESULT
            [PreserveSig]
            int OnFileOk([MarshalAs(UnmanagedType.Interface)] IFileDialog pfd);

            [PreserveSig]
            int OnFolderChanging(
                [MarshalAs(UnmanagedType.Interface)] IFileDialog pfd,
                [MarshalAs(UnmanagedType.Interface)] IShellItem psiFolder);

            void OnFolderChange([MarshalAs(UnmanagedType.Interface)] IFileDialog pfd);

            void OnSelectionChange([MarshalAs(UnmanagedType.Interface)] IFileDialog pfd);

            void OnShareViolation(
                [MarshalAs(UnmanagedType.Interface)] IFileDialog pfd,
                [MarshalAs(UnmanagedType.Interface)] IShellItem psi,
                out NativeEnums.FDE_SHAREVIOLATION_RESPONSE pResponse);

            void OnTypeChange([MarshalAs(UnmanagedType.Interface)] IFileDialog pfd);

            void OnOverwrite(
                [MarshalAs(UnmanagedType.Interface)] IFileDialog pfd,
                [MarshalAs(UnmanagedType.Interface)] IShellItem psi,
                out NativeEnums.FDE_OVERWRITE_RESPONSE pResponse);
        }

        [ComImport]
        [Guid(NativeConstants.IID_IFileDialogCustomize)]
        [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        internal interface IFileDialogCustomize
        {
            void EnableOpenDropDown(uint dwIDCtl);

            void AddMenu(uint dwIDCtl, [MarshalAs(UnmanagedType.LPWStr)] string pszLabel);

            void AddPushButton(uint dwIDCtl, [MarshalAs(UnmanagedType.LPWStr)] string pszLabel);

            void AddComboBox(uint dwIDCtl);

            void AddRadioButtonList(uint dwIDCtl);

            void AddCheckButton(uint dwIDCtl, [MarshalAs(UnmanagedType.LPWStr)] string pszLabel, bool bChecked);

            void AddEditBox(uint dwIDCtl, [MarshalAs(UnmanagedType.LPWStr)] string pszText);

            void AddSeparator(uint dwIDCtl);

            void AddText(uint dwIDCtl, [MarshalAs(UnmanagedType.LPWStr)] string pszText);

            void SetControlLabel(uint dwIDCtl, [MarshalAs(UnmanagedType.LPWStr)] string pszLabel);

            void GetControlState(uint dwIDCtl, out NativeEnums.CDCONTROLSTATE pdwState);

            void SetControlState(uint dwIDCtl, NativeEnums.CDCONTROLSTATE dwState);

            void GetEditBoxText(uint dwIDCtl, out IntPtr ppszText);

            void SetEditBoxText(uint dwIDCtl, [MarshalAs(UnmanagedType.LPWStr)] string pszText);

            void GetCheckButtonState(uint dwIDCtl, out bool pbChecked);

            void SetCheckButtonState(uint dwIDCtl, bool bChecked);

            void AddControlItem(uint dwIDCtl, uint dwIDItem, [MarshalAs(UnmanagedType.LPWStr)] string pszLabel);

            void RemoveControlItem(uint dwIDCtl, uint dwIDItem);

            void RemoveAllControlItems(uint dwIDCtl);

            void GetControlItemState(uint dwIDCtl, uint dwIDItem, out NativeEnums.CDCONTROLSTATE pdwState);

            void SetControlItemState(uint dwIDCtl, uint dwIDItem, NativeEnums.CDCONTROLSTATE dwState);

            void GetSelectedControlItem(uint dwIDCtl, out int pdwIDItem);

            void SetSelectedControlItem(uint dwIDCtl, uint dwIDItem); // Not valid for OpenDropDown

            void StartVisualGroup(uint dwIDCtl, [MarshalAs(UnmanagedType.LPWStr)] string pszLabel);

            void EndVisualGroup();

            void MakeProminent(uint dwIDCtl);
        }

        [ComImport]
        [Guid(NativeConstants.IID_IFileDialogControlEvents)]
        [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        internal interface IFileDialogControlEvents
        {
            void OnItemSelected([MarshalAs(UnmanagedType.Interface)] IFileDialogCustomize pfdc, uint dwIDCtl, uint dwIDItem);

            void OnButtonClicked([MarshalAs(UnmanagedType.Interface)] IFileDialogCustomize pfdc, uint dwIDCtl);

            void OnCheckButtonToggled([MarshalAs(UnmanagedType.Interface)] IFileDialogCustomize pfdc, uint dwIDCtl, bool bChecked);

            void OnControlActivating([MarshalAs(UnmanagedType.Interface)] IFileDialogCustomize pfdc, uint dwIDCtl);
        }

        [ComImport]
        [Guid(NativeConstants.IID_IShellItem)]
        [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        internal interface IShellItem
        {
            // Not supported: IBindCtx

            void BindToHandler(IntPtr pbc, ref Guid bhid, ref Guid riid, out IntPtr ppv);

            void GetParent([MarshalAs(UnmanagedType.Interface)] out IShellItem ppsi);

            void GetDisplayName(NativeEnums.SIGDN sigdnName, [MarshalAs(UnmanagedType.LPWStr)] out string ppwszName);

            void GetAttributes(NativeEnums.SFGAO sfgaoMask, out NativeEnums.SFGAO psfgaoAttribs);

            void Compare([MarshalAs(UnmanagedType.Interface)] IShellItem psi, uint hint, out int piOrder);
        }

        [ComImport]
        [Guid(NativeConstants.IID_IShellItemArray)]
        [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        internal interface IShellItemArray
        {
            // Not supported: IBindCtx

            void BindToHandler([MarshalAs(UnmanagedType.Interface)] IntPtr pbc, ref Guid rbhid, ref Guid riid, out IntPtr ppvOut);

            void GetPropertyStore(int Flags, ref Guid riid, out IntPtr ppv);

            void GetPropertyDescriptionList(ref NativeStructs.PROPERTYKEY keyType, ref Guid riid, out IntPtr ppv);

            void GetAttributes(NativeEnums.SIATTRIBFLAGS dwAttribFlags, NativeEnums.SFGAO sfgaoMask, out NativeEnums.SFGAO psfgaoAttribs);

            void GetCount(out uint pdwNumItems);

            void GetItemAt(uint dwIndex, [MarshalAs(UnmanagedType.Interface)] out IShellItem ppsi);

            // Not supported: IEnumShellItems (will use GetCount and GetItemAt instead)

            void EnumItems([MarshalAs(UnmanagedType.Interface)] out IntPtr ppenumShellItems);
        }

        // ---------------------------------------------------------
        // Coclass interfaces - designed to "look like" the object
        // in the API, so that the 'new' operator can be used in a
        // straightforward way. Behind the scenes, the C# compiler
        // morphs all 'new CoClass()' calls to 'new CoClassWrapper()'

        [ComImport]
        [Guid(NativeConstants.IID_IFileOpenDialog)]
        [CoClass(typeof(FileOpenDialogRCW))]
        internal interface NativeFileOpenDialog : IFileOpenDialog
        {
        }

        [ComImport]
        [Guid(NativeConstants.IID_IShellLinkW)]
        [CoClass(typeof(ShellLinkRCW))]
        internal interface NativeShellLink : IShellLinkW
        {
        }

        // ---------------------------------------------------
        // .NET classes representing runtime callable wrappers

        [ComImport]
        [ClassInterface(ClassInterfaceType.None)]
        [TypeLibType(TypeLibTypeFlags.FCanCreate)]
        [Guid(NativeConstants.CLSID_FileOpenDialog)]
        internal class FileOpenDialogRCW
        {
        }

        [ComImport]
        [Guid(NativeConstants.CLSID_ShellLink)]
        internal class ShellLinkRCW
        {
        }
    }
}
