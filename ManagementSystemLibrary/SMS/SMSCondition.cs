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
            { SMSConditionType.Message, EvaluateMessage },
            { SMSConditionType.QuestionYesNo, EvaluateQuestionYesNo },
            { SMSConditionType.IdentityCheck, EvaluateIdentityCheck },
            { SMSConditionType.Start, EvaluateStatic },
            { SMSConditionType.Static, EvaluateStatic },
            { SMSConditionType.Input, EvaluateStatic },
            { SMSConditionType.SkillUpdate, EvaluateSkillUpdate },
            { SMSConditionType.IfStatement, EvaluateIfStatement },
            { SMSConditionType.IfElseStatement, EvaluateIfElseStatement },
            { SMSConditionType.Reset, EvaluateReset },
        };

        private SMSConditionType? type;
        private object? value;

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
        /// Occurs when an output value changed.
        /// </summary>
        public event EventHandler<SMSOutputEventArgs>? OutputChanged;

        /// <summary>
        /// Gets or sets an function for a message.
        /// </summary>
        public static Func<string, string, Task>? InvokeMessageAsync { get; set; }

        /// <summary>
        /// Gets or sets an function for a question with yes/no answer.
        /// </summary>
        public static Func<string, string, Task<bool>>? InvokeQuestionYesNoAsync { get; set; }

        /// <summary>
        /// Gets a list of input <see cref="SMSBond"/>.
        /// </summary>
        public List<SMSBond> Inputs { get; } = new ();

        /// <summary>
        /// Gets a list of output <see cref="SMSBond"/>.
        /// </summary>
        public List<SMSBond> Outputs { get; } = new ();

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
        /// Gets or sets the value of the <see cref="SMSCondition"/>.
        /// </summary>
        public object? Value
        {
            get
            {
                _ = this.GetValueAsync();
                return this.value;
            }

            set
            {
                _ = this.SaveValueAsync(value);
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
            if (this.type is not null)
            {
                if (ConditionEvaluation.ContainsKey(this.type.Value))
                {
                    ConditionEvaluation[this.type.Value](this.Inputs.ToArray(), this.Outputs.ToArray());
                }
                else if (this.type.Value == SMSConditionType.Output
                    && (this.value ?? -1) is int index)
                {
                    this.OnOutputChanged(new SMSOutputEventArgs { Index = index, Value = this.Inputs.FirstOrDefault(input => input.Value is not null)?.Value });
                }
                else if (this.type.Value == SMSConditionType.Task
                    && this.value is SMSTask task)
                {
                    this.EvaluateTask(task);
                }
                else
                {
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
        /// Gets the <see cref="Value"/> of the <see cref="SMSCondition"/>.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task<object?> GetValueAsync()
        {
            if (this.value is null
                && await this.GetDataAsync().ConfigureAwait(false) is byte[] array
                && array?.Length > 1
                && await this.GetTypeAsync().ConfigureAwait(false) is SMSConditionType type)
            {
                if (type == SMSConditionType.Start
                    | type == SMSConditionType.Static
                    | type == SMSConditionType.Input
                    | type == SMSConditionType.Output
                    | type == SMSConditionType.Task)
                {
                    this.value = array[1..].GetObject(this.Parent.Association);
                    if (type == SMSConditionType.Start
                        | type == SMSConditionType.Static)
                    {
                        this.Inputs.Add(new SMSBond(this.Parent, -1) { Output = this, Value = this.value });
                    }
                    else if (type == SMSConditionType.Task
                        && this.value is SMSTask task)
                    {
                        task.OutputChanged += this.TaskOnOutputChanged;
                    }
                    else
                    {
                    }

                    this.OnPropertyChanged(new PropertyChangedEventArgs(nameof(this.Value)));
                }
            }

            return this.value;
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

        /// <summary>
        /// Saves the <see cref="Value"/> of the <see cref="SMSCondition"/>.
        /// </summary>
        /// <param name="value">The new value.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task SaveValueAsync(object? value)
        {
            if (this.value != value
                && value is not null
                && await this.GetTypeAsync().ConfigureAwait(false) is SMSConditionType type
                && value?.GetBytes() is IEnumerable<byte> valueArray)
            {
                this.value = value;
                this.OnPropertyChanged(new PropertyChangedEventArgs(nameof(this.Value)));

                await this.SaveDataAsync(new byte[] { (byte)type }.Concat(valueArray).ToArray()).ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Invokes the OutputChanged of the <see cref="SMSCondition"/>.
        /// </summary>
        /// <param name="eventArgs">The <see cref="SMSOutputEventArgs"/> of the mehtod.</param>
        protected virtual void OnOutputChanged(SMSOutputEventArgs eventArgs)
        {
            this.OutputChanged?.Invoke(this, eventArgs);
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
            bool? outputValue = inputs.Length == 1;
            if (inputs.ElementAtOrDefault(0)?.Value is bool booleanValue
                && outputValue is true)
            {
                outputValue = !booleanValue;
            }
            else
            {
                outputValue = null;
            }

            foreach (SMSBond output in outputs)
            {
                output.Value = outputValue;
            }
        }

        private static void EvaluateMessage(SMSBond[] inputs, SMSBond[] outputs)
        {
            if (inputs.FirstOrDefault(input => input.OutputIndex == 0)?.Value is bool isVisible
                && inputs.FirstOrDefault(input => input.OutputIndex == 1)?.Value is string title
                && inputs.FirstOrDefault(input => input.OutputIndex == 2)?.Value is string message
                && InvokeMessageAsync is not null)
            {
                if (isVisible)
                {
                    Task.Run(async () =>
                    {
                        await InvokeMessageAsync(title, message).ConfigureAwait(false);
                        foreach (SMSBond output in outputs)
                        {
                            output.Value = true;
                        }
                    });
                }
            }
        }

        private static void EvaluateQuestionYesNo(SMSBond[] inputs, SMSBond[] outputs)
        {
            if (inputs.FirstOrDefault(input => input.OutputIndex == 0)?.Value is bool isVisible
                && inputs.FirstOrDefault(input => input.OutputIndex == 1)?.Value is string title
                && inputs.FirstOrDefault(input => input.OutputIndex == 2)?.Value is string message
                && InvokeQuestionYesNoAsync is not null)
            {
                if (isVisible)
                {
                    Task.Run(async () =>
                    {
                        bool outputValue = await InvokeQuestionYesNoAsync(title, message).ConfigureAwait(false);
                        foreach (SMSBond output in outputs)
                        {
                            output.Value = outputValue;
                        }
                    });
                }
            }
        }

        private static void EvaluateIdentityCheck(SMSBond[] inputs, SMSBond[] outputs)
        {
            bool? outputValue = inputs.Length > 1;
            if (outputValue == true)
            {
                if (inputs.FirstOrDefault()?.Value is MSDatabaseObject reference)
                {
                    foreach (SMSBond input in inputs)
                    {
                        if (input.Value is MSDatabaseObject pair)
                        {
                            if (pair.GetType() != reference.GetType()
                                | pair.ID != reference.ID)
                            {
                                outputValue = false;
                                break;
                            }
                        }
                        else
                        {
                            outputValue = false;
                            break;
                        }
                    }
                }
            }
            else
            {
                outputValue = null;
            }

            foreach (SMSBond output in outputs)
            {
                output.Value = outputValue;
            }
        }

        private static void EvaluateStatic(SMSBond[] inputs, SMSBond[] outputs)
        {
            if (inputs.FirstOrDefault()?.Value is object value)
            {
                foreach (SMSBond output in outputs)
                {
                    output.Value = value;
                }
            }
        }

        private static void EvaluateSkillUpdate(SMSBond[] inputs, SMSBond[] outputs)
        {
            if (inputs.FirstOrDefault(input => input.OutputIndex == 0)?.Value is bool isActive
                && inputs.FirstOrDefault(input => input.OutputIndex == 1)?.Value is SMSSkill skill
                && inputs.FirstOrDefault(input => input.OutputIndex == 2)?.Value is string name
                && inputs.FirstOrDefault(input => input.OutputIndex == 3)?.Value is double amount)
            {
                if (isActive)
                {
                    Task.Run(async () =>
                    {
                        await SMSUpdate.CreateAsync(skill, name, amount).ConfigureAwait(false);
                        foreach (SMSBond output in outputs)
                        {
                            output.Value = true;
                        }
                    });
                }
            }
        }

        private static void EvaluateIfStatement(SMSBond[] inputs, SMSBond[] outputs)
        {
            if (inputs.FirstOrDefault(input => input.OutputIndex == 0)?.Value is bool isActive)
            {
                if (isActive)
                {
                    foreach (SMSBond output in outputs)
                    {
                        output.Value = inputs.FirstOrDefault(input => input.OutputIndex == 1)?.Value;
                    }
                }
            }
        }

        private static void EvaluateIfElseStatement(SMSBond[] inputs, SMSBond[] outputs)
        {
            if (inputs.FirstOrDefault(input => input.OutputIndex == 0)?.Value is bool isActive)
            {
                object? outputValue = inputs.FirstOrDefault(input => input.OutputIndex == 2)?.Value;
                if (isActive)
                {
                    outputValue = inputs.FirstOrDefault(input => input.OutputIndex == 1)?.Value;
                }

                foreach (SMSBond output in outputs)
                {
                    output.Value = outputValue;
                }
            }
        }

        private static void EvaluateReset(SMSBond[] inputs, SMSBond[] outputs)
        {
            if (inputs.FirstOrDefault(input => input.Value is not null) is SMSBond input)
            {
                foreach (SMSBond output in outputs)
                {
                    output.Value = input.Value;
                }

                input.Value = null;
            }
        }

        private void EvaluateTask(SMSTask task)
        {
            Task.Run(async () =>
            {
                await task.ExecuteAsync(this.Inputs.Select(input => (input.OutputIndex ?? -1, input.Value)).ToArray()).ConfigureAwait(false);
            });
        }

        private void TaskOnOutputChanged(object? sender, SMSOutputEventArgs eventArgs)
        {
            foreach (SMSBond output in this.Outputs.Where(output => output.InputIndex == eventArgs.Index))
            {
                output.Value = eventArgs.Value;
            }
        }
    }
}
