// <copyright file="AMSDevice.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace ManagementSystemLibrary.AMS
{
    using System;
    using System.ComponentModel;
    using System.Security.Cryptography;
    using System.Text;
    using System.Threading.Tasks;
    using ManagementSystemLibrary.ManagementSystem;
    using ManagementSystemLibrary.Pipeline;
    using Npgsql;
    using NpgsqlTypes;

    /// <summary>
    /// Represents a device of an <see cref="AMSAccount"/>.
    /// </summary>
    public class AMSDevice : MSAccessObject
    {
        private AMSAssociation? mainAssociation;
        private bool? verification;

        /// <summary>
        /// Initializes a new instance of the <see cref="AMSDevice"/> class.
        /// </summary>
        /// <param name="pipeline">The <see cref="Pipeline"/> of the <see cref="AMSDevice"/>.</param>
        /// <param name="id">The identifier of the <see cref="AMSDevice"/>.</param>
        /// <param name="key">The private key of the <see cref="AMSDevice"/>.</param>
        /// <param name="signature">The private signature of the <see cref="AMSDevice"/>.</param>
        public AMSDevice(Pipeline pipeline, long id, byte[] key, byte[] signature)
            : base(new AMSAssociation(pipeline, -1), id, key, signature)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AMSDevice"/> class.
        /// </summary>
        /// <param name="association">The <see cref="AMSAssociation"/> of the <see cref="AMSDevice"/>.</param>
        /// <param name="id">The identifier of the <see cref="AMSDevice"/>.</param>
        public AMSDevice(AMSAssociation association, long id)
            : base(association, id)
        {
        }

        /// <inheritdoc/>
        public override AMSAccount Account
        {
            get
            {
                _ = this.GetAccountAsync();
                return this.ProtectedAccount ?? new AMSAccount(this.Association, -1);
            }
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
        /// Gets a value indicating whether the <see cref="AMSDevice"/> is verified.
        /// </summary>
        public bool? Verification
        {
            get
            {
                _ = this.VerifyAsync();
                return this.verification;
            }
        }

        /// <summary>
        /// Creates a new <see cref="AMSDevice"/>.
        /// </summary>
        /// <param name="pipeline">The <see cref="Pipeline"/> of the <see cref="AMSDevice"/>.</param>
        /// <param name="accountID">The identifier of the <see cref="AMSAccount"/> of the <see cref="AMSDevice"/>.</param>
        /// <param name="name">The name of the <see cref="AMSDevice"/>.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public static async Task<AMSDevice?> CreateAsync(Pipeline pipeline, long accountID, string name)
        {
            return await CreateAsync(new AMSAccount(new AMSAssociation(pipeline, -1), accountID), name);
        }

        /// <summary>
        /// Creates a new <see cref="AMSDevice"/>.
        /// </summary>
        /// <param name="account">The <see cref="AMSAccount"/> of the <see cref="AMSDevice"/>.</param>
        /// <param name="name">The name of the <see cref="AMSDevice"/>.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public static async Task<AMSDevice?> CreateAsync(AMSAccount account, string name)
        {
            await account.GenerateHashAsync().ConfigureAwait(false);
            if (Array.Empty<byte>() is byte[] keyArray
                && Array.Empty<byte>() is byte[] signatureArray
                && await CreateAsync<AMSDevice>(account.Association, name, (PipelineItem item, NpgsqlCommand command, DateTime _, AMSAssociation _, Aes access, string _, RSA key, RSA signature, StringBuilder builder) =>
            {
                builder.Append(',')
                .Append(item.AddParameter(command, "child", NpgsqlDbType.Bytea, access.EncryptCbc(BitConverter.GetBytes(account.ID), access.IV)))
                .Append(',')
                .Append(item.AddParameter(command, "childhash", NpgsqlDbType.Bytea, account.PublicHash))
                .Append(',')
                .Append(item.AddParameter(command, "parent", NpgsqlDbType.Bytea, access.EncryptCbc(BitConverter.GetBytes(account.ID), access.IV)))
                .Append(',')
                .Append(item.AddParameter(command, "privateaccess", NpgsqlDbType.Bytea, RandomNumberGenerator.GetBytes(2432)))
                .Append(',')
                .Append(item.AddParameter(command, "childverification", NpgsqlDbType.Bytea, RandomNumberGenerator.GetBytes(256)));
                keyArray = key.ExportRSAPrivateKey();
                signatureArray = signature.ExportRSAPrivateKey();
            }).ConfigureAwait(false) is long id
            && keyArray.Length > 0
            && signatureArray.Length > 0)
            {
                AMSDevice device = new (account.Pipeline, id, keyArray, signatureArray);
                if (account.AccessType <= MSAccessType.Administrator)
                {
                    await device.GiveAccessAsync(account).ConfigureAwait(false);
                }

                return device;
            }

            return null;
        }

        /// <summary>
        /// Gets the <see cref="AMSAccount"/> of the <see cref="AMSDevice"/>.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task<AMSAccount?> GetAccountAsync()
        {
            if (this.ProtectedAccount is null
                && await this.GetAccessAsync().ConfigureAwait(false) is not null)
            {
                await new PipelineItem(this.Pipeline)
                {
                    BatchCommand = this.GetAccountBatchCommand,
                    ReaderExecution = this.GetAccountReaderExecution,
                }.ExecuteAsync().ConfigureAwait(false);
            }

            return this.ProtectedAccount;
        }

        /// <summary>
        /// Gets the <see cref="AMSAssociation"/> of the <see cref="AMSDevice"/>.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task<AMSAssociation?> GetMainAssociationAsync()
        {
            if (this.mainAssociation is null
                && await this.GetAccountAsync().ConfigureAwait(false) is AMSAccount account)
            {
                this.mainAssociation = await account.GetMainAssociationAsync().ConfigureAwait(false);
                this.OnPropertyChanged(new PropertyChangedEventArgs(nameof(this.MainAssociation)));
            }

            return this.mainAssociation;
        }

        /// <summary>
        /// Gives the <see cref="AMSDevice"/> access to the <see cref="AMSAccount"/>.
        /// </summary>
        /// <param name="account">The <see cref="AMSAccount"/> giving access.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task GiveAccessAsync(AMSAccount account)
        {
            if (await this.GetCreationTimeAsync().ConfigureAwait(false) is not null
                && account.AccessType == MSAccessType.Administrator
                && await this.GetPublicKeyAsync().ConfigureAwait(false) is not null
                && await account.GetAccessAsync().ConfigureAwait(false) is not null
                && await account.GenerateHashAsync().ConfigureAwait(false) is not null)
            {
                await new PipelineItem(this.Pipeline)
                {
                    BatchCommand = this.GiveAccessBatchCommand(account),
                }.ExecuteAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Verifies the <see cref="AMSDevice"/>.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task<bool?> VerifyAsync()
        {
            if (await this.GetAccountAsync().ConfigureAwait(false) is AMSAccount account)
            {
                if (await account.GetPublicSignatureAsync().ConfigureAwait(false) is not null)
                {
                    await new PipelineItem(this.Pipeline)
                    {
                        BatchCommand = this.BasicGetByIDBatchCommand("verify", string.Empty),
                        ReaderExecution = this.VerifyReaderExecution,
                    }.ExecuteAsync().ConfigureAwait(false);
                }
            }

            return this.verification;
        }

        private bool GetAccountBatchCommand(PipelineItem item, NpgsqlCommand command)
        {
            command.CommandText += new StringBuilder("SELECT ")
                .Append(item.AddParameter(command, "itemid", NpgsqlDbType.Integer, item.ID))
                .Append(", * FROM getamsdevicechild(")
                .Append(item.AddParameter(command, "id", NpgsqlDbType.Bigint, this.ID))
                .Append(");");
            return true;
        }

        private Func<PipelineItem, NpgsqlCommand, bool> GiveAccessBatchCommand(AMSAccount account)
        {
            return (PipelineItem item, NpgsqlCommand command) =>
            {
                if (this.PublicKey is not null
                    && this.Access is not null
                    && account.Access is not null
                    && account.Hash is not null
                    && account.PrivateKey.ExportRSAPrivateKey() is byte[] privateKeyArray)
                {
                    command.CommandText += new StringBuilder("SELECT ")
                        .Append(item.AddParameter(command, "itemid", NpgsqlDbType.Integer, item.ID))
                        .Append(", giveaccessamsdevice(")
                        .Append(item.AddParameter(command, "id", NpgsqlDbType.Bigint, this.ID))
                        .Append(',')
                        .Append(item.AddParameter(command, "childhash", NpgsqlDbType.Bytea, account.Hash))
                        .Append(',')
                        .Append(item.AddParameter(command, "privateaccess", NpgsqlDbType.Bytea, this.Access.EncryptCbc(account.Access.Key.Concat(account.Access.IV).Concat(BitConverter.GetBytes(privateKeyArray.Length)).Concat(privateKeyArray).Concat(account.PrivateSignature.ExportRSAPrivateKey()).ToArray(), this.Access.IV)))
                        .Append(',')
                        .Append(item.AddParameter(command, "childverification", NpgsqlDbType.Bytea, account.PrivateSignature.SignData(BitConverter.GetBytes(this.ID).ToArray(), HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1)))
                        .Append(");");
                    return true;
                }

                return false;
            };
        }

        private void GetAccountReaderExecution(NpgsqlDataReader reader)
        {
            if (!reader.IsDBNull(1)
                && !reader.IsDBNull(2)
                && !reader.IsDBNull(3)
                && this.Access is not null)
            {
                this.ProtectedAccount = new (this.Association, BitConverter.ToInt64(this.Access.DecryptCbc((byte[])reader[1], this.Access.IV)));
                if (!((ReadOnlySpan<byte>)this.ProtectedAccount.PublicHash).SequenceEqual((ReadOnlySpan<byte>)(byte[])reader[2]))
                {
                    byte[] data = this.Access.DecryptCbc((byte[])reader[3], this.Access.IV);
                    this.ProtectedAccount = new (this.Pipeline, this.ProtectedAccount.ID, data[52.. (BitConverter.ToInt32(data, 48) + 52)], data[(BitConverter.ToInt32(data, 48) + 52) ..]);
                }

                this.OnPropertyChanged(new PropertyChangedEventArgs(nameof(this.Account)));
            }
        }

        private void VerifyReaderExecution(NpgsqlDataReader reader)
        {
            if (!reader.IsDBNull(1)
                && this.ProtectedAccount?.PublicSignature is not null)
            {
                this.verification = this.ProtectedAccount.PublicSignature.VerifyData(BitConverter.GetBytes(this.ID), (byte[])reader[1], Pipeline.HashAlgorithmName, Pipeline.RSASignaturePadding);
                this.OnPropertyChanged(new PropertyChangedEventArgs(nameof(this.Verification)));
            }
        }
    }
}
