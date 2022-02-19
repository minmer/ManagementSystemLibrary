// <copyright file="AMSAssociation.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace ManagementSystemLibrary.AMS
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Linq;
    using System.Security.Cryptography;
    using System.Text;
    using System.Threading.Tasks;
    using ManagementSystemLibrary.ManagementSystem;
    using ManagementSystemLibrary.Pipeline;
    using ManagementSystemLibrary.PMS;
    using ManagementSystemLibrary.SMS;
    using ManagementSystemLibrary.TMS;
    using Npgsql;
    using NpgsqlTypes;

    /// <summary>
    /// Represents a association.
    /// </summary>
    public class AMSAssociation : MSAccessObject
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AMSAssociation"/> class.
        /// </summary>
        /// <param name="association">The <see cref="AMSAssociation"/> calling the <see cref="AMSAssociation"/>.</param>
        /// <param name="id">The identifier of the <see cref="AMSAssociation"/>.</param>
        /// <param name="key">The private <see cref="RSA"/> key of the <see cref="AMSAssociation"/>.</param>
        /// <param name="signature">The private <see cref="RSA"/> signature of the <see cref="AMSAssociation"/>.</param>
        public AMSAssociation(AMSAssociation association, long id, byte[] key, byte[] signature)
            : base(association, id, key, signature)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AMSAssociation"/> class.
        /// </summary>
        /// <param name="association">The <see cref="AMSAssociation"/> calling the <see cref="AMSAssociation"/>.</param>
        /// <param name="id">The identifier of the <see cref="AMSAssociation"/>.</param>
        /// <param name="key">The private <see cref="RSA"/> key of the <see cref="AMSAssociation"/>.</param>
        public AMSAssociation(AMSAssociation association, long id, byte[] key)
            : base(association, id, key)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AMSAssociation"/> class.
        /// </summary>
        /// <param name="association">The <see cref="AMSAssociation"/> calling the <see cref="AMSAssociation"/>.</param>
        /// <param name="id">The identifier of the <see cref="AMSAssociation"/>.</param>
        /// <param name="access">The key of the <see cref="AMSAssociation"/>.</param>
        public AMSAssociation(AMSAssociation association, long id, Aes access)
            : base(association, id, access)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AMSAssociation"/> class.
        /// </summary>
        /// <param name="association">The <see cref="AMSAssociation"/> calling the <see cref="AMSAssociation"/>.</param>
        /// <param name="id">The identifier of the <see cref="AMSAssociation"/>.</param>
        public AMSAssociation(AMSAssociation association, long id)
            : base(association, id)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AMSAssociation"/> class.
        /// </summary>
        /// <param name="account">The <see cref="AMSAccount"/> of the <see cref="AMSAssociation"/>.</param>
        /// <param name="id">The identifier of the <see cref="AMSAssociation"/>.</param>
        /// <param name="key">The private <see cref="RSA"/> key of the <see cref="AMSAssociation"/>.</param>
        /// <param name="signature">The private <see cref="RSA"/> signature of the <see cref="AMSAssociation"/>.</param>
        internal AMSAssociation(AMSAccount account, long id, byte[] key, byte[] signature)
            : base(account, id, key, signature)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AMSAssociation"/> class.
        /// </summary>
        /// <param name="pipeline">The <see cref="Pipeline"/> of the <see cref="AMSAssociation"/>.</param>
        /// <param name="id">The identifier of the <see cref="AMSAssociation"/>.</param>
        internal AMSAssociation(Pipeline pipeline, long id)
            : base(pipeline, id)
        {
        }

        /// <inheritdoc/>
        public override AMSAccount Account
        {
            get
            {
                if (this == this.Association)
                {
                    return this.ProtectedAccount ?? new AMSAccount(this.Association, -1);
                }

                return this.Association.Account;
            }
        }

        /// <summary>
        /// Creates a new <see cref="AMSAssociation"/>.
        /// </summary>
        /// <param name="association">The <see cref="AMSAssociation"/> that creates the <see cref="AMSAssociation"/>.</param>
        /// <param name="name">The name of the <see cref="AMSAssociation"/>.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public static async Task<AMSAssociation?> CreateAsync(AMSAssociation association, string name)
        {
            if (Array.Empty<byte>() is byte[] keyArray
                && Array.Empty<byte>() is byte[] signatureArray
                && await CreateAsync<AMSAssociation>(association, name, (PipelineItem _, NpgsqlCommand _, DateTime _, AMSAssociation _, Aes _, string _, RSA key, RSA signature, StringBuilder _) =>
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
        /// Loads <see cref="AMSAssociate"/> associated to the <see cref="AMSAssociation"/>.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task<IEnumerable<AMSAssociate>> LoadAssociatesAsync()
        {
            return (await this.LoadParentsAsync<AMSAssociate, AMSAssociation, AMSAssociation>().ConfigureAwait(false)).Select(id => new AMSAssociate(this, id));
        }

        /// <summary>
        /// Loads <see cref="AMSAssociation"/> associated to the <see cref="AMSAssociation"/>.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task<IEnumerable<AMSAssociate>> LoadAssociationsAsync()
        {
            return (await this.LoadChildrenAsync<AMSAssociate, AMSAssociation, AMSAssociation>().ConfigureAwait(false)).Select(id => new AMSAssociate(id, this));
        }

        /// <summary>
        /// Loads <see cref="TMSRole"/> related to the <see cref="AMSAssociation"/>.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task<IEnumerable<TMSRole>> LoadTalksAsync()
        {
            return (await this.LoadChildrenAsync<TMSRole, TMSTalk, AMSAssociation>().ConfigureAwait(false)).Select(id => new TMSRole(id, this));
        }

        /// <summary>
        /// Loads <see cref="PMSAffiliate"/> related to the <see cref="AMSAssociation"/>.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task<IEnumerable<PMSAffiliate>> LoadPlannersAsync()
        {
            return (await this.LoadChildrenAsync<PMSAffiliate, PMSPlanner, AMSAssociation>().ConfigureAwait(false)).Select(id => new PMSAffiliate(id, this));
        }

        /// <summary>
        /// Loads <see cref="SMSScenario"/> related to the <see cref="AMSAssociation"/>.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task<IEnumerable<SMSContender>> LoadScenariosAsync()
        {
            return (await this.LoadChildrenAsync<SMSContender, SMSScenario, AMSAssociation>().ConfigureAwait(false)).Select(id => new SMSContender(id, this));
        }

        /// <summary>
        /// Loads <see cref="SMSSkill"/> related to the <see cref="AMSAssociation"/>.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task<IEnumerable<SMSConstraint>> LoadSkillsAsync()
        {
            return (await this.LoadChildrenAsync<SMSConstraint, SMSSkill, AMSAssociation>().ConfigureAwait(false)).Select(id => new SMSConstraint(id, this));
        }
    }
}
