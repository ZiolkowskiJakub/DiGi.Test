using DiGi.Core.Classes;
using DiGi.Geometry.Planar.Classes;
using DiGi.Geometry.Spatial.Classes;
using DiGi.Solar.Classes;

namespace DiGi.Solar.xUnit
{
    public partial class Facts
    {
        /// <summary>
        /// Tests that a receiver with nothing in front of it sees its whole sky and ground: a lone wall and a lone roof of one box corner get visibilities of exactly 1, so open surfaces keep the open-sky irradiance bit for bit.
        /// <para>The roof lies behind the wall's plane and the wall below the roof's plane, so neither blocks the other.</para>
        /// </summary>
        [Fact]
        public void ViewFactorResults_Open()
        {
            ShadingModel shadingModel = new(Core.Enums.UTC.Plus0100, new Coordinates(52.0, 21.0));
            Assert.True(shadingModel.Update(ViewFactorFixture_Face("Wall", false, new Point3D(0, 0, 0), new Point3D(10, 0, 0), new Point3D(10, 0, 10), new Point3D(0, 0, 10))));
            Assert.True(shadingModel.Update(ViewFactorFixture_Face("Roof", false, new Point3D(0, 0, 10), new Point3D(10, 0, 10), new Point3D(10, 10, 10), new Point3D(0, 10, 10))));

            Dictionary<string, Vector3D> normals = new() { ["Wall"] = new Vector3D(0, -1, 0), ["Roof"] = new Vector3D(0, 0, 1) };

            List<ViewFactorResult>? viewFactorResults = shadingModel.ViewFactorResults(normals, Core.Constants.Tolerance.Distance);
            Assert.NotNull(viewFactorResults);
            Assert.Equal(2, viewFactorResults.Count);

            foreach (ViewFactorResult viewFactorResult in viewFactorResults)
            {
                Assert.Equal(1.0, viewFactorResult.SkyVisibility);
                Assert.Equal(1.0, viewFactorResult.GroundVisibility);
            }
        }

        /// <summary>
        /// Tests that a wall fully covered by a touching neighbour (a party wall) sees neither sky nor ground (ZiolkowskiJakub/DiGi.GIS.WebAPI.UI#61).
        /// <para>The neighbour is a closed shading-only 10 m box in front of the wall. The box's face on the wall's plane casts nothing, but every ray reaching the wall passes through the rest of the box: the roof for the sky, the floor slab for the ground.</para>
        /// <para>The same wall solved with its stored normal pointing away from the neighbour (no outward normal supplied) looks into the empty half-space and is fully open, which is why the outward normal is an input.</para>
        /// </summary>
        [Fact]
        public void ViewFactorResults_PartyWall()
        {
            ShadingModel shadingModel = new(Core.Enums.UTC.Plus0100, new Coordinates(52.0, 21.0));
            Assert.True(shadingModel.Update(ViewFactorFixture_Face("Wall", false, new Point3D(0, 0, 0), new Point3D(0, 0, 10), new Point3D(10, 0, 10), new Point3D(10, 0, 0))));
            ViewFactorFixture_Box(shadingModel, new Point3D(0, -10, 0), new Point3D(10, 0, 10));

            List<ViewFactorResult>? viewFactorResults = shadingModel.ViewFactorResults(new Dictionary<string, Vector3D>() { ["Wall"] = new Vector3D(0, -1, 0) }, Core.Constants.Tolerance.Distance);
            Assert.NotNull(viewFactorResults);
            ViewFactorResult viewFactorResult = Assert.Single(viewFactorResults);
            Assert.True(viewFactorResult.SkyVisibility <= 0.01, $"sky {viewFactorResult.SkyVisibility}");
            Assert.True(viewFactorResult.GroundVisibility <= 0.01, $"ground {viewFactorResult.GroundVisibility}");

            // The stored normal of the wall points to +Y, away from the neighbour.
            Assert.Equal(1.0, shadingModel.GetShadingElements<ShadingElement>(false)![0].PolygonalFace3D!.Plane!.Normal!.Y, 9);
            List<ViewFactorResult>? viewFactorResults_Stored = shadingModel.ViewFactorResults(null, Core.Constants.Tolerance.Distance);
            Assert.NotNull(viewFactorResults_Stored);
            ViewFactorResult viewFactorResult_Stored = Assert.Single(viewFactorResults_Stored);
            Assert.Equal(1.0, viewFactorResult_Stored.SkyVisibility);
            Assert.Equal(1.0, viewFactorResult_Stored.GroundVisibility);
        }

        /// <summary>
        /// Tests the sky visibility against the analytic view factor of a horizontal surface beside an infinitely long screen: a screen rising to elevation β leaves the surface <c>(1 + cos β) / 2</c> of its sky (crossed strings).
        /// <para>A 1 x 1 m horizontal receiver faces a 2 km long shading-only screen 20 m away, 20 m high (β = 45°, 0.853553). The receiver spans 19.5 to 20.5 m from the screen, which moves the area-averaged exact value by less than 1e-4.
        /// The patch discretisation is the error: the default 6 x 24 patches agree within 0.01, and 12 x 48 patches halve the error at least, so the result converges on the analytic value rather than merely landing near it.</para>
        /// </summary>
        [Fact]
        public void ViewFactorResults_Screen()
        {
            ShadingModel shadingModel = new(Core.Enums.UTC.Plus0100, new Coordinates(52.0, 21.0));
            Assert.True(shadingModel.Update(ViewFactorFixture_Face("Receiver", false, new Point3D(-0.5, -0.5, 0), new Point3D(0.5, -0.5, 0), new Point3D(0.5, 0.5, 0), new Point3D(-0.5, 0.5, 0))));
            Assert.True(shadingModel.Update(ViewFactorFixture_Face("Screen", true, new Point3D(20, -1000, 0), new Point3D(20, 1000, 0), new Point3D(20, 1000, 20), new Point3D(20, -1000, 20))));

            Dictionary<string, Vector3D> normals = new() { ["Receiver"] = new Vector3D(0, 0, 1) };
            double expected = (1 + Math.Cos(Math.PI / 4)) / 2;

            List<ViewFactorResult>? viewFactorResults = shadingModel.ViewFactorResults(normals, Core.Constants.Tolerance.Distance);
            Assert.NotNull(viewFactorResults);
            ViewFactorResult viewFactorResult = Assert.Single(viewFactorResults);

            List<ViewFactorResult>? viewFactorResults_Fine = shadingModel.ViewFactorResults(normals, Core.Constants.Tolerance.Distance, 12, 48);
            Assert.NotNull(viewFactorResults_Fine);
            ViewFactorResult viewFactorResult_Fine = Assert.Single(viewFactorResults_Fine);

            double error = Math.Abs(viewFactorResult.SkyVisibility - expected);
            double error_Fine = Math.Abs(viewFactorResult_Fine.SkyVisibility - expected);
            testOutputHelper.WriteLine($"expected {expected:F6}, 6x24 {viewFactorResult.SkyVisibility:F6} (error {error:E2}), 12x48 {viewFactorResult_Fine.SkyVisibility:F6} (error {error_Fine:E2})");

            Assert.True(error < 0.01, $"6x24 error {error}");
            Assert.True(error_Fine <= error / 2, $"12x48 error {error_Fine} against 6x24 {error}");

            // A horizontal receiver has no ground in front of it.
            Assert.Equal(1.0, viewFactorResult.GroundVisibility);
        }

        /// <summary>
        /// Tests the serialization round-trip and cloning of ViewFactorResult.
        /// </summary>
        [Fact]
        public void ViewFactorResult_Serialization()
        {
            ViewFactorResult viewFactorResult = new("Wall", 0.25, 0.75);

            Assert.Equal("Wall", viewFactorResult.Reference);
            Assert.Equal(0.25, viewFactorResult.SkyVisibility);
            Assert.Equal(0.75, viewFactorResult.GroundVisibility);

            string? json = Core.Convert.ToSystem_String(viewFactorResult);
            Assert.NotNull(json);
            ViewFactorResult? viewFactorResult_Json = Core.Convert.ToDiGi<ViewFactorResult>(json)?.FirstOrDefault();
            Assert.NotNull(viewFactorResult_Json);
            Assert.Equal("Wall", viewFactorResult_Json.Reference);
            Assert.Equal(0.25, viewFactorResult_Json.SkyVisibility);
            Assert.Equal(0.75, viewFactorResult_Json.GroundVisibility);

            Core.xUnit.Query.SerializationCheck(viewFactorResult);
            Core.xUnit.Query.SerializationCheck(new ViewFactorResult(null, 1, 1));
        }

        /// <summary>
        /// Builds a planar quadrilateral shading element whose stored normal follows the right-hand rule of the corner order.
        /// </summary>
        /// <param name="reference">The reference of the element.</param>
        /// <param name="shadingOnly">True for a shading-only caster; false for a receiver.</param>
        /// <param name="point3D_1">The first corner.</param>
        /// <param name="point3D_2">The second corner.</param>
        /// <param name="point3D_3">The third corner.</param>
        /// <param name="point3D_4">The fourth corner.</param>
        /// <returns>The shading element.</returns>
        private static ShadingElement ViewFactorFixture_Face(string reference, bool shadingOnly, Point3D point3D_1, Point3D point3D_2, Point3D point3D_3, Point3D point3D_4)
        {
            Vector3D? normal = Geometry.Spatial.Query.Normal(point3D_1, point3D_2, point3D_3);
            Assert.NotNull(normal);

            Plane plane = new(point3D_1, normal);

            List<Point2D> point2Ds = [];
            foreach (Point3D point3D in new Point3D[] { point3D_1, point3D_2, point3D_3, point3D_4 })
            {
                Point2D? point2D = Geometry.Spatial.Query.Convert(plane, point3D);
                Assert.NotNull(point2D);
                point2Ds.Add(point2D);
            }

            PolygonalFace3D? polygonalFace3D = Geometry.Spatial.Create.PolygonalFace3D(new Polygon3D(plane, point2Ds), []);
            Assert.NotNull(polygonalFace3D);

            return new ShadingElement(reference, polygonalFace3D, shadingOnly);
        }

        /// <summary>
        /// Adds the six faces of an axis-aligned box to the shading model as shading-only casters.
        /// </summary>
        /// <param name="shadingModel">The shading model.</param>
        /// <param name="min">The minimum corner.</param>
        /// <param name="max">The maximum corner.</param>
        private static void ViewFactorFixture_Box(ShadingModel shadingModel, Point3D min, Point3D max)
        {
            double x_1 = min.X, y_1 = min.Y, z_1 = min.Z, x_2 = max.X, y_2 = max.Y, z_2 = max.Z;

            ShadingElement[] shadingElements =
            [
                ViewFactorFixture_Face("Floor", true, new Point3D(x_1, y_1, z_1), new Point3D(x_1, y_2, z_1), new Point3D(x_2, y_2, z_1), new Point3D(x_2, y_1, z_1)),
                ViewFactorFixture_Face("Roof", true, new Point3D(x_1, y_1, z_2), new Point3D(x_2, y_1, z_2), new Point3D(x_2, y_2, z_2), new Point3D(x_1, y_2, z_2)),
                ViewFactorFixture_Face("South", true, new Point3D(x_1, y_1, z_1), new Point3D(x_2, y_1, z_1), new Point3D(x_2, y_1, z_2), new Point3D(x_1, y_1, z_2)),
                ViewFactorFixture_Face("North", true, new Point3D(x_1, y_2, z_1), new Point3D(x_1, y_2, z_2), new Point3D(x_2, y_2, z_2), new Point3D(x_2, y_2, z_1)),
                ViewFactorFixture_Face("West", true, new Point3D(x_1, y_1, z_1), new Point3D(x_1, y_1, z_2), new Point3D(x_1, y_2, z_2), new Point3D(x_1, y_2, z_1)),
                ViewFactorFixture_Face("East", true, new Point3D(x_2, y_1, z_1), new Point3D(x_2, y_2, z_1), new Point3D(x_2, y_2, z_2), new Point3D(x_2, y_1, z_2)),
            ];

            foreach (ShadingElement shadingElement in shadingElements)
            {
                Assert.True(shadingModel.Update(shadingElement));
            }
        }
    }
}
