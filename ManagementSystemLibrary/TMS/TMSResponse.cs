// <copyright file="TMSResponse.cs" company="PlaceholderCompany">
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
    /// Represents a response to a <see cref="TMSMessage"/>.
    /// </summary>
    public class TMSResponse : MSDataObject<TMSMessage>
    {
        private TMSMessage? relatedMessage;

        /// <summary>
        /// Initializes a new instance of the <see cref="TMSResponse"/> class.
        /// </summary>
        /// <param name="message">The parent <see cref="TMSMessage"/> of the <see cref="TMSResponse"/>.</param>
        /// <param name="id">The identifier of the <see cref="TMSResponse"/>.</param>
        public TMSResponse(TMSMessage message, long id)
            : base(message, id)
        {
        }

        /// <summary>
        /// Gets the related Message of the <see cref="TMSResponse"/>.
        /// </summary>
        public TMSMessage? RelatedMessage
        {
            get
            {
                _ = this.GetRelatedMessageAsync();
                return this.relatedMessage;
            }
        }

        /// <summary>
        /// Creates a new <see cref="TMSResponse"/>.
        /// </summary>
        /// <param name="parent">The parent of the created <see cref="TMSResponse"/>.</param>
        /// <param name="name">The name of the created <see cref="TMSResponse"/>.</param>
        /// <param name="message">The message of the created <see cref="TMSResponse"/>.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public static async Task<TMSResponse?> CreateAsync(TMSMessage parent, string name, TMSMessage message)
        {
            if (await CreateAsync<TMSResponse>(parent, name, BitConverter.GetBytes(message.ID), null) is long id)
            {
                return new (parent, id);
            }

            return null;
        }

        /// <summary>
        /// Gets the <see cref="RelatedMessage"/> of the <see cref="TMSResponse"/>.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task<object?> GetRelatedMessageAsync()
        {
            if (this.relatedMessage is null
                && await this.GetDataAsync().ConfigureAwait(false) is byte[] array
                && this.GetAccessParent() is TMSTalk talk)
            {
                this.relatedMessage = new TMSMessage(talk, BitConverter.ToInt64(array));
                this.OnPropertyChanged(new PropertyChangedEventArgs(nameof(this.RelatedMessage)));
            }

            return this.relatedMessage;
        }
    }
}
