using System.Collections.Generic;
using System.Linq;

namespace DiGi.Laya.xUnit
{
    public partial class Facts
    {
        /// <summary>
        /// Verifies that <see cref="Classes.LayaRequestMessage"/> keeps its request id and message, survives a string round trip and <c>SerializationCheck</c>, and that its copy constructor copies both members, for a populated message - with a quote in the request id - and for one with every member <c>null</c>.
        /// </summary>
        [Fact]
        public void LayaRequestMessage()
        {
            Classes.LayaRequestMessage layaRequestMessage = new("r'4", "The request 'r'4' was refused by Laya: ValueError: too long");

            static void AssertPopulated(Classes.LayaRequestMessage? layaRequestMessage_Actual)
            {
                Assert.NotNull(layaRequestMessage_Actual);
                Assert.Equal("r'4", layaRequestMessage_Actual!.RequestId);
                Assert.Equal("The request 'r'4' was refused by Laya: ValueError: too long", layaRequestMessage_Actual.Message);
            }

            AssertPopulated(layaRequestMessage);

            string? json = Core.Convert.ToSystem_String(layaRequestMessage);
            Assert.False(string.IsNullOrWhiteSpace(json));

            List<Classes.LayaRequestMessage>? layaRequestMessages = Core.Convert.ToDiGi<Classes.LayaRequestMessage>(json);
            AssertPopulated(layaRequestMessages?.FirstOrDefault());

            Core.xUnit.Query.SerializationCheck(layaRequestMessage);

            AssertPopulated(new Classes.LayaRequestMessage(layaRequestMessage));

            Classes.LayaRequestMessage layaRequestMessage_Empty = new(null, null);
            Core.xUnit.Query.SerializationCheck(layaRequestMessage_Empty);

            Classes.LayaRequestMessage layaRequestMessage_EmptyCopy = new(layaRequestMessage_Empty);
            Assert.Null(layaRequestMessage_EmptyCopy.RequestId);
            Assert.Null(layaRequestMessage_EmptyCopy.Message);
        }
    }
}
