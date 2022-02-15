// <copyright file="ServerParameters.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace ManagementSystemLibrary.Pipeline
{
    using System.ComponentModel;
    using Npgsql;

    /// <summary>
    /// Represents the parameters of a <see cref="NpgsqlConnection"/>.
    /// </summary>
    public class ServerParameters : INotifyPropertyChanged
    {
        private string? database;
        private string? ftpServerAddress;
        private string? ftpUser;
        private string? ftpPassword;
        private string? owner;
        private string? password;
        private int port;
        private string? server;
        private string? user;

        /// <inheritdoc/>
        public event PropertyChangedEventHandler? PropertyChanged;

        /// <summary>
        /// Gets or sets the database of the <see cref="ServerParameters"/>.
        /// </summary>
        public string? Database
        {
            get => this.database;
            set
            {
                this.database = value;
                this.OnPropertyChanged(new PropertyChangedEventArgs(nameof(this.Database)));
            }
        }

        /// <summary>
        /// Gets or sets the ftp server address of the <see cref="ServerParameters"/>.
        /// </summary>
        public string? FtpServerAddress
        {
            get => this.ftpServerAddress;
            set
            {
                this.ftpServerAddress = value;
                this.OnPropertyChanged(new PropertyChangedEventArgs(nameof(this.FtpServerAddress)));
            }
        }

        /// <summary>
        /// Gets or sets the ftp user of the <see cref="ServerParameters"/>.
        /// </summary>
        public string? FtpUser
        {
            get => this.ftpUser;
            set
            {
                this.ftpUser = value;
                this.OnPropertyChanged(new PropertyChangedEventArgs(nameof(this.FtpUser)));
            }
        }

        /// <summary>
        /// Gets or sets the ftp password of the <see cref="ServerParameters"/>.
        /// </summary>
        public string? FtpPassword
        {
            get => this.ftpPassword;
            set
            {
                this.ftpPassword = value;
                this.OnPropertyChanged(new PropertyChangedEventArgs(nameof(this.FtpPassword)));
            }
        }

        /// <summary>
        /// Gets or sets the owner of the <see cref="ServerParameters"/>.
        /// </summary>
        public string? Owner
        {
            get => this.owner;
            set
            {
                this.owner = value;
                this.OnPropertyChanged(new PropertyChangedEventArgs(nameof(this.Owner)));
            }
        }

        /// <summary>
        /// Gets or sets the password of the <see cref="ServerParameters"/>.
        /// </summary>
        public string? Password
        {
            get => this.password;
            set
            {
                this.password = value;
                this.OnPropertyChanged(new PropertyChangedEventArgs(nameof(this.Password)));
            }
        }

        /// <summary>
        /// Gets or sets the port of the <see cref="ServerParameters"/>.
        /// </summary>
        public int Port
        {
            get => this.port;
            set
            {
                this.port = value;
                this.OnPropertyChanged(new PropertyChangedEventArgs(nameof(this.Port)));
            }
        }

        /// <summary>
        /// Gets or sets the server address of the <see cref="ServerParameters"/>.
        /// </summary>
        public string? Server
        {
            get => this.server;
            set
            {
                this.server = value;
                this.OnPropertyChanged(new PropertyChangedEventArgs(nameof(this.Server)));
            }
        }

        /// <summary>
        /// Gets or sets the user of the <see cref="ServerParameters"/>.
        /// </summary>
        public string? User
        {
            get => this.user;
            set
            {
                this.user = value;
                this.OnPropertyChanged(new PropertyChangedEventArgs(nameof(this.User)));
            }
        }

        /// <summary>
        /// Invokes the PropertyChanged of the <see cref="ServerParameters"/>.
        /// </summary>
        /// <param name="eventArgs">The PropertyChangedEventArgs of the mehtod.</param>
        protected virtual void OnPropertyChanged(PropertyChangedEventArgs eventArgs)
        {
            this.PropertyChanged?.Invoke(this, eventArgs);
        }
    }
}
