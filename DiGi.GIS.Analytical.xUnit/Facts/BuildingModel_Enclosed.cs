using DiGi.Analytical.Building.Classes;
using DiGi.Analytical.Classes;
using DiGi.CityGML.Classes;
using DiGi.Geometry.Spatial;
using DiGi.Geometry.Spatial.Classes;
using DiGi.GIS.Classes;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace DiGi.GIS.Analytical.xUnit
{
    public partial class Facts
    {
        /// <summary>
        /// Tests the BuildingModel extension method taking a Building and matching Building2D, evaluating whether extracted shells are enclosed across tolerances between 1e-6 to 0.05 (and up to 0.5), and logging a detailed report of the results.
        /// </summary>
        [Fact]
        public void BuildingModel_BuildingAndBuilding2D_ShellsAreEnclosed()
        {
            Assembly assembly = Assembly.GetExecutingAssembly();
            string? directory_Files = Core.xUnit.Query.FilesDirectory(assembly);

            Assert.False(string.IsNullOrWhiteSpace(directory_Files));

            string path_BuildingsJson = Path.Combine(directory_Files!, "Buildings.json");
            string path_Building2DsJson = Path.Combine(directory_Files!, "Building2Ds.json");

            Assert.True(File.Exists(path_BuildingsJson));
            Assert.True(File.Exists(path_Building2DsJson));

            string json_Buildings = File.ReadAllText(path_BuildingsJson);
            string json_Building2Ds = File.ReadAllText(path_Building2DsJson);

            List<Building>? buildings = Core.Convert.ToDiGi<Building>(json_Buildings);
            List<Building2D>? building2Ds = Core.Convert.ToDiGi<Building2D>(json_Building2Ds);

            Assert.NotNull(buildings);
            Assert.NotEmpty(buildings);
            Assert.NotNull(building2Ds);
            Assert.NotEmpty(building2Ds);

            Dictionary<string, Building2D> building2D_ByReference = [];
            foreach (Building2D building2D in building2Ds)
            {
                if (!string.IsNullOrWhiteSpace(building2D.Reference))
                {
                    building2D_ByReference[building2D.Reference] = building2D;
                }
            }

            double[] testTolerances = [1e-6, 1e-5, 1e-4, 1e-3, 0.01, 0.05, 0.1, 0.2, 0.5];
            int totalPairsEvaluated = 0;
            int totalShellsEvaluated = 0;
            int closedWithin05Count = 0;

            List<string> reportLines = [];

            foreach (Building building in buildings)
            {
                string? buildingId = building.GetValue<string>(CityGML.Enums.BuildingParameter.buildingId);

                if (string.IsNullOrWhiteSpace(buildingId))
                {
                    string? uniqueId = building.UniqueId;
                    if (!string.IsNullOrWhiteSpace(uniqueId) && uniqueId.StartsWith("ID-"))
                    {
                        string[] parts = uniqueId.Split('-');
                        if (parts.Length >= 3)
                        {
                            buildingId = string.Join("-", parts.Skip(2));
                        }
                    }
                }

                if (string.IsNullOrWhiteSpace(buildingId) || !building2D_ByReference.TryGetValue(buildingId, out Building2D? building2D))
                {
                    reportLines.Add(string.Format("Building UniqueId={0} | Reference={1} | Missing matching Building2D", building.UniqueId, buildingId ?? "None"));
                    continue;
                }

                totalPairsEvaluated++;

                BuildingModel? buildingModel = Create.BuildingModel(building, building2D);

                if (buildingModel is null)
                {
                    reportLines.Add(string.Format("Building {0} | Reference {1} | BuildingModel creation returned null", building.UniqueId, buildingId));
                    continue;
                }

                List<Shell>? shells = buildingModel.GetShells<Space>();

                if (shells is null || shells.Count == 0)
                {
                    reportLines.Add(string.Format("Building {0} | Reference {1} | GetShells returned no shells", building.UniqueId, buildingId));
                    continue;
                }

                for (int i = 0; i < shells.Count; i++)
                {
                    Shell shell = shells[i];
                    totalShellsEvaluated++;

                    bool isClosed = false;
                    double minTol = -1;

                    foreach (double tol in testTolerances)
                    {
                        if (shell.IsClosed(tol))
                        {
                            isClosed = true;
                            minTol = tol;
                            break;
                        }
                    }

                    if (isClosed && minTol <= 0.05)
                    {
                        closedWithin05Count++;
                        reportLines.Add(string.Format("Building {0} | Reference {1} | Shell {2} | Enclosed: YES | Min Tolerance: {3:E2}", building.UniqueId, buildingId, i, minTol));
                    }
                    else if (isClosed)
                    {
                        reportLines.Add(string.Format("Building {0} | Reference {1} | Shell {2} | Enclosed: YES (at >0.05) | Min Tolerance: {3:E2}", building.UniqueId, buildingId, i, minTol));
                    }
                    else
                    {
                        reportLines.Add(string.Format("Building {0} | Reference {1} | Shell {2} | Enclosed: NO (Open up to 0.5)", building.UniqueId, buildingId, i));
                    }
                }
            }

            Assert.True(totalPairsEvaluated > 0, "At least one Building and Building2D pair must be evaluated.");

            string path_Report = Path.Combine(directory_Files!, "BuildingModel_Enclosed_Report.txt");
            File.WriteAllLines(path_Report, reportLines);
        }
    }
}
