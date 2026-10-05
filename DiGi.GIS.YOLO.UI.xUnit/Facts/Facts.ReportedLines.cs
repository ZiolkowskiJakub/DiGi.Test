using System;
using System.Collections.Generic;

namespace DiGi.GIS.YOLO.UI.xUnit
{
    public partial class Facts
    {
        /// <summary>
        /// Collects the lines a runner reports through its <c>information</c> parameter, synchronously and in order.
        /// <para><see cref="Progress{T}"/> posts each line through the synchronization context xUnit installs, so the lines can arrive after the awaited call returns and an assertion would race them; this collector records each line on the reporting thread.</para>
        /// </summary>
        private sealed class ReportedLines : IProgress<string>
        {
            private readonly List<string> values = [];

            /// <summary>
            /// Gets a copy of the lines reported so far, in the order they were reported.
            /// </summary>
            public List<string> Values
            {
                get
                {
                    lock (values)
                    {
                        return [.. values];
                    }
                }
            }

            /// <summary>
            /// Records one reported line.
            /// </summary>
            /// <param name="value">The reported line.</param>
            public void Report(string value)
            {
                lock (values)
                {
                    values.Add(value);
                }
            }
        }
    }
}
