// <copyright file="RMSRecord.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace ManagementSystemLibrary.RMS
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
    /// Represents a record of an <see cref="AMSAssociation"/>.
    /// </summary>
    public class RMSRecord : MSDataObject<MSDatabaseObject>
    {
        private object? record;

        /// <summary>
        /// Initializes a new instance of the <see cref="RMSRecord"/> class.
        /// </summary>
        /// <param name="parent">The <see cref="MSDatabaseObject"/> of the <see cref="RMSRecord"/>.</param>
        /// <param name="id">The identifier of the <see cref="RMSRecord"/>.</param>
        public RMSRecord(MSDatabaseObject parent, long id)
            : base(parent, id)
        {
        }

        /// <summary>
        /// Gets or sets the record of the <see cref="RMSRecord"/>.
        /// </summary>
        public object? Record
        {
            get
            {
                _ = this.GetRecordAsync();
                return this.record;
            }

            set
            {
                _ = this.SaveRecordAsync(value);
            }
        }

        /// <summary>
        /// Creates a new <see cref="RMSRecord"/>.
        /// </summary>
        /// <param name="parent">The parent of the created <see cref="RMSRecord"/>.</param>
        /// <param name="name">The name of the created <see cref="RMSRecord"/>.</param>
        /// <param name="record">The record of the created <see cref="RMSRecord"/>.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public static async Task<RMSRecord?> CreateAsync(MSDatabaseObject parent, string name, object record)
        {
            if (await CreateAsync<RMSRecord>(parent, name, record.GetBytes().ToArray(), null) is long id)
            {
                return new (parent, id);
            }

            return null;
        }

        /// <summary>
        /// Gets the data of the <see cref="RMSRecord"/>.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task<object?> GetRecordAsync()
        {
            if (this.record is null
                && await this.GetDataAsync().ConfigureAwait(false) is byte[] array)
            {
                this.record = array.GetObject();
                this.OnPropertyChanged(new PropertyChangedEventArgs(nameof(this.Record)));
            }

            return this.record;
        }

        /// <summary>
        /// Saves the record of the <see cref="RMSRecord"/>.
        /// </summary>
        /// <param name="value">The new value.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task SaveRecordAsync(object? value)
        {
            if (value is not null)
            {
                this.record = value;
                this.OnPropertyChanged(new PropertyChangedEventArgs(nameof(this.Record)));
                await this.SaveDataAsync(this.record.GetBytes().ToArray()).ConfigureAwait(false);
            }
        }
    }
}
