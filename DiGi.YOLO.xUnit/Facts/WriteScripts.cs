using DiGi.YOLO;
using System.IO;

namespace DiGi.YOLO.xUnit
{
    public partial class Facts
    {
        /// <summary>
        /// Verifies that <see cref="Modify.WriteScripts(string?)"/> writes all Python scripts and configuration files into the specified directory.
        /// <para>Run from a test assembly there is no YOLO folder beside DiGi.YOLO.dll, so this exercises the embedded resource path - the one that has to work in a deployed host.</para>
        /// </summary>
        [Fact]
        public void WriteScripts()
        {
            string tempDirectory = Path.Combine(Path.GetTempPath(), "DiGi_YOLO_Test_" + Path.GetRandomFileName());
            try
            {
                bool success = Modify.WriteScripts(tempDirectory);
                Assert.True(success);

                string exportPath = Path.Combine(tempDirectory, "export.py");
                string trainPath = Path.Combine(tempDirectory, "train.py");
                string validatePath = Path.Combine(tempDirectory, "val.py");
                string predictPath = Path.Combine(tempDirectory, "predict.py");
                string utilsPath = Path.Combine(tempDirectory, "utils.py");
                string checkPath = Path.Combine(tempDirectory, "check.py");
                string checkpointPath = Path.Combine(tempDirectory, "checkpoint.py");
                string requirementsPath = Path.Combine(tempDirectory, "requirements.txt");
                string confPath = Path.Combine(tempDirectory, "conf.yaml");

                Assert.True(File.Exists(exportPath));
                Assert.True(File.Exists(trainPath));
                Assert.True(File.Exists(validatePath));
                Assert.True(File.Exists(predictPath));
                Assert.True(File.Exists(utilsPath));
                Assert.True(File.Exists(checkPath));
                Assert.True(File.Exists(checkpointPath));
                Assert.True(File.Exists(requirementsPath));
                Assert.True(File.Exists(confPath));

                string predictContent = File.ReadAllText(predictPath);
                Assert.Contains("argparse", predictContent);
                Assert.Contains("--model", predictContent);
                Assert.Contains("--source", predictContent);
                Assert.Contains("--conf", predictContent);
                Assert.Contains("--output", predictContent);

                //export.py is the only reason the frozen checkpoint can become the ONNX graph the in-process detector scores with, and it has to be laid down by the same call that lays the rest down
                string exportContent = File.ReadAllText(exportPath);
                Assert.Contains("format=\"onnx\"", exportContent);
                Assert.Contains("--opset", exportContent);
                Assert.Contains("SHA256", exportContent);

                //train.py and val.py are argument driven, and print the lines Modify.Train and Modify.Validate parse
                string trainContent = File.ReadAllText(trainPath);
                Assert.Contains("argparse", trainContent);
                Assert.Contains("--patience", trainContent);
                Assert.Contains("\"--epochs\", type=int, default=150", trainContent);
                Assert.Contains("--resume", trainContent);
                Assert.Contains("resume=True", trainContent);
                Assert.Contains(Constants.OutputPrefix.Weights, trainContent);
                Assert.Contains(Constants.OutputPrefix.SHA256, trainContent);
                Assert.Contains(Constants.OutputPrefix.AMP, trainContent);
                Assert.Contains(Constants.OutputPrefix.ResumeEpoch, trainContent);
                Assert.Contains(Constants.OutputPrefix.ResumeEpochs, trainContent);

                //checkpoint.py reads a .pt without ultralytics and states its own torch.load intent, exactly as check.py does
                string checkpointContent = File.ReadAllText(checkpointPath);
                Assert.Contains("weights_only=False", checkpointContent);
                Assert.Contains(Constants.Marker.CheckpointJsonBegin, checkpointContent);
                Assert.Contains(Constants.Marker.CheckpointJsonEnd, checkpointContent);
                Assert.Contains("\"finished\"", checkpointContent);

                string validateContent = File.ReadAllText(validatePath);
                Assert.Contains("--split", validateContent);
                Assert.Contains(Constants.OutputPrefix.MAP50, validateContent);
                Assert.Contains(Constants.OutputPrefix.MAP50_95, validateContent);

                string utilsContent = File.ReadAllText(utilsPath);
                Assert.Contains("isdigit()", utilsContent);
                Assert.Contains("model.pt", utilsContent);

                //check.py must state its own intent rather than inheriting torch.load's weights_only default from the ultralytics
                //monkeypatch: torch 2.6 flipped the default to weights_only=True, which refuses the ultralytics classes a checkpoint
                //carries. A model whose header cannot be parsed is a warning, not a refusal, so it must surface in the warnings array
                //and leave runnable true - see ZiolkowskiJakub/DiGi.YOLO#15
                string checkContent = File.ReadAllText(checkPath);
                Assert.Contains("weights_only=False", checkContent);
                Assert.Contains("warnings.append", checkContent);
                Assert.Contains("\"warnings\": warnings", checkContent);

                //The detector is frozen, so ultralytics is pinned exactly: 8.4.165 was adopted in ZiolkowskiJakub/DiGi.YOLO#17
                //only after reproducing the 8.3.130 checkpoint's detections on 1000 held images, and a different ultralytics
                //can be a different detector
                string requirementsContent = File.ReadAllText(requirementsPath);
                Assert.Contains("ultralytics==8.4.165", requirementsContent);
                Assert.Contains("torch", requirementsContent);
                Assert.Contains("onnx", requirementsContent);

                string confContent = File.ReadAllText(confPath);
                Assert.Contains("path: training", confContent);

                //The template is written only into a directory without a conf.yaml; the scripts are rewritten every time
                File.WriteAllText(confPath, "path: dataset");
                File.WriteAllText(trainPath, "stale");
                Assert.True(Modify.WriteScripts(tempDirectory));
                Assert.Equal("path: dataset", File.ReadAllText(confPath));
                Assert.Contains("argparse", File.ReadAllText(trainPath));
            }
            finally
            {
                if (Directory.Exists(tempDirectory))
                {
                    Directory.Delete(tempDirectory, true);
                }
            }
        }
    }
}
