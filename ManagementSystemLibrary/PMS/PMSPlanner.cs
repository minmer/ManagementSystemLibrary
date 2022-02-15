// <copyright file="PMSPlanner.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace ManagementSystemLibrary.PMS
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
    /// Represents a planner of the planner management system.
    /// </summary>
    public class PMSPlanner : MSScheduleObject<PMSPlanner, PMSAppointment>
    {
        static PMSPlanner()
        {
            ChildInitialization = (PMSPlanner planner, long id) => { return new PMSAppointment(planner, id); };
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PMSPlanner"/> class.
        /// </summary>
        /// <param name="association">The <see cref="AMSAssociation"/> of the <see cref="PMSPlanner"/>.</param>
        /// <param name="id">The identifier of the <see cref="PMSPlanner"/>.</param>
        /// <param name="key">The private <see cref="RSA"/> key of the <see cref="PMSPlanner"/>.</param>
        /// <param name="signature">The private <see cref="RSA"/> signature of the <see cref="PMSPlanner"/>.</param>
        public PMSPlanner(AMSAssociation association, long id, byte[] key, byte[] signature)
            : base(association, id, key, signature)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PMSPlanner"/> class.
        /// </summary>
        /// <param name="association">The <see cref="AMSAssociation"/> of the <see cref="PMSPlanner"/>.</param>
        /// <param name="id">The identifier of the <see cref="PMSPlanner"/>.</param>
        /// <param name="key">The private <see cref="RSA"/> key of the <see cref="PMSPlanner"/>.</param>
        public PMSPlanner(AMSAssociation association, long id, byte[] key)
            : base(association, id, key)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PMSPlanner"/> class.
        /// </summary>
        /// <param name="association">The <see cref="AMSAssociation"/> of the <see cref="PMSPlanner"/>.</param>
        /// <param name="id">The identifier of the <see cref="PMSPlanner"/>.</param>
        /// <param name="access">The key of the <see cref="PMSPlanner"/>.</param>
        public PMSPlanner(AMSAssociation association, long id, Aes access)
            : base(association, id, access)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PMSPlanner"/> class.
        /// </summary>
        /// <param name="association">The <see cref="AMSAssociation"/> of the <see cref="PMSPlanner"/>.</param>
        /// <param name="id">The identifier of the <see cref="PMSPlanner"/>.</param>
        public PMSPlanner(AMSAssociation association, long id)
            : base(association, id)
        {
        }

        /// <summary>
        /// Creates a new <see cref="PMSPlanner"/>.
        /// </summary>
        /// <param name="association">The <see cref="AMSAssociation"/> that creates the <see cref="PMSPlanner"/>.</param>
        /// <param name="name">The name of the <see cref="PMSPlanner"/>.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public static async Task<PMSPlanner?> CreateAsync(AMSAssociation association, string name)
        {
            if (Array.Empty<byte>() is byte[] keyArray
                && Array.Empty<byte>() is byte[] signatureArray
                && await MSScheduleObject<PMSPlanner, PMSAppointment>.CreateAsync<PMSPlanner>(association, name, (PipelineItem _, NpgsqlCommand _, DateTime _, AMSAssociation _, Aes _, string _, RSA key, RSA signature, double _, double _, StringBuilder _) =>
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
        /// Loads <see cref="PMSAffiliate"/> related to the <see cref="PMSPlanner"/>.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task<IEnumerable<PMSAffiliate>> LoadAffiliatesAsync()
        {
            return (await this.LoadParentsAsync<PMSAffiliate, PMSPlanner, AMSAssociation>().ConfigureAwait(false)).Select(id => new PMSAffiliate(this, id));
        }

        /// <summary>
        /// Loads <see cref="PMSAppointment"/> related to the <see cref="PMSPlanner"/>.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task<IEnumerable<PMSAppointment>> LoadAppointmentsAsync()
        {
            return (await this.LoadItemsAsync<PMSPlanner, PMSAppointment>().ConfigureAwait(false)).Select(id => new PMSAppointment(this, id));
        }
    }
}
