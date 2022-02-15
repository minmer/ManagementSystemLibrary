// <copyright file="MSObjectParameter.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace ManagementSystemLibrary.Database
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    /// <summary>
    /// The parameters of an object in management system.
    /// </summary>
    public struct MSObjectParameter
    {
        /// <summary>
        /// Gets or sets the level of the <see cref="MSObjectParameter"/>.
        /// </summary>
        public int Level { get; set; }

        /// <summary>
        /// Gets or sets the name of the <see cref="MSObjectParameter"/>.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the type of the <see cref="MSObjectParameter"/>.
        /// </summary>
        public string Type { get; set; }

        /// <summary>
        /// Gets or sets the constrains of the <see cref="MSObjectParameter"/>.
        /// </summary>
        public string Constrains { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the <see cref="MSObjectParameter"/> has a get function.
        /// </summary>
        public bool HasGetFunction { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the <see cref="MSObjectParameter"/> has a verification.
        /// </summary>
        public bool HasVerification { get; set; }
    }
}
