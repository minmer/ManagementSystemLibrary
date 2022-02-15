// <copyright file="TMSReadReceipt.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace ManagementSystemLibrary.TMS
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
    /// Represents a read receipt of a <see cref="TMSMessage"/>.
    /// </summary>
    public class TMSReadReceipt : MSDataObject<TMSMessage>
    {
        private AMSAccount? account;

        /// <summary>
        /// Initializes a new instance of the <see cref="TMSReadReceipt"/> class.
        /// </summary>
        /// <param name="message">The parent <see cref="TMSMessage"/> of the <see cref="TMSReadReceipt"/>.</param>
        /// <param name="id">The identifier of the <see cref="TMSReadReceipt"/>.</param>
        public TMSReadReceipt(TMSMessage message, long id)
            : base(message, id)
        {
        }

        /// <summary>
        /// Gets the account of the <see cref="TMSReadReceipt"/>.
        /// </summary>
        public AMSAccount? Account
        {
            get
            {
                _ = this.GetAccountAsync();
                return this.account;
            }
        }

        /// <summary>
        /// Creates a new <see cref="TMSReadReceipt"/>.
        /// </summary>
        /// <param name="parent">The parent of the created <see cref="TMSReadReceipt"/>.</param>
        /// <param name="name">The name of the created <see cref="TMSReadReceipt"/>.</param>
        /// <param name="account">The account of the created <see cref="TMSReadReceipt"/>.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public static async Task<TMSReadReceipt?> CreateAsync(TMSMessage parent, string name, AMSAccount account)
        {
            if (await CreateAsync<TMSReadReceipt>(parent, name, BitConverter.GetBytes(account.ID), null) is long id)
            {
                return new (parent, id);
            }

            return null;
        }

        /// <summary>
        /// Gets the <see cref="Account"/> of the <see cref="TMSReadReceipt"/>.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task<object?> GetAccountAsync()
        {
            if (this.account is null
                && await this.GetDataAsync().ConfigureAwait(false) is byte[] array)
            {
                this.account = new AMSAccount(this.Parent.Parent.Association, BitConverter.ToInt64(array));
                this.OnPropertyChanged(new PropertyChangedEventArgs(nameof(this.Account)));
            }

            return this.account;
        }
    }
}
