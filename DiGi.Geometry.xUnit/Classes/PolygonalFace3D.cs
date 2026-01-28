using DiGi.Geometry.Core;
using DiGi.Geometry.Spatial;
using DiGi.Geometry.Spatial.Classes;
using DiGi.Geometry.Spatial.Interfaces;

namespace DiGi.Geometry.xUnit
{
    public partial class Classes
    {
        [Fact]
        public void PolygonalFace3D()
        {
            List<Point3D> point3Ds;

            Plane plane = Spatial.Constans.Plane.WorldZ;

            point3Ds = [new Point3D(0, 0, 0), new Point3D(10, 0, 0), new Point3D(10, 10, 0), new Point3D(0, 10, 0)];

            Polygon3D externalEdge = new(plane, point3Ds.ConvertAll(plane.Convert)!);

            Assert.True(Core.Enums.Orientation.CounterClockwise == externalEdge.Orientation());

            point3Ds = [new Point3D(1, 1, 0), new Point3D(1, 5, 0), new Point3D(5, 5, 0), new Point3D(5, 1, 0)];

            Polygon3D internalEdge = new(plane, point3Ds.ConvertAll(plane.Convert)!);

            Assert.True(Core.Enums.Orientation.Clockwise == internalEdge.Orientation());

            PolygonalFace3D? polygonalFace3D = Spatial.Create.PolygonalFace3D(externalEdge, [internalEdge]);
            Assert.NotNull(polygonalFace3D);

            if (polygonalFace3D is null)
            {
                return;
            }

            List<IPolygonal3D>? internalEdges = polygonalFace3D.InternalEdges;
            Assert.NotNull(internalEdges);

            if (internalEdges is null)
            {
                return;
            }

            Assert.Single(internalEdges);

            if (internalEdges.Count != 1)
            {
                return;
            }

            polygonalFace3D.Orient(externalEdge.Orientation().Opposite(), internalEdge.Orientation().Opposite());

            Assert.True(polygonalFace3D.ExternalEdge.Orientation() == externalEdge.Orientation().Opposite());
            Assert.True(polygonalFace3D.InternalEdges?[0]?.Orientation() == internalEdge.Orientation().Opposite());
        }
    }
}