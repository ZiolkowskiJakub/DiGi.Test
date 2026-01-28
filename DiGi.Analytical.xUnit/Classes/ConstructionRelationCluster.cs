using DiGi.Analytical.Building.Classes;
using DiGi.Analytical.Building.Interfaces;
using DiGi.Analytical.Classes;
using DiGi.Analytical.Interfaces;
using DiGi.Core.Classes;

namespace DiGi.Analytical.xUnit
{
    public partial class Classes
    {
        [Fact]
        public void ConstructionRelationCluster()
        {
            IMaterial material_1 = new OpaqueMaterial("Opaque Material 1")
            {
                Emissivity = new SurfaceValue(0.9, 0.8)
            };

            IMaterial material_2 = new OpaqueMaterial("Opaque Material 2")
            {
                Conductivity = 0.1
            };

            WallConstruction wallConstruction = new() { Name = "Sample wall construction" };

            ConstructionRelationCluster constructionRelationCluster = [];
            constructionRelationCluster.Add(wallConstruction);
            constructionRelationCluster.Add(material_1);
            constructionRelationCluster.Add(material_2);

            WallConstruction? wallConstruction_Temp = constructionRelationCluster.GetValue<WallConstruction>(new GuidReference(wallConstruction));

            Assert.NotNull(wallConstruction_Temp);

            StructureLayer? structureLayer_1 = Building.Create.StructureLayer(constructionRelationCluster, wallConstruction, material_1, 10);
            StructureLayer? structureLayer_2 = Building.Create.StructureLayer(constructionRelationCluster, wallConstruction, material_2, 20);

            Assert.NotNull(structureLayer_1);
            Assert.NotNull(structureLayer_2);

            if (structureLayer_1 is null)
            {
                return;
            }

            IMaterial? mateial_Temp = constructionRelationCluster.GetMaterial(structureLayer_1);

            Assert.NotNull(mateial_Temp);

            if (mateial_Temp is not null)
            {
                Assert.Equal(material_1.Name, mateial_Temp.Name);
            }

            List<IStructureLayer>? structureLayers = constructionRelationCluster.GetStructureLayers(wallConstruction);

            Assert.NotNull(structureLayers);

            if (structureLayers is not null)
            {
                Assert.NotEmpty(structureLayers);
                if (structureLayers.Count != 0)
                {
                    Assert.Equal(2, structureLayers.Count);

                    Assert.Equal(structureLayer_1.Thickness, structureLayers[0].Thickness);
                }
            }
        }
    }
}