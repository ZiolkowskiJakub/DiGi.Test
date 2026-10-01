using System.Collections.Generic;

namespace DiGi.YOLO.xUnit
{
    public partial class Facts
    {
        /// <summary>
        /// Verifies that <see cref="Query.CheckJsonLine(System.Collections.Generic.List{string})"/> finds the check.py payload between its marker lines, with and without the ultralytics settings notice printed before it, and gives <c>null</c> when the markers are absent.
        /// </summary>
        [Fact]
        public void CheckJsonLine()
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

            Assert.Equal(payload, Query.CheckJsonLine(standardOutput_Notice));

            List<string> standardOutput_Quiet =
            [
                "Ultralytics 8.4.165 Python-3.13.14 torch-2.7.0+cu128 CUDA:0 (NVIDIA GeForce RTX 5090, 32768MiB)",
                Constants.Marker.CheckJsonBegin,
                payload,
                Constants.Marker.CheckJsonEnd
            ];

            Assert.Equal(payload, Query.CheckJsonLine(standardOutput_Quiet));

            //The old contract: the payload alone, no markers - the caller must not guess it
            List<string> standardOutput_NoMarkers =
            [
                "Ultralytics 8.4.165 Python-3.13.14 torch-2.7.0+cu128 CUDA:0",
                payload
            ];

            Assert.Null(Query.CheckJsonLine(standardOutput_NoMarkers));

            //The same contract serves checkpoint.py through its own marker pair
            string payload_Checkpoint = "{\"readable\": true, \"epoch\": 0, \"finished\": false}";

            List<string> standardOutput_Checkpoint =
            [
                "Ultralytics 8.4.165 Python-3.13.14 torch-2.7.0+cu128 CUDA:0",
                Constants.Marker.CheckpointJsonBegin,
                payload_Checkpoint,
                Constants.Marker.CheckpointJsonEnd
            ];

            Assert.Equal(payload_Checkpoint, Query.CheckJsonLine(standardOutput_Checkpoint, Constants.Marker.CheckpointJsonBegin, Constants.Marker.CheckpointJsonEnd));

            //The check.py markers must not find the checkpoint payload
            Assert.Null(Query.CheckJsonLine(standardOutput_Checkpoint));
        }
    }
}
