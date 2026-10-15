namespace DiGi.Laya.xUnit
{
    public partial class Facts
    {
        /// <summary>
        /// Verifies that <see cref="Query.TrainArguments(string?, string?, string?, string?, string?, Enums.LayaDevice)"/> builds train.py's command line with every path quoted (a drive root's trailing backslash doubled) and the device lower-case, and refuses a blank path.
        /// </summary>
        [Fact]
        public void Query_TrainArguments()
        {
            Assert.Equal(
                "\"C:\\w\\train.py\" --base \"D:\\Laya\\checkpoints\\multilingual\" --data \"D:\\Laya\\data\\training set.jsonl\" --output \"E:\\\\\" --config \"C:\\w\\train_config.json\" --device cuda",
                Query.TrainArguments(@"C:\w\train.py", @"D:\Laya\checkpoints\multilingual", @"D:\Laya\data\training set.jsonl", @"E:\", @"C:\w\train_config.json", Enums.LayaDevice.Cuda));

            Assert.Null(Query.TrainArguments(@"C:\w\train.py", " ", "d", "o", "c", Enums.LayaDevice.Auto));
            Assert.Null(Query.TrainArguments(@"C:\w\train.py", "b", "d", "o", null, Enums.LayaDevice.Auto));
        }
    }
}
