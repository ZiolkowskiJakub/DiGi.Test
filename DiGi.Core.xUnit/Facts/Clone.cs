namespace DiGi.Core.xUnit
{
    public partial class Facts
    {
        /// <summary>
        /// Tests that <see cref="Core.Query.Clone{TSerializableObject}(TSerializableObject)"/> produces an
        /// equivalent clone of a serializable object.
        /// </summary>
        [Fact]
        public void Clone_SerializableObject_ProducesEquivalentClone()
        {
            TestObject testObject = new("CloneTest");

            TestObject? clone = Core.Query.Clone(testObject);
            Assert.NotNull(clone);
            Assert.Equal(testObject.Name, clone.Name);
            Assert.NotSame(testObject, clone);
        }

        /// <summary>
        /// Tests that cloning a null serializable object returns null/default rather than throwing.
        /// </summary>
        [Fact]
        public void Clone_NullObject_ReturnsDefault()
        {
            TestObject? testObject = null;

            TestObject? clone = Core.Query.Clone(testObject);

            Assert.Null(clone);
        }
    }
}
