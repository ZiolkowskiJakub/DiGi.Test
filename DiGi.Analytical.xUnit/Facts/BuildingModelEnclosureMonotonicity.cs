using DiGi.Analytical.Building;
using DiGi.Analytical.Building.Classes;
using DiGi.Analytical.Classes;
using DiGi.Geometry.Spatial;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace DiGi.Analytical.xUnit
{
    public partial class Facts
    {
        /// <summary>
        /// Verifies that the building reported on DiGi.Geometry issue 1 is enclosed at every tolerance, not only at the fine ones.
        /// <para>The fixture is the stored model of building 2FE7DA6C-EA8A-B139-E053-CC2BA8C0A463 of county 18536, twelve faces around a single space, taken from the deployed database. It carries a genuine 5 cm feature, and the vertex-welding closure query that shipped previously reported it closed from 1E-06 to 0.04, open at 0.05, and closed again at 0.1 - welding at a tolerance equal to a real feature collapsed some instances of that feature and not others, and the edge counts stopped pairing.</para>
        /// <para>Nothing is welded now, so the shell must report closed across the whole ladder with no dip, and <see cref="DiGi.Analytical.Building.Query.IsEnclosed(BuildingModel?, bool, double)"/> must agree at the coarse end where it previously needed its retry.</para>
        /// </summary>
        [Fact]
        public void BuildingModel_EnclosureMonotonicity()
        {
            string? path = DiGi.Core.xUnit.Query.FilePath(Assembly.GetExecutingAssembly(), "BuildingModel_NonMonotonicClosure.json");
            Assert.False(string.IsNullOrWhiteSpace(path));
            Assert.True(File.Exists(path));

            List<BuildingModel>? buildingModels = DiGi.Core.Convert.ToDiGi<BuildingModel>((DiGi.Core.Classes.Path)path!);
            Assert.NotNull(buildingModels);
            Assert.Single(buildingModels);

            BuildingModel buildingModel = buildingModels[0];

            List<Space>? spaces = buildingModel.GetSpaces<Space>();
            Assert.NotNull(spaces);
            Assert.Single(spaces);

            List<Shell>? shells = buildingModel.GetShells(spaces, tolerance: DiGi.Core.Constants.Tolerance.MacroDistance);
            Assert.NotNull(shells);
            Assert.Single(shells);

            Shell shell = shells[0];

            double[] tolerances = [1E-06, 1E-05, 0.0001, 0.001, 0.005, 0.01, 0.02, 0.03, 0.04, 0.05, 0.06, 0.1, 0.2];

            bool closed = false;
            for (int i = 0; i < tolerances.Length; i++)
            {
                bool closed_Temp = shell.IsClosed(tolerances[i]);

                // The value the report measured as open sits in the middle of this ladder.
                Assert.True(closed_Temp, $"The reported building is open at tolerance {tolerances[i]}.");
                Assert.True(!closed || closed_Temp, $"Closure went from closed back to open at tolerance {tolerances[i]}.");

                closed = closed_Temp;
            }

            // The tightest candidate that closes it, which is the finest one offered.
            Assert.Equal(1E-06, shell.ClosingTolerance(tolerances));

            Assert.True(buildingModel.IsEnclosed(DiGi.Core.Constants.Tolerance.MacroDistance));
            Assert.True(buildingModel.IsEnclosed(0.05));
        }
    }
}
