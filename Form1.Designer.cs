namespace ChangeFilterCategory
{
    partial class Form1
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.folderBrowserDialog = new ChangeFilterCategory.PlatformFolderBrowserDialog();
            this.filterTreeView = new System.Windows.Forms.TreeView();
            this.filterSearchWorker = new System.ComponentModel.BackgroundWorker();
            this.filterCategoryContextMenu = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.renameCategoryMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.filterItemContextMenu = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.changeCategoryMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.editCategoryButton = new System.Windows.Forms.Button();
            this.loadFilterButton = new System.Windows.Forms.Button();
            this.fileNameLabel = new System.Windows.Forms.Label();
            this.filterCategoryContextMenu.SuspendLayout();
            this.filterItemContextMenu.SuspendLayout();
            this.SuspendLayout();
            //
            // folderBrowserDialog
            //
            this.folderBrowserDialog.ClassicFolderBrowserDescription = "Select a folder containing Adobe® Photoshop®-compatible filters.";
            this.folderBrowserDialog.VistaFolderBrowserTitle = "Select a Folder Containing Adobe® Photoshop®-Compatible Filters";
            //
            // filterTreeView
            //
            this.filterTreeView.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
            | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.filterTreeView.Location = new System.Drawing.Point(12, 12);
            this.filterTreeView.Name = "filterTreeView";
            this.filterTreeView.Size = new System.Drawing.Size(304, 363);
            this.filterTreeView.TabIndex = 2;
            this.filterTreeView.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.filterTreeView_AfterSelect);
            this.filterTreeView.NodeMouseClick += new System.Windows.Forms.TreeNodeMouseClickEventHandler(this.filterTreeView_NodeMouseClick);
            //
            // filterSearchWorker
            //
            this.filterSearchWorker.DoWork += new System.ComponentModel.DoWorkEventHandler(this.filterSearchWorker_DoWork);
            this.filterSearchWorker.RunWorkerCompleted += new System.ComponentModel.RunWorkerCompletedEventHandler(this.filterSearchWorker_RunWorkerCompleted);
            //
            // filterCategoryContextMenu
            //
            this.filterCategoryContextMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.renameCategoryMenuItem});
            this.filterCategoryContextMenu.Name = "contextMenuStrip1";
            this.filterCategoryContextMenu.Size = new System.Drawing.Size(178, 26);
            //
            // renameCategoryMenuItem
            //
            this.renameCategoryMenuItem.Name = "renameCategoryMenuItem";
            this.renameCategoryMenuItem.Size = new System.Drawing.Size(177, 22);
            this.renameCategoryMenuItem.Text = "Rename Category...";
            this.renameCategoryMenuItem.Click += new System.EventHandler(this.renameCategoryMenuItem_Click);
            //
            // filterItemContextMenu
            //
            this.filterItemContextMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.changeCategoryMenuItem});
            this.filterItemContextMenu.Name = "filterItemContextMenu";
            this.filterItemContextMenu.Size = new System.Drawing.Size(176, 26);
            //
            // changeCategoryMenuItem
            //
            this.changeCategoryMenuItem.Name = "changeCategoryMenuItem";
            this.changeCategoryMenuItem.Size = new System.Drawing.Size(175, 22);
            this.changeCategoryMenuItem.Text = "Change Category...";
            this.changeCategoryMenuItem.Click += new System.EventHandler(this.changeCategoryMenuItem_Click);
            //
            // editCategoryButton
            //
            this.editCategoryButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.editCategoryButton.Enabled = false;
            this.editCategoryButton.Location = new System.Drawing.Point(93, 394);
            this.editCategoryButton.Name = "editCategoryButton";
            this.editCategoryButton.Size = new System.Drawing.Size(96, 23);
            this.editCategoryButton.TabIndex = 4;
            this.editCategoryButton.Text = "Edit category...";
            this.editCategoryButton.UseVisualStyleBackColor = true;
            this.editCategoryButton.Click += new System.EventHandler(this.editCategoryButton_Click);
            //
            // loadFilterButton
            //
            this.loadFilterButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.loadFilterButton.Location = new System.Drawing.Point(12, 394);
            this.loadFilterButton.Name = "loadFilterButton";
            this.loadFilterButton.Size = new System.Drawing.Size(75, 23);
            this.loadFilterButton.TabIndex = 6;
            this.loadFilterButton.Text = "Load filters...";
            this.loadFilterButton.UseVisualStyleBackColor = true;
            this.loadFilterButton.Click += new System.EventHandler(this.loadFilterButton_Click);
            //
            // fileNameLabel
            //
            this.fileNameLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.fileNameLabel.AutoSize = true;
            this.fileNameLabel.Location = new System.Drawing.Point(13, 378);
            this.fileNameLabel.Name = "fileNameLabel";
            this.fileNameLabel.Size = new System.Drawing.Size(74, 13);
            this.fileNameLabel.TabIndex = 3;
            this.fileNameLabel.Text = "fileNameLabel";
            //
            // Form1
            //
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.ClientSize = new System.Drawing.Size(328, 429);
            this.Controls.Add(this.loadFilterButton);
            this.Controls.Add(this.editCategoryButton);
            this.Controls.Add(this.fileNameLabel);
            this.Controls.Add(this.filterTreeView);
            this.Name = "Form1";
            this.Text = "Change filter category";
            this.filterCategoryContextMenu.ResumeLayout(false);
            this.filterItemContextMenu.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private ChangeFilterCategory.PlatformFolderBrowserDialog folderBrowserDialog;
        private System.Windows.Forms.TreeView filterTreeView;
        private System.ComponentModel.BackgroundWorker filterSearchWorker;
        private System.Windows.Forms.ContextMenuStrip filterCategoryContextMenu;
        private System.Windows.Forms.ToolStripMenuItem renameCategoryMenuItem;
        private System.Windows.Forms.ContextMenuStrip filterItemContextMenu;
        private System.Windows.Forms.ToolStripMenuItem changeCategoryMenuItem;
        private System.Windows.Forms.Button editCategoryButton;
        private System.Windows.Forms.Button loadFilterButton;
        private System.Windows.Forms.Label fileNameLabel;
    }
}