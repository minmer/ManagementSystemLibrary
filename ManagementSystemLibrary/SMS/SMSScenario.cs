// <copyright file="SMSScenario.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace ManagementSystemLibrary.SMS
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Security.Cryptography;
    using System.Text;
    using System.Threading.Tasks;
    using ManagementSystemLibrary.AMS;
    using ManagementSystemLibrary.ManagementSystem;
    using ManagementSystemLibrary.Pipeline;
    using Npgsql;

    /// <summary>
    /// Represents a scenario of the skill management system.
    /// </summary>
    public class SMSScenario : MSAccessObject
    {
        private SMSCondition[]? conditions;

        /// <summary>
        /// Initializes a new instance of the <see cref="SMSScenario"/> class.
        /// </summary>
        /// <param name="association">The <see cref="AMSAssociation"/> of the <see cref="SMSScenario"/>.</param>
        /// <param name="id">The identifier of the <see cref="SMSScenario"/>.</param>
        /// <param name="key">The private <see cref="RSA"/> key of the <see cref="SMSScenario"/>.</param>
        /// <param name="signature">The private <see cref="RSA"/> signature of the <see cref="SMSScenario"/>.</param>
        public SMSScenario(AMSAssociation association, long id, byte[] key, byte[] signature)
            : base(association, id, key, signature)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SMSScenario"/> class.
        /// </summary>
        /// <param name="association">The <see cref="AMSAssociation"/> of the <see cref="SMSScenario"/>.</param>
        /// <param name="id">The identifier of the <see cref="SMSScenario"/>.</param>
        /// <param name="key">The private <see cref="RSA"/> key of the <see cref="SMSScenario"/>.</param>
        public SMSScenario(AMSAssociation association, long id, byte[] key)
            : base(association, id, key)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SMSScenario"/> class.
        /// </summary>
        /// <param name="association">The <see cref="AMSAssociation"/> of the <see cref="SMSScenario"/>.</param>
        /// <param name="id">The identifier of the <see cref="SMSScenario"/>.</param>
        /// <param name="access">The key of the <see cref="SMSScenario"/>.</param>
        public SMSScenario(AMSAssociation association, long id, Aes access)
            : base(association, id, access)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SMSScenario"/> class.
        /// </summary>
        /// <param name="association">The <see cref="AMSAssociation"/> of the <see cref="SMSScenario"/>.</param>
        /// <param name="id">The identifier of the <see cref="SMSScenario"/>.</param>
        public SMSScenario(AMSAssociation association, long id)
            : base(association, id)
        {
        }

        /// <summary>
        /// Creates a new <see cref="SMSScenario"/>.
        /// </summary>
        /// <param name="association">The <see cref="AMSAssociation"/> that creates the <see cref="SMSScenario"/>.</param>
        /// <param name="name">The name of the <see cref="SMSScenario"/>.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public static async Task<SMSScenario?> CreateAsync(AMSAssociation association, string name)
        {
            if (Array.Empty<byte>() is byte[] keyArray
                && Array.Empty<byte>() is byte[] signatureArray
                && await MSAccessObject.CreateAsync<SMSScenario>(association, name, (PipelineItem _, NpgsqlCommand _, DateTime _, AMSAssociation _, Aes _, string _, RSA key, RSA signature, StringBuilder _) =>
            {
                keyArray = key.ExportRSAPrivateKey();
                signatureArray = signature.ExportRSAPrivateKey();
            }).ConfigureAwait(false) is long id
            && keyArray.Length > 0
            && signatureArray.Length > 0)
            {
                return new (association, id, keyArray, signatureArray);
            }

            return null;
        }

        /// <summary>
        /// Executes the <see cref="SMSScenario"/>.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task<IEnumerable<SMSCondition>> PrepareAsync()
        {
            if (this.conditions is null)
            {
                this.conditions = (await this.LoadConditionsAsync().ConfigureAwait(false)).ToArray();
                SMSBond[] bonds = (await this.LoadBondsAsync().ConfigureAwait(false)).ToArray();
                await PrepareBondsAsync(this.conditions, bonds).ConfigureAwait(false);
                await PrepareStaticConditionsAsync(this.conditions).ConfigureAwait(false);
            }

            return this.conditions;
        }

        /// <summary>
        /// Loads <see cref="SMSContender"/> related to the <see cref="SMSScenario"/>.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task<IEnumerable<SMSContender>> LoadContendersAsync()
        {
            return (await this.LoadParentsAsync<SMSContender, SMSScenario, AMSAssociation>().ConfigureAwait(false)).Select(id => new SMSContender(this, id));
        }

        /// <summary>
        /// Loads <see cref="SMSCondition"/> related to the <see cref="SMSScenario"/>.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task<IEnumerable<SMSCondition>> LoadConditionsAsync()
        {
            return (await this.LoadItemsAsync<SMSCondition, SMSScenario>().ConfigureAwait(false)).Select(id => new SMSCondition(this, id));
        }

        /// <summary>
        /// Loads <see cref="SMSBond"/> related to the <see cref="SMSScenario"/>.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task<IEnumerable<SMSBond>> LoadBondsAsync()
        {
            return (await this.LoadItemsAsync<SMSBond, SMSScenario>().ConfigureAwait(false)).Select(id => new SMSBond(this, id));
        }

        private static async Task PrepareBondsAsync(SMSCondition[] conditions, SMSBond[] bonds)
        {
            foreach (SMSBond bond in bonds)
            {
                await bond.GetBondAsync().ConfigureAwait(false);
                if (conditions.FirstOrDefault(condition => condition.ID == bond.InputID) is SMSCondition inputCondition)
                {
                    bond.Input = inputCondition;
                    inputCondition.Outputs.Add(bond);
                }

                if (conditions.FirstOrDefault(condition => condition.ID == bond.OutputID) is SMSCondition outputCondition)
                {
                    bond.Output = outputCondition;
                    outputCondition.Inputs.Add(bond);
                }
            }
        }

        private static async Task PrepareStaticConditionsAsync(SMSCondition[] conditions)
        {
            for (int index = 0; index < conditions.Length; index++)
            {
                if (await conditions[index].GetTypeAsync().ConfigureAwait(false) == SMSConditionType.Static
                    | conditions[index].Type == SMSConditionType.Start
                    | conditions[index].Type == SMSConditionType.Output
                    | conditions[index].Type == SMSConditionType.Task)
                {
                    _ = await conditions[index].GetValueAsync().ConfigureAwait(false);
                }
            }

            foreach (SMSCondition staticCondition in conditions.Where(condition => condition.Type == SMSConditionType.Static))
            {
                staticCondition.Evaluate();
            }
        }
    }
}
