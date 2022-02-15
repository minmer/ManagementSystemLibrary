// <copyright file="PMSAffiliate.cs" company="PlaceholderCompany">
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
    /// Represents a affiliate of <see cref="PMSPlanner"/>.
    /// </summary>
    public class PMSAffiliate : MSLinkObject<PMSPlanner, AMSAssociation>
    {
        static PMSAffiliate()
        {
            PMSAffiliate.ChildAdministratorInitialization = (AMSAssociation association, long id, byte[] key, byte[] signature) => { return new PMSPlanner(association, id, key, signature); };
            PMSAffiliate.ChildContributorInitialization = (AMSAssociation association, long id, byte[] key) => { return new PMSPlanner(association, id, key); };
            PMSAffiliate.ChildObservatorInitialization = (AMSAssociation association, long id, Aes access) => { return new PMSPlanner(association, id, access); };
            PMSAffiliate.ChildPublicInitialization = (AMSAssociation association, long id) => { return new PMSPlanner(association, id); };
            PMSAffiliate.ParentInitialization = (AMSAssociation association, long id) => { return new AMSAssociation(association, id); };
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PMSAffiliate"/> class.
        /// </summary>
        /// <param name="child">The child <see cref="PMSPlanner"/> of the <see cref="PMSAffiliate"/>.</param>
        /// <param name="id">The identifier of the <see cref="PMSAffiliate"/>.</param>
        public PMSAffiliate(PMSPlanner child, long id)
            : base(child, id)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PMSAffiliate"/> class.
        /// </summary>
        /// <param name="id">The identifier of the <see cref="PMSAffiliate"/>.</param>
        /// <param name="parent">The parent <see cref="AMSAssociation"/> of the <see cref="PMSAffiliate"/>.</param>
        public PMSAffiliate(long id, AMSAssociation parent)
            : base(id, parent)
        {
        }

        /// <summary>
        /// Creates a new <see cref="PMSAffiliate"/>.
        /// </summary>
        /// <param name="child">The child <see cref="PMSPlanner"/> of the <see cref="PMSAffiliate"/>.</param>
        /// <param name="parent">The parent <see cref="AMSAssociation"/> of the <see cref="PMSAffiliate"/>.</param>
        /// <param name="type">The type of the <see cref="PMSAffiliate"/>.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public static async Task<PMSAffiliate?> CreateAsync(PMSPlanner child, AMSAssociation parent, MSAccessType type)
        {
            if (await CreateAsync<PMSAffiliate>(child, parent, type, null).ConfigureAwait(false) is long id)
            {
                return new (id, parent);
            }

            return null;
        }
    }
}
