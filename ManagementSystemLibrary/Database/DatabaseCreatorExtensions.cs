// <copyright file="DatabaseCreatorExtensions.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace ManagementSystemLibrary.Database
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using ManagementSystemLibrary.AMS;
    using ManagementSystemLibrary.ManagementSystem;
    using ManagementSystemLibrary.Pipeline;
    using ManagementSystemLibrary.PMS;
    using ManagementSystemLibrary.RMS;
    using ManagementSystemLibrary.SMS;
    using ManagementSystemLibrary.TMS;
    using Npgsql;
    using NpgsqlTypes;

    /// <summary>
    /// Prepares the database for the management system.
    /// </summary>
    public static class DatabaseCreatorExtensions
    {
        private static readonly MSObjectParameter[] AccessObjectParameters;
        private static readonly MSObjectParameter[] DatabaseObjectParameters;
        private static readonly MSObjectParameter[] DataObjectParameters;
        private static readonly MSObjectParameter[] LinkObjectParameters;
        private static readonly MSObjectParameter[] ScheduleObjectParameters;
        private static readonly MSObjectParameter[] TimeObjectParameters;
        private static readonly MSObjectParameter[] RangeObjectParameters;
        private static readonly MSObjectParameter[] AMSAccountParameters;
        private static readonly MSObjectParameter[] AMSDeviceParameters;
        private static readonly MSObjectParameter[] SMSSkillParameters;
        private static readonly MSObjectParameter[] SMSTaskParameters;
        private static readonly (Type, MSObjectParameter[])[] InitializationTable;

        static DatabaseCreatorExtensions()
        {
            DatabaseObjectParameters = new MSObjectParameter[]
            {
                new () { Level = 0, Name = "creationtime", Type = "bytea", Constrains = "NOT NULL", HasGetFunction = true },
                new () { Level = 0, Name = "creator", Type = "bytea", Constrains = "NOT NULL", HasGetFunction = true, HasVerification = true },
            };
            AccessObjectParameters = DatabaseObjectParameters.Concat(new MSObjectParameter[]
            {
                new () { Level = 1, Name = "access", Type = "bytea", Constrains = "NOT NULL", HasGetFunction = true },
                new () { Level = 1, Name = "name", Type = "bytea", Constrains = "NOT NULL", HasGetFunction = true, HasVerification = true },
                new () { Level = 1, Name = "publickey", Type = "bytea", Constrains = "NOT NULL", HasGetFunction = true },
                new () { Level = 1, Name = "publicsignature", Type = "bytea", Constrains = "NOT NULL", HasGetFunction = true },
            }).ToArray();
            DataObjectParameters = DatabaseObjectParameters.Concat(new MSObjectParameter[]
            {
                new () { Level = 1, Name = "access", Type = "bytea", Constrains = "NOT NULL", HasGetFunction = true },
                new () { Level = 1, Name = "data", Type = "bytea", Constrains = "NOT NULL", HasGetFunction = true, HasVerification = true },
                new () { Level = 1, Name = "modificationtime", Type = "bytea", Constrains = "NOT NULL", HasGetFunction = true, },
                new () { Level = 1, Name = "modifier", Type = "bytea", Constrains = "NOT NULL", HasGetFunction = true },
                new () { Level = 1, Name = "name", Type = "bytea", Constrains = "NOT NULL" },
                new () { Level = 1, Name = "parent", Type = "bytea", Constrains = "NOT NULL" },
            }).ToArray();
            LinkObjectParameters = DatabaseObjectParameters.Concat(new MSObjectParameter[]
            {
                new () { Level = 1, Name = "child", Type = "bytea", Constrains = "NOT NULL" },
                new () { Level = 1, Name = "childaccess", Type = "bytea", Constrains = "NOT NULL" },
                new () { Level = 1, Name = "childhash", Type = "bytea", Constrains = "NOT NULL" },
                new () { Level = 1, Name = "parent", Type = "bytea", Constrains = "NOT NULL", HasGetFunction = true, HasVerification = true },
                new () { Level = 1, Name = "parentaccess", Type = "bytea", Constrains = "NOT NULL" },
                new () { Level = 1, Name = "parenthash", Type = "bytea", Constrains = "NOT NULL" },
                new () { Level = 1, Name = "privateaccess", Type = "bytea", Constrains = "NOT NULL" },
                new () { Level = 1, Name = "type", Type = "bytea", Constrains = "NOT NULL", HasGetFunction = true },
            }).ToArray();
            ScheduleObjectParameters = AccessObjectParameters.Concat(new MSObjectParameter[]
            {
                new () { Level = 2, Name = "parameters", Type = "bytea", Constrains = "NOT NULL", HasGetFunction = true },
            }).ToArray();
            TimeObjectParameters = DataObjectParameters.Concat(new MSObjectParameter[]
            {
                new () { Level = 2, Name = "pa", Type = "double precision", Constrains = "NOT NULL" },
                new () { Level = 2, Name = "pm", Type = "double precision", Constrains = "NOT NULL" },
                new () { Level = 2, Name = "time", Type = "bytea", Constrains = "NOT NULL", HasGetFunction = true, HasVerification = true },
            }).ToArray();
            RangeObjectParameters = TimeObjectParameters.Concat(new MSObjectParameter[]
            {
                new () { Level = 3, Name = "pb", Type = "double precision", Constrains = "NOT NULL" },
                new () { Level = 3, Name = "pn", Type = "double precision", Constrains = "NOT NULL" },
                new () { Level = 3, Name = "endtime", Type = "bytea", Constrains = "NOT NULL", HasGetFunction = true, HasVerification = true },
            }).ToArray();
            AMSAccountParameters = AccessObjectParameters.Concat(new MSObjectParameter[]
            {
                new () { Level = 2, Name = "child", Type = "bytea", Constrains = "NOT NULL" },
                new () { Level = 2, Name = "childhash", Type = "bytea", Constrains = "NOT NULL" },
                new () { Level = 2, Name = "parent", Type = "bytea", Constrains = "NOT NULL" },
                new () { Level = 2, Name = "privateaccess", Type = "bytea", Constrains = "NOT NULL" },
            }).ToArray();
            AMSDeviceParameters = AccessObjectParameters.Concat(new MSObjectParameter[]
            {
                new () { Level = 2, Name = "child", Type = "bytea", Constrains = "NOT NULL", HasVerification = true },
                new () { Level = 2, Name = "childhash", Type = "bytea", Constrains = "NOT NULL" },
                new () { Level = 2, Name = "parent", Type = "bytea", Constrains = "NOT NULL" },
                new () { Level = 2, Name = "privateaccess", Type = "bytea", Constrains = "NOT NULL" },
            }).ToArray();
            SMSSkillParameters = ScheduleObjectParameters.Concat(new MSObjectParameter[]
            {
                new () { Level = 3, Name = "parent", Type = "bytea", Constrains = "NOT NULL", HasGetFunction = true, HasVerification = true },
                new () { Level = 3, Name = "parenthash", Type = "bytea", Constrains = "NOT NULL" },
                new () { Level = 3, Name = "namehash", Type = "bytea", Constrains = "NOT NULL" },
            }).ToArray();
            SMSTaskParameters = AccessObjectParameters.Concat(new MSObjectParameter[]
            {
                new () { Level = 2, Name = "scenario", Type = "bytea", Constrains = "NOT NULL", HasGetFunction = true, HasVerification = true },
                new () { Level = 2, Name = "scenariohash", Type = "bytea", Constrains = "NOT NULL" },
            }).ToArray();
            InitializationTable = new (Type, MSObjectParameter[])[]
            {
                (typeof(AMSAccount), AMSAccountParameters),
                (typeof(AMSAssociate), LinkObjectParameters),
                (typeof(AMSAssociation), AccessObjectParameters),
                (typeof(AMSDevice), AMSDeviceParameters),
                (typeof(RMSRecord), DataObjectParameters),
                (typeof(PMSAffiliate), LinkObjectParameters),
                (typeof(PMSAppointment), RangeObjectParameters),
                (typeof(PMSPlanner), ScheduleObjectParameters),
                (typeof(SMSBond), DataObjectParameters),
                (typeof(SMSCondition), DataObjectParameters),
                (typeof(SMSConstraint), LinkObjectParameters),
                (typeof(SMSContender), LinkObjectParameters),
                (typeof(SMSScenario), AccessObjectParameters),
                (typeof(SMSSkill), SMSSkillParameters),
                (typeof(SMSTask), SMSTaskParameters),
                (typeof(SMSUpdate), TimeObjectParameters),
                (typeof(TMSAttachment), DataObjectParameters),
                (typeof(TMSMessage), TimeObjectParameters),
                (typeof(TMSReadReceipt), DataObjectParameters),
                (typeof(TMSResponse), DataObjectParameters),
                (typeof(TMSRole), LinkObjectParameters),
                (typeof(TMSTalk), ScheduleObjectParameters),
                (typeof(TMSThread), DataObjectParameters),
            };
        }

        /// <summary>
        /// Prepares all the tables and functions used by the <see cref="ManagementSystemLibrary"/>.
        /// </summary>
        /// <param name="pipeline">The <see cref="Pipeline"/> used for access to the database.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public static async Task CreateWholeDatabaseAsync(this Pipeline pipeline)
        {
            foreach ((Type, MSObjectParameter[]) element in InitializationTable)
            {
                await CreateWholeElementAsync(pipeline, element.Item1, element.Item2).ConfigureAwait(false);
            }

            await CreateLinkElementAsync(pipeline, typeof(AMSAssociate)).ConfigureAwait(false);
            await CreateLinkElementAsync(pipeline, typeof(TMSRole)).ConfigureAwait(false);
            await CreateLinkElementAsync(pipeline, typeof(PMSAffiliate)).ConfigureAwait(false);
            await CreateLinkElementAsync(pipeline, typeof(SMSConstraint)).ConfigureAwait(false);
            await CreateLinkElementAsync(pipeline, typeof(SMSContender)).ConfigureAwait(false);
            await CreateSaveTimeFunctionAsync(pipeline, typeof(TMSMessage).GetDatabaseAbbreviation()).ConfigureAwait(false);
            await CreateSaveTimeFunctionAsync(pipeline, typeof(SMSUpdate).GetDatabaseAbbreviation()).ConfigureAwait(false);
            await CreateSaveTimeFunctionAsync(pipeline, typeof(PMSAppointment).GetDatabaseAbbreviation()).ConfigureAwait(false);
            await CreateSaveEndTimeFunctionAsync(pipeline, typeof(PMSAppointment).GetDatabaseAbbreviation()).ConfigureAwait(false);
            await CreateGetLinkObjectChildFunctionsAsync(pipeline, typeof(AMSDevice).GetDatabaseAbbreviation()).ConfigureAwait(false);
            await CreateGetLinkObjectChildFunctionsAsync(pipeline, typeof(AMSAccount).GetDatabaseAbbreviation()).ConfigureAwait(false);
            await CreateGiveDeviceAccessFunctionAsync(pipeline).ConfigureAwait(false);
            await CreateVerifyAMSAccountAssociationFunctionAsync(pipeline).ConfigureAwait(false);
            await CreateLoadItemsFunctionsAsync(pipeline, typeof(AMSDevice).GetDatabaseAbbreviation()).ConfigureAwait(false);
            await CreateLoadItemsFunctionsAsync(pipeline, typeof(PMSAppointment).GetDatabaseAbbreviation()).ConfigureAwait(false);
            await CreateLoadItemsFunctionsAsync(pipeline, typeof(SMSBond).GetDatabaseAbbreviation()).ConfigureAwait(false);
            await CreateLoadItemsFunctionsAsync(pipeline, typeof(SMSCondition).GetDatabaseAbbreviation()).ConfigureAwait(false);
            await CreateLoadItemsFunctionsAsync(pipeline, typeof(TMSMessage).GetDatabaseAbbreviation()).ConfigureAwait(false);
            await CreateLoadItemsFunctionsAsync(pipeline, typeof(SMSUpdate).GetDatabaseAbbreviation()).ConfigureAwait(false);
            await CreateSaveDataFunctionAsync(pipeline, typeof(SMSBond).GetDatabaseAbbreviation()).ConfigureAwait(false);
            await CreateSaveDataFunctionAsync(pipeline, typeof(SMSCondition).GetDatabaseAbbreviation()).ConfigureAwait(false);
            await CreateSharedNameTableAsync(pipeline).ConfigureAwait(false);
            await CreateIDGeneratorFunctionAsync(pipeline, "sharedname").ConfigureAwait(false);
            await CreateDepositeSharedNameFunctionAsync(pipeline).ConfigureAwait(false);
            await CreateSearchSharedNameFunctionsAsync(pipeline).ConfigureAwait(false);
        }

        private static async Task<bool> CreateWholeElementAsync(Pipeline pipeline, Type type, MSObjectParameter[] parameters)
        {
            await CreateTableAsync(pipeline, type.GetDatabaseAbbreviation(), parameters).ConfigureAwait(false);
            await CreateIDGeneratorFunctionAsync(pipeline, type.GetDatabaseAbbreviation()).ConfigureAwait(false);
            await CreateCreateFunctionAsync(pipeline, type.GetDatabaseAbbreviation(), parameters).ConfigureAwait(false);
            await CreateRemoveFunctionAsync(pipeline, type.GetDatabaseAbbreviation(), parameters).ConfigureAwait(false);
            await CreateGetFunctionsAsync(pipeline, type.GetDatabaseAbbreviation(), parameters).ConfigureAwait(false);
            if (type.IsSubclassOf(typeof(MSDataObject<MSDatabaseObject>)))
            {
                await CreateLoadItemsFunctionsAsync(pipeline, type.GetDatabaseAbbreviation()).ConfigureAwait(false);
                await CreateSaveDataFunctionAsync(pipeline, type.GetDatabaseAbbreviation()).ConfigureAwait(false);
            }

            if (type.IsSubclassOf(typeof(MSAccessObject)))
            {
                await CreateSaveNameFunctionAsync(pipeline, type.GetDatabaseAbbreviation()).ConfigureAwait(false);
            }

            return true;
        }

        private static async Task<bool> CreateLinkElementAsync(Pipeline pipeline, Type type)
        {
            await CreateGetLinkObjectAccessFunctionsAsync(pipeline, type.GetDatabaseAbbreviation()).ConfigureAwait(false);
            await CreateGetLinkObjectChildFunctionsAsync(pipeline, type.GetDatabaseAbbreviation()).ConfigureAwait(false);
            await CreateGiveAccessFunctionAsync(pipeline, type.GetDatabaseAbbreviation()).ConfigureAwait(false);
            await CreateLoadChildrenFunctionsAsync(pipeline, type.GetDatabaseAbbreviation()).ConfigureAwait(false);
            await CreateLoadParentsFunctionsAsync(pipeline, type.GetDatabaseAbbreviation()).ConfigureAwait(false);

            return true;
        }

        private static async Task<bool> CreateTableAsync(Pipeline pipeline, string abbreviation, MSObjectParameter[] parameters)
        {
            await new PipelineItem(pipeline)
            {
                BatchCommand = (PipelineItem item, NpgsqlCommand command) =>
                {
                    StringBuilder builder = new StringBuilder().AppendFormat(
                    @"CREATE TABLE IF NOT EXISTS public.{0}
                    (id bigint NOT NULL",
                    abbreviation);
                    foreach (MSObjectParameter parameter in parameters)
                    {
                        builder.AppendFormat(",{0} {1} {2}", parameter.Name, parameter.Type, parameter.Constrains);
                    }

                    foreach (MSObjectParameter parameter in parameters)
                    {
                        if (parameter.HasVerification)
                        {
                            builder.AppendFormat(",{0}verification bytea {1}", parameter.Name, parameter.Constrains);
                        }
                    }

                    command.CommandText += builder.AppendFormat(
                    @",CONSTRAINT {0}_pkey PRIMARY KEY (id));
                    ALTER TABLE IF EXISTS public.{0} OWNER TO ""{1}"";
                    SELECT {2};",
                    abbreviation,
                    pipeline.Parameters.Owner,
                    item.ID).Replace("\t", string.Empty).ToString();
                    return true;
                },
            }.ExecuteAsync().ConfigureAwait(false);
            return true;
        }

        private static async Task<bool> CreateSharedNameTableAsync(Pipeline pipeline)
        {
            await new PipelineItem(pipeline)
            {
                BatchCommand = (PipelineItem item, NpgsqlCommand command) =>
                {
                    command.CommandText += new StringBuilder().AppendFormat(
                    @"CREATE TABLE IF NOT EXISTS public.sharedname
                    (id bigint NOT NULL,
                    hash bytea,
                    name bytea NOT NULL,
                    CONSTRAINT sharedname_pkey PRIMARY KEY (id));
                    ALTER TABLE IF EXISTS public.sharedname OWNER TO ""{0}"";
                    SELECT {1};",
                    pipeline.Parameters.Owner,
                    item.ID).Replace("\t", string.Empty).ToString();
                    return true;
                },
            }.ExecuteAsync().ConfigureAwait(false);
            return true;
        }

        private static async Task<bool> CreateGetLinkObjectAccessFunctionsAsync(Pipeline pipeline, string abbreviation)
        {
            await new PipelineItem(pipeline)
            {
                BatchCommand = (PipelineItem item, NpgsqlCommand command) =>
                {
                    command.CommandText += new StringBuilder().AppendFormat(
                    @"CREATE OR REPLACE FUNCTION public.get{0}access(arg_id bigint) RETURNS table(childaccess bytea, parentaccess bytea) LANGUAGE 'sql' AS
                    $BODY$SELECT childaccess, parentaccess FROM {0} WHERE id = arg_id;$BODY$;
                    ALTER FUNCTION public.get{0}access(bigint) OWNER TO ""{1}"";
                    SELECT {2};",
                    abbreviation,
                    pipeline.Parameters.Owner,
                    item.ID).Replace("\t", string.Empty).ToString();
                    return true;
                },
            }.ExecuteAsync().ConfigureAwait(false);

            return true;
        }

        private static async Task<bool> CreateIDGeneratorFunctionAsync(Pipeline pipeline, string abbreviation)
        {
            await new PipelineItem(pipeline)
            {
                BatchCommand = (PipelineItem item, NpgsqlCommand command) =>
                {
                    command.CommandText += new StringBuilder().AppendFormat(
                    @"CREATE OR REPLACE FUNCTION public.generate{0}id(arg_proposal bigint) RETURNS bigint LANGUAGE 'sql' AS
                    $BODY$SELECT CASE WHEN((SELECT COUNT(*) FROM {0} WHERE id = arg_proposal) > 0)
                    THEN generate{0}id(arg_proposal + 1) else arg_proposal end;$BODY$;
                    ALTER FUNCTION public.generate{0}id(bigint) OWNER TO ""{1}"";
                    SELECT {2};",
                    abbreviation,
                    pipeline.Parameters.Owner,
                    item.ID).Replace("\t", string.Empty).ToString();
                    return true;
                },
            }.ExecuteAsync().ConfigureAwait(false);
            return true;
        }

        private static async Task<bool> CreateCreateFunctionAsync(Pipeline pipeline, string abbreviation, MSObjectParameter[] parameters)
        {
            await new PipelineItem(pipeline)
            {
                BatchCommand = (PipelineItem item, NpgsqlCommand command) =>
                {
                    StringBuilder builder = new StringBuilder().AppendFormat(
                    @"CREATE OR REPLACE FUNCTION public.create{0}(",
                    abbreviation);
                    for (int index = 0; index < 5; index++)
                    {
                        foreach (MSObjectParameter parameter in parameters)
                        {
                            if (parameter.Level == index)
                            {
                                builder.AppendFormat("arg_{0} {1},", parameter.Name, parameter.Type);
                            }
                        }

                        foreach (MSObjectParameter parameter in parameters)
                        {
                            if (parameter.HasVerification
                                && parameter.Level == index)
                            {
                                builder.AppendFormat("arg_{0}verification bytea,", parameter.Name);
                            }
                        }
                    }

                    builder.Remove(builder.Length - 1, 1).AppendFormat(
                    @") RETURNS bigint LANGUAGE 'sql' AS $BODY$INSERT INTO {0}(id", abbreviation);
                    foreach (MSObjectParameter parameter in parameters)
                    {
                        builder.AppendFormat(",{0}", parameter.Name);
                    }

                    foreach (MSObjectParameter parameter in parameters)
                    {
                        if (parameter.HasVerification)
                        {
                            builder.AppendFormat(",{0}verification", parameter.Name);
                        }
                    }

                    builder.AppendFormat(
                    @")VALUES(generate{0}id(floor(random()* 9000000000000 + 1000000000000)::bigint)", abbreviation);
                    foreach (MSObjectParameter parameter in parameters)
                    {
                        builder.AppendFormat(",arg_{0}", parameter.Name);
                    }

                    foreach (MSObjectParameter parameter in parameters)
                    {
                        if (parameter.HasVerification)
                        {
                            builder.AppendFormat(",arg_{0}verification", parameter.Name);
                        }
                    }

                    builder.AppendFormat(
                    @") RETURNING id;$BODY$;ALTER FUNCTION public.create{0}(", abbreviation);
                    for (int index = 0; index < 5; index++)
                    {
                        foreach (MSObjectParameter parameter in parameters)
                        {
                            if (parameter.Level == index)
                            {
                                builder.AppendFormat("{0},", parameter.Type);
                            }
                        }

                        foreach (MSObjectParameter parameter in parameters)
                        {
                            if (parameter.HasVerification
                                && parameter.Level == index)
                            {
                                builder.Append("bytea,");
                            }
                        }
                    }

                    command.CommandText += builder.Remove(builder.Length - 1, 1).AppendFormat(
                    @") OWNER TO ""{0}"";
                    SELECT {1};",
                    pipeline.Parameters.Owner,
                    item.ID).Replace("\t", string.Empty).ToString();
                    return true;
                },
            }.ExecuteAsync().ConfigureAwait(false);
            return true;
        }

        private static async Task<bool> CreateRemoveFunctionAsync(Pipeline pipeline, string abbreviation, MSObjectParameter[] parameters)
        {
            await new PipelineItem(pipeline)
            {
                BatchCommand = (PipelineItem item, NpgsqlCommand command) =>
                {
                    command.CommandText += new StringBuilder().AppendFormat(
                    @"CREATE OR REPLACE FUNCTION public.remove{0}(arg_id bigint) RETURNS boolean LANGUAGE 'sql' AS
                    $BODY$DELETE FROM {0} WHERE id = arg_id;
                    SELECT TRUE;$BODY$;
                    ALTER FUNCTION public.remove{0}(bigint) OWNER TO ""{1}"";
                    SELECT {2};",
                    abbreviation,
                    pipeline.Parameters.Owner,
                    item.ID).Replace("\t", string.Empty).ToString();
                    return true;
                },
            }.ExecuteAsync().ConfigureAwait(false);
            return true;
        }

        private static async Task<bool> CreateDepositeSharedNameFunctionAsync(Pipeline pipeline)
        {
            await new PipelineItem(pipeline)
            {
                BatchCommand = (PipelineItem item, NpgsqlCommand command) =>
                {
                    command.CommandText += new StringBuilder().AppendFormat(
                    @"CREATE OR REPLACE FUNCTION public.depositesharedname
                    (arg_hash bytea, arg_name bytea)
                    RETURNS boolean LANGUAGE 'sql' AS
                    $BODY$INSERT INTO sharedname
                    (id, hash, name)
                    VALUES(generatesharednameid(floor(random()* 9000000000000 + 1000000000000)::bigint),arg_hash,arg_name);
                    SELECT TRUE;$BODY$;
                    ALTER FUNCTION public.depositesharedname(bytea,bytea) OWNER TO ""{0}"";
                    SELECT {1};",
                    pipeline.Parameters.Owner,
                    item.ID).Replace("\t", string.Empty).ToString();
                    return true;
                },
            }.ExecuteAsync().ConfigureAwait(false);

            return true;
        }

        private static async Task<bool> CreateSearchSharedNameFunctionsAsync(Pipeline pipeline)
        {
            await new PipelineItem(pipeline)
            {
                BatchCommand = (PipelineItem item, NpgsqlCommand command) =>
                {
                    command.CommandText += new StringBuilder().AppendFormat(
                    @"CREATE OR REPLACE FUNCTION public.searchsharedname(arg_publichash bytea,arg_hash bytea) RETURNS bytea LANGUAGE 'sql' AS
                    $BODY$UPDATE sharedname SET hash = arg_hash WHERE hash = arg_publichash;
                    SELECT name FROM sharedname WHERE hash = arg_hash;$BODY$;
                    ALTER FUNCTION public.searchsharedname(bytea,bytea) OWNER TO ""{0}"";
                    SELECT {1};",
                    pipeline.Parameters.Owner,
                    item.ID).Replace("\t", string.Empty).ToString();
                    return true;
                },
            }.ExecuteAsync().ConfigureAwait(false);

            return true;
        }

        private static async Task<bool> CreateGetLinkObjectChildFunctionsAsync(Pipeline pipeline, string abbreviation)
        {
            await new PipelineItem(pipeline)
            {
                BatchCommand = (PipelineItem item, NpgsqlCommand command) =>
                {
                    command.CommandText += new StringBuilder().AppendFormat(
                    @"CREATE OR REPLACE FUNCTION public.get{0}child(arg_id bigint) RETURNS table(child bytea, childhash bytea, privateaccess bytea) LANGUAGE 'sql' AS
                    $BODY$SELECT child, childhash, privateaccess FROM {0} WHERE id = arg_id;$BODY$;
                    ALTER FUNCTION public.get{0}child(bigint) OWNER TO ""{1}"";
                    SELECT {2};",
                    abbreviation,
                    pipeline.Parameters.Owner,
                    item.ID).Replace("\t", string.Empty).ToString();
                    return true;
                },
            }.ExecuteAsync().ConfigureAwait(false);

            return true;
        }

        private static async Task<bool> CreateGetFunctionsAsync(Pipeline pipeline, string abbreviation, MSObjectParameter[] parameters)
        {
            foreach (MSObjectParameter parameter in parameters)
            {
                if (parameter.HasGetFunction)
                {
                    await new PipelineItem(pipeline)
                    {
                        BatchCommand = (PipelineItem item, NpgsqlCommand command) =>
                        {
                            command.CommandText += new StringBuilder().AppendFormat(
                            @"CREATE OR REPLACE FUNCTION public.get{0}{1}(arg_id bigint) RETURNS {2} LANGUAGE 'sql' AS
                            $BODY$SELECT {1} FROM {0} WHERE id = arg_id;$BODY$;
                            ALTER FUNCTION public.get{0}{1}(bigint) OWNER TO ""{3}"";
                            SELECT {4};",
                            abbreviation,
                            parameter.Name,
                            parameter.Type,
                            pipeline.Parameters.Owner,
                            item.ID).Replace("\t", string.Empty).ToString();
                            return true;
                        },
                    }.ExecuteAsync().ConfigureAwait(false);
                }

                if (parameter.HasVerification)
                {
                    await new PipelineItem(pipeline)
                    {
                        BatchCommand = (PipelineItem item, NpgsqlCommand command) =>
                        {
                            command.CommandText += new StringBuilder().AppendFormat(
                            @"CREATE OR REPLACE FUNCTION public.verify{0}{1}(arg_id bigint) RETURNS bytea LANGUAGE 'sql' AS
                            $BODY$SELECT {1}verification FROM {0} WHERE id = arg_id;$BODY$;
                            ALTER FUNCTION public.verify{0}{1}(bigint) OWNER TO ""{2}"";
                            SELECT {3};",
                            abbreviation,
                            parameter.Name,
                            pipeline.Parameters.Owner,
                            item.ID).Replace("\t", string.Empty).ToString();
                            return true;
                        },
                    }.ExecuteAsync().ConfigureAwait(false);
                }
            }

            return true;
        }

        private static async Task<bool> CreateLoadChildrenFunctionsAsync(Pipeline pipeline, string abbreviation)
        {
            await new PipelineItem(pipeline)
            {
                BatchCommand = (PipelineItem item, NpgsqlCommand command) =>
                {
                    command.CommandText += new StringBuilder().AppendFormat(
                    @"CREATE OR REPLACE FUNCTION public.load{0}children(arg_parenthash bytea) RETURNS SETOF bigint LANGUAGE 'sql' AS
                    $BODY$SELECT search.* FROM (SELECT id FROM {0} WHERE parenthash = arg_parenthash) AS search FULL JOIN(SELECT 0) as cnt ON TRUE;$BODY$;
                    ALTER FUNCTION public.load{0}children(bytea) OWNER TO ""{1}"";
                    SELECT {2};",
                    abbreviation,
                    pipeline.Parameters.Owner,
                    item.ID).Replace("\t", string.Empty).ToString();
                    return true;
                },
            }.ExecuteAsync().ConfigureAwait(false);

            return true;
        }

        private static async Task<bool> CreateLoadItemsFunctionsAsync(Pipeline pipeline, string abbreviation)
        {
            await new PipelineItem(pipeline)
            {
                BatchCommand = (PipelineItem item, NpgsqlCommand command) =>
                {
                    command.CommandText += new StringBuilder().AppendFormat(
                    @"CREATE OR REPLACE FUNCTION public.load{0}items(arg_parent bytea) RETURNS SETOF bigint LANGUAGE 'sql' AS
                    $BODY$SELECT search.* FROM (SELECT id FROM {0} WHERE parent = arg_parent) AS search FULL JOIN(SELECT 0) as cnt ON TRUE;$BODY$;
                    ALTER FUNCTION public.load{0}items(bytea) OWNER TO ""{1}"";
                    SELECT {2};",
                    abbreviation,
                    pipeline.Parameters.Owner,
                    item.ID).Replace("\t", string.Empty).ToString();
                    return true;
                },
            }.ExecuteAsync().ConfigureAwait(false);

            return true;
        }

        private static async Task<bool> CreateLoadParentsFunctionsAsync(Pipeline pipeline, string abbreviation)
        {
            await new PipelineItem(pipeline)
            {
                BatchCommand = (PipelineItem item, NpgsqlCommand command) =>
                {
                    command.CommandText += new StringBuilder().AppendFormat(
                    @"CREATE OR REPLACE FUNCTION public.load{0}parents(arg_childhash bytea,arg_childpublichash bytea) RETURNS SETOF bigint LANGUAGE 'sql' AS
                    $BODY$SELECT search.* FROM (SELECT id FROM {0} WHERE childhash = arg_childhash OR childhash = arg_childpublichash) AS search FULL JOIN(SELECT 0) as cnt ON TRUE;$BODY$;
                    ALTER FUNCTION public.load{0}parents(bytea,bytea) OWNER TO ""{1}"";
                    SELECT {2};",
                    abbreviation,
                    pipeline.Parameters.Owner,
                    item.ID).Replace("\t", string.Empty).ToString();
                    return true;
                },
            }.ExecuteAsync().ConfigureAwait(false);

            return true;
        }

        private static async Task<bool> CreateGiveAccessFunctionAsync(Pipeline pipeline, string abbreviation)
        {
            await new PipelineItem(pipeline)
            {
                BatchCommand = (PipelineItem item, NpgsqlCommand command) =>
                {
                    command.CommandText += new StringBuilder().AppendFormat(
                    @"CREATE OR REPLACE FUNCTION public.giveaccess{0}(arg_id bigint, arg_childhash bytea, arg_privateaccess bytea, arg_parentverification bytea) RETURNS boolean LANGUAGE 'sql' AS
                    $BODY$UPDATE {0} SET
                    childhash = arg_childhash,
                    privateaccess = arg_privateaccess,
                    parentverification = arg_parentverification
                    WHERE id = arg_id;
                    SELECT TRUE;$BODY$;
                    ALTER FUNCTION public.giveaccess{0}(bigint,bytea,bytea,bytea) OWNER TO ""{1}"";
                    SELECT {2};",
                    abbreviation,
                    pipeline.Parameters.Owner,
                    item.ID).Replace("\t", string.Empty).ToString();
                    return true;
                },
            }.ExecuteAsync().ConfigureAwait(false);

            return true;
        }

        private static async Task<bool> CreateGiveDeviceAccessFunctionAsync(Pipeline pipeline)
        {
            await new PipelineItem(pipeline)
            {
                BatchCommand = (PipelineItem item, NpgsqlCommand command) =>
                {
                    command.CommandText += new StringBuilder().AppendFormat(
                    @"CREATE OR REPLACE FUNCTION public.giveaccessamsdevice(arg_id bigint, arg_childhash bytea, arg_privateaccess bytea, arg_childverification bytea) RETURNS boolean LANGUAGE 'sql' AS
                    $BODY$UPDATE amsdevice SET
                    childhash = arg_childhash,
                    privateaccess = arg_privateaccess,
                    childverification = arg_childverification
                    WHERE id = arg_id;
                    SELECT TRUE;$BODY$;
                    ALTER FUNCTION public.giveaccessamsdevice(bigint,bytea,bytea,bytea) OWNER TO ""{0}"";
                    SELECT {1};",
                    pipeline.Parameters.Owner,
                    item.ID).Replace("\t", string.Empty).ToString();
                    return true;
                },
            }.ExecuteAsync().ConfigureAwait(false);

            return true;
        }

        private static async Task<bool> CreateVerifyAMSAccountAssociationFunctionAsync(Pipeline pipeline)
        {
            await new PipelineItem(pipeline)
            {
                BatchCommand = (PipelineItem item, NpgsqlCommand command) =>
                {
                    command.CommandText += new StringBuilder().AppendFormat(
                    @"CREATE OR REPLACE FUNCTION public.verifyamsaccountassociation(arg_id bigint, arg_creator bytea, arg_creatorverification bytea) RETURNS boolean LANGUAGE 'sql' AS
                    $BODY$UPDATE amsassociation SET
                    creator = arg_creator,
                    creatorverification = arg_creatorverification
                    WHERE id = arg_id;
                    SELECT TRUE;$BODY$;
                    ALTER FUNCTION public.verifyamsaccountassociation(bigint,bytea,bytea) OWNER TO ""{0}"";
                    SELECT {1};",
                    pipeline.Parameters.Owner,
                    item.ID).Replace("\t", string.Empty).ToString();
                    return true;
                },
            }.ExecuteAsync().ConfigureAwait(false);

            return true;
        }

        private static async Task<bool> CreateSaveNameFunctionAsync(Pipeline pipeline, string abbreviation)
        {
            await new PipelineItem(pipeline)
            {
                BatchCommand = (PipelineItem item, NpgsqlCommand command) =>
                {
                    command.CommandText += new StringBuilder().AppendFormat(
                    @"CREATE OR REPLACE FUNCTION public.save{0}name(arg_id bigint, arg_name bytea, arg_nameverification bytea) RETURNS boolean LANGUAGE 'sql' AS
                    $BODY$UPDATE {0} SET
                    name = arg_name,
                    nameverification = arg_nameverification
                    WHERE id = arg_id;
                    SELECT TRUE;$BODY$;
                    ALTER FUNCTION public.save{0}name(bigint,bytea,bytea) OWNER TO ""{1}"";
                    SELECT {2};",
                    abbreviation,
                    pipeline.Parameters.Owner,
                    item.ID).Replace("\t", string.Empty).ToString();
                    return true;
                },
            }.ExecuteAsync().ConfigureAwait(false);

            return true;
        }

        private static async Task<bool> CreateSaveDataFunctionAsync(Pipeline pipeline, string abbreviation)
        {
            await new PipelineItem(pipeline)
            {
                BatchCommand = (PipelineItem item, NpgsqlCommand command) =>
                {
                    command.CommandText += new StringBuilder().AppendFormat(
                    @"CREATE OR REPLACE FUNCTION public.save{0}data(arg_id bigint, arg_data bytea, arg_modificationtime bytea, arg_modifier bytea, arg_dataverification bytea) RETURNS boolean LANGUAGE 'sql' AS
                    $BODY$UPDATE {0} SET
                    data = arg_data,
                    modificationtime = arg_modificationtime,
                    modifier = arg_modifier,
                    dataverification = arg_dataverification
                    WHERE id = arg_id;
                    SELECT TRUE;$BODY$;
                    ALTER FUNCTION public.save{0}data(bigint,bytea,bytea,bytea,bytea) OWNER TO ""{1}"";
                    SELECT {2};",
                    abbreviation,
                    pipeline.Parameters.Owner,
                    item.ID).Replace("\t", string.Empty).ToString();
                    return true;
                },
            }.ExecuteAsync().ConfigureAwait(false);

            return true;
        }

        private static async Task<bool> CreateSaveTimeFunctionAsync(Pipeline pipeline, string abbreviation)
        {
            await new PipelineItem(pipeline)
            {
                BatchCommand = (PipelineItem item, NpgsqlCommand command) =>
                {
                    command.CommandText += new StringBuilder().AppendFormat(
                    @"CREATE OR REPLACE FUNCTION public.save{0}time(arg_id bigint, arg_pa double precision, arg_pm double precision, arg_time bytea, arg_timeverification bytea) RETURNS boolean LANGUAGE 'sql' AS
                    $BODY$UPDATE {0} SET
                    pa = arg_pa,
                    pm = arg_pm,
                    time = arg_time,
                    timeverification = arg_timeverification
                    WHERE id = arg_id;
                    SELECT TRUE;$BODY$;
                    ALTER FUNCTION public.save{0}time(bigint,double precision,double precision,bytea,bytea) OWNER TO ""{1}"";
                    SELECT {2};",
                    abbreviation,
                    pipeline.Parameters.Owner,
                    item.ID).Replace("\t", string.Empty).ToString();
                    return true;
                },
            }.ExecuteAsync().ConfigureAwait(false);

            return true;
        }

        private static async Task<bool> CreateSaveEndTimeFunctionAsync(Pipeline pipeline, string abbreviation)
        {
            await new PipelineItem(pipeline)
            {
                BatchCommand = (PipelineItem item, NpgsqlCommand command) =>
                {
                    command.CommandText += new StringBuilder().AppendFormat(
                    @"CREATE OR REPLACE FUNCTION public.save{0}endtime(arg_id bigint, arg_pb double precision, arg_pn double precision, arg_endtime bytea, arg_endtimeverification bytea) RETURNS boolean LANGUAGE 'sql' AS
                    $BODY$UPDATE {0} SET
                    pb = arg_pb,
                    pn = arg_pn,
                    endtime = arg_endtime,
                    endtimeverification = arg_endtimeverification
                    WHERE id = arg_id;
                    SELECT TRUE;$BODY$;
                    ALTER FUNCTION public.save{0}endtime(bigint,double precision,double precision,bytea,bytea) OWNER TO ""{1}"";
                    SELECT {2};",
                    abbreviation,
                    pipeline.Parameters.Owner,
                    item.ID).Replace("\t", string.Empty).ToString();
                    return true;
                },
            }.ExecuteAsync().ConfigureAwait(false);

            return true;
        }
    }
}
