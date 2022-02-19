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
        /// An identity check for <see cref="MSDatabaseObject"/> <see cref="SMSCondition"/>.
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
        /// An input <see cref="SMSCondition"/>.
        /// </summary>
        Input = 10,

        /// <summary>
        /// An output <see cref="SMSCondition"/>.
        /// </summary>
        Output = 11,

        /// <summary>
        /// A skill update <see cref="SMSCondition"/>.
        /// </summary>
        SkillUpdate = 12,

        /// <summary>
        /// An if statement <see cref="SMSCondition"/>.
        /// </summary>
        IfStatement = 13,

        /// <summary>
        /// An if-else statement <see cref="SMSCondition"/>.
        /// </summary>
        IfElseStatement = 43,

        /// <summary>
        /// A reset <see cref="SMSCondition"/>.
        /// </summary>
        Reset = 15,

        /// <summary>
        /// A <see cref="SMSTask"/> as a <see cref="SMSCondition"/>.
        /// </summary>
        Task = 255,
    }
}
