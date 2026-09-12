using DiGi.Typology.Classes;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace DiGi.Typology.xUnit.Classes
{
    /// <summary>
    /// A <see cref="TypologyItem"/> carrying one extra field, for testing that a derived item type takes
    /// part in the equality, ordering and serialization of a derived typology.
    /// </summary>
    public class TypologyItemTest : TypologyItem
    {
        [JsonInclude, JsonPropertyName(nameof(Weight))]
        private double weight;

        /// <summary>
        /// Initializes a new, empty instance of the <see cref="TypologyItemTest"/> class.
        /// </summary>
        public TypologyItemTest()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TypologyItemTest"/> class.
        /// </summary>
        /// <param name="values">The sequence of integers defining the typology path.</param>
        /// <param name="name">The name of the item.</param>
        /// <param name="weight">The extra payload.</param>
        public TypologyItemTest(IEnumerable<int>? values, string? name, double weight)
            : base(values, name)
        {
            this.weight = weight;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TypologyItemTest"/> class using a specific typology path and
        /// the name, description and weight of another item.
        /// </summary>
        /// <param name="typologyPath">The path to assign to this item.</param>
        /// <param name="typologyItemTest">The source item to copy the name, the description and the weight from.</param>
        public TypologyItemTest(TypologyPath? typologyPath, TypologyItemTest typologyItemTest)
            : base(typologyPath, typologyItemTest)
        {
            weight = typologyItemTest.weight;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TypologyItemTest"/> class by cloning an existing item.
        /// </summary>
        /// <param name="typologyItemTest">The source item to clone.</param>
        public TypologyItemTest(TypologyItemTest? typologyItemTest)
            : base(typologyItemTest)
        {
            if (typologyItemTest is not null)
            {
                weight = typologyItemTest.weight;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TypologyItemTest"/> class from a JSON object.
        /// </summary>
        /// <param name="jsonObject">The JSON object containing item data.</param>
        public TypologyItemTest(JsonObject? jsonObject)
            : base(jsonObject)
        {
        }

        /// <summary>
        /// Gets the extra payload.
        /// </summary>
        [JsonIgnore]
        public double Weight
        {
            get
            {
                return weight;
            }
        }

        /// <summary>
        /// Compares by the base item first, then by <see cref="Weight"/>; a plain <see cref="TypologyItem"/>
        /// orders before an equal-valued test item.
        /// </summary>
        /// <param name="typologyItem">The item to compare with this instance.</param>
        /// <returns>A value indicating the relative order of the objects being compared.</returns>
        public override int CompareTo(TypologyItem typologyItem)
        {
            int compare = base.CompareTo(typologyItem);
            if (compare != 0)
            {
                return compare;
            }

            return typologyItem is TypologyItemTest typologyItemTest ? weight.CompareTo(typologyItemTest.weight) : 1;
        }

        /// <summary>
        /// Determines whether the specified item is value-equal to the current item, including <see cref="Weight"/>.
        /// </summary>
        /// <param name="typologyItem">The item to compare with the current instance.</param>
        /// <returns>True if the item is a <see cref="TypologyItemTest"/> equal in base value and weight; otherwise, false.</returns>
        public override bool Equals(TypologyItem? typologyItem)
        {
            return typologyItem is TypologyItemTest typologyItemTest && base.Equals(typologyItemTest) && weight == typologyItemTest.weight;
        }

        /// <summary>
        /// Returns a hash code combining the base item hash with <see cref="Weight"/>.
        /// </summary>
        /// <returns>A 32-bit signed integer hash code.</returns>
        public override int GetHashCode()
        {
            unchecked
            {
                return base.GetHashCode() * 31 + weight.GetHashCode();
            }
        }
    }
}
