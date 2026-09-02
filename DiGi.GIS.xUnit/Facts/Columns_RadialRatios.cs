using DiGi.Core.IO.Table.Classes;
using System.Collections.Generic;

namespace DiGi.GIS.xUnit
{
    public partial class Facts
    {
        /// <summary>
        /// Verifies that Create.Columns_RadialRatios creates properly configured radial coverage and floor area ratio columns across single radiuses and collections.
        /// </summary>
        [Fact]
        public void Columns_RadialRatios()
        {
            List<Column> columns_Single = GIS.IO.Create.Columns_RadialRatios(200.0);
            Assert.NotNull(columns_Single);
            Assert.Equal(2, columns_Single.Count);
            Assert.Equal("Radial Building Coverage Ratio 200m", columns_Single[0].Name);
            Assert.Equal("Radial Floor Area Ratio 200m", columns_Single[1].Name);
            Assert.Equal(typeof(float), ((ExtendedColumn)columns_Single[0]).Type);
            Assert.Equal(typeof(float), ((ExtendedColumn)columns_Single[1]).Type);

            List<Column> columns_Collection = GIS.IO.Create.Columns_RadialRatios((IEnumerable<double>)[200, 400, 600, 1000]);
            Assert.NotNull(columns_Collection);
            Assert.Equal(8, columns_Collection.Count);
            Assert.Equal("Radial Building Coverage Ratio 200m", columns_Collection[0].Name);
            Assert.Equal("Radial Floor Area Ratio 200m", columns_Collection[1].Name);
            Assert.Equal("Radial Building Coverage Ratio 1000m", columns_Collection[6].Name);
            Assert.Equal("Radial Floor Area Ratio 1000m", columns_Collection[7].Name);

            List<Column> columns_Null = GIS.IO.Create.Columns_RadialRatios((IEnumerable<double>?)null);
            Assert.NotNull(columns_Null);
            Assert.Empty(columns_Null);

            Assert.Empty(GIS.IO.Create.Columns_RadialRatios(0));
            Assert.Empty(GIS.IO.Create.Columns_RadialRatios(-100));
            Assert.Empty(GIS.IO.Create.Columns_RadialRatios(double.NaN));
            Assert.Empty(GIS.IO.Create.Columns_RadialRatios(double.PositiveInfinity));
        }
    }
}
