using System.Collections.Generic;
using System.Globalization;

namespace DiGi.Laya.xUnit
{
    public partial class Facts
    {
        /// <summary>
        /// Builds a small labelled dataset for the facts that train or calibrate the real model: 24 support tickets, a single-choice "department" set (billing, technical) and a multi-label "flags" set (refund), each ticket labelled in both sets.
        /// </summary>
        /// <returns>The items, the label sets and the assignments.</returns>
        private static (List<Classes.LayaItem> LayaItems, List<Classes.LayaLabelSet> LayaLabelSets, List<Classes.LayaLabelAssignment> LayaLabelAssignments) TicketData()
        {
            string[] billing = ["We were billed twice for March.", "My invoice shows the wrong amount.", "Please refund my last payment.", "Why was my card charged again?"];
            string[] technical = ["The app crashes when I log in.", "The website is down for me.", "I get error 500 on upload.", "Sync stopped working yesterday."];

            List<Classes.LayaLabelSet> layaLabelSets =
            [
                new Classes.LayaLabelSet("department", "Which team should handle this ticket?", Enums.LayaLabelSetKind.SingleChoice, [new Classes.LayaLabel("billing", "invoices, payments and refunds", null), new Classes.LayaLabel("technical", "bugs, outages and errors", null)]),
                new Classes.LayaLabelSet("flags", "The customer asks for {label}.", Enums.LayaLabelSetKind.MultiLabel, [new Classes.LayaLabel("refund", "a refund", null)])
            ];

            List<Classes.LayaItem> layaItems = [];
            List<Classes.LayaLabelAssignment> layaLabelAssignments = [];
            for (int i = 0; i < 24; i++)
            {
                bool isBilling = i % 2 == 0;
                string text = string.Concat(isBilling ? billing[i / 2 % billing.Length] : technical[i / 2 % technical.Length], " (ticket ", i.ToString(CultureInfo.InvariantCulture), ")");
                string id = string.Concat("ticket-", i.ToString("00", CultureInfo.InvariantCulture));

                layaItems.Add(new Classes.LayaItem(id, text, false));
                layaLabelAssignments.Add(new Classes.LayaLabelAssignment(id, "department", isBilling ? "billing" : "technical", true, 1.0, null));
                layaLabelAssignments.Add(new Classes.LayaLabelAssignment(id, "flags", "refund", text.Contains("refund"), text.Contains("refund") ? 1.0 : 0.8, null));
            }

            return (layaItems, layaLabelSets, layaLabelAssignments);
        }
    }
}
