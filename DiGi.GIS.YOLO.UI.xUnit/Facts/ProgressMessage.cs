namespace DiGi.GIS.YOLO.UI.xUnit
{
    public partial class Facts
    {
        /// <summary>
        /// Verifies that a progress line the headless runner writes is read back as the count it reports, and that nothing else the runner writes is mistaken for one.
        /// <para>The runner's standard output is the only channel a process driving it has for learning what a long run is doing - the tray application's background task pumps these lines and reports them into its progress row. Nothing links the two sides at compile time except <see cref="Create.ProgressMessage(long)"/> and <see cref="Query.ProgressCount(string)"/> being the only places the format is written and read, so this fact is what keeps them together.</para>
        /// <para>The banners, notes and tallies below are the runner's real output. Each has to answer null rather than a count, because a caller taking any of them as a count would report a run's progress as the number that happened to appear in a headline.</para>
        /// </summary>
        [Fact]
        public void ProgressMessage()
        {
            long[] counts = [0, 1, 7, 44492, 372347, long.MaxValue];

            foreach (long count in counts)
            {
                string message = Create.ProgressMessage(count);

                Assert.StartsWith(Constants.MessagePrefix.Progress, message);
                Assert.Equal(count, Query.ProgressCount(message));

                // The runner writes whole lines and a reader hands over whatever the stream gave it, which on a
                // Windows pipe still carries its carriage return.
                Assert.Equal(count, Query.ProgressCount($"  {message}\r"));
            }

            // A machine reads these, so the count has to stay parseable on a machine whose own settings would
            // group it. A grouped count would come back as the digits before the first separator.
            Assert.DoesNotContain(",", Create.ProgressMessage(1234567L));
            Assert.Equal(1234567L, Query.ProgressCount(Create.ProgressMessage(1234567L)));

            string[] lines_NotProgress =
            [
                null!,
                string.Empty,
                "   ",
                "=================================================",
                " DiGi.GIS.YOLO.UI Headless Prediction Runner",
                "[INFO] Starting Year Built prediction pipeline for county IDs: 73485, 73482",
                "[NOTE] County 2212 is not a county row. A county is named by its identifier, never by its four character code.",
                "[ERROR] Pipeline completed with 2 failed step(s):",
                " Buildings: 44492",
                " Detections: 372347",
                "[PROGRESSION] Processed 12 items..."
            ];

            foreach (string line in lines_NotProgress)
            {
                Assert.Null(Query.ProgressCount(line));
            }
        }
    }
}
