using DiGi.GIS.PostgreSQL.Enums;

namespace DiGi.GIS.PostgreSQL.xUnit
{
    public partial class Facts
    {
        /// <summary>
        /// Verifies the column each administrative areal type is stored in, and that nothing stores a Subdivision identifier.
        /// </summary>
        [Fact]
        public void IdColumnName_Levels()
        {
            Assert.Equal("country_id", Query.IdColumnName(AdministrativeArealType.Country));
            Assert.Equal("voivodeship_id", Query.IdColumnName(AdministrativeArealType.Voivodeship));
            Assert.Equal("county_id", Query.IdColumnName(AdministrativeArealType.County));
            Assert.Equal("municipality_id", Query.IdColumnName(AdministrativeArealType.Municipality));

            Assert.Null(Query.IdColumnName(AdministrativeArealType.Subdivision));
            Assert.Null(Query.IdColumnName(AdministrativeArealType.Undefined));
        }

        /// <summary>
        /// Verifies that the relative form still names the level directly above, so a search that has not had to step over an empty
        /// level behaves exactly as before.
        /// </summary>
        [Fact]
        public void ParentIdColumnName_Levels()
        {
            Assert.Equal("municipality_id", Query.ParentIdColumnName(AdministrativeArealType.Subdivision));
            Assert.Equal("county_id", Query.ParentIdColumnName(AdministrativeArealType.Municipality));
            Assert.Equal("voivodeship_id", Query.ParentIdColumnName(AdministrativeArealType.County));
            Assert.Equal("country_id", Query.ParentIdColumnName(AdministrativeArealType.Voivodeship));

            Assert.Null(Query.ParentIdColumnName(AdministrativeArealType.Country));
            Assert.Null(Query.ParentIdColumnName(AdministrativeArealType.Undefined));
        }

        /// <summary>
        /// Verifies that the two forms agree wherever a level has a parent, which is what makes <see cref="Query.IdColumnName(AdministrativeArealType)"/>
        /// a safe substitute in the level-by-level searches.
        /// </summary>
        [Fact]
        public void IdColumnName_AgreesWithParentIdColumnName()
        {
            foreach (AdministrativeArealType administrativeArealType in new[] { AdministrativeArealType.Voivodeship, AdministrativeArealType.County, AdministrativeArealType.Municipality, AdministrativeArealType.Subdivision })
            {
                AdministrativeArealType? administrativeArealType_Parent = Query.ParentAdministrativeArealType(administrativeArealType);
                Assert.NotNull(administrativeArealType_Parent);

                Assert.Equal(Query.ParentIdColumnName(administrativeArealType), Query.IdColumnName(administrativeArealType_Parent.Value));
            }
        }
    }
}
