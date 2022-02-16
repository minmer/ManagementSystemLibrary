// <copyright file="SMSConditionType.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace ManagementSystemLibrary.SMS
{
    using ManagementSystemLibrary.ManagementSystem;

    /// <summary>
    /// Specifies the type of the <see cref="SMSCondition"/>.
    /// </summary>
    public enum SMSConditionType
    {
        /// <summary>
        /// A default <see cref="SMSCondition"/>.
        /// </summary>
        None = 0,

        /// <summary>
        /// A logical and <see cref="SMSCondition"/>.
        /// </summary>
        LogicAnd = 1,

        /// <summary>
        /// A logical or <see cref="SMSCondition"/>.
        /// </summary>
        LogicOr = 2,

        /// <summary>
        /// A logical xor <see cref="SMSCondition"/>.
        /// </summary>
        LogicXor = 3,

        /// <summary>
        /// A logical not <see cref="SMSCondition"/>.
        /// </summary>
        LogicNot = 4,

        /// <summary>
        /// A message <see cref="SMSCondition"/>.
        /// </summary>
        Message = 5,

        /// <summary>
        /// A question with yes/no answer <see cref="SMSCondition"/>.
        /// </summary>
        QuestionYesNo = 6,

        /// <summary>
        /// A identity check for <see cref="MSDatabaseObject"/> <see cref="SMSCondition"/>.
        /// </summary>
        IdentityCheck = 7,

        /// <summary>
        /// A start <see cref="SMSCondition"/>.
        /// </summary>
        Start = 8,

        /// <summary>
        /// A static <see cref="SMSCondition"/>.
        /// </summary>
        Static = 9,

        /// <summary>
        /// A <see cref="SMSTask"/> as a <see cref="SMSCondition"/>.
        /// </summary>
        Task = 255,
    }
}
