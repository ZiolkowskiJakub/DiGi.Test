using DiGi.Geometry.Planar.Classes;
using DiGi.Geometry.Planar.Interfaces;
using DiGi.Geometry.Spatial;
using DiGi.Geometry.Spatial.Classes;

namespace DiGi.Geometry.xUnit
{
    public partial class Classes
    {
        [Fact]
        public void PolygonalFace2DInternalPointSolver()
        {
            PolygonalFace3D? polygonalFace3D = Create.PolygonalFace3D(Spatial.Constans.Plane.WorldZ, new Point2D(0, 0), new Point2D(0, 10), new Point2D(10, 10), new Point2D(10, 0));
            Assert.NotNull(polygonalFace3D);

            if(polygonalFace3D is null)
            {
                return;
            }

            int count = 1000;

            IPolygonalFace2D? polygonalFace2D = polygonalFace3D.Geometry2D;

            PolygonalFace2DInternalPointSolver polygonalFace2DInternalPointSolver = new (100, polygonalFace2D);

            List<Point2D> point2Ds = [];
            for (int i = 0; i < count; i++)
            {
                if(!polygonalFace2DInternalPointSolver.Solve())
                {
                    break;
                }

                if(polygonalFace2DInternalPointSolver.InternalPoint is not Point2D point2D)
                {
                    break;
                }

                point2Ds.Add(point2D);

                Assert.True(polygonalFace2D.Inside(point2D));
            }

        }
    }
}