namespace DiGi.Solar.xUnit
{
    public partial class Facts
    {
        /// <summary>
        /// Carries one hourly row of the committed PVGIS reference series.
        /// <para>Irradiance values are W/m2. The horizontal columns describe a plane of slope 0, the tilted columns a plane of slope 35 degrees facing south.</para>
        /// </summary>
        public class PVGISReferenceRow
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="PVGISReferenceRow"/> class.
            /// </summary>
            /// <param name="dateTime">The instant the row describes, in UTC.</param>
            /// <param name="sunElevationDegrees">The solar elevation published by PVGIS, in degrees above the horizon.</param>
            /// <param name="beamHorizontal">The direct beam irradiance on a horizontal plane, in W/m2.</param>
            /// <param name="diffuseHorizontal">The sky diffuse irradiance on a horizontal plane, in W/m2.</param>
            /// <param name="beamTilted35">The direct beam irradiance on the tilted plane, in W/m2.</param>
            /// <param name="diffuseTilted35">The sky diffuse irradiance on the tilted plane, in W/m2.</param>
            /// <param name="groundTilted35">The ground-reflected irradiance on the tilted plane, in W/m2.</param>
            public PVGISReferenceRow(System.DateTime dateTime, double sunElevationDegrees, double beamHorizontal, double diffuseHorizontal, double beamTilted35, double diffuseTilted35, double groundTilted35)
            {
                DateTime = dateTime;
                SunElevationDegrees = sunElevationDegrees;
                BeamHorizontal = beamHorizontal;
                DiffuseHorizontal = diffuseHorizontal;
                BeamTilted35 = beamTilted35;
                DiffuseTilted35 = diffuseTilted35;
                GroundTilted35 = groundTilted35;
            }

            /// <summary>
            /// Gets the instant the row describes, in UTC.
            /// </summary>
            public System.DateTime DateTime { get; }

            /// <summary>
            /// Gets the solar elevation published by PVGIS, in degrees above the horizon.
            /// </summary>
            public double SunElevationDegrees { get; }

            /// <summary>
            /// Gets the direct beam irradiance on a horizontal plane, in W/m2.
            /// </summary>
            public double BeamHorizontal { get; }

            /// <summary>
            /// Gets the sky diffuse irradiance on a horizontal plane, in W/m2.
            /// </summary>
            public double DiffuseHorizontal { get; }

            /// <summary>
            /// Gets the direct beam irradiance on the tilted plane, in W/m2.
            /// </summary>
            public double BeamTilted35 { get; }

            /// <summary>
            /// Gets the sky diffuse irradiance on the tilted plane, in W/m2.
            /// </summary>
            public double DiffuseTilted35 { get; }

            /// <summary>
            /// Gets the ground-reflected irradiance on the tilted plane, in W/m2.
            /// </summary>
            public double GroundTilted35 { get; }
        }
    }
}
