using System.Collections.Generic;
using System.Linq;

namespace DiGi.Laya.xUnit
{
    public partial class Facts
    {
        /// <summary>
        /// Verifies that <see cref="Create.LayaRequests(IEnumerable{Classes.LayaItem}?, IEnumerable{Classes.LayaLabelSet}?)"/> makes one request per item, in item order and keeping each item's state, that asks a single-choice set as one choice question named after the set (label id as answer key, label text as description) and a multi-label set of three labels as three noul questions named "&lt;set&gt;:&lt;label&gt;" - each statement the set's template with {label} replaced by the label text, or the label's own template when it has one; and that <c>null</c> items, sets and labels and sets without labels are skipped, while a <c>null</c> argument gives no requests.
        /// </summary>
        [Fact]
        public void Create_LayaRequests()
        {
            Classes.LayaLabelSet layaLabelSet_Category = new("category", "Which course is this recipe?", Enums.LayaLabelSetKind.SingleChoice, [new Classes.LayaLabel("soup", "a soup or broth", null), null!, new Classes.LayaLabel("dessert", "a sweet course", null)]);
            Classes.LayaLabelSet layaLabelSet_Tags = new("tags", "The recipe is {label}.", Enums.LayaLabelSetKind.MultiLabel,
            [
                new Classes.LayaLabel("vegan", "vegan", null),
                new Classes.LayaLabel("spicy", "spicy", "The dish tastes {label}."),
                new Classes.LayaLabel("quick", null, null)
            ]);
            Classes.LayaLabelSet layaLabelSet_Empty = new("empty", "Nothing to ask.", Enums.LayaLabelSetKind.SingleChoice, []);

            List<Classes.LayaItem> layaItems = [new Classes.LayaItem("recipe-1", "Żurek z kiełbasą", false), null!, new Classes.LayaItem("recipe-2", "{\"title\":\"Sernik\"}", true)];

            List<Classes.LayaRequest> layaRequests = Create.LayaRequests(layaItems, [layaLabelSet_Category, null!, layaLabelSet_Tags, layaLabelSet_Empty]);
            Assert.Equal(["recipe-1", "recipe-2"], layaRequests.Select(x => x.Id));
            Assert.Equal("Żurek z kiełbasą", layaRequests[0].State);
            Assert.False(layaRequests[0].StateIsJson);
            Assert.True(layaRequests[1].StateIsJson);
            Assert.Null(layaRequests[0].MaxLength);
            Assert.Null(layaRequests[0].HeadMaxLength);

            List<Classes.LayaQuestion> layaQuestions = layaRequests[1].Questions!;
            Assert.Equal(["category", "tags:vegan", "tags:spicy", "tags:quick"], layaQuestions.Select(x => x.Name));

            Classes.LayaChoiceQuestion layaChoiceQuestion = Assert.IsType<Classes.LayaChoiceQuestion>(layaQuestions[0]);
            Assert.Equal("Which course is this recipe?", layaChoiceQuestion.Instructions);
            Assert.Equal(["soup", "dessert"], layaChoiceQuestion.Criteria!.Select(x => x.Label));
            Assert.Equal(["a soup or broth", "a sweet course"], layaChoiceQuestion.Criteria!.Select(x => x.Description));

            Assert.Equal("The recipe is vegan.", Assert.IsType<Classes.LayaNoulQuestion>(layaQuestions[1]).Instructions);
            Assert.Equal("The dish tastes spicy.", Assert.IsType<Classes.LayaNoulQuestion>(layaQuestions[2]).Instructions);
            Assert.Equal("The recipe is quick.", Assert.IsType<Classes.LayaNoulQuestion>(layaQuestions[3]).Instructions);

            //Every request can be written for Laya
            Assert.All(layaRequests, x => Assert.NotNull(x.ToLaya_JsonObject()));

            Assert.Empty(Create.LayaRequests(null, [layaLabelSet_Category]));
            Assert.Empty(Create.LayaRequests(layaItems, null));
        }
    }
}
