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

using ChangeFilterCategory.Properties;
using System;
using System.Globalization;
using System.Windows.Forms;

namespace ChangeFilterCategory
{
    internal partial class EditCategoryForm : Form
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="EditCategoryForm"/> class.
        /// </summary>
        /// <param name="caption">The caption of the form.</param>
        /// <param name="category">The category of the filter.</param>
        public EditCategoryForm(string caption, string category) : this(caption, category, null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="EditCategoryForm"/> class.
        /// </summary>
        /// <param name="caption">The caption of the form.</param>
        /// <param name="category">The category of the filter.</param>
        /// <param name="filterTitle">The title of the filter.</param>
        public EditCategoryForm(string caption, string category, string filterTitle)
        {
            InitializeComponent();
            Text = caption;
            categoryTextBox.Text = category;
            if (!string.IsNullOrEmpty(filterTitle))
            {
                descriptionLabel.Text = string.Format(CultureInfo.CurrentCulture, Resources.ChangeItemCategoryLabelFormat, filterTitle.TrimEnd('.'));
            }
            else
            {
                descriptionLabel.Text = Resources.RenameFilterCategoryLabelText;
            }
        }

        /// <summary>
        /// Gets the filter category text.
        /// </summary>
        /// <value>
        /// The category text.
        /// </value>
        public string CategoryText
        {
            get
            {
                return categoryTextBox.Text;
            }
        }

        private void okButton_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.OK;
            Close();
        }

        private void cancelButton_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }

        private void categoryTextBox_Validating(object sender, System.ComponentModel.CancelEventArgs e)
        {
            string text = categoryTextBox.Text;

            if (StringUtil.IsNullOrWhiteSpace(text))
            {
                errorProvider1.SetError(categoryTextBox, Resources.EditCategoryNullOrWhiteSpaceError);
                e.Cancel = true;
            }
            else if (PluginData.IsCategoryNameTooLong(text))
            {
                errorProvider1.SetError(categoryTextBox, Resources.EditCategoryNameTooLongError);
                e.Cancel = true;
            }
        }

        private void categoryTextBox_Validated(object sender, EventArgs e)
        {
            // If all conditions have been met, clear the ErrorProvider of errors.
            errorProvider1.SetError(categoryTextBox, string.Empty);
        }
    }
}
