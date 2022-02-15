// <copyright file="SMSUpdate.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace ManagementSystemLibrary.SMS
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
    /// Represents an update of a <see cref="SMSSkill"/>.
    /// </summary>
    public class SMSUpdate : MSTimeObject<SMSUpdate, SMSSkill>
    {
        private double? amount;

        /// <summary>
        /// Initializes a new instance of the <see cref="SMSUpdate"/> class.
        /// </summary>
        /// <param name="skill">The parent <see cref="SMSSkill"/> of the <see cref="SMSUpdate"/>.</param>
        /// <param name="id">The identifier of the <see cref="SMSUpdate"/>.</param>
        public SMSUpdate(SMSSkill skill, long id)
            : base(skill, id)
        {
        }

        /// <summary>
        /// Gets the amount of the <see cref="SMSUpdate"/>.
        /// </summary>
        public double? Amount
        {
            get
            {
                _ = this.GetAmountAsync();
                return this.amount;
            }
        }

        /// <summary>
        /// Creates a new <see cref="SMSUpdate"/>.
        /// </summary>
        /// <param name="parent">The parent of the created <see cref="SMSUpdate"/>.</param>
        /// <param name="name">The name of the created <see cref="SMSUpdate"/>.</param>
        /// <param name="amount">The amount of the created <see cref="SMSUpdate"/>.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public static async Task<SMSUpdate?> CreateAsync(SMSSkill parent, string name, double amount)
        {
            if (await MSTimeObject<SMSUpdate, SMSSkill>.CreateAsync(parent, name, BitConverter.GetBytes(amount), DateTime.Now, null) is long id)
            {
                return new (parent, id);
            }

            return null;
        }

        /// <summary>
        /// Gets the <see cref="Amount"/> of the <see cref="SMSUpdate"/>.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task<double?> GetAmountAsync()
        {
            if (this.amount is null
                && await this.GetDataAsync().ConfigureAwait(false) is byte[] array)
            {
                this.amount = BitConverter.ToDouble(array);
                this.OnPropertyChanged(new PropertyChangedEventArgs(nameof(this.Amount)));
            }

            return this.amount;
        }
    }
}
