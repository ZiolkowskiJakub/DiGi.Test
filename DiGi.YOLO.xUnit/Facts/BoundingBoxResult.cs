using System.Globalization;

namespace DiGi.YOLO.xUnit
{
    public partial class Facts
    {
        /// <summary>
        /// Verifies that <see cref="Create.BoundingBoxResult(string?)"/> parses a well formed tab separated line and rejects lines that are too short or that hold non numeric values.
        /// </summary>
        [Fact]
        public void BoundingBoxResult()
        {
            Classes.BoundingBoxResult? boundingBoxResult = Create.BoundingBoxResult("0207_2021\t0\t12.5\t20.25\t40.5\t60.75\t0.93");

            Assert.NotNull(boundingBoxResult);
            Assert.Equal("0207_2021", boundingBoxResult!.Name);
            Assert.Equal(0, boundingBoxResult.LabelIndex);
            Assert.Equal(12.5, boundingBoxResult.X);
            Assert.Equal(20.25, boundingBoxResult.Y);
            Assert.Equal(40.5, boundingBoxResult.Width);
            Assert.Equal(60.75, boundingBoxResult.Height);
            Assert.Equal(0.93, boundingBoxResult.Confidence);

            Assert.Null(Create.BoundingBoxResult(null));
            Assert.Null(Create.BoundingBoxResult(string.Empty));

            //A line holding only the image name is how predict.py records an image with no detections
            Assert.Null(Create.BoundingBoxResult("0207_2021"));
            Assert.Null(Create.BoundingBoxResult("0207_2021\t0\t12.5\t20.25\t40.5\t60.75"));

            Assert.Null(Create.BoundingBoxResult("0207_2021\tlabel\t12.5\t20.25\t40.5\t60.75\t0.93"));
            Assert.Null(Create.BoundingBoxResult("0207_2021\t0\tx\t20.25\t40.5\t60.75\t0.93"));
        }

        /// <summary>
        /// Verifies that <see cref="Create.BoundingBoxResult(string?)"/> reads the invariant decimal point written by predict.py while a comma decimal culture is current.
        /// <para>The file is produced by Python and is invariant by construction, so parsing it under the current culture rejects every detection line on a machine whose culture uses a comma, and the caller sees an empty result rather than an error.</para>
        /// </summary>
        [Fact]
        public void BoundingBoxResult_Culture()
        {
            CultureInfo cultureInfo = CultureInfo.CurrentCulture;

            try
            {
                CultureInfo.CurrentCulture = new CultureInfo("pl-PL");

                Classes.BoundingBoxResult? boundingBoxResult = Create.BoundingBoxResult("0207_2021\t0\t12.5\t20.25\t40.5\t60.75\t0.93");

                Assert.NotNull(boundingBoxResult);
                Assert.Equal(12.5, boundingBoxResult!.X);
                Assert.Equal(20.25, boundingBoxResult.Y);
                Assert.Equal(40.5, boundingBoxResult.Width);
                Assert.Equal(60.75, boundingBoxResult.Height);
                Assert.Equal(0.93, boundingBoxResult.Confidence);
            }
            finally
            {
                CultureInfo.CurrentCulture = cultureInfo;
            }
        }

        /// <summary>
        /// Verifies that <see cref="Classes.BoundingBoxResult.ToString()"/> includes confidence and invariant decimal formatting, and can be parsed back by <see cref="Create.BoundingBoxResult(string?)"/>.
        /// </summary>
        [Fact]
        public void BoundingBoxResult_ToString()
        {
            Classes.BoundingBoxResult boundingBoxResult = new("0207_2021", 0, 12.5, 20.25, 40.5, 60.75, 0.93);
            string formatted = boundingBoxResult.ToString();

            Assert.Equal("0207_2021\t0\t12.5\t20.25\t40.5\t60.75\t0.93", formatted);

            Classes.BoundingBoxResult? parsed = Create.BoundingBoxResult(formatted);
            Assert.NotNull(parsed);
            Assert.Equal(boundingBoxResult, parsed);
        }

        /// <summary>
        /// Verifies that <see cref="Classes.BoundingBoxResult.Equals(object?)"/> and <see cref="Classes.BoundingBoxResult.GetHashCode()"/> accurately evaluate member equality.
        /// </summary>
        [Fact]
        public void BoundingBoxResult_Equals()
        {
            Classes.BoundingBoxResult result1 = new("box1", 1, 10.0, 20.0, 30.0, 40.0, 0.85);
            Classes.BoundingBoxResult result2 = new("box1", 1, 10.0, 20.0, 30.0, 40.0, 0.85);
            Classes.BoundingBoxResult resultDifferentConfidence = new("box1", 1, 10.0, 20.0, 30.0, 40.0, 0.95);
            Classes.BoundingBoxResult resultDifferentName = new("box2", 1, 10.0, 20.0, 30.0, 40.0, 0.85);

            Assert.Equal(result1, result2);
            Assert.Equal(result1.GetHashCode(), result2.GetHashCode());
            Assert.NotEqual(result1, resultDifferentConfidence);
            Assert.NotEqual(result1, resultDifferentName);
        }
    }
}
