using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using System.IO;
using System.Reflection;

namespace DiGi.YOLO.ONNX.xUnit
{
    public partial class Facts
    {
        /// <summary>
        /// Verifies that <see cref="Modify.Predict(Classes.YOLOONNXPredictionOptions, System.Threading.CancellationToken)"/> rejects options that name no run at all.
        /// </summary>
        [Fact]
        public void Predict_Invalid()
        {
            Assert.Null(Modify.Predict(null));
            Assert.Null(Modify.Predict(new Classes.YOLOONNXPredictionOptions()));
            Assert.Null(Modify.Predict(new Classes.YOLOONNXPredictionOptions() { ModelPath = @"C:\YOLO\models\model.onnx" }));
        }

        /// <summary>
        /// Verifies that a source directory holding no images is answered without the model ever being loaded.
        /// <para>The model path names nothing on disk here, so a run that reached the session would fail. It succeeds instead, which is the point: an empty directory is a run with nothing to do rather than a run that went wrong, and the CPython path draws the same distinction.</para>
        /// </summary>
        [Fact]
        public void Predict_NoImages()
        {
            string directory = Path.Combine(Path.GetTempPath(), "DiGi_YOLO_ONNX_Test_" + Path.GetRandomFileName());

            try
            {
                string directory_Source = Path.Combine(directory, "input");
                Directory.CreateDirectory(directory_Source);

                Classes.YOLOONNXPredictionOptions yOLOONNXPredictionOptions = new()
                {
                    ModelPath = Path.Combine(directory, "model.onnx"),
                    OutputPath = Path.Combine(directory, "output", "results.bbrf"),
                    SourceDirectory = directory_Source
                };

                File.WriteAllText(yOLOONNXPredictionOptions.ModelPath!, "not a model, but a file that exists");

                Classes.YOLOONNXPredictionResult? yOLOONNXPredictionResult = Modify.Predict(yOLOONNXPredictionOptions);

                Assert.NotNull(yOLOONNXPredictionResult);
                Assert.True(yOLOONNXPredictionResult!.Succeeded);
                Assert.Equal(0, yOLOONNXPredictionResult.ImageCount);
                Assert.Empty(yOLOONNXPredictionResult.Values!);
                Assert.False(File.Exists(yOLOONNXPredictionResult.OutputPath));
            }
            finally
            {
                if (Directory.Exists(directory))
                {
                    Directory.Delete(directory, true);
                }
            }
        }

        /// <summary>
        /// Verifies that a missing source directory is reported rather than thrown, so an unattended run learns why it scored nothing.
        /// </summary>
        [Fact]
        public void Predict_MissingSourceDirectory()
        {
            Classes.YOLOONNXPredictionOptions yOLOONNXPredictionOptions = new()
            {
                ModelPath = Path.Combine(Path.GetTempPath(), "model.onnx"),
                OutputPath = Path.Combine(Path.GetTempPath(), "results.bbrf"),
                SourceDirectory = Path.Combine(Path.GetTempPath(), "DiGi_YOLO_ONNX_Missing_" + Path.GetRandomFileName())
            };

            Classes.YOLOONNXPredictionResult? yOLOONNXPredictionResult = Modify.Predict(yOLOONNXPredictionOptions);

            Assert.NotNull(yOLOONNXPredictionResult);
            Assert.False(yOLOONNXPredictionResult!.Succeeded);
            Assert.NotNull(yOLOONNXPredictionResult.Messages);
            Assert.Contains(yOLOONNXPredictionResult.Messages!, x => x.Contains("Source directory does not exist"));
        }

        /// <summary>
        /// Verifies that a model which will not load is reported with the reason, and that the stale result file of an earlier run is gone rather than left to be read back as this run's answer.
        /// </summary>
        [Fact]
        public void Predict_BadModel()
        {
            string directory = Path.Combine(Path.GetTempPath(), "DiGi_YOLO_ONNX_Test_" + Path.GetRandomFileName());

            try
            {
                string directory_Source = Path.Combine(directory, "input");
                Directory.CreateDirectory(directory_Source);

                //One image, so the run gets as far as loading the model
                File.WriteAllBytes(Path.Combine(directory_Source, "0207_2021.jpeg"), [0xFF, 0xD8, 0xFF, 0xD9]);

                string path_Model = Path.Combine(directory, "model.onnx");
                File.WriteAllText(path_Model, "not a model");

                string path_Output = Path.Combine(directory, "output", "results.bbrf");
                Directory.CreateDirectory(Path.GetDirectoryName(path_Output)!);
                File.WriteAllText(path_Output, "0000_1900\t0\t1\t2\t3\t4\t0.5");

                Classes.YOLOONNXPredictionOptions yOLOONNXPredictionOptions = new()
                {
                    ModelPath = path_Model,
                    OutputPath = path_Output,
                    SourceDirectory = directory_Source
                };

                Classes.YOLOONNXPredictionResult? yOLOONNXPredictionResult = Modify.Predict(yOLOONNXPredictionOptions);

                Assert.NotNull(yOLOONNXPredictionResult);
                Assert.False(yOLOONNXPredictionResult!.Succeeded);
                Assert.Null(yOLOONNXPredictionResult.Values);
                Assert.NotNull(yOLOONNXPredictionResult.Messages);
                Assert.Contains(yOLOONNXPredictionResult.Messages!, x => x.Contains("Model could not be loaded"));

                //The earlier run's answer must not survive a failed run
                Assert.False(File.Exists(path_Output));
            }
            finally
            {
                if (Directory.Exists(directory))
                {
                    Directory.Delete(directory, true);
                }
            }
        }

        /// <summary>
        /// Verifies that a graph exported as the end-to-end (NMS-free) head is refused before any image is scored, by its metadata and, when it carries none, by its declared output.
        /// <para>Ultralytics 8.4 exports a model that has a one-to-one head as that head when told nms=False, answering [batch, max_det, 6]. That is a different detector from the one-to-many head the CPython path scores, and every check in the decoder lets the layout through, so without this refusal the run would decode nonsense and report success.</para>
        /// <para>The fixtures are minimal graphs of a few hundred bytes built with onnx.helper (opset 12, IR 7): images [batch, 3, 640, 640] through ReduceMean, Reshape and Expand to output0 [batch, 300, 6]. YOLO_End2End.onnx carries the metadata ultralytics writes (end2end True); YOLO_End2End_NoMetadata.onnx carries none, so only its shape can give it away.</para>
        /// </summary>
        [Fact]
        public void Predict_End2EndModel()
        {
            Classes.YOLOONNXPredictionResult? yOLOONNXPredictionResult_Metadata = Predict_Fixture("YOLO_End2End.onnx", out bool exists_Metadata);

            Assert.NotNull(yOLOONNXPredictionResult_Metadata);
            Assert.False(yOLOONNXPredictionResult_Metadata!.Succeeded);
            Assert.Null(yOLOONNXPredictionResult_Metadata.Values);
            Assert.Contains(yOLOONNXPredictionResult_Metadata.Messages!, x => x.Contains("end-to-end (NMS-free) export"));
            Assert.False(exists_Metadata);

            Classes.YOLOONNXPredictionResult? yOLOONNXPredictionResult_Shape = Predict_Fixture("YOLO_End2End_NoMetadata.onnx", out bool exists_Shape);

            Assert.NotNull(yOLOONNXPredictionResult_Shape);
            Assert.False(yOLOONNXPredictionResult_Shape!.Succeeded);
            Assert.Null(yOLOONNXPredictionResult_Shape.Values);
            Assert.Contains(yOLOONNXPredictionResult_Shape.Messages!, x => x.Contains("Model declares an output of [-1, 300, 6]"));
            Assert.False(exists_Shape);
        }

        /// <summary>
        /// Verifies that a graph declaring the raw [batch, 4 + classes, anchors] layout passes the refusal of end-to-end graphs, so the guard does not turn away the detector it exists to protect.
        /// <para>The fixture YOLO_Raw.onnx is built like YOLO_End2End.onnx but expands to output0 [batch, 5, 8400] and carries end2end False, which is what ultralytics writes for a raw export. Its one image is black, so every anchor scores zero and the run succeeds with a line carrying only the image's name.</para>
        /// </summary>
        [Fact]
        public void Predict_RawModel()
        {
            Classes.YOLOONNXPredictionResult? yOLOONNXPredictionResult = Predict_Fixture("YOLO_Raw.onnx", out bool exists);

            Assert.NotNull(yOLOONNXPredictionResult);
            Assert.True(yOLOONNXPredictionResult!.Succeeded, string.Join(" | ", yOLOONNXPredictionResult.Messages ?? []));
            Assert.Equal("0207_2021", Assert.Single(yOLOONNXPredictionResult.Values!));
            Assert.DoesNotContain(yOLOONNXPredictionResult.Messages ?? [], x => x.Contains("end-to-end") || x.Contains("Model declares an output"));
            Assert.True(exists);
        }

        /// <summary>
        /// Verifies that a single undecodable image is reported and skipped rather than aborting the run, so one bad file among many no longer discards the whole directory's result.
        /// <para>Emgu 4.12's Imread throws instead of returning an empty Mat when a file will not decode - a truncated image raises ArgumentException and a 0-byte file raises CvException - so without handling both the exception escapes the per-image loop, the run is marked failed, and the half-written result file is deleted. The three images use one extension each so the listing order is fixed by the *.jpg / *.jpeg / *.png glob, and the valid one is black so it scores to a name-only line like the undecodable ones.</para>
        /// </summary>
        [Fact]
        public void Predict_UndecodableImage()
        {
            string? path_Model = Core.xUnit.Query.FilePath(Assembly.GetExecutingAssembly(), "YOLO_Raw.onnx");
            Assert.True(File.Exists(path_Model), "YOLO_Raw.onnx");

            string directory = Path.Combine(Path.GetTempPath(), "DiGi_YOLO_ONNX_Test_" + Path.GetRandomFileName());

            try
            {
                string directory_Source = Path.Combine(directory, "input");
                Directory.CreateDirectory(directory_Source);

                //One image per extension, so the listing order is fixed by the glob: the truncated image, the valid black image, the 0-byte file
                File.WriteAllBytes(Path.Combine(directory_Source, "undecodable_4byte.jpg"), [0xFF, 0xD8, 0xFF, 0xD9]);

                using (Mat mat = new(32, 32, DepthType.Cv8U, 3))
                {
                    mat.SetTo(new MCvScalar(0, 0, 0));
                    CvInvoke.Imwrite(Path.Combine(directory_Source, "0207_2021.jpeg"), mat);
                }

                File.WriteAllBytes(Path.Combine(directory_Source, "undecodable_0byte.png"), []);

                string path_Output = Path.Combine(directory, "output", "results.bbrf");
                Directory.CreateDirectory(Path.GetDirectoryName(path_Output)!);
                File.WriteAllText(path_Output, "0000_1900\t0\t1\t2\t3\t4\t0.5");

                Classes.YOLOONNXPredictionOptions yOLOONNXPredictionOptions = new()
                {
                    ModelPath = path_Model,
                    OutputPath = path_Output,
                    SourceDirectory = directory_Source
                };

                Classes.YOLOONNXPredictionResult? yOLOONNXPredictionResult = Modify.Predict(yOLOONNXPredictionOptions);

                Assert.NotNull(yOLOONNXPredictionResult);
                Assert.True(yOLOONNXPredictionResult!.Succeeded, string.Join(" | ", yOLOONNXPredictionResult.Messages ?? []));

                string[] values_Expected = ["undecodable_4byte", "0207_2021", "undecodable_0byte"];
                Assert.Equal(values_Expected, yOLOONNXPredictionResult.Values);

                Assert.Contains(yOLOONNXPredictionResult.Messages!, x => x.Contains("Image could not be decoded") && x.Contains("undecodable_4byte.jpg"));
                Assert.Contains(yOLOONNXPredictionResult.Messages!, x => x.Contains("Image could not be decoded") && x.Contains("undecodable_0byte.png"));

                //A run that got this far must have written its result file, not deleted it
                Assert.True(File.Exists(path_Output));
            }
            finally
            {
                if (Directory.Exists(directory))
                {
                    Directory.Delete(directory, true);
                }
            }
        }

        /// <summary>
        /// Runs <see cref="Modify.Predict(Classes.YOLOONNXPredictionOptions, System.Threading.CancellationToken)"/> with one of the shared ONNX fixtures on one black image, over a result file left by an earlier run.
        /// </summary>
        /// <param name="fileName">The fixture's file name in the shared test data folder.</param>
        /// <param name="exists">Whether a result file exists once the run is over.</param>
        /// <returns>The result of the run.</returns>
        private static Classes.YOLOONNXPredictionResult? Predict_Fixture(string fileName, out bool exists)
        {
            string? path_Model = Core.xUnit.Query.FilePath(Assembly.GetExecutingAssembly(), fileName);
            Assert.True(File.Exists(path_Model), fileName);

            string directory = Path.Combine(Path.GetTempPath(), "DiGi_YOLO_ONNX_Test_" + Path.GetRandomFileName());

            try
            {
                string directory_Source = Path.Combine(directory, "input");
                Directory.CreateDirectory(directory_Source);

                //One real black image, so the run gets past loading the model and, for a graph that is accepted, through inference: the fixtures answer
                //the mean of the canvas everywhere, which for a black square is a score of zero on every anchor and therefore no detection at all
                using (Mat mat = new(32, 32, DepthType.Cv8U, 3))
                {
                    mat.SetTo(new MCvScalar(0, 0, 0));
                    CvInvoke.Imwrite(Path.Combine(directory_Source, "0207_2021.jpeg"), mat);
                }

                string path_Output = Path.Combine(directory, "output", "results.bbrf");
                Directory.CreateDirectory(Path.GetDirectoryName(path_Output)!);
                File.WriteAllText(path_Output, "0000_1900\t0\t1\t2\t3\t4\t0.5");

                Classes.YOLOONNXPredictionOptions yOLOONNXPredictionOptions = new()
                {
                    ModelPath = path_Model,
                    OutputPath = path_Output,
                    SourceDirectory = directory_Source
                };

                Classes.YOLOONNXPredictionResult? result = Modify.Predict(yOLOONNXPredictionOptions);

                exists = File.Exists(path_Output);

                return result;
            }
            finally
            {
                if (Directory.Exists(directory))
                {
                    Directory.Delete(directory, true);
                }
            }
        }
    }
}
