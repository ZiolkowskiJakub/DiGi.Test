namespace DiGi.Core.xUnit
{
    public partial class Facts
    {
        /// <summary>
        /// Tests that <see cref="Parameter.Query.Description(System.Enum)"/> returns the correct
        /// [Description] attribute text for enum values.
        /// </summary>
        [Fact]
        public void Description_Enum_ReturnsAttributeText()
        {
            // Call DiGi.Core.Parameter.Query.Description explicitly: a same-signature extension method also
            // exists in the enclosing DiGi.Core namespace, which would otherwise win extension-method lookup.
            string? description1 = Core.Parameter.Query.Description(TestEnum.Test1);
            Assert.Equal("Test 1", description1);

            string? description2 = Core.Parameter.Query.Description(TestEnum.Test2);
            Assert.Equal("Test 2", description2);
        }

        /// <summary>
        /// Tests that <see cref="Parameter.Query.Description(System.Type)"/> returns null for a type
        /// without a [Description] attribute.
        /// </summary>
        [Fact]
        public void Description_Type_ReturnsNullWhenAttributeMissing()
        {
            string? description = Core.Parameter.Query.Description(typeof(TestObject));
            Assert.Null(description);
        }

        /// <summary>
        /// Tests that <see cref="Parameter.Query.Description(System.Enum)"/> returns null for a null enum input.
        /// </summary>
        [Fact]
        public void Description_Enum_NullInput_ReturnsNull()
        {
            System.Enum? @enum = null;
            Assert.Null(Core.Parameter.Query.Description(@enum));
        }
    }
}