using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.Json.Nodes;

namespace DiGi.Laya.xUnit
{
    public partial class Facts
    {
        /// <summary>
        /// Verifies that <see cref="Convert.ToDiGi_LayaResponse(JsonObject?)"/> reads the responses of Laya_Response.jsonl - captured from predict.py answering Laya_Request.jsonl with laya 0.4.7 and laya-multilingual on CUDA (2026-10-15), so a real result plus the request id, including <c>usage</c> keys DiGi.Laya does not read - into answers of their own type in document order: the choice label and its probabilities in criteria order, the expected score level and its distribution by level index, the noul probability, both confidences of each, the input token count and the truncation flag; all under <c>pl-PL</c>, where the decimal separator is a comma.
        /// <para>Also verifies that an answer of an unknown type or that is not an object is skipped, that a score distribution with a missing level is dropped rather than shifted, that a response without usage has no token count, and that <c>null</c> gives <c>null</c>.</para>
        /// </summary>
        [Fact]
        public void Convert_ToDiGi_LayaResponse()
        {
            CultureInfo cultureInfo = CultureInfo.CurrentCulture;

            try
            {
                CultureInfo.CurrentCulture = new CultureInfo("pl-PL");

                string? path = Core.xUnit.Query.FilePath(Assembly.GetExecutingAssembly(), "Laya_Response.jsonl");
                Assert.True(File.Exists(path));

                List<Classes.LayaResponse?> layaResponses = [.. File.ReadAllLines(path!).Where(x => !string.IsNullOrWhiteSpace(x)).Select(x => (JsonNode.Parse(x) as JsonObject).ToDiGi_LayaResponse())];
                Assert.Equal(2, layaResponses.Count);

                Classes.LayaResponse layaResponse = layaResponses[0]!;
                Assert.Equal("item-1", layaResponse.RequestId);
                Assert.Equal(142, layaResponse.InputTokenCount);
                Assert.False(layaResponse.Truncated);
                Assert.Equal(["department", "urgency", "refund"], layaResponse.Answers!.Select(x => x.Name));

                Classes.LayaChoiceAnswer layaChoiceAnswer = Assert.IsType<Classes.LayaChoiceAnswer>(layaResponse.Answers![0]);
                Assert.Equal("billing", layaChoiceAnswer.Label);
                Assert.Equal(0.9987, layaChoiceAnswer.Confidence);
                Assert.Equal(0.9999, layaChoiceAnswer.AnswerConfidence);
                Assert.Equal(["billing", "technical"], layaChoiceAnswer.Probabilities!.Select(x => x.Label));
                Assert.Equal([0.9999, 0.0001], layaChoiceAnswer.Probabilities!.Select(x => x.Value));

                Classes.LayaScoreAnswer layaScoreAnswer = Assert.IsType<Classes.LayaScoreAnswer>(layaResponse.Answers![1]);
                Assert.Equal(1.5378, layaScoreAnswer.Level);
                Assert.Equal(0.2558, layaScoreAnswer.Confidence);
                Assert.Equal(0.5801, layaScoreAnswer.AnswerConfidence);
                Assert.Equal([0.0424, 0.3775, 0.5801], layaScoreAnswer.Distribution!);

                Classes.LayaNoulAnswer layaNoulAnswer = Assert.IsType<Classes.LayaNoulAnswer>(layaResponse.Answers![2]);
                Assert.Equal(0.0009, layaNoulAnswer.Probability);
                Assert.Equal(0.9991, layaNoulAnswer.Confidence);
                Assert.Equal(0.9991, layaNoulAnswer.AnswerConfidence);

                Classes.LayaResponse layaResponse_Second = layaResponses[1]!;
                Assert.Equal("item-2", layaResponse_Second.RequestId);
                Assert.Equal(50, layaResponse_Second.InputTokenCount);
                Assert.False(layaResponse_Second.Truncated);
                Classes.LayaNoulAnswer layaNoulAnswer_Second = Assert.IsType<Classes.LayaNoulAnswer>(Assert.Single(layaResponse_Second.Answers!));
                Assert.Equal(0.9896, layaNoulAnswer_Second.Probability);
                Assert.Equal(0.9896, layaNoulAnswer_Second.AnswerConfidence);

                //The read value survives the DiGi serializer
                Core.xUnit.Query.SerializationCheck(layaResponse);

                //Unknown and malformed answers are skipped; a level gap drops the distribution; no usage, no token count
                JsonObject? jsonObject_Odd = JsonNode.Parse(
                    "{\"id\":\"item-3\",\"answers\":{" +
                    "\"future\":{\"type\":\"rank\",\"rank\":[1,2]}," +
                    "\"broken\":42," +
                    "\"urgency\":{\"type\":\"score\",\"score\":1.0,\"probabilities\":{\"0\":0.5,\"2\":0.5}}," +
                    "\"department\":{\"type\":\"choice\",\"choice\":7,\"probabilities\":{\"7\":1.0}}}}") as JsonObject;

                Classes.LayaResponse? layaResponse_Odd = jsonObject_Odd.ToDiGi_LayaResponse();
                Assert.NotNull(layaResponse_Odd);
                Assert.Null(layaResponse_Odd!.InputTokenCount);
                Assert.Null(layaResponse_Odd.Truncated);
                Assert.Equal(["urgency", "department"], layaResponse_Odd.Answers!.Select(x => x.Name));

                Classes.LayaScoreAnswer layaScoreAnswer_Gap = Assert.IsType<Classes.LayaScoreAnswer>(layaResponse_Odd.Answers![0]);
                Assert.Equal(1.0, layaScoreAnswer_Gap.Level);
                Assert.Null(layaScoreAnswer_Gap.Distribution);
                Assert.Null(layaScoreAnswer_Gap.Confidence);

                //A numeric label comes back as its JSON text
                Assert.Equal("7", Assert.IsType<Classes.LayaChoiceAnswer>(layaResponse_Odd.Answers![1]).Label);

                Assert.Null(((JsonObject?)null).ToDiGi_LayaResponse());
            }
            finally
            {
                CultureInfo.CurrentCulture = cultureInfo;
            }
        }
    }
}
