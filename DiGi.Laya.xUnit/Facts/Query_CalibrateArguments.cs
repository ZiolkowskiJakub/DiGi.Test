namespace DiGi.Laya.xUnit
{
    public partial class Facts
    {
        /// <summary>
        /// Verifies that <see cref="Query.CalibrateArguments(string?, string?, string?, string?, Enums.LayaDevice, int)"/> builds calibrate.py's command line with every path quoted, the device lower-case and the seed in the invariant culture, and refuses a blank path.
        /// </summary>
        [Fact]
        public void Query_CalibrateArguments()
        {
            Assert.Equal(
                "\"C:\\w\\calibrate.py\" --model \"D:\\Laya\\checkpoints\\multilingual\" --data \"D:\\Laya\\data\\calibration.jsonl\" --output \"D:\\Laya\\runs\\m.calibration.json\" --device cpu --seed -3",
                Query.CalibrateArguments(@"C:\w\calibrate.py", @"D:\Laya\checkpoints\multilingual", @"D:\Laya\data\calibration.jsonl", @"D:\Laya\runs\m.calibration.json", Enums.LayaDevice.Cpu, -3));

            Assert.Null(Query.CalibrateArguments(@"C:\w\calibrate.py", "m", "d", "", Enums.LayaDevice.Auto, 0));
        }
    }
}
