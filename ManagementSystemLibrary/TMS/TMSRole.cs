// <copyright file="TMSRole.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace ManagementSystemLibrary.TMS
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
    /// Represents a role of <see cref="TMSTalk"/>.
    /// </summary>
    public class TMSRole : MSLinkObject<TMSTalk, AMSAssociation>
    {
        static TMSRole()
        {
            TMSRole.ChildAdministratorInitialization = (AMSAssociation association, long id, byte[] key, byte[] signature) => { return new TMSTalk(association, id, key, signature); };
            TMSRole.ChildContributorInitialization = (AMSAssociation association, long id, byte[] key) => { return new TMSTalk(association, id, key); };
            TMSRole.ChildObservatorInitialization = (AMSAssociation association, long id, Aes access) => { return new TMSTalk(association, id, access); };
            TMSRole.ChildPublicInitialization = (AMSAssociation association, long id) => { return new TMSTalk(association, id); };
            TMSRole.ParentInitialization = (AMSAssociation association, long id) => { return new AMSAssociation(association, id); };
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TMSRole"/> class.
        /// </summary>
        /// <param name="child">The child <see cref="TMSTalk"/> of the <see cref="TMSRole"/>.</param>
        /// <param name="id">The identifier of the <see cref="TMSRole"/>.</param>
        public TMSRole(TMSTalk child, long id)
            : base(child, id)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TMSRole"/> class.
        /// </summary>
        /// <param name="id">The identifier of the <see cref="TMSRole"/>.</param>
        /// <param name="parent">The parent <see cref="AMSAssociation"/> of the <see cref="TMSRole"/>.</param>
        public TMSRole(long id, AMSAssociation parent)
            : base(id, parent)
        {
        }

        /// <summary>
        /// Creates a new <see cref="TMSRole"/>.
        /// </summary>
        /// <param name="child">The child <see cref="TMSTalk"/> of the <see cref="TMSRole"/>.</param>
        /// <param name="parent">The parent <see cref="AMSAssociation"/> of the <see cref="TMSRole"/>.</param>
        /// <param name="type">The type of the <see cref="TMSRole"/>.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public static async Task<TMSRole?> CreateAsync(TMSTalk child, AMSAssociation parent, MSAccessType type)
        {
            if (await CreateAsync<TMSRole>(child, parent, type, null).ConfigureAwait(false) is long id)
            {
                return new (id, parent);
            }

            return null;
        }
    }
}
