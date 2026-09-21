using DiGi.Analytical.Building.Classes;
using DiGi.CityGML.Classes;
using DiGi.GIS.Classes;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace DiGi.GIS.PostgreSQL.UI.xUnit
{
    public partial class Facts
    {
        /// <summary>
        /// Reference of the courtyard building of county 1465 (Warsaw) reported by DiGi.GIS.WebAPI.UI#45.
        /// </summary>
        private const string reference_Courtyard_1465 = "38F62224-C903-F520-E053-CA2BA8C0BE14";

        /// <summary>
        /// Pins the courtyard-audit predicate of <see cref="DiGi.GIS.PostgreSQL.UI.Classes.UIBuildingModelsCourtyardAuditTask"/> against the real LOD2 courtyard building of DiGi.GIS.WebAPI.UI#45: the source ground surface carries the courtyard as an interior ring, the fixed stored model keeps it as an internal edge on its footprints, and the affected test is the conjunction of the two halves.
        /// </summary>
        [Fact]
        public void UIBuildingModelsCourtyardAuditTask_Predicate()
        {
            Building source = CityGML_Building_1465_Courtyard();
            Building2D building2D = Building2D_1465_Courtyard();

            BuildingModel? stored = DiGi.GIS.Analytical.Create.BuildingModel(source, building2D);
            Assert.NotNull(stored);

            double tolerance = DiGi.Core.Constants.Tolerance.Distance;

            // The source half: the ground surface carries the courtyard as an interior ring.
            Assert.True(DiGi.GIS.PostgreSQL.UI.Classes.UIBuildingModelsCourtyardAuditTask.SourceHasRing(source), "The source ground surface should carry a courtyard interior ring.");
            Assert.False(DiGi.GIS.PostgreSQL.UI.Classes.UIBuildingModelsCourtyardAuditTask.SourceHasRing(null), "A null source has no courtyard ring.");

            // The stored half: the fixed model keeps the courtyard as an internal edge on its footprints.
            Assert.True(DiGi.GIS.PostgreSQL.UI.Classes.UIBuildingModelsCourtyardAuditTask.StoredHasHole(stored, tolerance), "The fixed stored model should keep the courtyard as an internal edge.");
            Assert.False(DiGi.GIS.PostgreSQL.UI.Classes.UIBuildingModelsCourtyardAuditTask.StoredHasHole(null, tolerance), "A null stored model has no internal edge.");

            // The affected test is the conjunction: a source courtyard kept by the stored model is Ok, a source courtyard with no stored model is Missing, and a null source is NoCourtyard - none of which is Affected.
            Assert.False(DiGi.GIS.PostgreSQL.UI.Classes.UIBuildingModelsCourtyardAuditTask.IsAffected(source, stored, tolerance), "A source courtyard kept by the stored model is Ok, not Affected.");
            Assert.False(DiGi.GIS.PostgreSQL.UI.Classes.UIBuildingModelsCourtyardAuditTask.IsAffected(source, null, tolerance), "A source courtyard with no stored model is Missing, not Affected.");
            Assert.False(DiGi.GIS.PostgreSQL.UI.Classes.UIBuildingModelsCourtyardAuditTask.IsAffected(null, null, tolerance), "A null source is NoCourtyard, not Affected.");

            // And the affected case itself: the courtyard source paired with a hole-less stored model is Affected. A plain box building is the hole-less model - the self-check below pins that the fixture really carries no courtyard.
            Building source_NoCourtyard = NoCourtyard_Building();
            Assert.False(DiGi.GIS.PostgreSQL.UI.Classes.UIBuildingModelsCourtyardAuditTask.SourceHasRing(source_NoCourtyard), "The no-courtyard fixture should not carry an interior ring.");

            BuildingModel? stored_NoHole = DiGi.GIS.Analytical.Create.BuildingModel(source_NoCourtyard);
            Assert.NotNull(stored_NoHole);
            Assert.False(DiGi.GIS.PostgreSQL.UI.Classes.UIBuildingModelsCourtyardAuditTask.StoredHasHole(stored_NoHole, tolerance), "A model from a no-courtyard building should have no internal edge.");

            Assert.True(DiGi.GIS.PostgreSQL.UI.Classes.UIBuildingModelsCourtyardAuditTask.IsAffected(source, stored_NoHole, tolerance), "A source courtyard paired with a hole-less stored model is Affected.");
        }

        /// <summary>
        /// Loads a plain box building with no courtyard from the "Buildings.json" fixture - a source that carries no interior ring, so its model is hole-less.
        /// </summary>
        /// <returns>The first building of the "Buildings.json" fixture.</returns>
        private static Building NoCourtyard_Building()
        {
            string? path = Core.xUnit.Query.FilePath(Assembly.GetExecutingAssembly(), "Buildings.json");

            Assert.False(string.IsNullOrWhiteSpace(path));
            Assert.True(File.Exists(path));

            List<Building>? buildings = Core.Convert.ToDiGi<Building>(File.ReadAllText(path!));

            Assert.NotNull(buildings);
            Assert.NotEmpty(buildings);

            return buildings[0];
        }

        /// <summary>
        /// Loads the LOD2 courtyard building of county 1465 reported by DiGi.GIS.WebAPI.UI#45.
        /// </summary>
        /// <returns>The building of the "1465_38F62224-C903_CityGML.gml" fixture.</returns>
        private static Building CityGML_Building_1465_Courtyard()
        {
            string? path = Core.xUnit.Query.FilePath(Assembly.GetExecutingAssembly(), "1465_38F62224-C903_CityGML.gml");

            Assert.False(string.IsNullOrWhiteSpace(path));
            Assert.True(File.Exists(path));

            List<CityModel>? cityModels = CityGML.Create.CityModels(path);
            Assert.NotNull(cityModels);
            Assert.Single(cityModels);

            Building? result = null;
            if (cityModels[0].Buildings is IEnumerable<Building> buildings)
            {
                foreach (Building building in buildings)
                {
                    if (CityGML.Query.Reference(building) == reference_Courtyard_1465)
                    {
                        result = building;
                        break;
                    }
                }
            }

            Assert.NotNull(result);

            return result!;
        }

        /// <summary>
        /// Loads the 2D building pairing with the LOD2 courtyard building of county 1465 reported by DiGi.GIS.WebAPI.UI#45.
        /// </summary>
        /// <returns>The 2D building of the "1465_38F62224-C903_Building2D.json" fixture.</returns>
        private static Building2D Building2D_1465_Courtyard()
        {
            string? path = Core.xUnit.Query.FilePath(Assembly.GetExecutingAssembly(), "1465_38F62224-C903_Building2D.json");

            Assert.False(string.IsNullOrWhiteSpace(path));
            Assert.True(File.Exists(path));

            List<Building2D>? building2Ds = Core.Convert.ToDiGi<Building2D>(File.ReadAllText(path!));

            Assert.NotNull(building2Ds);
            Assert.Single(building2Ds);
            Assert.Equal(reference_Courtyard_1465, building2Ds[0].Reference);

            return building2Ds[0];
        }
    }
}
