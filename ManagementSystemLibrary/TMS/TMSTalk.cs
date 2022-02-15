// <copyright file="TMSTalk.cs" company="PlaceholderCompany">
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
    /// Represents a talk of the talk management system.
    /// </summary>
    public class TMSTalk : MSScheduleObject<TMSTalk, TMSMessage>
    {
        static TMSTalk()
        {
            ChildInitialization = (TMSTalk talk, long id) => { return new TMSMessage(talk, id); };
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TMSTalk"/> class.
        /// </summary>
        /// <param name="association">The <see cref="AMSAssociation"/> of the <see cref="TMSTalk"/>.</param>
        /// <param name="id">The identifier of the <see cref="TMSTalk"/>.</param>
        /// <param name="key">The private <see cref="RSA"/> key of the <see cref="TMSTalk"/>.</param>
        /// <param name="signature">The private <see cref="RSA"/> signature of the <see cref="TMSTalk"/>.</param>
        public TMSTalk(AMSAssociation association, long id, byte[] key, byte[] signature)
            : base(association, id, key, signature)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TMSTalk"/> class.
        /// </summary>
        /// <param name="association">The <see cref="AMSAssociation"/> of the <see cref="TMSTalk"/>.</param>
        /// <param name="id">The identifier of the <see cref="TMSTalk"/>.</param>
        /// <param name="key">The private <see cref="RSA"/> key of the <see cref="TMSTalk"/>.</param>
        public TMSTalk(AMSAssociation association, long id, byte[] key)
            : base(association, id, key)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TMSTalk"/> class.
        /// </summary>
        /// <param name="association">The <see cref="AMSAssociation"/> of the <see cref="TMSTalk"/>.</param>
        /// <param name="id">The identifier of the <see cref="TMSTalk"/>.</param>
        /// <param name="access">The key of the <see cref="TMSTalk"/>.</param>
        public TMSTalk(AMSAssociation association, long id, Aes access)
            : base(association, id, access)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TMSTalk"/> class.
        /// </summary>
        /// <param name="association">The <see cref="AMSAssociation"/> of the <see cref="TMSTalk"/>.</param>
        /// <param name="id">The identifier of the <see cref="TMSTalk"/>.</param>
        public TMSTalk(AMSAssociation association, long id)
            : base(association, id)
        {
        }

        /// <summary>
        /// Creates a new <see cref="TMSTalk"/>.
        /// </summary>
        /// <param name="association">The <see cref="AMSAssociation"/> that creates the <see cref="TMSTalk"/>.</param>
        /// <param name="name">The name of the <see cref="TMSTalk"/>.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public static async Task<TMSTalk?> CreateAsync(AMSAssociation association, string name)
        {
            if (Array.Empty<byte>() is byte[] keyArray
                && Array.Empty<byte>() is byte[] signatureArray
                && await MSScheduleObject<TMSTalk, TMSMessage>.CreateAsync<TMSTalk>(association, name, (PipelineItem _, NpgsqlCommand _, DateTime _, AMSAssociation _, Aes _, string _, RSA key, RSA signature, double _, double _, StringBuilder _) =>
            {
                keyArray = key.ExportRSAPrivateKey();
                signatureArray = signature.ExportRSAPrivateKey();
            }).ConfigureAwait(false) is long id
            && keyArray.Length > 0
            && signatureArray.Length > 0)
            {
                return new (association, id, keyArray, signatureArray);
            }

            return null;
        }

        /// <summary>
        /// Loads <see cref="TMSRole"/> related to the <see cref="TMSTalk"/>.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task<IEnumerable<TMSRole>> LoadRolesAsync()
        {
            return (await this.LoadParentsAsync<TMSRole, TMSTalk, AMSAssociation>().ConfigureAwait(false)).Select(id => new TMSRole(this, id));
        }

        /// <summary>
        /// Loads <see cref="TMSMessage"/> related to the <see cref="TMSTalk"/>.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task<IEnumerable<TMSMessage>> LoadMessagesAsync()
        {
            return (await this.LoadItemsAsync<TMSMessage, TMSTalk>().ConfigureAwait(false)).Select(id => new TMSMessage(this, id));
        }
    }
}
