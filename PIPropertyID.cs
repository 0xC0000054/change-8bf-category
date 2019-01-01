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

/* Adapted from PIProperties.h
 * Copyright (c) 1992-1998, Adobe Systems Incorporated.
 * All rights reserved.
*/

namespace ChangeFilterCategory
{
    internal static class PIPropertyID
    {
        /// <summary>
        /// The property giving the plug-in's kind; 8BFM for filters - 'kind'
        /// </summary>
        internal const uint PIKindProperty = 0x6b696e64U;
        /// <summary>
        /// Win32 Intel code descriptor; Entrypoint - 'wx86'
        /// </summary>
        internal const uint PIWin32X86CodeProperty = 0x77783836U;
        /// <summary>
        /// Major(int16).Minor(int16) version number - 'vers'
        /// </summary>
        internal const uint PIVersionProperty = 0x76657273U;
        /// <summary>
        /// Image modes supported flags - 'mode'
        /// </summary>
        internal const uint PIImageModesProperty = 0x6d6f6465U;
        /// <summary>
        /// Category name that appears on top level menu - 'catg'
        /// </summary>
        internal const uint PICategoryProperty = 0x63617467U;
        /// <summary>
        /// Menu name - 'name'
        /// </summary>
        internal const uint PINameProperty = 0x6e616d65U;
        /// <summary>
        /// Has Terminology Property - 'hstm'
        /// </summary>
        internal const uint PIHasTerminologyProperty = 0x6873746dU;
        /// <summary>
        /// FilterCaseInfo Property - 'fici'
        /// </summary>
        internal const uint PIFilterCaseInfoProperty = 0x66696369U;
        /// <summary>
        /// EnableInfo property - 'enbl'
        /// </summary>
        internal const uint EnableInfo = 0x656e626cU;
        /// <summary>
        /// Creator code of required host, such as '8BIM' for Adobe Photoshop. - 'host'
        /// </summary>
        internal const uint PIRequiredHostProperty = 0x686f7374U;
    }
}