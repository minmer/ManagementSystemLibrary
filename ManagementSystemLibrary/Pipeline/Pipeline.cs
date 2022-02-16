// <copyright file="Pipeline.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace ManagementSystemLibrary.Pipeline
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using System.Security.Cryptography;
    using System.Threading.Tasks;
    using Npgsql;

    /// <summary>
    /// Represents a pipeline of <see cref="NpgsqlCommand"/>.
    /// </summary>
    public class Pipeline
    {
        private readonly List<PipelineItem> items = new ();
        private readonly List<PipelineItem> executionItems = new ();
        private readonly Dictionary<long, PipelineItem[]> executedItems = new ();
        private NpgsqlCommand? internalCommand;
        private bool isConnectionHold;
        private long commandIndex;

        /// <summary>
        /// Initializes a new instance of the <see cref="Pipeline"/> class.
        /// </summary>
        /// <param name="parameters">The parameters for a <see cref="NpgsqlConnection"/> of the <see cref="Pipeline"/>.</param>
        public Pipeline(ServerParameters parameters)
        {
            this.Parameters = parameters;
        }

        /// <summary>
        /// Gets the <see cref="Random"/> used by all managment systems.
        /// </summary>
        public static Random Random { get; } = new ();

        /// <summary>
        /// Gets the parameters for a <see cref="NpgsqlConnection"/> of the <see cref="Pipeline"/>.
        /// </summary>
        public ServerParameters Parameters { get; }

        /// <summary>
        /// Gets the <see cref="HashAlgorithmName"/> of the <see cref="Pipeline"/>.
        /// </summary>
        internal static HashAlgorithmName HashAlgorithmName { get; } = HashAlgorithmName.SHA256;

        /// <summary>
        /// Gets the <see cref="RSAEncryptionPadding"/> of the <see cref="Pipeline"/>.
        /// </summary>
        internal static RSAEncryptionPadding RSAEncryptionPadding { get; } = RSAEncryptionPadding.Pkcs1;

        /// <summary>
        /// Gets the <see cref="RSAEncryptionPaddingMode"/> of the <see cref="Pipeline"/>.
        /// </summary>
        internal static RSAEncryptionPaddingMode RSAEncryptionPaddingMode { get; } = RSAEncryptionPaddingMode.Pkcs1;

        /// <summary>
        /// Gets the <see cref="RSASignaturePadding"/> of the <see cref="Pipeline"/>.
        /// </summary>
        internal static RSASignaturePadding RSASignaturePadding { get; } = RSASignaturePadding.Pkcs1;

        /// <summary>
        /// Registers a <see cref="PipelineItem"/> to the <see cref="Pipeline"/>.
        /// </summary>
        /// <param name="item">The added item.</param>
        /// <returns>The unique identifier of the item.</returns>
        public int RegisterItem(PipelineItem item)
        {
            this.items.Add(item);
            return this.items.Count;
        }

        /// <summary>
        /// Executes a <see cref="PipelineItem"/> asynchrously.
        /// </summary>
        /// <param name="item">The that should be executed.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task ExecuteAsync(PipelineItem item)
        {
            this.executionItems.Add(item);
            if (!this.isConnectionHold)
            {
                this.isConnectionHold = true;
                if (this.executionItems.Count > 0)
                {
                    new Task(async () =>
                    {
                        await this.TryConnectAsync();
                    }).Start();
                }
            }

            await item.ExecutionCompleted.Task.ConfigureAwait(false);
        }

        private async Task TryConnectAsync()
        {
            try
            {
                if (this.Parameters.Server != null && this.Parameters.User != null && this.Parameters.Password != null && this.Parameters.Database != null)
                {
                    using NpgsqlConnection connection = new (
                        new NpgsqlConnectionStringBuilder
                        {
                            { "Server", this.Parameters.Server },
                            { "Port", this.Parameters.Port },
                            { "User Id", this.Parameters.User },
                            { "Password", this.Parameters.Password },
                            { "Database", this.Parameters.Database },
                            { "Timeout", 30 },
                        }.ToString());
                    await connection.OpenAsync().ConfigureAwait(false);
                    while (this.executionItems.Count > 0)
                    {
                        await this.ExecuteItemsAsync(connection).ConfigureAwait(false);
                    }
                }
            }
            catch (Exception exception) when (exception.GetType() == typeof(Exception))
            {
                await this.TryConnectAsync().ConfigureAwait(false);
            }

            this.isConnectionHold = false;
        }

        private async Task ExecuteItemsAsync(NpgsqlConnection connection)
        {
            if (this.commandIndex is long commandIndex && this.BatchCommands(connection) is NpgsqlCommand command)
            {
                await this.ExecuteCommandAsync(command, commandIndex).ConfigureAwait(false);
            }
        }

        private async Task ExecuteCommandAsync(NpgsqlCommand command, long commandIndex)
        {
            using NpgsqlDataReader reader = await command.ExecuteReaderAsync().ConfigureAwait(false);
            do
            {
                if (await reader.ReadAsync().ConfigureAwait(false))
                {
                    if (this.executedItems[commandIndex].FirstOrDefault(item => item.ID == reader.GetInt64(0)) is PipelineItem pipelineItem)
                    {
                        pipelineItem.ReaderExecution?.Invoke(reader);
                        pipelineItem.ExecutionCompleted.TrySetResult();
                    }
                }
            }
            while (await reader.NextResultAsync().ConfigureAwait(false));
        }

        private NpgsqlCommand BatchCommands(NpgsqlConnection connection)
        {
            this.internalCommand = new (string.Empty, connection);
            List<PipelineItem> items = new ();
            this.AddCommandToBatch(this.internalCommand, items, 0);
            this.executedItems.Add(this.commandIndex, items.ToArray());
            this.commandIndex++;
            return this.internalCommand;
        }

        private void AddCommandToBatch(NpgsqlCommand command, List<PipelineItem> items, int start)
        {
            int index = start;
            while (index < this.executionItems.Count && items.Count < 1024)
            {
                if (this.executionItems[index].BatchCommand?.Invoke(this.executionItems[index], command) == true)
                {
                    items.Add(this.executionItems[index]);
                    this.executionItems.RemoveAt(index);
                }
                else
                {
                    index++;
                }
            }
        }
    }
}
