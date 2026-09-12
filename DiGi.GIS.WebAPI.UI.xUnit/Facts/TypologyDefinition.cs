using DiGi.Core.Classes;
using DiGi.GIS.WebAPI.UI.Classes;
using DiGi.PostgreSQL.Table.Classes;
using DiGi.Typology.Visual.Classes;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.Json;

namespace DiGi.GIS.WebAPI.UI.xUnit
{
    public partial class Facts
    {
        // The page state exactly as typology.js posts it: camelCase, values as JSON primitives, both row
        // lists present on every level. Deserialized with the web options so that the unique values bind as
        // JsonElement, the form the controller receives.
        private const string TypologyDefinitionJson = @"{ ""levels"": [
            { ""uniqueId"": ""predicted_year_built"", ""ruleType"": ""VisualIntegerRangeFilterRule"",
              ""ranges"": [ { ""min"": 0, ""max"": 2003, ""color"": ""#1f77b4"" }, { ""min"": 2004, ""max"": 2020, ""color"": ""#ff7f0e"" }, { ""min"": 2021, ""max"": 2147483647, ""color"": ""#2ca02c"" } ],
              ""uniqueValueColors"": [] },
            { ""uniqueId"": ""occupancy"", ""ruleType"": ""VisualUniqueValueFilterRule"", ""ranges"": [],
              ""uniqueValueColors"": [ { ""value"": ""Residential"", ""color"": ""#d62728"" }, { ""value"": null, ""color"": ""#7f7f7f"" }, { ""value"": ""Industrial"", ""color"": ""#9467bd"" } ] },
            { ""uniqueId"": ""floor_area"", ""ruleType"": ""VisualDoubleRangeFilterRule"",
              ""ranges"": [ { ""min"": 0.5, ""max"": 12.25, ""color"": ""#8c564b"" }, { ""min"": 12.5, ""max"": 1000000, ""color"": ""#e377c2"" } ],
              ""uniqueValueColors"": [] } ] }";

        /// <summary>
        /// Validates the Export then Import round trip of a Typology definition (#17): the page state becomes a
        /// <see cref="VisualColumnTypologyFilter"/> chain whose rule <c>_type</c>s are the three non-generic Visual rules
        /// and whose colors are filed under the invariant <c>"[min, max]"</c> and value keys, the chain survives the DiGi
        /// JSON round trip and <c>SerializationCheck</c>, and reading it back yields the page state it came from - levels,
        /// ranges, colors and unique values alike, with the column described from the catalog.
        /// <para>The whole trip is repeated under <c>pl-PL</c>, where the decimal separator is a comma, so a range key
        /// or a bound rendered through the current culture would break the second pass.</para>
        /// </summary>
        [Fact]
        public void TypologyDefinition_RoundTrip()
        {
            List<Column> columns = Create.BuildingDataColumns();

            RoundTrip(columns);

            CultureInfo cultureInfo = CultureInfo.CurrentCulture;
            try
            {
                CultureInfo.CurrentCulture = new CultureInfo("pl-PL");
                Assert.NotEqual("[0.5, 12.25]", new Range<double>(0.5, 12.25).ToString());
                RoundTrip(columns);
            }
            finally
            {
                CultureInfo.CurrentCulture = cultureInfo;
            }

            static void RoundTrip(List<Column> columns)
            {
                TypologyDefinitionParameter? typologyDefinitionParameter = JsonSerializer.Deserialize<TypologyDefinitionParameter>(TypologyDefinitionJson, JsonSerializerOptions.Web);
                Assert.NotNull(typologyDefinitionParameter);
                Assert.IsType<JsonElement>(typologyDefinitionParameter.Levels![1].UniqueValueColors![0].Value);

                Assert.Empty(typologyDefinitionParameter.TypologyDefinitionErrors(columns));

                VisualColumnTypologyFilter? visualColumnTypologyFilter = typologyDefinitionParameter.VisualColumnTypologyFilter(columns);
                Assert.NotNull(visualColumnTypologyFilter);

                string? json = Core.Convert.ToSystem_String(visualColumnTypologyFilter);
                Assert.NotNull(json);
                Assert.Contains("DiGi.Typology.Visual.Classes.VisualColumnTypologyFilter,DiGi.Typology.Visual", json);
                Assert.Contains("DiGi.Typology.Visual.Classes.VisualIntegerRangeFilterRule,DiGi.Typology.Visual", json);
                Assert.Contains("DiGi.Typology.Visual.Classes.VisualDoubleRangeFilterRule,DiGi.Typology.Visual", json);
                Assert.Contains("DiGi.Typology.Visual.Classes.VisualUniqueValueFilterRule,DiGi.Typology.Visual", json);
                Assert.Contains("\"[2004, 2020]\"", json);
                Assert.Contains("\"[0.5, 12.25]\"", json);
                Assert.Contains("\"null\"", json);
                Assert.DoesNotContain("VisualColumnTypologyFilter`1", json);

                VisualColumnTypologyFilter? visualColumnTypologyFilter_RoundTrip = Core.Convert.ToDiGi<VisualColumnTypologyFilter>(json)?.FirstOrDefault();
                Assert.NotNull(visualColumnTypologyFilter_RoundTrip);
                Assert.Equal("predicted_year_built", Core.IO.Query.UniqueId(visualColumnTypologyFilter_RoundTrip.Value));
                Assert.Equal(typeof(ushort), visualColumnTypologyFilter_RoundTrip.Value?.Type);
                Assert.Equal(2, visualColumnTypologyFilter_RoundTrip.Value?.Index);

                Assert.Empty(visualColumnTypologyFilter_RoundTrip.TypologyDefinitionErrors(columns));

                TypologyDefinitionParameter? typologyDefinitionParameter_RoundTrip = visualColumnTypologyFilter_RoundTrip.TypologyDefinitionParameter(columns);
                Assert.NotNull(typologyDefinitionParameter_RoundTrip?.Levels);
                Assert.Equal(3, typologyDefinitionParameter_RoundTrip.Levels.Count);

                TypologyDefinitionLevelParameter level_Year = typologyDefinitionParameter_RoundTrip.Levels[0];
                Assert.Equal("predicted_year_built", level_Year.UniqueId);
                Assert.Equal("Predicted year built", level_Year.Name);
                Assert.Equal(4, level_Year.DataType);
                Assert.True(level_Year.IsNumeric);
                Assert.Equal(nameof(VisualIntegerRangeFilterRule), level_Year.RuleType);
                Assert.Equal([0.0, 2004.0, 2021.0], level_Year.Ranges!.ConvertAll(x => x.Min));
                Assert.Equal([2003.0, 2020.0, 2147483647.0], level_Year.Ranges.ConvertAll(x => x.Max));
                Assert.Equal(["#1f77b4", "#ff7f0e", "#2ca02c"], level_Year.Ranges.ConvertAll(x => x.Color));
                Assert.Empty(level_Year.UniqueValueColors!);

                TypologyDefinitionLevelParameter level_Occupancy = typologyDefinitionParameter_RoundTrip.Levels[1];
                Assert.Equal("occupancy", level_Occupancy.UniqueId);
                Assert.Equal(14, level_Occupancy.DataType);
                Assert.False(level_Occupancy.IsNumeric);
                Assert.Equal(nameof(VisualUniqueValueFilterRule), level_Occupancy.RuleType);
                Assert.Equal(["Residential", null, "Industrial"], level_Occupancy.UniqueValueColors!.ConvertAll(x => x.Value));
                Assert.Equal(["#d62728", "#7f7f7f", "#9467bd"], level_Occupancy.UniqueValueColors.ConvertAll(x => x.Color));
                Assert.Empty(level_Occupancy.Ranges!);

                TypologyDefinitionLevelParameter level_FloorArea = typologyDefinitionParameter_RoundTrip.Levels[2];
                Assert.Equal("floor_area", level_FloorArea.UniqueId);
                Assert.Equal(nameof(VisualDoubleRangeFilterRule), level_FloorArea.RuleType);
                Assert.Equal([0.5, 12.5], level_FloorArea.Ranges!.ConvertAll(x => x.Min));
                Assert.Equal([12.25, 1000000.0], level_FloorArea.Ranges.ConvertAll(x => x.Max));
                Assert.Equal(["#8c564b", "#e377c2"], level_FloorArea.Ranges.ConvertAll(x => x.Color));

                Core.xUnit.Query.SerializationCheck(visualColumnTypologyFilter);
            }
        }

        /// <summary>
        /// Validates that <c>Query.TypologyDefinitionErrors</c> refuses each defect the Import and Export actions must
        /// reject with a message naming it, on the page state side and on the document side, and that a valid state is
        /// accepted - so every guard is shown to fail on the case it exists for rather than merely to exist.
        /// </summary>
        [Fact]
        public void TypologyDefinition_Errors()
        {
            List<Column> columns = Create.BuildingDataColumns();

            Assert.Contains(Query.TypologyDefinitionErrors((TypologyDefinitionParameter?)null, columns), x => x.Contains("no levels"));
            Assert.Contains(Query.TypologyDefinitionErrors(new TypologyDefinitionParameter() { Levels = [] }, columns), x => x.Contains("no levels"));
            Assert.Contains(Level("predicted_year_built", nameof(VisualIntegerRangeFilterRule), [Range(0, 10, "#000000")]).TypologyDefinitionErrors(null), x => x.Contains("catalog is unavailable"));

            Assert.Contains(Errors(Level("no_such_column", nameof(VisualIntegerRangeFilterRule), [Range(0, 10, "#000000")])), x => x.Contains("not a building data column"));
            Assert.Contains(Errors(Level("predicted_year_built", null, [Range(0, 10, "#000000")])), x => x.Contains("no rule type chosen"));
            Assert.Contains(Errors(Level("predicted_year_built", "IntegerRangeFilterRule", [Range(0, 10, "#000000")])), x => x.Contains("unknown rule type"));
            Assert.Contains(Errors(Level("occupancy", nameof(VisualIntegerRangeFilterRule), [Range(0, 10, "#000000")])), x => x.Contains("needs an integer column"));
            Assert.Contains(Errors(Level("predicted_year_built", nameof(VisualDoubleRangeFilterRule), [Range(0, 10, "#000000")])), x => x.Contains("needs a floating point column"));
            Assert.Contains(Errors(Level("floor_area", nameof(VisualIntegerRangeFilterRule), [Range(0, 10, "#000000")])), x => x.Contains("needs an integer column"));

            Assert.Contains(Errors(Level("predicted_year_built", nameof(VisualIntegerRangeFilterRule), [])), x => x.Contains("at least one range"));
            Assert.Contains(Errors(Level("predicted_year_built", nameof(VisualIntegerRangeFilterRule), [Range(null, 10, "#000000")])), x => x.Contains("both bounds"));
            Assert.Contains(Errors(Level("predicted_year_built", nameof(VisualIntegerRangeFilterRule), [Range(0.5, 10, "#000000")])), x => x.Contains("whole number"));
            Assert.Contains(Errors(Level("predicted_year_built", nameof(VisualIntegerRangeFilterRule), [Range(10, 0, "#000000")])), x => x.Contains("minimum exceeds"));
            Assert.Contains(Errors(Level("predicted_year_built", nameof(VisualIntegerRangeFilterRule), [Range(0, 10, "#zz0000")])), x => x.Contains("not a color"));
            Assert.Contains(Errors(Level("predicted_year_built", nameof(VisualIntegerRangeFilterRule), [Range(0, 10, null)])), x => x.Contains("not a color"));
            Assert.Contains(Errors(Level("predicted_year_built", nameof(VisualIntegerRangeFilterRule), [Range(0, 10, "#000000"), Range(0, 20, "#000000")])), x => x.Contains("two ranges start at 0"));
            Assert.Contains(Errors(Level("predicted_year_built", nameof(VisualIntegerRangeFilterRule), [Range(11, 20, "#000000"), Range(0, 11, "#000000")])), x => x.Contains("[0, 11] and [11, 20] overlap"));
            Assert.Contains(Errors(Level("floor_area", nameof(VisualDoubleRangeFilterRule), [Range(0.5, 12.25, "#000000"), Range(12.0, 20, "#000000")])), x => x.Contains("[0.5, 12.25] and [12, 20] overlap"));

            Assert.Contains(Errors(Level("occupancy", nameof(VisualUniqueValueFilterRule), null, [])), x => x.Contains("at least one value"));
            Assert.Contains(Errors(Level("predicted_year_built", nameof(VisualUniqueValueFilterRule), null, [UniqueValue("abc", "#000000")])), x => x.Contains("not a UShort"));
            Assert.Contains(Errors(Level("occupancy", nameof(VisualUniqueValueFilterRule), null, [UniqueValue("A", "#000000"), UniqueValue("A", "#ffffff")])), x => x.Contains("'A' is listed twice"));
            Assert.Contains(Errors(Level("occupancy", nameof(VisualUniqueValueFilterRule), null, [UniqueValue("A", "#12")])), x => x.Contains("not a color"));

            TypologyDefinitionParameter typologyDefinitionParameter_Twice = new() { Levels = [Level("occupancy", nameof(VisualUniqueValueFilterRule), null, [UniqueValue("A", "#000000")]).Levels![0], Level("occupancy", nameof(VisualUniqueValueFilterRule), null, [UniqueValue("B", "#000000")]).Levels![0]] };
            Assert.Contains(typologyDefinitionParameter_Twice.TypologyDefinitionErrors(columns), x => x.Contains("used by an earlier level"));

            // A numeric column classified by unique value is legitimate: the rule kind is never inferred from the column.
            TypologyDefinitionParameter typologyDefinitionParameter_Valid = Level("predicted_year_built", nameof(VisualUniqueValueFilterRule), null, [UniqueValue(2010, "#000000"), UniqueValue(null, "#ffffff")]);
            Assert.Empty(typologyDefinitionParameter_Valid.TypologyDefinitionErrors(columns));
            VisualColumnTypologyFilter? visualColumnTypologyFilter_Valid = typologyDefinitionParameter_Valid.VisualColumnTypologyFilter(columns);
            Assert.NotNull(visualColumnTypologyFilter_Valid);
            Assert.Equal(["2010", "null"], Assert.IsType<VisualUniqueValueFilterRule>(visualColumnTypologyFilter_Valid.Rule).TypologyAppearanceCollection.Keys);
            Assert.Null(Level("no_such_column", nameof(VisualIntegerRangeFilterRule), [Range(0, 10, "#000000")]).VisualColumnTypologyFilter(columns));

            // Document side: what a file may carry that the page state cannot.
            Assert.Contains(Query.TypologyDefinitionErrors((VisualColumnTypologyFilter?)null, columns), x => x.Contains("no levels"));

            Core.Classes.Color color = new(255, 0, 0, 0);

            VisualIntegerRangeFilterRule visualIntegerRangeFilterRule = new([new Range<int>(0, 10), new Range<int>(20, 30)]);
            visualIntegerRangeFilterRule.TypologyAppearanceCollection[new Range<int>(0, 10)] = color.TypologyAppearance();
            VisualColumnTypologyFilter visualColumnTypologyFilter = new() { Value = new Core.IO.Table.Classes.Column(2, "Predicted year built", typeof(ushort)), Rule = visualIntegerRangeFilterRule };
            Assert.Contains(visualColumnTypologyFilter.TypologyDefinitionErrors(columns), x => x.Contains("range [20, 30] has no color"));

            visualIntegerRangeFilterRule.TypologyAppearanceCollection[new Range<int>(20, 30)] = color.TypologyAppearance();
            Assert.Empty(visualColumnTypologyFilter.TypologyDefinitionErrors(columns));
            Assert.Null(visualColumnTypologyFilter.TypologyDefinitionParameter(null));

            visualIntegerRangeFilterRule.TypologyAppearanceCollection["[1, 2]"] = color.TypologyAppearance();
            Assert.Contains(visualColumnTypologyFilter.TypologyDefinitionErrors(columns), x => x.Contains("filed under '[1, 2]' matches no range"));
            Assert.Null(visualColumnTypologyFilter.TypologyDefinitionParameter(columns));

            visualIntegerRangeFilterRule.TypologyAppearanceCollection["[1, 2]"] = null;
            visualIntegerRangeFilterRule.TypologyAppearanceCollection[new Range<int>(20, 30)] = new TypologyAppearance();
            Assert.Contains(visualColumnTypologyFilter.TypologyDefinitionErrors(columns), x => x.Contains("appearance of range [20, 30] carries no color"));

            VisualIntegerRangeFilterRule visualIntegerRangeFilterRule_Overlap = new([new Range<int>(0, 10), new Range<int>(5, 20)]);
            visualIntegerRangeFilterRule_Overlap.TypologyAppearanceCollection[new Range<int>(0, 10)] = color.TypologyAppearance();
            visualIntegerRangeFilterRule_Overlap.TypologyAppearanceCollection[new Range<int>(5, 20)] = color.TypologyAppearance();
            visualColumnTypologyFilter.Rule = visualIntegerRangeFilterRule_Overlap;
            Assert.Contains(visualColumnTypologyFilter.TypologyDefinitionErrors(columns), x => x.Contains("[0, 10] and [5, 20] overlap"));

            visualColumnTypologyFilter.Rule = null;
            Assert.Contains(visualColumnTypologyFilter.TypologyDefinitionErrors(columns), x => x.Contains("no rule"));

            visualColumnTypologyFilter.Rule = new VisualDoubleRangeFilterRule([new Range<double>(0, 1)]);
            Assert.Contains(visualColumnTypologyFilter.TypologyDefinitionErrors(columns), x => x.Contains("needs a floating point column"));

            VisualUniqueValueFilterRule visualUniqueValueFilterRule = new();
            visualUniqueValueFilterRule.TypologyAppearanceCollection["abc"] = color.TypologyAppearance();
            visualColumnTypologyFilter.Rule = visualUniqueValueFilterRule;
            Assert.Contains(visualColumnTypologyFilter.TypologyDefinitionErrors(columns), x => x.Contains("'abc' is not a UShort"));

            visualColumnTypologyFilter.Value = new Core.IO.Table.Classes.Column(9, "No such column", typeof(string));
            Assert.Contains(visualColumnTypologyFilter.TypologyDefinitionErrors(columns), x => x.Contains("(no_such_column): not a building data column"));

            visualColumnTypologyFilter.Value = null;
            Assert.Contains(visualColumnTypologyFilter.TypologyDefinitionErrors(columns), x => x.Contains("no column"));

            visualColumnTypologyFilter.Value = new Core.IO.Table.Classes.Column(5, "Occupancy", typeof(string));
            visualColumnTypologyFilter.Filter = visualColumnTypologyFilter;
            Assert.Contains(visualColumnTypologyFilter.TypologyDefinitionErrors(columns), x => x.Contains("links back on itself"));

            // A file whose _type names no known class deserializes to nothing rather than to an emptied level.
            Assert.Null(Core.Convert.ToDiGi<VisualColumnTypologyFilter>("{ \"_type\": \"DiGi.Typology.Visual.Classes.NoSuchFilter,DiGi.Typology.Visual\", \"Rule\": null }")?.FirstOrDefault());

            static List<string> Errors(TypologyDefinitionParameter typologyDefinitionParameter)
            {
                return typologyDefinitionParameter.TypologyDefinitionErrors(Create.BuildingDataColumns());
            }

            static TypologyDefinitionParameter Level(string uniqueId, string? ruleType, List<TypologyDefinitionRangeParameter>? ranges, List<TypologyDefinitionUniqueValueParameter>? uniqueValues = null)
            {
                return new TypologyDefinitionParameter() { Levels = [new TypologyDefinitionLevelParameter() { UniqueId = uniqueId, RuleType = ruleType, Ranges = ranges, UniqueValueColors = uniqueValues }] };
            }

            static TypologyDefinitionRangeParameter Range(double? min, double? max, string? color)
            {
                return new TypologyDefinitionRangeParameter() { Min = min, Max = max, Color = color };
            }

            static TypologyDefinitionUniqueValueParameter UniqueValue(object? value, string? color)
            {
                return new TypologyDefinitionUniqueValueParameter() { Value = value, Color = color };
            }
        }

        /// <summary>
        /// Validates <see cref="Query.Hex(Core.Classes.Color)"/>: a color renders as the lower case <c>#rrggbb</c> the
        /// color picker holds, the string parses back to the same color through <c>Core.Convert.ToDrawing</c>, and null
        /// renders as null.
        /// </summary>
        [Fact]
        public void Query_Hex()
        {
            Core.Classes.Color color = new(255, 0x1f, 0x77, 0xb4);
            Assert.Equal("#1f77b4", color.Hex());
            Assert.Equal("#000000", new Core.Classes.Color(255, 0, 0, 0).Hex());
            Assert.Equal("#ffffff", new Core.Classes.Color(255, 255, 255, 255).Hex());
            Assert.Null(Query.Hex(null));

            Core.Classes.Color color_RoundTrip = Core.Convert.ToDiGi(Core.Convert.ToDrawing(color.Hex()));
            Assert.Equal(color.Red, color_RoundTrip.Red);
            Assert.Equal(color.Green, color_RoundTrip.Green);
            Assert.Equal(color.Blue, color_RoundTrip.Blue);
            Assert.Equal("#1f77b4", color_RoundTrip.Hex());
        }
    }
}
