using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace DiGi.Laya.xUnit
{
    public partial class Facts
    {
        /// <summary>
        /// Verifies that <see cref="Convert.ToLaya_JsonObject(Classes.LayaRequest?)"/> writes Laya's request shape for all three question types - <c>questions</c> keyed by name in question order, choice criteria as an ordered label-to-description object with a <c>null</c> description kept, score levels as an array, noul with instructions only - a JSON state as an object and a text state as a string (non-ASCII text surviving the escape), and the token budgets only when set; and that it writes exactly the lines of Laya_Request.jsonl, the input the captured Laya_Response.jsonl answers.
        /// <para>Also verifies that every request or question Laya's <c>_check_question</c> would refuse is refused as <c>null</c>: blank name or instructions, a choice question without criteria or with a repeated or <c>null</c> label, a score question without levels, two questions of one name, no questions, no state, and a state flagged as JSON that is malformed or a JSON scalar.</para>
        /// </summary>
        [Fact]
        public void Convert_ToLaya_JsonObject()
        {
            Classes.LayaChoiceQuestion layaChoiceQuestion = new("department", "Which team should handle this?", [new Classes.LayaCriterion("billing", "invoices, refunds"), new Classes.LayaCriterion("technical", null)]);
            Classes.LayaScoreQuestion layaScoreQuestion = new("urgency", "How urgent is this request?", ["low", "mid", "high"]);
            Classes.LayaNoulQuestion layaNoulQuestion = new("refund", "The customer asks for a refund.");

            Classes.LayaRequest layaRequest = new("item-1", "{\"subject\":\"Double charge\",\"body\":\"We were billed twice for March.\"}", true, [layaChoiceQuestion, layaScoreQuestion, layaNoulQuestion], 1024, 256);

            JsonObject? jsonObject = layaRequest.ToLaya_JsonObject();
            Assert.NotNull(jsonObject);
            Assert.Equal(
                "{\"id\":\"item-1\"," +
                "\"state\":{\"subject\":\"Double charge\",\"body\":\"We were billed twice for March.\"}," +
                "\"questions\":{" +
                "\"department\":{\"type\":\"choice\",\"instructions\":\"Which team should handle this?\",\"criteria\":{\"billing\":\"invoices, refunds\",\"technical\":null}}," +
                "\"urgency\":{\"type\":\"score\",\"instructions\":\"How urgent is this request?\",\"criteria\":[\"low\",\"mid\",\"high\"]}," +
                "\"refund\":{\"type\":\"noul\",\"instructions\":\"The customer asks for a refund.\"}}," +
                "\"max_len\":1024,\"head_max_len\":256}",
                jsonObject!.ToJsonString());

            //Laya_Request.jsonl is the input Laya_Response.jsonl was captured from; it must be exactly what this converter writes
            string? path = Core.xUnit.Query.FilePath(Assembly.GetExecutingAssembly(), "Laya_Request.jsonl");
            Assert.True(File.Exists(path));

            List<JsonNode?> jsonNodes_Fixture = [.. File.ReadAllLines(path!).Where(x => !string.IsNullOrWhiteSpace(x)).Select(x => JsonNode.Parse(x))];
            Assert.Equal(2, jsonNodes_Fixture.Count);
            Assert.True(JsonNode.DeepEquals(jsonNodes_Fixture[0], jsonObject));
            Assert.True(JsonNode.DeepEquals(jsonNodes_Fixture[1], new Classes.LayaRequest("item-2", "Proszę o zwrot pieniędzy za podwójną opłatę.", false, [layaNoulQuestion], null, null).ToLaya_JsonObject()));

            //A text state, no id and the checkpoint's own budgets: the optional keys are absent, and the text reads back unchanged
            JsonObject? jsonObject_Text = new Classes.LayaRequest(null, "Proszę o zwrot pieniędzy.", false, [layaNoulQuestion], null, null).ToLaya_JsonObject();
            Assert.NotNull(jsonObject_Text);
            Assert.False(jsonObject_Text!.ContainsKey("id"));
            Assert.False(jsonObject_Text.ContainsKey("max_len"));
            Assert.False(jsonObject_Text.ContainsKey("head_max_len"));

            JsonNode? jsonNode_State = JsonNode.Parse(jsonObject_Text.ToJsonString())?["state"];
            Assert.Equal(JsonValueKind.String, jsonNode_State!.GetValueKind());
            Assert.Equal("Proszę o zwrot pieniędzy.", jsonNode_State.GetValue<string>());

            //A JSON array state is accepted as such
            JsonObject? jsonObject_Array = new Classes.LayaRequest("item-2", "[{\"role\":\"user\",\"content\":\"Hi\"}]", true, [layaNoulQuestion], null, null).ToLaya_JsonObject();
            Assert.IsType<JsonArray>(jsonObject_Array?["state"]);

            //Refused questions
            Assert.Null(((Classes.LayaQuestion?)null).ToLaya_JsonObject());
            Assert.Null(new Classes.LayaNoulQuestion(" ", "statement").ToLaya_JsonObject());
            Assert.Null(new Classes.LayaNoulQuestion("refund", " ").ToLaya_JsonObject());
            Assert.Null(new Classes.LayaChoiceQuestion("department", "Which team?", null).ToLaya_JsonObject());
            Assert.Null(new Classes.LayaChoiceQuestion("department", "Which team?", []).ToLaya_JsonObject());
            Assert.Null(new Classes.LayaChoiceQuestion("department", "Which team?", [new Classes.LayaCriterion("billing", "a"), new Classes.LayaCriterion("billing", "b")]).ToLaya_JsonObject());
            Assert.Null(new Classes.LayaChoiceQuestion("department", "Which team?", [new Classes.LayaCriterion(null, "a")]).ToLaya_JsonObject());
            Assert.Null(new Classes.LayaScoreQuestion("urgency", "How urgent?", []).ToLaya_JsonObject());
            Assert.Null(new Classes.LayaScoreQuestion("urgency", "How urgent?", null).ToLaya_JsonObject());

            //Refused requests
            Assert.Null(((Classes.LayaRequest?)null).ToLaya_JsonObject());
            Assert.Null(new Classes.LayaRequest("item-3", "text", false, [layaNoulQuestion, new Classes.LayaNoulQuestion("refund", "Another statement.")], null, null).ToLaya_JsonObject());
            Assert.Null(new Classes.LayaRequest("item-3", "text", false, [layaNoulQuestion, new Classes.LayaNoulQuestion("refund", " ")], null, null).ToLaya_JsonObject());
            Assert.Null(new Classes.LayaRequest("item-3", "text", false, [], null, null).ToLaya_JsonObject());
            Assert.Null(new Classes.LayaRequest("item-3", "text", false, null, null, null).ToLaya_JsonObject());
            Assert.Null(new Classes.LayaRequest("item-3", null, false, [layaNoulQuestion], null, null).ToLaya_JsonObject());
            Assert.Null(new Classes.LayaRequest("item-3", "{not json", true, [layaNoulQuestion], null, null).ToLaya_JsonObject());
            Assert.Null(new Classes.LayaRequest("item-3", "\"a JSON string\"", true, [layaNoulQuestion], null, null).ToLaya_JsonObject());
        }
    }
}
