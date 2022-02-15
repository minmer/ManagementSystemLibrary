// <copyright file="BMSBudget.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>
/*
namespace ManagementSystemLibrary.RMS
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using ManagementSystemLibrary.AMS;
    using ManagementSystemLibrary.Pipeline;
    using Npgsql;
    using NpgsqlTypes;
    using PCLCrypto;

    /// <summary>
    /// Represents a shared <see cref="RMSRecord"/>.
    /// </summary>
    public class RMSSharedRecord : INotifyPropertyChanged
    {
        private DateTime? creationtime;
        private AMSAssociation? receiver;
        private object? data;
        private string? description;
        private ICryptographicKey? accessKey;

        /// <summary>
        /// Initializes a new instance of the <see cref="RMSSharedRecord"/> class.
        /// </summary>
        /// <param name="id">The identifier of the <see cref="RMSSharedRecord"/>.</param>
        /// <param name="sender">The sender of the <see cref="RMSSharedRecord"/>.</param>
        /// <param name="receiver">The receiver of the <see cref="RMSSharedRecord"/>.</param>
        public RMSSharedRecord(long id, AMSAssociation sender, AMSAssociation receiver)
        {
            this.ID = id;
            this.Sender = sender ?? throw new ArgumentNullException(nameof(sender));
            this.receiver = receiver;
        }

        /// <inheritdoc/>
        public event PropertyChangedEventHandler? PropertyChanged;

        /// <summary>
        /// Gets the identifier of the <see cref="RMSSharedRecord"/>.
        /// </summary>
        public long id { get; }

        /// <summary>
        /// Gets the sender of the <see cref="RMSSharedRecord"/>.
        /// </summary>
        public AMSAssociation Sender { get; }

        /// <summary>
        /// Gets the description of the <see cref="RMSSharedRecord"/>.
        /// </summary>
        public string Description
        {
            get
            {
                _ = this.GetDescriptionAsync();
                return this.description ?? string.Empty;
            }
        }

        /// <summary>
        /// Gets the creeationtime of the <see cref="RMSSharedRecord"/>.
        /// </summary>
        public DateTime? Creationtime
        {
            get
            {
                _ = this.GetCreationtimeAsync();
                return this.creationtime;
            }
        }

        /// <summary>
        /// Gets the receiver of the <see cref="RMSSharedRecord"/>.
        /// </summary>
        public AMSAssociation? Receiver
        {
            get
            {
                _ = this.GetReceiverAsync();
                return this.receiver;
            }
        }

        /// <summary>
        /// Gets the data of the <see cref="RMSSharedRecord"/>.
        /// </summary>
        public object? Data
        {
            get
            {
                _ = this.GetDataAsync();
                return this.data;
            }
        }

        /// <summary>
        /// Gets the access <see cref="ICryptographicKey"/> for the <see cref="RMSSharedRecord"/>.
        /// </summary>
        public ICryptographicKey? AccessKey
        {
            get
            {
                _ = this.GetAccessKeyAsync();
                return this.accessKey;
            }
        }

        /// <summary>
        /// Creates a new <see cref="RMSSharedRecord"/>.
        /// </summary>
        /// <param name="sender">The sender of the <see cref="RMSSharedRecord"/>.</param>
        /// <param name="receiver">The receiver of the <see cref="RMSSharedRecord"/>.</param>
        /// <param name="description">The description of the <see cref="RMSSharedRecord"/>.</param>
        /// <param name="data">The data of the <see cref="RMSSharedRecord"/>.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public static async Task<RMSSharedRecord?> CreateAsync(AMSAssociation sender, AMSAssociation receiver, string description, object data)
        {
            if (sender.AccessKey != null && !string.IsNullOrWhiteSpace(description) && await sender.GetSeedAsync().ConfigureAwait(false) != 0 && await receiver.GetPublicKeyAsync().ConfigureAwait(false) != null)
            {
                long id = 0;
                await new PipelineItem(sender.Pipeline)
                {
                    BatchCommand = CreateBatchCommand(sender, receiver, description, data),
                    ReaderExecution = (NpgsqlDataReader reader) =>
                    {
                        if (!reader.IsDBNull(1))
                        {
                            id = reader.GetInt64(1);
                        }
                    },
                }.ExecuteAsync().ConfigureAwait(false);
                return new RMSSharedRecord(id, sender, receiver);
            }

            return null;
        }

        /// <summary>
        /// Searches a <see cref="RMSSharedRecord"/>.
        /// </summary>
        /// <param name="sender">The sender of the <see cref="RMSSharedRecord"/>.</param>
        /// <param name="receiver">The receiver of the <see cref="RMSSharedRecord"/>.</param>
        /// <param name="account">The <see cref="AMSAccount"/> that searches a <see cref="RMSSharedRecord"/>.</param>
        /// <param name="description">The description of the <see cref="RMSSharedRecord"/>.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public static async Task<RMSSharedRecord?> SearchAsync(AMSAssociation sender, AMSAssociation receiver, AMSAccount account, string description)
        {
            if ((!string.IsNullOrWhiteSpace(description)) && (await account.GetPublicKeyAsync().ConfigureAwait(false) != null) && (0L is long id))
            {
                await new PipelineItem(account.Pipeline)
                {
                    BatchCommand = SearchBatchCommand(sender, receiver, account, description),
                    ReaderExecution = (NpgsqlDataReader reader) =>
                    {
                        if (!reader.IsDBNull(1))
                        {
                            id = reader.GetInt64(1);
                        }
                    },
                }.ExecuteAsync().ConfigureAwait(false);
                if (id != 0)
                {
                    return new RMSSharedRecord(id, sender, receiver);
                }
            }

            return null;
        }

        /// <summary>
        /// Gets the receiver of the <see cref="RMSSharedRecord"/>.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task<AMSAssociation?> GetReceiverAsync()
        {
            if (this.receiver is null && this.Sender?.AccessKey != null)
            {
                await new PipelineItem(this.Sender.Pipeline)
                {
                    BatchCommand = this.GetReceiverBatchCommand,
                    ReaderExecution = this.GetReceiverReaderExecution,
                }.ExecuteAsync().ConfigureAwait(false);
            }

            return this.receiver;
        }

        /// <summary>
        /// Gets the name of the <see cref="RMSSharedRecord"/>.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task<object?> GetDataAsync()
        {
            if (this.data is null)
            {
                if (await this.GetAccessKeyAsync().ConfigureAwait(false) != null)
                {
#pragma warning disable CS8604 // Possible null reference argument.
                    await new PipelineItem(this.Sender?.Pipeline ?? this.receiver?.Pipeline)
#pragma warning restore CS8604 // Possible null reference argument.
                    {
                        BatchCommand = this.GetDataBatchCommand,
                        ReaderExecution = this.GetDataReaderExecution,
                    }.ExecuteAsync().ConfigureAwait(false);
                }
            }

            return this.data;
        }

        /// <summary>
        /// Gets the description of the <see cref="RMSSharedRecord"/>.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task<string?> GetDescriptionAsync()
        {
            if (this.description is null)
            {
                if (this.Sender?.AccessKey != null)
                {
                    await new PipelineItem(this.Sender.Pipeline)
                    {
                        BatchCommand = this.GetDescriptionBatchCommand,
                        ReaderExecution = this.GetDescriptionReaderExecution,
                    }.ExecuteAsync().ConfigureAwait(false);
                }
            }

            return this.description;
        }

        /// <summary>
        /// Gets the creationtime of the <see cref="RMSSharedRecord"/>.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task<DateTime?> GetCreationtimeAsync()
        {
            if (this.creationtime == DateTime.MinValue)
            {
                if (this.AccessKey != null)
                {
#pragma warning disable CS8604 // Possible null reference argument.
                    await new PipelineItem(this.Sender?.Pipeline ?? this.receiver?.Pipeline)
#pragma warning restore CS8604 // Possible null reference argument.
                    {
                        BatchCommand = this.GetCreationtimeBatchCommand,
                        ReaderExecution = this.GetCreationtimeReaderExecution,
                    }.ExecuteAsync().ConfigureAwait(false);
                }
            }

            return this.creationtime;
        }

        /// <summary>
        /// Gets the access <see cref="ICryptographicKey"/> of the <see cref="RMSSharedRecord"/>.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task<ICryptographicKey?> GetAccessKeyAsync()
        {
            if (this.accessKey is null)
            {
                if (this.Sender?.AccessKey != null)
                {
                    await new PipelineItem(this.Sender.Pipeline)
                    {
                        BatchCommand = this.GetAccessKeyBySenderBatchCommand,
                        ReaderExecution = this.GetAccessKeyBySenderReaderExecution,
                    }.ExecuteAsync().ConfigureAwait(false);
                }
                else if (this.receiver?.PrivateKey != null)
                {
                    await new PipelineItem(this.receiver.Pipeline)
                    {
                        BatchCommand = this.GetAccessKeyByReceiverBatchCommand,
                        ReaderExecution = this.GetAccessKeyByReceiverReaderExecution,
                    }.ExecuteAsync().ConfigureAwait(false);
                }
                else
                {
                }
            }

            return this.accessKey;
        }

        /// <summary>
        /// Invokes the PropertyChanged of the <see cref="RMSSharedRecord"/>.
        /// </summary>
        /// <param name="eventArgs">The PropertyChangedEventArgs of the mehtod.</param>
        protected virtual void OnPropertyChanged(PropertyChangedEventArgs eventArgs)
        {
            this.PropertyChanged?.Invoke(this, eventArgs.ThrowIfNull(nameof(eventArgs)));
        }

        private static Func<PipelineItem, NpgsqlCommand, bool> CreateBatchCommand(AMSAssociation sender, AMSAssociation receiver, string description, object data)
        {
            return (PipelineItem item, NpgsqlCommand command) =>
            {
                byte[] keyMaterial = WinRTCrypto.CryptographicBuffer.GenerateRandom(Pipeline.SymetricProvider.BlockLength);
                ICryptographicKey key = Pipeline.SymetricProvider.CreateSymmetricKey(keyMaterial);
                command.CommandText += new StringBuilder("SELECT ")
                .Append(item.AddParameter(command, "itemid", NpgsqlDbType.Integer, item.ID))
                .Append(", createsharedrecord(")
                .Append(item.AddParameter(command, "sender", NpgsqlDbType.Bytea, Pipeline.HashProvider.HashData(new byte[] { 1 }.PrefixLong(sender.Seed).PrefixInt(sender.ID).ToArray())))
                .Append(",")
                .Append(item.AddParameter(command, "receiver", NpgsqlDbType.Bytea, WinRTCrypto.CryptographicEngine.Encrypt(sender.AccessKey, BitConverter.GetBytes(receiver.ID))))
                .Append(",")
                .Append(item.AddParameter(command, "name", NpgsqlDbType.Bytea, Pipeline.HashProvider.HashData(Encoding.Unicode.GetBytes(description).PrefixInt(receiver.ID).PrefixInt(sender.ID).ToArray())))
                .Append(",")
                .Append(item.AddParameter(command, "description", NpgsqlDbType.Bytea, WinRTCrypto.CryptographicEngine.Encrypt(sender.AccessKey, Encoding.Unicode.GetBytes(description))))
                .Append(",")
                .Append(item.AddParameter(command, "data", NpgsqlDbType.Bytea, WinRTCrypto.CryptographicEngine.Encrypt(key, RMSRecord.GetDataAsByteArray(data).ToArray())))
                .Append(",")
                .Append(item.AddParameter(command, "creationtime", NpgsqlDbType.Bytea, WinRTCrypto.CryptographicEngine.Encrypt(key, BitConverter.GetBytes(DateTime.Now.Ticks))))
                .Append(",")
                .Append(item.AddParameter(command, "senderkey", NpgsqlDbType.Bytea, WinRTCrypto.CryptographicEngine.Encrypt(sender.AccessKey, keyMaterial)))
                .Append(",")
                .Append(item.AddParameter(command, "receiverkey", NpgsqlDbType.Bytea, WinRTCrypto.CryptographicEngine.Encrypt(receiver.PublicKey, keyMaterial)))
                .Append(");");
                return true;
            };
        }

        private static Func<PipelineItem, NpgsqlCommand, bool> SearchBatchCommand(AMSAssociation sender, AMSAssociation receiver, AMSAccount account, string description)
        {
            return (PipelineItem item, NpgsqlCommand command) =>
            {
                command.CommandText += new StringBuilder("SELECT ")
                .Append(item.AddParameter(command, "itemid", NpgsqlDbType.Integer, item.ID))
                .Append(", searchsharedrecord(")
                .Append(item.AddParameter(command, "name", NpgsqlDbType.Bytea, Pipeline.HashProvider.HashData(Encoding.Unicode.GetBytes(description).PrefixInt(receiver.ID).PrefixInt(sender.ID).ToArray())))
                .Append(",")
                .Append(item.AddParameter(command, "record", NpgsqlDbType.Bytea, Pipeline.HashProvider.HashData(Encoding.Unicode.GetBytes(description).PrefixInt(sender.ID).PrefixLong(receiver.ID).ToArray())))
                .Append(",")
                .Append(item.AddParameter(command, "account", NpgsqlDbType.Bytea, WinRTCrypto.CryptographicEngine.Encrypt(sender.PublicKey, BitConverter.GetBytes(account.ID))))
                .Append(");");
                return true;
            };
        }

        private bool GetReceiverBatchCommand(PipelineItem item, NpgsqlCommand command)
        {
            command.CommandText += new StringBuilder("SELECT ")
                .Append(item.AddParameter(command, "itemid", NpgsqlDbType.Integer, item.ID))
                .Append(", getsharedrecordreceiver(")
                .Append(item.AddParameter(command, "id", NpgsqlDbType.Integer, this.ID))
                .Append(");");
            return true;
        }

        private bool GetAccessKeyBySenderBatchCommand(PipelineItem item, NpgsqlCommand command)
        {
            command.CommandText += new StringBuilder("SELECT ")
                .Append(item.AddParameter(command, "itemid", NpgsqlDbType.Integer, item.ID))
                .Append(", getsharedrecordsenderkey(")
                .Append(item.AddParameter(command, "id", NpgsqlDbType.Integer, this.ID))
                .Append(");");
            return true;
        }

        private bool GetAccessKeyByReceiverBatchCommand(PipelineItem item, NpgsqlCommand command)
        {
            command.CommandText += new StringBuilder("SELECT ")
                .Append(item.AddParameter(command, "itemid", NpgsqlDbType.Integer, item.ID))
                .Append(", getsharedrecordreceiverkey(")
                .Append(item.AddParameter(command, "id", NpgsqlDbType.Integer, this.ID))
                .Append(");");
            return true;
        }

        private bool GetDataBatchCommand(PipelineItem item, NpgsqlCommand command)
        {
            command.CommandText += new StringBuilder("SELECT ")
                .Append(item.AddParameter(command, "itemid", NpgsqlDbType.Integer, item.ID))
                .Append(", getsharedrecorddata(")
                .Append(item.AddParameter(command, "id", NpgsqlDbType.Integer, this.ID))
                .Append(");");
            return true;
        }

        private bool GetDescriptionBatchCommand(PipelineItem item, NpgsqlCommand command)
        {
            command.CommandText += new StringBuilder("SELECT ")
                .Append(item.AddParameter(command, "itemid", NpgsqlDbType.Integer, item.ID))
                .Append(", getsharedrecorddescription(")
                .Append(item.AddParameter(command, "id", NpgsqlDbType.Integer, this.ID))
                .Append(");");
            return true;
        }

        private bool GetCreationtimeBatchCommand(PipelineItem item, NpgsqlCommand command)
        {
            command.CommandText += new StringBuilder("SELECT ")
                .Append(item.AddParameter(command, "itemid", NpgsqlDbType.Integer, item.ID))
                .Append(", getsharedrecordcreationtime(")
                .Append(item.AddParameter(command, "id", NpgsqlDbType.Integer, this.ID))
                .Append(");");
            return true;
        }

        private void GetReceiverReaderExecution(NpgsqlDataReader reader)
        {
            if (!reader.IsDBNull(1))
            {
                this.receiver = new AMSAssociation(this.Sender.Pipeline, BitConverter.ToInt64(WinRTCrypto.CryptographicEngine.Decrypt(this.Sender.AccessKey, (byte[])reader[1]), 0));
                this.OnPropertyChanged(new PropertyChangedEventArgs(nameof(this.Receiver)));
            }
        }

        private void GetAccessKeyBySenderReaderExecution(NpgsqlDataReader reader)
        {
            if (!reader.IsDBNull(1))
            {
                this.accessKey = Pipeline.SymetricProvider.CreateSymmetricKey(WinRTCrypto.CryptographicEngine.Decrypt(this.Sender?.AccessKey, (byte[])reader[1]));
                this.OnPropertyChanged(new PropertyChangedEventArgs(nameof(this.AccessKey)));
            }
        }

        private void GetAccessKeyByReceiverReaderExecution(NpgsqlDataReader reader)
        {
            if (!reader.IsDBNull(1))
            {
                this.accessKey = Pipeline.SymetricProvider.CreateSymmetricKey(WinRTCrypto.CryptographicEngine.Decrypt(this.receiver?.PrivateKey, (byte[])reader[1]));
                this.OnPropertyChanged(new PropertyChangedEventArgs(nameof(this.AccessKey)));
            }
        }

        private void GetDataReaderExecution(NpgsqlDataReader reader)
        {
            if (!reader.IsDBNull(1))
            {
                this.data = RMSRecord.GetDataAsObject(WinRTCrypto.CryptographicEngine.Decrypt(this.AccessKey, (byte[])reader[1]));
                this.OnPropertyChanged(new PropertyChangedEventArgs(nameof(this.Data)));
            }
        }

        private void GetCreationtimeReaderExecution(NpgsqlDataReader reader)
        {
            if (!reader.IsDBNull(1))
            {
                this.creationtime = new DateTime(BitConverter.ToInt64(WinRTCrypto.CryptographicEngine.Decrypt(this.AccessKey, (byte[])reader[1]), 0));
                this.OnPropertyChanged(new PropertyChangedEventArgs(nameof(this.Creationtime)));
            }
        }

        private void GetDescriptionReaderExecution(NpgsqlDataReader reader)
        {
            if (!reader.IsDBNull(1))
            {
                this.description = Encoding.Unicode.GetString(WinRTCrypto.CryptographicEngine.Decrypt(this.Sender?.AccessKey, (byte[])reader[1]));
                this.OnPropertyChanged(new PropertyChangedEventArgs(nameof(this.Description)));
            }
        }
    }
}
*/