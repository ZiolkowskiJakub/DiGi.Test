using DiGi.Communication.Obselete.Classes;
using System.Numerics;

namespace DiGi.Communication.xUnit
{
    public partial class Facts
    {
        /// <summary>
        /// Tests the complex relative electrical permittivity epsilon' = epsilon_w - j * 60 * lambda * sigma for epsilon_w = 15, sigma = 0.005 S/m and lambda = 2 m.
        /// </summary>
        [Fact]
        public void ComplexRelativePermittivity()
        {
            MaterialProperties materialProperties = new(15, 0.005);

            Complex complexRelativePermittivity = materialProperties.ComplexRelativePermittivity(2);

            Assert.Equal(15.0, complexRelativePermittivity.Real, 9);
            Assert.Equal(-0.6, complexRelativePermittivity.Imaginary, 9);
        }
    }
}
