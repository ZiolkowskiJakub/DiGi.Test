using DiGi.Typology.Classes;

namespace DiGi.Typology.xUnit
{
    public partial class Facts
    {
        /// <summary>
        /// Tests the functionality of the typology system, including the creation of typology items,
        /// updating typology hierarchies, and verifying the retrieval of sub-typologies.
        /// </summary>
        [Fact]
        public void Typology()
        {
            TypologyItem typologyItem = new([1, 2], "CCC", "Test CCC");
            string text = typologyItem.ToString();

            Typology.Classes.Typology? typology = new("Typology", "Sample Typology");
            typology.Update("AAA", "Test AAA");
            typology.Update([1, 2], "CCC", "Test CCC");

            Assert.NotNull(typology.SubTypologies);

            if (typology.SubTypologies is not List<Typology.Classes.Typology> subTypologies)
            {
                return;
            }

            Assert.NotEmpty(subTypologies);

            Typology.Classes.Typology? subTypology = subTypologies.FirstOrDefault();

            Assert.NotNull(subTypology?.SubTypologies);

            subTypology = subTypology?.SubTypologies.FirstOrDefault();

            Assert.NotNull(subTypology);

            //typology.Update([2], "BBB", "Test BBB");
            typology.Update([3, 1], "DDD", "Test DDD");
            typology.Update([3], "EEE", "Test EEE");
            typology.Update([3, 2], "FFF", "Test FFF");

            Typology.Classes.Typology? typology_Temp;

            typology_Temp = typology.GetTypology([3]);
            Assert.NotNull(typology_Temp);

            if (typology_Temp is null)
            {
                return;
            }

            subTypology = typology_Temp?.SubTypologies?.FirstOrDefault();
            Assert.NotNull(subTypology);
            if (subTypology is null)
            {
                return;
            }

            Assert.NotNull(subTypology?.SubTypologies);
            Assert.NotNull(typology_Temp?.GetTypology([1]));
            Assert.NotNull(typology_Temp?.GetTypology([2]));

            Assert.NotNull(typology?.GetTypology([3, 1]));
            Assert.NotNull(typology?.GetTypology([3, 2]));

            Core.xUnit.Query.SerializationCheck(typology);
            Core.xUnit.Query.SerializationCheck(subTypology);

            typology_Temp = typology.GetTypology([]);
            Assert.NotNull(typology_Temp);

            if (typology_Temp is null)
            {
                return;
            }

            bool contains = typology.TryGetTypologies("CCC", out List<Typology.Classes.Typology>? typologies);

            Assert.True(!contains && (typologies is null || typologies.Count == 0));

            typology.TryGetTypologies([3], "DDD", out typologies);

            Assert.NotNull(typologies);
            Assert.NotEmpty(typologies);

            Assert.True(typologies[0].Name == "DDD");

            Assert.True(typologies[0].Description == "Test DDD");

            List<TypologyPath>? typologyPaths = typology.GetTypologyPaths(true);

            Assert.NotNull(typologyPaths);
            Assert.NotEmpty(typologyPaths);

            if (typologyPaths is null)
            {
                return;
            }

            foreach (TypologyPath typologyPath in typologyPaths)
            {
                typology_Temp = typology.GetTypology(typologyPath);
                Assert.NotNull(typology_Temp);
            }

            string json_Typology = "{\"_type\":\"DiGi.Typology.Classes.Typology,DiGi.Typology\",\"Description\":null,\"Name\":\"residential buildings\",\"SubTypologies\":[{\"_type\":\"DiGi.Typology.Classes.Typology,DiGi.Typology\",\"Description\":null,\"Name\":\"Area (200, 300\\u003E\",\"SubTypologies\":[{\"_type\":\"DiGi.Typology.Classes.Typology,DiGi.Typology\",\"Description\":null,\"Name\":\"L\",\"SubTypologies\":[{\"_type\":\"DiGi.Typology.Classes.Typology,DiGi.Typology\",\"Description\":null,\"Name\":\"Occupancy (5, 10\\u003E\",\"SubTypologies\":[],\"References\":[\"aed9498e-739b-42a6-ae9a-d0fbfaa0a656\",\"271ea18f-8c1b-406b-b40d-0512adcb6511\",\"d3f8b2d6-9ff8-4ab1-9e8b-8e823adecb0d\"],\"TypologyItem\":{\"_type\":\"DiGi.Typology.Classes.TypologyItem,DiGi.Typology\",\"TypologyPath\":{\"_type\":\"DiGi.Typology.Classes.TypologyPath,DiGi.Typology\",\"Count\":6,\"Index\":1,\"Parent\":{\"_type\":\"DiGi.Typology.Classes.TypologyPath,DiGi.Typology\",\"Count\":5,\"Index\":1,\"Parent\":{\"_type\":\"DiGi.Typology.Classes.TypologyPath,DiGi.Typology\",\"Count\":4,\"Index\":1,\"Parent\":{\"_type\":\"DiGi.Typology.Classes.TypologyPath,DiGi.Typology\",\"Count\":3,\"Index\":1,\"Parent\":{\"_type\":\"DiGi.Typology.Classes.TypologyPath,DiGi.Typology\",\"Count\":2,\"Index\":1,\"Parent\":{\"_type\":\"DiGi.Typology.Classes.TypologyPath,DiGi.Typology\",\"Count\":1,\"Index\":1,\"Parent\":null,\"ParentCount\":0,\"Values\":[1]},\"ParentCount\":1,\"Values\":[1,1]},\"ParentCount\":2,\"Values\":[1,1,1]},\"ParentCount\":3,\"Values\":[1,1,1,1]},\"ParentCount\":4,\"Values\":[1,1,1,1,1]},\"ParentCount\":5,\"Values\":[1,1,1,1,1,1]},\"Description\":null,\"Name\":\"Occupancy (5, 10\\u003E\"}}],\"References\":[],\"TypologyItem\":{\"_type\":\"DiGi.Typology.Classes.TypologyItem,DiGi.Typology\",\"TypologyPath\":{\"_type\":\"DiGi.Typology.Classes.TypologyPath,DiGi.Typology\",\"Count\":5,\"Index\":1,\"Parent\":{\"_type\":\"DiGi.Typology.Classes.TypologyPath,DiGi.Typology\",\"Count\":4,\"Index\":1,\"Parent\":{\"_type\":\"DiGi.Typology.Classes.TypologyPath,DiGi.Typology\",\"Count\":3,\"Index\":1,\"Parent\":{\"_type\":\"DiGi.Typology.Classes.TypologyPath,DiGi.Typology\",\"Count\":2,\"Index\":1,\"Parent\":{\"_type\":\"DiGi.Typology.Classes.TypologyPath,DiGi.Typology\",\"Count\":1,\"Index\":1,\"Parent\":null,\"ParentCount\":0,\"Values\":[1]},\"ParentCount\":1,\"Values\":[1,1]},\"ParentCount\":2,\"Values\":[1,1,1]},\"ParentCount\":3,\"Values\":[1,1,1,1]},\"ParentCount\":4,\"Values\":[1,1,1,1,1]},\"Description\":null,\"Name\":\"L\"}}],\"References\":[],\"TypologyItem\":{\"_type\":\"DiGi.Typology.Classes.TypologyItem,DiGi.Typology\",\"TypologyPath\":{\"_type\":\"DiGi.Typology.Classes.TypologyPath,DiGi.Typology\",\"Count\":4,\"Index\":1,\"Parent\":{\"_type\":\"DiGi.Typology.Classes.TypologyPath,DiGi.Typology\",\"Count\":3,\"Index\":1,\"Parent\":{\"_type\":\"DiGi.Typology.Classes.TypologyPath,DiGi.Typology\",\"Count\":2,\"Index\":1,\"Parent\":{\"_type\":\"DiGi.Typology.Classes.TypologyPath,DiGi.Typology\",\"Count\":1,\"Index\":1,\"Parent\":null,\"ParentCount\":0,\"Values\":[1]},\"ParentCount\":1,\"Values\":[1,1]},\"ParentCount\":2,\"Values\":[1,1,1]},\"ParentCount\":3,\"Values\":[1,1,1,1]},\"Description\":null,\"Name\":\"Area (200, 300\\u003E\"}}],\"References\":[],\"TypologyItem\":{\"_type\":\"DiGi.Typology.Classes.TypologyItem,DiGi.Typology\",\"TypologyPath\":{\"_type\":\"DiGi.Typology.Classes.TypologyPath,DiGi.Typology\",\"Count\":3,\"Index\":1,\"Parent\":{\"_type\":\"DiGi.Typology.Classes.TypologyPath,DiGi.Typology\",\"Count\":2,\"Index\":1,\"Parent\":{\"_type\":\"DiGi.Typology.Classes.TypologyPath,DiGi.Typology\",\"Count\":1,\"Index\":1,\"Parent\":null,\"ParentCount\":0,\"Values\":[1]},\"ParentCount\":1,\"Values\":[1,1]},\"ParentCount\":2,\"Values\":[1,1,1]},\"Description\":null,\"Name\":\"residential buildings\"}}";
            string name = "Area (300, 400>";

            typology = Core.Convert.ToDiGi<Typology.Classes.Typology>(json_Typology)?.FirstOrDefault();
            Assert.NotNull(typology);

            if (typology is null)
            {
                return;
            }

            typology_Temp = typology.Update(name);

            Assert.NotNull(typology_Temp);

            Assert.True(typology.TryGetLastIndex(out int lastIndex));
            Assert.True(lastIndex == 2);
        }
    }
}