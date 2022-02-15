// <copyright file="PMSAppointment.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace ManagementSystemLibrary.PMS
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Linq;
    using System.Security.Cryptography;
    using System.Text;
    using System.Threading.Tasks;
    using ManagementSystemLibrary.AMS;
    using ManagementSystemLibrary.ManagementSystem;
    using ManagementSystemLibrary.Pipeline;
    using Npgsql;
    using NpgsqlTypes;

    /// <summary>
    /// Represents an appointment of a <see cref="PMSPlanner"/>.
    /// </summary>
    public class PMSAppointment : MSRangeObject<PMSAppointment, PMSPlanner>
    {
        private string? title;

        /// <summary>
        /// Initializes a new instance of the <see cref="PMSAppointment"/> class.
        /// </summary>
        /// <param name="talk">The parent <see cref="PMSPlanner"/> of the <see cref="PMSAppointment"/>.</param>
        /// <param name="id">The identifier of the <see cref="PMSAppointment"/>.</param>
        public PMSAppointment(PMSPlanner talk, long id)
            : base(talk, id)
        {
        }

        /// <summary>
        /// Gets or sets the title of the <see cref="PMSAppointment"/>.
        /// </summary>
        public string Title
        {
            get
            {
                _ = this.GetMessageAsync();
                return this.title ?? string.Empty;
            }

            set
            {
                _ = this.SaveMessageAsync(value);
            }
        }

        /// <summary>
        /// Creates a new <see cref="PMSAppointment"/>.
        /// </summary>
        /// <param name="parent">The parent of the created <see cref="PMSAppointment"/>.</param>
        /// <param name="name">The name of the created <see cref="PMSAppointment"/>.</param>
        /// <param name="message">The message of the created <see cref="PMSAppointment"/>.</param>
        /// <param name="time">The time of the created <see cref="PMSAppointment"/>.</param>
        /// <param name="endTime">The end time of the created <see cref="PMSAppointment"/>.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public static async Task<PMSAppointment?> CreateAsync(PMSPlanner parent, string name, string message, DateTime time, DateTime endTime)
        {
            if (await MSRangeObject<PMSAppointment, PMSPlanner>.CreateAsync(parent, name, Encoding.Unicode.GetBytes(message), time, endTime, null) is long id)
            {
                return new (parent, id);
            }

            return null;
        }

        /// <summary>
        /// Gets the <see cref="Title"/> of the <see cref="PMSAppointment"/>.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task<object?> GetMessageAsync()
        {
            if (this.title is null
                && await this.GetDataAsync().ConfigureAwait(false) is byte[] array)
            {
                this.title = Encoding.Unicode.GetString(array);
                this.OnPropertyChanged(new PropertyChangedEventArgs(nameof(this.Title)));
            }

            return this.title;
        }

        /// <summary>
        /// Saves the <see cref="Title"/> of the <see cref="PMSAppointment"/>.
        /// </summary>
        /// <param name="value">The new value.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task SaveMessageAsync(string? value)
        {
            if (this.title != value
                && !string.IsNullOrEmpty(value))
            {
                this.title = value;
                this.OnPropertyChanged(new PropertyChangedEventArgs(nameof(this.Title)));
                await this.SaveDataAsync(Encoding.Unicode.GetBytes(this.title)).ConfigureAwait(false);
            }
        }
    }
}
