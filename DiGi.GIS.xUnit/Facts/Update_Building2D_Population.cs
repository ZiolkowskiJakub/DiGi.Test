using DiGi.Core.Classes;
using DiGi.Core.IO.Table.Classes;
using DiGi.GIS.Classes;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DiGi.GIS.xUnit
{
    public partial class Facts
    {
        /// <summary>
        /// Verifies that Update_Building2D_Population populates population series across buildings from StatisticalYearlyDoubleData and StatisticalDataCollection.
        /// </summary>
        [Fact]
        public void Update_Building2D_Population()
        {
            Building2D building2D_1 = new(Guid.NewGuid(), "b_ref_001", null, 2, null, null, []);
            Building2D building2D_2 = new(Guid.NewGuid(), "b_ref_002", null, 1, null, null, []);

            List<KeyValuePair<short, double>> values =
            [
                new(2020, 15000.0),
                new(2021, 15500.0),
                new(2022, 16000.0)
            ];

            StatisticalYearlyDoubleData statisticalYearlyDoubleData = new("Population", "Population", values);

            Table table = new();
            int countyId = 2212;

            GIS.IO.Modify.Update_Building2D_Population(table, countyId, [building2D_1, building2D_2], statisticalYearlyDoubleData, new Range<int>(2020, 2022));

            Assert.Equal(2, table.RowCount);

            Column? column_Reference = table.Columns?.FirstOrDefault(c => c.Name == GIS.IO.Constants.Column.Reference.Name);
            Column? column_CountyId = table.Columns?.FirstOrDefault(c => c.Name == GIS.IO.Constants.Column.CountyId.Name);
            Column? column_Pop2020 = table.Columns?.FirstOrDefault(c => c.Name == "Municipality population 2020");
            Column? column_Pop2021 = table.Columns?.FirstOrDefault(c => c.Name == "Municipality population 2021");
            Column? column_Pop2022 = table.Columns?.FirstOrDefault(c => c.Name == "Municipality population 2022");

            Assert.NotNull(column_Reference);
            Assert.NotNull(column_CountyId);
            Assert.NotNull(column_Pop2020);
            Assert.NotNull(column_Pop2021);
            Assert.NotNull(column_Pop2022);

            Row? row_1 = table.GetRow(0);
            Assert.NotNull(row_1);
            Assert.True(row_1.TryGetValue(column_Reference.Index, out string? ref_1));
            Assert.Equal("b_ref_001", ref_1);
            Assert.True(row_1.TryGetValue(column_CountyId.Index, out int cId_1));
            Assert.Equal(countyId, cId_1);
            Assert.True(row_1.TryGetValue(column_Pop2020.Index, out int pop2020_1));
            Assert.Equal(15000, pop2020_1);
            Assert.True(row_1.TryGetValue(column_Pop2021.Index, out int pop2021_1));
            Assert.Equal(15500, pop2021_1);
            Assert.True(row_1.TryGetValue(column_Pop2022.Index, out int pop2022_1));
            Assert.Equal(16000, pop2022_1);

            // Test StatisticalDataCollection overload
            StatisticalDataCollection collection = new(Guid.NewGuid(), new UnitCode("012345678901"));
            collection.Add(statisticalYearlyDoubleData);

            Table table_Collection = new();
            GIS.IO.Modify.Update_Building2D_Population(table_Collection, countyId, [building2D_1], collection, new Range<int>(2020, 2021));

            Assert.Equal(1, table_Collection.RowCount);
            Row? row_Coll = table_Collection.GetRow(0);
            Assert.NotNull(row_Coll);

            Column? column_CollPop2020 = table_Collection.Columns?.FirstOrDefault(c => c.Name == "Municipality population 2020");
            Assert.NotNull(column_CollPop2020);
            Assert.True(row_Coll.TryGetValue(column_CollPop2020.Index, out int collPop2020));
            Assert.Equal(15000, collPop2020);

            // Null guards
            Table? table_Null = null;
            GIS.IO.Modify.Update_Building2D_Population(table_Null, countyId, [building2D_1], statisticalYearlyDoubleData);
            GIS.IO.Modify.Update_Building2D_Population(table, countyId, null, statisticalYearlyDoubleData);
            GIS.IO.Modify.Update_Building2D_Population(table, countyId, [], statisticalYearlyDoubleData);
            GIS.IO.Modify.Update_Building2D_Population(table, countyId, [building2D_1], (StatisticalYearlyDoubleData?)null);
            GIS.IO.Modify.Update_Building2D_Population(table, countyId, [building2D_1], (StatisticalDataCollection?)null);
        }
    }
}
