/////////////////////////////////////////////////////////////////////////////////
//
// Change 8bf category
//
// This software is provided under the MIT License:
//   Copyright (C) 2016-2017 Nicholas Hayes
// 
// See LICENSE.txt for complete licensing and attribution information.
//
/////////////////////////////////////////////////////////////////////////////////

using System;
using System.Xml.Serialization;

namespace ChangeFilterCategory
{
    [Serializable]
    [XmlRoot("settings")]
    public sealed class XmlSettings
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="XmlSettings"/> class.
        /// </summary>
        public XmlSettings()
        {
            this.LastPluginDirectory = null;
        }

        /// <summary>
        /// Gets or sets the last plug-in directory.
        /// </summary>
        /// <value>
        /// The last plug-in directory.
        /// </value>
        public string LastPluginDirectory
        {
            get;
            set;
        }
    }
}
