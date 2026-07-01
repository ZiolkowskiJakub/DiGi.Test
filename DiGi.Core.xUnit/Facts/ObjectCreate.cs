namespace DiGi.Core.xUnit
{
    public partial class Facts
    {
        /// <summary>
        /// Tests that <see cref="DiGi.Core.Create.Object{T}(object[])"/> still resolves constructors correctly
        /// after optimizing the constructor-argument lookup (removing a throwaway ToList().ConvertAll() allocation)
        /// and the O(n^2) parameter-matching logic (replaced List.Find/Remove with an index-based used-flags array).
        /// </summary>
        [Fact]
        public void Object_Create_ExactConstructorMatch()
        {
            // Exact constructor match: TestObject(string name)
            TestObject? testObject = Create.Object<TestObject>("AAAA");

            Assert.NotNull(testObject);
            Assert.Equal("AAAA", testObject.Name);
        }

        /// <summary>
        /// Tests that the parameterless constructor is used when no arguments are supplied.
        /// </summary>
        [Fact]
        public void Object_Create_ParameterlessConstructor()
        {
            TestObject? testObject = Create.Object<TestObject>();

            Assert.NotNull(testObject);
        }

        /// <summary>
        /// Tests that constructor arguments are matched by assignable type regardless of the order
        /// they are supplied in, exercising the parameter-matching fallback logic.
        /// </summary>
        [Fact]
        public void Object_Create_ParametersMatchedRegardlessOfOrder()
        {
            // TestObject(string name, double min, double max) - supply args out of declared order
            TestObject? testObject = Create.Object<TestObject>(1.0, 2.0, "Name");

            Assert.NotNull(testObject);
            Assert.Equal("Name", testObject.Name);
        }

        /// <summary>
        /// Tests that object creation returns null/default when no constructor matches the supplied arguments.
        /// </summary>
        [Fact]
        public void Object_Create_NoMatchingConstructor_ReturnsDefault()
        {
            TestObject? testObject = Create.Object<TestObject>(new object(), new object(), new object(), new object());

            Assert.Null(testObject);
        }
    }
}