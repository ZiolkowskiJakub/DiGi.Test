using DiGi.Analytical.Building.Classes;
using DiGi.Core.Interfaces;
using DiGi.Geometry.Spatial.Classes;

namespace DiGi.Analytical.xUnit
{
    public partial class Facts
    {
        /// <summary>
        /// Tests the creation and initialization of a SpaceRelation object to ensure it correctly associates a FaceFloor with a Space and maintains its unique references.
        /// </summary>
        [Fact]
        public void SpaceRelation()
        {
            Plane? plane = Geometry.Spatial.Create.Plane(0.0);

            PolygonalFace3D? polygonalFace3D = Geometry.Spatial.Create.PolygonalFace3D(plane,
            [
                new Geometry.Planar.Classes.Point2D(0, 0),
                new Geometry.Planar.Classes.Point2D(0, 10),
                new Geometry.Planar.Classes.Point2D(10, 0),
                new Geometry.Planar.Classes.Point2D(10, 10)
            ]);

            FaceFloor faceFloor = new(polygonalFace3D);

            Assert.NotNull(faceFloor?.Geometry);

            Space space = new(new Point3D(0, 0, 0), "Space 1");

            SpaceRelation spaceRelation = new(faceFloor, space);

            Assert.NotNull(spaceRelation?.UniqueReferences);

            if (spaceRelation.UniqueReferences is List<IUniqueReference> uniqueReferences)
            {
                return;
            }

            Assert.Equal(2, spaceRelation.UniqueReferences.Count);

            if (spaceRelation.UniqueReferences.Count != 2)
            {
                return;
            }

            IUniqueReference? uniqueReference = Core.Create.UniqueReference(faceFloor);

            Assert.Contains(uniqueReference, spaceRelation.UniqueReferences);
        }
    }
}