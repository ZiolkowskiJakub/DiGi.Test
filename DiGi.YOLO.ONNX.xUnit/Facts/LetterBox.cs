namespace DiGi.YOLO.ONNX.xUnit
{
    public partial class Facts
    {
        /// <summary>
        /// Verifies that <see cref="Create.LetterBox(int, int, int)"/> reproduces ultralytics' own transform, including the case this pipeline actually runs.
        /// <para>The images the pipeline scores are 320 pixels square and the frozen weights were trained at 640, so the transform is a plain doubling with no padding at all. That is worth pinning down: a guard against enlarging an image - which several letterbox implementations carry - would leave the detector looking at a quarter of the canvas it was trained on, and would do it silently.</para>
        /// </summary>
        [Fact]
        public void LetterBox()
        {
            Classes.LetterBox? letterBox = Create.LetterBox(320, 320, 640);

            Assert.NotNull(letterBox);
            Assert.Equal(2, letterBox!.Scale);
            Assert.Equal(640, letterBox.Width);
            Assert.Equal(640, letterBox.Height);
            Assert.Equal(0, letterBox.OffsetX);
            Assert.Equal(0, letterBox.OffsetY);
            Assert.Equal(320, letterBox.SourceWidth);
            Assert.Equal(320, letterBox.SourceHeight);
            Assert.Equal(640, letterBox.Size);
        }

        /// <summary>
        /// Verifies that a source image which is not square is fitted whole and padded on the shorter side.
        /// </summary>
        [Fact]
        public void LetterBox_Padded()
        {
            Classes.LetterBox? letterBox = Create.LetterBox(640, 480, 640);

            Assert.NotNull(letterBox);
            Assert.Equal(1, letterBox!.Scale);
            Assert.Equal(640, letterBox.Width);
            Assert.Equal(480, letterBox.Height);
            Assert.Equal(0, letterBox.OffsetX);

            //Eighty rows above and eighty below, which is what ultralytics halves the remainder into
            Assert.Equal(80, letterBox.OffsetY);
            Assert.True(letterBox.Height + (2 * letterBox.OffsetY) == letterBox.Size);
        }

        /// <summary>
        /// Verifies that a remainder which does not halve evenly puts the smaller border above the content, the way ultralytics rounds it.
        /// </summary>
        [Fact]
        public void LetterBox_OddRemainder()
        {
            Classes.LetterBox? letterBox = Create.LetterBox(640, 639, 640);

            Assert.NotNull(letterBox);
            Assert.Equal(639, letterBox!.Height);

            //A remainder of one pixel halves to 0.5, and ultralytics takes a tenth off before rounding, so the top border is none and the bottom border is the whole pixel
            Assert.Equal(0, letterBox.OffsetY);
        }

        /// <summary>
        /// Verifies that a size which cannot describe an image is rejected rather than producing a transform that divides by zero further on.
        /// </summary>
        [Fact]
        public void LetterBox_Invalid()
        {
            Assert.Null(Create.LetterBox(0, 320, 640));
            Assert.Null(Create.LetterBox(320, 0, 640));
            Assert.Null(Create.LetterBox(320, 320, 0));
            Assert.Null(Create.LetterBox(-320, 320, 640));
        }
    }
}
