using DiGi.Core;
using DiGi.Core.Classes;
using DiGi.Core.IO.Table.Classes;
using System.Collections.Generic;
using System.Linq;

namespace DiGi.GIS.xUnit
{
    public partial class Facts
    {
        /// <summary>
        /// Verifies that YearBuiltPredictionInputColumns never includes any column from YearBuiltPredictionOutputColumns or their unique identifiers, preventing target leakage.
        /// </summary>
        [Fact]
        public void YearBuiltPredictionColumns_Disjointness()
        {
            List<Column> inputColumns = GIS.IO.Query.YearBuiltPredictionInputColumns();
            Assert.NotNull(inputColumns);
            Assert.NotEmpty(inputColumns);

            List<Column> outputColumns = GIS.IO.Query.YearBuiltPredictionOutputColumns();
            Assert.NotNull(outputColumns);
            Assert.NotEmpty(outputColumns);

            foreach (Column outputColumn in outputColumns)
            {
                Assert.DoesNotContain(outputColumn, inputColumns);
                Assert.DoesNotContain(inputColumns, c => c.Name == outputColumn.Name);
                Assert.DoesNotContain(inputColumns, c => c.UniqueId() == outputColumn.UniqueId());
            }
        }

        /// <summary>
        /// Verifies that YearBuiltPredictionInputColumns includes all expected geometric, administrative, grid cell, and detection features.
        /// </summary>
        [Fact]
        public void YearBuiltPredictionColumns_Completeness()
        {
            List<Column> columns = GIS.IO.Query.YearBuiltPredictionInputColumns();
            Assert.NotNull(columns);

            // 31 core features + 25 grid cells + (18 years * 5 detections) + 18 population = 164 columns
            Assert.Equal(164, columns.Count);

            Assert.Contains(columns, c => c.Name == "Floor area");
            Assert.Contains(columns, c => c.Name == "Total area");
            Assert.Contains(columns, c => c.Name == "Storeys");
            Assert.Contains(columns, c => c.Name == "Internal Point X");
            Assert.Contains(columns, c => c.Name == "Internal Point Y");
            Assert.Contains(columns, c => c.Name == "Voivodeship name");
            Assert.Contains(columns, c => c.Name == "County Id");
            Assert.Contains(columns, c => c.Name == "County name");
            Assert.Contains(columns, c => c.Name == "Building general function");
            Assert.Contains(columns, c => c.Name == "Subdivision occupancy");
            Assert.Contains(columns, c => c.Name == "Calculated occupancy");

            // Grid cells
            Assert.Contains(columns, c => c.Name == "Grid cell coverage [0,0]");
            Assert.Contains(columns, c => c.Name == "Grid cell coverage [4,4]");

            // Detections 2008..2025
            Assert.Contains(columns, c => c.Name == "Prediction Confidence 2008");
            Assert.Contains(columns, c => c.Name == "Prediction Confidence 2025");
            Assert.Contains(columns, c => c.Name == "Prediction BoundingBox Height 2025");

            // Population 2008..2025
            Assert.Contains(columns, c => c.Name == "Municipality population 2008");
            Assert.Contains(columns, c => c.Name == "Municipality population 2025");
        }

        /// <summary>
        /// Verifies that all columns returned by YearBuiltPredictionInputColumns have valid and distinct unique identifiers.
        /// </summary>
        [Fact]
        public void YearBuiltPredictionColumns_UniqueIds()
        {
            List<Column> columns = GIS.IO.Query.YearBuiltPredictionInputColumns();
            Assert.NotNull(columns);

            HashSet<string> uniqueIds = [];
            foreach (Column column in columns)
            {
                string? uniqueId = column.UniqueId();
                Assert.False(string.IsNullOrWhiteSpace(uniqueId));
                Assert.True(uniqueIds.Add(uniqueId!), $"Duplicate column UniqueId found: {uniqueId} for column {column.Name}");
            }
        }
    }
}
