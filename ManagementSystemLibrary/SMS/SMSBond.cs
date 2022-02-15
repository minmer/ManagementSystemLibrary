// <copyright file="SMSBond.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace ManagementSystemLibrary.SMS
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using ManagementSystemLibrary.ManagementSystem;

    /// <summary>
    /// Represents a skill.
    /// </summary>
    public class SMSBond : MSDataObject<SMSScenario>
    {
        private SMSCondition? input;
        private long? inputID;
        private int? inputIndex;
        private SMSCondition? output;
        private long? outputID;
        private int? outputIndex;
        private object? value;

        /// <summary>
        /// Initializes a new instance of the <see cref="SMSBond"/> class.
        /// </summary>
        /// <param name="scenario">The parent <see cref="SMSScenario"/> of the <see cref="SMSBond"/>.</param>
        /// <param name="id">The identifier of the <see cref="SMSBond"/>.</param>
        public SMSBond(SMSScenario scenario, long id)
            : base(scenario, id)
        {
        }

        /// <summary>
        /// Gets or sets the input <see cref="SMSBond"/>.
        /// </summary>
        public SMSCondition? Input
        {
            get
            {
                return this.input;
            }

            set
            {
                this.input = value;
                this.inputID = this.input?.ID;
                _ = this.SaveBondAsync();
            }
        }

        /// <summary>
        /// Gets or sets the <see cref="Input"/> ID.
        /// </summary>
        public long? InputID
        {
            get
            {
                _ = this.GetBondAsync();
                return this.inputID;
            }

            set
            {
                this.inputID = value;
                _ = this.SaveBondAsync();
            }
        }

        /// <summary>
        /// Gets or sets the index of the <see cref="Input"/>.
        /// </summary>
        public int? InputIndex
        {
            get
            {
                _ = this.GetBondAsync();
                return this.inputIndex;
            }

            set
            {
                this.inputIndex = value;
                _ = this.SaveBondAsync();
            }
        }

        /// <summary>
        /// Gets or sets the output <see cref="SMSBond"/>.
        /// </summary>
        public SMSCondition? Output
        {
            get
            {
                return this.output;
            }

            set
            {
                this.output = value;
                this.outputID = this.output?.ID;
                _ = this.SaveBondAsync();
            }
        }

        /// <summary>
        /// Gets or sets the <see cref="Output"/> ID.
        /// </summary>
        public long? OutputID
        {
            get
            {
                _ = this.GetBondAsync();
                return this.outputID;
            }

            set
            {
                this.outputID = value;
                _ = this.SaveBondAsync();
            }
        }

        /// <summary>
        /// Gets or sets the index of the <see cref="Output"/>.
        /// </summary>
        public int? OutputIndex
        {
            get
            {
                _ = this.GetBondAsync();
                return this.outputIndex;
            }

            set
            {
                this.outputIndex = value;
                _ = this.SaveBondAsync();
            }
        }

        /// <summary>
        /// Gets or sets the value of the <see cref="SMSBond"/>.
        /// </summary>
        public object? Value
        {
            get
            {
                return this.value;
            }

            set
            {
                if (this.value != value)
                {
                    this.value = value;
                    this.Output?.Evaluate();
                }
            }
        }

        /// <summary>
        /// Creates a new <see cref="SMSBond"/>.
        /// </summary>
        /// <param name="input">The input <see cref="SMSCondition"/> of the created <see cref="SMSBond"/>.</param>
        /// <param name="inputIndex">The index of input <see cref="SMSCondition"/>.</param>
        /// <param name="output">The output <see cref="SMSCondition"/> of the created <see cref="SMSBond"/>.</param>
        /// <param name="outputIndex">The index of output <see cref="SMSCondition"/>.</param>
        /// <param name="name">The name of the created <see cref="SMSBond"/>.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public static async Task<SMSBond?> CreateAsync(SMSCondition input, int inputIndex, SMSCondition output, int outputIndex, string name)
        {
            if (await CreateAsync<SMSBond>(input.Parent, name, BitConverter.GetBytes(input.ID).Concat(BitConverter.GetBytes(inputIndex)).Concat(BitConverter.GetBytes(output.ID)).Concat(BitConverter.GetBytes(outputIndex)).ToArray(), null) is long id)
            {
                return new (input.Parent, id) { Input = input, Output = output };
            }

            return null;
        }

        /// <summary>
        /// Gets the parameters of the <see cref="SMSBond"/>.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task GetBondAsync()
        {
            if ((this.inputID is null
                | this.inputIndex is null
                | this.outputID is null
                | this.outputIndex is null)
                && await this.GetDataAsync().ConfigureAwait(false) is byte[] array)
            {
                this.inputID ??= BitConverter.ToInt64(array, 0);
                this.inputIndex ??= BitConverter.ToInt32(array, 8);
                this.outputID ??= BitConverter.ToInt64(array, 12);
                this.outputIndex ??= BitConverter.ToInt32(array, 20);
                this.OnParametersChanged();
            }
        }

        /// <summary>
        /// Saves the parameters of the <see cref="SMSBond"/>.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task SaveBondAsync()
        {
            await this.GetBondAsync().ConfigureAwait(false);
            if (this.inputID is not null
                && this.inputIndex is not null
                && this.outputID is not null
                && this.outputIndex is not null)
            {
                this.OnParametersChanged();
                await this.SaveDataAsync(BitConverter.GetBytes(this.inputID.Value).Concat(BitConverter.GetBytes(this.inputIndex.Value)).Concat(BitConverter.GetBytes(this.outputID.Value)).Concat(BitConverter.GetBytes(this.outputIndex.Value)).ToArray()).ConfigureAwait(false);
            }
        }

        private void OnParametersChanged()
        {
            this.OnPropertyChanged(new PropertyChangedEventArgs(nameof(this.InputID)));
            this.OnPropertyChanged(new PropertyChangedEventArgs(nameof(this.InputIndex)));
            this.OnPropertyChanged(new PropertyChangedEventArgs(nameof(this.OutputID)));
            this.OnPropertyChanged(new PropertyChangedEventArgs(nameof(this.OutputIndex)));
        }
    }
}
