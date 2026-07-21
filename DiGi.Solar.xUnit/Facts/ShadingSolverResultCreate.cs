using DiGi.Geometry.Planar.Classes;
using DiGi.Geometry.Planar.Interfaces;
using DiGi.Geometry.Spatial.Classes;
using DiGi.Solar.Classes;
using DiGi.Solar.ComputeSharp;
using DiGi.Solar.Enums;
using DiGi.Solar.Interfaces;
using System.Runtime.Versioning;

namespace DiGi.Solar.xUnit
{
    public partial class Facts
    {
        /// <summary>
        /// Tests the factory method Create.ShadingSolverResult for a geometrical solver result type.
        /// </summary>
        [Fact]
        [SupportedOSPlatform("windows")]
        public void ShadingSolverResult_Create_Geometrical()
        {
            DateTime dateTime = new(2026, 6, 26, 12, 0, 0);
            Plane plane = Geometry.Spatial.Constants.Plane.WorldZ;
            Polygon2D externalEdge = new([new Point2D(0.0, 0.0), new Point2D(10.0, 0.0), new Point2D(10.0, 10.0), new Point2D(0.0, 10.0)]);
            PolygonalFace2D polygonalFace2D = Geometry.Planar.Create.PolygonalFace2D(externalEdge, [])!;
            List<IPolygonalFace2D> polygonalFace2Ds = [polygonalFace2D];

            IShadingSolverResult? result = Create.ShadingSolverResult(ShadingSolverType.Geometrical, dateTime, plane, polygonalFace2Ds);

            Assert.NotNull(result);
            Assert.IsType<GeometricalShadingSolverResult>(result);
            Assert.Equal(dateTime, result.DateTime);
            Assert.Equal(100.0, result.Area, 5);

            // Null plane boundary case for Geometrical type
            IShadingSolverResult? result_NullPlane = Create.ShadingSolverResult(ShadingSolverType.Geometrical, dateTime, null, polygonalFace2Ds);
            Assert.Null(result_NullPlane);
        }

        /// <summary>
        /// Tests the factory method Create.ShadingSolverResult for a numerical solver result type.
        /// </summary>
        [Fact]
        [SupportedOSPlatform("windows")]
        public void ShadingSolverResult_Create_Numerical()
        {
            DateTime dateTime = new(2026, 6, 26, 12, 0, 0);
            Polygon2D externalEdge_1 = new([new Point2D(0.0, 0.0), new Point2D(5.0, 0.0), new Point2D(5.0, 5.0), new Point2D(0.0, 5.0)]);
            Polygon2D externalEdge_2 = new([new Point2D(0.0, 0.0), new Point2D(10.0, 0.0), new Point2D(10.0, 10.0), new Point2D(0.0, 10.0)]);
            PolygonalFace2D polygonalFace2D_1 = Geometry.Planar.Create.PolygonalFace2D(externalEdge_1, [])!;
            PolygonalFace2D polygonalFace2D_2 = Geometry.Planar.Create.PolygonalFace2D(externalEdge_2, [])!;
            List<IPolygonalFace2D> polygonalFace2Ds = [polygonalFace2D_1, polygonalFace2D_2];

            IShadingSolverResult? result = Create.ShadingSolverResult(ShadingSolverType.Numerical, dateTime, null, polygonalFace2Ds);

            Assert.NotNull(result);
            Assert.IsType<NumericalShadingSolverResult>(result);
            Assert.Equal(dateTime, result.DateTime);
            Assert.Equal(125.0, result.Area, 5);
        }

        /// <summary>
        /// Tests that the factory method Create.ShadingSolverResult returns null when the solver type is Undefined.
        /// </summary>
        [Fact]
        [SupportedOSPlatform("windows")]
        public void ShadingSolverResult_Create_Undefined()
        {
            DateTime dateTime = new(2026, 6, 26, 12, 0, 0);
            IShadingSolverResult? result = Create.ShadingSolverResult(ShadingSolverType.Undefined, dateTime, null, null);
            Assert.Null(result);
        }
    }
}
