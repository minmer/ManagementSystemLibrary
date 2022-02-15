// <copyright file="MSRangeObject.cs" company="PlaceholderCompany">
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
    public abstract class MSRangeObject<T1, T2> : MSTimeObject<T1, T2>
        where T1 : MSRangeObject<T1, T2>
        where T2 : MSScheduleObject<T2, T1>
    {
        private bool? endTimeVerification;
        private DateTime? endTime;

        /// <summary>
        /// Initializes a new instance of the <see cref="MSRangeObject{T1,T2}"/> class.
        /// </summary>
        /// <param name="parent">The parent of the <see cref="MSRangeObject{T1,T2}"/>.</param>
        /// <param name="id">The identifier of the <see cref="MSRangeObject{T1,T2}"/>.</param>
        public MSRangeObject(T2 parent, long id)
            : base(parent, id)
        {
        }

        /// <summary>
        /// Gets or sets the end time of the <see cref="MSRangeObject{T1,T2}"/>.
        /// </summary>
        public DateTime? EndTime
        {
            get
            {
                _ = this.GetEndTimeAsync();
                return this.endTime;
            }

            set
            {
                _ = this.SaveTimeAsync(value);
            }
        }

        /// <summary>
        /// Gets a value indicating whether the time of the <see cref="MSRangeObject{T1,T2}"/> is verified.
        /// </summary>
        public bool? EndTimeVerification
        {
            get
            {
                _ = this.VerifyEndTimeAsync();
                return this.endTimeVerification;
            }
        }

        /// <summary>
        /// Gets the <see cref="EndTime"/> of the <see cref="MSRangeObject{T1,T2}"/>.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task<DateTime?> GetEndTimeAsync()
        {
            if (this.endTime is null
                && await this.GetAccessAsync().ConfigureAwait(false) is not null)
            {
                await new PipelineItem(this.Pipeline)
                {
                    BatchCommand = this.BasicGetByIDBatchCommand("get", "endtime"),
                    ReaderExecution = this.GetEndTimeReaderExecution,
                }.ExecuteAsync().ConfigureAwait(false);
            }

            return this.endTime;
        }

        /// <summary>
        /// Saves the end time of the <see cref="MSRangeObject{T1,T2}"/>.
        /// </summary>
        /// <param name="value">The new value.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task SaveEndTimeAsync(DateTime? value)
        {
            if (this.endTime != value
                && value is not null
                && await this.GetAccessAsync().ConfigureAwait(false) is not null
                && await this.Parent.GetParametersAsync().ConfigureAwait(false)
                && this.Parent.AccessType <= MSAccessType.Contributor)
            {
                this.endTime = value;
                this.OnPropertyChanged(new PropertyChangedEventArgs(nameof(this.EndTime)));
                await new PipelineItem(this.Pipeline)
                {
                    BatchCommand = this.SaveEndTimeBatchCommand,
                }.ExecuteAsync().ConfigureAwait(false);
                await this.SaveDataAsync(await this.GetDataAsync().ConfigureAwait(false)).ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Verifies the <see cref="EndTime"/> of the <see cref="MSRangeObject{T1,T2}"/>.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task<bool?> VerifyEndTimeAsync()
        {
            if (await this.GetTimeAsync().ConfigureAwait(false) is not null
                && await this.GetModificationTimeAsync().ConfigureAwait(false) is not null
                && await this.GetModifierAsync().ConfigureAwait(false) is AMSAccount modifier)
            {
                if (await modifier.GetPublicSignatureAsync().ConfigureAwait(false) is not null)
                {
                    await new PipelineItem(this.Pipeline)
                    {
                        BatchCommand = this.BasicGetByIDBatchCommand("verify", "endtime"),
                        ReaderExecution = this.VerifyEndTimeReaderExecution,
                    }.ExecuteAsync().ConfigureAwait(false);
                }
            }

            return this.endTimeVerification;
        }

        /// <summary>
        /// Creates a new <see cref="MSRangeObject{T,T}"/>.
        /// </summary>
        /// <param name="parent">The parent <see cref="MSRangeObject{T,T}"/>.</param>
        /// <param name="name">The name of the <see cref="MSRangeObject{T,T}"/>.</param>
        /// <param name="data">The data of the  <see cref="MSRangeObject{T,T}"/>.</param>
        /// <param name="time">The time of the  <see cref="MSRangeObject{T,T}"/>.</param>
        /// <param name="endTime">The end time of the  <see cref="MSRangeObject{T,T}"/>.</param>
        /// <param name="parameterMethod">A method for additional parameters.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        protected static async Task<long?> CreateAsync(T2 parent, string name, byte[] data, DateTime time, DateTime endTime, Action<PipelineItem, NpgsqlCommand, DateTime, Aes, byte[], MSDatabaseObject, StringBuilder>? parameterMethod)
        {
            if (BitConverter.GetBytes(DateTime.Now.Ticks) is byte[] timeArray
                && await parent.GetAccessAsync().ConfigureAwait(false) is Aes parentAccess
                && await parent.GetParametersAsync().ConfigureAwait(false)
                && parent.PA is not null
                && parent.PM is not null
                && 682700 + (Pipeline.Random.NextDouble() * 1000) is double pb)
            {
                return await MSTimeObject<T1, T2>.CreateAsync(parent, name, data, time, (PipelineItem item, NpgsqlCommand command, DateTime creationtime, Aes access, byte[] _, MSDatabaseObject _, StringBuilder builder) =>
                {
                    builder.Append(',')
                    .Append(item.AddParameter(command, "pb", NpgsqlDbType.Double, pb))
                    .Append(',')
                    .Append(item.AddParameter(command, "pn", NpgsqlDbType.Double, endTime.Ticks / parent.PM / (parent.PA + pb)))
                    .Append(',')
                    .Append(item.AddParameter(command, "endtime", NpgsqlDbType.Bytea, access.EncryptCbc(BitConverter.GetBytes(endTime.Ticks), access.IV)))
                    .Append(',')
                    .Append(item.AddParameter(command, "endtimeverification", NpgsqlDbType.Bytea, parent.Account.PrivateSignature.SignData(BitConverter.GetBytes(endTime.Ticks).Concat(BitConverter.GetBytes(parent.Account.ID)).Concat(BitConverter.GetBytes(creationtime.Ticks)).ToArray(), Pipeline.HashAlgorithmName, Pipeline.RSASignaturePadding)));
                    parameterMethod?.Invoke(item, command, creationtime, access, data, parent, builder);
                }).ConfigureAwait(false);
            }

            return null;
        }

        private bool SaveEndTimeBatchCommand(PipelineItem item, NpgsqlCommand command)
        {
            if (this.Access is not null
                && this.ModificationTime is not null
                && this.Modifier?.PrivateSignature is not null
                && this.endTime is not null
                && this.Parent.PA is not null
                && this.Parent.PM is not null
                && 682700 + (Pipeline.Random.NextDouble() * 1000) is double pa)
            {
                command.CommandText += new StringBuilder("SELECT ")
                    .Append(item.AddParameter(command, "itemid", NpgsqlDbType.Integer, item.ID))
                    .AppendFormat(", save{0}time(", this.Abbreviation)
                    .Append(item.AddParameter(command, "id", NpgsqlDbType.Bigint, this.ID))
                    .Append(',')
                    .Append(item.AddParameter(command, "pb", NpgsqlDbType.Double, pa))
                    .Append(',')
                    .Append(item.AddParameter(command, "pn", NpgsqlDbType.Double, this.endTime.Value.Ticks / this.Parent.PM / (this.Parent.PA + pa)))
                    .Append(',')
                    .Append(item.AddParameter(command, "endtime", NpgsqlDbType.Bytea, this.Access.EncryptCbc(BitConverter.GetBytes(this.endTime.Value.Ticks), this.Access.IV)))
                    .Append(',')
                    .Append(item.AddParameter(command, "endtimeverification", NpgsqlDbType.Bytea, this.Parent.PrivateSignature.SignData(BitConverter.GetBytes(this.endTime.Value.Ticks).Concat(BitConverter.GetBytes(this.Modifier.ID)).Concat(BitConverter.GetBytes(this.ModificationTime.Value.Ticks)).ToArray(), Pipeline.HashAlgorithmName, Pipeline.RSASignaturePadding)));
                return true;
            }

            return false;
        }

        private void GetEndTimeReaderExecution(NpgsqlDataReader reader)
        {
            if (!reader.IsDBNull(1)
                && this.Access is not null)
            {
                this.endTime = new DateTime(BitConverter.ToInt64(this.Access.DecryptCbc((byte[])reader[1], this.Access.IV), 0));
                this.OnPropertyChanged(new PropertyChangedEventArgs(nameof(this.EndTime)));
            }
        }

        private void VerifyEndTimeReaderExecution(NpgsqlDataReader reader)
        {
            if (!reader.IsDBNull(1)
                && this.Modifier?.PublicSignature is not null
                && this.ModificationTime is not null
                && this.endTime is not null)
            {
                this.endTimeVerification = this.Modifier.PublicSignature.VerifyData(BitConverter.GetBytes(this.endTime.Value.Ticks).Concat(BitConverter.GetBytes(this.Modifier.ID)).Concat(BitConverter.GetBytes(this.ModificationTime.Value.Ticks)).ToArray(), (byte[])reader[1], Pipeline.HashAlgorithmName, Pipeline.RSASignaturePadding);
                this.OnPropertyChanged(new PropertyChangedEventArgs(nameof(this.EndTimeVerification)));
            }
        }
    }
}
