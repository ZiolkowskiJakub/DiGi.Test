using DiGi.Core.Classes;

namespace DiGi.Typology.xUnit.Classes
{
    /// <summary>
    /// Represents a unique object with dictionary-backed properties for testing typology solver functionality.
    /// </summary>
    public class UniqueObjectTest : UniqueObject
    {
        private Dictionary<string, object?> dictionary = [];

        /// <summary>
        /// Initializes a new instance of the <see cref="UniqueObjectTest"/> class with a specified unique identifier.
        /// </summary>
        /// <param name="uniqueId">The unique identifier.</param>
        public UniqueObjectTest(string uniqueId)
            : base()
        {
            UniqueId = uniqueId;
        }

        /// <summary>
        /// Retrieves a value associated with the specified property name.
        /// </summary>
        /// <param name="name">The property name.</param>
        /// <returns>The associated value, or null if the name is null or not found.</returns>
        public object? GetValue(string? name)
        {
            if (name is null)
            {
                return null;
            }

            if (dictionary.TryGetValue(name, out object? result))
            {
                return result;
            }

            return null;
        }

        /// <summary>
        /// Sets a value associated with the specified property name.
        /// </summary>
        /// <param name="name">The property name.</param>
        /// <param name="value">The value to set.</param>
        /// <returns>True if the value was set successfully; otherwise, false.</returns>
        public bool SetValue(string? name, object? value)
        {
            if (name is null)
            {
                return false;
            }

            dictionary[name] = value;
            return true;
        }

        /// <summary>
        /// Gets the unique identifier for the test object.
        /// </summary>
        public override string? UniqueId { get; }

        /// <summary>
        /// Gets or sets the property value with the specified name.
        /// </summary>
        /// <param name="name">The property name.</param>
        /// <returns>The value associated with the property.</returns>
        public object? this[string name]
        {
            get
            {
                return GetValue(name);
            }

            set
            {
                SetValue(name, value);
            }
        }
    }
}