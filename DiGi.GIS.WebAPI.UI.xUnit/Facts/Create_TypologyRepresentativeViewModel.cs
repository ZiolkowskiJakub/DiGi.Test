using DiGi.Core.IO.Table.Classes;
using DiGi.GIS.WebAPI.UI.Classes;
using DiGi.GIS.WebAPI.UI.ViewModels;
using System.Collections.Generic;
using System.Globalization;

namespace DiGi.GIS.WebAPI.UI.xUnit
{
    public partial class Facts
    {
        /// <summary>
        /// Tests the view model of a typology node's representative (<see cref="UI.Create.TypologyRepresentativeViewModel"/>), built from the building data table the GIS Web API answers.
        /// <para>A range level is compared as a number and a unique-value level as a category. The representative is the medoid among the buildings of the modal category, and it carries its own Database Id and county part. Each level reports the representative's value beside the node's median or modal value. The synthetic archetype's size is the median floor area and the rounded median storeys.</para>
        /// <para>Every number is rendered with the invariant culture: the fact runs under pl-PL, where the current culture would write a decimal comma (Coding - General, section 1.19).</para>
        /// <para>A level the catalog does not know is skipped, and a table without the reference column answers null.</para>
        /// </summary>
        [Fact]
        public void Create_TypologyRepresentativeViewModel()
        {
            CultureInfo cultureInfo = CultureInfo.CurrentCulture;
            try
            {
                CultureInfo.CurrentCulture = new CultureInfo("pl-PL");

                Table table = new([
                    new Column(0, "Reference", typeof(string)),
                    new Column(1, "County Id", typeof(int)),
                    new Column(2, "Database Id", typeof(long)),
                    new Column(3, "Floor area", typeof(float)),
                    new Column(4, "Storeys", typeof(int)),
                    new Column(5, "Function", typeof(string))
                ]);
                table.AddRow(new Dictionary<int, object?>() { [0] = "A", [1] = 5, [2] = 101L, [3] = 100.5f, [4] = 1, [5] = "house" });
                table.AddRow(new Dictionary<int, object?>() { [0] = "B", [1] = 5, [2] = 102L, [3] = 110.5f, [4] = 2, [5] = "house" });
                table.AddRow(new Dictionary<int, object?>() { [0] = "C", [1] = 5, [2] = 103L, [3] = 120.5f, [4] = 2, [5] = "shop" });
                table.AddRow(new Dictionary<int, object?>() { [0] = "D", [1] = 7, [2] = 104L, [3] = 130.5f, [4] = 3, [5] = "house" });
                table.AddRow(new Dictionary<int, object?>() { [0] = "E", [1] = 7, [2] = 105L, [3] = 900.5f, [4] = 2, [5] = "house" });

                List<DiGi.PostgreSQL.Table.Classes.Column> columns = [
                    new() { Name = "Reference", UniqueId = "reference" },
                    new() { Name = "Floor area", UniqueId = "floor_area" },
                    new() { Name = "Storeys", UniqueId = "storeys" },
                    new() { Name = "Function", UniqueId = "function" }
                ];

                TypologyDefinitionParameter typologyDefinitionParameter = new()
                {
                    Levels = [
                        new TypologyDefinitionLevelParameter() { UniqueId = "function", Name = "Function", RuleType = "VisualUniqueValueFilterRule" },
                        new TypologyDefinitionLevelParameter() { UniqueId = "floor_area", Name = "Floor area", IsNumeric = true, RuleType = "VisualDoubleRangeFilterRule" },
                        new TypologyDefinitionLevelParameter() { UniqueId = "unknown", Name = "Unknown", IsNumeric = true, RuleType = "VisualDoubleRangeFilterRule" }
                    ]
                };

                TypologyRepresentativeViewModel? typologyRepresentativeViewModel = table.TypologyRepresentativeViewModel(typologyDefinitionParameter, columns);
                Assert.NotNull(typologyRepresentativeViewModel);

                // "C" is the numeric centre but a shop; among the houses the medoid is "B" (B and D tie on totals, B is ordinal-first).
                Assert.Equal("B", typologyRepresentativeViewModel!.Reference);
                Assert.Equal(102L, typologyRepresentativeViewModel.Id);
                Assert.Equal(5, typologyRepresentativeViewModel.CountyId);
                Assert.Equal(5, typologyRepresentativeViewModel.BuildingCount);
                Assert.Equal(4, typologyRepresentativeViewModel.CandidateCount);
                Assert.Equal(1, typologyRepresentativeViewModel.ParameterCount);

                Assert.Equal(2, typologyRepresentativeViewModel.Parameters.Count);
                TypologyRepresentativeParameterViewModel parameter_Function = typologyRepresentativeViewModel.Parameters[0];
                Assert.Equal("Function", parameter_Function.Name);
                Assert.False(parameter_Function.IsNumeric);
                Assert.Equal("house", parameter_Function.Value);
                Assert.Equal("house", parameter_Function.Typical);

                TypologyRepresentativeParameterViewModel parameter_FloorArea = typologyRepresentativeViewModel.Parameters[1];
                Assert.True(parameter_FloorArea.IsNumeric);
                Assert.Equal("110.5", parameter_FloorArea.Value);
                Assert.Equal("120.5", parameter_FloorArea.Typical);

                Assert.Equal(120.5, typologyRepresentativeViewModel.SyntheticFloorArea!.Value, 6);
                Assert.Equal(2, typologyRepresentativeViewModel.SyntheticStoreys);

                Table table_NoReference = new([new Column(0, "Floor area", typeof(float))]);
                table_NoReference.AddRow(new Dictionary<int, object?>() { [0] = 1f });
                Assert.Null(table_NoReference.TypologyRepresentativeViewModel(typologyDefinitionParameter, columns));
                Assert.Null(((Table?)null).TypologyRepresentativeViewModel(typologyDefinitionParameter, columns));
            }
            finally
            {
                CultureInfo.CurrentCulture = cultureInfo;
            }
        }
    }
}
