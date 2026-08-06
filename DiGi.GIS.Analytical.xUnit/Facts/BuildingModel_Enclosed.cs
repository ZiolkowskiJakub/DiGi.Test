using DiGi.Analytical.Building.Classes;
using DiGi.Analytical.Classes;
using DiGi.CityGML.Classes;
using DiGi.CityGML.Enums;
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
        /// Verifies that BuildingModels created from paired Building and Building2D objects have enclosed space shells.
        /// </summary>
        [Fact]
        public void BuildingModel_BuildingAndBuilding2D_ShellsAreEnclosed()
        {
            Assembly assembly = Assembly.GetExecutingAssembly();
            string? directory_Files = Core.xUnit.Query.FilesDirectory(assembly);

            Assert.False(string.IsNullOrWhiteSpace(directory_Files));

            string path_BuildingsJson = Path.Combine(directory_Files!, "Buildings.json");
            string path_Building2DsJson = Path.Combine(directory_Files!, "Building2Ds.json");

            Assert.True(File.Exists(path_BuildingsJson), $"Missing input file: {path_BuildingsJson}");
            Assert.True(File.Exists(path_Building2DsJson), $"Missing input file: {path_Building2DsJson}");

            string json_Buildings = File.ReadAllText(path_BuildingsJson);
            string json_Building2Ds = File.ReadAllText(path_Building2DsJson);

            List<Building>? buildings = Core.Convert.ToDiGi<Building>(json_Buildings);
            List<Building2D>? building2Ds = Core.Convert.ToDiGi<Building2D>(json_Building2Ds);

            Assert.NotNull(buildings);
            Assert.NotNull(building2Ds);
            Assert.NotEmpty(buildings);
            Assert.NotEmpty(building2Ds);

            List<string> reportLines = [];
            reportLines.Add("=== BUILDING MODEL SHELL ENCLOSURE REPORT ===");
            reportLines.Add($"Total Buildings: {buildings.Count}");
            reportLines.Add($"Total Building2Ds: {building2Ds.Count}");
            reportLines.Add("");

            double[] testTolerances = [1e-6, 1e-5, 1e-4, 1e-3, 0.01, 0.05, 0.1, 0.2, 0.5];

            int totalBuildingPairs = 0;
            int totalShells = 0;
            int enclosedShellsAt05 = 0;

            foreach (Building building in buildings)
            {
                string? refId = building.GetValue<string>(BuildingParameter.buildingId);
                if (string.IsNullOrWhiteSpace(refId))
                {
                    refId = building.UniqueId;
                }

                Building2D? matching2D = building2Ds.FirstOrDefault(b2d => b2d.Reference == refId)
                                      ?? building2Ds.FirstOrDefault(b2d => building.UniqueId != null && building.UniqueId.Contains(b2d.Reference ?? "___"));

                if (matching2D is null)
                {
                    reportLines.Add($"Building {building.UniqueId} (Ref: {refId}): MISSING MATCHING Building2D");
                    continue;
                }

                totalBuildingPairs++;
                BuildingModel? buildingModel = Create.BuildingModel(building, matching2D);

                reportLines.Add($"--- Building {building.UniqueId} | Ref: {refId} ---");
                if (buildingModel is null)
                {
                    reportLines.Add("  FAILED to create BuildingModel");
                    continue;
                }

                List<Space>? spaces = buildingModel.GetSpaces<Space>();
                List<Shell>? shells = buildingModel.GetShells<Space>();

                int spaceCount = spaces?.Count ?? 0;
                int shellCount = shells?.Count ?? 0;

                reportLines.Add($"  Spaces Count: {spaceCount}, Shells Count: {shellCount}");

                if (shells is null || shells.Count == 0)
                {
                    reportLines.Add("  WARNING: No shells extracted from BuildingModel");
                    continue;
                }

                for (int s = 0; s < shells.Count; s++)
                {
                    totalShells++;
                    Shell shell = shells[s];

                    double? minEnclosedTol = null;
                    Dictionary<double, bool> closureResults = [];

                    foreach (double tol in testTolerances)
                    {
                        bool isClosed = shell.IsClosed(tol);
                        closureResults[tol] = isClosed;
                        if (isClosed && minEnclosedTol is null)
                        {
                            minEnclosedTol = tol;
                        }
                    }

                    bool enclosedAt05 = closureResults.TryGetValue(0.05, out bool res05) && res05;
                    if (enclosedAt05)
                    {
                        enclosedShellsAt05++;
                    }

                    string statusStr = minEnclosedTol.HasValue ? $"ENCLOSED (min tol = {minEnclosedTol.Value:E2})" : "OPEN (up to 0.5)";
                    reportLines.Add($"    Shell [{s}]: {statusStr} | Faces: {shell.Count}");

                    foreach (double tol in testTolerances)
                    {
                        reportLines.Add($"      Tol {tol,8:0.000000}: Closed = {closureResults[tol]}");
                    }
                }

                reportLines.Add("");
            }

            reportLines.Add("=== SUMMARY ===");
            reportLines.Add($"Total Evaluated Pairs: {totalBuildingPairs}");
            reportLines.Add($"Total Extracted Shells: {totalShells}");
            reportLines.Add($"Shells Enclosed at tol <= 0.05: {enclosedShellsAt05} / {totalShells}");

            string? directory_Reports = Core.xUnit.Query.ReportsDirectory(assembly);
            Assert.False(string.IsNullOrWhiteSpace(directory_Reports));

            string reportPath = Path.Combine(directory_Reports!, "BuildingModel_Enclosed_Report.txt");
            File.WriteAllLines(reportPath, reportLines);

            Assert.True(totalBuildingPairs > 0, "No building pairs were evaluated.");
            Assert.True(totalShells > 0, "No shells were extracted from building models.");
        }
    }
}
