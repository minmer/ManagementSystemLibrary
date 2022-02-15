// <copyright file="TMSMessage.cs" company="PlaceholderCompany">
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
    /// Represents a message of a <see cref="TMSTalk"/>.
    /// </summary>
    public class TMSMessage : MSTimeObject<TMSMessage, TMSTalk>
    {
        private AMSAssociation? association;
        private string? message;

        /// <summary>
        /// Initializes a new instance of the <see cref="TMSMessage"/> class.
        /// </summary>
        /// <param name="talk">The parent <see cref="TMSTalk"/> of the <see cref="TMSMessage"/>.</param>
        /// <param name="id">The identifier of the <see cref="TMSMessage"/>.</param>
        public TMSMessage(TMSTalk talk, long id)
            : base(talk, id)
        {
        }

        /// <summary>
        /// Gets the <see cref="AMSAssociation"/> of the <see cref="TMSMessage"/>.
        /// </summary>
        public AMSAssociation? Association
        {
            get
            {
                _ = this.GetAssociationAsync();
                return this.association;
            }
        }

        /// <summary>
        /// Gets or sets the message of the <see cref="TMSMessage"/>.
        /// </summary>
        public string Message
        {
            get
            {
                _ = this.GetMessageAsync();
                return this.message ?? string.Empty;
            }

            set
            {
                _ = this.SaveMessageAsync(value);
            }
        }

        /// <summary>
        /// Creates a new <see cref="TMSMessage"/>.
        /// </summary>
        /// <param name="parent">The parent of the created <see cref="TMSMessage"/>.</param>
        /// <param name="name">The name of the created <see cref="TMSMessage"/>.</param>
        /// <param name="message">The message of the created <see cref="TMSMessage"/>.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public static async Task<TMSMessage?> CreateAsync(TMSTalk parent, string name, string message)
        {
            if (await MSTimeObject<TMSMessage, TMSTalk>.CreateAsync(parent, name, Encoding.Unicode.GetBytes(message), DateTime.Now, null) is long id)
            {
                return new (parent, id);
            }

            return null;
        }

        /// <summary>
        /// Gets the <see cref="AMSAssociation"/> of the <see cref="TMSMessage"/>.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task<AMSAssociation?> GetAssociationAsync()
        {
            if (this.association is null
                && await this.GetAccessAsync().ConfigureAwait(false) is not null)
            {
                await new PipelineItem(this.Parent.Pipeline)
                {
                    BatchCommand = this.BasicGetByIDBatchCommand("get", "association"),
                    ReaderExecution = this.GetAssociationReaderExecution,
                }.ExecuteAsync().ConfigureAwait(false);
            }

            return this.association;
        }

        /// <summary>
        /// Gets the <see cref="Message"/> of the <see cref="TMSMessage"/>.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task<string> GetMessageAsync()
        {
            if (this.message is null
                && await this.GetDataAsync().ConfigureAwait(false) is byte[] array)
            {
                this.message = Encoding.Unicode.GetString(array);
                this.OnPropertyChanged(new PropertyChangedEventArgs(nameof(this.Message)));
            }

            return this.message ?? string.Empty;
        }

        /// <summary>
        /// Loads <see cref="TMSThread"/> related to the <see cref="TMSMessage"/>.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task<IEnumerable<TMSThread>> LoadThreadsAsync()
        {
            return (await this.LoadLinkedDataAsync<TMSMessage, TMSTalk>(typeof(TMSThread).GetDatabaseAbbreviation()).ConfigureAwait(false)).Select(id => new TMSThread(this.Parent, id));
        }

        /// <summary>
        /// Saves the <see cref="Message"/> of the <see cref="TMSMessage"/>.
        /// </summary>
        /// <param name="value">The new value.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task SaveMessageAsync(string? value)
        {
            if (this.message != value
                && !string.IsNullOrEmpty(value))
            {
                this.message = value;
                this.OnPropertyChanged(new PropertyChangedEventArgs(nameof(this.Message)));
                await this.SaveDataAsync(Encoding.Unicode.GetBytes(this.message)).ConfigureAwait(false);
            }
        }

        private void GetAssociationReaderExecution(NpgsqlDataReader reader)
        {
            if (!reader.IsDBNull(1)
                && this.Access is not null
                && this.Parent.Association is not null)
            {
                long id = BitConverter.ToInt64(this.Access.DecryptCbc((byte[])reader[1], this.Access.IV), 0);
                if (id == this.Parent.Association.ID)
                {
                    this.association = this.Parent.Association;
                }
                else
                {
                    this.association = new (this.Parent.Association, id);
                }

                this.OnPropertyChanged(new PropertyChangedEventArgs(nameof(this.Association)));
            }
        }
    }
}
