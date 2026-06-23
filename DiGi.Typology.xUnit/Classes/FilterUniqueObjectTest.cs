using DiGi.Core.Classes;

namespace DiGi.Typology.xUnit.Classes
{
    /// <summary>
    /// Represents a unique object used for testing typology filters.
    /// </summary>
    public class FilterUniqueObjectTest : UniqueObject
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="FilterUniqueObjectTest"/> class with a specified name.
        /// </summary>
        /// <param name="name">The name of the test object.</param>
        public FilterUniqueObjectTest(string name)
            : base()
        {
            Name = name;
        }

        /// <summary>
        /// Gets or sets the name of the test object.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets the unique identifier for the test object, which is its name.
        /// </summary>
        public override string? UniqueId => Name;
    }
}