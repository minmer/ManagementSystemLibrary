// <copyright file="MSTimeObject.cs" company="PlaceholderCompany">
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
    /// <typeparam name="T1">The type of the scheduled items.</typeparam>
    /// <typeparam name="T2">The type of the scheduler.</typeparam>
    public abstract class MSTimeObject<T1, T2> : MSDataObject<T2>
        where T1 : MSTimeObject<T1, T2>
        where T2 : MSScheduleObject<T2, T1>
    {
        private bool? timeVerification;
        private DateTime? time;

        /// <summary>
        /// Initializes a new instance of the <see cref="MSTimeObject{T1,T2}"/> class.
        /// </summary>
        /// <param name="parent">The parent of the <see cref="MSTimeObject{T1,T2}"/>.</param>
        /// <param name="id">The identifier of the <see cref="MSTimeObject{T1,T2}"/>.</param>
        public MSTimeObject(T2 parent, long id)
            : base(parent, id)
        {
        }

        /// <summary>
        /// Gets or sets the time of the <see cref="MSTimeObject{T1,T2}"/>.
        /// </summary>
        public DateTime? Time
        {
            get
            {
                _ = this.GetTimeAsync();
                return this.time;
            }

            set
            {
                _ = this.SaveTimeAsync(value);
            }
        }

        /// <summary>
        /// Gets a value indicating whether the time of the <see cref="MSTimeObject{T1,T2}"/> is verified.
        /// </summary>
        public bool? TimeVerification
        {
            get
            {
                _ = this.VerifyTimeAsync();
                return this.timeVerification;
            }
        }

        /// <summary>
        /// Gets the <see cref="Time"/> of the <see cref="MSTimeObject{T1,T2}"/>.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task<DateTime?> GetTimeAsync()
        {
            if (this.time is null
                && await this.GetAccessAsync().ConfigureAwait(false) is not null)
            {
                await new PipelineItem(this.Pipeline)
                {
                    BatchCommand = this.BasicGetByIDBatchCommand("get", "time"),
                    ReaderExecution = this.GetTimeReaderExecution,
                }.ExecuteAsync().ConfigureAwait(false);
            }

            return this.time;
        }

        /// <summary>
        /// Saves the time of the <see cref="MSTimeObject{T1,T2}"/>.
        /// </summary>
        /// <param name="value">The new value.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task SaveTimeAsync(DateTime? value)
        {
            if (this.time != value
                && value is not null
                && await this.GetAccessAsync().ConfigureAwait(false) is not null
                && await this.Parent.GetParametersAsync().ConfigureAwait(false)
                && this.Parent.AccessType <= MSAccessType.Contributor)
            {
                this.time = value;
                this.OnPropertyChanged(new PropertyChangedEventArgs(nameof(this.Time)));
                await new PipelineItem(this.Pipeline)
                {
                    BatchCommand = this.SaveTimeBatchCommand,
                }.ExecuteAsync().ConfigureAwait(false);
                await this.SaveDataAsync(await this.GetDataAsync().ConfigureAwait(false)).ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Verifies the <see cref="Time"/> of the <see cref="MSTimeObject{T1,T2}"/>.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task<bool?> VerifyTimeAsync()
        {
            if (await this.GetTimeAsync().ConfigureAwait(false) is not null
                && await this.GetModificationTimeAsync().ConfigureAwait(false) is not null
                && await this.GetModifierAsync().ConfigureAwait(false) is AMSAccount modifier)
            {
                if (await modifier.GetPublicSignatureAsync().ConfigureAwait(false) is not null)
                {
                    await new PipelineItem(this.Pipeline)
                    {
                        BatchCommand = this.BasicGetByIDBatchCommand("verify", "time"),
                        ReaderExecution = this.VerifyTimeReaderExecution,
                    }.ExecuteAsync().ConfigureAwait(false);
                }
            }

            return this.timeVerification;
        }

        /// <summary>
        /// Creates a new <see cref="MSTimeObject{T,T}"/>.
        /// </summary>
        /// <param name="parent">The parent <see cref="MSTimeObject{T,T}"/>.</param>
        /// <param name="name">The name of the <see cref="MSTimeObject{T,T}"/>.</param>
        /// <param name="data">The data of the  <see cref="MSTimeObject{T,T}"/>.</param>
        /// <param name="time">The time of the  <see cref="MSTimeObject{T,T}"/>.</param>
        /// <param name="parameterMethod">A method for additional parameters.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        protected static async Task<long?> CreateAsync(T2 parent, string name, byte[] data, DateTime time, Action<PipelineItem, NpgsqlCommand, DateTime, Aes, byte[], MSDatabaseObject, StringBuilder>? parameterMethod)
        {
            if (BitConverter.GetBytes(DateTime.Now.Ticks) is byte[] timeArray
                && await parent.GetAccessAsync().ConfigureAwait(false) is Aes parentAccess
                && await parent.GetParametersAsync().ConfigureAwait(false)
                && parent.PA is not null
                && parent.PM is not null
                && 682700 + (Pipeline.Random.NextDouble() * 1000) is double pa)
            {
                return await MSDataObject<T2>.CreateAsync<T1>(parent, name, data, (PipelineItem item, NpgsqlCommand command, DateTime creationtime, Aes access, byte[] _, MSDatabaseObject _, StringBuilder builder) =>
                {
                    builder.Append(',')
                    .Append(item.AddParameter(command, "pa", NpgsqlDbType.Double, pa))
                    .Append(',')
                    .Append(item.AddParameter(command, "pm", NpgsqlDbType.Double, time.Ticks / parent.PM / (parent.PA + pa)))
                    .Append(',')
                    .Append(item.AddParameter(command, "time", NpgsqlDbType.Bytea, access.EncryptCbc(BitConverter.GetBytes(time.Ticks), access.IV)))
                    .Append(',')
                    .Append(item.AddParameter(command, "timeverification", NpgsqlDbType.Bytea, parent.Account.PrivateSignature.SignData(BitConverter.GetBytes(time.Ticks).Concat(BitConverter.GetBytes(parent.Account.ID)).Concat(BitConverter.GetBytes(creationtime.Ticks)).ToArray(), Pipeline.HashAlgorithmName, Pipeline.RSASignaturePadding)));
                    parameterMethod?.Invoke(item, command, creationtime, access, data, parent, builder);
                }).ConfigureAwait(false);
            }

            return null;
        }

        private bool SaveTimeBatchCommand(PipelineItem item, NpgsqlCommand command)
        {
            if (this.Access is not null
                && this.ModificationTime is not null
                && this.Modifier?.PrivateSignature is not null
                && this.time is not null
                && this.Parent.PA is not null
                && this.Parent.PM is not null
                && 682700 + (Pipeline.Random.NextDouble() * 1000) is double pa)
            {
                command.CommandText += new StringBuilder("SELECT ")
                    .Append(item.AddParameter(command, "itemid", NpgsqlDbType.Integer, item.ID))
                    .AppendFormat(", save{0}time(", this.Abbreviation)
                    .Append(item.AddParameter(command, "id", NpgsqlDbType.Bigint, this.ID))
                    .Append(',')
                    .Append(item.AddParameter(command, "pa", NpgsqlDbType.Double, pa))
                    .Append(',')
                    .Append(item.AddParameter(command, "pm", NpgsqlDbType.Double, this.time.Value.Ticks / this.Parent.PM / (this.Parent.PA + pa)))
                    .Append(',')
                    .Append(item.AddParameter(command, "time", NpgsqlDbType.Bytea, this.Access.EncryptCbc(BitConverter.GetBytes(this.time.Value.Ticks), this.Access.IV)))
                    .Append(',')
                    .Append(item.AddParameter(command, "timeverification", NpgsqlDbType.Bytea, this.Parent.PrivateSignature.SignData(BitConverter.GetBytes(this.time.Value.Ticks).Concat(BitConverter.GetBytes(this.Modifier.ID)).Concat(BitConverter.GetBytes(this.ModificationTime.Value.Ticks)).ToArray(), Pipeline.HashAlgorithmName, Pipeline.RSASignaturePadding)));
                return true;
            }

            return false;
        }

        private void GetTimeReaderExecution(NpgsqlDataReader reader)
        {
            if (!reader.IsDBNull(1)
                && this.Access is not null)
            {
                this.time = new DateTime(BitConverter.ToInt64(this.Access.DecryptCbc((byte[])reader[1], this.Access.IV), 0));
                this.OnPropertyChanged(new PropertyChangedEventArgs(nameof(this.Time)));
            }
        }

        private void VerifyTimeReaderExecution(NpgsqlDataReader reader)
        {
            if (!reader.IsDBNull(1)
                && this.Modifier?.PublicSignature is not null
                && this.ModificationTime is not null
                && this.time is not null)
            {
                this.timeVerification = this.Modifier.PublicSignature.VerifyData(BitConverter.GetBytes(this.time.Value.Ticks).Concat(BitConverter.GetBytes(this.Modifier.ID)).Concat(BitConverter.GetBytes(this.ModificationTime.Value.Ticks)).ToArray(), (byte[])reader[1], Pipeline.HashAlgorithmName, Pipeline.RSASignaturePadding);
                this.OnPropertyChanged(new PropertyChangedEventArgs(nameof(this.TimeVerification)));
            }
        }
    }
}
