using System.Globalization;

namespace DiGi.Laya.xUnit
{
    public partial class Facts
    {
        /// <summary>
        /// Verifies that <see cref="Query.PredictArguments(string?, string?, string?, string?, Enums.LayaDevice, int, string?)"/> quotes every path, doubles the trailing backslash of a drive root so it cannot escape the closing quote, writes the device lower-case and the batch size with the invariant culture (also under <c>pl-PL</c>), appends --calibration only when a file is given, and refuses a blank path or a batch size that is not positive.
        /// </summary>
        [Fact]
        public void Query_PredictArguments()
        {
            CultureInfo cultureInfo = CultureInfo.CurrentCulture;

            try
            {
                CultureInfo.CurrentCulture = new CultureInfo("pl-PL");

                Assert.Equal(
                    "\"C:\\work dir\\predict.py\" --model \"D:\\Laya\\checkpoints\\multilingual\" --input \"C:\\work dir\\requests.jsonl\" --output \"C:\\work dir\\responses.jsonl\" --device cuda --batch-size 1024",
                    Query.PredictArguments(@"C:\work dir\predict.py", @"D:\Laya\checkpoints\multilingual", @"C:\work dir\requests.jsonl", @"C:\work dir\responses.jsonl", Enums.LayaDevice.Cuda, 1024, null));

                Assert.Equal(
                    "\"C:\\w\\predict.py\" --model \"E:\\\\\" --input \"C:\\w\\requests.jsonl\" --output \"C:\\w\\responses.jsonl\" --device auto --batch-size 16 --calibration \"D:\\Laya\\runs\\m.calibration.json\"",
                    Query.PredictArguments(@"C:\w\predict.py", @"E:\", @"C:\w\requests.jsonl", @"C:\w\responses.jsonl", Enums.LayaDevice.Auto, 16, @"D:\Laya\runs\m.calibration.json"));

                Assert.EndsWith("--device cpu --batch-size 1", Query.PredictArguments("p", "m", "i", "o", Enums.LayaDevice.Cpu, 1, " "));

                Assert.Null(Query.PredictArguments("p", "m", "i", "o", Enums.LayaDevice.Cpu, 0, null));
                Assert.Null(Query.PredictArguments(null, "m", "i", "o", Enums.LayaDevice.Cpu, 1, null));
                Assert.Null(Query.PredictArguments("p", " ", "i", "o", Enums.LayaDevice.Cpu, 1, null));
                Assert.Null(Query.PredictArguments("p", "m", null, "o", Enums.LayaDevice.Cpu, 1, null));
                Assert.Null(Query.PredictArguments("p", "m", "i", "", Enums.LayaDevice.Cpu, 1, null));
            }
            finally
            {
                CultureInfo.CurrentCulture = cultureInfo;
            }
        }
    }
}
