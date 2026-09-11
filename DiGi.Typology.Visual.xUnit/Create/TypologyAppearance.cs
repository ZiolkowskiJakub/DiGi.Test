using DiGi.Geometry.Visual.Core.Classes;
using DiGi.Typology.Visual.Classes;

namespace DiGi.Typology.Visual.xUnit
{
    public static partial class Create
    {
        /// <summary>
        /// Creates a typology appearance holding a curve, a face and a mesh appearance in the given colour, with the
        /// given thickness on every edge, so that a fact can tell two appearances apart by colour alone.
        /// </summary>
        /// <param name="color">The colour of every shape appearance.</param>
        /// <param name="thickness">The thickness of every curve and edge.</param>
        /// <returns>The typology appearance.</returns>
        public static Classes.TypologyAppearance TypologyAppearance(System.Drawing.Color color, double thickness = 1)
        {
            Core.Classes.Color color_Temp = new(color);

            return new Classes.TypologyAppearance([new CurveAppearance(color_Temp, thickness), new FaceAppearance(color_Temp, color_Temp, thickness), new MeshAppearance(color_Temp, color_Temp, thickness)]);
        }
    }
}
