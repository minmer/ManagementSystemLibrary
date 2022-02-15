// <copyright file="AMSAccount.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace ManagementSystemLibrary.AMS
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Linq;
    using System.Security.Cryptography;
    using System.Text;
    using System.Threading.Tasks;
    using ManagementSystemLibrary.ManagementSystem;
    using ManagementSystemLibrary.Pipeline;
    using Npgsql;
    using NpgsqlTypes;

    /// <summary>
    /// Represents an account of the account managment system (AMS).
    /// </summary>
    public class AMSAccount : MSAccessObject
    {
        private AMSAssociation? mainAssociation;

        /// <summary>
        /// Initializes a new instance of the <see cref="AMSAccount"/> class.
        /// </summary>
        /// <param name="pipeline">The <see cref="Pipeline"/> of the <see cref="AMSAccount"/>.</param>
        /// <param name="id">The identifier of the <see cref="AMSAccount"/>.</param>
        /// <param name="key">The private <see cref="RSA"/> key of the <see cref="AMSAccount"/>.</param>
        /// <param name="signature">The private <see cref="RSA"/> signature of the <see cref="AMSAccount"/>.</param>
        public AMSAccount(Pipeline pipeline, long id, byte[] key, byte[] signature)
            : base(new AMSAssociation(pipeline, -1), id, key, signature)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AMSAccount"/> class.
        /// </summary>
        /// <param name="association">The <see cref="AMSAssociation"/> of the <see cref="AMSAccount"/>.</param>
        /// <param name="id">The identifier of the <see cref="AMSAccount"/>.</param>
        /// <param name="key">The private <see cref="RSA"/> key of the <see cref="AMSAccount"/>.</param>
        public AMSAccount(AMSAssociation association, long id, byte[] key)
            : base(association, id, key)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AMSAccount"/> class.
        /// </summary>
        /// <param name="association">The <see cref="AMSAssociation"/> of the <see cref="AMSAccount"/>.</param>
        /// <param name="id">The identifier of the <see cref="AMSAccount"/>.</param>
        /// <param name="access">The key of the <see cref="AMSAccount"/>.</param>
        public AMSAccount(AMSAssociation association, long id, Aes access)
            : base(association, id, access)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AMSAccount"/> class.
        /// </summary>
        /// <param name="association">The <see cref="AMSAssociation"/> of the <see cref="AMSAccount"/>.</param>
        /// <param name="id">The identifier of the <see cref="AMSAccount"/>.</param>
        public AMSAccount(AMSAssociation association, long id)
            : base(association, id)
        {
        }

        /// <summary>
        /// Gets the main <see cref="AMSAssociation"/> of the <see cref="MSAccessObject"/>.
        /// </summary>
        public virtual AMSAssociation? MainAssociation
        {
            get
            {
                _ = this.GetMainAssociationAsync();
                return this.mainAssociation;
            }
        }

        /// <summary>
        /// Creates a new <see cref="AMSAccount"/>.
        /// </summary>
        /// <param name="pipeline">The <see cref="Pipeline"/> that creates the <see cref="AMSAccount"/>.</param>
        /// <param name="name">The name of the <see cref="AMSAccount"/>.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public static async Task<AMSAccount?> CreateAsync(Pipeline pipeline, string name)
        {
            if (await AMSAssociation.CreateAsync(new (pipeline, -1), name).ConfigureAwait(false) is AMSAssociation association
                && association?.PrivateKey.ExportRSAPrivateKey() is byte[] associationPrivateKey
                && Array.Empty<byte>() is byte[] keyArray
                && Array.Empty<byte>() is byte[] signatureArray)
            {
                if (await association.GetAccessAsync().ConfigureAwait(false) is Aes associationAccess
                    && await CreateAsync<AMSAccount>(association, name, (PipelineItem item, NpgsqlCommand command, DateTime _, AMSAssociation _, Aes access, string _, RSA key, RSA signature, StringBuilder builder) =>
                {
                    keyArray = key.ExportRSAPrivateKey();
                    signatureArray = signature.ExportRSAPrivateKey();
                    builder.Append(',')
                    .Append(item.AddParameter(command, "child", NpgsqlDbType.Bytea, access.EncryptCbc(BitConverter.GetBytes(association.ID), access.IV)))
                    .Append(',')
                    .Append(item.AddParameter(command, "childhash", NpgsqlDbType.Bytea, association.PublicHash))
                    .Append(',')
                    .Append(item.AddParameter(command, "parent", NpgsqlDbType.Bytea, access.EncryptCbc(BitConverter.GetBytes(association.ID), access.IV)))
                    .Append(',')
                    .Append(item.AddParameter(command, "privateaccess", NpgsqlDbType.Bytea, access.EncryptCbc(associationAccess.Key.Concat(associationAccess.IV).Concat(BitConverter.GetBytes(associationPrivateKey.Length)).Concat(associationPrivateKey).Concat(association.PrivateSignature.ExportRSAPrivateKey()).ToArray(), access.IV)));
                }).ConfigureAwait(false) is long id
                && keyArray.Length > 0
                && signatureArray.Length > 0)
                {
                    return await new AMSAccount(association.Pipeline, id, keyArray, signatureArray).VerifyMainAssociationAsync().ConfigureAwait(false);
                }
            }

            return null;
        }

        /// <summary>
        /// Gets the main <see cref="AMSAssociation"/> of the <see cref="AMSAccount"/>.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task<AMSAssociation?> GetMainAssociationAsync()
        {
            if (this.mainAssociation is null
                && await this.GetAccessAsync().ConfigureAwait(false) is not null)
            {
                await new PipelineItem(this.Pipeline)
                {
                    BatchCommand = this.GetMainAssociationBatchCommand,
                    ReaderExecution = this.GetMainAssociationReaderExecution,
                }.ExecuteAsync().ConfigureAwait(false);
            }

            return this.mainAssociation;
        }

        /// <summary>
        /// Loads <see cref="AMSDevice"/> related to the <see cref="AMSAccount"/>.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task<IEnumerable<AMSDevice>> LoadDevicesAsync()
        {
            if (await this.GetMainAssociationAsync().ConfigureAwait(false) is AMSAssociation association)
            {
                return (await this.LoadItemsAsync<AMSDevice, AMSAccount>().ConfigureAwait(false)).Select(id => new AMSDevice(association, id));
            }

            return Array.Empty<AMSDevice>();
        }

        private async Task<AMSAccount> VerifyMainAssociationAsync()
        {
            if (await this.GetMainAssociationAsync().ConfigureAwait(false) is AMSAssociation mainAssociation)
            {
                if (await mainAssociation.GetCreationTimeAsync().ConfigureAwait(false) is not null)
                {
                    await new PipelineItem(this.Pipeline)
                    {
                        BatchCommand = this.VerifyMainAssociationBatchCommand,
                    }.ExecuteAsync().ConfigureAwait(false);
                }
            }

            return this;
        }

        private bool GetMainAssociationBatchCommand(PipelineItem item, NpgsqlCommand command)
        {
            command.CommandText += new StringBuilder("SELECT ")
                .Append(item.AddParameter(command, "itemid", NpgsqlDbType.Integer, item.ID))
                .Append(", * FROM getamsaccountchild(")
                .Append(item.AddParameter(command, "id", NpgsqlDbType.Bigint, this.ID))
                .Append(");");
            return true;
        }

        private bool VerifyMainAssociationBatchCommand(PipelineItem item, NpgsqlCommand command)
        {
            if (this.MainAssociation?.Access is not null
                && this.MainAssociation?.CreationTime is not null
                && BitConverter.GetBytes(this.ID) is byte[] creatorArray)
            {
                command.CommandText += new StringBuilder("SELECT ")
                    .Append(item.AddParameter(command, "itemid", NpgsqlDbType.Integer, item.ID))
                    .Append(", verifyamsaccountassociation(")
                    .Append(item.AddParameter(command, "id", NpgsqlDbType.Bigint, this.MainAssociation.ID))
                    .Append(',')
                    .Append(item.AddParameter(command, "creator", NpgsqlDbType.Bytea, this.MainAssociation.Access.EncryptCbc(creatorArray, this.MainAssociation.Access.IV)))
                    .Append(',')
                    .Append(item.AddParameter(command, "creatorverification", NpgsqlDbType.Bytea, this.PrivateSignature.SignData(creatorArray.Concat(BitConverter.GetBytes(this.MainAssociation.CreationTime.Value.Ticks)).ToArray(), Pipeline.HashAlgorithmName, Pipeline.RSASignaturePadding)))
                    .Append(");");
                return true;
            }

            return false;
        }

        private void GetMainAssociationReaderExecution(NpgsqlDataReader reader)
        {
            if (!reader.IsDBNull(1)
                && !reader.IsDBNull(3)
                && this.Access is not null)
            {
                byte[] data = this.Access.DecryptCbc((byte[])reader[3], this.Access.IV);
                this.mainAssociation = new (this, BitConverter.ToInt64(this.Access.DecryptCbc((byte[])reader[1], this.Access.IV)), data[52.. (BitConverter.ToInt32(data, 48) + 52)], data[(BitConverter.ToInt32(data, 48) + 52) ..]);
                this.OnPropertyChanged(new PropertyChangedEventArgs(nameof(this.MainAssociation)));
            }
        }
    }
}
