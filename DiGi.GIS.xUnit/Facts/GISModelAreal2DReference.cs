namespace DiGi.GIS.xUnit
{
    public partial class Facts
    {
        /// <summary>
        /// Tests the creation and property assignment of a <see cref="Classes.GISModelAreal2DReference"/> object based on various input reference string formats, verifying that the Areal2DReference and GISModelReference properties are correctly populated.
        /// </summary>
        [Fact]
        public void GISModelAreal2DReference()
        {
            string reference;
            Classes.GISModelAreal2DReference? gISModelAreal2DReference;

            reference = string.Empty;
            gISModelAreal2DReference = Create.GISModelAreal2DReference(reference);
            Assert.Null(gISModelAreal2DReference);

            reference = "AAA";
            gISModelAreal2DReference = Create.GISModelAreal2DReference(reference);
            Assert.NotNull(gISModelAreal2DReference);

            Assert.NotNull(gISModelAreal2DReference.Areal2DReference);
            Assert.Null(gISModelAreal2DReference.GISModelReference);

            reference = "[AAA]";
            gISModelAreal2DReference = Create.GISModelAreal2DReference(reference);
            Assert.NotNull(gISModelAreal2DReference);

            Assert.Null(gISModelAreal2DReference.Areal2DReference);
            Assert.NotNull(gISModelAreal2DReference.GISModelReference);

            reference = "[AAA]BBB";
            gISModelAreal2DReference = Create.GISModelAreal2DReference(reference);
            Assert.NotNull(gISModelAreal2DReference);

            Assert.NotNull(gISModelAreal2DReference.Areal2DReference);
            Assert.NotNull(gISModelAreal2DReference.GISModelReference);

            reference = "[AAA]BBB";
            Classes.GISModelAreal2DReference? gISModelAreal2DReference_1 = Create.GISModelAreal2DReference(reference);
            Assert.NotNull(gISModelAreal2DReference_1);

            Classes.GISModelAreal2DReference? gISModelAreal2DReference_2 = Create.GISModelAreal2DReference(reference);
            Assert.NotNull(gISModelAreal2DReference_2);

            Assert.True(gISModelAreal2DReference_1.Equals(gISModelAreal2DReference_2));
            Assert.True(gISModelAreal2DReference_1 == gISModelAreal2DReference_2);
        }
    }
}