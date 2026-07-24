using DiGi.Analytical.Building.Classes;
using DiGi.Analytical.Building.Enums;
using DiGi.Geometry.Spatial.Classes;

namespace DiGi.Analytical.xUnit
{
    public partial class Facts
    {
        /// <summary>
        /// Creates a horizontal square <see cref="PolygonalFace3D"/> used as the geometry of the components under test.
        /// </summary>
        /// <returns>The created <see cref="PolygonalFace3D"/>.</returns>
        private static PolygonalFace3D ComponentStructurePosition_PolygonalFace3D()
        {
            Plane? plane = Geometry.Spatial.Create.Plane(0.0);

            PolygonalFace3D? polygonalFace3D = Geometry.Spatial.Create.PolygonalFace3D(plane,
            [
                new Geometry.Planar.Classes.Point2D(0, 0),
                new Geometry.Planar.Classes.Point2D(10, 0),
                new Geometry.Planar.Classes.Point2D(10, 10),
                new Geometry.Planar.Classes.Point2D(0, 10)
            ]);

            Assert.NotNull(polygonalFace3D);

            return polygonalFace3D;
        }

        /// <summary>
        /// Tests that the copy constructors of <see cref="PhysicalComponent"/> carry <see cref="StructurePosition"/> over to the new instance and leave the source instance untouched.
        /// </summary>
        [Fact]
        public void ComponentStructurePosition_PhysicalComponent()
        {
            PolygonalFace3D polygonalFace3D = ComponentStructurePosition_PolygonalFace3D();

            FaceFloor faceFloor = new(polygonalFace3D)
            {
                StructurePosition = StructurePosition.Internal
            };

            FaceFloor faceFloor_Copy = new(faceFloor);
            Assert.Equal(StructurePosition.Internal, faceFloor_Copy.StructurePosition);
            Assert.Equal(StructurePosition.Internal, faceFloor.StructurePosition);

            FaceFloor faceFloor_Copy_Guid = new(System.Guid.NewGuid(), faceFloor);
            Assert.Equal(StructurePosition.Internal, faceFloor_Copy_Guid.StructurePosition);
            Assert.Equal(StructurePosition.Internal, faceFloor.StructurePosition);

            FaceFloor faceFloor_Copy_Geometry = new(faceFloor, polygonalFace3D);
            Assert.Equal(StructurePosition.Internal, faceFloor_Copy_Geometry.StructurePosition);
            Assert.Equal(StructurePosition.Internal, faceFloor.StructurePosition);
            Assert.Equal(faceFloor.Guid, faceFloor_Copy_Geometry.Guid);

            Core.xUnit.Query.SerializationCheck(faceFloor);
        }

        /// <summary>
        /// Tests that the copy constructors of <see cref="AbstractComponent"/> carry <see cref="StructurePosition"/> over to the new instance and leave the source instance untouched.
        /// </summary>
        [Fact]
        public void ComponentStructurePosition_AbstractComponent()
        {
            PolygonalFace3D polygonalFace3D = ComponentStructurePosition_PolygonalFace3D();

            SurfaceAir surfaceAir = new(polygonalFace3D)
            {
                StructurePosition = StructurePosition.External
            };

            SurfaceAir surfaceAir_Copy = new(surfaceAir);
            Assert.Equal(StructurePosition.External, surfaceAir_Copy.StructurePosition);
            Assert.Equal(StructurePosition.External, surfaceAir.StructurePosition);

            SurfaceAir surfaceAir_Copy_Guid = new(System.Guid.NewGuid(), surfaceAir);
            Assert.Equal(StructurePosition.External, surfaceAir_Copy_Guid.StructurePosition);
            Assert.Equal(StructurePosition.External, surfaceAir.StructurePosition);

            SurfaceAir surfaceAir_Copy_Geometry = new(surfaceAir, polygonalFace3D);
            Assert.Equal(StructurePosition.External, surfaceAir_Copy_Geometry.StructurePosition);
            Assert.Equal(StructurePosition.External, surfaceAir.StructurePosition);
            Assert.Equal(surfaceAir.Guid, surfaceAir_Copy_Geometry.Guid);

            Core.xUnit.Query.SerializationCheck(surfaceAir);
        }
    }
}
