using DiGi.Core.Parameter.Classes;
using DiGi.GIS.Analytical.Enums;
using DiGi.GIS.PostgreSQL.Classes;
using System;
using System.Text.Json.Nodes;

namespace DiGi.GIS.PostgreSQL.xUnit
{
    public partial class Facts
    {
        /// <summary>
        /// Verifies that a building model converted for storage is keyed on its own identifier and not on the reference of the building it describes.
        /// <para>This is the regression guard for the row identity of <c>building_model</c>. The table constrains <c>UNIQUE (county_id, unique_id)</c>, so what goes in that column decides what a row means: the model's identifier makes a row one stored model, the way every other referenced-object table works, while the building's reference pins the table to one row per building and silently discards every record after the first.</para>
        /// <para>The reference still has to reach <c>Reference</c>, because <c>(CountyId, Reference)</c> is what addresses everything held for a building. The two columns carry different values and the point of the check is that they are no longer the same one.</para>
        /// </summary>
        [Fact]
        public void ToPostgreSQL_BuildingModel_KeyedOnUniqueId()
        {
            const string reference = "ba5HMOrk-qIrU-QuZ4-GbM6-cyWrMZheOe2j";
            const int countyId = 5;

            // A model with no components is valid - the converter's last gate checks the planes of the
            // components there are - so the reference is all this needs to be storable.
            DiGi.Analytical.Building.Classes.BuildingModel buildingModel = new();
            Assert.True(buildingModel.SetValue(BuildingModelParameter.Reference, reference, new SetValueSettings(true, false)));

            BuildingModel? buildingModel_PostgreSQL = buildingModel.ToPostgreSQL(countyId);

            Assert.NotNull(buildingModel_PostgreSQL);
            Assert.Equal(reference, buildingModel_PostgreSQL.Reference);
            Assert.Equal(countyId, buildingModel_PostgreSQL.CountyId);

            Assert.Equal(buildingModel.UniqueId, buildingModel_PostgreSQL.UniqueId);
            Assert.NotEqual(reference, buildingModel_PostgreSQL.UniqueId);
        }

        /// <summary>
        /// Verifies that the identifier written to the row is the model's guid in the form the database migration reproduces from the stored JSON.
        /// <para>The migration promotes the identifier already held inside the <c>object</c> column into <c>unique_id</c>, and it does that in SQL by stripping the dashes from the serialized guid. That only lands on the same value because <c>GuidObject.UniqueId</c> is the guid formatted <c>N</c> - 32 hexadecimal characters, lower case, no separators - while the serialized form is the ordinary dashed one. If that ever stops being true the migration writes an identifier the converter would never emit, and rows migrated before the change stop matching the models written after it.</para>
        /// </summary>
        [Fact]
        public void ToPostgreSQL_BuildingModel_UniqueIdMatchesStoredGuid()
        {
            const string reference = "ba5HMOrk-qIrU-QuZ4-GbM6-cyWrMZheOe2j";

            DiGi.Analytical.Building.Classes.BuildingModel buildingModel = new();
            Assert.True(buildingModel.SetValue(BuildingModelParameter.Reference, reference, new SetValueSettings(true, false)));

            BuildingModel? buildingModel_PostgreSQL = buildingModel.ToPostgreSQL(5);

            Assert.NotNull(buildingModel_PostgreSQL);
            Assert.NotNull(buildingModel_PostgreSQL.UniqueId);

            Assert.Equal(32, buildingModel_PostgreSQL.UniqueId.Length);
            Assert.DoesNotContain("-", buildingModel_PostgreSQL.UniqueId, StringComparison.Ordinal);
            Assert.Equal(buildingModel_PostgreSQL.UniqueId, buildingModel_PostgreSQL.UniqueId.ToLowerInvariant());

            // Asserted against the serialized form rather than the in-memory node, because that is what the
            // column holds: the node still carries a System.Guid, and the dashed string the migration reads
            // with object->>'Guid' only exists once the converter has written it out with ToJsonString.
            Assert.NotNull(buildingModel_PostgreSQL.Object);
            JsonNode? jsonNode_Stored = JsonNode.Parse(buildingModel_PostgreSQL.Object.ToJsonString());
            Assert.NotNull(jsonNode_Stored);

            string? guid = jsonNode_Stored[Core.Constants.Serialization.PropertyName.Guid]?.GetValue<string>();
            Assert.False(string.IsNullOrWhiteSpace(guid));

            // This is the transformation the migration performs in SQL, written out in C#.
            Assert.Equal(buildingModel_PostgreSQL.UniqueId, guid!.Replace("-", string.Empty).ToLowerInvariant());
        }

        /// <summary>
        /// Verifies that a building model carrying no reference is refused rather than stored.
        /// <para><c>Reference</c> is not nullable in the table and is half of what addresses the row, so a model that cannot state which building it describes has nowhere to go. The converter answering null is what lets the caller count it as not written instead of failing the whole batch.</para>
        /// </summary>
        [Fact]
        public void ToPostgreSQL_BuildingModel_WithoutReference()
        {
            DiGi.Analytical.Building.Classes.BuildingModel buildingModel = new();

            Assert.Null(buildingModel.ToPostgreSQL(5));
        }
    }
}
