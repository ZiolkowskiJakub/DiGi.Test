using DiGi.Geometry.Spatial;
using DiGi.Geometry.Spatial.Classes;
using DiGi.Geometry.Spatial.Interfaces;

namespace DiGi.Geometry.xUnit
{
    public partial class Classes
    {
        /// <summary>
        /// Tests the extrusion of a polygonal face to verify that it correctly generates a valid polyhedron and allows for the retrieval of an internal point.
        /// </summary>
        [Fact]
        public void PolygonalFaceExtrusion()
        {
            PolygonalFace3D? polygonalFace3D = Create.PolygonalFace3D(Spatial.Constants.Plane.WorldZ, new Planar.Classes.Point2D(0, 0), new Planar.Classes.Point2D(0, 10), new Planar.Classes.Point2D(10, 10), new Planar.Classes.Point2D(10, 0));

            Assert.NotNull(polygonalFace3D);

            if (polygonalFace3D is null)
            {
                return;
            }

            PolygonalFaceExtrusion polygonalFaceExtrusion = new(polygonalFace3D, Spatial.Constants.Vector3D.WorldZ * 10);

            Assert.NotNull(polygonalFaceExtrusion);

            if (polygonalFaceExtrusion is null)
            {
                return;
            }

            Polyhedron? polyhedron = Create.Polyhedron(polygonalFaceExtrusion);
            Assert.NotNull(polyhedron);

            if (polyhedron is null)
            {
                return;
            }

            Point3D? internalPoint = polyhedron.GetInternalPoint();
            Assert.NotNull(internalPoint);

            for (int i = 0; i < polyhedron.Count; i++)
            {
                IPolygonalFace3D? polygonalFace3D_Polyhedron = polyhedron[i];
                Assert.NotNull(polygonalFace3D_Polyhedron);

                if (polygonalFace3D_Polyhedron is null)
                {
                    continue;
                }

                Point3D? internalPoint_PolygonalFace3D = polygonalFace3D_Polyhedron.GetInternalPoint();
                Assert.NotNull(internalPoint_PolygonalFace3D);

                if (internalPoint_PolygonalFace3D is null)
                {
                    continue;
                }

                Vector3D? normal_Internal = polyhedron.GetNormal(i, Core.Enums.Side.Internal);
                Assert.NotNull(normal_Internal);

                Vector3D? normal_External = polyhedron.GetNormal(i, Core.Enums.Side.External);
                Assert.NotNull(normal_External);

                if (normal_External is null)
                {
                    continue;
                }

                Point3D? point3D = internalPoint_PolygonalFace3D.GetMoved(normal_External);
                Assert.NotNull(point3D);
                if (point3D is null)
                {
                    continue;
                }

                Assert.True(!polyhedron.Inside(point3D));
            }
        }
    }
}
