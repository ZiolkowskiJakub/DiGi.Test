using System.Collections.Generic;
using System.Linq;

namespace DiGi.GIS.YOLO.UI.xUnit
{
    public partial class Facts
    {
        /// <summary>
        /// Verifies that <see cref="Query.PredictionChunks(IEnumerable{string?}?, int)"/> splits references into complete, disjoint, ordinally ordered chunks of at most the chunk size (DiGi-Organization/DiGi.GIS.YOLO.UI#30).
        /// <para>The sizes are checked on both sides of a boundary - a county exactly the chunk size makes one chunk, one building more makes a second chunk of one - and duplicate, null and blank references are dropped. Ordinal order puts upper case before lower case, which a culture-aware sort would not, and the same buildings must always make the same chunks or a resumed run cannot recognise its markers.</para>
        /// </summary>
        [Fact]
        public void PredictionChunks()
        {
            List<string?> references = ["e", "b", "a", "B", "a", null, " ", "d", "c"];

            List<List<string>> chunks = Query.PredictionChunks(references, 2);
            Assert.Equal(3, chunks.Count);
            Assert.Equal(["B", "a"], chunks[0]);
            Assert.Equal(["b", "c"], chunks[1]);
            Assert.Equal(["d", "e"], chunks[2]);

            //Complete and disjoint
            List<string> references_Chunked = [.. chunks.SelectMany(x => x)];
            Assert.Equal(6, references_Chunked.Count);
            Assert.Equal(6, references_Chunked.Distinct().Count());

            List<string> references_Four = ["d", "c", "b", "a"];

            //Exactly the chunk size, one more, and twice the chunk size plus one
            Assert.Single(Query.PredictionChunks(references_Four, 4));

            List<List<string>> chunks_SizePlusOne = Query.PredictionChunks(references_Four, 3);
            Assert.Equal(2, chunks_SizePlusOne.Count);
            Assert.Equal(3, chunks_SizePlusOne[0].Count);
            Assert.Single(chunks_SizePlusOne[1]);
            Assert.Equal("d", chunks_SizePlusOne[1][0]);

            List<string> references_Five = ["a", "b", "c", "d", "e"];
            List<List<string>> chunks_TwiceSizePlusOne = Query.PredictionChunks(references_Five, 2);
            Assert.Equal(3, chunks_TwiceSizePlusOne.Count);
            Assert.All(chunks_TwiceSizePlusOne, x => Assert.True(x.Count <= 2));

            Assert.Equal(5, Query.PredictionChunks(references_Five, 1).Count);

            //Nothing to split, or no valid size
            Assert.Empty(Query.PredictionChunks(null, 2));
            Assert.Empty(Query.PredictionChunks([], 2));
            Assert.Empty(Query.PredictionChunks(references_Five, 0));
        }
    }
}
