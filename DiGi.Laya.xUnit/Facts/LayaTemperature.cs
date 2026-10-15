using System.Collections.Generic;
using System.Linq;

namespace DiGi.Laya.xUnit
{
    public partial class Facts
    {
        /// <summary>
        /// Verifies that <see cref="Classes.LayaTemperature"/> keeps every member through a string round trip and <c>SerializationCheck</c>, and that its copy constructor copies every member, for a populated instance and for one with every optional member <c>null</c>.
        /// </summary>
        [Fact]
        public void LayaTemperature()
        {
            Classes.LayaTemperature layaTemperature = new(Enums.LayaQuestionType.Noul, 2.4319565296173096);

            static void AssertPopulated(Classes.LayaTemperature? layaTemperature_Actual)
            {
                Assert.NotNull(layaTemperature_Actual);
                Assert.Equal(Enums.LayaQuestionType.Noul, layaTemperature_Actual!.QuestionType);
                Assert.Equal(2.4319565296173096, layaTemperature_Actual.Value);
            }

            AssertPopulated(layaTemperature);

            string? json = Core.Convert.ToSystem_String(layaTemperature);
            Assert.False(string.IsNullOrWhiteSpace(json));

            List<Classes.LayaTemperature>? layaTemperatures = Core.Convert.ToDiGi<Classes.LayaTemperature>(json);
            AssertPopulated(layaTemperatures?.FirstOrDefault());

            Core.xUnit.Query.SerializationCheck(layaTemperature);

            Classes.LayaTemperature layaTemperature_Copy = new(layaTemperature);
            AssertPopulated(layaTemperature_Copy);

            Classes.LayaTemperature layaTemperature_Empty = new(Enums.LayaQuestionType.Choice, 1.0);
            Core.xUnit.Query.SerializationCheck(layaTemperature_Empty);

            Classes.LayaTemperature layaTemperature_EmptyCopy = new(layaTemperature_Empty);
            Assert.Equal(Enums.LayaQuestionType.Choice, layaTemperature_EmptyCopy.QuestionType);
            Assert.Equal(1.0, layaTemperature_EmptyCopy.Value);
        }
    }
}
