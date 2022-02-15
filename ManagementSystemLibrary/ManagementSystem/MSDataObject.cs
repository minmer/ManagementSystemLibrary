// <copyright file="MSDataObject.cs" company="PlaceholderCompany">
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
    /// A data object for the management system.
    /// </summary>
    /// <typeparam name="T">The type of the parent.</typeparam>
    public abstract class MSDataObject<T> : MSDatabaseObject
        where T : MSDatabaseObject
    {
        private Aes? access;
        private byte[]? data;
        private bool? dataVerification;
        private AMSAccount? modifier;
        private DateTime? modificationTime;

        /// <summary>
        /// Initializes a new instance of the <see cref="MSDataObject{T}"/> class.
        /// </summary>
        /// <param name="parent">The parent of the <see cref="MSDataObject{T}"/>.</param>
        /// <param name="id">The identifier of the <see cref="MSDataObject{T}"/>.</param>
        public MSDataObject(T parent, long id)
            : base(parent.Pipeline, id)
        {
            this.Parent = parent ?? throw new ArgumentNullException(nameof(parent));
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
        /// Gets or sets the data of the <see cref="MSDataObject{T}"/>.
        /// </summary>
        public byte[]? Data
        {
            get
            {
                _ = this.GetDataAsync();
                return this.data;
            }

            set
            {
                _ = this.SaveDataAsync(value);
            }
        }

        /// <summary>
        /// Gets a value indicating whether the data of the <see cref="MSDataObject{T}"/> is verified.
        /// </summary>
        public bool? DataVerification
        {
            get
            {
                _ = this.VerifyDataAsync();
                return this.dataVerification;
            }
        }

        /// <summary>
        /// Gets the last modification time of the <see cref="MSDataObject{T}"/>.
        /// </summary>
        public DateTime? ModificationTime
        {
            get
            {
                _ = this.GetModificationTimeAsync();
                return this.modificationTime;
            }
        }

        /// <summary>
        /// Gets the last modifier <see cref="AMSAccount"/> of the <see cref="MSDataObject{T}"/>.
        /// </summary>
        public AMSAccount? Modifier
        {
            get
            {
                _ = this.GetModifierAsync();
                return this.modifier;
            }
        }

        /// <summary>
        /// Gets the parent of the <see cref="MSDataObject{T}"/>.
        /// </summary>
        public T Parent { get; }

        /// <inheritdoc/>
        public override async Task<Aes?> GetAccessAsync()
        {
            if (this.access is null)
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
        /// Gets the data of the <see cref="MSDataObject{T}"/>.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task<byte[]?> GetDataAsync()
        {
            if (this.data is null
                && await this.GetAccessAsync().ConfigureAwait(false) is not null)
            {
                await new PipelineItem(this.Parent.Pipeline)
                {
                    BatchCommand = this.BasicGetByIDBatchCommand("get", "data"),
                    ReaderExecution = this.GetDataReaderExecution,
                }.ExecuteAsync().ConfigureAwait(false);
            }

            return this.data;
        }

        /// <summary>
        /// Gets the last modification time of the <see cref="MSDataObject{T}"/>.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task<DateTime?> GetModificationTimeAsync()
        {
            if (this.modificationTime is null
                && await this.GetAccessAsync().ConfigureAwait(false) is not null)
            {
                await new PipelineItem(this.Pipeline)
                {
                    BatchCommand = this.BasicGetByIDBatchCommand("get", "modificationtime"),
                    ReaderExecution = this.GetModificationTimeReaderExecution,
                }.ExecuteAsync().ConfigureAwait(false);
            }

            return this.modificationTime;
        }

        /// <summary>
        /// Gets the last modifier <see cref="AMSAccount"/> of the <see cref="MSDataObject{T}"/>.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task<AMSAccount?> GetModifierAsync()
        {
            if (this.modifier is null
                && await this.GetAccessAsync().ConfigureAwait(false) is not null)
            {
                await new PipelineItem(this.Pipeline)
                {
                    BatchCommand = this.BasicGetByIDBatchCommand("get", "modifier"),
                    ReaderExecution = this.GetModifierReaderExecution,
                }.ExecuteAsync().ConfigureAwait(false);
            }

            return this.modifier;
        }

        /// <summary>
        /// Saves the data of the <see cref="MSDataObject{T}"/>.
        /// </summary>
        /// <param name="value">The new value.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task SaveDataAsync(byte[]? value)
        {
            if (value is not null
                && await this.GetAccessAsync().ConfigureAwait(false) is not null)
            {
                this.data = value;
                this.modifier = this.GetAccessParent().Account;
                this.modificationTime = DateTime.Now;
                this.OnPropertyChanged(new PropertyChangedEventArgs(nameof(this.Data)));
                this.OnPropertyChanged(new PropertyChangedEventArgs(nameof(this.Modifier)));
                this.OnPropertyChanged(new PropertyChangedEventArgs(nameof(this.ModificationTime)));
                await new PipelineItem(this.Pipeline)
                {
                    BatchCommand = this.SaveDataBatchCommand,
                }.ExecuteAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Verifies the <see cref="Data"/> of the <see cref="MSDataObject{T}"/>.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task<bool?> VerifyDataAsync()
        {
            if (await this.GetDataAsync().ConfigureAwait(false) is not null
                && await this.GetModificationTimeAsync().ConfigureAwait(false) is not null
                && await this.GetModifierAsync().ConfigureAwait(false) is AMSAccount modifier)
            {
                if (await modifier.GetPublicSignatureAsync().ConfigureAwait(false) is not null)
                {
                    await new PipelineItem(this.Pipeline)
                    {
                        BatchCommand = this.BasicGetByIDBatchCommand("verify", "data"),
                        ReaderExecution = this.VerifyDataReaderExecution,
                    }.ExecuteAsync().ConfigureAwait(false);
                }
            }

            return this.dataVerification;
        }

        /// <summary>
        /// Gets the next parent of type <see cref="MSAccessObject"/>.
        /// </summary>
        /// <returns>The next parent of type <see cref="MSAccessObject"/>.</returns>
        /// <exception cref="NullReferenceException">Thrown if there is no parent of type <see cref="MSAccessObject"/>.</exception>
        internal MSAccessObject GetAccessParent()
        {
            return this.Parent as MSAccessObject ?? (this.Parent as MSDataObject<T>)?.GetAccessParent() ?? throw new NullReferenceException(nameof(this.Parent));
        }

        /// <summary>
        /// Creates a new <see cref="MSDataObject{T}"/>.
        /// </summary>
        /// <param name="parent">The parent <see cref="MSDataObject{T}"/>.</param>
        /// <param name="name">The name of the <see cref="MSDataObject{T}"/>.</param>
        /// <param name="data">The data of the  <see cref="MSDataObject{T}"/>.</param>
        /// <param name="parameterMethod">A method for additional parameters.</param>
        /// <typeparam name="TObj">The created child type.</typeparam>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        protected static async Task<long?> CreateAsync<TObj>(MSDatabaseObject parent, string name, byte[] data, Action<PipelineItem, NpgsqlCommand, DateTime, Aes, byte[], MSDatabaseObject, StringBuilder>? parameterMethod)
        {
            if (((parent as MSAccessObject) ?? (parent as MSDataObject<MSDatabaseObject>)?.GetAccessParent()) is MSAccessObject parentAccessObject)
            {
                if (BitConverter.GetBytes(parentAccessObject.Account.ID) is byte[] modifierArray
                    && parentAccessObject.AccessType <= MSAccessType.Contributor
                    && await parentAccessObject.GenerateHashAsync().ConfigureAwait(false) is byte[] parentHash
                    && await parentAccessObject.GetAccessAsync().ConfigureAwait(false) is Aes parentAccess
                    && Aes.Create().ImportKey(parentAccessObject.Access?.Key ?? Aes.Create().Key, Aes.Create().IV) is Aes access)
                {
                    return await MSDatabaseObject.CreateAsync<TObj>(parentAccessObject.Association, access, (PipelineItem item, NpgsqlCommand command, DateTime creationTime, AMSAssociation _, Aes _, StringBuilder builder) =>
                    {
                        builder.Append(',')
                        .Append(item.AddParameter(command, "access", NpgsqlDbType.Bytea, parentAccess.EncryptCbc(access.IV, parentAccess.IV)))
                        .Append(',')
                        .Append(item.AddParameter(command, "data", NpgsqlDbType.Bytea, access.EncryptCbc(data, access.IV)))
                        .Append(',')
                        .Append(item.AddParameter(command, "modificationtime", NpgsqlDbType.Bytea, access.EncryptCbc(BitConverter.GetBytes(creationTime.Ticks), access.IV)))
                        .Append(',')
                        .Append(item.AddParameter(command, "modifier", NpgsqlDbType.Bytea, access.EncryptCbc(modifierArray, access.IV)))
                        .Append(',')
                        .Append(item.AddParameter(command, "name", NpgsqlDbType.Bytea, SHA256.HashData(parentHash.Concat(typeof(TObj).GetDatabaseAbbreviationHash()).Concat(Encoding.Unicode.GetBytes(name)).ToArray())))
                        .Append(',')
                        .Append(item.AddParameter(command, "parent", NpgsqlDbType.Bytea, parentHash))
                        .Append(',')
                        .Append(item.AddParameter(command, "dataverification", NpgsqlDbType.Bytea, parentAccessObject.Account.PrivateSignature.SignData(data.Concat(modifierArray).Concat(BitConverter.GetBytes(creationTime.Ticks)).ToArray(), Pipeline.HashAlgorithmName, Pipeline.RSASignaturePadding)));
                        parameterMethod?.Invoke(item, command, creationTime, access, data, parent, builder);
                    }).ConfigureAwait(false);
                }
            }

            return null;
        }

        /// <summary>
        /// Loads the <see cref="MSDataObject{T}"/> linked to the <see cref="MSDataObject{T}"/>.
        /// </summary>
        /// <param name="dataAbbreviation">The abbreviation of the data type.</param>
        /// <typeparam name="T1">The type of the <see cref="MSDataObject{T}"/>.</typeparam>
        /// <typeparam name="T2">The parent type of the <see cref="MSDatabaseObject"/>.</typeparam>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        protected async Task<IEnumerable<long>> LoadLinkedDataAsync<T1, T2>(string dataAbbreviation)
            where T1 : MSDataObject<T2>
            where T2 : MSDatabaseObject
        {
            List<long> links = new ();
            if (await this.GenerateHashAsync().ConfigureAwait(false) is not null)
            {
                await new PipelineItem(this.Pipeline)
                {
                    BatchCommand = this.LoadLinkedDataBatchCommand(dataAbbreviation),
                    ReaderExecution = (NpgsqlDataReader reader) =>
                    {
                        do
                        {
                            if (!reader.IsDBNull(1))
                            {
                                links.Add(reader.GetInt64(1));
                            }
                        }
                        while (reader.Read());
                    },
                }.ExecuteAsync().ConfigureAwait(false);
            }

            return links.ToArray();
        }

        private Func<PipelineItem, NpgsqlCommand, bool> LoadLinkedDataBatchCommand(string dataAbbreviation)
        {
            return (PipelineItem item, NpgsqlCommand command) =>
            {
                if (this.GetAccessParent().Hash is byte[] hash)
                {
                    command.CommandText += new StringBuilder("SELECT ")
                        .Append(item.AddParameter(command, "itemid", NpgsqlDbType.Integer, item.ID))
                        .AppendFormat(", * FROM load{0}{1}linkeddata(", this.Abbreviation, dataAbbreviation)
                        .Append(item.AddParameter(command, "child", NpgsqlDbType.Bytea, hash))
                        .Append(");");
                    return true;
                }

                return false;
            };
        }

        private bool SaveDataBatchCommand(PipelineItem item, NpgsqlCommand command)
        {
            if (this.Access is not null
                && this.modifier?.PrivateSignature is not null
                && this.data is not null
                && BitConverter.GetBytes(DateTime.Now.Ticks) is byte[] timeArray
                && BitConverter.GetBytes(this.modifier.ID) is byte[] modifierArray)
            {
                command.CommandText += new StringBuilder("SELECT ")
                    .Append(item.AddParameter(command, "itemid", NpgsqlDbType.Integer, item.ID))
                    .AppendFormat(", save{0}data(", this.Abbreviation)
                    .Append(item.AddParameter(command, "id", NpgsqlDbType.Bigint, this.ID))
                    .Append(',')
                    .Append(item.AddParameter(command, "data", NpgsqlDbType.Bytea, this.Access.EncryptCbc(this.data, this.Access.IV)))
                    .Append(',')
                    .Append(item.AddParameter(command, "modifier", NpgsqlDbType.Bytea, this.Access.EncryptCbc(timeArray, this.Access.IV)))
                    .Append(',')
                    .Append(item.AddParameter(command, "modificationtime", NpgsqlDbType.Bytea, this.Access.EncryptCbc(modifierArray, this.Access.IV)))
                    .Append(',')
                    .Append(item.AddParameter(command, "dataverification", NpgsqlDbType.Bytea, this.modifier.PrivateSignature.SignData(this.data.Concat(modifierArray).Concat(timeArray).ToArray(), Pipeline.HashAlgorithmName, Pipeline.RSASignaturePadding)))
                    .Append(");");
                return true;
            }

            return false;
        }

        private void GetAccessReaderExecution(NpgsqlDataReader reader)
        {
            if (!reader.IsDBNull(1)
                && this.Parent.Access is not null)
            {
                this.access = Aes.Create().ImportKey(this.Parent.Access.Key, this.Parent.Access.DecryptCbc((byte[])reader[1], this.Parent.Access.IV));
                this.OnPropertyChanged(new PropertyChangedEventArgs(nameof(this.Access)));
            }
        }

        private void GetDataReaderExecution(NpgsqlDataReader reader)
        {
            if (!reader.IsDBNull(1)
                && this.access is not null)
            {
                this.data = this.access.DecryptCbc((byte[])reader[1], this.access.IV);
                this.OnPropertyChanged(new PropertyChangedEventArgs(nameof(this.Data)));
            }
        }

        private void GetModificationTimeReaderExecution(NpgsqlDataReader reader)
        {
            if (!reader.IsDBNull(1)
                && this.Access is not null)
            {
                this.modificationTime = new DateTime(BitConverter.ToInt64(this.Access.DecryptCbc((byte[])reader[1], this.Access.IV), 0));
                this.OnPropertyChanged(new PropertyChangedEventArgs(nameof(this.ModificationTime)));
            }
        }

        private void GetModifierReaderExecution(NpgsqlDataReader reader)
        {
            if (!reader.IsDBNull(1)
                && this.Access is not null)
            {
                this.modifier = new (this.GetAccessParent().Association, BitConverter.ToInt64(this.Access.DecryptCbc((byte[])reader[1], this.Access.IV), 0));
                this.OnPropertyChanged(new PropertyChangedEventArgs(nameof(this.Modifier)));
            }
        }

        private void VerifyDataReaderExecution(NpgsqlDataReader reader)
        {
            if (!reader.IsDBNull(1)
                && this.modifier?.PublicSignature is not null
                && this.modificationTime is not null
                && this.data is not null)
            {
                this.dataVerification = this.modifier.PublicSignature.VerifyData(this.data.Concat(BitConverter.GetBytes(this.modifier.ID)).Concat(BitConverter.GetBytes(this.modificationTime.Value.Ticks)).ToArray(), (byte[])reader[1], Pipeline.HashAlgorithmName, Pipeline.RSASignaturePadding);
                this.OnPropertyChanged(new PropertyChangedEventArgs(nameof(this.DataVerification)));
            }
        }
    }
}
