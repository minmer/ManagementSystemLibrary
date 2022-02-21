// <copyright file="MSDatabaseObject.cs" company="PlaceholderCompany">
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
    /// An database object for the management system.
    /// </summary>
    public abstract class MSDatabaseObject : INotifyPropertyChanged
    {
        private DateTime? creationTime;
        private AMSAccount? creator;
        private bool? creatorVerification;
        private byte[]? hash;
        private byte[]? publicHash;

        /// <summary>
        /// Initializes a new instance of the <see cref="MSDatabaseObject"/> class.
        /// </summary>
        /// <param name="pipeline">The <see cref="Pipeline"/> of the <see cref="MSDatabaseObject"/>.</param>
        /// <param name="id">The identifier of the <see cref="MSDatabaseObject"/>.</param>
        public MSDatabaseObject(Pipeline pipeline, long id)
        {
            this.Pipeline = pipeline ?? throw new ArgumentNullException(nameof(pipeline));
            this.ID = id;
        }

        /// <inheritdoc/>
        public event PropertyChangedEventHandler? PropertyChanged;

        /// <summary>
        /// Gets the string representation in the database of the <see cref="MSDatabaseObject"/>.
        /// </summary>
        public string Abbreviation { get => this.GetType().GetDatabaseAbbreviation(); }

        /// <summary>
        /// Gets the hash representation in the database of the <see cref="MSDatabaseObject"/>.
        /// </summary>
        public byte[] AbbreviationHash { get => this.GetType().GetDatabaseAbbreviationHash(); }

        /// <summary>
        /// Gets the access <see cref="Aes"/> of the <see cref="MSDatabaseObject"/>.
        /// </summary>
        public abstract Aes? Access { get; }

        /// <summary>
        /// Gets the creationtime of the <see cref="MSDatabaseObject"/>.
        /// </summary>
        public DateTime? CreationTime
        {
            get
            {
                _ = this.GetCreationTimeAsync();
                return this.creationTime;
            }
        }

        /// <summary>
        /// Gets the creator of the <see cref="MSDatabaseObject"/>.
        /// </summary>
        public AMSAccount? Creator
        {
            get
            {
                _ = this.GetCreatorAsync();
                return this.creator;
            }
        }

        /// <summary>
        /// Gets a value indicating whether the creator of the <see cref="MSDatabaseObject"/> is verified.
        /// </summary>
        public bool? CreatorVerification
        {
            get
            {
                _ = this.VerifyCreatorAsync();
                return this.creatorVerification;
            }
        }

        /// <summary>
        /// Gets the hash representation of the <see cref="MSDatabaseObject"/>.
        /// </summary>
        public byte[]? Hash
        {
            get
            {
                _ = this.GenerateHashAsync();
                return this.hash;
            }
        }

        /// <summary>
        /// Gets the identifier of the <see cref="MSDatabaseObject"/>.
        /// </summary>
        public long ID { get; }

        /// <summary>
        /// Gets the <see cref="Pipeline"/> of the <see cref="MSDatabaseObject"/>.
        /// </summary>
        public Pipeline Pipeline { get; }

        /// <summary>
        /// Gets the public hash representation of the <see cref="MSDatabaseObject"/>.
        /// </summary>
        public byte[] PublicHash
        {
            get
            {
                if (this.publicHash is null)
                {
                    this.publicHash = SHA256.HashData(this.AbbreviationHash.Concat(BitConverter.GetBytes(this.ID)).ToArray());
                }

                return this.publicHash;
            }
        }

        /// <summary>
        /// Geneartes the <see cref="Hash"/> of the <see cref="MSDatabaseObject"/>.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task<byte[]?> GenerateHashAsync()
        {
            if (await this.GetAccessAsync().ConfigureAwait(false) is Aes access
                && this.hash is null)
            {
                this.hash = SHA256.HashData(access.IV.Concat(this.AbbreviationHash).Concat(BitConverter.GetBytes(this.ID)).ToArray());
                this.OnPropertyChanged(new PropertyChangedEventArgs(nameof(this.Hash)));
            }

            return this.hash;
        }

        /// <summary>
        /// Gets the <see cref="Access"/> of the <see cref="MSDatabaseObject"/>.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public abstract Task<Aes?> GetAccessAsync();

        /// <summary>
        /// Gets the <see cref="CreationTime"/> of the <see cref="MSDatabaseObject"/>.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task<DateTime?> GetCreationTimeAsync()
        {
            if (this.creationTime is null
                && await this.GetAccessAsync().ConfigureAwait(false) is not null)
            {
                await new PipelineItem(this.Pipeline)
                {
                    BatchCommand = this.BasicGetByIDBatchCommand("get", "creationtime"),
                    ReaderExecution = this.GetCreationTimeReaderExecution,
                }.ExecuteAsync().ConfigureAwait(false);
            }

            return this.creationTime;
        }

        /// <summary>
        /// Gets the <see cref="Creator"/> of the <see cref="MSDatabaseObject"/>.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task<AMSAccount?> GetCreatorAsync()
        {
            if (this.creator is null
                && await this.GetAccessAsync().ConfigureAwait(false) is not null)
            {
                await new PipelineItem(this.Pipeline)
                {
                    BatchCommand = this.BasicGetByIDBatchCommand("get", "creator"),
                    ReaderExecution = this.GetCreatorReaderExecution,
                }.ExecuteAsync().ConfigureAwait(false);
            }

            return this.creator;
        }

        /// <summary>
        /// Verifies the <see cref="Creator"/> of the <see cref="MSDatabaseObject"/>.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task<bool?> VerifyCreatorAsync()
        {
            if (await this.GetCreationTimeAsync().ConfigureAwait(false) is not null
                && await this.GetCreatorAsync().ConfigureAwait(false) is AMSAccount creator)
            {
                if (await creator.GetPublicSignatureAsync().ConfigureAwait(false) is not null)
                {
                    await new PipelineItem(this.Pipeline)
                    {
                        BatchCommand = this.BasicGetByIDBatchCommand("verify", "creator"),
                        ReaderExecution = this.VerifyCreatorReaderExecution,
                    }.ExecuteAsync().ConfigureAwait(false);
                }
            }

            return this.creatorVerification;
        }

        /// <summary>
        /// Removes the <see cref="MSDatabaseObject"/>.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public virtual async Task RemoveAsync()
        {
            await new PipelineItem(this.Pipeline)
            {
                BatchCommand = this.RemoveBatchCommand,
            }.ExecuteAsync().ConfigureAwait(false);
        }

        /// <summary>
        /// Loads the <see cref="MSDataObject{T}"/> related to the <see cref="MSDatabaseObject"/>.
        /// </summary>
        /// <typeparam name="T1">The type of the <see cref="MSDataObject{T}"/>.</typeparam>
        /// <typeparam name="T2">The item type of the <see cref="MSDatabaseObject"/>.</typeparam>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        internal async Task<IEnumerable<long>> LoadItemsAsync<T1, T2>()
            where T1 : MSDatabaseObject
            where T2 : MSDatabaseObject
        {
            List<long> items = new ();
            if (await this.GenerateHashAsync().ConfigureAwait(false) is not null
                && this is T2 parent)
            {
                await new PipelineItem(this.Pipeline)
                {
                    BatchCommand = this.LoadItemsBatchCommand(typeof(T1).GetDatabaseAbbreviation()),
                    ReaderExecution = (NpgsqlDataReader reader) =>
                    {
                        do
                        {
                            if (!reader.IsDBNull(1))
                            {
                                items.Add(reader.GetInt64(1));
                            }
                        }
                        while (reader.Read());
                    },
                }.ExecuteAsync().ConfigureAwait(false);
            }

            return items.ToArray();
        }

        /// <summary>
        /// Loads the <see cref="MSDataObject{T}"/> related to the <see cref="MSDatabaseObject"/>.
        /// </summary>
        /// <typeparam name="T1">The type of the <see cref="MSDataObject{T}"/>.</typeparam>
        /// <typeparam name="T2">The item type of the <see cref="MSDatabaseObject"/>.</typeparam>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        internal async Task<IEnumerable<long>> LoadPublicItemsAsync<T1, T2>()
            where T1 : MSDatabaseObject
            where T2 : MSDatabaseObject
        {
            List<long> items = new ();
            if (this is T2)
            {
                await new PipelineItem(this.Pipeline)
                {
                    BatchCommand = this.LoadPublicItemsBatchCommand(typeof(T1).GetDatabaseAbbreviation()),
                    ReaderExecution = (NpgsqlDataReader reader) =>
                    {
                        do
                        {
                            if (!reader.IsDBNull(1))
                            {
                                items.Add(reader.GetInt64(1));
                            }
                        }
                        while (reader.Read());
                    },
                }.ExecuteAsync().ConfigureAwait(false);
            }

            return items.ToArray();
        }

        /// <summary>
        /// Creates a new <see cref="MSDatabaseObject"/>.
        /// </summary>
        /// <param name="creator">The creator <see cref="AMSAccount"/>.</param>
        /// <param name="access">The access of the  <see cref="MSDatabaseObject"/>.</param>
        /// <param name="parameterMethod">A method for additional parameters.</param>
        /// <typeparam name="T">Type of the created object.</typeparam>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        protected static async Task<long?> CreateAsync<T>(AMSAssociation creator, Aes access, Action<PipelineItem, NpgsqlCommand, DateTime, AMSAssociation, Aes, StringBuilder> parameterMethod)
        {
            if (0L is long id
                && ((creator.AccessType <= MSAccessType.Contributor)
                | creator.ID == -1
                | typeof(T) == typeof(AMSDevice)))
            {
                await new PipelineItem(creator.Pipeline)
                {
                    BatchCommand = CreateBatchCommand<T>(creator, access, parameterMethod),
                    ReaderExecution = (NpgsqlDataReader reader) =>
                    {
                        if (!reader.IsDBNull(1))
                        {
                            id = reader.GetInt64(1);
                        }
                    },
                }.ExecuteAsync().ConfigureAwait(false);
                return id;
            }

            return null;
        }

        /// <summary>
        /// Generate a basic select by ID sql query.
        /// </summary>
        /// <param name="type">The type of the query.</param>
        /// <param name="column">The name of the column.</param>
        /// <returns>The BatchCommand.</returns>
        protected Func<PipelineItem, NpgsqlCommand, bool> BasicGetByIDBatchCommand(string type, string column)
        {
            return (PipelineItem item, NpgsqlCommand command) =>
            {
                command.CommandText += new StringBuilder("SELECT ")
                    .Append(item.AddParameter(command, "itemid", NpgsqlDbType.Integer, item.ID))
                    .AppendFormat(", {0}{1}{2}(", type, this.Abbreviation, column)
                    .Append(item.AddParameter(command, "id", NpgsqlDbType.Bigint, this.ID))
                    .Append(");");
                return true;
            };
        }

        /// <summary>
        /// Invokes the PropertyChanged of the <see cref="MSDatabaseObject"/>.
        /// </summary>
        /// <param name="eventArgs">The PropertyChangedEventArgs of the mehtod.</param>
        protected virtual void OnPropertyChanged(PropertyChangedEventArgs eventArgs)
        {
            this.PropertyChanged?.Invoke(this, eventArgs);
        }

        private static Func<PipelineItem, NpgsqlCommand, bool> CreateBatchCommand<T>(AMSAssociation creator, Aes access, Action<PipelineItem, NpgsqlCommand, DateTime, AMSAssociation, Aes, StringBuilder>? parameterMethod)
        {
            return (PipelineItem item, NpgsqlCommand command) =>
            {
                if (DateTime.Now is DateTime creationTime
                    && BitConverter.GetBytes(creator.Account.ID) is byte[] creatorArray
                    && new StringBuilder("SELECT ") is StringBuilder builder)
                {
                    builder.Append(item.AddParameter(command, "itemid", NpgsqlDbType.Integer, item.ID))
                    .Append(", create")
                    .Append(typeof(T).GetDatabaseAbbreviation())
                    .Append('(')
                    .Append(item.AddParameter(command, "creationtime", NpgsqlDbType.Bytea, access.EncryptCbc(BitConverter.GetBytes(creationTime.Ticks), access.IV)))
                    .Append(',')
                    .Append(item.AddParameter(command, "creator", NpgsqlDbType.Bytea, access.EncryptCbc(creatorArray, access.IV)))
                    .Append(',')
                    .Append(item.AddParameter(command, "creatorverification", NpgsqlDbType.Bytea, creator.PrivateSignature.SignData(creatorArray.Concat(BitConverter.GetBytes(creationTime.Ticks)).ToArray(), Pipeline.HashAlgorithmName, Pipeline.RSASignaturePadding)));
                    parameterMethod?.Invoke(item, command, creationTime, creator, access, builder);
                    command.CommandText += builder.Append(");");
                    return true;
                }

                return false;
            };
        }

        private Func<PipelineItem, NpgsqlCommand, bool> LoadItemsBatchCommand(string itemAbbreviation)
        {
            return (PipelineItem item, NpgsqlCommand command) =>
            {
                if (this.Hash is not null)
                {
                    command.CommandText += new StringBuilder("SELECT ")
                        .Append(item.AddParameter(command, "itemid", NpgsqlDbType.Integer, item.ID))
                        .AppendFormat(", load{0}items(", itemAbbreviation)
                        .Append(item.AddParameter(command, "parent", NpgsqlDbType.Bytea, this.Hash))
                        .Append(");");
                    return true;
                }

                return false;
            };
        }

        private Func<PipelineItem, NpgsqlCommand, bool> LoadPublicItemsBatchCommand(string itemAbbreviation)
        {
            return (PipelineItem item, NpgsqlCommand command) =>
            {
                command.CommandText += new StringBuilder("SELECT ")
                    .Append(item.AddParameter(command, "itemid", NpgsqlDbType.Integer, item.ID))
                    .AppendFormat(", load{0}items(", itemAbbreviation)
                    .Append(item.AddParameter(command, "parent", NpgsqlDbType.Bytea, this.PublicHash))
                    .Append(");");
                return true;
            };
        }

        private bool RemoveBatchCommand(PipelineItem item, NpgsqlCommand command)
        {
            if (this.Hash is not null)
            {
                command.CommandText += new StringBuilder("SELECT ")
                    .Append(item.AddParameter(command, "itemid", NpgsqlDbType.Integer, item.ID))
                    .AppendFormat(", remove{0}(", this.Abbreviation)
                    .Append(item.AddParameter(command, "id", NpgsqlDbType.Bigint, this.ID))
                    .Append(");");
                return true;
            }

            return false;
        }

        private void GetCreationTimeReaderExecution(NpgsqlDataReader reader)
        {
            if (!reader.IsDBNull(1)
                && this.Access is not null)
            {
                this.creationTime = new DateTime(BitConverter.ToInt64(this.Access.DecryptCbc((byte[])reader[1], this.Access.IV), 0));
                this.OnPropertyChanged(new PropertyChangedEventArgs(nameof(this.CreationTime)));
            }
        }

        private void GetCreatorReaderExecution(NpgsqlDataReader reader)
        {
            if (!reader.IsDBNull(1)
                && this.Access is not null
                && ((this as MSAccessObject)?.Association
                ?? (this as MSDataObject<MSDatabaseObject>)?.GetAccessParent().Association
                ?? (this as MSLinkObject<MSAccessObject, MSAccessObject>)?.Parent?.Association
                ?? (this as MSLinkObject<MSAccessObject, MSAccessObject>)?.Child?.Association
                ?? new AMSAssociation(this.Pipeline, -1)) is AMSAssociation association)
            {
                this.creator = new AMSAccount(association, BitConverter.ToInt64(this.Access.DecryptCbc((byte[])reader[1], this.Access.IV), 0));
                if (this is MSAccessObject accessObject)
                {
                    if (accessObject.Account.ID == this.creator.ID
                        && accessObject.AccessType <= MSAccessType.Administrator)
                    {
                        accessObject.AccessType = MSAccessType.Creator;
                    }
                }

                this.OnPropertyChanged(new PropertyChangedEventArgs(nameof(this.Creator)));
            }
        }

        private void VerifyCreatorReaderExecution(NpgsqlDataReader reader)
        {
            if (!reader.IsDBNull(1)
                && this.creator?.PublicSignature is not null
                && this.creationTime is not null)
            {
                this.creatorVerification = this.creator.PublicSignature.VerifyData(BitConverter.GetBytes(this.creator.ID).Concat(BitConverter.GetBytes(this.creationTime.Value.Ticks)).ToArray(), (byte[])reader[1], Pipeline.HashAlgorithmName, Pipeline.RSASignaturePadding);
                this.OnPropertyChanged(new PropertyChangedEventArgs(nameof(this.CreatorVerification)));
            }
        }
    }
}
