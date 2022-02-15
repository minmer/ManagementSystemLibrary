// <copyright file="AMSAssociate.cs" company="PlaceholderCompany">
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
    using Npgsql;
    using NpgsqlTypes;

    /// <summary>
    /// Represents a associatie of an <see cref="AMSAssociation"/>.
    /// </summary>
    public class AMSAssociate : MSLinkObject<AMSAssociation, AMSAssociation>
    {
        static AMSAssociate()
        {
            AMSAssociate.ChildAdministratorInitialization = (AMSAssociation association, long id, byte[] key, byte[] signature) => { return new AMSAssociation(association, id, key, signature); };
            AMSAssociate.ChildContributorInitialization = (AMSAssociation association, long id, byte[] key) => { return new AMSAssociation(association, id, key); };
            AMSAssociate.ChildObservatorInitialization = (AMSAssociation association, long id, Aes access) => { return new AMSAssociation(association, id, access); };
            AMSAssociate.ChildPublicInitialization = (AMSAssociation association, long id) => { return new AMSAssociation(association, id); };
            AMSAssociate.ParentInitialization = (AMSAssociation association, long id) => { return new AMSAssociation(association, id); };
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AMSAssociate"/> class.
        /// </summary>
        /// <param name="child">The child <see cref="AMSAssociation"/> of the <see cref="AMSAssociate"/>.</param>
        /// <param name="id">The identifier of the <see cref="AMSAssociate"/>.</param>
        public AMSAssociate(AMSAssociation child, long id)
            : base(child, id)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AMSAssociate"/> class.
        /// </summary>
        /// <param name="id">The identifier of the <see cref="AMSAssociate"/>.</param>
        /// <param name="parent">The parent <see cref="AMSAssociation"/> of the <see cref="AMSAssociate"/>.</param>
        public AMSAssociate(long id, AMSAssociation parent)
            : base(id, parent)
        {
        }

        /// <summary>
        /// Gets whether the <see cref="AMSAssociate"/> is signed.
        /// </summary>
        public bool? IsProofed { get; private set; }

        /// <summary>
        /// Creates a new <see cref="AMSAssociate"/>.
        /// </summary>
        /// <param name="child">The child <see cref="AMSAssociation"/> of the <see cref="AMSAssociate"/>.</param>
        /// <param name="parent">The parent <see cref="AMSAssociation"/> of the <see cref="AMSAssociate"/>.</param>
        /// <param name="type">The type of the <see cref="AMSAssociate"/>.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public static async Task<AMSAssociate?> CreateAsync(AMSAssociation child, AMSAssociation parent, MSAccessType type)
        {
            if (await CreateAsync<AMSAssociate>(child, parent, type, null).ConfigureAwait(false) is long id)
            {
                return new (id, parent);
            }

            return null;
        }
    }
}
