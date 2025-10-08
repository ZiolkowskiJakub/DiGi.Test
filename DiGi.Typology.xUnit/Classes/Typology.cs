using DiGi.Typology.Classes;

namespace DiGi.Typology.xUnit
{
    public partial class Classes
    {
        [Fact]
        public void Typology()
        {
            TypologyItem typologyItem = new ([1, 2], "CCC", "Test CCC");
            string text = typologyItem.ToString();


            Typology.Classes.Typology typology = new("Typology", "Sample Typology");
            typology.Update("AAA", "Test AAA");
            typology.Update([1, 2], "CCC", "Test CCC");

            Assert.NotNull(typology.SubTypologies);

            if(typology.SubTypologies is not List<Typology.Classes.Typology> subTypologies)
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

            Typology.Classes.Typology? typology_Temp = typology.GetTypology([3]);
            Assert.NotNull(typology_Temp);

            if(typology_Temp is null)
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
        }
    }
}