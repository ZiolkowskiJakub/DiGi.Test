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
        /// Verifies that Update_Building2D_PredictedYearBuilt writes the latest predicted year of each building, appends a reference the table does not hold yet, and leaves a building carrying no prediction alone.
        /// <para>A building may hold several stored YearBuiltData records, so the fact also covers the newest prediction winning across records that share a reference.</para>
        /// </summary>
        [Fact]
        public void Update_Building2D_PredictedYearBuilt()
        {
            int countyId = 2212;

            YearBuiltData yearBuiltData_Older = new("b_ref_001");
            yearBuiltData_Older.SetPredictedYearBuilt(new DateTime(2025, 1, 1), 1975);

            YearBuiltData yearBuiltData_Newer = new("b_ref_001");
            yearBuiltData_Newer.SetPredictedYearBuilt(new DateTime(2026, 6, 1), 1981);

            YearBuiltData yearBuiltData_Appended = new("b_ref_002");
            yearBuiltData_Appended.SetPredictedYearBuilt(new DateTime(2026, 6, 1), 2003);

            YearBuiltData yearBuiltData_UserOnly = new("b_ref_003");
            yearBuiltData_UserOnly.SetUserYearBuilt(1960);

            Table table = new();

            Column? column_Reference = table.AddColumn(GIS.IO.Constants.Column.Reference);
            Column? column_CountyId = table.AddColumn(GIS.IO.Constants.Column.CountyId);
            Assert.NotNull(column_Reference);
            Assert.NotNull(column_CountyId);

            foreach (string reference in new string[] { "b_ref_001", "b_ref_003" })
            {
                Row row = table.AddRow();
                GIS.IO.Modify.SetValue(row, column_Reference, reference);
                GIS.IO.Modify.SetValue(row, column_CountyId, countyId);
                table.AddRow(row, false);
            }

            Assert.Equal(2, table.RowCount);

            GIS.IO.Modify.Update_Building2D_PredictedYearBuilt(table, countyId, [yearBuiltData_Older, yearBuiltData_Newer, yearBuiltData_Appended, yearBuiltData_UserOnly]);

            //b_ref_002 was not in the table and is appended; b_ref_003 stays as the row it already was
            Assert.Equal(3, table.RowCount);

            Column? column_PredictedYearBuilt = table.Columns?.FirstOrDefault(x => x.Name == GIS.IO.Constants.Column.PredictedYearBuilt.Name);
            Assert.NotNull(column_PredictedYearBuilt);

            Dictionary<string, Row> dictionary = [];
            for (int i = 0; i < table.RowCount; i++)
            {
                Row? row = table.GetRow(i);
                Assert.NotNull(row);
                Assert.True(row.TryGetValue(column_Reference.Index, out string? reference));
                Assert.NotNull(reference);
                dictionary[reference!] = row;
            }

            //The newer of the two records for b_ref_001 is the one written
            Assert.True(dictionary["b_ref_001"].TryGetValue(column_PredictedYearBuilt.Index, out ushort year_001));
            Assert.Equal(1981, year_001);

            Assert.True(dictionary["b_ref_002"].TryGetValue(column_PredictedYearBuilt.Index, out ushort year_002));
            Assert.Equal(2003, year_002);
            Assert.True(dictionary["b_ref_002"].TryGetValue(column_CountyId.Index, out int countyId_002));
            Assert.Equal(countyId, countyId_002);

            //A user-supplied year is not a prediction and must not reach the column
            Assert.False(dictionary["b_ref_003"].TryGetValue(column_PredictedYearBuilt.Index, out ushort _));

            //A county the rows do not belong to leaves the table untouched
            Table table_OtherCounty = new();
            GIS.IO.Modify.Update_Building2D_PredictedYearBuilt(table_OtherCounty, countyId, null);
            Assert.Equal(0, table_OtherCounty.RowCount);
        }

        /// <summary>
        /// Pins the defect that made the stored predicted year built branch of PostgreSQLBuildingDataUpdateTask unable to write anything.
        /// <para>The branch projected stored records through ToDiGi, which yields IYearBuiltData, and then filtered them with OfType&lt;Building2DYearBuiltPredictions&gt;. Building2DYearBuiltPredictions does not implement IYearBuiltData, so the filter compiled and evaluated to nothing on every call. YearBuiltData is the type such a projection actually yields.</para>
        /// </summary>
        [Fact]
        public void Update_Building2D_PredictedYearBuilt_ProjectionContract()
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
