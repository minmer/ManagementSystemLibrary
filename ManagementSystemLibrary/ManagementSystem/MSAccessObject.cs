// <copyright file="MSAccessObject.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace ManagementSystemLibrary.ManagementSystem
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Linq;
    using System.Security.Cryptography;
    using System.Text;
    using System.Threading.Tasks;
    using ManagementSystemLibrary.AMS;
    using ManagementSystemLibrary.Pipeline;
    using Npgsql;
    using NpgsqlTypes;

    /// <summary>
    /// An access object for the management system.
    /// </summary>
    public abstract class MSAccessObject : MSDatabaseObject
    {
        private Aes? access;
        private string? name;
        private bool? nameVerification;
        private RSA? publicKey;
        private RSA? publicSignature;

        /// <summary>
        /// Initializes a new instance of the <see cref="MSAccessObject"/> class.
        /// </summary>
        /// <param name="association">The <see cref="AMSAssociation"/> calling the <see cref="MSAccessObject"/>.</param>
        /// <param name="id">The identifier of the <see cref="MSAccessObject"/>.</param>
        /// <param name="key">The private <see cref="RSA"/> key of the <see cref="MSAccessObject"/>.</param>
        /// <param name="signature">The private <see cref="RSA"/> signature of the <see cref="MSAccessObject"/>.</param>
        public MSAccessObject(AMSAssociation association, long id, byte[] key, byte[] signature)
            : base(association.Pipeline, id)
        {
            this.AccessType = MSAccessType.Administrator;
            this.Association = association;
            this.PrivateKey.ImportRSAPrivateKey(key, out _);
            this.PrivateSignature.ImportRSAPrivateKey(signature, out _);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MSAccessObject"/> class.
        /// </summary>
        /// <param name="association">The <see cref="AMSAssociation"/> calling the <see cref="MSAccessObject"/>.</param>
        /// <param name="id">The identifier of the <see cref="MSAccessObject"/>.</param>
        /// <param name="key">The private <see cref="RSA"/> key of the <see cref="MSAccessObject"/>.</param>
        public MSAccessObject(AMSAssociation association, long id, byte[] key)
            : base(association.Pipeline, id)
        {
            this.AccessType = MSAccessType.Contributor;
            this.Association = association;
            this.PrivateKey.ImportRSAPrivateKey(key, out _);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MSAccessObject"/> class.
        /// </summary>
        /// <param name="association">The <see cref="AMSAssociation"/> calling the <see cref="MSAccessObject"/>.</param>
        /// <param name="id">The identifier of the <see cref="MSAccessObject"/>.</param>
        /// <param name="access">The access of the <see cref="MSAccessObject"/>.</param>
        public MSAccessObject(AMSAssociation association, long id, Aes access)
            : base(association.Pipeline, id)
        {
            this.AccessType = MSAccessType.Observator;
            this.access = access;
            this.Association = association;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MSAccessObject"/> class.
        /// </summary>
        /// <param name="association">The <see cref="AMSAssociation"/> calling the <see cref="MSAccessObject"/>.</param>
        /// <param name="id">The identifier of the <see cref="MSAccessObject"/>.</param>
        public MSAccessObject(AMSAssociation association, long id)
            : base(association.Pipeline, id)
        {
            this.AccessType = MSAccessType.Public;
            this.Association = association;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MSAccessObject"/> class.
        /// </summary>
        /// <param name="account">The <see cref="AMSAccount"/> calling the <see cref="MSAccessObject"/>.</param>
        /// <param name="id">The identifier of the <see cref="MSAccessObject"/>.</param>
        /// <param name="key">The private <see cref="RSA"/> key of the <see cref="MSAccessObject"/>.</param>
        /// <param name="signature">The private <see cref="RSA"/> signature of the <see cref="MSAccessObject"/>.</param>
        internal MSAccessObject(AMSAccount account, long id, byte[] key, byte[] signature)
            : base(account.Pipeline, id)
        {
            this.AccessType = MSAccessType.Administrator;
            this.ProtectedAccount = account;
            this.Association = this as AMSAssociation ?? throw new NullReferenceException();
            this.PrivateKey.ImportRSAPrivateKey(key, out _);
            this.PrivateSignature.ImportRSAPrivateKey(signature, out _);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MSAccessObject"/> class.
        /// </summary>
        /// <param name="pipeline">The <see cref="Pipeline"/> calling the <see cref="MSAccessObject"/>.</param>
        /// <param name="id">The identifier of the <see cref="MSAccessObject"/>.</param>
        internal MSAccessObject(Pipeline pipeline, long id)
            : base(pipeline, id)
        {
            this.AccessType = MSAccessType.Administrator;
            this.Association = this as AMSAssociation ?? throw new NullReferenceException();
        }

        /// <inheritdoc/>
        public override Aes? Access
        {
            get
            {
                _ = this.GetAccessAsync();
                return this.access;
            }
        }

        /// <summary>
        /// Gets the <see cref="MSAccessType"/> of the <see cref="MSAccessObject"/>.
        /// </summary>
        public MSAccessType AccessType { get; internal set; }

        /// <summary>
        /// Gets the <see cref="AMSAccount"/> of the <see cref="MSAccessObject"/>.
        /// </summary>
        public virtual AMSAccount Account
        {
            get
            {
                if (this == this.Association)
                {
                    return this.ProtectedAccount ?? new AMSAccount(this.Association, -1);
                }
                else if (this is AMSDevice device)
                {
                    _ = device.GetAccountAsync();
                }

                return this.Association.Account;
            }

            internal set
            {
                if (this == this.Association)
                {
                    this.ProtectedAccount = value;
                }

                this.Association.ProtectedAccount = value;
            }
        }

        /// <summary>
        /// Gets the <see cref="AMSAssociation"/> of the <see cref="MSAccessObject"/>.
        /// </summary>
        public AMSAssociation Association { get; }

        /// <summary>
        /// Gets or sets the name of the <see cref="MSAccessObject"/>.
        /// </summary>
        public string Name
        {
            get
            {
                _ = this.GetNameAsync();
                return this.name ?? this.ID.ToString();
            }

            set
            {
                _ = this.SaveNameAsync(value);
            }
        }

        /// <summary>
        /// Gets a value indicating whether the name of the <see cref="MSDatabaseObject"/> is verified.
        /// </summary>
        public bool? NameVerification
        {
            get
            {
                _ = this.VerifyCreatorAsync();
                return this.nameVerification;
            }
        }

        /// <summary>
        /// Gets the private <see cref="RSA"/> key of the <see cref="MSAccessObject"/>.
        /// </summary>
        public RSA PrivateKey { get; } = RSA.Create(2048);

        /// <summary>
        /// Gets the private <see cref="RSA"/> signature of the <see cref="MSAccessObject"/>.
        /// </summary>
        public RSA PrivateSignature { get; } = RSA.Create(2048);

        /// <summary>
        /// Gets the public <see cref="RSA"/> key of the <see cref="MSAccessObject"/>.
        /// </summary>
        public RSA? PublicKey
        {
            get
            {
                _ = this.GetPublicKeyAsync();
                return this.publicKey;
            }
        }

        /// <summary>
        /// Gets the public <see cref="RSA"/> signature of the <see cref="MSAccessObject"/>.
        /// </summary>
        public RSA? PublicSignature
        {
            get
            {
                _ = this.GetPublicSignatureAsync();
                return this.publicSignature;
            }
        }

        /// <summary>
        /// Gets or sets the protected <see cref="Account"/>.
        /// </summary>
        protected AMSAccount? ProtectedAccount { get; set; }

        /// <summary>
        /// Deposits the name of the <see cref="MSAccessObject"/> for an <see cref="AMSAssociation"/>.
        /// </summary>
        /// <param name="destination">The destination of the deposition.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task DepositeNameAsync(AMSAssociation destination)
        {
            if (await this.GetNameAsync().ConfigureAwait(false) is not null
                && await this.GetAccessAsync().ConfigureAwait(false) is not null
                && this.Association?.AccessType <= MSAccessType.Contributor
                && await destination.GetPublicKeyAsync().ConfigureAwait(false) is not null)
                {
                    await new PipelineItem(this.Pipeline)
                    {
                        BatchCommand = this.DepositNameBatchCommand(destination),
                    }.ExecuteAsync().ConfigureAwait(false);
                }
        }

        /// <inheritdoc/>
        public override async Task<Aes?> GetAccessAsync()
        {
            if (this.AccessType <= MSAccessType.Contributor
                && this.access is null)
            {
                await new PipelineItem(this.Pipeline)
                {
                    BatchCommand = this.BasicGetByIDBatchCommand("get", "access"),
                    ReaderExecution = this.GetAccessReaderExecution,
                }.ExecuteAsync().ConfigureAwait(false);
            }

            return this.access;
        }

        /// <summary>
        /// Gets the name of the <see cref="MSAccessObject"/>.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task<string> GetNameAsync()
        {
            if (this.name is null)
            {
                if (await this.GetAccessAsync().ConfigureAwait(false) is not null)
                {
                    await new PipelineItem(this.Pipeline)
                    {
                        BatchCommand = this.BasicGetByIDBatchCommand("get", "name"),
                        ReaderExecution = this.GetNameReaderExecution,
                    }.ExecuteAsync().ConfigureAwait(false);
                }
                else if (this.Association is not null)
                {
                    if (await this.Association.GenerateHashAsync().ConfigureAwait(false) is not null)
                    {
                        await new PipelineItem(this.Pipeline)
                        {
                            BatchCommand = this.SearchNameBatchCommand,
                            ReaderExecution = this.SearchNameReaderExecution,
                        }.ExecuteAsync().ConfigureAwait(false);
                    }
                }
                else
                {
                }
            }

            return this.name ?? this.ID.ToString();
        }

        /// <summary>
        /// Gets the public <see cref="RSA"/> key of the <see cref="MSAccessObject"/>.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task<RSA?> GetPublicKeyAsync()
        {
            if (this.publicKey is null)
            {
                await new PipelineItem(this.Pipeline)
                {
                    BatchCommand = this.BasicGetByIDBatchCommand("get", "publickey"),
                    ReaderExecution = this.GetPublicKeyReaderExecution,
                }.ExecuteAsync().ConfigureAwait(false);
            }

            return this.publicKey;
        }

        /// <summary>
        /// Gets the public <see cref="RSA"/> signature of the <see cref="MSAccessObject"/>.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task<RSA?> GetPublicSignatureAsync()
        {
            if (this.publicSignature is null)
            {
                await new PipelineItem(this.Pipeline)
                {
                    BatchCommand = this.BasicGetByIDBatchCommand("get", "publicsignature"),
                    ReaderExecution = this.GetPublicSignatureReaderExecution,
                }.ExecuteAsync().ConfigureAwait(false);
            }

            return this.publicSignature;
        }

        /// <summary>
        /// Saves the name of the <see cref="MSAccessObject"/>.
        /// </summary>
        /// <param name="value">The new value.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task SaveNameAsync(string value)
        {
            if (this.name != value
                && !string.IsNullOrWhiteSpace(value)
                && await this.GetCreatorAsync().ConfigureAwait(false) is not null
                && await this.GetAccessAsync().ConfigureAwait(false) is not null
                && this.AccessType <= MSAccessType.Creator)
            {
                this.name = value;
                this.OnPropertyChanged(new PropertyChangedEventArgs(nameof(this.Name)));
                await new PipelineItem(this.Pipeline)
                {
                    BatchCommand = this.SaveNameBatchCommand,
                }.ExecuteAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Verifies the <see cref="Name"/> of the <see cref="MSDatabaseObject"/>.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task<bool?> VerifyNameAsync()
        {
            if (await this.GetNameAsync().ConfigureAwait(false) is not null
                && await this.GetCreatorAsync().ConfigureAwait(false) is AMSAccount creator)
            {
                if (await creator.GetPublicSignatureAsync().ConfigureAwait(false) is not null)
                {
                    await new PipelineItem(this.Pipeline)
                    {
                        BatchCommand = this.BasicGetByIDBatchCommand("verify", "name"),
                        ReaderExecution = this.VerifyNameReaderExecution,
                    }.ExecuteAsync().ConfigureAwait(false);
                }
            }

            return this.nameVerification;
        }

        /// <summary>
        /// Creates a new <see cref="MSAccessObject"/>.
        /// </summary>
        /// <param name="creator">The creator of the <see cref="MSAccessObject"/>.</param>
        /// <param name="name">The name of the <see cref="MSAccessObject"/>.</param>
        /// <param name="parameterMethod">A method for additional parameters.</param>
        /// <typeparam name="T">The created object type.</typeparam>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        protected static async Task<long?> CreateAsync<T>(AMSAssociation creator, string name, Action<PipelineItem, NpgsqlCommand, DateTime, AMSAssociation, Aes, string, RSA, RSA, StringBuilder>? parameterMethod)
        {
            if (RSA.Create(2048) is RSA privateKey
                && RSA.Create(2048) is RSA privateSignature
                && Aes.Create() is Aes access)
            {
                return await MSDatabaseObject.CreateAsync<T>(creator, access, (PipelineItem item, NpgsqlCommand command, DateTime creationTime, AMSAssociation _, Aes _, StringBuilder builder) =>
                {
                    builder.Append(',')
                    .Append(item.AddParameter(command, "access", NpgsqlDbType.Bytea, privateKey.Encrypt(access.Key.Concat(access.IV).ToArray(), Pipeline.RSAEncryptionPadding)))
                    .Append(',')
                    .Append(item.AddParameter(command, "name", NpgsqlDbType.Bytea, access.EncryptCbc(Encoding.Unicode.GetBytes(name), access.IV)))
                    .Append(',')
                    .Append(item.AddParameter(command, "publickey", NpgsqlDbType.Bytea, privateKey.ExportRSAPublicKey()))
                    .Append(',')
                    .Append(item.AddParameter(command, "publicsignature", NpgsqlDbType.Bytea, privateSignature.ExportRSAPublicKey()))
                    .Append(',')
                    .Append(item.AddParameter(command, "nameverification", NpgsqlDbType.Bytea, creator.PrivateSignature.SignData(Encoding.Unicode.GetBytes(name), Pipeline.HashAlgorithmName, Pipeline.RSASignaturePadding)));
                    parameterMethod?.Invoke(item, command, creationTime, creator, access, name, privateKey, privateSignature, builder);
                }).ConfigureAwait(false);
            }

            return null;
        }

        /// <summary>
        /// Loads the children of the <see cref="MSAccessObject"/>.
        /// </summary>
        /// <typeparam name="T1">The type of the <see cref="MSLinkObject{T,T}"/>.</typeparam>
        /// <typeparam name="T2">The child type of the <see cref="MSLinkObject{T,T}"/>.</typeparam>
        /// <typeparam name="T3">The parent type of the <see cref="MSLinkObject{T,T}"/>.</typeparam>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        protected async Task<IEnumerable<long>> LoadChildrenAsync<T1, T2, T3>()
            where T1 : MSLinkObject<T2, T3>
            where T2 : MSAccessObject
            where T3 : MSAccessObject
        {
            List<long> children = new ();
            if (await this.GenerateHashAsync().ConfigureAwait(false) is not null
                && this is T3 parent)
            {
                await new PipelineItem(this.Pipeline)
                {
                    BatchCommand = this.LoadChildrenBatchCommand(typeof(T1).GetDatabaseAbbreviation()),
                    ReaderExecution = (NpgsqlDataReader reader) =>
                    {
                        do
                        {
                            if (!reader.IsDBNull(1))
                            {
                                children.Add(reader.GetInt64(1));
                            }
                        }
                        while (reader.Read());
                    },
                }.ExecuteAsync().ConfigureAwait(false);
            }

            return children.ToArray();
        }

        /// <summary>
        /// Loads the parents of the <see cref="MSAccessObject"/>.
        /// </summary>
        /// <typeparam name="T1">The type of the <see cref="MSLinkObject{T,T}"/>.</typeparam>
        /// <typeparam name="T2">The child type of the <see cref="MSLinkObject{T,T}"/>.</typeparam>
        /// <typeparam name="T3">The parent type of the <see cref="MSLinkObject{T,T}"/>.</typeparam>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        protected async Task<IEnumerable<long>> LoadParentsAsync<T1, T2, T3>()
            where T1 : MSLinkObject<T2, T3>
            where T2 : MSAccessObject
            where T3 : MSAccessObject
        {
            List<long> parents = new ();
            if (await this.GenerateHashAsync().ConfigureAwait(false) is not null
                && this is T2 child)
            {
                await new PipelineItem(this.Pipeline)
                {
                    BatchCommand = this.LoadParentsBatchCommand(typeof(T1).GetDatabaseAbbreviation()),
                    ReaderExecution = (NpgsqlDataReader reader) =>
                    {
                        do
                        {
                            if (!reader.IsDBNull(1))
                            {
                                parents.Add(reader.GetInt64(1));
                            }
                        }
                        while (reader.Read());
                    },
                }.ExecuteAsync().ConfigureAwait(false);
            }

            return parents.ToArray();
        }

        private Func<PipelineItem, NpgsqlCommand, bool> DepositNameBatchCommand(MSAccessObject accessObject)
        {
            return (PipelineItem item, NpgsqlCommand command) =>
            {
                if (this.Access is not null
                    && Aes.Create() is Aes tempAccess
                    && accessObject.PublicKey is not null)
                {
                    command.CommandText += new StringBuilder("SELECT ")
                        .Append(item.AddParameter(command, "itemid", NpgsqlDbType.Integer, item.ID))
                        .Append(", depositesharedname(")
                        .Append(item.AddParameter(command, "hash", NpgsqlDbType.Bytea, SHA256.HashData(accessObject.PublicHash.Concat(this.PublicHash).ToArray())))
                        .Append(',')
                        .Append(item.AddParameter(command, "name", NpgsqlDbType.Bytea, accessObject.PublicKey.Encrypt(tempAccess.Key.Concat(tempAccess.IV).ToArray(), Pipeline.RSAEncryptionPadding).Concat(tempAccess.EncryptCbc(Encoding.Unicode.GetBytes(this.name ?? "_"), tempAccess.IV)).ToArray()))
                        .Append(");");
                    return true;
                }

                return false;
            };
        }

        private Func<PipelineItem, NpgsqlCommand, bool> LoadChildrenBatchCommand(string linkObjectAbbreviation)
        {
            return (PipelineItem item, NpgsqlCommand command) =>
            {
                if (this.Hash is not null)
                {
                    command.CommandText += new StringBuilder("SELECT ")
                        .Append(item.AddParameter(command, "itemid", NpgsqlDbType.Integer, item.ID))
                        .AppendFormat(", load{0}children(", linkObjectAbbreviation)
                        .Append(item.AddParameter(command, "parent", NpgsqlDbType.Bytea, this.Hash))
                        .Append(");");
                    return true;
                }

                return false;
            };
        }

        private Func<PipelineItem, NpgsqlCommand, bool> LoadParentsBatchCommand(string linkObjectAbbreviation)
        {
            return (PipelineItem item, NpgsqlCommand command) =>
            {
                if (this.Hash is not null)
                {
                    command.CommandText += new StringBuilder("SELECT ")
                        .Append(item.AddParameter(command, "itemid", NpgsqlDbType.Integer, item.ID))
                        .AppendFormat(", load{0}parents(", linkObjectAbbreviation)
                        .Append(item.AddParameter(command, "publicchild", NpgsqlDbType.Bytea, this.PublicHash))
                        .Append(',')
                        .Append(item.AddParameter(command, "child", NpgsqlDbType.Bytea, this.Hash))
                        .Append(");");
                    return true;
                }

                return false;
            };
        }

        private bool SaveNameBatchCommand(PipelineItem item, NpgsqlCommand command)
        {
            if (this.Access is not null
                && this.Creator is not null)
            {
                command.CommandText += new StringBuilder("SELECT ")
                    .Append(item.AddParameter(command, "itemid", NpgsqlDbType.Integer, item.ID))
                    .AppendFormat(", save{0}name(", this.Abbreviation)
                    .Append(item.AddParameter(command, "id", NpgsqlDbType.Bigint, this.ID))
                    .Append(',')
                    .Append(item.AddParameter(command, "name", NpgsqlDbType.Bytea, this.Access.EncryptCbc(Encoding.Unicode.GetBytes(this.name ?? "_"), this.Access.IV)))
                    .Append(',')
                    .Append(item.AddParameter(command, "namesignature", NpgsqlDbType.Bytea, this.Creator.PrivateSignature.SignData(Encoding.Unicode.GetBytes(this.name ?? "_"), Pipeline.HashAlgorithmName, Pipeline.RSASignaturePadding)))
                    .Append(");");
                return true;
            }

            return false;
        }

        private bool SearchNameBatchCommand(PipelineItem item, NpgsqlCommand command)
        {
            if (this.Association?.Hash is not null)
            {
                command.CommandText += new StringBuilder("SELECT ")
                    .Append(item.AddParameter(command, "itemid", NpgsqlDbType.Integer, item.ID))
                    .Append(", searchsharedname(")
                    .Append(item.AddParameter(command, "publichash", NpgsqlDbType.Bytea, SHA256.HashData(this.Association.PublicHash.Concat(this.PublicHash).ToArray())))
                    .Append(',')
                    .Append(item.AddParameter(command, "hash", NpgsqlDbType.Bytea, SHA256.HashData(this.Association.Hash.Concat(this.PublicHash).ToArray())))
                    .Append(");");
                return true;
            }

            return false;
        }

        private void GetAccessReaderExecution(NpgsqlDataReader reader)
        {
            if (!reader.IsDBNull(1)
                && this.AccessType <= MSAccessType.Contributor)
            {
                byte[] data = this.PrivateKey.Decrypt((byte[])reader[1], Pipeline.RSAEncryptionPadding);
                this.access = Aes.Create().ImportKey(data[..32], data[32..]);
                this.OnPropertyChanged(new PropertyChangedEventArgs(nameof(this.Access)));
            }
        }

        private void GetNameReaderExecution(NpgsqlDataReader reader)
        {
            if (!reader.IsDBNull(1)
                && this.Access is not null)
            {
                this.name = Encoding.Unicode.GetString(this.Access.DecryptCbc((byte[])reader[1], this.Access.IV));
                this.OnPropertyChanged(new PropertyChangedEventArgs(nameof(this.Name)));
            }
        }

        private void GetPublicKeyReaderExecution(NpgsqlDataReader reader)
        {
            if (!reader.IsDBNull(1))
            {
                this.publicKey = RSA.Create(2048);
                this.publicKey.ImportRSAPublicKey((byte[])reader[1], out _);
                this.OnPropertyChanged(new PropertyChangedEventArgs(nameof(this.PublicKey)));
            }
        }

        private void GetPublicSignatureReaderExecution(NpgsqlDataReader reader)
        {
            if (!reader.IsDBNull(1))
            {
                this.publicSignature = RSA.Create(2048);
                this.publicSignature.ImportRSAPublicKey((byte[])reader[1], out _);
                this.OnPropertyChanged(new PropertyChangedEventArgs(nameof(this.PublicSignature)));
            }
        }

        private void VerifyNameReaderExecution(NpgsqlDataReader reader)
        {
            if (!reader.IsDBNull(1)
                && this.Creator?.PublicSignature is not null
                && this.name is not null)
            {
                this.nameVerification = this.Creator.PublicSignature.VerifyData(Encoding.Unicode.GetBytes(this.name), (byte[])reader[1], Pipeline.HashAlgorithmName, Pipeline.RSASignaturePadding);
                this.OnPropertyChanged(new PropertyChangedEventArgs(nameof(this.NameVerification)));
            }
        }

        private void SearchNameReaderExecution(NpgsqlDataReader reader)
        {
            if (!reader.IsDBNull(1)
                && this.Association is not null)
            {
                byte[] data = this.Association.PrivateKey.Decrypt(((byte[])reader[1])[..256], Pipeline.RSAEncryptionPadding);
                Aes access = Aes.Create().ImportKey(data[..32], data[32..]);
                this.name = Encoding.Unicode.GetString(access.DecryptCbc(((byte[])reader[1])[256..], access.IV));
                this.OnPropertyChanged(new PropertyChangedEventArgs(nameof(this.Name)));
            }
        }
    }
}
