using DiGi.GIS.PostgreSQL.Classes;
using System.Collections.Generic;

namespace DiGi.GIS.PostgreSQL.xUnit
{
    public partial class Facts
    {
        /// <summary>
        /// Verifies that <see cref="Building2DReferenceDuplicate"/> properties are correctly initialized and survive serialization and copying.
        /// </summary>
        [Fact]
        public void Building2DReferenceDuplicate_Serialization()
        {
            string reference = "2BE0D403-72F3-6A3E-E053-CA2BA8C0618D";
            long count = 2;
            List<int> countyIds = [10365, 104106];

            Building2DReferenceDuplicate building2DReferenceDuplicate = new(reference, count, countyIds);

            Assert.Equal(reference, building2DReferenceDuplicate.Reference);
            Assert.Equal(count, building2DReferenceDuplicate.Count);
            Assert.NotNull(building2DReferenceDuplicate.CountyIds);
            Assert.Equal(2, building2DReferenceDuplicate.CountyIds.Count);
            Assert.Equal(10365, building2DReferenceDuplicate.CountyIds[0]);
            Assert.Equal(104106, building2DReferenceDuplicate.CountyIds[1]);

            Core.xUnit.Query.SerializationCheck(building2DReferenceDuplicate);

            Building2DReferenceDuplicate building2DReferenceDuplicate_Clone = new(building2DReferenceDuplicate);

            Assert.Equal(building2DReferenceDuplicate.Reference, building2DReferenceDuplicate_Clone.Reference);
            Assert.Equal(building2DReferenceDuplicate.Count, building2DReferenceDuplicate_Clone.Count);
            Assert.NotNull(building2DReferenceDuplicate_Clone.CountyIds);
            Assert.Equal(building2DReferenceDuplicate.CountyIds.Count, building2DReferenceDuplicate_Clone.CountyIds.Count);
        }
    }
}
