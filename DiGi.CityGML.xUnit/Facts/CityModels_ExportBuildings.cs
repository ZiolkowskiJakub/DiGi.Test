using DiGi.CityGML.Classes;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace DiGi.CityGML.xUnit
{
    public partial class Facts
    {
        /// <summary>
        /// Reads specified CityGML zip and gml test fixtures, converts them to CityGML buildings, and exports the resulting collection to Buildings.json in the shared files directory.
        /// </summary>
        [Fact]
        public void CityModels_ExportBuildingsToJson()
        {
            List<string> fileNames =
            [
                "2476_CityGML.zip",
                "2862_CityGML.zip",
                "2862_N-34-77-D-b-1-1.gml",
                "0201_M-33-19-B-d-3-2.gml"
            ];

            List<Building> buildings = [];
            Assembly assembly = Assembly.GetExecutingAssembly();

            foreach (string fileName in fileNames)
            {
                string? path = Core.xUnit.Query.FilePath(assembly, fileName);

                Assert.False(string.IsNullOrWhiteSpace(path));
                Assert.True(File.Exists(path));

                List<CityModel>? cityModels = Create.CityModels(path);
                if (cityModels is null)
                {
                    continue;
                }

                foreach (CityModel cityModel in cityModels)
                {
                    if (cityModel?.Buildings is null)
                    {
                        continue;
                    }

                    foreach (Building building in cityModel.Buildings)
                    {
                        if (building?.UniqueId != null && !buildings.Any(b => b.UniqueId == building.UniqueId))
                        {
                            buildings.Add(building);
                        }
                    }
                }
            }

            Assert.NotEmpty(buildings);

            string? json = Core.Convert.ToSystem_String(buildings);

            Assert.False(string.IsNullOrWhiteSpace(json));

            string? directory_Files = Core.xUnit.Query.FilesDirectory(assembly);

            Assert.False(string.IsNullOrWhiteSpace(directory_Files));

            string path_Output = Path.Combine(directory_Files!, "Buildings.json");

            File.WriteAllText(path_Output, json);

            Assert.True(File.Exists(path_Output));
        }
    }
}
