using DiGi.GIS.Classes;
using DiGi.GIS.Enums;
using System;
using System.Collections.Generic;

namespace DiGi.GIS.xUnit
{
    public partial class Facts
    {
        /// <summary>Verifies the GISModel lookup surface: Contains correctly reports membership for added and unrelated objects (including null), and GetReferences, GetObject, GetObjects and TryGetObjects resolve objects by reference while honouring the maximum count.</summary>
        [Fact]
        public void GISModel()
        {
            Building2D building2D_1 = CreateBuilding2D("reference_1");
            Building2D building2D_2 = CreateBuilding2D("reference_2");
            Building2D building2D_3 = CreateBuilding2D("reference_3");
            Building2D building2D_Unrelated = CreateBuilding2D("reference_Unrelated");

            GISModel gISModel = new();

            Assert.True(gISModel.Update(building2D_1));
            Assert.True(gISModel.Update(building2D_2));
            Assert.True(gISModel.Update(building2D_3));

            Assert.True(gISModel.Contains(building2D_1));
            Assert.True(gISModel.Contains(building2D_2));
            Assert.True(gISModel.Contains(building2D_3));

            Assert.False(gISModel.Contains(building2D_Unrelated));
            Assert.False(gISModel.Contains((Building2D?)null));

            AdministrativeSubdivision administrativeSubdivision = new(Guid.NewGuid(), "reference_Subdivision", "code_Subdivision", null, default, "name_Subdivision", null);

            Assert.False(gISModel.Contains((AdministrativeAreal2D?)null));
            Assert.False(gISModel.Contains(administrativeSubdivision));
            Assert.True(gISModel.Update(administrativeSubdivision));
            Assert.True(gISModel.Contains(administrativeSubdivision));

            HashSet<string>? references = gISModel.GetReferences<Building2D>();
            Assert.NotNull(references);
            Assert.Equal(3, references.Count);
            Assert.Contains("reference_1", references);
            Assert.Contains("reference_2", references);
            Assert.Contains("reference_3", references);

            Building2D? building2D_Found = gISModel.GetObject<Building2D>("reference_2");
            Assert.NotNull(building2D_Found);
            Assert.Equal("reference_2", building2D_Found.Reference);

            Assert.Null(gISModel.GetObject<Building2D>("reference_Unrelated"));

            List<Building2D>? building2Ds_Found = gISModel.GetObjects<Building2D>(["reference_1", "reference_3", "reference_Unrelated"]);
            Assert.NotNull(building2Ds_Found);
            Assert.Equal(2, building2Ds_Found.Count);

            Assert.True(gISModel.TryGetObjects(["reference_1", "reference_2", "reference_3"], out List<Building2D>? building2Ds_Limited, 2));
            Assert.NotNull(building2Ds_Limited);
            Assert.Equal(2, building2Ds_Limited.Count);

            Assert.False(gISModel.TryGetObjects(["reference_Unrelated"], out List<Building2D>? building2Ds_Missing));
            Assert.True(building2Ds_Missing is null || building2Ds_Missing.Count == 0);
        }

        private static Building2D CreateBuilding2D(string reference)
        {
            return new Building2D(Guid.NewGuid(), reference, null, 1, BuildingPhase.occupied, BuildingGeneralFunction.residential_buildings, [BuildingSpecificFunction.single_family_building]);
        }
    }
}
