using System.Text.Json;

namespace DiGi.GIS.WebAPI.UI.xUnit
{
    public partial class Facts
    {
        /// <summary>
        /// Round-trips the <c>POST /typology/buildings</c> response DTO through System.Text.Json (#22): the recursive node
        /// tree survives with each node's name, color and path, a leaf's null children stay null, and the flat building list
        /// carries its reference, county part and path.
        /// </summary>
        [Fact]
        public void TypologyBuildingsViewModel_Serialization()
        {
            ViewModels.TypologyBuildingViewModel building = new("A1", 101, [0]);
            ViewModels.TypologyTreeNodeViewModel leaf = new("Storeys 1-2", "Low", "#0000ff", [0], null);
            ViewModels.TypologyTreeNodeViewModel root = new(null, null, null, [], [leaf]);
            ViewModels.TypologyBuildingsViewModel viewModel = new(root, [building]);

            string json = JsonSerializer.Serialize(viewModel);
            ViewModels.TypologyBuildingsViewModel? roundTrip = JsonSerializer.Deserialize<ViewModels.TypologyBuildingsViewModel>(json);
            Assert.NotNull(roundTrip);

            ViewModels.TypologyTreeNodeViewModel? rootNode = roundTrip.Root;
            Assert.NotNull(rootNode);
            Assert.NotNull(rootNode!.Children);
            Assert.Single(rootNode.Children!);

            ViewModels.TypologyTreeNodeViewModel roundTrip_Leaf = rootNode.Children![0];
            Assert.Equal("Storeys 1-2", roundTrip_Leaf.Name);
            Assert.Equal("Low", roundTrip_Leaf.Description);
            Assert.Equal("#0000ff", roundTrip_Leaf.Color);
            Assert.Equal([0], roundTrip_Leaf.Path);
            Assert.Null(roundTrip_Leaf.Children);

            Assert.Single(roundTrip.Buildings);
            ViewModels.TypologyBuildingViewModel roundTrip_Building = roundTrip.Buildings[0];
            Assert.Equal("A1", roundTrip_Building.Reference);
            Assert.Equal(101, roundTrip_Building.CountyId);
            Assert.Equal([0], roundTrip_Building.Path);
        }
    }
}
