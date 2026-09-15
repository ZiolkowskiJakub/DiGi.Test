using DiGi.Core.IO.Table.Classes;
using DiGi.Typology.Visual.Classes;
using System.Collections.Generic;
using System.Linq;

namespace DiGi.GIS.WebAPI.UI.xUnit
{
    public partial class Facts
    {
        /// <summary>
        /// Tests the table overload of the flatten (#21 review): each building's county part and database identifier are read from
        /// the merged building data table's <c>County Id</c> and <c>Database Id</c> columns by its <c>Reference</c>, a reference
        /// the table does not carry falls back to 0 for both, a table lacking the identifier column leaves every identifier at 0,
        /// and a null table leaves every part and identifier at 0.
        /// </summary>
        [Fact]
        public void Create_TypologyBuildingsViewModel_Table()
        {
            Core.Classes.Color color = new(System.Drawing.Color.FromArgb(0, 0, 255));

            VisualTypology child = new(new VisualTypologyItem([0], "Storeys 1-2", null, color.TypologyAppearance()));
            child.AddReference("A1");
            child.AddReference("A2");
            child.AddReference("Missing");

            VisualTypology root = new((string?)null, (string?)null);
            DiGi.Typology.Modify.AddSubTypologies(root, [child]);

            Table table = new([
                new Column(0, "Reference", typeof(string)),
                new Column(1, "County Id", typeof(int)),
                new Column(2, "Database Id", typeof(long))
            ]);
            table.AddRow(new Dictionary<int, object?>() { [0] = "A1", [1] = 101, [2] = 9157L });
            table.AddRow(new Dictionary<int, object?>() { [0] = "A2", [1] = 102 });

            ViewModels.TypologyBuildingsViewModel? viewModel = root.TypologyBuildingsViewModel(table);
            Assert.NotNull(viewModel);
            Assert.Equal(3, viewModel.Buildings.Count);

            ViewModels.TypologyBuildingViewModel building_A1 = viewModel.Buildings.First(b => b.Reference == "A1");
            Assert.Equal(101, building_A1.CountyId);
            Assert.Equal(9157, building_A1.Id);

            // A row without a database identifier leaves the entry at 0; the part is still read.
            ViewModels.TypologyBuildingViewModel building_A2 = viewModel.Buildings.First(b => b.Reference == "A2");
            Assert.Equal(102, building_A2.CountyId);
            Assert.Equal(0, building_A2.Id);

            // A reference the table does not carry falls back to 0 for both.
            ViewModels.TypologyBuildingViewModel building_Missing = viewModel.Buildings.First(b => b.Reference == "Missing");
            Assert.Equal(0, building_Missing.CountyId);
            Assert.Equal(0, building_Missing.Id);

            // A table lacking the identifier column leaves every identifier at 0 and still reads the parts.
            Table table_NoId = new([
                new Column(0, "Reference", typeof(string)),
                new Column(1, "County Id", typeof(int))
            ]);
            table_NoId.AddRow(new Dictionary<int, object?>() { [0] = "A1", [1] = 101 });

            ViewModels.TypologyBuildingsViewModel? viewModel_NoId = root.TypologyBuildingsViewModel(table_NoId);
            Assert.NotNull(viewModel_NoId);
            Assert.All(viewModel_NoId.Buildings, b => Assert.Equal(0, b.Id));
            Assert.Equal(101, viewModel_NoId.Buildings.First(b => b.Reference == "A1").CountyId);

            // A null table leaves every part and identifier at 0.
            ViewModels.TypologyBuildingsViewModel? viewModel_NoTable = root.TypologyBuildingsViewModel((Table?)null);
            Assert.NotNull(viewModel_NoTable);
            Assert.All(viewModel_NoTable.Buildings, b => Assert.Equal(0, b.CountyId));
            Assert.All(viewModel_NoTable.Buildings, b => Assert.Equal(0, b.Id));
        }
    }
}
