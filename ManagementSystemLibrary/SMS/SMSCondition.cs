// <copyright file="SMSCondition.cs" company="PlaceholderCompany">
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
    /// Represents a condition of the skill management system.
    /// </summary>
    public class SMSCondition : MSDataObject<SMSScenario>
    {
        private static readonly Dictionary<SMSConditionType, Action<SMSBond[], SMSBond[]>> ConditionEvaluation = new Dictionary<SMSConditionType, Action<SMSBond[], SMSBond[]>>()
        {
            { SMSConditionType.LogicAnd, EvaluateLogicAnd },
            { SMSConditionType.LogicOr, EvaluateLogicOr },
            { SMSConditionType.LogicXor, EvaluateLogicXor },
            { SMSConditionType.LogicNot, EvaluateLogicNot },
            { SMSConditionType.LogicNot, EvaluateLogicNot },
        };

        private SMSConditionType? type;

        /// <summary>
        /// Initializes a new instance of the <see cref="SMSCondition"/> class.
        /// </summary>
        /// <param name="scenario">The parent <see cref="SMSScenario"/> of the <see cref="SMSCondition"/>.</param>
        /// <param name="id">The identifier of the <see cref="SMSCondition"/>.</param>
        public SMSCondition(SMSScenario scenario, long id)
            : base(scenario, id)
        {
        }

        /// <summary>
        /// Gets or sets an array of input <see cref="SMSBond"/>.
        /// </summary>
        public SMSBond[]? Inputs { get; set; }

        /// <summary>
        /// Gets or sets an array of output <see cref="SMSBond"/>.
        /// </summary>
        public SMSBond[]? Outputs { get; set; }

        /// <summary>
        /// Gets or sets the type of the <see cref="SMSCondition"/>.
        /// </summary>
        public SMSConditionType? Type
        {
            get
            {
                _ = this.GetTypeAsync();
                return this.type;
            }

            set
            {
                _ = this.SaveTypeAsync(value);
            }
        }

        /// <summary>
        /// Creates a new <see cref="SMSCondition"/>.
        /// </summary>
        /// <param name="parent">The parent of the created <see cref="SMSCondition"/>.</param>
        /// <param name="type">The type of the created <see cref="SMSCondition"/>.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public static async Task<SMSCondition?> CreateAsync(SMSScenario parent, SMSConditionType type)
        {
            if (await CreateAsync<SMSCondition>(parent, type.ToString(), new byte[] { (byte)type }, null) is long id)
            {
                return new (parent, id);
            }

            return null;
        }

        /// <summary>
        /// Evaluates the inputs of the <see cref="SMSCondition"/>.
        /// </summary>
        public void Evaluate()
        {
            if (this.Inputs is not null
                && this.Outputs is not null
                && this.type is not null)
            {
                if (ConditionEvaluation.ContainsKey(this.type.Value))
                {
                    ConditionEvaluation[this.type.Value](this.Inputs, this.Outputs);
                }
            }
        }

        /// <summary>
        /// Gets the <see cref="Type"/> of the <see cref="SMSCondition"/>.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task<SMSConditionType?> GetTypeAsync()
        {
            if (this.type is null
                && (await this.GetDataAsync().ConfigureAwait(false))?.ElementAtOrDefault(0) is byte typeByte)
            {
                this.type = (SMSConditionType)typeByte;
                this.OnPropertyChanged(new PropertyChangedEventArgs(nameof(this.Type)));
            }

            return this.type;
        }

        /// <summary>
        /// Saves the <see cref="Type"/> of the <see cref="SMSCondition"/>.
        /// </summary>
        /// <param name="value">The new value.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task SaveTypeAsync(SMSConditionType? value)
        {
            if (this.type != value
                && value is not null)
            {
                this.type = value;
                this.OnPropertyChanged(new PropertyChangedEventArgs(nameof(this.Type)));
                await this.SaveDataAsync(new byte[] { (byte)this.type }).ConfigureAwait(false);
            }
        }

        private static void EvaluateLogicAnd(SMSBond[] inputs, SMSBond[] outputs)
        {
            bool? outputValue = null;
            foreach (SMSBond input in inputs)
            {
                if (input.Value is bool booleanValue
                    && booleanValue != true)
                {
                    outputValue = false;
                    break;
                }

                outputValue = true;
            }

            foreach (SMSBond output in outputs)
            {
                output.Value = outputValue;
            }
        }

        private static void EvaluateLogicOr(SMSBond[] inputs, SMSBond[] outputs)
        {
            bool? outputValue = null;
            foreach (SMSBond input in inputs)
            {
                if (input.Value is bool booleanValue
                    && booleanValue == true)
                {
                    outputValue = true;
                    break;
                }

                outputValue = false;
            }

            foreach (SMSBond output in outputs)
            {
                output.Value = outputValue;
            }
        }

        private static void EvaluateLogicXor(SMSBond[] inputs, SMSBond[] outputs)
        {
            bool? outputValue = null;
            foreach (SMSBond input in inputs)
            {
                if (input.Value is bool booleanValue
                    && booleanValue == true)
                {
                    if (outputValue == false)
                    {
                        outputValue = true;
                    }
                    else
                    {
                        outputValue = false;
                        break;
                    }
                }

                outputValue ??= false;
            }

            foreach (SMSBond output in outputs)
            {
                output.Value = outputValue;
            }
        }

        private static void EvaluateLogicNot(SMSBond[] inputs, SMSBond[] outputs)
        {
            bool? outputValue = inputs.Length != 1;
            if (inputs.ElementAtOrDefault(0)?.Value is bool booleanValue
                && outputValue is null)
            {
                outputValue = !booleanValue;
            }

            foreach (SMSBond output in outputs)
            {
                output.Value = outputValue;
            }
        }
    }
}
