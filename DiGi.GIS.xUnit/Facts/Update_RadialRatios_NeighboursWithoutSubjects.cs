using DiGi.Core.IO.Table.Classes;
using DiGi.Geometry.Planar.Classes;
using DiGi.GIS.Classes;
using DiGi.GIS.IO;
using System;
using System.Collections.Generic;

namespace DiGi.GIS.xUnit
{
    public partial class Facts
    {
        /// <summary>
        /// Pins what <see cref="IO.Modify.Update_RadialRatios(Table, IEnumerable{double}, int, IEnumerable{Building2D}, IEnumerable{Building2D}, double)"/> does when the surroundings it is handed do not contain the buildings it is measuring.
        /// <para>The surroundings are read separately from the subjects, so a read that misses the partition the subjects are filed under hands this method a set the subjects are not in. Neither outcome is an error: an empty set adds no radial column at all, leaving the stored values untouched, and a set holding only distant buildings writes zeros. Both look like data afterwards, which is how twelve multi-part counties came to read as partly filled with zeros while the run that wrote them reported success.</para>
        /// <para>This is documented behaviour rather than a defect to fix here - the caller is what must not hand over surroundings the subjects are missing from, and <c>PostgreSQLBuildingDataUpdateTask</c> now counts it. See https://github.com/ZiolkowskiJakub/DiGi.GIS.PostgreSQL/issues/64.</para>
        /// </summary>
        [Fact]
        public void Update_RadialRatios_NeighboursWithoutSubjects()
        {
            int countyId = 456;
            double[] radiuses = [200.0];

            // The subject: a 10 m square at the origin.
            List<Point2D> point2Ds_Subject = [new(0, 0), new(10, 0), new(10, 10), new(0, 10)];
            PolygonalFace2D? polygonalFace2D_Subject = Geometry.Planar.Create.PolygonalFace2D([.. point2Ds_Subject]);
            Assert.NotNull(polygonalFace2D_Subject);
            Building2D building2D_Subject = new(Guid.NewGuid(), "building_subject", polygonalFace2D_Subject, 2, Enums.BuildingPhase.occupied, Enums.BuildingGeneralFunction.residential_buildings, []);

            // A building 10 km away - inside no radius, so it contributes nothing.
            List<Point2D> point2Ds_Distant = [new(10000, 10000), new(10010, 10000), new(10010, 10010), new(10000, 10010)];
            PolygonalFace2D? polygonalFace2D_Distant = Geometry.Planar.Create.PolygonalFace2D([.. point2Ds_Distant]);
            Assert.NotNull(polygonalFace2D_Distant);
            Building2D building2D_Distant = new(Guid.NewGuid(), "building_distant", polygonalFace2D_Distant, 2, Enums.BuildingPhase.occupied, Enums.BuildingGeneralFunction.residential_buildings, []);

            string? name_CoverageRatio = IO.Create.Column_RadialBuildingCoverageRatio(200.0).Name;
            string? name_FloorAreaRatio = IO.Create.Column_RadialFloorAreaRatio(200.0).Name;
            Assert.NotNull(name_CoverageRatio);
            Assert.NotNull(name_FloorAreaRatio);

            // 1. Empty surroundings: no row, and no radial column either - a push of this table would leave
            // whatever the columns already held in place, which is why the gap does not show up as a NULL.
            Table table_Empty = new();
            table_Empty.Update_RadialRatios(radiuses, countyId, [building2D_Subject], []);

            Assert.Equal(0, table_Empty.RowCount);
            Assert.Equal(-1, table_Empty.GetColumnIndex(name_CoverageRatio));
            Assert.Equal(-1, table_Empty.GetColumnIndex(name_FloorAreaRatio));

            // 2. Surroundings the subject itself is missing from: the columns are written, as zeros. A
            // building always counts towards its own ratios, so a genuine measurement of this subject can
            // never be zero - its own 100 m2 over a 200 m radius is 0.000796.
            Table table_Zero = new();
            table_Zero.Update_RadialRatios(radiuses, countyId, [building2D_Subject], [building2D_Distant]);

            Assert.Equal(1, table_Zero.RowCount);

            Row? row = table_Zero.GetRow(0);
            Assert.NotNull(row);

            Column? column_CoverageRatio = table_Zero.GetColumn(table_Zero.GetColumnIndex(name_CoverageRatio));
            Column? column_FloorAreaRatio = table_Zero.GetColumn(table_Zero.GetColumnIndex(name_FloorAreaRatio));
            Assert.NotNull(column_CoverageRatio);
            Assert.NotNull(column_FloorAreaRatio);

            Assert.True(row.TryGetValue(column_CoverageRatio.Index, out float coverageRatio));
            Assert.True(row.TryGetValue(column_FloorAreaRatio.Index, out float floorAreaRatio));
            Assert.Equal(0.0f, coverageRatio);
            Assert.Equal(0.0f, floorAreaRatio);

            // 3. The control: hand over surroundings that do contain the subject and the ratio is non-zero.
            Table table_Correct = new();
            table_Correct.Update_RadialRatios(radiuses, countyId, [building2D_Subject], [building2D_Subject, building2D_Distant]);

            Row? row_Correct = table_Correct.GetRow(0);
            Assert.NotNull(row_Correct);

            Column? column_CoverageRatio_Correct = table_Correct.GetColumn(table_Correct.GetColumnIndex(name_CoverageRatio));
            Assert.NotNull(column_CoverageRatio_Correct);

            Assert.True(row_Correct.TryGetValue(column_CoverageRatio_Correct.Index, out float coverageRatio_Correct));
            Assert.True(coverageRatio_Correct > 0.0f);
        }
    }
}
