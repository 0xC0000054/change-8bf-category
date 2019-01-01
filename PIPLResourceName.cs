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

namespace ChangeFilterCategory
{
    internal sealed class PIPLResourceName
    {
        private readonly int ordinal;
        private readonly string name;

        /// <summary>
        /// Initializes a new instance of the <see cref="PIPLResourceName"/> class.
        /// </summary>
        /// <param name="ordinal">The resource ordinal.</param>
        public PIPLResourceName(int ordinal)
        {
            this.ordinal = ordinal;
            this.name = null;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PIPLResourceName"/> class.
        /// </summary>
        /// <param name="name">The resource name.</param>
        public PIPLResourceName(string name)
        {
            this.ordinal = 0;
            this.name = name;
        }

        /// <summary>
        /// Gets a value indicating whether the resource name is a string.
        /// </summary>
        /// <value>
        ///   <c>true</c> if the resource name is a string; otherwise, <c>false</c>.
        /// </value>
        public bool IsString
        {
            get
            {
                return this.name != null;
            }
        }

        /// <summary>
        /// Gets the resource name.
        /// </summary>
        /// <value>
        /// The resource name.
        /// </value>
        public string Name
        {
            get
            {
                return this.name;
            }
        }

        /// <summary>
        /// Gets the resource ordinal.
        /// </summary>
        /// <value>
        /// The resource ordinal.
        /// </value>
        public int Ordinal
        {
            get
            {
                return this.ordinal;
            }
        }
    }
}
