// <copyright file="RMSRecordExtensions.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace ManagementSystemLibrary.RMS
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using ManagementSystemLibrary.ManagementSystem;
    using ManagementSystemLibrary.Pipeline;
    using Npgsql;
    using NpgsqlTypes;

    /// <summary>
    /// Extensions related to the <see cref="RMSRecord"/>.
    /// </summary>
    public static class RMSRecordExtensions
    {
        /// <summary>
        /// Searches for <see cref="RMSRecord"/> by name.
        /// </summary>
        /// <param name="parent">The parent <see cref="MSDatabaseObject"/>.</param>
        /// <param name="name">The name of the <see cref="RMSRecord"/>.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public static async Task<RMSRecord?> SearchRecordAsync(this MSDatabaseObject parent, string name)
        {
            RMSRecord? record = null;
            if (parent is not null
                && name is not null)
            {
                if (await parent.GenerateHashAsync().ConfigureAwait(false) is not null)
                {
                    await new PipelineItem(parent.Pipeline)
                    {
                        BatchCommand = SearchRecordBatchCommand(parent),
                        ReaderExecution = (NpgsqlDataReader reader) =>
                        {
                            do
                            {
                                if (!reader.IsDBNull(1))
                                {
                                    record = new (parent, reader.GetInt64(1));
                                }
                            }
                            while (reader.Read());
                        },
                    }.ExecuteAsync().ConfigureAwait(false);
                }
            }

            return record;
        }

        /// <summary>
        /// Loads <see cref="RMSRecord"/> related to the <see cref="MSDatabaseObject"/>.
        /// </summary>
        /// <param name="parent">The parent <see cref="MSDatabaseObject"/>.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public static async Task<IEnumerable<RMSRecord>> LoadRecordsAsync(this MSDatabaseObject parent)
        {
            return (await parent.LoadItemsAsync<RMSRecord, MSDatabaseObject>().ConfigureAwait(false)).Select(id => new RMSRecord(parent, id));
        }

        private static Func<PipelineItem, NpgsqlCommand, bool> SearchRecordBatchCommand(MSDatabaseObject parent)
        {
            return (PipelineItem item, NpgsqlCommand command) =>
            {
                if (parent.Hash is not null)
                {
                    command.CommandText += new StringBuilder("SELECT ")
                        .Append(item.AddParameter(command, "itemid", NpgsqlDbType.Integer, item.ID))
                        .Append(", searchrmsrecord(")
                        .Append(item.AddParameter(command, "child", NpgsqlDbType.Bytea, parent.Hash))
                        .Append(");");
                    return true;
                }

                return false;
            };
        }
    }
}
