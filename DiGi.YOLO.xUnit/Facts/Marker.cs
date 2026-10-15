using System.Collections.Generic;

namespace DiGi.YOLO.xUnit
{
    public partial class Facts
    {
        /// <summary>
        /// Verifies that the check.py and checkpoint.py marker pairs of <see cref="Constants.Marker"/>, read through <see cref="Python.Query.MarkedJson(IEnumerable{string}?, string, string)"/> as the preflight and the checkpoint probe read them, find the check.py payload between its marker lines, with and without the ultralytics settings notice printed before it, gives <c>null</c> when the markers are absent, never finds one pair's payload through the other pair, reads a repeated pair by its last report and gives <c>null</c> for a run cut off after its begin marker.
        /// </summary>
        [Fact]
        public void Marker()
        {
            string payload = "{\"runnable\": true, \"ultralytics_version\": \"8.3.130\", \"messages\": [], \"warnings\": []}";

            //The lines ultralytics 8.3.130 prints when it finds the 8.4.165 settings schema on stdout, before the payload
            List<string> standardOutput_Notice =
            [
                "Ultralytics 8.3.130 Python-3.13.14 torch-2.7.0+cu128 CUDA:0 (NVIDIA GeForce RTX 5090, 32768MiB)",
                "ERROR Error reading from C:\\Users\\jakub\\AppData\\Roaming\\Ultralytics\\settings.json: \"No Ultralytics setting 'openvino_msg'. ...\"",
                "Creating new Ultralytics Settings v0.0.6 file",
                Constants.Marker.CheckJsonBegin,
                payload,
                Constants.Marker.CheckJsonEnd
            ];

            Assert.Equal(payload, Python.Query.MarkedJson(standardOutput_Notice, Constants.Marker.CheckJsonBegin, Constants.Marker.CheckJsonEnd));

            List<string> standardOutput_Quiet =
            [
                "Ultralytics 8.4.165 Python-3.13.14 torch-2.7.0+cu128 CUDA:0 (NVIDIA GeForce RTX 5090, 32768MiB)",
                Constants.Marker.CheckJsonBegin,
                payload,
                Constants.Marker.CheckJsonEnd
            ];

            Assert.Equal(payload, Python.Query.MarkedJson(standardOutput_Quiet, Constants.Marker.CheckJsonBegin, Constants.Marker.CheckJsonEnd));

            //The old contract: the payload alone, no markers - the caller must not guess it
            List<string> standardOutput_NoMarkers =
            [
                "Ultralytics 8.4.165 Python-3.13.14 torch-2.7.0+cu128 CUDA:0",
                payload
            ];

            Assert.Null(Python.Query.MarkedJson(standardOutput_NoMarkers, Constants.Marker.CheckJsonBegin, Constants.Marker.CheckJsonEnd));

            //The same contract serves checkpoint.py through its own marker pair
            string payload_Checkpoint = "{\"readable\": true, \"epoch\": 0, \"finished\": false}";

            List<string> standardOutput_Checkpoint =
            [
                "Ultralytics 8.4.165 Python-3.13.14 torch-2.7.0+cu128 CUDA:0",
                Constants.Marker.CheckpointJsonBegin,
                payload_Checkpoint,
                Constants.Marker.CheckpointJsonEnd
            ];

            Assert.Equal(payload_Checkpoint, Python.Query.MarkedJson(standardOutput_Checkpoint, Constants.Marker.CheckpointJsonBegin, Constants.Marker.CheckpointJsonEnd));

            //The check.py markers must not find the checkpoint payload
            Assert.Null(Python.Query.MarkedJson(standardOutput_Checkpoint, Constants.Marker.CheckJsonBegin, Constants.Marker.CheckJsonEnd));

            //A repeated pair is read by its last report - the retired CheckJsonLine took the first
            string payload_Final = "{\"runnable\": false, \"ultralytics_version\": \"8.4.165\", \"messages\": [\"torch is missing\"], \"warnings\": []}";

            List<string> standardOutput_Repeated =
            [
                Constants.Marker.CheckJsonBegin,
                payload,
                Constants.Marker.CheckJsonEnd,
                Constants.Marker.CheckJsonBegin,
                payload_Final,
                Constants.Marker.CheckJsonEnd
            ];

            Assert.Equal(payload_Final, Python.Query.MarkedJson(standardOutput_Repeated, Constants.Marker.CheckJsonBegin, Constants.Marker.CheckJsonEnd));

            //A run cut off after the begin marker carries no payload - the retired CheckJsonLine still returned the line, which then failed to parse
            List<string> standardOutput_CutOff =
            [
                Constants.Marker.CheckJsonBegin,
                payload
            ];

            Assert.Null(Python.Query.MarkedJson(standardOutput_CutOff, Constants.Marker.CheckJsonBegin, Constants.Marker.CheckJsonEnd));
        }
    }
}
