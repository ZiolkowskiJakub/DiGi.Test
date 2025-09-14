using DiGi.Analytical.Building.Classes;
using DiGi.Geometry.Spatial.Classes;
using DiGi.Core.Interfaces;

namespace DiGi.Analytical.xUnit
{
    public partial class Classes
    {
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

            FaceFloor faceFloor = new (polygonalFace3D);

            Assert.NotNull(faceFloor?.Geometry);

            Space space = new(new Point3D(0, 0, 0), "Space 1");

            Building.Classes.SpaceRelation spaceRelation = new (faceFloor, space);

            Assert.NotNull(spaceRelation?.UniqueReferences);

            if(spaceRelation.UniqueReferences is List<IUniqueReference> uniqueReferences)
            {
                return;
            }

            Assert.Equal(2, spaceRelation.UniqueReferences.Count);

            if(spaceRelation.UniqueReferences.Count != 2)
            {  
                return; 
            }

            IUniqueReference? uniqueReference = Core.Create.UniqueReference(faceFloor);

            Assert.True(spaceRelation.UniqueReferences.Contains(uniqueReference));


        }
    }
}