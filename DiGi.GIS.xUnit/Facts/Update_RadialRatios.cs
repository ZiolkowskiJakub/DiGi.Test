using DiGi.Core.IO.Table.Classes;
using DiGi.Geometry.Planar;
using DiGi.Geometry.Planar.Classes;
using DiGi.GIS.Classes;
using DiGi.GIS.IO;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace DiGi.GIS.xUnit
{
    public partial class Facts
    {
        /// <summary>
        /// Verifies that the <see cref="DiGi.GIS.IO.Modify.Update_RadialRatios"/> method correctly computes and updates radial ratios in a table.
        /// <para>This tests both the GMF-based model loading and a mock-based exact mathematical verification.</para>
        /// </summary>
        [Fact]
        public void Update_RadialRatios()
        {
            // 1. Math and logic verification using a mock dataset
            Table table_Mock = new();
            int countyId_Mock = 456;
            double[] doubles_RadiusesMock = [10.0, 30.0];

            List<Building2D> list_MockBuildings = [];
            Guid guid_Target = Guid.NewGuid();
            string reference_Target = "building_target";
            // Centered at (5, 5), Area = 100.0
            List<Point2D> point2Ds_Target = [new(0, 0), new(10, 0), new(10, 10), new(0, 10)];
            PolygonalFace2D? polygonalFace2D_Target = Geometry.Planar.Create.PolygonalFace2D(point2Ds_Target.ToArray());
            Assert.NotNull(polygonalFace2D_Target);
            Building2D building2D_Target = new(guid_Target, reference_Target, polygonalFace2D_Target, 2, Enums.BuildingPhase.occupied, Enums.BuildingGeneralFunction.residential_buildings, []);
            list_MockBuildings.Add(building2D_Target);

            // Neighbour 1: Centered at (25, 5), Area = 100.0, closest point at (20, 5), distance from target centroid (5, 5) = 15.0
            Guid guid_Neighbour1 = Guid.NewGuid();
            string reference_Neighbour1 = "building_neighbour1";
            List<Point2D> point2Ds_Neighbour1 = [new(20, 0), new(30, 0), new(30, 10), new(20, 10)];
            PolygonalFace2D? polygonalFace2D_Neighbour1 = Geometry.Planar.Create.PolygonalFace2D(point2Ds_Neighbour1.ToArray());
            Assert.NotNull(polygonalFace2D_Neighbour1);
            Building2D building2D_Neighbour1 = new(guid_Neighbour1, reference_Neighbour1, polygonalFace2D_Neighbour1, 3, Enums.BuildingPhase.occupied, Enums.BuildingGeneralFunction.residential_buildings, []);
            list_MockBuildings.Add(building2D_Neighbour1);

            // Neighbour 2: Centered at (5, 25), Area = 100.0, closest point at (5, 20), distance from target centroid (5, 5) = 15.0
            // Has 0 storeys, should fallback to 1 storey
            Guid guid_Neighbour2 = Guid.NewGuid();
            string reference_Neighbour2 = "building_neighbour2";
            List<Point2D> point2Ds_Neighbour2 = [new(0, 20), new(10, 20), new(10, 30), new(0, 30)];
            PolygonalFace2D? polygonalFace2D_Neighbour2 = Geometry.Planar.Create.PolygonalFace2D(point2Ds_Neighbour2.ToArray());
            Assert.NotNull(polygonalFace2D_Neighbour2);
            Building2D building2D_Neighbour2 = new(guid_Neighbour2, reference_Neighbour2, polygonalFace2D_Neighbour2, 0, Enums.BuildingPhase.occupied, Enums.BuildingGeneralFunction.residential_buildings, []);
            list_MockBuildings.Add(building2D_Neighbour2);

            // Run update to create a row in the table
            table_Mock.Update_RadialRatios(doubles_RadiusesMock, countyId_Mock, building2D_Target, list_MockBuildings);

            Assert.Equal(1, table_Mock.RowCount);
            Row? row_Mock = table_Mock.GetRow(0);
            Assert.NotNull(row_Mock);

            Column? column_Reference = table_Mock.GetColumn(table_Mock.GetColumnIndex(IO.Constants.Column.Reference.Name));
            Column? column_CountyId = table_Mock.GetColumn(table_Mock.GetColumnIndex(IO.Constants.Column.CountyId.Name));
            Assert.NotNull(column_Reference);
            Assert.NotNull(column_CountyId);

            Assert.True(row_Mock.TryGetValue(column_Reference.Index, out string? reference_Row));
            Assert.True(row_Mock.TryGetValue(column_CountyId.Index, out int countyId_Row));
            Assert.Equal(reference_Target, reference_Row);
            Assert.Equal(countyId_Mock, countyId_Row);

            Column? column_CoverageRatio10 = table_Mock.GetColumn(table_Mock.GetColumnIndex(IO.Create.Column_RadialBuildingCoverageRatio(10.0).Name));
            Column? column_FloorAreaRatio10 = table_Mock.GetColumn(table_Mock.GetColumnIndex(IO.Create.Column_RadialFloorAreaRatio(10.0).Name));
            Column? column_CoverageRatio30 = table_Mock.GetColumn(table_Mock.GetColumnIndex(IO.Create.Column_RadialBuildingCoverageRatio(30.0).Name));
            Column? column_FloorAreaRatio30 = table_Mock.GetColumn(table_Mock.GetColumnIndex(IO.Create.Column_RadialFloorAreaRatio(30.0).Name));

            Assert.NotNull(column_CoverageRatio10);
            Assert.NotNull(column_FloorAreaRatio10);
            Assert.NotNull(column_CoverageRatio30);
            Assert.NotNull(column_FloorAreaRatio30);

            Assert.True(row_Mock.TryGetValue(column_CoverageRatio10.Index, out float float_CoverageRatio10));
            Assert.True(row_Mock.TryGetValue(column_FloorAreaRatio10.Index, out float float_FloorAreaRatio10));
            Assert.True(row_Mock.TryGetValue(column_CoverageRatio30.Index, out float float_CoverageRatio30));
            Assert.True(row_Mock.TryGetValue(column_FloorAreaRatio30.Index, out float float_FloorAreaRatio30));

            // Radius 10 expectations: circular area = 100 * PI. Only target building is within radius.
            // Expected coverage area = 100.0. Coverage ratio = 100.0 / (100 * PI) = 1 / PI
            // Expected floor area = 100.0 * 2 = 200.0. Floor area ratio = 200.0 / (100 * PI) = 2 / PI
            double double_CircularArea10 = Math.PI * 100.0;
            float float_ExpectedCoverage10 = (float)(100.0 / double_CircularArea10);
            float float_ExpectedFloor10 = (float)(200.0 / double_CircularArea10);
            Assert.Equal(float_ExpectedCoverage10, float_CoverageRatio10, 4);
            Assert.Equal(float_ExpectedFloor10, float_FloorAreaRatio10, 4);

            // Radius 30 expectations: circular area = 900 * PI. All 3 buildings within radius (since distances are 0, 15, 15).
            // Expected coverage area = 100.0 (Target) + 100.0 (N1) + 100.0 (N2) = 300.0
            // Expected floor area = 100.0 * 2 (Target) + 100.0 * 3 (N1) + 100.0 * 1 (N2 fallback) = 600.0
            // Coverage ratio = 300.0 / (900 * PI) = 1 / (3 * PI)
            // Floor area ratio = 600.0 / (900 * PI) = 2 / (3 * PI)
            double double_CircularArea30 = Math.PI * 900.0;
            float float_ExpectedCoverage30 = (float)(300.0 / double_CircularArea30);
            float float_ExpectedFloor30 = (float)(600.0 / double_CircularArea30);
            Assert.Equal(float_ExpectedCoverage30, float_CoverageRatio30, 4);
            Assert.Equal(float_ExpectedFloor30, float_FloorAreaRatio30, 4);

            // Verify row updates (does not duplicate row and correctly updates values)
            table_Mock.Update_RadialRatios(doubles_RadiusesMock, countyId_Mock, building2D_Target, list_MockBuildings);
            Assert.Equal(1, table_Mock.RowCount);

            // Verify lookup ignores rows with other references
            Building2D building2D_Dummy = new(Guid.NewGuid(), "dummy_ref", polygonalFace2D_Target, 1, Enums.BuildingPhase.occupied, Enums.BuildingGeneralFunction.residential_buildings, []);
            table_Mock.Update_RadialRatios(doubles_RadiusesMock, countyId_Mock, building2D_Dummy, list_MockBuildings);
            Assert.Equal(2, table_Mock.RowCount);

            // 2. Integration verification using loaded GMF file
            string? path = Core.xUnit.Query.FilePath(Assembly.GetExecutingAssembly(), "2476_GML.gmf");
            Assert.False(string.IsNullOrWhiteSpace(path));
            Assert.True(File.Exists(path));

            GISModel? gISModel = null;
            using (GISModelFile gISModelFile = new(path))
            {
                Assert.True(gISModelFile.Open());
                gISModel = gISModelFile.Value;
            }

            Assert.NotNull(gISModel);

            List<Building2D>? building2Ds = gISModel.GetObjects<Building2D>();
            Assert.NotNull(building2Ds);
            Assert.NotEmpty(building2Ds);

            Building2D? building2D_GmfTarget = building2Ds.FirstOrDefault(x => x.PolygonalFace2D?.Centroid() is not null && x.Storeys > 0);
            Assert.NotNull(building2D_GmfTarget);

            Table table_Gmf = new();
            int countyId_Gmf = 789;
            double[] doubles_RadiusesGmf = [50.0];

            table_Gmf.Update_RadialRatios(doubles_RadiusesGmf, countyId_Gmf, building2D_GmfTarget, building2Ds);

            Assert.Equal(1, table_Gmf.RowCount);
            Row? row_Gmf = table_Gmf.GetRow(0);
            Assert.NotNull(row_Gmf);

            Column? column_CoverageRatio50 = table_Gmf.GetColumn(table_Gmf.GetColumnIndex(IO.Create.Column_RadialBuildingCoverageRatio(50.0).Name));
            Column? column_FloorAreaRatio50 = table_Gmf.GetColumn(table_Gmf.GetColumnIndex(IO.Create.Column_RadialFloorAreaRatio(50.0).Name));
            Assert.NotNull(column_CoverageRatio50);
            Assert.NotNull(column_FloorAreaRatio50);

            Assert.True(row_Gmf.TryGetValue(column_CoverageRatio50.Index, out float float_CoverageRatio50));
            Assert.True(row_Gmf.TryGetValue(column_FloorAreaRatio50.Index, out float float_FloorAreaRatio50));

            Assert.True(float_CoverageRatio50 >= 0f);
            Assert.True(float_FloorAreaRatio50 >= 0f);
        }
    }
}
