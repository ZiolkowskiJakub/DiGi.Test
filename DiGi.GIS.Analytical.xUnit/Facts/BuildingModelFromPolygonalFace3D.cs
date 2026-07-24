using DiGi.Analytical.Building.Classes;
using DiGi.Analytical.Building.Interfaces;
using DiGi.Geometry.Planar.Classes;
using DiGi.Geometry.Spatial.Classes;
using DiGi.Geometry.Spatial.Interfaces;
using System.Collections.Generic;

namespace DiGi.GIS.Analytical.xUnit
{
    public partial class Facts
    {
        /// <summary>
        /// Tests that <see cref="Create.BuildingModel(IPolygonalFace3D?, ushort, double, double)"/> correctly accounts for non-zero base elevation (<c>minElevation</c>) when extruding a polygonal face into storeys and placing the roof surface.
        /// </summary>
        [Fact]
        public void BuildingModel_FromPolygonalFace3D_NonZeroMinElevation()
        {
            double minElevation = 100.0;
            ushort storeys = 3;
            double storeyHeight = 3.5;

            Plane plane = Geometry.Spatial.Create.Plane(minElevation)!;

            PolygonalFace3D? polygonalFace3D = Geometry.Spatial.Create.PolygonalFace3D(
                plane,
                [
                    new Point2D(0, 0),
                    new Point2D(10, 0),
                    new Point2D(10, 10),
                    new Point2D(0, 10)
                ]);

            Assert.NotNull(polygonalFace3D);

            BuildingModel? buildingModel = Create.BuildingModel(polygonalFace3D, storeys, storeyHeight);

            Assert.NotNull(buildingModel);

            BoundingBox3D? boundingBox3D = buildingModel.GetBoundingBox();
            Assert.NotNull(boundingBox3D);

            double expectedMaxZ = minElevation + (storeys * storeyHeight);
            Assert.True(System.Math.Abs(boundingBox3D.MinZ - minElevation) < Core.Constants.Tolerance.Distance, $"MinZ is {boundingBox3D.MinZ} instead of expected {minElevation}");
            Assert.True(System.Math.Abs(boundingBox3D.MaxZ - expectedMaxZ) < Core.Constants.Tolerance.Distance, $"MaxZ is {boundingBox3D.MaxZ} instead of expected {expectedMaxZ}");

            List<IRoof>? roofs = buildingModel.GetComponents<IRoof>();
            Assert.NotNull(roofs);
            Assert.Single(roofs);

            BoundingBox3D? boundingBox3D_Roof = roofs[0].GetBoundingBox();
            Assert.NotNull(boundingBox3D_Roof);
            Assert.True(System.Math.Abs(boundingBox3D_Roof.MinZ - expectedMaxZ) < Core.Constants.Tolerance.Distance, $"Roof MinZ is {boundingBox3D_Roof.MinZ} instead of expected {expectedMaxZ}");

            Core.xUnit.Query.SerializationCheck(buildingModel);
        }
    }
}
