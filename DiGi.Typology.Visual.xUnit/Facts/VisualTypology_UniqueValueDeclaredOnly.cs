using DiGi.Core.IO.Table.Classes;
using DiGi.Typology.Classes;
using DiGi.Typology.Visual.Classes;
using System.Collections.Generic;

namespace DiGi.Typology.Visual.xUnit
{
    public partial class Facts
    {
        /// <summary>
        /// Issue DiGi.GIS.WebAPI.UI #39: a unique value level of a solved Visual typology holds exactly the values its definition declares, not every value the table carries.
        /// <para>Reproduces the reported symptom on the typology view: the level declared one value of a column the area carries several of, yet the solved tree listed every value present, while a range level lists only the ranges it declares. Aligned with the range rule, a value with no filed appearance resolves to no bucket and is dropped from the level's subtree.</para>
        /// </summary>
        [Fact]
        public void VisualTypology_UniqueValueDeclaredOnly()
        {
            string declaredValue = "agricultural production, service, and utility buildings";

            Table table = new();

            Assert.NotNull(table.AddColumn("building_reference", typeof(string)));
            Assert.NotNull(table.AddColumn("building_general_function", typeof(string)));

            AddRow("b1", declaredValue);
            AddRow("b2", declaredValue);
            AddRow("b3", "residential");
            AddRow("b4", "industrial");

            VisualUniqueValueFilterRule visualUniqueValueFilterRule = new();

            TypologyAppearance typologyAppearance = Create.TypologyAppearance(System.Drawing.Color.Red);
            visualUniqueValueFilterRule.TypologyAppearanceCollection[declaredValue] = typologyAppearance;

            VisualColumnTypologyFilter<Column> visualColumnTypologyFilter = new()
            {
                Value = new Column(-1, "building_general_function", typeof(string)),
                Rule = visualUniqueValueFilterRule
            };

            Column column_Reference = new("building_reference", typeof(string));

            VisualTypology? visualTypology = Typology.Visual.Create.VisualTypology(table, visualColumnTypologyFilter, column_Reference);

            Assert.NotNull(visualTypology);
            Assert.NotNull(visualTypology.SubTypologies);

            // The level holds exactly the declared value the table contains - not the values it also carries.
            VisualTypology visualTypology_Declared = Assert.Single(visualTypology.SubTypologies!);

            Assert.Equal($"building_general_function {declaredValue}", visualTypology_Declared.Name);
            Assert.Equal(Core.Convert.ToSystem_String(typologyAppearance), Core.Convert.ToSystem_String(visualTypology_Declared.TypologyItem?.Appearance));
            Assert.Equal(["b1", "b2"], [.. visualTypology_Declared.References.OrderBy(x => x)]);

            void AddRow(string buildingReference, string buildingGeneralFunction)
            {
                List<object?> values = [buildingReference, buildingGeneralFunction];

                Assert.NotNull(table.AddRow(values));
            }
        }
    }
}
