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

        /// <summary>
        /// Creates a <see cref="TypologyTest"/> for the specified full path, taking the name, the description and the
        /// weight of the source item.
        /// </summary>
        /// <param name="source">The item to take the name, the description and the weight from; null for an unnamed intermediate node.</param>
        /// <param name="path">The full path of the node to create.</param>
        /// <returns>The created typology.</returns>
        public override TypologyTest CreateNode(TypologyItemTest? source, TypologyPath? path)
        {
            TypologyItemTest item = source is null ? new TypologyItemTest(path, null, 0) : new TypologyItemTest(path, source);
            return new TypologyTest(item);
        }
    }
}
