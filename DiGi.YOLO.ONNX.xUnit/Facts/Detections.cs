using DiGi.YOLO.Classes;
using System.Collections.Generic;

namespace DiGi.YOLO.ONNX.xUnit
{
    public partial class Facts
    {
        /// <summary>
        /// Verifies that <see cref="Query.Detections(float[], int[], int, string, Classes.LetterBox, Classes.YOLOONNXPredictionOptions)"/> reads a raw detection output the way ultralytics reads it: centred boxes in canvas pixels, filtered by confidence, and carried back to source pixels as a corner and its extents.
        /// <para>The buffer is laid out the way the network answers - one row per value across all anchors, not one row per anchor - so an implementation that transposed it would still produce boxes, just the wrong ones. The transform here doubles, matching the pipeline's 320-pixel images on a 640-pixel canvas, so a detection at canvas 200 has to come back at source 100.</para>
        /// </summary>
        [Fact]
        public void Detections()
        {
            int count_Anchor = 4;
            int count_Channel = 5;

            float[] values = new float[count_Channel * count_Anchor];

            void Set(int anchor, float x, float y, float width, float height, float confidence)
            {
                values[(0 * count_Anchor) + anchor] = x;
                values[(1 * count_Anchor) + anchor] = y;
                values[(2 * count_Anchor) + anchor] = width;
                values[(3 * count_Anchor) + anchor] = height;
                values[(4 * count_Anchor) + anchor] = confidence;
            }

            //A clear detection, centred at canvas (200, 300) and 100 by 80 across
            Set(0, 200, 300, 100, 80, 0.9f);

            //Below the threshold, so it is never a candidate
            Set(1, 400, 400, 50, 50, 0.05f);

            //Far away from the first and comfortably above the threshold
            Set(2, 40, 40, 20, 20, 0.4f);

            //Exactly on the threshold, which ultralytics discards - it keeps what is strictly above it.
            //The threshold is a quarter rather than the default tenth because a quarter is exact in both
            //widths: 0.1f widens to 0.10000000149 as a double and would sit just above a 0.1 threshold,
            //so the boundary this fact is about would never be tested.
            Set(3, 500, 500, 30, 30, 0.25f);

            Classes.LetterBox? letterBox = Create.LetterBox(320, 320, 640);
            Assert.NotNull(letterBox);

            Classes.YOLOONNXPredictionOptions yOLOONNXPredictionOptions = new() { Confidence = 0.25 };

            List<BoundingBoxResult>? boundingBoxResults = Query.Detections(values, [1, count_Channel, count_Anchor], 0, "0207_2021", letterBox, yOLOONNXPredictionOptions);

            Assert.NotNull(boundingBoxResults);
            Assert.Equal(2, boundingBoxResults!.Count);

            //Descending confidence, which is the order ultralytics hands its detections back in and therefore the order predict.py writes them in
            Assert.Equal(0.9, boundingBoxResults[0].Confidence, 5);
            Assert.Equal(0.4, boundingBoxResults[1].Confidence, 5);

            Assert.Equal("0207_2021", boundingBoxResults[0].Name);
            Assert.Equal(0, boundingBoxResults[0].LabelIndex);

            //Canvas centre (200, 300) with extents (100, 80) is a corner at (150, 260); halved by the transform that is (75, 130), 50 by 40
            Assert.Equal(75, boundingBoxResults[0].X, 4);
            Assert.Equal(130, boundingBoxResults[0].Y, 4);
            Assert.Equal(50, boundingBoxResults[0].Width, 4);
            Assert.Equal(40, boundingBoxResults[0].Height, 4);
        }

        /// <summary>
        /// Verifies that two detections of the same class overlapping more than the threshold leave only the stronger one behind, and that the same pair survives when the threshold is raised above their overlap.
        /// </summary>
        [Fact]
        public void Detections_Suppression()
        {
            int count_Anchor = 2;
            int count_Channel = 5;

            //Two boxes of the same size, one shifted by ten canvas pixels out of a hundred, so they overlap by roughly 0.82
            float[] values = [200, 210, 300, 300, 100, 100, 100, 100, 0.9f, 0.8f];

            Classes.LetterBox? letterBox = Create.LetterBox(320, 320, 640);
            Assert.NotNull(letterBox);

            List<BoundingBoxResult>? boundingBoxResults = Query.Detections(values, [1, count_Channel, count_Anchor], 0, "0207_2021", letterBox, new Classes.YOLOONNXPredictionOptions() { IoU = 0.7 });
            Assert.NotNull(boundingBoxResults);
            Assert.Single(boundingBoxResults!);
            Assert.Equal(0.9, boundingBoxResults![0].Confidence, 5);

            List<BoundingBoxResult>? boundingBoxResults_Loose = Query.Detections(values, [1, count_Channel, count_Anchor], 0, "0207_2021", letterBox, new Classes.YOLOONNXPredictionOptions() { IoU = 0.9 });
            Assert.NotNull(boundingBoxResults_Loose);
            Assert.Equal(2, boundingBoxResults_Loose!.Count);
        }

        /// <summary>
        /// Verifies that the detection cap keeps the strongest detections rather than the first ones the anchors happened to be listed in.
        /// </summary>
        [Fact]
        public void Detections_MaxDetections()
        {
            int count_Anchor = 3;
            int count_Channel = 5;

            //Three boxes far enough apart never to suppress one another, listed weakest first
            float[] values = [50, 250, 450, 50, 250, 450, 20, 20, 20, 20, 20, 20, 0.2f, 0.5f, 0.8f];

            Classes.LetterBox? letterBox = Create.LetterBox(320, 320, 640);
            Assert.NotNull(letterBox);

            List<BoundingBoxResult>? boundingBoxResults = Query.Detections(values, [1, count_Channel, count_Anchor], 0, "0207_2021", letterBox, new Classes.YOLOONNXPredictionOptions() { MaxDetections = 2 });

            Assert.NotNull(boundingBoxResults);
            Assert.Equal(2, boundingBoxResults!.Count);
            Assert.Equal(0.8, boundingBoxResults[0].Confidence, 5);
            Assert.Equal(0.5, boundingBoxResults[1].Confidence, 5);
        }

        /// <summary>
        /// Verifies that a detection running off the edge of the canvas is clipped to the source image, so that no stored box describes ground the image does not cover.
        /// </summary>
        [Fact]
        public void Detections_Clipped()
        {
            int count_Anchor = 1;
            int count_Channel = 5;

            //Centred on the canvas origin, so three of its four corners are off the image
            float[] values = [0, 0, 200, 200, 0.9f];

            Classes.LetterBox? letterBox = Create.LetterBox(320, 320, 640);
            Assert.NotNull(letterBox);

            List<BoundingBoxResult>? boundingBoxResults = Query.Detections(values, [1, count_Channel, count_Anchor], 0, "0207_2021", letterBox, new Classes.YOLOONNXPredictionOptions());

            Assert.NotNull(boundingBoxResults);
            Assert.Single(boundingBoxResults!);
            Assert.Equal(0, boundingBoxResults![0].X, 4);
            Assert.Equal(0, boundingBoxResults[0].Y, 4);
            Assert.Equal(50, boundingBoxResults[0].Width, 4);
            Assert.Equal(50, boundingBoxResults[0].Height, 4);
        }

        /// <summary>
        /// Verifies that a buffer, a shape or a transform that does not describe a run is refused rather than read past its end.
        /// </summary>
        [Fact]
        public void Detections_Invalid()
        {
            float[] values = [200, 300, 100, 80, 0.9f];
            Classes.LetterBox? letterBox = Create.LetterBox(320, 320, 640);
            Classes.YOLOONNXPredictionOptions yOLOONNXPredictionOptions = new();

            Assert.Null(Query.Detections(null, [1, 5, 1], 0, "0207_2021", letterBox, yOLOONNXPredictionOptions));
            Assert.Null(Query.Detections(values, null, 0, "0207_2021", letterBox, yOLOONNXPredictionOptions));
            Assert.Null(Query.Detections(values, [5, 1], 0, "0207_2021", letterBox, yOLOONNXPredictionOptions));
            Assert.Null(Query.Detections(values, [1, 5, 1], 1, "0207_2021", letterBox, yOLOONNXPredictionOptions));
            Assert.Null(Query.Detections(values, [1, 5, 1], 0, "0207_2021", null, yOLOONNXPredictionOptions));
            Assert.Null(Query.Detections(values, [1, 5, 1], 0, "0207_2021", letterBox, null));

            //A shape claiming more values than the buffer holds
            Assert.Null(Query.Detections(values, [1, 5, 2], 0, "0207_2021", letterBox, yOLOONNXPredictionOptions));
        }

        /// <summary>
        /// Verifies that settings which cannot describe a run are refused rather than quietly answering that there is nothing on the image.
        /// <para>A detection cap of zero is the dangerous one. Read literally it means "keep no detections", so every image would come back clean and the run would look like a detector that found nothing rather than like a mis-configured one - the exact shape of failure this path exists to rule out.</para>
        /// </summary>
        [Fact]
        public void Detections_InvalidOptions()
        {
            float[] values = [200, 300, 100, 80, 0.9f];
            Classes.LetterBox? letterBox = Create.LetterBox(320, 320, 640);

            Assert.Null(Query.Detections(values, [1, 5, 1], 0, "0207_2021", letterBox, new Classes.YOLOONNXPredictionOptions() { MaxDetections = 0 }));
            Assert.Null(Query.Detections(values, [1, 5, 1], 0, "0207_2021", letterBox, new Classes.YOLOONNXPredictionOptions() { MaxDetections = -1 }));
            Assert.Null(Query.Detections(values, [1, 5, 1], 0, "0207_2021", letterBox, new Classes.YOLOONNXPredictionOptions() { Confidence = double.NaN }));
            Assert.Null(Query.Detections(values, [1, 5, 1], 0, "0207_2021", letterBox, new Classes.YOLOONNXPredictionOptions() { IoU = double.NaN }));

            //The same call with settings that do describe a run answers normally, so the guard above is what refused the others
            Assert.Single(Query.Detections(values, [1, 5, 1], 0, "0207_2021", letterBox, new Classes.YOLOONNXPredictionOptions())!);
        }
    }
}
