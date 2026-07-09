using DiGi.Communication.Enums;
using System.Numerics;

namespace DiGi.Communication.xUnit
{
    public partial class Facts
    {
        /// <summary>
        /// Tests the complex reflection coefficient for both polarizations: at grazing incidence (gamma = 0) the coefficient equals -1, at normal incidence (gamma = pi / 2) the vertical and horizontal coefficients are opposite and their magnitude stays below 1.
        /// </summary>
        [Fact]
        public void ReflectionCoefficient()
        {
            Complex complexRelativePermittivity = new(15, -0.3);

            Complex reflectionCoefficient_Vertical_Grazing = Query.ReflectionCoefficient(complexRelativePermittivity, 0, Polarization.Vertical);
            Assert.Equal(-1.0, reflectionCoefficient_Vertical_Grazing.Real, 9);
            Assert.Equal(0.0, reflectionCoefficient_Vertical_Grazing.Imaginary, 9);

            Complex reflectionCoefficient_Horizontal_Grazing = Query.ReflectionCoefficient(complexRelativePermittivity, 0, Polarization.Horizontal);
            Assert.Equal(-1.0, reflectionCoefficient_Horizontal_Grazing.Real, 9);
            Assert.Equal(0.0, reflectionCoefficient_Horizontal_Grazing.Imaginary, 9);

            Complex reflectionCoefficient_Vertical_Normal = Query.ReflectionCoefficient(complexRelativePermittivity, Math.PI / 2, Polarization.Vertical);
            Complex reflectionCoefficient_Horizontal_Normal = Query.ReflectionCoefficient(complexRelativePermittivity, Math.PI / 2, Polarization.Horizontal);

            Assert.Equal(-reflectionCoefficient_Horizontal_Normal.Real, reflectionCoefficient_Vertical_Normal.Real, 9);
            Assert.Equal(-reflectionCoefficient_Horizontal_Normal.Imaginary, reflectionCoefficient_Vertical_Normal.Imaginary, 9);

            Assert.True(reflectionCoefficient_Vertical_Normal.Magnitude < 1);
            Assert.True(reflectionCoefficient_Horizontal_Normal.Magnitude < 1);
        }
    }
}
