// <copyright file="MSScheduleObject.cs" company="PlaceholderCompany">
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
    /// <typeparam name="T1">The type of the scheduler.</typeparam>
    /// <typeparam name="T2">The type of the scheduled items.</typeparam>
    public abstract class MSScheduleObject<T1, T2> : MSAccessObject
        where T1 : MSScheduleObject<T1, T2>
        where T2 : MSTimeObject<T2, T1>
    {
        private double? pM;
        private double? pA;

        /// <summary>
        /// Initializes a new instance of the <see cref="MSScheduleObject{T1, T2}"/> class.
        /// </summary>
        /// <param name="association">The <see cref="AMSAssociation"/> calling the <see cref="MSScheduleObject{T1, T2}"/>.</param>
        /// <param name="id">The identifier of the <see cref="MSScheduleObject{T1,T2}"/>.</param>
        /// <param name="key">The private <see cref="RSA"/> key of the <see cref="MSScheduleObject{T1,T2}"/>.</param>
        /// <param name="signature">The private <see cref="RSA"/> signature of the <see cref="MSScheduleObject{T1,T2}"/>.</param>
        public MSScheduleObject(AMSAssociation association, long id, byte[] key, byte[] signature)
            : base(association, id, key, signature)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MSScheduleObject{T1,T2}"/> class.
        /// </summary>
        /// <param name="association">The <see cref="AMSAssociation"/> calling the <see cref="MSScheduleObject{T1, T2}"/>.</param>
        /// <param name="id">The identifier of the <see cref="MSScheduleObject{T1,T2}"/>.</param>
        /// <param name="key">The private <see cref="RSA"/> key of the <see cref="MSScheduleObject{T1,T2}"/>.</param>
        public MSScheduleObject(AMSAssociation association, long id, byte[] key)
            : base(association, id, key)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MSScheduleObject{T1,T2}"/> class.
        /// </summary>
        /// <param name="association">The <see cref="AMSAssociation"/> calling the <see cref="MSScheduleObject{T1, T2}"/>.</param>
        /// <param name="id">The identifier of the <see cref="MSScheduleObject{T1,T2}"/>.</param>
        /// <param name="access">The key of the <see cref="MSScheduleObject{T1,T2}"/>.</param>
        public MSScheduleObject(AMSAssociation association, long id, Aes access)
            : base(association, id, access)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MSScheduleObject{T1,T2}"/> class.
        /// </summary>
        /// <param name="association">The <see cref="AMSAssociation"/> calling the <see cref="MSScheduleObject{T1, T2}"/>.</param>
        /// <param name="id">The identifier of the <see cref="MSScheduleObject{T1,T2}"/>.</param>
        public MSScheduleObject(AMSAssociation association, long id)
            : base(association, id)
        {
        }

        /// <summary>
        /// Gets the private multiplicator parameter of the <see cref="MSScheduleObject{T1,T2}"/>.
        /// </summary>
        public double? PM
        {
            get
            {
                _ = this.GetParametersAsync();
                return this.pM;
            }
        }

        /// <summary>
        /// Gets the private addition parameter of the <see cref="MSScheduleObject{T1,T2}"/>.
        /// </summary>
        public double? PA
        {
            get
            {
                _ = this.GetParametersAsync();
                return this.pA;
            }
        }

        /// <summary>
        /// Gets or sets the function to initialize the childs of the <see cref="MSScheduleObject{T1,T2}"/>.
        /// </summary>
        protected static Func<T1, long, T2>? ChildInitialization { get; set; }

        /// <summary>
        /// Gets the parameters of the <see cref="MSScheduleObject{T1,T2}"/>.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task<bool> GetParametersAsync()
        {
            if (this.pA is null
                && await this.GetAccessAsync().ConfigureAwait(false) is not null)
            {
                await new PipelineItem(this.Pipeline)
                {
                    BatchCommand = this.BasicGetByIDBatchCommand("get", "parameters"),
                    ReaderExecution = this.GetParametersReaderExecution,
                }.ExecuteAsync().ConfigureAwait(false);
            }

            return this.pA is not null;
        }

        /// <summary>
        /// Loads the childs related to the <see cref="MSScheduleObject{T1,T2}"/>.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task<T2[]> LoadChildsAsync()
        {
            return await this.LoadChildsAsync(DateTime.MinValue).ConfigureAwait(false);
        }

        /// <summary>
        /// Loads the childs related to the <see cref="MSScheduleObject{T1,T2}"/>.
        /// </summary>
        /// <param name="startTime">The starttime of the load.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task<T2[]> LoadChildsAsync(DateTime startTime)
        {
            return await this.LoadChildsAsync(startTime, DateTime.MaxValue).ConfigureAwait(false);
        }

        /// <summary>
        /// Loads the childs related to the <see cref="MSScheduleObject{T1,T2}"/>.
        /// </summary>
        /// <param name="startTime">The starttime of the load.</param>
        /// <param name="endTime">The endtime of the load.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task<T2[]> LoadChildsAsync(DateTime startTime, DateTime endTime)
        {
            return await this.LoadChildsAsync(startTime, endTime, int.MaxValue).ConfigureAwait(false);
        }

        /// <summary>
        /// Loads the childs related to the <see cref="MSScheduleObject{T1,T2}"/>.
        /// </summary>
        /// <param name="startTime">The starttime of the load.</param>
        /// <param name="endTime">The endtime of the load.</param>
        /// <param name="count">The count of the load.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public virtual async Task<T2[]> LoadChildsAsync(DateTime startTime, DateTime endTime, int count)
        {
            List<T2> childs = new ();
            if (await this.GenerateHashAsync().ConfigureAwait(false) is not null
                && await this.GetParametersAsync().ConfigureAwait(false)
                && this is T1 scheduleObject)
            {
                await new PipelineItem(this.Pipeline)
                {
                    BatchCommand = this.LoadChildrenBatchCommand(startTime, endTime, count),
                    ReaderExecution = (NpgsqlDataReader reader) =>
                    {
                        do
                        {
                            if (!reader.IsDBNull(1))
                            {
                                if (ChildInitialization?.Invoke(scheduleObject, reader.GetInt64(1)) is T2 child)
                                {
                                    childs.Add(child);
                                }
                            }
                        }
                        while (reader.Read());
                    },
                }.ExecuteAsync().ConfigureAwait(false);
            }

            return childs.ToArray();
        }

        /// <summary>
        /// Creates a new <see cref="MSScheduleObject{T1,T2}"/>.
        /// </summary>
        /// <param name="creator">The creator of the <see cref="MSScheduleObject{T1,T2}"/>.</param>
        /// <param name="name">The name of the <see cref="MSScheduleObject{T1,T2}"/>.</param>
        /// <param name="parameterMethod">A method for additional parameters.</param>
        /// <typeparam name="T">Type of the created object.</typeparam>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        protected static async Task<long?> CreateAsync<T>(AMSAssociation creator, string name, Action<PipelineItem, NpgsqlCommand, DateTime, AMSAssociation, Aes, string, RSA, RSA, double, double, StringBuilder>? parameterMethod)
        {
            if (682700 + (Pipeline.Random.NextDouble() * 1000) is double pa
                && 682700 + (Pipeline.Random.NextDouble() * 1000) is double pm)
            {
                return await MSAccessObject.CreateAsync<T>(creator, name, (PipelineItem item, NpgsqlCommand command, DateTime creationTime, AMSAssociation _, Aes access, string name, RSA privateKey, RSA privateSignature, StringBuilder builder) =>
                {
                    builder.Append(',')
                    .Append(item.AddParameter(command, "parameters", NpgsqlDbType.Bytea, access.EncryptCbc(BitConverter.GetBytes(pa).Concat(BitConverter.GetBytes(pm)).ToArray(), access.IV)));
                    parameterMethod?.Invoke(item, command, creationTime, creator, access, name, privateKey, privateSignature, pa, pm, builder);
                }).ConfigureAwait(false);
            }

            return null;
        }

        private Func<PipelineItem, NpgsqlCommand, bool> LoadChildrenBatchCommand(DateTime startTime, DateTime endTime, int count)
        {
            return (PipelineItem item, NpgsqlCommand command) =>
            {
                if (this.Hash is not null
                    && this.pA is not null
                    && this.pM is not null)
                {
                    command.CommandText += new StringBuilder("SELECT ")
                        .Append(item.AddParameter(command, "itemid", NpgsqlDbType.Integer, item.ID))
                        .AppendFormat(", load{0}{1}s(", this.Abbreviation, typeof(T2).GetDatabaseAbbreviation())
                        .Append(item.AddParameter(command, "talk", NpgsqlDbType.Bytea, this.Hash))
                        .Append(',')
                        .Append(item.AddParameter(command, "pa", NpgsqlDbType.Double, this.pA))
                        .Append(',')
                        .Append(item.AddParameter(command, "pm", NpgsqlDbType.Double, this.pM))
                        .Append(',')
                        .Append(item.AddParameter(command, "starttime", NpgsqlDbType.Bigint, startTime.Ticks))
                        .Append(',')
                        .Append(item.AddParameter(command, "endtime", NpgsqlDbType.Bigint, endTime.Ticks))
                        .Append(',')
                        .Append(item.AddParameter(command, "count", NpgsqlDbType.Integer, count))
                        .Append(");");
                    return true;
                }

                return false;
            };
        }

        private void GetParametersReaderExecution(NpgsqlDataReader reader)
        {
            if (!reader.IsDBNull(1)
                && this.Access is not null)
            {
                byte[] rawData = this.Access.DecryptCbc((byte[])reader[1], this.Access.IV);
                this.pA = BitConverter.ToDouble(rawData, 0);
                this.pM = BitConverter.ToDouble(rawData, 8);
                this.OnPropertyChanged(new PropertyChangedEventArgs(nameof(this.PA)));
                this.OnPropertyChanged(new PropertyChangedEventArgs(nameof(this.PM)));
            }
        }
    }
}
