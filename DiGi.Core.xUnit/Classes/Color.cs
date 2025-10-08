namespace DiGi.Core.xUnit
{
    public partial class Classes
    {
        [Fact]
        public void Color()
        {
            System.Drawing.Color drawingColor_1 = System.Drawing.Color.Aqua;

            Core.Classes.Color color_1 = new(drawingColor_1);

            string? string_1 = color_1.ToSystem_String();

            Assert.NotNull(string_1);

            Core.Classes.Color? color_2 = Convert.ToDiGi<Core.Classes.Color>(string_1)?.FirstOrDefault();

            Assert.NotNull(color_2);

            string? string_2 = color_2.ToSystem_String();

            Assert.NotNull(string_2);

            Assert.Equal(string_1, string_2);

            Core.Classes.Color? color_3 = Convert.ToDiGi<Core.Classes.Color>(string_2)?.FirstOrDefault();

            Assert.Equal(color_3.ToSystem_String(), string_2);

            System.Drawing.Color drawingColor_2 = color_3.ToDrawing();

            Assert.Equal(drawingColor_1.A, drawingColor_2.A);
            Assert.Equal(drawingColor_1.R, drawingColor_2.R);
            Assert.Equal(drawingColor_1.G, drawingColor_2.G);
            Assert.Equal(drawingColor_1.B, drawingColor_2.B);

            Query.SerializationCheck(color_1);
        }
    }
}