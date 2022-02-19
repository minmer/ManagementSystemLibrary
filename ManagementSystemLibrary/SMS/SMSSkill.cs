// <copyright file="SMSSkill.cs" company="PlaceholderCompany">
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
    /// Represents a skill of the skill management system.
    /// </summary>
    public class SMSSkill : MSScheduleObject<SMSSkill, SMSUpdate>
    {
        private AMSAssociation? parent;

        static SMSSkill()
        {
            ChildInitialization = (SMSSkill skill, long id) => { return new SMSUpdate(skill, id); };
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SMSSkill"/> class.
        /// </summary>
        /// <param name="association">The <see cref="AMSAssociation"/> of the <see cref="SMSSkill"/>.</param>
        /// <param name="id">The identifier of the <see cref="SMSSkill"/>.</param>
        /// <param name="key">The private <see cref="RSA"/> key of the <see cref="SMSSkill"/>.</param>
        /// <param name="signature">The private <see cref="RSA"/> signature of the <see cref="SMSSkill"/>.</param>
        public SMSSkill(AMSAssociation association, long id, byte[] key, byte[] signature)
            : base(association, id, key, signature)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SMSSkill"/> class.
        /// </summary>
        /// <param name="association">The <see cref="AMSAssociation"/> of the <see cref="SMSSkill"/>.</param>
        /// <param name="id">The identifier of the <see cref="SMSSkill"/>.</param>
        /// <param name="key">The private <see cref="RSA"/> key of the <see cref="SMSSkill"/>.</param>
        public SMSSkill(AMSAssociation association, long id, byte[] key)
            : base(association, id, key)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SMSSkill"/> class.
        /// </summary>
        /// <param name="association">The <see cref="AMSAssociation"/> of the <see cref="SMSSkill"/>.</param>
        /// <param name="id">The identifier of the <see cref="SMSSkill"/>.</param>
        /// <param name="access">The key of the <see cref="SMSSkill"/>.</param>
        public SMSSkill(AMSAssociation association, long id, Aes access)
            : base(association, id, access)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SMSSkill"/> class.
        /// </summary>
        /// <param name="association">The <see cref="AMSAssociation"/> of the <see cref="SMSSkill"/>.</param>
        /// <param name="id">The identifier of the <see cref="SMSSkill"/>.</param>
        public SMSSkill(AMSAssociation association, long id)
            : base(association, id)
        {
        }

        /// <summary>
        /// Gets the <see cref="AMSAssociation"/> of the <see cref="SMSSkill"/>.
        /// </summary>
        public AMSAssociation? Parent
        {
            get
            {
                _ = this.GetParentAsync();
                return this.parent;
            }
        }

        /// <summary>
        /// Creates a new <see cref="SMSSkill"/>.
        /// </summary>
        /// <param name="association">The <see cref="AMSAssociation"/> that is related to the <see cref="SMSSkill"/>.</param>
        /// <param name="name">The name of the <see cref="SMSSkill"/>.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public static async Task<SMSSkill?> CreateAsync(AMSAssociation association, string name)
        {
            if (Array.Empty<byte>() is byte[] keyArray
                && Array.Empty<byte>() is byte[] signatureArray
                && await association.Association.GetAccessAsync().ConfigureAwait(false) is Aes associationAccess
                && await association.Association.GenerateHashAsync().ConfigureAwait(false) is byte[] associationHash
                && await MSScheduleObject<SMSSkill, SMSUpdate>.CreateAsync<SMSSkill>(association.Association, name, (PipelineItem item, NpgsqlCommand command, DateTime _, AMSAssociation _, Aes access, string _, RSA key, RSA signature, double _, double _, StringBuilder builder) =>
                {
                    builder.Append(',')
                    .Append(item.AddParameter(command, "parent", NpgsqlDbType.Bytea, access.EncryptCbc(BitConverter.GetBytes(association.ID).ToArray(), access.IV)))
                    .Append(',')
                    .Append(item.AddParameter(command, "parenthash", NpgsqlDbType.Bytea, associationHash))
                    .Append(',')
                    .Append(item.AddParameter(command, "namehash", NpgsqlDbType.Bytea, SHA256.HashData(associationAccess.IV.Concat(Encoding.Unicode.GetBytes(name)).ToArray())))
                    .Append(',')
                    .Append(item.AddParameter(command, "parentverification", NpgsqlDbType.Bytea, association.PrivateSignature.SignData(BitConverter.GetBytes(association.ID), Pipeline.HashAlgorithmName, Pipeline.RSASignaturePadding)));
                    keyArray = key.ExportRSAPrivateKey();
                    signatureArray = signature.ExportRSAPrivateKey();
            }).ConfigureAwait(false) is long id
            && keyArray.Length > 0
            && signatureArray.Length > 0)
            {
                return new (association.Association, id, keyArray, signatureArray);
            }

            return null;
        }

        /// <summary>
        /// Gets the <see cref="AMSAssociation"/> of the <see cref="SMSUpdate"/>.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task<AMSAssociation?> GetParentAsync()
        {
            if (this.parent is null
                && await this.GetAccessAsync().ConfigureAwait(false) is not null)
            {
                await new PipelineItem(this.Pipeline)
                {
                    BatchCommand = this.BasicGetByIDBatchCommand("get", "parent"),
                    ReaderExecution = this.GetParentReaderExecution,
                }.ExecuteAsync().ConfigureAwait(false);
            }

            return this.parent;
        }

        /// <summary>
        /// Loads <see cref="SMSConstraint"/> related to the <see cref="SMSSkill"/>.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task<IEnumerable<SMSConstraint>> LoadRolesAsync()
        {
            return (await this.LoadParentsAsync<SMSConstraint, SMSSkill, AMSAssociation>().ConfigureAwait(false)).Select(id => new SMSConstraint(this, id));
        }

        /// <summary>
        /// Loads <see cref="SMSUpdate"/> related to the <see cref="SMSSkill"/>.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task<IEnumerable<SMSUpdate>> LoadUpdatesAsync()
        {
            return (await this.LoadItemsAsync<SMSUpdate, SMSSkill>().ConfigureAwait(false)).Select(id => new SMSUpdate(this, id));
        }

        private void GetParentReaderExecution(NpgsqlDataReader reader)
        {
            if (!reader.IsDBNull(1)
                && this.Access is not null)
            {
                long id = BitConverter.ToInt64(this.Access.DecryptCbc((byte[])reader[1], this.Access.IV), 0);
                this.parent = new (this.Association, id);
                this.OnPropertyChanged(new PropertyChangedEventArgs(nameof(this.Parent)));
            }
        }
    }
}
