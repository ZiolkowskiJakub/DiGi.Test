using DiGi.Geometry.Planar;
using DiGi.Geometry.Planar.Classes;
using DiGi.GIS.Enums;
using System;
using System.Collections.Generic;

namespace DiGi.GIS.WebAPI.UI.xUnit
{
    public partial class Facts
    {
        /// <summary>
        /// Tests the synthetic archetype of a typology node (<see cref="UI.Create.Building2D(GIS.Classes.Building2D?, double, ushort)"/>): the real representative's footprint scaled about its centroid to the given floor area, with the given storeys.
        /// <para>The archetype's area equals the target and its centroid stays put, so the archetype is not shifted by the scale about the origin a uniform <c>Transform2D.Scale(factor)</c> would apply in a composed transform. Its storeys are the given ones, its phase, functions and reference are the source's, and its identifier is new. The source is left untouched. A missing footprint, a non-positive or non-finite floor area and 0 storeys answer null.</para>
        /// </summary>
        [Fact]
        public void Building2D_Representative()
        {
            List<Point2D> point2Ds = [new(100, 100), new(120, 100), new(120, 105), new(100, 105)];
            PolygonalFace2D polygonalFace2D = new Polygon2D(point2Ds).PolygonalFace2D()!;
            GIS.Classes.Building2D building2D = new(Guid.NewGuid(), "REF-1", polygonalFace2D, 2, BuildingPhase.occupied, BuildingGeneralFunction.residential_buildings, [BuildingSpecificFunction.single_family_building]);

            GIS.Classes.Building2D? building2D_Archetype = building2D.Building2D(400, 3);

            Assert.NotNull(building2D_Archetype);
            Assert.Equal(400, building2D_Archetype!.PolygonalFace2D!.GetArea(), 6);
            Assert.Equal((ushort)3, building2D_Archetype.Storeys);
            Assert.Equal("REF-1", building2D_Archetype.Reference);
            Assert.NotEqual(building2D.Guid, building2D_Archetype.Guid);
            Assert.Equal(BuildingPhase.occupied, building2D_Archetype.BuildingPhase);
            Assert.Equal(BuildingGeneralFunction.residential_buildings, building2D_Archetype.BuildingGeneralFunction);
            Assert.Contains(BuildingSpecificFunction.single_family_building, building2D_Archetype.BuildingSpecificFunctions);

            Point2D centroid = building2D.PolygonalFace2D!.Centroid()!;
            Point2D centroid_Archetype = building2D_Archetype.PolygonalFace2D.Centroid()!;
            Assert.Equal(centroid.X, centroid_Archetype.X, 6);
            Assert.Equal(centroid.Y, centroid_Archetype.Y, 6);

            // The proportions are kept: a 20 x 5 footprint at 400 m² is 40 x 10.
            BoundingBox2D boundingBox2D = building2D_Archetype.PolygonalFace2D.GetBoundingBox()!;
            Assert.Equal(40, boundingBox2D.Width, 6);
            Assert.Equal(10, boundingBox2D.Height, 6);

            // The source is not modified.
            Assert.Equal(100, building2D.PolygonalFace2D!.GetArea(), 6);
            Assert.Equal((ushort)2, building2D.Storeys);

            Assert.Null(((GIS.Classes.Building2D?)null).Building2D(400, 3));
            Assert.Null(building2D.Building2D(0, 3));
            Assert.Null(building2D.Building2D(-1, 3));
            Assert.Null(building2D.Building2D(double.NaN, 3));
            Assert.Null(building2D.Building2D(400, 0));
        }
    }
}
