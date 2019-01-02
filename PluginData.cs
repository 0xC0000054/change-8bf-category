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
using System.Collections.ObjectModel;

namespace ChangeFilterCategory
{
    internal sealed class PluginData
    {
        private readonly string path;
        private readonly PIPLResourceName resourceName;
        private readonly ushort resourceLanguage;
        private readonly int newCategoryIndex;
        private readonly string title;

        private PIProperty[] properties;
        private string category;
        private bool dirty;

        /// <summary>
        /// Initializes a new instance of the <see cref="PluginData"/> class.
        /// </summary>
        /// <param name="path">The path.</param>
        /// <param name="resourceName">Name of the resource.</param>
        /// <param name="resourceLanguage">The resource language.</param>
        /// <param name="piplProperties">The properties.</param>
        public PluginData(string path, PIPLResourceName resourceName, ushort resourceLanguage, PIProperty[] piplProperties)
        {
            if (path == null)
            {
                throw new ArgumentNullException(nameof(path));
            }

            if (resourceName == null)
            {
                throw new ArgumentNullException(nameof(resourceName));
            }

            if (piplProperties == null)
            {
                throw new ArgumentNullException(nameof(piplProperties));
            }

            this.path = path;
            this.resourceName = resourceName;
            this.resourceLanguage = resourceLanguage;
            // Add one to the existing property count to allow the new category to
            // be appended to the existing properties.
            properties = new PIProperty[piplProperties.Length + 1];
            piplProperties.CopyTo(properties, 0);
            newCategoryIndex = properties.Length - 1;

            for (int i = 0; i < piplProperties.Length; i++)
            {
                PIProperty prop = piplProperties[i];
                switch (prop.Key)
                {
                    case PIPropertyID.PICategoryProperty:
                        category = PascalStringHelpers.ConvertToString(prop.GetPropertyDataReadOnly());
                        break;
                    case PIPropertyID.PINameProperty:
                        title = PascalStringHelpers.ConvertToString(prop.GetPropertyDataReadOnly());
                        break;
                }
            }
            dirty = false;
        }

        /// <summary>
        /// Gets the path of the plug-in.
        /// </summary>
        /// <value>
        /// The path of the plug-in.
        /// </value>
        public string Path
        {
            get
            {
                return path;
            }
        }

        /// <summary>
        /// Gets the resource name of the plug-in.
        /// </summary>
        /// <value>
        /// The resource name of the plug-in.
        /// </value>
        public PIPLResourceName ResourceName
        {
            get
            {
                return resourceName;
            }
        }

        /// <summary>
        /// Gets the resource language of the plug-in.
        /// </summary>
        /// <value>
        /// The resource language of the plug-in.
        /// </value>
        public ushort ResourceLanguage
        {
            get
            {
                return resourceLanguage;
            }
        }

        /// <summary>
        /// Gets the properties of the plug-in.
        /// </summary>
        /// <value>
        /// The properties of the plug-in.
        /// </value>
        public ReadOnlyCollection<PIProperty> Properties
        {
            get
            {
                return new ReadOnlyCollection<PIProperty>(properties);
            }
        }

        /// <summary>
        /// Gets or sets the category of the plug-in.
        /// </summary>
        /// <value>
        /// The category of the plug-in.
        /// </value>
        /// <exception cref="ArgumentNullException"><paramref name="value"/> is null.</exception>
        /// <exception cref="ArgumentException"><paramref name="value"/> exceeds the maximum length for a filter category.</exception>
        public string Category
        {
            get
            {
                return category;
            }
            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException(nameof(value));
                }

                if (!category.Equals(value, StringComparison.Ordinal))
                {
                    if (IsCategoryNameTooLong(value))
                    {
                        throw new ArgumentException("The specified value exceeds the maximum length for a filter category.", nameof(value));
                    }

                    category = value;
                    properties[newCategoryIndex] = new PIProperty(PIPropertyID.PICategoryProperty, PascalStringHelpers.CreateFromString(value));
                    dirty = true;
                }
            }
        }

        /// <summary>
        /// Gets the title of the plug-in.
        /// </summary>
        /// <value>
        /// The title of the plug-in.
        /// </value>
        public string Title
        {
            get
            {
                return title;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether this instance has unsaved changes.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance has unsaved changes; otherwise, <c>false</c>.
        /// </value>
        public bool Dirty
        {
            get
            {
                return dirty;
            }
            set
            {
                dirty = value;
            }
        }

        /// <summary>
        /// Determines whether the specified value is too long for a filter category name.
        /// </summary>
        /// <param name="value">The value to check.</param>
        /// <returns><c>true</c> if the specified value too long for a filter category name; otherwise, <c>false</c>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="value"/> is null.</exception>
        internal static bool IsCategoryNameTooLong(string value)
        {
            return !PascalStringHelpers.IsLengthValid(value);
        }
    }
}
