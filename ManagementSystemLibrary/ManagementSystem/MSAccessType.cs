// <copyright file="MSAccessType.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace ManagementSystemLibrary.ManagementSystem
{
    /// <summary>
    /// Specifies the type of access.
    /// </summary>
    public enum MSAccessType
    {
        /// <summary>
        /// A creator access with full rights.
        /// </summary>
        Creator = 0,

        /// <summary>
        /// A administrator access with full rights.
        /// </summary>
        Administrator = 1,

        /// <summary>
        /// A contributor access with rights to read and write.
        /// </summary>
        Contributor = 2,

        /// <summary>
        /// A observator access with rights to read.
        /// </summary>
        Observator = 3,

        /// <summary>
        /// A public access with limited rights.
        /// </summary>
        Public = 4,
    }
}
