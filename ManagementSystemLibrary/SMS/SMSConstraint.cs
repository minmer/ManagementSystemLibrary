// <copyright file="SMSConstraint.cs" company="PlaceholderCompany">
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
    /// Represents a constraint between a <see cref="SMSSkill"/> and a <see cref="AMSAssociation"/>.
    /// </summary>
    public class SMSConstraint : MSLinkObject<SMSSkill, AMSAssociation>
    {
        static SMSConstraint()
        {
            SMSConstraint.ChildAdministratorInitialization = (AMSAssociation association, long id, byte[] key, byte[] signature) => { return new SMSSkill(association, id, key, signature); };
            SMSConstraint.ChildContributorInitialization = (AMSAssociation association, long id, byte[] key) => { return new SMSSkill(association, id, key); };
            SMSConstraint.ChildObservatorInitialization = (AMSAssociation association, long id, Aes access) => { return new SMSSkill(association, id, access); };
            SMSConstraint.ChildPublicInitialization = (AMSAssociation association, long id) => { return new SMSSkill(association, id); };
            SMSConstraint.ParentInitialization = (AMSAssociation association, long id) => { return new AMSAssociation(association, id); };
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SMSConstraint"/> class.
        /// </summary>
        /// <param name="child">The child <see cref="SMSSkill"/> of the <see cref="SMSConstraint"/>.</param>
        /// <param name="id">The identifier of the <see cref="SMSConstraint"/>.</param>
        public SMSConstraint(SMSSkill child, long id)
            : base(child, id)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SMSConstraint"/> class.
        /// </summary>
        /// <param name="id">The identifier of the <see cref="SMSConstraint"/>.</param>
        /// <param name="parent">The parent <see cref="AMSAssociation"/> of the <see cref="SMSConstraint"/>.</param>
        public SMSConstraint(long id, AMSAssociation parent)
            : base(id, parent)
        {
        }

        /// <summary>
        /// Creates a new <see cref="SMSConstraint"/>.
        /// </summary>
        /// <param name="child">The child <see cref="SMSSkill"/> of the <see cref="SMSConstraint"/>.</param>
        /// <param name="parent">The parent <see cref="AMSAssociation"/> of the <see cref="SMSConstraint"/>.</param>
        /// <param name="type">The type of the <see cref="SMSConstraint"/>.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public static async Task<SMSConstraint?> CreateAsync(SMSSkill child, AMSAssociation parent, MSAccessType type)
        {
            if (await CreateAsync<SMSConstraint>(child, parent, type, null).ConfigureAwait(false) is long id)
            {
                return new (id, parent);
            }

            return null;
        }
    }
}
