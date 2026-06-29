using DiGi.Core.Interfaces;

namespace DiGi.Core.xUnit
{
    public partial class Facts
    {
        /// <summary>
        /// Tests that SerializableObjectCollection.CopyTo still copies all elements correctly after removing a
        /// dead, unreachable duplicate null-check on the backing values list (the same check was performed
        /// twice in a row).
        /// </summary>
        [Fact]
        public void SerializableObjectCollection_CopyTo_CopiesAllElements()
        {
            Core.Classes.SerializableObjectCollection collection = new();
            collection.Add(new Core.Classes.Size(1, 2));
            collection.Add(new Core.Classes.Size(3, 4));
            collection.Add(new Core.Classes.Size(5, 6));

            ISerializableObject[] array = new ISerializableObject[5];
            collection.CopyTo(array, 1);

            Assert.Null(array[0]);
            Assert.Same(collection[0], array[1]);
            Assert.Same(collection[1], array[2]);
            Assert.Same(collection[2], array[3]);
            Assert.Null(array[4]);
        }

        /// <summary>
        /// Tests that SerializableObjectCollection.CopyTo throws for a null destination array and for a
        /// negative arrayIndex, matching ICollection&lt;T&gt;.CopyTo semantics.
        /// </summary>
        [Fact]
        public void SerializableObjectCollection_CopyTo_ValidatesArguments()
        {
            Core.Classes.SerializableObjectCollection collection = new();
            collection.Add(new Core.Classes.Size(1, 2));

            ISerializableObject[]? array_Null = null;
            Assert.Throws<System.ArgumentNullException>(() => collection.CopyTo(array_Null!, 0));

            ISerializableObject[] array = new ISerializableObject[1];
            Assert.Throws<System.ArgumentOutOfRangeException>(() => collection.CopyTo(array, -1));
        }
    }
}
