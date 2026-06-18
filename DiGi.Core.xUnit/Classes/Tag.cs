using DiGi.Core.Classes;
using System;

namespace DiGi.Core.xUnit
{
    public partial class Classes
    {
        /// <summary>
        /// Verifies that <see cref="Tag"/> objects can be correctly serialized when instantiated with different data types such as double, string, integer, boolean, and DateTime.
        /// </summary>
        [Fact]
        public void Tag()
        {
            Query.SerializationCheck(new Tag(10.0));
            Query.SerializationCheck(new Tag("AAA"));
            Query.SerializationCheck(new Tag(1));
            Query.SerializationCheck(new Tag(true));
            Query.SerializationCheck(new Tag(DateTime.Now));
        }
    }
}
