// <copyright file="MSLinkObject.cs" company="PlaceholderCompany">
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
    /// A schedule object for the management system.
    /// </summary>
    /// <typeparam name="TChild">The child type.</typeparam>
    /// <typeparam name="TParent">The parent type.</typeparam>
    public abstract class MSLinkObject<TChild, TParent> : MSDatabaseObject
        where TChild : MSAccessObject
        where TParent : MSAccessObject
    {
        private Aes? access;
        private TChild? child;
        private TParent? parent;
        private bool? parentVerification;
        private MSAccessType? type;

        /// <summary>
        /// Initializes a new instance of the <see cref="MSLinkObject{T,T}"/> class.
        /// </summary>
        /// <param name="child">The child of the <see cref="MSLinkObject{T,T}"/>.</param>
        /// <param name="id">The identifier of the <see cref="MSLinkObject{T,T}"/>.</param>
        public MSLinkObject(TChild child, long id)
            : base(child.Pipeline, id)
        {
            this.child = child;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MSLinkObject{T,T}"/> class.
        /// </summary>
        /// <param name="id">The identifier of the <see cref="MSLinkObject{T,T}"/>.</param>
        /// <param name="parent">The child of the <see cref="MSLinkObject{T,T}"/>.</param>
        public MSLinkObject(long id, TParent parent)
            : base(parent.Pipeline, id)
        {
            this.parent = parent;
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
        /// Gets the child of the <see cref="MSLinkObject{T,T}"/>.
        /// </summary>
        public TChild? Child
        {
            get
            {
                _ = this.GetChildAsync();
                return this.child;
            }
        }

        /// <summary>
        /// Gets the parent of the <see cref="MSLinkObject{T,T}"/>.
        /// </summary>
        public TParent? Parent
        {
            get
            {
                _ = this.GetParentAsync();
                return this.parent;
            }
        }

        /// <summary>
        /// Gets a value indicating whether the parent of the <see cref="MSLinkObject{T,T}"/> is verified.
        /// </summary>
        public bool? ParentVerification
        {
            get
            {
                _ = this.VerifyParentAsync();
                return this.parentVerification;
            }
        }

        /// <summary>
        /// Gets the <see cref="MSAccessType"/> of the <see cref="MSLinkObject{T,T}"/>.
        /// </summary>
        public MSAccessType? Type
        {
            get
            {
                _ = this.GetTypeAsync();
                return this.Type;
            }
        }

        /// <summary>
        /// Gets or sets the function to initialize the administrator <see cref="Child"/> of the <see cref="MSLinkObject{T,T}"/>.
        /// </summary>
        protected static Func<AMSAssociation, long, byte[], byte[], TChild>? ChildAdministratorInitialization { get; set; }

        /// <summary>
        /// Gets or sets the function to initialize the contributor <see cref="Child"/> of the <see cref="MSLinkObject{T,T}"/>.
        /// </summary>
        protected static Func<AMSAssociation, long, byte[], TChild>? ChildContributorInitialization { get; set; }

        /// <summary>
        /// Gets or sets the function to initialize the observator <see cref="Child"/> of the <see cref="MSLinkObject{T,T}"/>.
        /// </summary>
        protected static Func<AMSAssociation, long, Aes, TChild>? ChildObservatorInitialization { get; set; }

        /// <summary>
        /// Gets or sets the function to initialize the public <see cref="Child"/> of the <see cref="MSLinkObject{T,T}"/>.
        /// </summary>
        protected static Func<AMSAssociation, long, TChild>? ChildPublicInitialization { get; set; }

        /// <summary>
        /// Gets or sets the function to initialize the <see cref="Parent"/> of the <see cref="MSLinkObject{T,T}"/>.
        /// </summary>
        protected static Func<AMSAssociation, long, TParent>? ParentInitialization { get; set; }

        /// <inheritdoc/>
        public override async Task<Aes?> GetAccessAsync()
        {
            if (this.access is null)
            {
                await new PipelineItem(this.Pipeline)
                {
                    BatchCommand = this.GetAccessBatchCommand,
                    ReaderExecution = this.GetAccessReaderExecution,
                }.ExecuteAsync().ConfigureAwait(false);
            }

            return this.access;
        }

        /// <summary>
        /// Gets the child of the <see cref="MSLinkObject{T,T}"/>.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task<TChild?> GetChildAsync()
        {
            if (this.child is null
                && await this.GetAccessAsync().ConfigureAwait(false) is not null
                && await this.GetTypeAsync().ConfigureAwait(false) is not null)
            {
                await new PipelineItem(this.Pipeline)
                {
                    BatchCommand = this.GetChildBatchCommand,
                    ReaderExecution = this.GetChildReaderExecution,
                }.ExecuteAsync().ConfigureAwait(false);
            }

            return this.child;
        }

        /// <summary>
        /// Gets the parent of the <see cref="MSLinkObject{T,T}"/>.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task<TParent?> GetParentAsync()
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
        /// Gets the <see cref="MSAccessType"/> of the <see cref="MSLinkObject{T,T}"/>.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task<MSAccessType?> GetTypeAsync()
        {
            if (this.type is null
                && await this.GetAccessAsync().ConfigureAwait(false) is not null)
            {
                await new PipelineItem(this.Pipeline)
                {
                    BatchCommand = this.BasicGetByIDBatchCommand("get", "type"),
                    ReaderExecution = this.GetTypeReaderExecution,
                }.ExecuteAsync().ConfigureAwait(false);
            }

            return this.type;
        }

        /// <summary>
        /// Gives the parent access to the child.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task GiveAccessAsync()
        {
            if (await this.GetParentAsync().ConfigureAwait(false) is TParent parent
                && await this.GetCreationTimeAsync().ConfigureAwait(false) is not null
                && this.Child?.AccessType == MSAccessType.Administrator)
            {
                if (await parent.GetPublicKeyAsync().ConfigureAwait(false) is not null
                    && await this.Child.GetAccessAsync().ConfigureAwait(false) is not null
                    && await this.Child.GenerateHashAsync().ConfigureAwait(false) is not null)
                {
                    await new PipelineItem(this.Pipeline)
                    {
                        BatchCommand = this.GiveAccessBatchCommand,
                    }.ExecuteAsync().ConfigureAwait(false);
                }
            }
        }

        /// <summary>
        /// Verifies the <see cref="Parent"/> of the <see cref="MSLinkObject{T,T}"/>.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task<bool?> VerifyParentAsync()
        {
            if (await this.GetParentAsync().ConfigureAwait(false) is not null
                && await this.GetTypeAsync().ConfigureAwait(false) is not null
                && await this.GetChildAsync().ConfigureAwait(false) is TChild child
                && this.parentVerification is null)
            {
                if (await child.GetPublicSignatureAsync().ConfigureAwait(false) is not null)
                {
                    await new PipelineItem(this.Pipeline)
                    {
                        BatchCommand = this.BasicGetByIDBatchCommand("verify", "parent"),
                        ReaderExecution = this.VerifyParentReaderExecution,
                    }.ExecuteAsync().ConfigureAwait(false);
                }
            }

            return this.parentVerification;
        }

        /// <inheritdoc/>
        public override async Task RemoveAsync()
        {
            await base.RemoveAsync().ConfigureAwait(false);
        }

        /// <summary>
        /// Creates a new <see cref="MSLinkObject{T,T}"/>.
        /// </summary>
        /// <param name="child">The child of the <see cref="MSLinkObject{T,T}"/>.</param>
        /// <param name="parent">The parent of the <see cref="MSLinkObject{T,T}"/>.</param>
        /// <param name="type">The type of the <see cref="MSLinkObject{T,T}"/>.</param>
        /// <param name="parameterMethod">A method for additional parameters.</param>
        /// <typeparam name="T">Type of the created object.</typeparam>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        protected static async Task<long?> CreateAsync<T>(MSAccessObject child, MSAccessObject parent, MSAccessType type, Action<PipelineItem, NpgsqlCommand, DateTime, AMSAssociation, Aes, MSAccessObject, MSAccessObject, MSAccessType, StringBuilder>? parameterMethod)
        {
            if (await child.GetPublicKeyAsync() is RSA childPublicKey
                && (await child.GenerateHashAsync().ConfigureAwait(false) ?? child.PublicHash) is byte[] childHash
                && (await child.GetAccessAsync().ConfigureAwait(false) ?? Aes.Create()) is Aes childAccess
                && RandomNumberGenerator.GetBytes(1194) is byte[] childPrivateKeyArray
                && RandomNumberGenerator.GetBytes(1194) is byte[] childPrivateSignatureArray
                && await parent.GenerateHashAsync().ConfigureAwait(false) is byte[] parentHash
                && parent.AccessType <= MSAccessType.Contributor
                && Aes.Create() is Aes access)
            {
                return await MSDatabaseObject.CreateAsync<T>(parent.Association, access, (PipelineItem item, NpgsqlCommand command, DateTime creationTime, AMSAssociation creator, Aes _, StringBuilder builder) =>
                {
                    if (type <= MSAccessType.Contributor)
                    {
                        childPrivateKeyArray = child.PrivateKey.ExportRSAPrivateKey();
                    }

                    if (type <= MSAccessType.Administrator)
                    {
                        childPrivateSignatureArray = child.PrivateSignature.ExportRSAPrivateKey();
                    }

                    builder.Append(',')
                    .Append(item.AddParameter(command, "child", NpgsqlDbType.Bytea, access.EncryptCbc(BitConverter.GetBytes(child.ID), access.IV)))
                    .Append(',')
                    .Append(item.AddParameter(command, "childaccess", NpgsqlDbType.Bytea, childPublicKey.Encrypt(access.Key.Concat(access.IV).ToArray(), Pipeline.RSAEncryptionPadding)))
                    .Append(',')
                    .Append(item.AddParameter(command, "childhash", NpgsqlDbType.Bytea, childHash))
                    .Append(',')
                    .Append(item.AddParameter(command, "parent", NpgsqlDbType.Bytea, access.EncryptCbc(BitConverter.GetBytes(parent.ID), access.IV)))
                    .Append(',')
                    .Append(item.AddParameter(command, "parentaccess", NpgsqlDbType.Bytea, parent.PrivateKey.Encrypt(access.Key.Concat(access.IV).ToArray(), Pipeline.RSAEncryptionPadding)))
                    .Append(',')
                    .Append(item.AddParameter(command, "parenthash", NpgsqlDbType.Bytea, parentHash))
                    .Append(',')
                    .Append(item.AddParameter(command, "privateaccess", NpgsqlDbType.Bytea, access.EncryptCbc(childAccess.Key.Concat(childAccess.IV).Concat(BitConverter.GetBytes(childPrivateKeyArray.Length)).Concat(childPrivateKeyArray).Concat(childPrivateSignatureArray).ToArray(), access.IV)))
                    .Append(',')
                    .Append(item.AddParameter(command, "type", NpgsqlDbType.Bytea, access.EncryptCbc(new byte[] { (byte)type }, access.IV)))
                    .Append(',')
                    .Append(item.AddParameter(command, "parentverification", NpgsqlDbType.Bytea, child.PrivateSignature.SignData(BitConverter.GetBytes(parent.ID).Concat(new byte[] { (byte)type }).ToArray(), HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1)));
                    parameterMethod?.Invoke(item, command, creationTime, creator, access, child, parent, type, builder);
                }).ConfigureAwait(false);
            }

            return null;
        }

        private bool GetAccessBatchCommand(PipelineItem item, NpgsqlCommand command)
        {
            command.CommandText += new StringBuilder("SELECT ")
                .Append(item.AddParameter(command, "itemid", NpgsqlDbType.Integer, item.ID))
                .AppendFormat(", * FROM get{0}access(", this.Abbreviation)
                .Append(item.AddParameter(command, "id", NpgsqlDbType.Bigint, this.ID))
                .Append(");");
            return true;
        }

        private bool GetChildBatchCommand(PipelineItem item, NpgsqlCommand command)
        {
            command.CommandText += new StringBuilder("SELECT ")
                .Append(item.AddParameter(command, "itemid", NpgsqlDbType.Integer, item.ID))
                .AppendFormat(", * FROM get{0}child(", this.Abbreviation)
                .Append(item.AddParameter(command, "id", NpgsqlDbType.Bigint, this.ID))
                .Append(");");
            return true;
        }

        private bool GiveAccessBatchCommand(PipelineItem item, NpgsqlCommand command)
        {
            if (this.parent?.PublicKey is not null
                && this.child?.Access is not null
                && this.child?.Hash is not null
                && this.access is not null
                && this.type <= MSAccessType.Observator
                && RandomNumberGenerator.GetBytes(1194) is byte[] keyArray
                && RandomNumberGenerator.GetBytes(1194) is byte[] signatureArray)
            {
                if (this.type <= MSAccessType.Contributor)
                {
                    keyArray = this.child.PrivateKey.ExportRSAPrivateKey();
                }

                if (this.type <= MSAccessType.Administrator)
                {
                    signatureArray = this.child.PrivateSignature.ExportRSAPrivateKey();
                }

                command.CommandText += new StringBuilder("SELECT ")
                    .Append(item.AddParameter(command, "itemid", NpgsqlDbType.Integer, item.ID))
                    .AppendFormat(", giveaccess{0}(", this.Abbreviation)
                    .Append(item.AddParameter(command, "id", NpgsqlDbType.Bigint, this.ID))
                    .Append(',')
                    .Append(item.AddParameter(command, "childhash", NpgsqlDbType.Bytea, this.child.Hash))
                    .Append(',')
                    .Append(item.AddParameter(command, "privateaccess", NpgsqlDbType.Bytea, this.access.EncryptCbc(this.child.Access.Key.Concat(this.child.Access.IV).Concat(BitConverter.GetBytes(keyArray.Length)).Concat(keyArray).Concat(signatureArray).ToArray(), this.access.IV)))
                    .Append(',')
                    .Append(item.AddParameter(command, "parentverification", NpgsqlDbType.Bytea, this.child.PrivateSignature.SignData(BitConverter.GetBytes(this.parent.ID).Concat(new byte[] { (byte)this.type }).ToArray(), HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1)))
                    .Append(");");
                return true;
            }

            return false;
        }

        private void GetAccessReaderExecution(NpgsqlDataReader reader)
        {
            if (!reader.IsDBNull(1)
                && this.Child is not null)
            {
                this.ExtractAccess(this.Child.PrivateKey.Decrypt((byte[])reader[1], Pipeline.RSAEncryptionPadding));
            }
            else if (!reader.IsDBNull(2)
                && this.Parent is not null)
            {
                this.ExtractAccess(this.Parent.PrivateKey.Decrypt((byte[])reader[2], Pipeline.RSAEncryptionPadding));
            }
            else
            {
            }
        }

        private void GetChildReaderExecution(NpgsqlDataReader reader)
        {
            if (!reader.IsDBNull(1)
                && !reader.IsDBNull(2)
                && !reader.IsDBNull(3)
                && this.Access is not null
                && (this.Parent as AMSAssociation ?? this.Parent?.Association) is AMSAssociation association)
            {
                this.child = ChildPublicInitialization?.Invoke(association, BitConverter.ToInt64(this.Access.DecryptCbc((byte[])reader[1], this.Access.IV)));
                if (this.child?.ID is long id
                    && !((ReadOnlySpan<byte>)this.child?.PublicHash).SequenceEqual((ReadOnlySpan<byte>)(byte[])reader[2]))
                {
                    this.ExtractChild(association, id, this.Access.DecryptCbc((byte[])reader[3], this.Access.IV));
                }

                this.OnPropertyChanged(new PropertyChangedEventArgs(nameof(this.Child)));
            }
        }

        private void ExtractChild(AMSAssociation association, long id, byte[] data)
        {
            switch (this.type)
            {
                case MSAccessType.Observator:
                    {
                        this.child = ChildObservatorInitialization?.Invoke(association, id, Aes.Create().ImportKey(data[..32], data[32..48]));
                        break;
                    }

                case MSAccessType.Contributor:
                    {
                        this.child = ChildContributorInitialization?.Invoke(association, id, data[52.. (BitConverter.ToInt32(data, 48) + 52)]);
                        break;
                    }

                case MSAccessType.Administrator:
                    {
                        this.child = ChildAdministratorInitialization?.Invoke(association, id, data[52.. (BitConverter.ToInt32(data, 48) + 52)], data[(BitConverter.ToInt32(data, 48) + 52) ..]);
                        break;
                    }

                default:
                    {
                        throw new InvalidOperationException();
                    }
            }
        }

        private void GetParentReaderExecution(NpgsqlDataReader reader)
        {
            if (!reader.IsDBNull(1)
                && this.Access is not null
                && (this.Child as AMSAssociation ?? this.Child?.Association) is AMSAssociation association)
            {
                this.parent = ParentInitialization?.Invoke(association, BitConverter.ToInt64(this.Access.DecryptCbc((byte[])reader[1], this.Access.IV)));
                this.OnPropertyChanged(new PropertyChangedEventArgs(nameof(this.Parent)));
            }
        }

        private void GetTypeReaderExecution(NpgsqlDataReader reader)
        {
            if (!reader.IsDBNull(1)
                && this.Access is not null)
            {
                if (this.Access.DecryptCbc((byte[])reader[1], this.Access.IV).ElementAtOrDefault(0) is byte type)
                {
                    this.type = (MSAccessType)type;
                    this.OnPropertyChanged(new PropertyChangedEventArgs(nameof(this.Type)));
                }
            }
        }

        private void VerifyParentReaderExecution(NpgsqlDataReader reader)
        {
            if (!reader.IsDBNull(1)
                && this.child?.PublicSignature is not null
                && this.parent is not null
                && this.type is not null)
            {
                this.parentVerification = this.child.PublicSignature.VerifyData(BitConverter.GetBytes(this.parent.ID).Concat(new byte[] { (byte)this.type }).ToArray(), (byte[])reader[1], Pipeline.HashAlgorithmName, Pipeline.RSASignaturePadding);
                this.OnPropertyChanged(new PropertyChangedEventArgs(nameof(this.ParentVerification)));
            }
        }

        private void ExtractAccess(byte[] data)
        {
            this.access = Aes.Create().ImportKey(data[..32], data[32..]);
            this.OnPropertyChanged(new PropertyChangedEventArgs(nameof(this.Access)));
        }
    }
}
