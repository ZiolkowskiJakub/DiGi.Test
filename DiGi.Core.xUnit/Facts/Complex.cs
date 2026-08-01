using DiGi.Core.Enums;

namespace DiGi.Core.xUnit
{
    public partial class Facts
    {
        /// <summary>
        /// Tests complex object initialization and serialization check.
        /// </summary>
        [Fact]
        public void Complex()
        {
            ComplexObject complexObject = new(new System.Numerics.Complex(1, 2));

            Query.SerializationCheck(complexObject);
        }

        /// <summary>
        /// Tests rounding of complex numbers with various tolerances and rounding methods.
        /// </summary>
        [Fact]
        public void Complex_Round()
        {
            System.Numerics.Complex complex = new(1.2345, -6.7891);

            System.Numerics.Complex complex_Rounded1 = complex.Round(0.1);
            Assert.Equal(1.2, complex_Rounded1.Real);
            Assert.Equal(-6.8, complex_Rounded1.Imaginary);

            System.Numerics.Complex complex_Rounded2 = complex.Round(0.1, 0.01, RoundingMethod.Floor);
            Assert.Equal(1.2, complex_Rounded2.Real);
            Assert.Equal(-6.79, complex_Rounded2.Imaginary);
        }

        /// <summary>
        /// Tests validity check of complex numbers with finite, NaN and infinite components.
        /// </summary>
        [Fact]
        public void Complex_IsValid()
        {
            Assert.True(new System.Numerics.Complex(1.2345, -6.7891).IsValid());

            Assert.False(new System.Numerics.Complex(double.NaN, 1).IsValid());
            Assert.False(new System.Numerics.Complex(1, double.NaN).IsValid());
            Assert.False(new System.Numerics.Complex(double.NaN, double.NaN).IsValid());

            Assert.False(new System.Numerics.Complex(double.PositiveInfinity, 1).IsValid());
            Assert.False(new System.Numerics.Complex(1, double.NegativeInfinity).IsValid());
        }

        /// <summary>
        /// Tests formatting complex numbers to system string with real and imaginary tolerances.
        /// </summary>
        [Fact]
        public void Complex_ToSystem_String()
        {
            System.Numerics.Complex complex1 = new(1.2345, 4.5678);
            string? string1 = complex1.ToSystem_String(0.01, 0.01);
            Assert.NotNull(string1);
            Assert.Equal("1.23+j4.57", string1);

            System.Numerics.Complex complex2 = new(2.5, -3.75);
            string? string2 = complex2.ToSystem_String(0.1, 0.1);
            Assert.NotNull(string2);
            Assert.Equal("2.5-j3.8", string2);
        }
    }
}