// <copyright file="PipelineItem.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace ManagementSystemLibrary.Pipeline
{
    using System;
    using System.Text;
    using System.Threading.Tasks;
    using Npgsql;
    using NpgsqlTypes;

    /// <summary>
    /// Represents an item of a <see cref="Pipeline"/>.
    /// </summary>
    public class PipelineItem
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PipelineItem"/> class.
        /// </summary>
        /// <param name="parent">The parent of the <see cref="PipelineItem"/>.</param>
        public PipelineItem(Pipeline parent)
        {
            this.Parent = parent;
            this.ID = parent.RegisterItem(this);
        }

        /// <summary>
        /// Gets the parent of the <see cref="PipelineItem"/>.
        /// </summary>
        public Pipeline Parent { get; }

        /// <summary>
        /// Gets or sets the function that adds the command to the batch.
        /// </summary>
        public Func<PipelineItem, NpgsqlCommand, bool>? BatchCommand { get; set; }

        /// <summary>
        /// Gets or sets the action that should be invoked during execution.
        /// </summary>
        public Action<NpgsqlDataReader>? ReaderExecution { get; set; }

        /// <summary>
        /// Gets the identifier of the <see cref="PipelineItem"/>.
        /// </summary>
        public int ID { get; }

        /// <summary>
        /// Gets the <see cref="TaskCompletionSource{T}"/> that indicates the completion of the execution.
        /// </summary>
        public TaskCompletionSource<bool> ExecutionCompleted { get; } = new ();

        /// <summary>
        /// Executes the <see cref="PipelineItem"/> asynchrously.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task ExecuteAsync()
        {
            await this.Parent.ExecuteAsync(this);
        }

        /// <summary>
        /// Adds the parameter to the <see cref="PipelineItem"/>.
        /// </summary>
        /// <param name="command">The command to that the parameter should be added.</param>
        /// <param name="name">The name of the parameter.</param>
        /// <param name="type">The type of the parameter.</param>
        /// <param name="value">The value of the parameter.</param>
        /// <returns>The new name of the parameter.</returns>
        public string AddParameter(NpgsqlCommand command, string name, NpgsqlDbType type, object value)
        {
            string idName = new StringBuilder("@")
                .Append(this.ID)
                .Append('_')
                .Append(name).ToString();
            command.Parameters.AddWithValue(idName, type, value);
            return idName;
        }
    }
}
