// <copyright file="SMSOutputEventArgs.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace ManagementSystemLibrary.SMS
{
    /// <summary>
    /// Represents an <see cref="EventArgs"/> for the output event of the <see cref="SMSCondition"/>.
    /// </summary>
    public class SMSOutputEventArgs : EventArgs
    {
        /// <summary>
        /// Gets or sets the index of the <see cref="SMSOutputEventArgs"/>.
        /// </summary>
        public int Index { get; set; }

        /// <summary>
        /// Gets or sets the value of the <see cref="SMSOutputEventArgs"/>.
        /// </summary>
        public object? Value { get; set; }
    }
}
