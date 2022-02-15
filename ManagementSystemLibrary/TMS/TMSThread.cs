// <copyright file="TMSThread.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace ManagementSystemLibrary.TMS
{
    using System;
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
    /// Represents a thread of a <see cref="TMSTalk"/>.
    /// </summary>
    public class TMSThread : MSDataObject<TMSTalk>
    {
        private string? description;

        /// <summary>
        /// Initializes a new instance of the <see cref="TMSThread"/> class.
        /// </summary>
        /// <param name="talk">The parent <see cref="TMSTalk"/> of the <see cref="TMSThread"/>.</param>
        /// <param name="id">The identifier of the <see cref="TMSThread"/>.</param>
        public TMSThread(TMSTalk talk, long id)
            : base(talk, id)
        {
        }

        /// <summary>
        /// Gets or sets the description of the <see cref="TMSThread"/>.
        /// </summary>
        public string Description
        {
            get
            {
                _ = this.GetDescriptionAsync();
                return this.description ?? string.Empty;
            }

            set
            {
                _ = this.SaveDescriptionAsync(value);
            }
        }

        /// <summary>
        /// Creates a new <see cref="TMSThread"/>.
        /// </summary>
        /// <param name="parent">The parent of the created <see cref="TMSThread"/>.</param>
        /// <param name="name">The name of the created <see cref="TMSThread"/>.</param>
        /// <param name="description">The description of the created <see cref="TMSThread"/>.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public static async Task<TMSThread?> CreateAsync(TMSTalk parent, string name, string description)
        {
            if (await CreateAsync<TMSThread>(parent, name, Encoding.Unicode.GetBytes(description), null) is long id)
            {
                return new (parent, id);
            }

            return null;
        }

        /// <summary>
        /// Gets the <see cref="Description"/> of the <see cref="TMSThread"/>.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task<object?> GetDescriptionAsync()
        {
            if (this.description is null
                && await this.GetDataAsync().ConfigureAwait(false) is byte[] array)
            {
                this.description = Encoding.Unicode.GetString(array);
                this.OnPropertyChanged(new PropertyChangedEventArgs(nameof(this.Description)));
            }

            return this.description;
        }

        /// <summary>
        /// Loads <see cref="TMSMessage"/> related to the <see cref="TMSThread"/>.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task<IEnumerable<TMSMessage>> LoadMessagesAsync()
        {
            return (await this.LoadLinkedDataAsync<TMSThread, TMSTalk>(typeof(TMSMessage).GetDatabaseAbbreviation()).ConfigureAwait(false)).Select(id => new TMSMessage(this.Parent, id));
        }

        /// <summary>
        /// Saves the <see cref="Description"/> of the <see cref="TMSThread"/>.
        /// </summary>
        /// <param name="value">The new value.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task SaveDescriptionAsync(string? value)
        {
            if (this.description != value
                && !string.IsNullOrEmpty(value))
            {
                this.description = value;
                this.OnPropertyChanged(new PropertyChangedEventArgs(nameof(this.Description)));
                await this.SaveDataAsync(Encoding.Unicode.GetBytes(this.description)).ConfigureAwait(false);
            }
        }
    }
}
