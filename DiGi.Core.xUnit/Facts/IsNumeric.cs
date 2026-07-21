namespace DiGi.Core.xUnit
{
    public partial class Facts
    {
        /// <summary>
        /// Tests the IsNumeric query method for various types, verifying correct numeric type identification and integer classification, including nullable types.
        /// </summary>
        [Fact]
        public void IsNumeric()
        {
            // Test standard integer types
            Assert.True(Core.Query.IsNumeric(typeof(int), out bool isInt_Int));
            Assert.True(isInt_Int);

            Assert.True(Core.Query.IsNumeric(typeof(long), out bool isInt_Long));
            Assert.True(isInt_Long);

            Assert.True(Core.Query.IsNumeric(typeof(byte), out bool isInt_Byte));
            Assert.True(isInt_Byte);

            // Test floating/decimal types
            Assert.True(Core.Query.IsNumeric(typeof(double), out bool isInt_Double));
            Assert.False(isInt_Double);

            Assert.True(Core.Query.IsNumeric(typeof(decimal), out bool isInt_Decimal));
            Assert.False(isInt_Decimal);

            // Test nullable types (Verifying the fix for Bug 1)
            Assert.True(Core.Query.IsNumeric(typeof(int?), out bool isInt_NullableInt));
            Assert.True(isInt_NullableInt);

            Assert.True(Core.Query.IsNumeric(typeof(double?), out bool isInt_NullableDouble));
            Assert.False(isInt_NullableDouble);

            // Test non-numeric types
            Assert.False(Core.Query.IsNumeric(typeof(string), out bool isInt_String));
            Assert.False(isInt_String);

            Assert.False(Core.Query.IsNumeric(typeof(object), out bool isInt_Object));
            Assert.False(isInt_Object);

            // Test instance-based IsNumeric
            object object_Int = 123;
            Assert.True(object_Int.IsNumeric(out bool isInt_ObjInt));
            Assert.True(isInt_ObjInt);

            object object_Double = 123.45;
            Assert.True(object_Double.IsNumeric(out bool isInt_ObjDouble));
            Assert.False(isInt_ObjDouble);

            object? object_Null = null;
            Assert.False(object_Null.IsNumeric());
        }
    }
}