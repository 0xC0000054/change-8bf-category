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
            fileNameLabel.Text = string.Empty;
            xmlSettings = new XmlSettings();
        }

        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);

            LoadSettings();
        }

        private void ShowErrorMessage(string message)
        {
            MessageBox.Show(this, message, Text, MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1, 0);
        }

        private void LoadSettings()
        {
            try
            {
                using (FileStream stream = new FileStream(SettingsPath, FileMode.Open, FileAccess.Read))
                {
                    XmlSerializer serializer = new XmlSerializer(typeof(XmlSettings));
                    xmlSettings = (XmlSettings)serializer.Deserialize(stream);
                }

                folderBrowserDialog.SelectedPath = xmlSettings.LastPluginDirectory;
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
                    serializer.Serialize(fs, xmlSettings);
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
            if (folderBrowserDialog.ShowDialog(this) == DialogResult.OK)
            {
                string selectedPath = folderBrowserDialog.SelectedPath;

                xmlSettings.LastPluginDirectory = selectedPath;
                SaveSettings();

                filterTreeView.Nodes.Clear();
                UseWaitCursor = true;
                filterSearchWorker.RunWorkerAsync(selectedPath);
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
                                ContextMenuStrip = filterItemContextMenu,
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
                                    ContextMenuStrip = filterCategoryContextMenu,
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
                filterTreeView.BeginUpdate();

                filterTreeView.TreeViewNodeSorter = null;
                filterTreeView.Nodes.AddRange((TreeNode[])e.Result);
                filterTreeView.TreeViewNodeSorter = new TreeNodeItemComparer();

                // Add and remove a dummy item to force the TreeView to update the item size calculations.
                // This fixes a bug with the TreeView clipping the text of the last root node.
                int dummyIndex = filterTreeView.Nodes.Add(new TreeNode());
                filterTreeView.Nodes.RemoveAt(dummyIndex);

                filterTreeView.EndUpdate();
            }
            UseWaitCursor = false;
        }

        private void filterTreeView_AfterSelect(object sender, TreeViewEventArgs e)
        {
            if (e.Node.Tag != null)
            {
                fileNameLabel.Text = Path.GetFileName(((PluginData)e.Node.Tag).Path);
            }
            else
            {
                fileNameLabel.Text = string.Empty;
            }
        }

        private void filterTreeView_NodeMouseClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            editCategoryButton.Enabled = true;
            contextMenuTreeNode = e.Node;
        }

        private void renameCategoryMenuItem_Click(object sender, EventArgs e)
        {
            if (contextMenuTreeNode != null)
            {
                RenameCategory(contextMenuTreeNode);
            }
        }

        private void changeCategoryMenuItem_Click(object sender, EventArgs e)
        {
            if (contextMenuTreeNode != null)
            {
                ChangeItemCategory(contextMenuTreeNode);
            }
        }

        private void editCategoryButton_Click(object sender, EventArgs e)
        {
            if (filterTreeView.SelectedNode != null)
            {
                if (filterTreeView.SelectedNode.Tag != null)
                {
                    ChangeItemCategory(filterTreeView.SelectedNode);
                }
                else
                {
                    RenameCategory(filterTreeView.SelectedNode);
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

                        int existingCategoryIndex = filterTreeView.Nodes.IndexOfKey(newCategory);
                        if (existingCategoryIndex != -1)
                        {
                            filterTreeView.BeginUpdate();
                            TreeNode existing = filterTreeView.Nodes[existingCategoryIndex];

                            filterTreeView.Nodes.Remove(selectedNode);

                            // Store the node count in a local variable to prevent an infinite loop
                            // when the name of the existing node matches the selected node.
                            int nodeCount = selectedNode.Nodes.Count;
                            for (int i = 0; i < nodeCount; i++)
                            {
                                existing.Nodes.Add(selectedNode.Nodes[i]);
                            }

                            filterTreeView.EndUpdate();
                        }
                        else
                        {
                            filterTreeView.BeginUpdate();
                            filterTreeView.Nodes.Remove(selectedNode);

                            TreeNode node = new TreeNode(newCategory)
                            {
                                ContextMenuStrip = filterCategoryContextMenu,
                                Name = newCategory
                            };

                            for (int i = 0; i < selectedNode.Nodes.Count; i++)
                            {
                                node.Nodes.Add(selectedNode.Nodes[i]);
                            }

                            filterTreeView.Nodes.Add(node);
                            filterTreeView.EndUpdate();
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

                        filterTreeView.BeginUpdate();

                        TreeNode parent = selectedNode.Parent;
                        parent.Nodes.Remove(selectedNode);
                        if (parent.Nodes.Count == 0)
                        {
                            filterTreeView.Nodes.Remove(parent);
                        }

                        if (filterTreeView.Nodes.ContainsKey(newCategory))
                        {
                            filterTreeView.Nodes[newCategory].Nodes.Add(selectedNode);
                        }
                        else
                        {
                            TreeNode node = new TreeNode(newCategory, new TreeNode[] { selectedNode })
                            {
                                ContextMenuStrip = filterCategoryContextMenu,
                                Name = newCategory,
                            };

                            filterTreeView.Nodes.Add(node);
                        }

                        filterTreeView.EndUpdate();
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
