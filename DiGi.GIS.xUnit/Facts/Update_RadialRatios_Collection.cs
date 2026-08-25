using DiGi.Core.IO.Table.Classes;
using DiGi.GIS.Classes;
using DiGi.GIS.IO;
using System;
using System.Collections.Generic;

namespace DiGi.GIS.xUnit
{
    public partial class Facts
    {
        /// <summary>
        /// Verifies that measuring a whole collection of buildings against one set of surroundings in a single call gives the same ratios as measuring each of them on its own.
        /// <para>The collection overload exists so that a caller does not read the surroundings, index the table and resolve every neighbour's area once per building. It reaches each building through a grid index rather than by walking the whole neighbour set, so what has to be proved is that the index does not lose a neighbour that the plain walk would have counted.</para>
        /// <para>The layout is deliberately awkward for a grid: the buildings sit on a line longer than the largest radius, so that subjects and neighbours fall into different cells and a neighbour near a cell edge has to be found from the cell next door.</para>
        /// </summary>
        [Fact]
        public void Update_RadialRatios_Collection()
        {
            int countyId = 456;
            double[] radiuses = [10.0, 30.0];

            List<Building2D> building2Ds = [];

            // A row of ten 10x10 buildings, 25 m apart, so the row is 250 m long against a 30 m largest radius.
            // Storeys vary so that the floor area ratio cannot come out right by accident.
            for (int i = 0; i < 10; i++)
            {
                double x = i * 25.0;

                List<Geometry.Planar.Classes.Point2D> point2Ds =
                [
                    new(x, 0),
                    new(x + 10, 0),
                    new(x + 10, 10),
                    new(x, 10)
                ];

                Geometry.Planar.Classes.PolygonalFace2D? polygonalFace2D = Geometry.Planar.Create.PolygonalFace2D([.. point2Ds]);
                Assert.NotNull(polygonalFace2D);

                building2Ds.Add(new Building2D(Guid.NewGuid(), $"building_{i}", polygonalFace2D, (ushort)(i % 4), Enums.BuildingPhase.occupied, Enums.BuildingGeneralFunction.residential_buildings, []));
            }

            // A neighbour that belongs to no cell of its own, placed just off the end of the row.
            List<Geometry.Planar.Classes.Point2D> point2Ds_Outlier = [new(235, 0), new(245, 0), new(245, 10), new(235, 10)];
            Geometry.Planar.Classes.PolygonalFace2D? polygonalFace2D_Outlier = Geometry.Planar.Create.PolygonalFace2D([.. point2Ds_Outlier]);
            Assert.NotNull(polygonalFace2D_Outlier);

            List<Building2D> building2Ds_Neighbour = [.. building2Ds, new Building2D(Guid.NewGuid(), "building_outlier", polygonalFace2D_Outlier, 5, Enums.BuildingPhase.occupied, Enums.BuildingGeneralFunction.residential_buildings, [])];

            Table table_Collection = new();
            table_Collection.Update_RadialRatios(radiuses, countyId, building2Ds, building2Ds_Neighbour);

            Table table_Single = new();
            foreach (Building2D building2D in building2Ds)
            {
                table_Single.Update_RadialRatios(radiuses, countyId, building2D, building2Ds_Neighbour);
            }

            Assert.Equal(building2Ds.Count, table_Collection.RowCount);
            Assert.Equal(table_Single.RowCount, table_Collection.RowCount);

            Column? column_Reference = table_Collection.GetColumn(table_Collection.GetColumnIndex(IO.Constants.Column.Reference.Name));
            Assert.NotNull(column_Reference);

            foreach (double radius in radiuses)
            {
                Column? column_CoverageRatio_Collection = table_Collection.GetColumn(table_Collection.GetColumnIndex(IO.Create.Column_RadialBuildingCoverageRatio(radius).Name));
                Column? column_FloorAreaRatio_Collection = table_Collection.GetColumn(table_Collection.GetColumnIndex(IO.Create.Column_RadialFloorAreaRatio(radius).Name));
                Column? column_CoverageRatio_Single = table_Single.GetColumn(table_Single.GetColumnIndex(IO.Create.Column_RadialBuildingCoverageRatio(radius).Name));
                Column? column_FloorAreaRatio_Single = table_Single.GetColumn(table_Single.GetColumnIndex(IO.Create.Column_RadialFloorAreaRatio(radius).Name));

                Assert.NotNull(column_CoverageRatio_Collection);
                Assert.NotNull(column_FloorAreaRatio_Collection);
                Assert.NotNull(column_CoverageRatio_Single);
                Assert.NotNull(column_FloorAreaRatio_Single);

                for (int i = 0; i < table_Collection.RowCount; i++)
                {
                    Row? row_Collection = table_Collection.GetRow(i);
                    Row? row_Single = table_Single.GetRow(i);
                    Assert.NotNull(row_Collection);
                    Assert.NotNull(row_Single);

                    Assert.True(row_Collection.TryGetValue(column_Reference.Index, out string? reference_Collection));
                    Assert.True(row_Single.TryGetValue(column_Reference.Index, out string? reference_Single));
                    Assert.Equal(reference_Single, reference_Collection);

                    Assert.True(row_Collection.TryGetValue(column_CoverageRatio_Collection.Index, out float float_CoverageRatio_Collection));
                    Assert.True(row_Single.TryGetValue(column_CoverageRatio_Single.Index, out float float_CoverageRatio_Single));
                    Assert.Equal(float_CoverageRatio_Single, float_CoverageRatio_Collection, 6);

                    Assert.True(row_Collection.TryGetValue(column_FloorAreaRatio_Collection.Index, out float float_FloorAreaRatio_Collection));
                    Assert.True(row_Single.TryGetValue(column_FloorAreaRatio_Single.Index, out float float_FloorAreaRatio_Single));
                    Assert.Equal(float_FloorAreaRatio_Single, float_FloorAreaRatio_Collection, 6);

                    // A ratio of exactly zero everywhere would make the comparison above pass without measuring anything.
                    Assert.True(float_CoverageRatio_Collection > 0f);
                }
            }
        }
    }
}
