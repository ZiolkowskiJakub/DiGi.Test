using System.Text.Json;

namespace DiGi.GIS.WebAPI.UI.xUnit
{
    public partial class Facts
    {
        /// <summary>
        /// Round-trips the <c>POST /typology/buildings</c> response DTO through System.Text.Json (#22): the recursive node
        /// tree survives with each node's name, color, path and count, a leaf's null children stay null, and the flat building list
        /// carries its reference, database identifier (#26), county part and path.
        /// </summary>
        [Fact]
        public void TypologyBuildingsViewModel_Serialization()
        {
            ViewModels.TypologyBuildingViewModel building = new("A1", 9157, 101, [0]);
            ViewModels.TypologyTreeNodeViewModel leaf = new("Storeys 1-2", "Low", "#0000ff", [0], 1, null);
            ViewModels.TypologyTreeNodeViewModel root = new(null, null, null, [], 2, [leaf]);
            ViewModels.TypologyBuildingsViewModel viewModel = new(root, [building]);

            string json = JsonSerializer.Serialize(viewModel);
            ViewModels.TypologyBuildingsViewModel? roundTrip = JsonSerializer.Deserialize<ViewModels.TypologyBuildingsViewModel>(json);
            Assert.NotNull(roundTrip);

            ViewModels.TypologyTreeNodeViewModel? rootNode = roundTrip.Root;
            Assert.NotNull(rootNode);
            Assert.Equal(2, rootNode!.Count);
            Assert.NotNull(rootNode.Children);
            Assert.Single(rootNode.Children!);

            ViewModels.TypologyTreeNodeViewModel roundTrip_Leaf = rootNode.Children![0];
            Assert.Equal("Storeys 1-2", roundTrip_Leaf.Name);
            Assert.Equal("Low", roundTrip_Leaf.Description);
            Assert.Equal("#0000ff", roundTrip_Leaf.Color);
            Assert.Equal([0], roundTrip_Leaf.Path);
            Assert.Equal(1, roundTrip_Leaf.Count);
            Assert.Null(roundTrip_Leaf.Children);

            Assert.Single(roundTrip.Buildings);
            ViewModels.TypologyBuildingViewModel roundTrip_Building = roundTrip.Buildings[0];
            Assert.Equal("A1", roundTrip_Building.Reference);
            Assert.Equal(9157, roundTrip_Building.Id);
            Assert.Equal(101, roundTrip_Building.CountyId);
            Assert.Equal([0], roundTrip_Building.Path);
        }
    }
}
