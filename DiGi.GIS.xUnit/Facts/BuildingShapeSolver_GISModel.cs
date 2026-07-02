using DiGi.Geometry.Planar.Classes;
using DiGi.Geometry.Planar.Interfaces;
using DiGi.GIS.Classes;
using DiGi.GIS.Enums;
using System.Collections.Generic;
using System.Reflection;

namespace DiGi.GIS.xUnit
{
    public partial class Facts
    {
        /// <summary>Runs the solver against every building in the sample 2476_GML.gmf GIS model and verifies that each footprint with a positive-area external edge resolves to a defined shape, that classification is deterministic across repeated runs, and that simple rectangular and square footprints form the majority.</summary>
        [Fact]
        public void BuildingShapeSolver_GISModel()
        {
            string? path = Core.xUnit.Query.FilePath(Assembly.GetExecutingAssembly(), "2476_GML.gmf");
            Assert.False(string.IsNullOrWhiteSpace(path));
            Assert.True(System.IO.File.Exists(path));

            using GISModelFile gISModelFile = new(path);
            Assert.True(gISModelFile.Open());

            List<Building2D>? building2Ds = gISModelFile.Value?.GetObjects<Building2D>();
            Assert.NotNull(building2Ds);
            Assert.NotEmpty(building2Ds);

            double microTolerance = Core.Constants.Tolerance.Distance;

            int solved = 0;
            int rectangularOrSquare = 0;

            foreach (Building2D building2D in building2Ds)
            {
                PolygonalFace2D? polygonalFace2D = building2D.PolygonalFace2D;
                IPolygonal2D? externalEdge = polygonalFace2D?.ExternalEdge;
                if (externalEdge is null || externalEdge.GetArea() <= microTolerance)
                {
                    continue;
                }

                BuildingShapeSolver buildingShapeSolver_First = new()
                {
                    Input = building2D
                };
                Assert.True(buildingShapeSolver_First.Solve());

                BuildingShape shape = buildingShapeSolver_First.Output;

                // A footprint with a valid external edge must always resolve to a defined shape.
                Assert.NotEqual(BuildingShape.Undefined, shape);

                // Classification must be deterministic across repeated runs on the same input.
                BuildingShapeSolver buildingShapeSolver_Second = new()
                {
                    Input = building2D
                };
                Assert.True(buildingShapeSolver_Second.Solve());
                Assert.Equal(shape, buildingShapeSolver_Second.Output);

                solved++;
                if (shape == BuildingShape.Rectangular || shape == BuildingShape.Square)
                {
                    rectangularOrSquare++;
                }
            }

            Assert.True(solved > 0);

            // Real building footprints are dominated by simple rectangular and square forms; a healthy
            // classifier keeps those as the clear majority rather than over-segmenting into complex shapes.
            Assert.True(rectangularOrSquare > solved / 2);
        }
    }
}