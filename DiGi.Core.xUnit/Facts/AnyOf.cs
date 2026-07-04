using DiGi.Core.Classes;
using System;

namespace DiGi.Core.xUnit
{
    public partial class Facts
    {
        /// <summary>
        /// Tests the basic construction, implicit conversions, and type resolution of the AnyOf class.
        /// </summary>
        [Fact]
        public void AnyOf_ConversionsAndTypes()
        {
            // Implicit conversions
            AnyOf anyInt = 42;
            AnyOf anyString = "Hello AnyOf";
            AnyOf anyDouble = 3.14159;
            Guid guid = Guid.NewGuid();
            AnyOf anyGuid = guid;
            DateTime now = DateTime.Now;
            AnyOf anyDateTime = now;
            AnyOf anyLong = 1234567890L;
            AnyOf anyBool = true;

            // Value checks
            Assert.Equal(42, anyInt.Value);
            Assert.Equal("Hello AnyOf", anyString.Value);
            Assert.Equal(3.14159, anyDouble.Value);
            Assert.Equal(guid, anyGuid.Value);
            Assert.Equal(now, anyDateTime.Value);
            Assert.Equal(1234567890L, anyLong.Value);
            Assert.Equal(true, anyBool.Value);

            // GetType check (custom override in AnyOf returns the type of the value)
            Assert.Equal(typeof(int), anyInt.GetType());
            Assert.Equal(typeof(string), anyString.GetType());
            Assert.Equal(typeof(double), anyDouble.GetType());
            Assert.Equal(typeof(Guid), anyGuid.GetType());
            Assert.Equal(typeof(DateTime), anyDateTime.GetType());
            Assert.Equal(typeof(long), anyLong.GetType());
            Assert.Equal(typeof(bool), anyBool.GetType());
        }

        /// <summary>
        /// Tests equality and inequality operators and Equals overrides on AnyOf.
        /// </summary>
        [Fact]
        public void AnyOf_EqualityAndOperators()
        {
            AnyOf anyInt1 = 100;
            AnyOf anyInt2 = 100;
            AnyOf anyInt3 = 200;
            AnyOf anyString = "100";

            // Equals method
            Assert.True(anyInt1.Equals(100));
            Assert.False(anyInt1.Equals(anyInt3));
            Assert.False(anyInt1.Equals(anyString));

            // == and != operators (operator == compares the wrapped value to a raw object)
            Assert.True(anyInt1 == 100);
            Assert.Equal(anyInt2.Value, anyInt1.Value);
            Assert.NotEqual(anyInt3.Value, anyInt1.Value);
            Assert.False(anyInt1 == anyString);

            Assert.True(anyInt1 != 200);
            Assert.NotEqual(anyInt3.Value, anyInt1.Value);
            Assert.False(anyInt1 == anyString);

            // Null equality checks
            AnyOf? nullAnyOf = null;
            Assert.True(nullAnyOf == null);

            AnyOf anyNullVal = new(null, typeof(string));
            Assert.True(anyNullVal == null);
            Assert.True(anyNullVal!.Equals(null));
        }
    }
}