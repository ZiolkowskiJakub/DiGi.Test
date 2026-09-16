using DiGi.Typology.Visual.Classes;
using System.Collections.Generic;
using System.Linq;

namespace DiGi.GIS.WebAPI.UI.xUnit
{
    public partial class Facts
    {
        /// <summary>
        /// Tests the issue #36 ordering of the view DTO: the children of every node come back sorted ascending by
        /// bucket value - a range by its Min, a unique value by the value itself, a multi-word string value by its
        /// whole text, a numeric key before a text one - at every level of the tree, while each child's name, color,
        /// path and count follow the node rather than its new position and the building list is emitted in that same
        /// sorted tree order.
        /// <para>The trees are filed in non-sorted order, as the solver files them in data-encounter order; the
        /// <c>SubTypologies</c> getter returns them in that filed order, which is the unsorted input under test.</para>
        /// </summary>
        [Fact]
        public void Create_TypologyBuildingsViewModel_SortOrder()
        {
            Core.Classes.Color color_A = new(System.Drawing.Color.FromArgb(0, 0, 255));
            Core.Classes.Color color_B = new(System.Drawing.Color.FromArgb(0, 255, 0));
            Core.Classes.Color color_C = new(System.Drawing.Color.FromArgb(255, 0, 0));

            // ----- ranges, filed in the solver's data-encounter order (non-sorted by Min) -----
            VisualTypology range_High = new(new VisualTypologyItem([2], "Floor area [200, 300)", null, color_C.TypologyAppearance()));
            range_High.AddReference("H1");
            VisualTypology range_Low = new(new VisualTypologyItem([0], "Floor area [0, 100)", null, color_A.TypologyAppearance()));
            range_Low.AddReference("L1");
            VisualTypology range_Mid = new(new VisualTypologyItem([1], "Floor area [100, 200)", null, color_B.TypologyAppearance()));
            range_Mid.AddReference("M1");

            // The second level under the middle range, filed non-sorted as well.
            VisualTypology storeys_Three = new(new VisualTypologyItem([1, 1], "Storeys 3", null, color_A.TypologyAppearance()));
            storeys_Three.AddReference("S3");
            VisualTypology storeys_One = new(new VisualTypologyItem([1, 0], "Storeys 1", null, color_B.TypologyAppearance()));
            storeys_One.AddReference("S1");
            DiGi.Typology.Modify.AddSubTypologies(range_Mid, [storeys_Three, storeys_One]);

            VisualTypology root_Ranges = new((string?)null, (string?)null);
            DiGi.Typology.Modify.AddSubTypologies(root_Ranges, [range_High, range_Low, range_Mid]);

            ViewModels.TypologyBuildingsViewModel? viewModel_Ranges = root_Ranges.TypologyBuildingsViewModel();
            Assert.NotNull(viewModel_Ranges);
            ViewModels.TypologyTreeNodeViewModel? root_Ranges_Node = viewModel_Ranges.Root;
            Assert.NotNull(root_Ranges_Node);
            Assert.Equal(3, root_Ranges_Node.Children!.Count);

            // Ascending by the Min of each range, whatever the filed order.
            Assert.Equal("Floor area [0, 100)", root_Ranges_Node.Children[0].Name);
            Assert.Equal("Floor area [100, 200)", root_Ranges_Node.Children[1].Name);
            Assert.Equal("Floor area [200, 300)", root_Ranges_Node.Children[2].Name);

            // The data follows the node, not its new position.
            Assert.Equal([0], root_Ranges_Node.Children[0].Path);
            Assert.Equal("#0000ff", root_Ranges_Node.Children[0].Color);
            Assert.Equal(1, root_Ranges_Node.Children[0].Count);
            Assert.Equal([1], root_Ranges_Node.Children[1].Path);
            Assert.Equal("#00ff00", root_Ranges_Node.Children[1].Color);
            Assert.Equal(2, root_Ranges_Node.Children[1].Count);
            Assert.Equal([2], root_Ranges_Node.Children[2].Path);
            Assert.Equal("#ff0000", root_Ranges_Node.Children[2].Color);

            // Recursion: the second level sorts the same way.
            ViewModels.TypologyTreeNodeViewModel node_Mid = root_Ranges_Node.Children[1];
            Assert.Equal(2, node_Mid.Children!.Count);
            Assert.Equal("Storeys 1", node_Mid.Children[0].Name);
            Assert.Equal([1, 0], node_Mid.Children[0].Path);
            Assert.Equal("Storeys 3", node_Mid.Children[1].Name);
            Assert.Equal([1, 1], node_Mid.Children[1].Path);

            // The building list is emitted in the sorted tree's order: Low, then Mid's children (sorted) and Mid, then High.
            List<string> references_Ranges = viewModel_Ranges.Buildings.Select(b => b.Reference).ToList();
            Assert.Equal(["L1", "S1", "S3", "M1", "H1"], references_Ranges);

            // ----- unique numeric values, filed non-sorted -----
            VisualTypology storeys_Eight = new(new VisualTypologyItem([2], "Storeys 8", null, color_C.TypologyAppearance()));
            storeys_Eight.AddReference("E8");
            VisualTypology storeys_Two = new(new VisualTypologyItem([0], "Storeys 2", null, color_A.TypologyAppearance()));
            storeys_Two.AddReference("E2");
            VisualTypology storeys_Five = new(new VisualTypologyItem([1], "Storeys 5", null, color_B.TypologyAppearance()));
            storeys_Five.AddReference("E5");

            VisualTypology root_Numerics = new((string?)null, (string?)null);
            DiGi.Typology.Modify.AddSubTypologies(root_Numerics, [storeys_Eight, storeys_Two, storeys_Five]);

            ViewModels.TypologyBuildingsViewModel? viewModel_Numerics = root_Numerics.TypologyBuildingsViewModel();
            Assert.NotNull(viewModel_Numerics);
            Assert.Equal(3, viewModel_Numerics.Root!.Children!.Count);
            Assert.Equal("Storeys 2", viewModel_Numerics.Root.Children[0].Name);
            Assert.Equal("Storeys 5", viewModel_Numerics.Root.Children[1].Name);
            Assert.Equal("Storeys 8", viewModel_Numerics.Root.Children[2].Name);

            // ----- multi-word string values: the whole value orders, not the last word -----
            VisualTypology function_ZebraApple = new(new VisualTypologyItem([2], "Function Zebra Apple", null, color_C.TypologyAppearance()));
            function_ZebraApple.AddReference("F1");
            VisualTypology function_AppleZebra = new(new VisualTypologyItem([0], "Function Apple Zebra", null, color_A.TypologyAppearance()));
            function_AppleZebra.AddReference("F2");
            VisualTypology function_Mango = new(new VisualTypologyItem([1], "Function Mango", null, color_B.TypologyAppearance()));
            function_Mango.AddReference("F3");

            VisualTypology root_Text = new((string?)null, (string?)null);
            DiGi.Typology.Modify.AddSubTypologies(root_Text, [function_ZebraApple, function_AppleZebra, function_Mango]);

            ViewModels.TypologyBuildingsViewModel? viewModel_Text = root_Text.TypologyBuildingsViewModel();
            Assert.NotNull(viewModel_Text);
            Assert.Equal(3, viewModel_Text.Root!.Children!.Count);
            Assert.Equal("Function Apple Zebra", viewModel_Text.Root.Children[0].Name);
            Assert.Equal("Function Mango", viewModel_Text.Root.Children[1].Name);
            Assert.Equal("Function Zebra Apple", viewModel_Text.Root.Children[2].Name);

            // ----- fallbacks: a numeric key precedes a text one -----
            VisualTypology value_Text = new(new VisualTypologyItem([1], "X abc", null, color_B.TypologyAppearance()));
            VisualTypology value_Numeric = new(new VisualTypologyItem([0], "X 12", null, color_A.TypologyAppearance()));

            VisualTypology root_Mixed = new((string?)null, (string?)null);
            DiGi.Typology.Modify.AddSubTypologies(root_Mixed, [value_Text, value_Numeric]);

            ViewModels.TypologyBuildingsViewModel? viewModel_Mixed = root_Mixed.TypologyBuildingsViewModel();
            Assert.NotNull(viewModel_Mixed);
            Assert.Equal(2, viewModel_Mixed.Root!.Children!.Count);
            Assert.Equal("X 12", viewModel_Mixed.Root.Children[0].Name);
            Assert.Equal("X abc", viewModel_Mixed.Root.Children[1].Name);

            // ----- equal keys keep the filed order: the sort is stable -----
            VisualTypology equal_Second = new(new VisualTypologyItem([1], "Custom", null, color_B.TypologyAppearance()));
            VisualTypology equal_First = new(new VisualTypologyItem([0], "Custom", null, color_A.TypologyAppearance()));

            VisualTypology root_Equal = new((string?)null, (string?)null);
            DiGi.Typology.Modify.AddSubTypologies(root_Equal, [equal_Second, equal_First]);

            ViewModels.TypologyBuildingsViewModel? viewModel_Equal = root_Equal.TypologyBuildingsViewModel();
            Assert.NotNull(viewModel_Equal);
            Assert.Equal(2, viewModel_Equal.Root!.Children!.Count);
            Assert.Equal("#00ff00", viewModel_Equal.Root.Children[0].Color);
            Assert.Equal("#0000ff", viewModel_Equal.Root.Children[1].Color);
        }
    }
}
