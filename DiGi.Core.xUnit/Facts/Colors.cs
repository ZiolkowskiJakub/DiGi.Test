using System.Collections.Generic;
using System.Reflection;

namespace DiGi.Core.xUnit
{
    public partial class Facts
    {
        /// <summary>
        /// Tests Create.Colors for null handling, bucket counts, determinism, the OKLab lightness band, sequential anchor handling, and the diverging centre bucket.
        /// <para>Expected anchor and middle colors are computed in the fact with the same lightness clamp the factory applies, so the assertions pin the behavior rather than a hardcoded byte triple.</para>
        /// </summary>
        [Fact]
        public void Colors()
        {
            Core.Enums.ColorSchemeType[] colorSchemeTypes = [Core.Enums.ColorSchemeType.Sequential, Core.Enums.ColorSchemeType.Diverging, Core.Enums.ColorSchemeType.Categorical];
            int[] counts = [1, 2, 5, 12, 24];

            foreach (Core.Enums.ColorSchemeType colorSchemeType in colorSchemeTypes)
            {
                Assert.Null(colorSchemeType.Colors(0));

                foreach (int count in counts)
                {
                    List<Core.Classes.Color> colors = colorSchemeType.Colors(count)!;
                    Assert.Equal(count, colors.Count);

                    foreach (Core.Classes.Color color in colors)
                    {
                        double lightness = color.ToOKLab().L;
                        Assert.InRange(lightness, 0.14, 0.94);
                    }

                    List<Core.Classes.Color> colors_Again = colorSchemeType.Colors(count)!;
                    for (int i = 0; i < count; i++)
                    {
                        Assert.Equal(colors[i], colors_Again[i]);
                    }
                }
            }

            Core.Classes.Color color_Start = new(255, 200, 0, 0);
            Core.Classes.Color color_End = new(255, 0, 0, 200);

            List<Core.Classes.Color> colors_Sequential = Core.Enums.ColorSchemeType.Sequential.Colors(8, color_Start, color_End)!;
            Assert.Equal(ClampLightness(color_Start.ToOKLab()).ToDiGi(), colors_Sequential[0]);
            Assert.Equal(ClampLightness(color_End.ToOKLab()).ToDiGi(), colors_Sequential[7]);

            List<Core.Classes.Color> colors_Sequential_Single = Core.Enums.ColorSchemeType.Sequential.Colors(1)!;
            Assert.Equal(ClampLightness(new Core.Classes.Color(Core.Constants.ColorScheme.SequentialEnd).ToOKLab()).ToDiGi(), colors_Sequential_Single[0]);

            List<Core.Classes.Color> colors_Diverging = Core.Enums.ColorSchemeType.Diverging.Colors(7)!;
            Assert.Equal(ClampLightness(new Core.Classes.Color(Core.Constants.ColorScheme.DivergingMiddle).ToOKLab()).ToDiGi(), colors_Diverging[3]);

            Core.Classes.Color colors_Diverging_Single = Core.Enums.ColorSchemeType.Diverging.Colors(1)![0];
            Assert.Equal(ClampLightness(new Core.Classes.Color(Core.Constants.ColorScheme.DivergingMiddle).ToOKLab()).ToDiGi(), colors_Diverging_Single);

            Core.Classes.OKLab ClampLightness(Core.Classes.OKLab okLab)
            {
                double lightness = System.Math.Max(Core.Constants.ColorScheme.LightnessMin, System.Math.Min(Core.Constants.ColorScheme.LightnessMax, okLab.L));

                return new Core.Classes.OKLab(lightness, okLab.A, okLab.B);
            }
        }

        /// <summary>
        /// Tests that the 12 curated categorical colors sit at least Constants.ColorScheme.CategoricalMinimumDistance apart in OKLab, and reports the achieved minimum distances for 12 and 24 buckets.
        /// <para>The floor is asserted for the curated palette only; beyond 12 buckets the golden-angle fallback is a best effort and its minimum distance is reported, not asserted.</para>
        /// </summary>
        [Fact]
        public void Colors_CategoricalDistance()
        {
            string? path_Reports = Core.xUnit.Query.ReportsDirectory(Assembly.GetExecutingAssembly());
            Assert.False(string.IsNullOrWhiteSpace(path_Reports));

            int[] counts = [12, 24];
            List<string> reportLines = [];

            foreach (int count in counts)
            {
                List<Core.Classes.Color> colors = Core.Enums.ColorSchemeType.Categorical.Colors(count)!;
                Assert.Equal(count, colors.Count);

                double minimumDistance = double.PositiveInfinity;

                for (int i = 0; i < count; i++)
                {
                    for (int j = i + 1; j < count; j++)
                    {
                        Core.Classes.OKLab okLab_i = colors[i].ToOKLab();
                        Core.Classes.OKLab okLab_j = colors[j].ToOKLab();
                        double distance = System.Math.Sqrt(
                            (okLab_i.L - okLab_j.L) * (okLab_i.L - okLab_j.L) +
                            (okLab_i.A - okLab_j.A) * (okLab_i.A - okLab_j.A) +
                            (okLab_i.B - okLab_j.B) * (okLab_i.B - okLab_j.B));

                        if (distance < minimumDistance)
                        {
                            minimumDistance = distance;
                        }
                    }
                }

                if (count == 12)
                {
                    Assert.True(minimumDistance >= Core.Constants.ColorScheme.CategoricalMinimumDistance, $"Minimum pairwise OKLab distance at 12 buckets is {minimumDistance:0.0000}, below the {Core.Constants.ColorScheme.CategoricalMinimumDistance} floor.");
                }

                reportLines.Add($"count = {count}: minimum pairwise OKLab distance = {minimumDistance:0.0000}");
            }

            System.IO.File.WriteAllLines(System.IO.Path.Combine(path_Reports!, "colors_categorical_distance.txt"), reportLines);
        }

        /// <summary>
        /// Renders the three color schemes at 8 buckets into a swatch report in user files/reports/colors.html for the visual check.
        /// </summary>
        [Fact]
        public void Colors_Report()
        {
            string? path_Reports = Core.xUnit.Query.ReportsDirectory(Assembly.GetExecutingAssembly());
            Assert.False(string.IsNullOrWhiteSpace(path_Reports));

            Core.Enums.ColorSchemeType[] colorSchemeTypes = [Core.Enums.ColorSchemeType.Sequential, Core.Enums.ColorSchemeType.Diverging, Core.Enums.ColorSchemeType.Categorical];

            List<string> lines = [];
            lines.Add("<!DOCTYPE html>");
            lines.Add("<html>");
            lines.Add("<head>");
            lines.Add("<meta charset=\"utf-8\">");
            lines.Add("<title>DiGi.Core Create.Colors</title>");
            lines.Add("<style>body { font-family: 'Segoe UI', sans-serif; } table { border-collapse: collapse; margin-bottom: 16px; } td { width: 72px; height: 48px; text-align: center; font-size: 11px; vertical-align: bottom; padding: 2px; border: 1px solid #dddddd; }</style>");
            lines.Add("</head>");
            lines.Add("<body>");
            lines.Add("<h1>Create.Colors</h1>");
            lines.Add("<p>Generated by DiGi.Core Create.Colors with count = 8. OKLab lightness band [0.15, 0.93]; anchors outside the band are clamped to it.</p>");

            foreach (Core.Enums.ColorSchemeType colorSchemeType in colorSchemeTypes)
            {
                List<Core.Classes.Color> colors = colorSchemeType.Colors(8)!;

                lines.Add($"<h2>{colorSchemeType}</h2>");
                lines.Add("<table><tr>");

                foreach (Core.Classes.Color color in colors)
                {
                    string hex = $"#{color.Red:X2}{color.Green:X2}{color.Blue:X2}";
                    lines.Add($"<td style=\"background:#{hex.Substring(1)}\">{hex}</td>");
                }

                lines.Add("</tr></table>");
            }

            lines.Add("</body>");
            lines.Add("</html>");

            System.IO.File.WriteAllLines(System.IO.Path.Combine(path_Reports!, "colors.html"), lines);
        }
    }
}
