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

using ChangeFilterCategory.Properties;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Windows.Forms;
using System.Xml.Serialization;

namespace ChangeFilterCategory
{
    internal partial class Form1 : Form
    {
        private TreeNode contextMenuTreeNode;
        private XmlSettings xmlSettings;

        private static readonly string SettingsPath = Path.Combine(Application.StartupPath, "ChangeFilterCategory.xml");

        public Form1()
        {
            InitializeComponent();
            this.fileNameLabel.Text = string.Empty;
            this.xmlSettings = new XmlSettings();
        }

        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);

            LoadSettings();
        }

        private void ShowErrorMessage(string message)
        {
            MessageBox.Show(this, message, this.Text, MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1, 0);
        }

        private void LoadSettings()
        {
            try
            {
                using (FileStream stream = new FileStream(SettingsPath, FileMode.Open, FileAccess.Read))
                {
                    XmlSerializer serializer = new XmlSerializer(typeof(XmlSettings));
                    this.xmlSettings = (XmlSettings)serializer.Deserialize(stream);
                }

                this.folderBrowserDialog.SelectedPath = this.xmlSettings.LastPluginDirectory;
            }
            catch (IOException ex)
            {
                ShowErrorMessage(ex.Message);
            }
            catch (UnauthorizedAccessException ex)
            {
                ShowErrorMessage(ex.Message);
            }
        }

        private void SaveSettings()
        {
            try
            {
                using (FileStream fs = new FileStream(SettingsPath, FileMode.Create, FileAccess.Write, FileShare.None))
                {
                    XmlSerializer serializer = new XmlSerializer(typeof(XmlSettings));
                    serializer.Serialize(fs, this.xmlSettings);
                }
            }
            catch (IOException ex)
            {
                ShowErrorMessage(ex.Message);
            }
            catch (UnauthorizedAccessException ex)
            {
                ShowErrorMessage(ex.Message);
            }
        }

        private void loadFilterButton_Click(object sender, EventArgs e)
        {
            if (this.folderBrowserDialog.ShowDialog(this) == DialogResult.OK)
            {
                string selectedPath = this.folderBrowserDialog.SelectedPath;

                this.xmlSettings.LastPluginDirectory = selectedPath;
                SaveSettings();

                this.filterTreeView.Nodes.Clear();
                this.UseWaitCursor = true;
                this.filterSearchWorker.RunWorkerAsync(selectedPath);
            }
        }

        private void filterSearchWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            Dictionary<string, TreeNode> nodes = new Dictionary<string, TreeNode>(StringComparer.Ordinal);

            using (FileEnumerator enumerator = new FileEnumerator((string)e.Argument, ".8bf", SearchOption.AllDirectories, true))
            {
                while (enumerator.MoveNext())
                {
                    foreach (var plugin in PIPLReader.LoadPluginsFromFile(enumerator.Current))
                    {
                        // The **Hidden** category is used for filters that are not directly invoked by the user.
                        if (!plugin.Category.Equals("**Hidden**", StringComparison.Ordinal))
                        {
                            TreeNode child = new TreeNode(plugin.Title)
                            {
                                ContextMenuStrip = this.filterItemContextMenu,
                                Name = plugin.Title,
                                Tag = plugin
                            };

                            if (nodes.ContainsKey(plugin.Category))
                            {
                                TreeNode parent = nodes[plugin.Category];
                                parent.Nodes.Add(child);
                            }
                            else
                            {
                                TreeNode node = new TreeNode(plugin.Category, new TreeNode[] { child })
                                {
                                    ContextMenuStrip = this.filterCategoryContextMenu,
                                    Name = plugin.Category,
                                };

                                nodes.Add(plugin.Category, node);
                            }
                        }
                    }
                }
            }

            TreeNode[] items = new TreeNode[nodes.Values.Count];
            nodes.Values.CopyTo(items, 0);

            e.Result = items;
        }

        private void filterSearchWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Error != null)
            {
                ShowErrorMessage(e.Error.Message);
            }
            else
            {
                this.filterTreeView.BeginUpdate();

                this.filterTreeView.TreeViewNodeSorter = null;
                this.filterTreeView.Nodes.AddRange((TreeNode[])e.Result);
                this.filterTreeView.TreeViewNodeSorter = new TreeNodeItemComparer();

                // Add and remove a dummy item to force the TreeView to update the item size calculations.
                // This fixes a bug with the TreeView clipping the text of the last root node.
                int dummyIndex = this.filterTreeView.Nodes.Add(new TreeNode());
                this.filterTreeView.Nodes.RemoveAt(dummyIndex);

                this.filterTreeView.EndUpdate();
            }
            this.UseWaitCursor = false;
        }

        private void filterTreeView_AfterSelect(object sender, TreeViewEventArgs e)
        {
            if (e.Node.Tag != null)
            {
                this.fileNameLabel.Text = Path.GetFileName(((PluginData)e.Node.Tag).Path);
            }
            else
            {
                this.fileNameLabel.Text = string.Empty;
            }
        }

        private void filterTreeView_NodeMouseClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            this.editCategoryButton.Enabled = true;
            this.contextMenuTreeNode = e.Node;
        }

        private void renameCategoryMenuItem_Click(object sender, EventArgs e)
        {
            if (this.contextMenuTreeNode != null)
            {
                RenameCategory(this.contextMenuTreeNode);
            }
        }

        private void changeCategoryMenuItem_Click(object sender, EventArgs e)
        {
            if (this.contextMenuTreeNode != null)
            {
                ChangeItemCategory(this.contextMenuTreeNode);
            }
        }

        private void editCategoryButton_Click(object sender, EventArgs e)
        {
            if (this.filterTreeView.SelectedNode != null)
            {
                if (this.filterTreeView.SelectedNode.Tag != null)
                {
                    ChangeItemCategory(this.filterTreeView.SelectedNode);
                }
                else
                {
                    RenameCategory(this.filterTreeView.SelectedNode);
                }
            }
        }

        private void RenameCategory(TreeNode selectedNode)
        {
            using (EditCategoryForm form = new EditCategoryForm(Resources.RenameFilterCategoryTitle, selectedNode.Text))
            {
                if (form.ShowDialog(this) == DialogResult.OK)
                {
                    string newCategory = form.CategoryText;

                    if (!newCategory.Equals(selectedNode.Text, StringComparison.Ordinal))
                    {
                        foreach (TreeNode item in selectedNode.Nodes)
                        {
                            SaveNewFilterCategory((PluginData)item.Tag, newCategory);
                        }

                        int existingCategoryIndex = this.filterTreeView.Nodes.IndexOfKey(newCategory);
                        if (existingCategoryIndex != -1)
                        {
                            this.filterTreeView.BeginUpdate();
                            TreeNode existing = this.filterTreeView.Nodes[existingCategoryIndex];

                            this.filterTreeView.Nodes.Remove(selectedNode);

                            // Store the node count in a local variable to prevent an infinite loop
                            // when the name of the existing node matches the selected node.
                            int nodeCount = selectedNode.Nodes.Count;
                            for (int i = 0; i < nodeCount; i++)
                            {
                                existing.Nodes.Add(selectedNode.Nodes[i]);
                            }

                            this.filterTreeView.EndUpdate();
                        }
                        else
                        {
                            this.filterTreeView.BeginUpdate();
                            this.filterTreeView.Nodes.Remove(selectedNode);

                            TreeNode node = new TreeNode(newCategory)
                            {
                                ContextMenuStrip = this.filterCategoryContextMenu,
                                Name = newCategory
                            };

                            for (int i = 0; i < selectedNode.Nodes.Count; i++)
                            {
                                node.Nodes.Add(selectedNode.Nodes[i]);
                            }

                            this.filterTreeView.Nodes.Add(node);
                            this.filterTreeView.EndUpdate();
                        }
                    }
                }
            }
        }

        private void ChangeItemCategory(TreeNode selectedNode)
        {
            PluginData data = (PluginData)selectedNode.Tag;
            using (EditCategoryForm form = new EditCategoryForm(Resources.ChangeItemCategoryTitle, data.Category, selectedNode.Text))
            {
                if (form.ShowDialog(this) == DialogResult.OK)
                {
                    string newCategory = form.CategoryText;

                    if (!newCategory.Equals(data.Category, StringComparison.Ordinal))
                    {
                        SaveNewFilterCategory(data, newCategory);

                        this.filterTreeView.BeginUpdate();

                        TreeNode parent = selectedNode.Parent;
                        parent.Nodes.Remove(selectedNode);
                        if (parent.Nodes.Count == 0)
                        {
                            this.filterTreeView.Nodes.Remove(parent);
                        }

                        if (this.filterTreeView.Nodes.ContainsKey(newCategory))
                        {
                            this.filterTreeView.Nodes[newCategory].Nodes.Add(selectedNode);
                        }
                        else
                        {
                            TreeNode node = new TreeNode(newCategory, new TreeNode[] { selectedNode })
                            {
                                ContextMenuStrip = this.filterCategoryContextMenu,
                                Name = newCategory,
                            };

                            this.filterTreeView.Nodes.Add(node);
                        }

                        this.filterTreeView.EndUpdate();
                    }
                }
            }
        }

        private void SaveNewFilterCategory(PluginData filter, string newCategory)
        {
            if (filter == null)
            {
                throw new ArgumentNullException(nameof(filter));
            }

            try
            {
                filter.Category = newCategory;

                PIPLWriter.UpdatePIPLResource(filter);
            }
            catch (ArgumentException ex)
            {
                ShowErrorMessage(ex.Message);
            }
            catch (Win32Exception ex)
            {
                ShowErrorMessage(ex.Message);
            }
        }
    }
}
