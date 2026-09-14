using DiGi.Typology.Visual.Classes;
using System.Collections.Generic;
using System.Linq;

namespace DiGi.GIS.WebAPI.UI.xUnit
{
    public partial class Facts
    {
        /// <summary>
        /// Tests the flatten of a solved <see cref="VisualTypology"/> into the view DTO (#22): the recursive tree carries each
        /// node's name, description, color (as a CSS hex string read from its appearance) and path, a leaf's references become
        /// the flat building entries with their node's path, and each building's CountyId comes from the reference-to-part map.
        /// </summary>
        [Fact]
        public void Create_TypologyBuildingsViewModel()
        {
            Core.Classes.Color color_A = new(System.Drawing.Color.FromArgb(0, 0, 255));
            Core.Classes.Color color_B = new(System.Drawing.Color.FromArgb(255, 0, 0));

            VisualTypology child_A = new(new VisualTypologyItem([0], "Storeys 1-2", "Low", color_A.TypologyAppearance()));
            child_A.AddReference("A1");
            child_A.AddReference("A2");

            VisualTypology child_B = new(new VisualTypologyItem([1], "Storeys 3-5", "High", color_B.TypologyAppearance()));
            child_B.AddReference("B1");

            VisualTypology root = new((string?)null, (string?)null);
            DiGi.Typology.Modify.AddSubTypologies(root, [child_A, child_B]);

            Dictionary<string, int> countyId_ByReference = new()
            {
                ["A1"] = 101,
                ["A2"] = 102,
                ["B1"] = 103
            };

            ViewModels.TypologyBuildingsViewModel? viewModel = root.TypologyBuildingsViewModel(countyId_ByReference);
            Assert.NotNull(viewModel);

            ViewModels.TypologyTreeNodeViewModel? rootNode = viewModel.Root;
            Assert.NotNull(rootNode);
            Assert.Null(rootNode.Name);
            Assert.Empty(rootNode.Path);
            Assert.Equal(2, rootNode.Children!.Count);

            ViewModels.TypologyTreeNodeViewModel node_A = rootNode.Children.First(c => c.Name == "Storeys 1-2");
            Assert.Equal("Low", node_A.Description);
            Assert.Equal("#0000ff", node_A.Color);
            Assert.Equal([0], node_A.Path);
            Assert.Null(node_A.Children);

            ViewModels.TypologyTreeNodeViewModel node_B = rootNode.Children.First(c => c.Name == "Storeys 3-5");
            Assert.Equal("High", node_B.Description);
            Assert.Equal("#ff0000", node_B.Color);
            Assert.Equal([1], node_B.Path);
            Assert.Null(node_B.Children);

            Assert.Equal(3, viewModel.Buildings.Count);
            List<string> references = viewModel.Buildings.Select(b => b.Reference).ToList();
            Assert.Contains("A1", references);
            Assert.Contains("A2", references);
            Assert.Contains("B1", references);

            ViewModels.TypologyBuildingViewModel building_A1 = viewModel.Buildings.First(b => b.Reference == "A1");
            Assert.Equal(101, building_A1.CountyId);
            Assert.Equal([0], building_A1.Path);

            ViewModels.TypologyBuildingViewModel building_B1 = viewModel.Buildings.First(b => b.Reference == "B1");
            Assert.Equal(103, building_B1.CountyId);
            Assert.Equal([1], building_B1.Path);

            // A reference absent from the map falls back to CountyId 0.
            ViewModels.TypologyBuildingsViewModel? viewModel_NoMap = root.TypologyBuildingsViewModel();
            Assert.NotNull(viewModel_NoMap);
            Assert.All(viewModel_NoMap!.Buildings, b => Assert.Equal(0, b.CountyId));
        }
    }
}
