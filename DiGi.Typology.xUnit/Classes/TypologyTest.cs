using DiGi.Typology.Classes;
using System.Text.Json.Nodes;

namespace DiGi.Typology.xUnit.Classes
{
    /// <summary>
    /// A typology over <see cref="TypologyItemTest"/>, for testing that
    /// <see cref="Typology{TTypology, TTypologyItem}"/> can be derived from with a specialised item type.
    /// </summary>
    public class TypologyTest : Typology<TypologyTest, TypologyItemTest>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TypologyTest"/> class with a specified item.
        /// </summary>
        /// <param name="typologyItemTest">The typology item to assign; it is cloned.</param>
        public TypologyTest(TypologyItemTest? typologyItemTest)
            : base(typologyItemTest)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TypologyTest"/> class by cloning an existing typology.
        /// </summary>
        /// <param name="typologyTest">The source typology to clone.</param>
        public TypologyTest(TypologyTest? typologyTest)
            : base(typologyTest)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TypologyTest"/> class from a JSON object.
        /// </summary>
        /// <param name="jsonObject">The JSON object containing typology data.</param>
        public TypologyTest(JsonObject? jsonObject)
            : base(jsonObject)
        {
        }
    }
}
