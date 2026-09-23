using DiGi.Core.IO.Table.Classes;
using DiGi.Typology.Visual.Classes;
using System.Collections.Generic;
using System.Linq;

namespace DiGi.GIS.WebAPI.UI.xUnit
{
    public partial class Facts
    {
        /// <summary>
        /// Verifies that the solve DTO keeps one building per <c>(reference, countyId)</c> (DiGi.GIS.WebAPI.UI#51). A reference filed under two county parts - two rows of the building data - gives two entries, each with its own county part and database identifier. The bucket's count counts both, so the tree total still equals the number of dots.
        /// <para>Before the change the table overload keyed its lookups and its emission on the reference alone: the two rows collapsed into one entry carrying whichever county part the table listed last, and the other building vanished from the map and the grid.</para>
        /// </summary>
        [Fact]
        public void Create_TypologyBuildingsViewModel_MultiPart()
        {
            Core.Classes.Color color = new(System.Drawing.Color.FromArgb(0, 0, 255));

            VisualTypology child = new(new VisualTypologyItem([0], "Storeys 1-2", null, color.TypologyAppearance()));
            child.AddReference("SHARED");
            child.AddReference("SINGLE");

            VisualTypology root = new((string?)null, (string?)null);
            DiGi.Typology.Modify.AddSubTypologies(root, [child]);

            Table table = new([
                new Column(0, "Reference", typeof(string)),
                new Column(1, "County Id", typeof(int)),
                new Column(2, "Database Id", typeof(long))
            ]);
            table.AddRow(new Dictionary<int, object?>() { [0] = "SHARED", [1] = 78238, [2] = 11L });
            table.AddRow(new Dictionary<int, object?>() { [0] = "SHARED", [1] = 78244, [2] = 12L });
            table.AddRow(new Dictionary<int, object?>() { [0] = "SINGLE", [1] = 78238, [2] = 13L });

            // The same (reference, county part) twice is one building.
            table.AddRow(new Dictionary<int, object?>() { [0] = "SINGLE", [1] = 78238, [2] = 13L });

            ViewModels.TypologyBuildingsViewModel? viewModel = root.TypologyBuildingsViewModel(table);
            Assert.NotNull(viewModel);

            List<(string Reference, int CountyId, long Id)> buildings = [.. viewModel.Buildings.Select(b => (b.Reference, b.CountyId, b.Id)).OrderBy(x => x.Id)];
            Assert.Equal([("SHARED", 78238, 11L), ("SHARED", 78244, 12L), ("SINGLE", 78238, 13L)], buildings);
            Assert.All(viewModel.Buildings, b => Assert.Equal([0], b.Path));

            ViewModels.TypologyTreeNodeViewModel? node_Root = viewModel.Root;
            Assert.NotNull(node_Root);
            Assert.NotNull(node_Root.Children);
            Assert.Equal(3, node_Root.Children.Single().Count);
            Assert.Equal(3, node_Root.Count);
        }
    }
}
