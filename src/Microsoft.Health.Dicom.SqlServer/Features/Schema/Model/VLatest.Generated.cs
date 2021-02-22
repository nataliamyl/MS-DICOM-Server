//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------
namespace Microsoft.Health.Dicom.SqlServer.Features.Schema.Model
{
    using Microsoft.Health.SqlServer.Features.Client;
    using Microsoft.Health.SqlServer.Features.Schema.Model;

    internal class VLatest
    {
        internal readonly static ChangeFeedTable ChangeFeed = new ChangeFeedTable();
        internal readonly static CustomTagTable CustomTag = new CustomTagTable();
        internal readonly static CustomTagBigIntTable CustomTagBigInt = new CustomTagBigIntTable();
        internal readonly static CustomTagDateTimeTable CustomTagDateTime = new CustomTagDateTimeTable();
        internal readonly static CustomTagDoubleTable CustomTagDouble = new CustomTagDoubleTable();
        internal readonly static CustomTagPersonNameTable CustomTagPersonName = new CustomTagPersonNameTable();
        internal readonly static CustomTagStringTable CustomTagString = new CustomTagStringTable();
        internal readonly static DeletedInstanceTable DeletedInstance = new DeletedInstanceTable();
        internal readonly static InstanceTable Instance = new InstanceTable();
        internal readonly static SeriesTable Series = new SeriesTable();
        internal readonly static StudyTable Study = new StudyTable();
        internal readonly static AddCustomTagsProcedure AddCustomTags = new AddCustomTagsProcedure();
        internal readonly static AddInstanceProcedure AddInstance = new AddInstanceProcedure();
        internal readonly static DeleteCustomTagProcedure DeleteCustomTag = new DeleteCustomTagProcedure();
        internal readonly static DeleteDeletedInstanceProcedure DeleteDeletedInstance = new DeleteDeletedInstanceProcedure();
        internal readonly static DeleteInstanceProcedure DeleteInstance = new DeleteInstanceProcedure();
        internal readonly static GetChangeFeedProcedure GetChangeFeed = new GetChangeFeedProcedure();
        internal readonly static GetChangeFeedLatestProcedure GetChangeFeedLatest = new GetChangeFeedLatestProcedure();
        internal readonly static GetCustomTagProcedure GetCustomTag = new GetCustomTagProcedure();
        internal readonly static GetInstanceProcedure GetInstance = new GetInstanceProcedure();
        internal readonly static IncrementDeletedInstanceRetryProcedure IncrementDeletedInstanceRetry = new IncrementDeletedInstanceRetryProcedure();
        internal readonly static RetrieveDeletedInstanceProcedure RetrieveDeletedInstance = new RetrieveDeletedInstanceProcedure();
        internal readonly static UpdateInstanceStatusProcedure UpdateInstanceStatus = new UpdateInstanceStatusProcedure();

        internal class ChangeFeedTable : Table
        {
            internal ChangeFeedTable() : base("dbo.ChangeFeed")
            {
            }

            internal readonly BigIntColumn Sequence = new BigIntColumn("Sequence");
            internal readonly DateTimeOffsetColumn Timestamp = new DateTimeOffsetColumn("Timestamp", 7);
            internal readonly TinyIntColumn Action = new TinyIntColumn("Action");
            internal readonly VarCharColumn StudyInstanceUid = new VarCharColumn("StudyInstanceUid", 64);
            internal readonly VarCharColumn SeriesInstanceUid = new VarCharColumn("SeriesInstanceUid", 64);
            internal readonly VarCharColumn SopInstanceUid = new VarCharColumn("SopInstanceUid", 64);
            internal readonly BigIntColumn OriginalWatermark = new BigIntColumn("OriginalWatermark");
            internal readonly NullableBigIntColumn CurrentWatermark = new NullableBigIntColumn("CurrentWatermark");
            internal readonly Index IXC_ChangeFeed = new Index("IXC_ChangeFeed");
            internal readonly Index IX_ChangeFeed_StudyInstanceUid_SeriesInstanceUid_SopInstanceUid = new Index("IX_ChangeFeed_StudyInstanceUid_SeriesInstanceUid_SopInstanceUid");
        }

        internal class CustomTagTable : Table
        {
            internal CustomTagTable() : base("dbo.CustomTag")
            {
            }

            internal readonly BigIntColumn TagKey = new BigIntColumn("TagKey");
            internal readonly VarCharColumn TagPath = new VarCharColumn("TagPath", 64);
            internal readonly VarCharColumn TagVR = new VarCharColumn("TagVR", 2);
            internal readonly TinyIntColumn TagLevel = new TinyIntColumn("TagLevel");
            internal readonly TinyIntColumn TagStatus = new TinyIntColumn("TagStatus");
            internal readonly Index IXC_CustomTag = new Index("IXC_CustomTag");
            internal readonly Index IX_CustomTag_TagPath = new Index("IX_CustomTag_TagPath");
        }

        internal class CustomTagBigIntTable : Table
        {
            internal CustomTagBigIntTable() : base("dbo.CustomTagBigInt")
            {
            }

            internal readonly BigIntColumn TagKey = new BigIntColumn("TagKey");
            internal readonly BigIntColumn TagValue = new BigIntColumn("TagValue");
            internal readonly BigIntColumn StudyKey = new BigIntColumn("StudyKey");
            internal readonly NullableBigIntColumn SeriesKey = new NullableBigIntColumn("SeriesKey");
            internal readonly NullableBigIntColumn InstanceKey = new NullableBigIntColumn("InstanceKey");
            internal readonly BigIntColumn Watermark = new BigIntColumn("Watermark");
            internal readonly Index IXC_CustomTagBigInt = new Index("IXC_CustomTagBigInt");
        }

        internal class CustomTagDateTimeTable : Table
        {
            internal CustomTagDateTimeTable() : base("dbo.CustomTagDateTime")
            {
            }

            internal readonly BigIntColumn TagKey = new BigIntColumn("TagKey");
            internal readonly DateTime2Column TagValue = new DateTime2Column("TagValue", 7);
            internal readonly BigIntColumn StudyKey = new BigIntColumn("StudyKey");
            internal readonly NullableBigIntColumn SeriesKey = new NullableBigIntColumn("SeriesKey");
            internal readonly NullableBigIntColumn InstanceKey = new NullableBigIntColumn("InstanceKey");
            internal readonly BigIntColumn Watermark = new BigIntColumn("Watermark");
            internal readonly Index IXC_CustomTagDateTime = new Index("IXC_CustomTagDateTime");
        }

        internal class CustomTagDoubleTable : Table
        {
            internal CustomTagDoubleTable() : base("dbo.CustomTagDouble")
            {
            }

            internal readonly BigIntColumn TagKey = new BigIntColumn("TagKey");
            internal readonly FloatColumn TagValue = new FloatColumn("TagValue", 53);
            internal readonly BigIntColumn StudyKey = new BigIntColumn("StudyKey");
            internal readonly NullableBigIntColumn SeriesKey = new NullableBigIntColumn("SeriesKey");
            internal readonly NullableBigIntColumn InstanceKey = new NullableBigIntColumn("InstanceKey");
            internal readonly BigIntColumn Watermark = new BigIntColumn("Watermark");
            internal readonly Index IXC_CustomTagDouble = new Index("IXC_CustomTagDouble");
        }

        internal class CustomTagPersonNameTable : Table
        {
            internal CustomTagPersonNameTable() : base("dbo.CustomTagPersonName")
            {
            }

            internal readonly BigIntColumn TagKey = new BigIntColumn("TagKey");
            internal readonly NVarCharColumn TagValue = new NVarCharColumn("TagValue", 200, "SQL_Latin1_General_CP1_CI_AI");
            internal readonly BigIntColumn StudyKey = new BigIntColumn("StudyKey");
            internal readonly NullableBigIntColumn SeriesKey = new NullableBigIntColumn("SeriesKey");
            internal readonly NullableBigIntColumn InstanceKey = new NullableBigIntColumn("InstanceKey");
            internal readonly BigIntColumn Watermark = new BigIntColumn("Watermark");
            internal const string WatermarkAndTagKey = "WatermarkAndTagKey";
            internal const string TagValueWords = "TagValueWords";
            internal readonly Index IXC_CustomTagPersonName = new Index("IXC_CustomTagPersonName");
            internal readonly Index IXC_CustomTagPersonName_WatermarkAndTagKey = new Index("IXC_CustomTagPersonName_WatermarkAndTagKey");
        }

        internal class CustomTagStringTable : Table
        {
            internal CustomTagStringTable() : base("dbo.CustomTagString")
            {
            }

            internal readonly BigIntColumn TagKey = new BigIntColumn("TagKey");
            internal readonly NVarCharColumn TagValue = new NVarCharColumn("TagValue", 64);
            internal readonly BigIntColumn StudyKey = new BigIntColumn("StudyKey");
            internal readonly NullableBigIntColumn SeriesKey = new NullableBigIntColumn("SeriesKey");
            internal readonly NullableBigIntColumn InstanceKey = new NullableBigIntColumn("InstanceKey");
            internal readonly BigIntColumn Watermark = new BigIntColumn("Watermark");
            internal readonly Index IXC_CustomTagString = new Index("IXC_CustomTagString");
        }

        internal class DeletedInstanceTable : Table
        {
            internal DeletedInstanceTable() : base("dbo.DeletedInstance")
            {
            }

            internal readonly VarCharColumn StudyInstanceUid = new VarCharColumn("StudyInstanceUid", 64);
            internal readonly VarCharColumn SeriesInstanceUid = new VarCharColumn("SeriesInstanceUid", 64);
            internal readonly VarCharColumn SopInstanceUid = new VarCharColumn("SopInstanceUid", 64);
            internal readonly BigIntColumn Watermark = new BigIntColumn("Watermark");
            internal readonly DateTimeOffsetColumn DeletedDateTime = new DateTimeOffsetColumn("DeletedDateTime", 0);
            internal readonly IntColumn RetryCount = new IntColumn("RetryCount");
            internal readonly DateTimeOffsetColumn CleanupAfter = new DateTimeOffsetColumn("CleanupAfter", 0);
            internal readonly Index IXC_DeletedInstance = new Index("IXC_DeletedInstance");
            internal readonly Index IX_DeletedInstance_RetryCount_CleanupAfter = new Index("IX_DeletedInstance_RetryCount_CleanupAfter");
        }

        internal class InstanceTable : Table
        {
            internal InstanceTable() : base("dbo.Instance")
            {
            }

            internal readonly BigIntColumn InstanceKey = new BigIntColumn("InstanceKey");
            internal readonly BigIntColumn SeriesKey = new BigIntColumn("SeriesKey");
            internal readonly BigIntColumn StudyKey = new BigIntColumn("StudyKey");
            internal readonly VarCharColumn StudyInstanceUid = new VarCharColumn("StudyInstanceUid", 64);
            internal readonly VarCharColumn SeriesInstanceUid = new VarCharColumn("SeriesInstanceUid", 64);
            internal readonly VarCharColumn SopInstanceUid = new VarCharColumn("SopInstanceUid", 64);
            internal readonly BigIntColumn Watermark = new BigIntColumn("Watermark");
            internal readonly TinyIntColumn Status = new TinyIntColumn("Status");
            internal readonly DateTime2Column LastStatusUpdatedDate = new DateTime2Column("LastStatusUpdatedDate", 7);
            internal readonly DateTime2Column CreatedDate = new DateTime2Column("CreatedDate", 7);
            internal readonly Index IXC_Instance = new Index("IXC_Instance");
            internal readonly Index IX_Instance_StudyInstanceUid_SeriesInstanceUid_SopInstanceUid = new Index("IX_Instance_StudyInstanceUid_SeriesInstanceUid_SopInstanceUid");
            internal readonly Index IX_Instance_StudyInstanceUid_Status = new Index("IX_Instance_StudyInstanceUid_Status");
            internal readonly Index IX_Instance_StudyInstanceUid_SeriesInstanceUid_Status = new Index("IX_Instance_StudyInstanceUid_SeriesInstanceUid_Status");
            internal readonly Index IX_Instance_SopInstanceUid_Status = new Index("IX_Instance_SopInstanceUid_Status");
            internal readonly Index IX_Instance_Watermark = new Index("IX_Instance_Watermark");
            internal readonly Index IX_Instance_SeriesKey_Status = new Index("IX_Instance_SeriesKey_Status");
            internal readonly Index IX_Instance_StudyKey_Status = new Index("IX_Instance_StudyKey_Status");
        }

        internal class SeriesTable : Table
        {
            internal SeriesTable() : base("dbo.Series")
            {
            }

            internal readonly BigIntColumn SeriesKey = new BigIntColumn("SeriesKey");
            internal readonly BigIntColumn StudyKey = new BigIntColumn("StudyKey");
            internal readonly VarCharColumn SeriesInstanceUid = new VarCharColumn("SeriesInstanceUid", 64);
            internal readonly NullableNVarCharColumn Modality = new NullableNVarCharColumn("Modality", 16);
            internal readonly NullableDateColumn PerformedProcedureStepStartDate = new NullableDateColumn("PerformedProcedureStepStartDate");
            internal readonly Index IXC_Series = new Index("IXC_Series");
            internal readonly Index IX_Series_SeriesKey = new Index("IX_Series_SeriesKey");
            internal readonly Index IX_Series_SeriesInstanceUid = new Index("IX_Series_SeriesInstanceUid");
            internal readonly Index IX_Series_Modality = new Index("IX_Series_Modality");
            internal readonly Index IX_Series_PerformedProcedureStepStartDate = new Index("IX_Series_PerformedProcedureStepStartDate");
        }

        internal class StudyTable : Table
        {
            internal StudyTable() : base("dbo.Study")
            {
            }

            internal readonly BigIntColumn StudyKey = new BigIntColumn("StudyKey");
            internal readonly VarCharColumn StudyInstanceUid = new VarCharColumn("StudyInstanceUid", 64);
            internal readonly NVarCharColumn PatientId = new NVarCharColumn("PatientId", 64);
            internal readonly NullableNVarCharColumn PatientName = new NullableNVarCharColumn("PatientName", 200, "SQL_Latin1_General_CP1_CI_AI");
            internal readonly NullableNVarCharColumn ReferringPhysicianName = new NullableNVarCharColumn("ReferringPhysicianName", 200, "SQL_Latin1_General_CP1_CI_AI");
            internal readonly NullableDateColumn StudyDate = new NullableDateColumn("StudyDate");
            internal readonly NullableNVarCharColumn StudyDescription = new NullableNVarCharColumn("StudyDescription", 64);
            internal readonly NullableNVarCharColumn AccessionNumber = new NullableNVarCharColumn("AccessionNumber", 16);
            internal const string PatientNameWords = "PatientNameWords";
            internal readonly Index IXC_Study = new Index("IXC_Study");
            internal readonly Index IX_Study_StudyInstanceUid = new Index("IX_Study_StudyInstanceUid");
            internal readonly Index IX_Study_PatientId = new Index("IX_Study_PatientId");
            internal readonly Index IX_Study_PatientName = new Index("IX_Study_PatientName");
            internal readonly Index IX_Study_ReferringPhysicianName = new Index("IX_Study_ReferringPhysicianName");
            internal readonly Index IX_Study_StudyDate = new Index("IX_Study_StudyDate");
            internal readonly Index IX_Study_StudyDescription = new Index("IX_Study_StudyDescription");
            internal readonly Index IX_Study_AccessionNumber = new Index("IX_Study_AccessionNumber");
        }

        internal class AddCustomTagsProcedure : StoredProcedure
        {
            internal AddCustomTagsProcedure() : base("dbo.AddCustomTags")
            {
            }

            private readonly AddCustomTagsInputTableTypeV1TableValuedParameterDefinition _customTags = new AddCustomTagsInputTableTypeV1TableValuedParameterDefinition("@customTags");

            public void PopulateCommand(SqlCommandWrapper command, global::System.Collections.Generic.IEnumerable<AddCustomTagsInputTableTypeV1Row> customTags)
            {
                command.CommandType = global::System.Data.CommandType.StoredProcedure;
                command.CommandText = "dbo.AddCustomTags";
                _customTags.AddParameter(command.Parameters, customTags);
            }

            public void PopulateCommand(SqlCommandWrapper command, AddCustomTagsTableValuedParameters tableValuedParameters)
            {
                PopulateCommand(command, customTags: tableValuedParameters.CustomTags);
            }
        }

        internal class AddCustomTagsTvpGenerator<TInput> : IStoredProcedureTableValuedParametersGenerator<TInput, AddCustomTagsTableValuedParameters>
        {
            public AddCustomTagsTvpGenerator(ITableValuedParameterRowGenerator<TInput, AddCustomTagsInputTableTypeV1Row> AddCustomTagsInputTableTypeV1RowGenerator)
            {
                this.AddCustomTagsInputTableTypeV1RowGenerator = AddCustomTagsInputTableTypeV1RowGenerator;
            }

            private readonly ITableValuedParameterRowGenerator<TInput, AddCustomTagsInputTableTypeV1Row> AddCustomTagsInputTableTypeV1RowGenerator;

            public AddCustomTagsTableValuedParameters Generate(TInput input)
            {
                return new AddCustomTagsTableValuedParameters(AddCustomTagsInputTableTypeV1RowGenerator.GenerateRows(input));
            }
        }

        internal struct AddCustomTagsTableValuedParameters
        {
            internal AddCustomTagsTableValuedParameters(global::System.Collections.Generic.IEnumerable<AddCustomTagsInputTableTypeV1Row> CustomTags)
            {
                this.CustomTags = CustomTags;
            }

            internal global::System.Collections.Generic.IEnumerable<AddCustomTagsInputTableTypeV1Row> CustomTags { get; }
        }

        internal class AddInstanceProcedure : StoredProcedure
        {
            internal AddInstanceProcedure() : base("dbo.AddInstance")
            {
            }

            private readonly ParameterDefinition<System.String> _studyInstanceUid = new ParameterDefinition<System.String>("@studyInstanceUid", global::System.Data.SqlDbType.VarChar, false, 64);
            private readonly ParameterDefinition<System.String> _seriesInstanceUid = new ParameterDefinition<System.String>("@seriesInstanceUid", global::System.Data.SqlDbType.VarChar, false, 64);
            private readonly ParameterDefinition<System.String> _sopInstanceUid = new ParameterDefinition<System.String>("@sopInstanceUid", global::System.Data.SqlDbType.VarChar, false, 64);
            private readonly ParameterDefinition<System.String> _patientId = new ParameterDefinition<System.String>("@patientId", global::System.Data.SqlDbType.NVarChar, false, 64);
            private readonly ParameterDefinition<System.String> _patientName = new ParameterDefinition<System.String>("@patientName", global::System.Data.SqlDbType.NVarChar, true, 325);
            private readonly ParameterDefinition<System.String> _referringPhysicianName = new ParameterDefinition<System.String>("@referringPhysicianName", global::System.Data.SqlDbType.NVarChar, true, 325);
            private readonly ParameterDefinition<System.Nullable<System.DateTime>> _studyDate = new ParameterDefinition<System.Nullable<System.DateTime>>("@studyDate", global::System.Data.SqlDbType.Date, true);
            private readonly ParameterDefinition<System.String> _studyDescription = new ParameterDefinition<System.String>("@studyDescription", global::System.Data.SqlDbType.NVarChar, true, 64);
            private readonly ParameterDefinition<System.String> _accessionNumber = new ParameterDefinition<System.String>("@accessionNumber", global::System.Data.SqlDbType.NVarChar, true, 64);
            private readonly ParameterDefinition<System.String> _modality = new ParameterDefinition<System.String>("@modality", global::System.Data.SqlDbType.NVarChar, true, 16);
            private readonly ParameterDefinition<System.Nullable<System.DateTime>> _performedProcedureStepStartDate = new ParameterDefinition<System.Nullable<System.DateTime>>("@performedProcedureStepStartDate", global::System.Data.SqlDbType.Date, true);
            private readonly ParameterDefinition<System.Byte> _initialStatus = new ParameterDefinition<System.Byte>("@initialStatus", global::System.Data.SqlDbType.TinyInt, false);

            public void PopulateCommand(SqlCommandWrapper command, System.String studyInstanceUid, System.String seriesInstanceUid, System.String sopInstanceUid, System.String patientId, System.String patientName, System.String referringPhysicianName, System.Nullable<System.DateTime> studyDate, System.String studyDescription, System.String accessionNumber, System.String modality, System.Nullable<System.DateTime> performedProcedureStepStartDate, System.Byte initialStatus)
            {
                command.CommandType = global::System.Data.CommandType.StoredProcedure;
                command.CommandText = "dbo.AddInstance";
                _studyInstanceUid.AddParameter(command.Parameters, studyInstanceUid);
                _seriesInstanceUid.AddParameter(command.Parameters, seriesInstanceUid);
                _sopInstanceUid.AddParameter(command.Parameters, sopInstanceUid);
                _patientId.AddParameter(command.Parameters, patientId);
                _patientName.AddParameter(command.Parameters, patientName);
                _referringPhysicianName.AddParameter(command.Parameters, referringPhysicianName);
                _studyDate.AddParameter(command.Parameters, studyDate);
                _studyDescription.AddParameter(command.Parameters, studyDescription);
                _accessionNumber.AddParameter(command.Parameters, accessionNumber);
                _modality.AddParameter(command.Parameters, modality);
                _performedProcedureStepStartDate.AddParameter(command.Parameters, performedProcedureStepStartDate);
                _initialStatus.AddParameter(command.Parameters, initialStatus);
            }
        }

        internal class DeleteCustomTagProcedure : StoredProcedure
        {
            internal DeleteCustomTagProcedure() : base("dbo.DeleteCustomTag")
            {
            }

            private readonly ParameterDefinition<System.String> _tagPath = new ParameterDefinition<System.String>("@tagPath", global::System.Data.SqlDbType.VarChar, false, 64);
            private readonly ParameterDefinition<System.Byte> _dataType = new ParameterDefinition<System.Byte>("@dataType", global::System.Data.SqlDbType.TinyInt, false);

            public void PopulateCommand(SqlCommandWrapper command, System.String tagPath, System.Byte dataType)
            {
                command.CommandType = global::System.Data.CommandType.StoredProcedure;
                command.CommandText = "dbo.DeleteCustomTag";
                _tagPath.AddParameter(command.Parameters, tagPath);
                _dataType.AddParameter(command.Parameters, dataType);
            }
        }

        internal class DeleteDeletedInstanceProcedure : StoredProcedure
        {
            internal DeleteDeletedInstanceProcedure() : base("dbo.DeleteDeletedInstance")
            {
            }

            private readonly ParameterDefinition<System.String> _studyInstanceUid = new ParameterDefinition<System.String>("@studyInstanceUid", global::System.Data.SqlDbType.VarChar, false, 64);
            private readonly ParameterDefinition<System.String> _seriesInstanceUid = new ParameterDefinition<System.String>("@seriesInstanceUid", global::System.Data.SqlDbType.VarChar, false, 64);
            private readonly ParameterDefinition<System.String> _sopInstanceUid = new ParameterDefinition<System.String>("@sopInstanceUid", global::System.Data.SqlDbType.VarChar, false, 64);
            private readonly ParameterDefinition<System.Int64> _watermark = new ParameterDefinition<System.Int64>("@watermark", global::System.Data.SqlDbType.BigInt, false);

            public void PopulateCommand(SqlCommandWrapper command, System.String studyInstanceUid, System.String seriesInstanceUid, System.String sopInstanceUid, System.Int64 watermark)
            {
                command.CommandType = global::System.Data.CommandType.StoredProcedure;
                command.CommandText = "dbo.DeleteDeletedInstance";
                _studyInstanceUid.AddParameter(command.Parameters, studyInstanceUid);
                _seriesInstanceUid.AddParameter(command.Parameters, seriesInstanceUid);
                _sopInstanceUid.AddParameter(command.Parameters, sopInstanceUid);
                _watermark.AddParameter(command.Parameters, watermark);
            }
        }

        internal class DeleteInstanceProcedure : StoredProcedure
        {
            internal DeleteInstanceProcedure() : base("dbo.DeleteInstance")
            {
            }

            private readonly ParameterDefinition<System.DateTimeOffset> _cleanupAfter = new ParameterDefinition<System.DateTimeOffset>("@cleanupAfter", global::System.Data.SqlDbType.DateTimeOffset, false, 0);
            private readonly ParameterDefinition<System.Byte> _createdStatus = new ParameterDefinition<System.Byte>("@createdStatus", global::System.Data.SqlDbType.TinyInt, false);
            private readonly ParameterDefinition<System.String> _studyInstanceUid = new ParameterDefinition<System.String>("@studyInstanceUid", global::System.Data.SqlDbType.VarChar, false, 64);
            private readonly ParameterDefinition<System.String> _seriesInstanceUid = new ParameterDefinition<System.String>("@seriesInstanceUid", global::System.Data.SqlDbType.VarChar, true, 64);
            private readonly ParameterDefinition<System.String> _sopInstanceUid = new ParameterDefinition<System.String>("@sopInstanceUid", global::System.Data.SqlDbType.VarChar, true, 64);

            public void PopulateCommand(SqlCommandWrapper command, System.DateTimeOffset cleanupAfter, System.Byte createdStatus, System.String studyInstanceUid, System.String seriesInstanceUid, System.String sopInstanceUid)
            {
                command.CommandType = global::System.Data.CommandType.StoredProcedure;
                command.CommandText = "dbo.DeleteInstance";
                _cleanupAfter.AddParameter(command.Parameters, cleanupAfter);
                _createdStatus.AddParameter(command.Parameters, createdStatus);
                _studyInstanceUid.AddParameter(command.Parameters, studyInstanceUid);
                _seriesInstanceUid.AddParameter(command.Parameters, seriesInstanceUid);
                _sopInstanceUid.AddParameter(command.Parameters, sopInstanceUid);
            }
        }

        internal class GetChangeFeedProcedure : StoredProcedure
        {
            internal GetChangeFeedProcedure() : base("dbo.GetChangeFeed")
            {
            }

            private readonly ParameterDefinition<System.Int32> _limit = new ParameterDefinition<System.Int32>("@limit", global::System.Data.SqlDbType.Int, false);
            private readonly ParameterDefinition<System.Int64> _offset = new ParameterDefinition<System.Int64>("@offset", global::System.Data.SqlDbType.BigInt, false);

            public void PopulateCommand(SqlCommandWrapper command, System.Int32 limit, System.Int64 offset)
            {
                command.CommandType = global::System.Data.CommandType.StoredProcedure;
                command.CommandText = "dbo.GetChangeFeed";
                _limit.AddParameter(command.Parameters, limit);
                _offset.AddParameter(command.Parameters, offset);
            }
        }

        internal class GetChangeFeedLatestProcedure : StoredProcedure
        {
            internal GetChangeFeedLatestProcedure() : base("dbo.GetChangeFeedLatest")
            {
            }

            public void PopulateCommand(SqlCommandWrapper command)
            {
                command.CommandType = global::System.Data.CommandType.StoredProcedure;
                command.CommandText = "dbo.GetChangeFeedLatest";
            }
        }

        internal class GetCustomTagProcedure : StoredProcedure
        {
            internal GetCustomTagProcedure() : base("dbo.GetCustomTag")
            {
            }

            private readonly ParameterDefinition<System.String> _tagPath = new ParameterDefinition<System.String>("@tagPath", global::System.Data.SqlDbType.VarChar, true, 64);

            public void PopulateCommand(SqlCommandWrapper command, System.String tagPath)
            {
                command.CommandType = global::System.Data.CommandType.StoredProcedure;
                command.CommandText = "dbo.GetCustomTag";
                _tagPath.AddParameter(command.Parameters, tagPath);
            }
        }

        internal class GetInstanceProcedure : StoredProcedure
        {
            internal GetInstanceProcedure() : base("dbo.GetInstance")
            {
            }

            private readonly ParameterDefinition<System.Byte> _validStatus = new ParameterDefinition<System.Byte>("@validStatus", global::System.Data.SqlDbType.TinyInt, false);
            private readonly ParameterDefinition<System.String> _studyInstanceUid = new ParameterDefinition<System.String>("@studyInstanceUid", global::System.Data.SqlDbType.VarChar, false, 64);
            private readonly ParameterDefinition<System.String> _seriesInstanceUid = new ParameterDefinition<System.String>("@seriesInstanceUid", global::System.Data.SqlDbType.VarChar, true, 64);
            private readonly ParameterDefinition<System.String> _sopInstanceUid = new ParameterDefinition<System.String>("@sopInstanceUid", global::System.Data.SqlDbType.VarChar, true, 64);

            public void PopulateCommand(SqlCommandWrapper command, System.Byte validStatus, System.String studyInstanceUid, System.String seriesInstanceUid, System.String sopInstanceUid)
            {
                command.CommandType = global::System.Data.CommandType.StoredProcedure;
                command.CommandText = "dbo.GetInstance";
                _validStatus.AddParameter(command.Parameters, validStatus);
                _studyInstanceUid.AddParameter(command.Parameters, studyInstanceUid);
                _seriesInstanceUid.AddParameter(command.Parameters, seriesInstanceUid);
                _sopInstanceUid.AddParameter(command.Parameters, sopInstanceUid);
            }
        }

        internal class IncrementDeletedInstanceRetryProcedure : StoredProcedure
        {
            internal IncrementDeletedInstanceRetryProcedure() : base("dbo.IncrementDeletedInstanceRetry")
            {
            }

            private readonly ParameterDefinition<System.String> _studyInstanceUid = new ParameterDefinition<System.String>("@studyInstanceUid", global::System.Data.SqlDbType.VarChar, false, 64);
            private readonly ParameterDefinition<System.String> _seriesInstanceUid = new ParameterDefinition<System.String>("@seriesInstanceUid", global::System.Data.SqlDbType.VarChar, false, 64);
            private readonly ParameterDefinition<System.String> _sopInstanceUid = new ParameterDefinition<System.String>("@sopInstanceUid", global::System.Data.SqlDbType.VarChar, false, 64);
            private readonly ParameterDefinition<System.Int64> _watermark = new ParameterDefinition<System.Int64>("@watermark", global::System.Data.SqlDbType.BigInt, false);
            private readonly ParameterDefinition<System.DateTimeOffset> _cleanupAfter = new ParameterDefinition<System.DateTimeOffset>("@cleanupAfter", global::System.Data.SqlDbType.DateTimeOffset, false, 0);

            public void PopulateCommand(SqlCommandWrapper command, System.String studyInstanceUid, System.String seriesInstanceUid, System.String sopInstanceUid, System.Int64 watermark, System.DateTimeOffset cleanupAfter)
            {
                command.CommandType = global::System.Data.CommandType.StoredProcedure;
                command.CommandText = "dbo.IncrementDeletedInstanceRetry";
                _studyInstanceUid.AddParameter(command.Parameters, studyInstanceUid);
                _seriesInstanceUid.AddParameter(command.Parameters, seriesInstanceUid);
                _sopInstanceUid.AddParameter(command.Parameters, sopInstanceUid);
                _watermark.AddParameter(command.Parameters, watermark);
                _cleanupAfter.AddParameter(command.Parameters, cleanupAfter);
            }
        }

        internal class RetrieveDeletedInstanceProcedure : StoredProcedure
        {
            internal RetrieveDeletedInstanceProcedure() : base("dbo.RetrieveDeletedInstance")
            {
            }

            private readonly ParameterDefinition<System.Int32> _count = new ParameterDefinition<System.Int32>("@count", global::System.Data.SqlDbType.Int, false);
            private readonly ParameterDefinition<System.Int32> _maxRetries = new ParameterDefinition<System.Int32>("@maxRetries", global::System.Data.SqlDbType.Int, false);

            public void PopulateCommand(SqlCommandWrapper command, System.Int32 count, System.Int32 maxRetries)
            {
                command.CommandType = global::System.Data.CommandType.StoredProcedure;
                command.CommandText = "dbo.RetrieveDeletedInstance";
                _count.AddParameter(command.Parameters, count);
                _maxRetries.AddParameter(command.Parameters, maxRetries);
            }
        }

        internal class UpdateInstanceStatusProcedure : StoredProcedure
        {
            internal UpdateInstanceStatusProcedure() : base("dbo.UpdateInstanceStatus")
            {
            }

            private readonly ParameterDefinition<System.String> _studyInstanceUid = new ParameterDefinition<System.String>("@studyInstanceUid", global::System.Data.SqlDbType.VarChar, false, 64);
            private readonly ParameterDefinition<System.String> _seriesInstanceUid = new ParameterDefinition<System.String>("@seriesInstanceUid", global::System.Data.SqlDbType.VarChar, false, 64);
            private readonly ParameterDefinition<System.String> _sopInstanceUid = new ParameterDefinition<System.String>("@sopInstanceUid", global::System.Data.SqlDbType.VarChar, false, 64);
            private readonly ParameterDefinition<System.Int64> _watermark = new ParameterDefinition<System.Int64>("@watermark", global::System.Data.SqlDbType.BigInt, false);
            private readonly ParameterDefinition<System.Byte> _status = new ParameterDefinition<System.Byte>("@status", global::System.Data.SqlDbType.TinyInt, false);

            public void PopulateCommand(SqlCommandWrapper command, System.String studyInstanceUid, System.String seriesInstanceUid, System.String sopInstanceUid, System.Int64 watermark, System.Byte status)
            {
                command.CommandType = global::System.Data.CommandType.StoredProcedure;
                command.CommandText = "dbo.UpdateInstanceStatus";
                _studyInstanceUid.AddParameter(command.Parameters, studyInstanceUid);
                _seriesInstanceUid.AddParameter(command.Parameters, seriesInstanceUid);
                _sopInstanceUid.AddParameter(command.Parameters, sopInstanceUid);
                _watermark.AddParameter(command.Parameters, watermark);
                _status.AddParameter(command.Parameters, status);
            }
        }
    }
}