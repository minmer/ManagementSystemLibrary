// <copyright file="SMSContender.cs" company="PlaceholderCompany">
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
    /// Represents a contender of a <see cref="SMSScenario"/>.
    /// </summary>
    public class SMSContender : MSLinkObject<SMSScenario, AMSAssociation>
    {
        static SMSContender()
        {
            SMSContender.ChildAdministratorInitialization = (AMSAssociation association, long id, byte[] key, byte[] signature) => { return new SMSScenario(association, id, key, signature); };
            SMSContender.ChildContributorInitialization = (AMSAssociation association, long id, byte[] key) => { return new SMSScenario(association, id, key); };
            SMSContender.ChildObservatorInitialization = (AMSAssociation association, long id, Aes access) => { return new SMSScenario(association, id, access); };
            SMSContender.ChildPublicInitialization = (AMSAssociation association, long id) => { return new SMSScenario(association, id); };
            SMSContender.ParentInitialization = (AMSAssociation association, long id) => { return new AMSAssociation(association, id); };
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SMSContender"/> class.
        /// </summary>
        /// <param name="child">The child <see cref="SMSScenario"/> of the <see cref="SMSContender"/>.</param>
        /// <param name="id">The identifier of the <see cref="SMSContender"/>.</param>
        public SMSContender(SMSScenario child, long id)
            : base(child, id)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SMSContender"/> class.
        /// </summary>
        /// <param name="id">The identifier of the <see cref="SMSContender"/>.</param>
        /// <param name="parent">The parent <see cref="AMSAssociation"/> of the <see cref="SMSContender"/>.</param>
        public SMSContender(long id, AMSAssociation parent)
            : base(id, parent)
        {
        }

        /// <summary>
        /// Creates a new <see cref="SMSContender"/>.
        /// </summary>
        /// <param name="child">The child <see cref="SMSScenario"/> of the <see cref="SMSContender"/>.</param>
        /// <param name="parent">The parent <see cref="AMSAssociation"/> of the <see cref="SMSContender"/>.</param>
        /// <param name="type">The type of the <see cref="SMSContender"/>.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public static async Task<SMSContender?> CreateAsync(SMSScenario child, AMSAssociation parent, MSAccessType type)
        {
            if (await CreateAsync<SMSContender>(child, parent, type, null).ConfigureAwait(false) is long id)
            {
                return new (id, parent);
            }

            return null;
        }
    }
}
