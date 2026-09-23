using DiGi.Core.IO.Table.Classes;
using System.Collections.Generic;
using System.Linq;

namespace DiGi.GIS.xUnit
{
    public partial class Facts
    {
        /// <summary>
        /// Verifies that the feature groups partition the input allow-list exactly, so the two cannot drift apart.
        /// <para>The allow-list is assembled from the groups, so this pins the reverse direction: every group column is an allow-list column, every allow-list column belongs to exactly one group, and the group sizes are the ones the model was trained against.</para>
        /// </summary>
        [Fact]
        public void YearBuiltPredictionFeatureGroups_Partition()
        {
            Dictionary<string, List<Column>> columns_ByGroup = IO.Query.YearBuiltPredictionFeatureGroups();
            Assert.NotNull(columns_ByGroup);
            Assert.Equal(5, columns_ByGroup.Count);

            Assert.Equal(31, columns_ByGroup[IO.Constants.YearBuiltPredictionFeatureGroup.Base].Count);
            Assert.Equal(25, columns_ByGroup[IO.Constants.YearBuiltPredictionFeatureGroup.GridCellCoverage].Count);
            Assert.Equal(90, columns_ByGroup[IO.Constants.YearBuiltPredictionFeatureGroup.Detection].Count);
            Assert.Equal(18, columns_ByGroup[IO.Constants.YearBuiltPredictionFeatureGroup.Population].Count);
            Assert.Equal(8, columns_ByGroup[IO.Constants.YearBuiltPredictionFeatureGroup.RadialRatio].Count);

            List<Column> columns_Input = IO.Query.YearBuiltPredictionInputColumns();

            HashSet<string> names_Group = [];
            foreach (KeyValuePair<string, List<Column>> keyValuePair in columns_ByGroup)
            {
                foreach (Column column in keyValuePair.Value)
                {
                    Assert.False(string.IsNullOrWhiteSpace(column.Name));

                    //Exactly one group per column - a column counted twice would be refused twice and warned about twice
                    Assert.True(names_Group.Add(column.Name!), "Column " + column.Name + " belongs to more than one group");
                }
            }

            Assert.Equal(columns_Input.Count, names_Group.Count);
            foreach (Column column in columns_Input)
            {
                Assert.Contains(column.Name!, names_Group);
            }

            //The output column is this pipeline's own answer and belongs to no input group
            Assert.DoesNotContain(IO.Constants.Column.PredictedYearBuilt.Name!, names_Group);
        }

        /// <summary>
        /// Verifies that the year range reaches both the detection and the population groups, so narrowing or widening it moves them together.
        /// </summary>
        [Fact]
        public void YearBuiltPredictionFeatureGroups_Years()
        {
            Dictionary<string, List<Column>> columns_ByGroup = IO.Query.YearBuiltPredictionFeatureGroups(new Core.Classes.Range<int>(2008, 2012), [200]);

            Assert.Equal(25, columns_ByGroup[IO.Constants.YearBuiltPredictionFeatureGroup.Detection].Count);
            Assert.Equal(5, columns_ByGroup[IO.Constants.YearBuiltPredictionFeatureGroup.Population].Count);
            Assert.Equal(2, columns_ByGroup[IO.Constants.YearBuiltPredictionFeatureGroup.RadialRatio].Count);

            //The groups that do not vary with the range stay where they are
            Assert.Equal(31, columns_ByGroup[IO.Constants.YearBuiltPredictionFeatureGroup.Base].Count);
            Assert.Equal(25, columns_ByGroup[IO.Constants.YearBuiltPredictionFeatureGroup.GridCellCoverage].Count);
        }

        /// <summary>
        /// Verifies that the feature-contract names move with the range exactly as the column allow-list does, so the orchestrator's comparison is against the same names the model binds by.
        /// <para>The defaults give the full 172 names; narrowing the years to 2008..2020 drops the detection and population features of 2021-2025 - exactly 30 - and narrowing the radiuses to 200, 400, 600 drops the two ratio features of the 1000 radius. Widening is the reverse: a superset, so nothing the model needs is ever missing.</para>
        /// </summary>
        [Fact]
        public void YearBuiltPredictionInputColumnNames_Range()
        {
            HashSet<string> names_Default = IO.Query.YearBuiltPredictionInputColumnNames();
            Assert.Equal(172, names_Default.Count);

            HashSet<string> names_NarrowedYears = IO.Query.YearBuiltPredictionInputColumnNames(new Core.Classes.Range<int>(2008, 2020), null);
            Assert.Equal(142, names_NarrowedYears.Count);

            List<string> missing_Years = [.. names_Default.Except(names_NarrowedYears).OrderBy(x => x)];
            Assert.Equal(30, missing_Years.Count);
            foreach (string name in missing_Years)
            {
                //Every dropped name is a detection or population feature of one of 2021-2025
                Assert.Matches("(2021|2022|2023|2024|2025)", name);
            }

            HashSet<string> names_NarrowedRadiuses = IO.Query.YearBuiltPredictionInputColumnNames(null, [200, 400, 600]);
            Assert.Equal(170, names_NarrowedRadiuses.Count);

            //Widening adds nothing the model needs, so the full set is a subset of the widened one
            HashSet<string> names_WidenedYears = IO.Query.YearBuiltPredictionInputColumnNames(new Core.Classes.Range<int>(2005, 2025), null);
            Assert.Empty(names_Default.Except(names_WidenedYears));
        }

        /// <summary>
        /// Verifies that UnpopulatedColumnNames reports a column the table does not carry and a column the table carries as the type default in every row, and reports neither when a single row holds a value.
        /// <para>The two cases are the same defect seen from opposite sides - a scorer reads an absent column and an all-default column identically - and telling them apart is exactly what a run cannot do once the table has reached the model.</para>
        /// </summary>
        [Fact]
        public void UnpopulatedColumnNames_AbsentAndDefault()
        {
            Column column_Reference = new("Reference", typeof(string));
            Column column_Confidence = IO.Create.Column_PredictionYearBuit(IO.Constants.ColumnNamePrefix.PredictionConfidence, 2019);
            Column column_Population = IO.Create.Column_Population(2019);
            Column column_Absent = IO.Create.Column_Population(2020);

            Table table = new([column_Reference, column_Confidence, column_Population]);
            table.AddRow(["A", 0d, 0]);
            table.AddRow(["B", 0d, 0]);

            List<Column> columns = [column_Reference, column_Confidence, column_Population, column_Absent];

            List<string> names_Unpopulated = IO.Query.UnpopulatedColumnNames(table, columns);

            //Zero in every row and absent altogether both count
            Assert.Contains(column_Confidence.Name!, names_Unpopulated);
            Assert.Contains(column_Population.Name!, names_Unpopulated);
            Assert.Contains(column_Absent.Name!, names_Unpopulated);

            //A column carrying text in every row is populated
            Assert.DoesNotContain(column_Reference.Name!, names_Unpopulated);

            //One value anywhere in the column is enough - a detection series is sparse by nature
            Table table_Sparse = new([column_Reference, column_Confidence, column_Population]);
            table_Sparse.AddRow(["A", 0d, 0]);
            table_Sparse.AddRow(["B", 0.87d, 0]);

            List<string> names_Unpopulated_Sparse = IO.Query.UnpopulatedColumnNames(table_Sparse, columns);

            Assert.DoesNotContain(column_Confidence.Name!, names_Unpopulated_Sparse);
            Assert.Contains(column_Population.Name!, names_Unpopulated_Sparse);
            Assert.Contains(column_Absent.Name!, names_Unpopulated_Sparse);
        }

        /// <summary>
        /// Verifies that a null table reports every column asked for, and that a null column collection reports nothing.
        /// <para>A null table carries nothing, so nothing it was asked for is populated - answering an empty list there would read as a clean bill of health for a table that does not exist.</para>
        /// </summary>
        [Fact]
        public void UnpopulatedColumnNames_Nulls()
        {
            List<Column> columns = IO.Query.YearBuiltPredictionFeatureGroups()[IO.Constants.YearBuiltPredictionFeatureGroup.Population];

            List<string> names_NullTable = IO.Query.UnpopulatedColumnNames(null, columns);
            Assert.Equal(columns.Count, names_NullTable.Count);

            List<string> names_NullColumns = IO.Query.UnpopulatedColumnNames(new Table(), null);
            Assert.Empty(names_NullColumns);
        }

        /// <summary>
        /// Verifies that UnpopulatedColumnNames and DefaultOnlyColumnNames answer different questions, so neither can be substituted for the other.
        /// <para>A column holding one county name in every row never varies and is perfectly populated; a detection column holding zero in every row varies just as little and is not.</para>
        /// </summary>
        [Fact]
        public void UnpopulatedColumnNames_NotDefaultOnly()
        {
            Column column_CountyName = IO.Constants.Column.CountyName;
            Column column_Confidence = IO.Create.Column_PredictionYearBuit(IO.Constants.ColumnNamePrefix.PredictionConfidence, 2019);

            Table table = new([column_CountyName, column_Confidence]);
            table.AddRow(["Swinoujscie", 0d]);
            table.AddRow(["Swinoujscie", 0d]);

            List<string> names_Constant = IO.Query.DefaultOnlyColumnNames(table);
            List<string> names_Unpopulated = IO.Query.UnpopulatedColumnNames(table, [column_CountyName, column_Confidence]);

            //Both columns are constant
            Assert.Contains(column_CountyName.Name!, names_Constant);
            Assert.Contains(column_Confidence.Name!, names_Constant);

            //Only one of them carries nothing
            Assert.DoesNotContain(column_CountyName.Name!, names_Unpopulated);
            Assert.Contains(column_Confidence.Name!, names_Unpopulated);
        }
    }
}
