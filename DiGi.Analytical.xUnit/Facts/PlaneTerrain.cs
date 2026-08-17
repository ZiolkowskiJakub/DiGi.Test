using DiGi.Analytical.Building.Classes;
using DiGi.Geometry.Spatial.Classes;
using System.Collections.Generic;

namespace DiGi.Analytical.xUnit
{
    public partial class Facts
    {
        /// <summary>
        /// Tests the construction, geometry assignment, copy constructor and serialization round trip of <see cref="PlaneTerrain"/>.
        /// </summary>
        [Fact]
        public void PlaneTerrain()
        {
            void AssertElevation(Plane? plane, double elevation)
            {
                Assert.NotNull(plane);

                Point3D? point3D = plane!.Origin;
                Assert.NotNull(point3D);
                Assert.Equal(elevation, point3D!.Z, 6);
            }

            Plane plane = new(new Point3D(0, 0, 12.5), new Vector3D(0, 0, 1));

            PlaneTerrain planeTerrain_1 = new(plane);
            AssertElevation(planeTerrain_1.Geometry, 12.5);

            // Copy constructor check
            PlaneTerrain planeTerrain_2 = new(planeTerrain_1);
            Assert.Equal(planeTerrain_1.Guid, planeTerrain_2.Guid);
            AssertElevation(planeTerrain_2.Geometry, 12.5);

            // Null geometry is allowed
            PlaneTerrain planeTerrain_3 = new((Plane?)null);
            Assert.Null(planeTerrain_3.Geometry);

            // String round trip
            string? json = Core.Convert.ToSystem_String(planeTerrain_1);
            Assert.False(string.IsNullOrWhiteSpace(json));

            List<PlaneTerrain>? planeTerrains = Core.Convert.ToDiGi<PlaneTerrain>(json);
            Assert.NotNull(planeTerrains);

            PlaneTerrain? planeTerrain_Temp = planeTerrains!.Count == 0 ? null : planeTerrains[0];
            Assert.NotNull(planeTerrain_Temp);
            Assert.Equal(planeTerrain_1.Guid, planeTerrain_Temp!.Guid);
            AssertElevation(planeTerrain_Temp.Geometry, 12.5);

            Core.xUnit.Query.SerializationCheck(planeTerrain_1);
        }
    }
}
