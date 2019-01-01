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

using ChangeFilterCategory.Interop;
using System;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;

namespace ChangeFilterCategory
{
    /// <summary>
    /// Prompts the user to select a folder using a Vista-style dialog.
    /// </summary>
    /// <seealso cref="CommonDialog"/>
    [DefaultProperty("SelectedPath")]
    [Description("Prompts the user to select a folder using a Vista-style dialog.")]
    internal sealed class VistaFolderBrowserDialog : CommonDialog
    {
        private string title;
        private string defaultFolder;
        private string selectedPath;
        private bool addToRecentDocuments;

        /// <summary>
        /// Initializes a new instance of the <see cref="VistaFolderBrowserDialog"/> class.
        /// </summary>
        /// <exception cref="PlatformNotSupportedException">Only supported on Windows Vista or newer.</exception>
        public VistaFolderBrowserDialog()
        {
            if (Environment.OSVersion.Version.Major < 6)
            {
                throw new PlatformNotSupportedException("Only supported on Windows Vista or newer.");
            }

            Reset();
        }

        [Browsable(false)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public new event EventHandler HelpRequest
        {
            add
            {
                base.HelpRequest += value;
            }
            remove
            {
                base.HelpRequest -= value;
            }
        }

        /// <summary>
        /// Gets or sets the folder browser dialog box title.
        /// </summary>
        /// <remarks>
        /// The string is placed in the title bar of the dialog box, this can be used to provide instructions to the user.
        /// If the title is an empty string, the system uses a default title.
        /// </remarks>
        /// <value>
        /// The folder browser dialog box title. The default value is an empty string ("").
        /// </value>
        [Category("Appearance")]
        [Description("The string to display in the title bar of the dialog box.")]
        [DefaultValue("")]
        [Localizable(true)]
        public string Title
        {
            get
            {
                return this.title ?? string.Empty;
            }
            set
            {
                this.title = value;
            }
        }

        /// <summary>
        /// Gets or sets the folder that browsing starts from if there is not a recently used folder value available.
        /// </summary>
        /// <value>
        /// The default folder used when there is not a recently used folder value available.
        /// </value>
        [Category("Folder Browsing")]
        [Description("The folder that browsing starts from if there is not a recently used folder value available.")]
        [DefaultValue("")]
        [Localizable(false)]
        public string DefaultFolder
        {
            get
            {
                return this.defaultFolder ?? string.Empty;
            }
            set
            {
                this.defaultFolder = value;
            }
        }

        /// <summary>
        /// Gets or sets the path selected by the user.
        /// </summary>
        /// <value>
        /// The path selected by the user.
        /// </value>
        [Category("Folder Browsing")]
        [Description("The path selected by the user.")]
        [DefaultValue("")]
        [Localizable(false)]
        public string SelectedPath
        {
            get
            {
                return this.selectedPath ?? string.Empty;
            }
            set
            {
                this.selectedPath = value;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the selected folder is added to the recent documents list.
        /// </summary>
        /// <value>
        /// <c>true</c> if the folder is added to the recent documents; otherwise, <c>false</c>.
        /// </value>
        [Category("Behavior")]
        [Description("Indicates whether the selected folder is added to the recent documents list.")]
        [DefaultValue(false)]
        [Localizable(false)]
        public bool AddToRecentDocuments
        {
            get
            {
                return this.addToRecentDocuments;
            }
            set
            {
                this.addToRecentDocuments = value;
            }
        }

        public override void Reset()
        {
            this.title = null;
            this.defaultFolder = null;
            this.selectedPath = null;
            this.addToRecentDocuments = false;
        }

        protected override bool RunDialog(IntPtr hwndOwner)
        {
            if (Application.OleRequired() != ApartmentState.STA)
            {
                throw new ThreadStateException("The calling thread must be STA.");
            }

            bool result = false;

            NativeInterfaces.IFileOpenDialog dialog = null;

            try
            {
                dialog = CreateDialog();

                OnBeforeShow(dialog);

                FolderBrowserDialogEvents dialogEvents = new FolderBrowserDialogEvents(this);
                uint eventCookie;
                dialog.Advise(dialogEvents, out eventCookie);
                try
                {
                    result = dialog.Show(hwndOwner) == NativeConstants.S_OK;
                }
                finally
                {
                    dialog.Unadvise(eventCookie);
                    // Prevent the IFileDialogEvents interface from being collected while the dialog is running.
                    GC.KeepAlive(dialogEvents);
                }
            }
            finally
            {
                if (dialog != null)
                {
                    Marshal.ReleaseComObject(dialog);
                    dialog = null;
                }
            }

            return result;
        }

        private static NativeInterfaces.IFileOpenDialog CreateDialog()
        {
            NativeInterfaces.IFileOpenDialog dialog = new NativeInterfaces.NativeFileOpenDialog();

            // Set a client GUID to allow this dialog to persist its state independently
            // of the standard OpenFileDialog when the AutoUpgradeEnabled property is true.

            Guid folderBrowserGuid = new Guid("8843A8E2-B62E-4BBA-89DF-E42CACD47130");
            dialog.SetClientGuid(ref folderBrowserGuid);

            return dialog;
        }

        private static bool CreateShellItemFromPath(string path, out NativeInterfaces.IShellItem item)
        {
            Guid riid = new Guid(NativeConstants.IID_IShellItem);
            if (UnsafeNativeMethods.SHCreateItemFromParsingName(path, IntPtr.Zero, ref riid, out item) != NativeConstants.S_OK)
            {
                item = null;
                return false;
            }

            return true;
        }

        private void OnBeforeShow(NativeInterfaces.IFileOpenDialog dialog)
        {
            SetDialogOptions(dialog);

            if (!string.IsNullOrEmpty(this.title))
            {
                dialog.SetTitle(this.title);
            }

            if (!string.IsNullOrEmpty(this.defaultFolder))
            {
                NativeInterfaces.IShellItem defaultFolderShellItem = null;

                try
                {
                    if (CreateShellItemFromPath(this.defaultFolder, out defaultFolderShellItem))
                    {
                        dialog.SetDefaultFolder(defaultFolderShellItem);
                    }
                }
                finally
                {
                    if (defaultFolderShellItem != null)
                    {
                        Marshal.ReleaseComObject(defaultFolderShellItem);
                        defaultFolderShellItem = null;
                    }
                }
            }

            if (!string.IsNullOrEmpty(this.selectedPath))
            {
                dialog.SetFileName(this.selectedPath);
            }
        }

        private void SetDialogOptions(NativeInterfaces.IFileOpenDialog dialog)
        {
            NativeEnums.FOS options;
            dialog.GetOptions(out options);

            // The FOS_FORCEFILESYSTEM flag restricts the dialog to selecting folders that are located on the file system.
            // This matches the behavior of the classic folder browser dialog which does not allow virtual folders to be selected.
            options |= NativeEnums.FOS.FOS_PICKFOLDERS | NativeEnums.FOS.FOS_FORCEFILESYSTEM;

            if (!this.addToRecentDocuments)
            {
                options |= NativeEnums.FOS.FOS_DONTADDTORECENT;
            }
            else
            {
                options &= ~NativeEnums.FOS.FOS_DONTADDTORECENT;
            }

            dialog.SetOptions(options);
        }

        private bool HandleFileOk(NativeInterfaces.IFileDialog pfd)
        {
            bool result = false;

            NativeInterfaces.IShellItem resultShellItem = null;
            try
            {
                pfd.GetResult(out resultShellItem);

                string path;
                resultShellItem.GetDisplayName(NativeEnums.SIGDN.SIGDN_FILESYSPATH, out path);

                this.selectedPath = path;
                result = true;
            }
            finally
            {
                if (resultShellItem != null)
                {
                    Marshal.ReleaseComObject(resultShellItem);
                    resultShellItem = null;
                }
            }

            return result;
        }

        private sealed class FolderBrowserDialogEvents : NativeInterfaces.IFileDialogEvents
        {
            private VistaFolderBrowserDialog dialog;

            public FolderBrowserDialogEvents(VistaFolderBrowserDialog folderDialog)
            {
                this.dialog = folderDialog;
            }

            int NativeInterfaces.IFileDialogEvents.OnFileOk(NativeInterfaces.IFileDialog pfd)
            {
                return this.dialog.HandleFileOk(pfd) ? NativeConstants.S_OK : NativeConstants.S_FALSE;
            }

            int NativeInterfaces.IFileDialogEvents.OnFolderChanging(NativeInterfaces.IFileDialog pfd, NativeInterfaces.IShellItem psiFolder)
            {
                return NativeConstants.S_OK;
            }

            void NativeInterfaces.IFileDialogEvents.OnFolderChange(NativeInterfaces.IFileDialog pfd)
            {
            }

            void NativeInterfaces.IFileDialogEvents.OnSelectionChange(NativeInterfaces.IFileDialog pfd)
            {
            }

            void NativeInterfaces.IFileDialogEvents.OnShareViolation(
                NativeInterfaces.IFileDialog pfd,
                NativeInterfaces.IShellItem psi,
                out NativeEnums.FDE_SHAREVIOLATION_RESPONSE pResponse)
            {
                pResponse = NativeEnums.FDE_SHAREVIOLATION_RESPONSE.FDESVR_DEFAULT;
            }

            void NativeInterfaces.IFileDialogEvents.OnTypeChange(NativeInterfaces.IFileDialog pfd)
            {
            }

            void NativeInterfaces.IFileDialogEvents.OnOverwrite(
                NativeInterfaces.IFileDialog pfd,
                NativeInterfaces.IShellItem psi,
                out NativeEnums.FDE_OVERWRITE_RESPONSE pResponse)
            {
                pResponse = NativeEnums.FDE_OVERWRITE_RESPONSE.FDEOR_DEFAULT;
            }
        }
    }
}
