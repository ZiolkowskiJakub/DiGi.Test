using DiGi.Analytical.Building.Classes;
using DiGi.Geometry.Planar.Classes;
using DiGi.Geometry.Spatial;
using DiGi.Geometry.Spatial.Classes;

namespace DiGi.Analytical.xUnit
{
    public partial class Facts
    {
        /// <summary>
        /// Creates the vertical XZ plane used by the hosting facts - axis X along world X, axis Y along world Z.
        /// </summary>
        /// <param name="x">The X coordinate of the plane origin.</param>
        /// <param name="y">The Y coordinate of the plane origin (the out-of-plane offset).</param>
        /// <param name="z">The Z coordinate of the plane origin.</param>
        /// <returns>The created <see cref="Plane"/>.</returns>
        private static Plane OpeningFixture_Plane(double x, double y, double z)
        {
            return new Plane(new Point3D(x, y, z), new Vector3D(1, 0, 0), new Vector3D(0, 0, 1));
        }

        /// <summary>
        /// Creates an axis-aligned rectangular <see cref="PolygonalFace2D"/>, optionally with rectangular holes.
        /// </summary>
        /// <param name="minX">The minimum X coordinate.</param>
        /// <param name="minY">The minimum Y coordinate.</param>
        /// <param name="maxX">The maximum X coordinate.</param>
        /// <param name="maxY">The maximum Y coordinate.</param>
        /// <param name="holes">The holes given as (minX, minY, maxX, maxY).</param>
        /// <returns>The created <see cref="PolygonalFace2D"/>.</returns>
        private static PolygonalFace2D OpeningFixture_Rectangle(double minX, double minY, double maxX, double maxY, params (double MinX, double MinY, double MaxX, double MaxY)[] holes)
        {
            static Polygon2D CreatePolygon2D(double minX, double minY, double maxX, double maxY)
            {
                return new Polygon2D([new Point2D(minX, minY), new Point2D(maxX, minY), new Point2D(maxX, maxY), new Point2D(minX, maxY)]);
            }

            List<Geometry.Planar.Interfaces.IPolygonal2D> polygonal2Ds_Internal = [];
            foreach ((double MinX, double MinY, double MaxX, double MaxY) hole in holes)
            {
                polygonal2Ds_Internal.Add(CreatePolygon2D(hole.MinX, hole.MinY, hole.MaxX, hole.MaxY));
            }

            PolygonalFace2D? polygonalFace2D = Geometry.Planar.Create.PolygonalFace2D(CreatePolygon2D(minX, minY, maxX, maxY), polygonal2Ds_Internal.Count == 0 ? null : polygonal2Ds_Internal);
            Assert.NotNull(polygonalFace2D);

            return polygonalFace2D;
        }

        /// <summary>
        /// Creates a 10 x 3 <see cref="SurfaceWall"/> on the vertical XZ plane through the world origin, optionally with rectangular holes.
        /// </summary>
        /// <param name="holes">The holes given in the wall's local coordinates as (minX, minY, maxX, maxY).</param>
        /// <returns>The created <see cref="SurfaceWall"/>.</returns>
        private static SurfaceWall OpeningFixture_SurfaceWall(params (double MinX, double MinY, double MaxX, double MaxY)[] holes)
        {
            PolygonalFace3D? polygonalFace3D = OpeningFixture_Plane(0, 0, 0).Convert(OpeningFixture_Rectangle(0, 0, 10, 3, holes));
            Assert.NotNull(polygonalFace3D);

            SurfaceWall? surfaceWall = Building.Create.SurfaceWall(polygonalFace3D);
            Assert.NotNull(surfaceWall);

            return surfaceWall;
        }

        /// <summary>
        /// Creates a <see cref="WindowConstruction"/> with a rectangular outline anchored at the plane origin.
        /// </summary>
        /// <param name="width">The width of the outline.</param>
        /// <param name="height">The height of the outline.</param>
        /// <returns>The created <see cref="WindowConstruction"/>.</returns>
        private static WindowConstruction OpeningFixture_WindowConstruction(double width, double height)
        {
            return new WindowConstruction("Window Construction", OpeningFixture_Rectangle(0, 0, width, height));
        }
    }
}
