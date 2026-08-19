using DiGi.GIS.PostgreSQL.Enums;

namespace DiGi.GIS.PostgreSQL.xUnit
{
    public partial class Facts
    {
        /// <summary>
        /// Verifies the slice <see cref="Query.AdministrativeCodeKey(string, AdministrativeArealType)"/> takes for each level a code
        /// can name: 2 characters for the voivodeship, 4 for the county and 6 for the municipality.
        /// </summary>
        [Fact]
        public void AdministrativeCodeKey_Levels()
        {
            // Kłecko-Kolonia, one of the settlements that lost its parent chain.
            const string code = "3003053";

            Assert.Equal("30", Query.AdministrativeCodeKey(code, AdministrativeArealType.Voivodeship));
            Assert.Equal("3003", Query.AdministrativeCodeKey(code, AdministrativeArealType.County));
            Assert.Equal("300305", Query.AdministrativeCodeKey(code, AdministrativeArealType.Municipality));

            // A municipality code slices the same way, so one call serves both sides of the lookup.
            Assert.Equal("3003", Query.AdministrativeCodeKey("3003053", AdministrativeArealType.County));
            Assert.Equal("30", Query.AdministrativeCodeKey("3003", AdministrativeArealType.Voivodeship));
        }

        /// <summary>
        /// Verifies that the municipality slice stops at 6 characters, so a settlement in the rural part of an urban-rural gmina still
        /// names the single gmina feature BDOT10k stores for it.
        /// <para>The 7th character is the gmina type - <c>3</c> for the urban-rural gmina itself against <c>4</c> for its town and
        /// <c>5</c> for its rural area - and 9 162 of the stored subdivisions carry a type digit no municipality row uses.</para>
        /// </summary>
        [Fact]
        public void AdministrativeCodeKey_MunicipalityTypeDigit()
        {
            string? key_Municipality = Query.AdministrativeCodeKey("2601013", AdministrativeArealType.Municipality);
            string? key_RuralArea = Query.AdministrativeCodeKey("2601015", AdministrativeArealType.Municipality);
            string? key_Town = Query.AdministrativeCodeKey("2601014", AdministrativeArealType.Municipality);

            Assert.Equal("260101", key_Municipality);
            Assert.Equal(key_Municipality, key_RuralArea);
            Assert.Equal(key_Municipality, key_Town);
        }

        /// <summary>
        /// Verifies that Country yields no key.
        /// <para>Every country row's code is <c>10</c>, which is also the voivodeship code of łódzkie, so a 2-character slice there
        /// would name one voivodeship's private ancestor chain rather than the country. A null tells the caller it has no code
        /// constraint and must search the whole level.</para>
        /// </summary>
        [Fact]
        public void AdministrativeCodeKey_Country()
        {
            Assert.Null(Query.AdministrativeCodeKey("3003053", AdministrativeArealType.Country));
            Assert.Null(Query.AdministrativeCodeKey("10", AdministrativeArealType.Country));
            Assert.Null(Query.AdministrativeCodeKey("1004032", AdministrativeArealType.Country));
            Assert.Null(Query.AdministrativeCodeKey("3003053", AdministrativeArealType.Undefined));
        }

        /// <summary>
        /// Verifies that a missing code, or one too short to reach the requested level, yields no key rather than a truncated one.
        /// </summary>
        [Fact]
        public void AdministrativeCodeKey_Unusable()
        {
            Assert.Null(Query.AdministrativeCodeKey(null, AdministrativeArealType.County));
            Assert.Null(Query.AdministrativeCodeKey(string.Empty, AdministrativeArealType.County));
            Assert.Null(Query.AdministrativeCodeKey("   ", AdministrativeArealType.County));

            // A county code reaches the voivodeship but not the municipality.
            Assert.Equal("30", Query.AdministrativeCodeKey("3003", AdministrativeArealType.Voivodeship));
            Assert.Null(Query.AdministrativeCodeKey("3003", AdministrativeArealType.Municipality));

            // A voivodeship code reaches nothing below itself.
            Assert.Null(Query.AdministrativeCodeKey("30", AdministrativeArealType.County));
        }
    }
}
