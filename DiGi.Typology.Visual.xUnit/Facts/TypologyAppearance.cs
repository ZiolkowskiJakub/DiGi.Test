using DiGi.Geometry.Visual.Core.Classes;
using DiGi.Geometry.Visual.Core.Interfaces;
using DiGi.Typology.Visual.Classes;

namespace DiGi.Typology.Visual.xUnit
{
    public partial class Facts
    {
        /// <summary>
        /// Tests <see cref="Classes.TypologyAppearance"/>: an appearance is filed and matched under its exact runtime type,
        /// filing a second one of the same type replaces the first, removal and containment follow the type, the
        /// serialized form lists the appearances in a canonical order whatever order they were filed in, the copy
        /// constructor deep-copies, and the container survives a string round trip and SerializationCheck with every
        /// shape kind filed.
        /// <para>Exact matching is the point of the type key: <c>CurveAppearance</c> derives from <c>PointAppearance</c>,
        /// so an assignable-to lookup could not tell which of the two a filed <c>CurveAppearance</c> answers for.</para>
        /// </summary>
        [Fact]
        public void TypologyAppearance()
        {
            Core.Classes.Color color_Red = new(System.Drawing.Color.Red);
            Core.Classes.Color color_Blue = new(System.Drawing.Color.Blue);

            CurveAppearance curveAppearance = new(color_Red, 2);
            FaceAppearance faceAppearance = new(color_Red, color_Blue, 1);

            Classes.TypologyAppearance typologyAppearance = new([faceAppearance, null, curveAppearance]);

            Assert.Equal(2, typologyAppearance.Count);
            Assert.Same(curveAppearance, typologyAppearance[typeof(CurveAppearance)]);
            Assert.Same(faceAppearance, typologyAppearance[typeof(FaceAppearance)]);

            // Exact match: a CurveAppearance is a PointAppearance, but is not filed as one.
            Assert.Null(typologyAppearance[typeof(PointAppearance)]);
            Assert.False(typologyAppearance.Contains(typeof(PointAppearance)));
            Assert.True(typologyAppearance.Contains(typeof(CurveAppearance)));
            Assert.Null(typologyAppearance[null]);
            Assert.False(typologyAppearance.Contains(null));

            // Filing the same kind again replaces it.
            CurveAppearance curveAppearance_Blue = new(color_Blue, 3);

            Assert.True(typologyAppearance.Add(curveAppearance_Blue));
            Assert.False(typologyAppearance.Add(null));
            Assert.Equal(2, typologyAppearance.Count);
            Assert.Same(curveAppearance_Blue, typologyAppearance[typeof(CurveAppearance)]);

            // Types are reported in ordinal order of their full names, whatever the filing order.
            Assert.Equal([typeof(CurveAppearance), typeof(FaceAppearance)], typologyAppearance.Types);

            // Removal follows the type.
            Assert.True(typologyAppearance.Remove(typeof(FaceAppearance)));
            Assert.False(typologyAppearance.Remove(typeof(FaceAppearance)));
            Assert.False(typologyAppearance.Remove(null));
            Assert.Equal(1, typologyAppearance.Count);
            Assert.Null(typologyAppearance[typeof(FaceAppearance)]);

            // The serialized form is canonical: two containers filed in opposite orders serialize identically.
            PointAppearance pointAppearance = new(color_Red, 1);
            MeshAppearance meshAppearance = new(color_Red, color_Blue, 1);
            SurfaceAppearance surfaceAppearance = new(color_Red, color_Blue, 1);

            Classes.TypologyAppearance typologyAppearance_1 = new([pointAppearance, curveAppearance, faceAppearance, meshAppearance, surfaceAppearance]);
            Classes.TypologyAppearance typologyAppearance_2 = new([surfaceAppearance, meshAppearance, faceAppearance, curveAppearance, pointAppearance]);

            string? json_1 = Core.Convert.ToSystem_String(typologyAppearance_1);
            string? json_2 = Core.Convert.ToSystem_String(typologyAppearance_2);

            Assert.False(string.IsNullOrWhiteSpace(json_1));
            Assert.Equal(json_1, json_2);

            // The copy constructor deep-copies: mutating the copy leaves the source alone.
            Classes.TypologyAppearance typologyAppearance_Copy = new(typologyAppearance_1);

            Assert.Equal(5, typologyAppearance_Copy.Count);
            Assert.Equal(json_1, Core.Convert.ToSystem_String(typologyAppearance_Copy));

            IAppearance? appearance_Copy = typologyAppearance_Copy[typeof(FaceAppearance)];

            Assert.NotNull(appearance_Copy);
            Assert.NotSame(faceAppearance, appearance_Copy);

            appearance_Copy.Opacity = 0.5;

            Assert.Equal(1, faceAppearance.Opacity);
            Assert.NotEqual(json_1, Core.Convert.ToSystem_String(typologyAppearance_Copy));

            // String round trip and SerializationCheck with every shape kind filed.
            Classes.TypologyAppearance? typologyAppearance_RoundTrip = Core.Convert.ToDiGi<Classes.TypologyAppearance>(json_1)?.FirstOrDefault();

            Assert.NotNull(typologyAppearance_RoundTrip);
            Assert.Equal(5, typologyAppearance_RoundTrip.Count);
            Assert.IsType<CurveAppearance>(typologyAppearance_RoundTrip[typeof(CurveAppearance)]);
            Assert.IsType<PointAppearance>(typologyAppearance_RoundTrip[typeof(PointAppearance)]);
            Assert.IsType<FaceAppearance>(typologyAppearance_RoundTrip[typeof(FaceAppearance)]);
            Assert.IsType<MeshAppearance>(typologyAppearance_RoundTrip[typeof(MeshAppearance)]);
            Assert.IsType<SurfaceAppearance>(typologyAppearance_RoundTrip[typeof(SurfaceAppearance)]);
            Assert.Equal(2, (typologyAppearance_RoundTrip[typeof(CurveAppearance)] as CurveAppearance)?.Thickness);

            Core.xUnit.Query.SerializationCheck(typologyAppearance_1);
        }
    }
}
