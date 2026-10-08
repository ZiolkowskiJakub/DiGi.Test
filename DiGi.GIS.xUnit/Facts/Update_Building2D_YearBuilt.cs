using DiGi.Core.IO.Table.Classes;
using DiGi.GIS.Classes;
using DiGi.GIS.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DiGi.GIS.xUnit
{
    public partial class Facts
    {
        /// <summary>
        /// Verifies that Update_Building2D_YearBuilt writes the three year built columns of each building, appends a reference the table does not hold yet, leaves a bounds-only building alone, and returns the number of rows it gave a value.
        /// <para>A building may hold several stored YearBuiltData records, so the fact also covers the most frequent entry winning across records that share a reference - with the majority beating the newest, the answer the latest rule would have given differently.</para>
        /// </summary>
        [Fact]
        public void Update_Building2D_YearBuilt()
        {
            int countyId = 2212;

            //b_ref_001: two 1975 predictions (older and newest stamp) beat the single newer 1981
            YearBuiltData yearBuiltData_1975_Older = new("b_ref_001");
            yearBuiltData_1975_Older.SetPredictedYearBuilt(new DateTime(2025, 1, 1), 1975);

            YearBuiltData yearBuiltData_1981 = new("b_ref_001");
            yearBuiltData_1981.SetPredictedYearBuilt(new DateTime(2026, 6, 1), 1981);

            YearBuiltData yearBuiltData_1975_Newest = new("b_ref_001");
            yearBuiltData_1975_Newest.SetPredictedYearBuilt(new DateTime(2026, 12, 1), 1975);

            YearBuiltData yearBuiltData_Appended = new("b_ref_002");
            yearBuiltData_Appended.SetPredictedYearBuilt(new DateTime(2026, 6, 1), 2003);

            YearBuiltData yearBuiltData_UserOnly = new("b_ref_003");
            yearBuiltData_UserOnly.SetUserYearBuilt(1960);

            //b_ref_004: a bound is a bound rather than a year and there is no prediction either, so no column gets a value and no row is left behind
            YearBuiltData yearBuiltData_BoundOnly = new("b_ref_004");
            yearBuiltData_BoundOnly.SetUserYearBuilt(new UserYearBuilt((short)1950, Enums.YearBuiltRelation.AtOrBefore));

            Table table = new();

            Column? column_Reference = table.AddColumn(IO.Constants.Column.Reference);
            Column? column_CountyId = table.AddColumn(IO.Constants.Column.CountyId);
            Assert.NotNull(column_Reference);
            Assert.NotNull(column_CountyId);

            foreach (string reference in new string[] { "b_ref_001", "b_ref_003" })
            {
                Row row = table.AddRow();
                IO.Modify.SetValue(row, column_Reference, reference);
                IO.Modify.SetValue(row, column_CountyId, countyId);
                table.AddRow(row, false);
            }

            Assert.Equal(2, table.RowCount);

            int count = IO.Modify.Update_Building2D_YearBuilt(table, countyId, [yearBuiltData_1975_Older, yearBuiltData_1981, yearBuiltData_1975_Newest, yearBuiltData_Appended, yearBuiltData_UserOnly, yearBuiltData_BoundOnly]);

            //b_ref_002 was not in the table and is appended; b_ref_003 stays as the row it already was; b_ref_004 leaves no row at all
            Assert.Equal(3, table.RowCount);

            //The count is the rows given at least one of the three values: b_ref_001 updated, b_ref_002 appended and b_ref_003, whose user and calculated columns are written
            Assert.Equal(3, count);

            Column? column_PredictedYearBuilt = table.Columns?.FirstOrDefault(x => x.Name == IO.Constants.Column.PredictedYearBuilt.Name);
            Column? column_UserYearBuilt = table.Columns?.FirstOrDefault(x => x.Name == IO.Constants.Column.UserYearBuilt.Name);
            Column? column_CalculatedYearBuilt = table.Columns?.FirstOrDefault(x => x.Name == IO.Constants.Column.CalculatedYearBuilt.Name);
            Assert.NotNull(column_PredictedYearBuilt);
            Assert.NotNull(column_UserYearBuilt);
            Assert.NotNull(column_CalculatedYearBuilt);

            Dictionary<string, Row> dictionary = [];
            for (int i = 0; i < table.RowCount; i++)
            {
                Row? row = table.GetRow(i);
                Assert.NotNull(row);
                Assert.True(row.TryGetValue(column_Reference.Index, out string? reference));
                Assert.NotNull(reference);
                dictionary[reference!] = row;
            }

            Assert.False(dictionary.ContainsKey("b_ref_004"), "A bounds-only building must not leave a row behind.");

            //The majority beats the newest: the two 1975 predictions win over the single 1981 the latest rule would have written
            Assert.True(dictionary["b_ref_001"].TryGetValue(column_PredictedYearBuilt.Index, out ushort year_001));
            Assert.Equal((ushort)1975, year_001);
            Assert.False(dictionary["b_ref_001"].TryGetValue(column_UserYearBuilt.Index, out ushort _));
            Assert.True(dictionary["b_ref_001"].TryGetValue(column_CalculatedYearBuilt.Index, out ushort calculated_001));
            Assert.Equal((ushort)1975, calculated_001);

            Assert.True(dictionary["b_ref_002"].TryGetValue(column_PredictedYearBuilt.Index, out ushort year_002));
            Assert.Equal((ushort)2003, year_002);
            Assert.False(dictionary["b_ref_002"].TryGetValue(column_UserYearBuilt.Index, out ushort _));
            Assert.True(dictionary["b_ref_002"].TryGetValue(column_CalculatedYearBuilt.Index, out ushort calculated_002));
            Assert.Equal((ushort)2003, calculated_002);
            Assert.True(dictionary["b_ref_002"].TryGetValue(column_CountyId.Index, out int countyId_002));
            Assert.Equal(countyId, countyId_002);

            //A user-supplied exact year reaches the user and the calculated column, never the prediction column
            Assert.False(dictionary["b_ref_003"].TryGetValue(column_PredictedYearBuilt.Index, out ushort _));
            Assert.True(dictionary["b_ref_003"].TryGetValue(column_UserYearBuilt.Index, out ushort year_003));
            Assert.Equal((ushort)1960, year_003);
            Assert.True(dictionary["b_ref_003"].TryGetValue(column_CalculatedYearBuilt.Index, out ushort calculated_003));
            Assert.Equal((ushort)1960, calculated_003);

            //A county the rows do not belong to leaves the table untouched
            Table table_OtherCounty = new();
            Assert.Equal(0, IO.Modify.Update_Building2D_YearBuilt(table_OtherCounty, countyId, null));
            Assert.Equal(0, table_OtherCounty.RowCount);
        }

        /// <summary>
        /// Verifies that the buildings named through the references parameter are emitted whatever their history holds, so pushing the table clears the columns of a building whose history no longer has a value.
        /// <para>Covers a named building with no stored record, a named building whose only record is empty, a named building whose row the table already held with stale values, and a named building that still has a value - and that leaving the parameter out keeps the old output.</para>
        /// </summary>
        [Fact]
        public void Update_Building2D_YearBuilt_References()
        {
            int countyId = 2212;

            YearBuiltData yearBuiltData_Value = new("b_ref_value");
            yearBuiltData_Value.SetPredictedYearBuilt(new DateTime(2026, 6, 1), 1990);

            //An object emptied by a removal: still stored, no entry left
            YearBuiltData yearBuiltData_Empty = new("b_ref_empty");

            Table table = new();

            Column? column_Reference = table.AddColumn(IO.Constants.Column.Reference);
            Column? column_CountyId = table.AddColumn(IO.Constants.Column.CountyId);
            Column? column_PredictedYearBuilt = table.AddColumn(IO.Constants.Column.PredictedYearBuilt);
            Column? column_UserYearBuilt = table.AddColumn(IO.Constants.Column.UserYearBuilt);
            Column? column_CalculatedYearBuilt = table.AddColumn(IO.Constants.Column.CalculatedYearBuilt);
            Assert.NotNull(column_Reference);
            Assert.NotNull(column_CountyId);
            Assert.NotNull(column_PredictedYearBuilt);
            Assert.NotNull(column_UserYearBuilt);
            Assert.NotNull(column_CalculatedYearBuilt);

            //b_ref_stale: the table already holds the row with the values of a prediction run that has since been removed
            Row row_Stale = table.AddRow();
            IO.Modify.SetValue(row_Stale, column_Reference, "b_ref_stale");
            IO.Modify.SetValue(row_Stale, column_CountyId, countyId);
            IO.Modify.SetValue(row_Stale, column_PredictedYearBuilt, (ushort)1955);
            IO.Modify.SetValue(row_Stale, column_CalculatedYearBuilt, (ushort)1955);
            table.AddRow(row_Stale, false);

            int count = IO.Modify.Update_Building2D_YearBuilt(table, countyId, [yearBuiltData_Value, yearBuiltData_Empty], ["b_ref_value", "b_ref_empty", "b_ref_missing", "b_ref_stale"]);

            //Only b_ref_value is given a value
            Assert.Equal(1, count);
            Assert.Equal(4, table.RowCount);

            Dictionary<string, Row> dictionary = [];
            for (int i = 0; i < table.RowCount; i++)
            {
                Row? row = table.GetRow(i);
                Assert.NotNull(row);
                Assert.True(row.TryGetValue(column_Reference.Index, out string? reference));
                Assert.NotNull(reference);
                dictionary[reference!] = row;
            }

            Assert.True(dictionary["b_ref_value"].TryGetValue(column_PredictedYearBuilt.Index, out ushort year_Value));
            Assert.Equal((ushort)1990, year_Value);

            foreach (string reference in new string[] { "b_ref_empty", "b_ref_missing", "b_ref_stale" })
            {
                Assert.False(dictionary[reference].TryGetValue(column_PredictedYearBuilt.Index, out ushort _), reference);
                Assert.False(dictionary[reference].TryGetValue(column_UserYearBuilt.Index, out ushort _), reference);
                Assert.False(dictionary[reference].TryGetValue(column_CalculatedYearBuilt.Index, out ushort _), reference);
                Assert.True(dictionary[reference].TryGetValue(column_CountyId.Index, out int countyId_Row), reference);
                Assert.Equal(countyId, countyId_Row);
            }

            //Without the parameter an empty history still leaves no row and no columns behind
            Table table_Default = new();
            Assert.Equal(0, IO.Modify.Update_Building2D_YearBuilt(table_Default, countyId, [yearBuiltData_Empty]));
            Assert.Equal(0, table_Default.RowCount);

            //Named buildings alone are enough: the three columns are added even with no history at all
            Table table_NoHistory = new();
            Assert.Equal(0, IO.Modify.Update_Building2D_YearBuilt(table_NoHistory, countyId, null, ["b_ref_missing"]));
            Assert.Equal(1, table_NoHistory.RowCount);
            Assert.NotNull(table_NoHistory.Columns?.FirstOrDefault(x => x.Name == IO.Constants.Column.CalculatedYearBuilt.Name));
        }

        /// <summary>
        /// Pins the defect that made the stored predicted year built branch of PostgreSQLBuildingDataUpdateTask unable to write anything.
        /// <para>The branch projected stored records through ToDiGi, which yields IYearBuiltData, and then filtered them with OfType&lt;Building2DYearBuiltPredictions&gt;. Building2DYearBuiltPredictions does not implement IYearBuiltData, so the filter compiled and evaluated to nothing on every call. YearBuiltData is the type such a projection actually yields.</para>
        /// </summary>
        [Fact]
        public void Update_Building2D_YearBuilt_ProjectionContract()
        {
            YearBuiltData yearBuiltData = new("b_ref_001");
            yearBuiltData.SetPredictedYearBuilt(new DateTime(2026, 6, 1), 1981);

            List<IYearBuiltData> yearBuiltDatas = [yearBuiltData];

            Assert.Empty(yearBuiltDatas.OfType<Building2DYearBuiltPredictions>());
            Assert.Single(yearBuiltDatas.OfType<YearBuiltData>());

            Assert.False(typeof(IYearBuiltData).IsAssignableFrom(typeof(Building2DYearBuiltPredictions)));
            Assert.True(typeof(IYearBuiltData).IsAssignableFrom(typeof(YearBuiltData)));
        }
    }
}
