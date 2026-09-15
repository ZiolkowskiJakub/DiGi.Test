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
        /// <para>#24: each bucket node carries the count of its reference set - the solver files a reference on the bucket that
        /// matched it at every level, never on the root - so the root counts the sum of its children, and a reference a bucket
        /// holds but none of its children does (a row dropped at the next level) stays in its count while absent from the flat
        /// building list.</para>
        /// <para>#26: each building entry carries the database identifier from the reference-to-id map, and 0 when the map lacks
        /// the reference or is absent.</para>
        /// </summary>
        [Fact]
        public void Create_TypologyBuildingsViewModel()
        {
            Core.Classes.Color color_A = new(System.Drawing.Color.FromArgb(0, 0, 255));
            Core.Classes.Color color_B = new(System.Drawing.Color.FromArgb(255, 0, 0));

            VisualTypology child_A = new(new VisualTypologyItem([0], "Storeys 1-2", "Low", color_A.TypologyAppearance()));
            child_A.AddReference("A1");
            child_A.AddReference("A2");
            child_A.AddReference("X");

            // A second level under A: "X" matched A but no bucket below it, as the solver leaves a row dropped at a lower level.
            VisualTypology grandChild_A1 = new(new VisualTypologyItem([0, 0], "Function A", null, color_A.TypologyAppearance()));
            grandChild_A1.AddReference("A1");
            VisualTypology grandChild_A2 = new(new VisualTypologyItem([0, 1], "Function B", null, color_B.TypologyAppearance()));
            grandChild_A2.AddReference("A2");
            DiGi.Typology.Modify.AddSubTypologies(child_A, [grandChild_A1, grandChild_A2]);

            VisualTypology child_B = new(new VisualTypologyItem([1], "Storeys 3-5", "High", color_B.TypologyAppearance()));
            child_B.AddReference("B1");

            // The root stores no references, as the solver leaves it.
            VisualTypology root = new((string?)null, (string?)null);
            DiGi.Typology.Modify.AddSubTypologies(root, [child_A, child_B]);

            Dictionary<string, int> countyId_ByReference = new()
            {
                ["A1"] = 101,
                ["A2"] = 102,
                ["B1"] = 103
            };

            Dictionary<string, long> id_ByReference = new()
            {
                ["A1"] = 9157,
                ["B1"] = 42
            };

            ViewModels.TypologyBuildingsViewModel? viewModel = root.TypologyBuildingsViewModel(countyId_ByReference, id_ByReference);
            Assert.NotNull(viewModel);

            ViewModels.TypologyTreeNodeViewModel? rootNode = viewModel.Root;
            Assert.NotNull(rootNode);
            Assert.Null(rootNode.Name);
            Assert.Empty(rootNode.Path);
            Assert.Equal(4, rootNode.Count);
            Assert.Equal(2, rootNode.Children!.Count);

            ViewModels.TypologyTreeNodeViewModel node_A = rootNode.Children.First(c => c.Name == "Storeys 1-2");
            Assert.Equal("Low", node_A.Description);
            Assert.Equal("#0000ff", node_A.Color);
            Assert.Equal([0], node_A.Path);
            Assert.Equal(3, node_A.Count);
            Assert.NotNull(node_A.Children);
            Assert.Equal(2, node_A.Children.Count);
            Assert.Equal(1, node_A.Children[0].Count);
            Assert.Equal(1, node_A.Children[1].Count);
            Assert.Equal([0, 1], node_A.Children[1].Path);

            ViewModels.TypologyTreeNodeViewModel node_B = rootNode.Children.First(c => c.Name == "Storeys 3-5");
            Assert.Equal("High", node_B.Description);
            Assert.Equal("#ff0000", node_B.Color);
            Assert.Equal([1], node_B.Path);
            Assert.Equal(1, node_B.Count);
            Assert.Null(node_B.Children);

            // The remainder A counts but no leaf files is not a building entry: the pie shows it as a neutral slice.
            Assert.Equal(3, viewModel.Buildings.Count);
            Assert.DoesNotContain(viewModel.Buildings, b => b.Reference == "X");
            List<string> references = viewModel.Buildings.Select(b => b.Reference).ToList();
            Assert.Contains("A1", references);
            Assert.Contains("A2", references);
            Assert.Contains("B1", references);

            ViewModels.TypologyBuildingViewModel building_A1 = viewModel.Buildings.First(b => b.Reference == "A1");
            Assert.Equal(101, building_A1.CountyId);
            Assert.Equal(9157, building_A1.Id);
            Assert.Equal([0, 0], building_A1.Path);

            // A reference absent from the id map falls back to Id 0.
            ViewModels.TypologyBuildingViewModel building_A2 = viewModel.Buildings.First(b => b.Reference == "A2");
            Assert.Equal(0, building_A2.Id);

            ViewModels.TypologyBuildingViewModel building_B1 = viewModel.Buildings.First(b => b.Reference == "B1");
            Assert.Equal(103, building_B1.CountyId);
            Assert.Equal(42, building_B1.Id);
            Assert.Equal([1], building_B1.Path);

            // A reference absent from the map falls back to CountyId 0.
            ViewModels.TypologyBuildingsViewModel? viewModel_NoMap = root.TypologyBuildingsViewModel();
            Assert.NotNull(viewModel_NoMap);
            Assert.All(viewModel_NoMap!.Buildings, b => Assert.Equal(0, b.CountyId));
            Assert.All(viewModel_NoMap.Buildings, b => Assert.Equal(0, b.Id));
        }
    }
}
