using DiGi.Core.IO.Table.Classes;
using DiGi.GIS.IO.Interfaces;
using System.Collections.Generic;

namespace DiGi.GIS.YOLO.UI.xUnit
{
    public partial class Facts
    {
        /// <summary>
        /// A year built predictor that returns a fixed answer, so the orchestrator can be driven without ML.NET, without a model file and without a database.
        /// </summary>
        private sealed class YearBuiltPredictorStub : IYearBuiltPredictor
        {
            private readonly short year;
            private readonly bool runnable;
            private readonly DiGi.Core.Classes.Range<int>? years;
            private readonly List<double>? radiuses;

            /// <summary>
            /// Initializes a new instance of the <see cref="YearBuiltPredictorStub"/> class.
            /// </summary>
            /// <param name="year">The construction year to answer with for every row handed in.</param>
            /// <param name="runnable">Whether the readiness probe reports this predictor can score.</param>
            /// <param name="years">The year range the predictor's readiness reports it was trained on, or null when it states no contract.</param>
            /// <param name="radiuses">The radiuses the predictor's readiness reports it was trained on, or null when it states no contract.</param>
            public YearBuiltPredictorStub(short year, bool runnable = true, DiGi.Core.Classes.Range<int>? years = null, List<double>? radiuses = null)
            {
                this.year = year;
                this.runnable = runnable;
                this.years = years;
                this.radiuses = radiuses;
            }

            /// <summary>
            /// Reports whether this predictor can score at all, as configured at construction.
            /// </summary>
            /// <returns>Runnable as configured, carrying a diagnostic when it is not.</returns>
            public IO.Classes.YearBuiltPredictorReadiness YearBuiltPredictorReadiness()
            {
                if (this.runnable)
                {
                    return new IO.Classes.YearBuiltPredictorReadiness(true, years: this.years, radiuses: this.radiuses);
                }

                return new IO.Classes.YearBuiltPredictorReadiness(false, ["the year built model is missing (stub)"], years: this.years, radiuses: this.radiuses);
            }

            /// <summary>
            /// Gets how many times the orchestrator asked this predictor to score a page of features.
            /// </summary>
            public int CallCount { get; private set; }

            /// <summary>
            /// Answers with the fixed year for every row of the table handed in, keyed by the same reference the row carried.
            /// </summary>
            /// <param name="table">The building features to score.</param>
            /// <returns>The predicted construction years, or null when the table carries no reference column.</returns>
            public Table? Predict(Table? table)
            {
                CallCount++;

                if (table is null)
                {
                    return null;
                }

                int index_Reference = table.GetColumnIndex(IO.Constants.Column.Reference.Name);
                if (index_Reference < 0)
                {
                    return null;
                }

                Table result = new();
                result.AddColumn(IO.Constants.Column.Reference.Name, typeof(string));
                result.AddColumn(IO.Constants.Column.PredictedYearBuilt.Name, typeof(ushort));

                for (int i = 0; i < table.RowCount; i++)
                {
                    if (table.GetValue<string>(i, index_Reference) is not string reference)
                    {
                        continue;
                    }

                    result.AddRow([reference, (ushort)year]);
                }

                return result;
            }
        }
    }
}
