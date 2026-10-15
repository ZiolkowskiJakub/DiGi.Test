using System.Globalization;

namespace DiGi.Laya.xUnit
{
    public partial class Facts
    {
        /// <summary>
        /// Verifies that <see cref="Convert.ToLaya_TrainingLine(Classes.LayaTrainingSample?)"/> writes the row <c>laya.train.finetune</c> reads - the request shape plus <c>expected</c> - for a choice label, a whole and a fractional score level, and a noul truth, and a <c>gold</c> soft target <c>{"false": 1 - p, "true": p}</c> for a noul probability, which takes precedence over the truth; that the line holds no line break even when the state does; and that the numbers are written the same under <c>pl-PL</c>, where the decimal separator is a comma.
        /// <para>Also verifies that a target which does not fit its question is refused as <c>null</c>: a label outside the criteria, a level outside the levels or not a number, a probability outside [0, 1] or on a question that is not a noul question, a noul question with neither target, and a sample without a question or a state.</para>
        /// </summary>
        [Fact]
        public void Convert_ToLaya_TrainingLine()
        {
            CultureInfo cultureInfo = CultureInfo.CurrentCulture;

            try
            {
                CultureInfo.CurrentCulture = new CultureInfo("pl-PL");

                Classes.LayaChoiceQuestion layaChoiceQuestion = new("department", "Which team should handle this?", [new Classes.LayaCriterion("billing", "invoices, refunds"), new Classes.LayaCriterion("technical", "bugs, outages")]);
                Classes.LayaScoreQuestion layaScoreQuestion = new("urgency", "How urgent is this request?", ["low", "mid", "high"]);
                Classes.LayaNoulQuestion layaNoulQuestion = new("refund", "The customer asks for a refund.");

                static Classes.LayaTrainingSample Sample(Classes.LayaQuestion? layaQuestion, string? label, double? level, bool? truth, double? probability)
                {
                    return new Classes.LayaTrainingSample("s1", "Please return my money.", false, layaQuestion, label, level, truth, probability, "customer-42");
                }

                Assert.Equal(
                    "{\"id\":\"s1\",\"state\":\"Please return my money.\"," +
                    "\"questions\":{\"department\":{\"type\":\"choice\",\"instructions\":\"Which team should handle this?\",\"criteria\":{\"billing\":\"invoices, refunds\",\"technical\":\"bugs, outages\"}}}," +
                    "\"expected\":{\"department\":\"billing\"}}",
                    Sample(layaChoiceQuestion, "billing", null, null, null).ToLaya_TrainingLine());

                string? line_Score = Sample(layaScoreQuestion, null, 1.5, null, null).ToLaya_TrainingLine();
                Assert.EndsWith(",\"expected\":{\"urgency\":1.5}}", line_Score);

                string? line_ScoreWhole = Sample(layaScoreQuestion, null, 2.0, null, null).ToLaya_TrainingLine();
                Assert.EndsWith(",\"expected\":{\"urgency\":2}}", line_ScoreWhole);

                Assert.Equal(
                    "{\"id\":\"s1\",\"state\":\"Please return my money.\"," +
                    "\"questions\":{\"refund\":{\"type\":\"noul\",\"instructions\":\"The customer asks for a refund.\"}}," +
                    "\"expected\":{\"refund\":false}}",
                    Sample(layaNoulQuestion, null, null, false, null).ToLaya_TrainingLine());

                //A soft target wins over the truth, and is written as gold probabilities
                string? line_Gold = Sample(layaNoulQuestion, null, null, true, 0.75).ToLaya_TrainingLine();
                Assert.EndsWith(",\"gold\":{\"refund\":{\"probabilities\":{\"false\":0.25,\"true\":0.75}}}}", line_Gold);
                Assert.DoesNotContain("\"expected\"", line_Gold);

                //A multi-line JSON state stays one JSONL line
                string? line_Json = new Classes.LayaTrainingSample(null, "{\n  \"body\": \"line one\\nline two\"\n}", true, layaNoulQuestion, null, null, true, null, null).ToLaya_TrainingLine();
                Assert.NotNull(line_Json);
                Assert.DoesNotContain("\n", line_Json);
                Assert.StartsWith("{\"state\":{\"body\":\"line one\\nline two\"},", line_Json);

                //Refused targets
                Assert.Null(Sample(layaChoiceQuestion, "sales", null, null, null).ToLaya_TrainingLine());
                Assert.Null(Sample(layaChoiceQuestion, null, null, null, null).ToLaya_TrainingLine());
                Assert.Null(Sample(layaChoiceQuestion, "billing", null, null, 0.75).ToLaya_TrainingLine());
                Assert.Null(Sample(layaScoreQuestion, null, 2.0001, null, null).ToLaya_TrainingLine());
                Assert.Null(Sample(layaScoreQuestion, null, -0.0001, null, null).ToLaya_TrainingLine());
                Assert.Null(Sample(layaScoreQuestion, null, double.NaN, null, null).ToLaya_TrainingLine());
                Assert.Null(Sample(layaScoreQuestion, null, null, null, null).ToLaya_TrainingLine());
                Assert.Null(Sample(layaNoulQuestion, null, null, null, null).ToLaya_TrainingLine());
                Assert.Null(Sample(layaNoulQuestion, null, null, null, 1.0001).ToLaya_TrainingLine());
                Assert.Null(Sample(layaNoulQuestion, null, null, null, -0.0001).ToLaya_TrainingLine());
                Assert.Null(Sample(null, "billing", null, null, null).ToLaya_TrainingLine());
                Assert.Null(new Classes.LayaTrainingSample("s1", " ", false, layaNoulQuestion, null, null, true, null, null).ToLaya_TrainingLine());
                Assert.Null(((Classes.LayaTrainingSample?)null).ToLaya_TrainingLine());

                //The boundaries themselves are accepted
                Assert.NotNull(Sample(layaScoreQuestion, null, 0.0, null, null).ToLaya_TrainingLine());
                Assert.NotNull(Sample(layaNoulQuestion, null, null, null, 0.0).ToLaya_TrainingLine());
                Assert.NotNull(Sample(layaNoulQuestion, null, null, null, 1.0).ToLaya_TrainingLine());
            }
            finally
            {
                CultureInfo.CurrentCulture = cultureInfo;
            }
        }
    }
}
