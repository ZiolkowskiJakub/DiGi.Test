using DiGi.Geometry.Planar.Classes;
using DiGi.GIS.PostgreSQL.Classes;
using System;

namespace DiGi.GIS.PostgreSQL.xUnit
{
    public partial class Facts
    {
        /// <summary>
        /// Verifies that <see cref="OrtoDatasReference"/> properties are correctly initialized, copied, and survive JSON serialization.
        /// </summary>
        [Fact]
        public void OrtoDatasReference_Serialization()
        {
            long id = 123456;
            int countyId = 55417;
            int subdivisionId = 3064;
            string reference = "2BE0D403-72F3-6A3E-E053-CA2BA8C0618D";
            BoundingBox2D boundingBox2D = new(new Point2D(10.0, 20.0), new Point2D(30.0, 40.0));
            DateTime createdAt = new(2026, 8, 25, 12, 0, 0, DateTimeKind.Utc);

            OrtoDatasReference ortoDatasReference = new()
            {
                Id = id,
                CountyId = countyId,
                SubdivisionId = subdivisionId,
                Reference = reference,
                BoundingBox2D = boundingBox2D,
                CreatedAt = createdAt
            };

            Assert.Equal(id, ortoDatasReference.Id);
            Assert.Equal(countyId, ortoDatasReference.CountyId);
            Assert.Equal(subdivisionId, ortoDatasReference.SubdivisionId);
            Assert.Equal(reference, ortoDatasReference.Reference);
            Assert.NotNull(ortoDatasReference.BoundingBox2D);
            Assert.Equal(10.0, ortoDatasReference.BoundingBox2D.Min.X);
            Assert.Equal(20.0, ortoDatasReference.BoundingBox2D.Min.Y);
            Assert.Equal(30.0, ortoDatasReference.BoundingBox2D.Max.X);
            Assert.Equal(40.0, ortoDatasReference.BoundingBox2D.Max.Y);
            Assert.Equal(createdAt, ortoDatasReference.CreatedAt);

            Core.xUnit.Query.SerializationCheck(ortoDatasReference);

            OrtoDatasReference ortoDatasReference_Clone = new(ortoDatasReference);

            Assert.Equal(ortoDatasReference.Id, ortoDatasReference_Clone.Id);
            Assert.Equal(ortoDatasReference.CountyId, ortoDatasReference_Clone.CountyId);
            Assert.Equal(ortoDatasReference.SubdivisionId, ortoDatasReference_Clone.SubdivisionId);
            Assert.Equal(ortoDatasReference.Reference, ortoDatasReference_Clone.Reference);
            Assert.NotNull(ortoDatasReference_Clone.BoundingBox2D);
            Assert.Equal(ortoDatasReference.BoundingBox2D.Min.X, ortoDatasReference_Clone.BoundingBox2D.Min.X);
            Assert.Equal(ortoDatasReference.CreatedAt, ortoDatasReference_Clone.CreatedAt);

            Building2DReference building2DReference = new()
            {
                Id = 789,
                CountyId = 55417,
                SubdivisionId = 3064,
                Reference = "BUILDING_REF_1"
            };
            OrtoDatasReference ortoDatasReference_FromBuilding = new(building2DReference);
            Assert.Equal(789, ortoDatasReference_FromBuilding.Id);
            Assert.Equal(55417, ortoDatasReference_FromBuilding.CountyId);
            Assert.Equal(3064, ortoDatasReference_FromBuilding.SubdivisionId);
            Assert.Equal("BUILDING_REF_1", ortoDatasReference_FromBuilding.Reference);
        }
    }
}
