using DiGi.Geometry.Spatial;
using DiGi.Geometry.Spatial.Classes;
using DiGi.Geometry.Spatial.Interfaces;

namespace DiGi.Geometry.xUnit
{
    public partial class Classes
    {
        /// <summary>
        /// Tests the functionality of the planar intersection result calculation, verifying that intersections are correctly identified across various combinations of input flags and point positions relative to a plane.
        /// </summary>
        [Fact]
        public void PlanarIntersectionResult()
        {
            Plane plane = Spatial.Constants.Plane.WorldZ;

            PlanarIntersectionResult? planarIntersectionResult;
            Point3D point3D_1;
            Point3D point3D_2;

            point3D_1 = new Point3D(0, 0, 1);
            point3D_2 = new Point3D(0, 0, 10);

            planarIntersectionResult = Create.PlanarIntersectionResult(plane, point3D_1, point3D_2, true, true);
            Assert.NotNull(planarIntersectionResult);
            Assert.True(!planarIntersectionResult.Intersect);

            planarIntersectionResult = Create.PlanarIntersectionResult(plane, point3D_1, point3D_2, false, true);
            Assert.NotNull(planarIntersectionResult);
            Assert.True(planarIntersectionResult.Intersect);

            planarIntersectionResult = Create.PlanarIntersectionResult(plane, point3D_1, point3D_2, true, false);
            Assert.NotNull(planarIntersectionResult);
            Assert.True(!planarIntersectionResult.Intersect);

            planarIntersectionResult = Create.PlanarIntersectionResult(plane, point3D_1, point3D_2, false, false);
            Assert.NotNull(planarIntersectionResult);
            Assert.True(planarIntersectionResult.Intersect);

            point3D_1 = new Point3D(0, 1, 0);
            point3D_2 = new Point3D(0, 10, 0);

            planarIntersectionResult = Create.PlanarIntersectionResult(plane, point3D_1, point3D_2, true, true);
            Assert.NotNull(planarIntersectionResult);
            if (planarIntersectionResult is not null)
            {
                Assert.True(planarIntersectionResult.Intersect);

                List<Segment3D>? segment3Ds = planarIntersectionResult.GetGeometry3Ds<Segment3D>();
                Assert.NotNull(segment3Ds);
                Assert.Single(segment3Ds);
                if (segment3Ds is not null && segment3Ds.Count > 0)
                {
                    ILinear3D linear3D = segment3Ds[0];

                    Assert.True(linear3D.On(point3D_1));
                    Assert.True(linear3D.On(point3D_2));
                }
            }

            planarIntersectionResult = Create.PlanarIntersectionResult(plane, point3D_1, point3D_2, false, true);
            Assert.NotNull(planarIntersectionResult);
            if (planarIntersectionResult is not null)
            {
                Assert.True(planarIntersectionResult.Intersect);

                List<Ray3D>? ray3Ds = planarIntersectionResult.GetGeometry3Ds<Ray3D>();
                Assert.NotNull(ray3Ds);
                Assert.Single(ray3Ds);
                if (ray3Ds is not null && ray3Ds.Count > 0)
                {
                    ILinear3D linear3D = ray3Ds[0];

                    Assert.True(linear3D.On(point3D_1));
                    Assert.True(linear3D.On(point3D_2));
                }
            }

            planarIntersectionResult = Create.PlanarIntersectionResult(plane, point3D_1, point3D_2, true, false);
            Assert.NotNull(planarIntersectionResult);
            if (planarIntersectionResult is not null)
            {
                Assert.True(planarIntersectionResult.Intersect);

                List<Ray3D>? ray3Ds = planarIntersectionResult.GetGeometry3Ds<Ray3D>();
                Assert.NotNull(ray3Ds);
                Assert.Single(ray3Ds);
                if (ray3Ds is not null && ray3Ds.Count > 0)
                {
                    ILinear3D linear3D = ray3Ds[0];

                    Assert.True(linear3D.On(point3D_1));
                    Assert.True(linear3D.On(point3D_2));
                }
            }

            planarIntersectionResult = Create.PlanarIntersectionResult(plane, point3D_1, point3D_2, false, false);
            Assert.NotNull(planarIntersectionResult);
            if (planarIntersectionResult is not null)
            {
                Assert.True(planarIntersectionResult.Intersect);

                List<Line3D>? line3Ds = planarIntersectionResult.GetGeometry3Ds<Line3D>();
                Assert.NotNull(line3Ds);
                Assert.Single(line3Ds);
                if (line3Ds is not null && line3Ds.Count > 0)
                {
                    ILinear3D linear3D = line3Ds[0];

                    Assert.True(linear3D.On(point3D_1));
                    Assert.True(linear3D.On(point3D_2));
                }
            }
        }
    }
}
