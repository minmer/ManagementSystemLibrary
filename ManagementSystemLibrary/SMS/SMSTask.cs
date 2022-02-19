// <copyright file="SMSTask.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace ManagementSystemLibrary.SMS
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Linq;
    using System.Security.Cryptography;
    using System.Text;
    using System.Threading.Tasks;
    using ManagementSystemLibrary.AMS;
    using ManagementSystemLibrary.ManagementSystem;
    using ManagementSystemLibrary.Pipeline;
    using Npgsql;
    using NpgsqlTypes;

    /// <summary>
    /// Represents a task of an <see cref="SMSScenario"/>.
    /// </summary>
    public class SMSTask : MSAccessObject
    {
        private SMSScenario? scenario;
        private bool? scenarioVerification;
        private bool areOutputsPrepared = false;

        /// <summary>
        /// Initializes a new instance of the <see cref="SMSTask"/> class.
        /// </summary>
        /// <param name="association">The <see cref="AMSAssociation"/> of the <see cref="SMSTask"/>.</param>
        /// <param name="id">The identifier of the <see cref="SMSTask"/>.</param>
        /// <param name="key">The private <see cref="RSA"/> key of the <see cref="SMSTask"/>.</param>
        /// <param name="signature">The private <see cref="RSA"/> signature of the <see cref="SMSTask"/>.</param>
        public SMSTask(AMSAssociation association, long id, byte[] key, byte[] signature)
            : base(association, id, key, signature)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SMSTask"/> class.
        /// </summary>
        /// <param name="association">The <see cref="AMSAssociation"/> of the <see cref="SMSTask"/>.</param>
        /// <param name="id">The identifier of the <see cref="SMSTask"/>.</param>
        /// <param name="key">The private <see cref="RSA"/> key of the <see cref="SMSTask"/>.</param>
        public SMSTask(AMSAssociation association, long id, byte[] key)
            : base(association, id, key)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SMSTask"/> class.
        /// </summary>
        /// <param name="association">The <see cref="AMSAssociation"/> of the <see cref="SMSTask"/>.</param>
        /// <param name="id">The identifier of the <see cref="SMSTask"/>.</param>
        /// <param name="access">The key of the <see cref="SMSTask"/>.</param>
        public SMSTask(AMSAssociation association, long id, Aes access)
            : base(association, id, access)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SMSTask"/> class.
        /// </summary>
        /// <param name="association">The <see cref="AMSAssociation"/> of the <see cref="SMSTask"/>.</param>
        /// <param name="id">The identifier of the <see cref="SMSTask"/>.</param>
        public SMSTask(AMSAssociation association, long id)
            : base(association, id)
        {
        }

        /// <summary>
        /// Occurs when an output value changed.
        /// </summary>
        public event EventHandler<SMSOutputEventArgs>? OutputChanged;

        /// <summary>
        /// Gets the <see cref="SMSScenario"/> of the <see cref="SMSTask"/>.
        /// </summary>
        public SMSScenario? Scenario
        {
            get
            {
                _ = this.GetScenarioAsync();
                return this.scenario;
            }
        }

        /// <summary>
        /// Gets a value indicating whether the scenario of the <see cref="SMSTask"/> is verified.
        /// </summary>
        public bool? ScenarioVerification
        {
            get
            {
                _ = this.VerifyScenarioAsync();
                return this.scenarioVerification;
            }
        }

        /// <summary>
        /// Creates a new <see cref="SMSTask"/>.
        /// </summary>
        /// <param name="scenario">The <see cref="SMSScenario"/> that creates the <see cref="SMSTask"/>.</param>
        /// <param name="name">The name of the <see cref="SMSTask"/>.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public static async Task<SMSTask?> CreateAsync(SMSScenario scenario, string name)
        {
            if (Array.Empty<byte>() is byte[] keyArray
                && Array.Empty<byte>() is byte[] signatureArray
                && await scenario.GenerateHashAsync().ConfigureAwait(false) is byte[] scenarioHash
                && await scenario.GetAccessAsync().ConfigureAwait(false) is Aes scenarioAccess
                && await MSAccessObject.CreateAsync<SMSTask>(scenario.Association, name, (PipelineItem item, NpgsqlCommand command, DateTime _, AMSAssociation _, Aes access, string _, RSA key, RSA signature, StringBuilder builder) =>
            {
                builder.Append(',')
                .Append(item.AddParameter(command, "scenario", NpgsqlDbType.Bytea, access.EncryptCbc(BitConverter.GetBytes(scenario.ID).Concat(scenarioAccess.Key).Concat(scenarioAccess.IV).ToArray(), access.IV)))
                .Append(',')
                .Append(item.AddParameter(command, "scenariohash", NpgsqlDbType.Bytea, scenarioHash))
                .Append(',')
                .Append(item.AddParameter(command, "scenarioverification", NpgsqlDbType.Bytea, scenario.Account.PrivateSignature.SignData(BitConverter.GetBytes(scenario.ID), Pipeline.HashAlgorithmName, Pipeline.RSASignaturePadding)));
                keyArray = key.ExportRSAPrivateKey();
                signatureArray = signature.ExportRSAPrivateKey();
            }).ConfigureAwait(false) is long id
            && keyArray.Length > 0
            && signatureArray.Length > 0)
            {
                return new (scenario.Association, id, keyArray, signatureArray);
            }

            return null;
        }

        /// <summary>
        /// Executes the <see cref="SMSTask"/>.
        /// </summary>
        /// <param name="inputs">The input values of the excution.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task ExecuteAsync((int, object?)[] inputs)
        {
            if (await this.GetScenarioAsync().ConfigureAwait(false) is SMSScenario scenario)
            {
                SMSCondition[] conditions = (await scenario.PrepareAsync().ConfigureAwait(false)).ToArray();
                await PrepareInputsAsync(inputs, scenario, conditions).ConfigureAwait(false);
                this.PrepareOutputs(conditions);

                foreach (SMSCondition startCondition in conditions.Where(condition => condition.Type == SMSConditionType.Start))
                {
                    startCondition.Evaluate();
                }
            }
        }

        /// <summary>
        /// Gets the <see cref="SMSScenario"/> of the <see cref="SMSTask"/>.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task<SMSScenario?> GetScenarioAsync()
        {
            if (this.scenario is null
                && await this.GetAccessAsync().ConfigureAwait(false) is not null)
            {
                await new PipelineItem(this.Pipeline)
                {
                    BatchCommand = this.BasicGetByIDBatchCommand("get", "scenario"),
                    ReaderExecution = this.GetScenarioReaderExecution,
                }.ExecuteAsync().ConfigureAwait(false);
            }

            return this.scenario;
        }

        /// <summary>
        /// Verifies the <see cref="Scenario"/> of the <see cref="SMSTask"/>.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task<bool?> VerifyScenarioAsync()
        {
            if (await this.GetScenarioAsync().ConfigureAwait(false) is not null
                && await this.GetCreatorAsync().ConfigureAwait(false) is AMSAccount creator)
            {
                if (await creator.GetPublicSignatureAsync().ConfigureAwait(false) is not null)
                {
                    await new PipelineItem(this.Pipeline)
                    {
                        BatchCommand = this.BasicGetByIDBatchCommand("verify", "scenario"),
                        ReaderExecution = this.VerifyScenarioReaderExecution,
                    }.ExecuteAsync().ConfigureAwait(false);
                }
            }

            return this.scenarioVerification;
        }

        /// <summary>
        /// Invokes the OutputChanged of the <see cref="SMSCondition"/>.
        /// </summary>
        /// <param name="sender">The sender <see cref="SMSCondition"/>.</param>
        /// <param name="eventArgs">The <see cref="SMSOutputEventArgs"/> of the mehtod.</param>
        protected virtual void OnOutputChanged(object? sender, SMSOutputEventArgs eventArgs)
        {
            this.OutputChanged?.Invoke(sender, eventArgs);
        }

        private static async Task PrepareInputsAsync((int, object?)[] inputs, SMSScenario scenario, SMSCondition[] conditions)
        {
            foreach (SMSCondition inputCondition in conditions.Where(condition => condition.Type == SMSConditionType.Input))
            {
                if (await inputCondition.GetValueAsync().ConfigureAwait(false) is int index)
                {
                    inputCondition.Inputs.Clear();
                    inputCondition.Inputs.AddRange(inputs.Where(input => input.Item1 == index).Select(input => new SMSBond(scenario, -1) { Output = inputCondition, Value = input.Item2 }));
                    inputCondition.Evaluate();
                }
            }
        }

        private void PrepareOutputs(SMSCondition[] conditions)
        {
            if (!this.areOutputsPrepared)
            {
                foreach (SMSCondition outputCondition in conditions.Where(condition => condition.Type == SMSConditionType.Output))
                {
                    outputCondition.OutputChanged += this.OnOutputChanged;
                }

                this.areOutputsPrepared = true;
            }
        }

        private void GetScenarioReaderExecution(NpgsqlDataReader reader)
        {
            if (!reader.IsDBNull(1)
                && this.Access is not null
                && this.Association is not null)
            {
                byte[] array = this.Access.DecryptCbc((byte[])reader[1], this.Access.IV);
                this.scenario = new SMSScenario(this.Association, BitConverter.ToInt64(array), Aes.Create().ImportKey(array[8..40], array[40..]));
                this.OnPropertyChanged(new PropertyChangedEventArgs(nameof(this.Scenario)));
            }
        }

        private void VerifyScenarioReaderExecution(NpgsqlDataReader reader)
        {
            if (!reader.IsDBNull(1)
                && this.Creator?.PublicSignature is not null
                && this.scenario is not null)
            {
                this.scenarioVerification = this.Creator.PublicSignature.VerifyData(BitConverter.GetBytes(this.scenario.ID), (byte[])reader[1], Pipeline.HashAlgorithmName, Pipeline.RSASignaturePadding);
                this.OnPropertyChanged(new PropertyChangedEventArgs(nameof(this.ScenarioVerification)));
            }
        }
    }
}
