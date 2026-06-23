using DiGi.Typology.Classes;
using System.Text.Json.Nodes;

namespace DiGi.Typology.xUnit.Classes
{
    /// <summary>
    /// A specialized, completely non-generic typology filter tailored specifically for ModelElement objects.
    /// </summary>
    public class TypologyFilterTest : TypologyFilter<TypologyFilterTest, string>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TypologyFilterTest"/> class.
        /// </summary>
        public TypologyFilterTest()
            : base()
        {
        }

        /// <summary>
        /// Initializes a new instance from a JsonObject for serialization/deserialization.
        /// </summary>
        /// <param name="jsonObject">The JSON object containing filter data.</param>
        public TypologyFilterTest(JsonObject jsonObject)
            : base(jsonObject)
        {
        }

        /// <summary>
        /// Copy constructor for deep cloning capabilities.
        /// </summary>
        /// <param name="typologyFilterTest">The source filter instance to copy from.</param>
        public TypologyFilterTest(TypologyFilterTest typologyFilterTest)
            : base(typologyFilterTest)
        {
        }
    }
}