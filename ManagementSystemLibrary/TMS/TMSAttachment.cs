// <copyright file="TMSAttachment.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace ManagementSystemLibrary.TMS
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Linq;
    using System.Net;
    using System.Security.Cryptography;
    using System.Text;
    using System.Threading.Tasks;
    using ManagementSystemLibrary.AMS;
    using ManagementSystemLibrary.ManagementSystem;
    using ManagementSystemLibrary.Pipeline;
    using Npgsql;
    using NpgsqlTypes;

    /// <summary>
    /// Represents an attachment to a <see cref="TMSMessage"/>.
    /// </summary>
    public class TMSAttachment : MSDataObject<TMSMessage>
    {
        private string? description;
        private string? path;

        /// <summary>
        /// Initializes a new instance of the <see cref="TMSAttachment"/> class.
        /// </summary>
        /// <param name="message">The parent <see cref="TMSMessage"/> of the <see cref="TMSAttachment"/>.</param>
        /// <param name="id">The identifier of the <see cref="TMSAttachment"/>.</param>
        public TMSAttachment(TMSMessage message, long id)
            : base(message, id)
        {
        }

        /// <summary>
        /// Gets the description of the <see cref="TMSAttachment"/>.
        /// </summary>
        public string? Description
        {
            get
            {
                _ = this.GetPathAsync();
                return this.description;
            }
        }

        /// <summary>
        /// Gets the path of the <see cref="TMSAttachment"/>.
        /// </summary>
        public string? Path
        {
            get
            {
                _ = this.GetPathAsync();
                return this.path;
            }
        }

#pragma warning disable SYSLIB0014 // Type or member is obsolete

        /// <summary>
        /// Creates a new <see cref="TMSAttachment"/>.
        /// </summary>
        /// <param name="message">The parent of the created <see cref="TMSAttachment"/>.</param>
        /// <param name="description">The name of the created <see cref="TMSAttachment"/>.</param>
        /// <param name="path">The path of the created <see cref="TMSAttachment"/>.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public static async Task<TMSAttachment?> CreateAsync(TMSMessage message, string description, string path)
        {
            if (Encoding.Unicode.GetBytes(description) is byte[] descriptionArray
                && Aes.Create() is Aes access
                && path.TrimStartToLastChar('\\') is string name)
            {
                if (await MSDataObject<TMSMessage>.CreateAsync<TMSAttachment>(message, name, BitConverter.GetBytes(descriptionArray.Length).Concat(descriptionArray).Concat(Encoding.Unicode.GetBytes(name)).ToArray(), (PipelineItem _, NpgsqlCommand _, DateTime _, Aes tempAccess, byte[] _, MSDatabaseObject _, StringBuilder _) => { access = tempAccess; }) is long id
                    && WebRequest.Create(message.Pipeline.Parameters.FtpServerAddress + Convert.ToBase64String(SHA256.HashData(Encoding.Unicode.GetBytes(name))) + ".sec") is FtpWebRequest request)
                {
                    request.Method = WebRequestMethods.Ftp.UploadFile;
                    using CryptoStream csEncrypt = new (await request.GetRequestStreamAsync().ConfigureAwait(false), access.CreateEncryptor(access.Key, access.IV), CryptoStreamMode.Write);
                    using FileStream stream = File.Open(path, FileMode.Open);
                    await csEncrypt.CopyToAsync(stream);

                    return new (message, id);
                }
            }

            return null;
        }

        /// <summary>
        /// Gets the <see cref="Path"/> of the <see cref="TMSAttachment"/>.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task<object?> GetPathAsync()
        {
            if (this.description is null
                && await this.GetDataAsync().ConfigureAwait(false) is byte[] array)
            {
                int length = BitConverter.ToInt32(array);
                this.description = Encoding.Unicode.GetString(array[4..length]);
                this.path = Encoding.Unicode.GetString(array[length..]);
                this.OnPropertyChanged(new PropertyChangedEventArgs(nameof(this.Description)));
                this.OnPropertyChanged(new PropertyChangedEventArgs(nameof(this.Path)));
            }

            return this.description;
        }

        /// <summary>
        /// Downloads the <see cref="TMSAttachment"/>.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task DownloadAsync()
        {
            if (await this.GetPathAsync().ConfigureAwait(false) is string path
                && await this.GetAccessAsync().ConfigureAwait(false) is Aes access
                && WebRequest.Create(this.Pipeline.Parameters.FtpServerAddress + Convert.ToBase64String(SHA256.HashData(Encoding.Unicode.GetBytes(path))) + ".sec") is FtpWebRequest request)
            {
                request.Method = WebRequestMethods.Ftp.DownloadFile;
                if (await request.GetResponseAsync().ConfigureAwait(false) is FtpWebResponse response)
                {
                    using CryptoStream csEncrypt = new (response.GetResponseStream(), access.CreateDecryptor(access.Key, access.IV), CryptoStreamMode.Read);
                    using FileStream stream = File.Open("/storage/emulated/0/documents/" + path, FileMode.Create);
                    await csEncrypt.CopyToAsync(stream);
                }
            }
        }
#pragma warning restore SYSLIB0014 // Type or member is obsolete
    }
}