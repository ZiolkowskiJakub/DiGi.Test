using DiGi.Core.Classes;
using DiGi.Core.IO.Table.Classes;
using System.Collections.Generic;

namespace DiGi.GIS.IO.xUnit
{
    public partial class Facts
    {
        /// <summary>
        /// Pins the first detection year to the rule the evaluation app inlines: the first year of the range with a confidence above zero, else the default, one entry per row in row order.
        /// <para>The fixture table carries the real column names through Create.Column_PredictionYearBuit, so the lookup cannot drift from the stored shape.</para>
        /// </summary>
        [Fact]
        public void FirstDetectionYears()
        {
            Table table = new();
            table.AddColumn(Constants.Column.Reference);
            for (int year = 2008; year <= 2012; year++)
            {
                table.AddColumn(Create.Column_PredictionYearBuit(Constants.ColumnNamePrefix.PredictionConfidence, year));
            }

            // R1: 2009 is the first year above zero.
            table.AddRow(["R1", 0.0, 0.7, 0.0, 0.0, 0.0]);
            // R2: nothing above zero - the default year.
            table.AddRow(["R2", 0.0, 0.0, 0.0, 0.0, 0.0]);
            // R3: the first year of the range wins outright.
            table.AddRow(["R3", 0.5, 0.0, 0.0, 0.0, 0.0]);
            // R4: the last year of the range, and a blank reference - the row still counts, aligned by index.
            table.AddRow(["", 0.0, 0.0, 0.0, 0.0, 1.0]);

            List<short> years = Query.FirstDetectionYears(table);

            Assert.Equal(4, years.Count);
            Assert.Equal(2009, years[0]);
            Assert.Equal(2008, years[1]);
            Assert.Equal(2008, years[2]);
            Assert.Equal(2012, years[3]);
        }

        /// <summary>
        /// Pins the range handling: years absent from the table are skipped, a narrower range ignores the columns it does not name, and year_Default replaces the 2008 fallback.
        /// </summary>
        [Fact]
        public void FirstDetectionYears_RangeAndDefault()
        {
            // The table carries 2008..2010 only; the default range 2008..2025 must skip the absent years.
            Table table = new();
            table.AddColumn(Constants.Column.Reference);
            for (int year = 2008; year <= 2010; year++)
            {
                table.AddColumn(Create.Column_PredictionYearBuit(Constants.ColumnNamePrefix.PredictionConfidence, year));
            }

            table.AddRow(["R1", 0.0, 1.0, 0.0]);
            table.AddRow(["R2", 0.0, 0.0, 0.0]);

            List<short> years = Query.FirstDetectionYears(table);
            Assert.Equal(2, years.Count);
            Assert.Equal(2009, years[0]);
            Assert.Equal(2008, years[1]);

            // year_Default replaces the 2008 fallback for a row with no hit, and does not touch a row with one.
            List<short> years_CustomDefault = Query.FirstDetectionYears(table, year_Default: 1999);
            Assert.Equal(2009, years_CustomDefault[0]);
            Assert.Equal(1999, years_CustomDefault[1]);

            // A narrower range than the columns ignores the earlier hit: 2009 is out of range, so 2010 and 2011 decide the two rows.
            Table table_Wide = new();
            table_Wide.AddColumn(Constants.Column.Reference);
            for (int year = 2008; year <= 2012; year++)
            {
                table_Wide.AddColumn(Create.Column_PredictionYearBuit(Constants.ColumnNamePrefix.PredictionConfidence, year));
            }

            table_Wide.AddRow(["R1", 0.0, 1.0, 1.0, 0.0, 0.0]);
            table_Wide.AddRow(["R2", 0.0, 1.0, 0.0, 1.0, 0.0]);

            List<short> years_Narrow = Query.FirstDetectionYears(table_Wide, new Range<int>(2010, 2011));
            Assert.Equal(2, years_Narrow.Count);
            Assert.Equal(2010, years_Narrow[0]);
            Assert.Equal(2011, years_Narrow[1]);
        }

        /// <summary>
        /// Pins the absent-input handling: a null table and a table with no rows give an empty result rather than throwing.
        /// </summary>
        [Fact]
        public void FirstDetectionYears_NullAndEmpty()
        {
            Assert.Empty(Query.FirstDetectionYears(null));
            Assert.Empty(Query.FirstDetectionYears(new Table()));
        }
    }
}
