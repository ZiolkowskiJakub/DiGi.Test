using System.Collections.Generic;
using System.Linq;

namespace DiGi.Core.xUnit
{
    public partial class Facts
    {
        /// <summary>
        /// Tests that <see cref="Core.Query.Split{X}(System.Collections.Generic.IEnumerable{X}, int)"/> still returns
        /// a single chunk containing all elements when the sequence is no larger than maxCount, after removing the
        /// double enumeration (Count() then a separate materialization of the same sequence).
        /// </summary>
        [Fact]
        public void Split_SequenceNotLargerThanMaxCount_ReturnsSingleChunk()
        {
            List<int> values = [1, 2, 3];

            List<List<int>>? result = values.Split(5);

            Assert.NotNull(result);
            Assert.Single(result);
            Assert.Equal(values, result[0]);
        }

        /// <summary>
        /// Tests that a sequence larger than maxCount is split into correctly-sized chunks, including a
        /// smaller trailing chunk for the remainder.
        /// </summary>
        [Fact]
        public void Split_SequenceLargerThanMaxCount_ReturnsMultipleChunks()
        {
            List<int> values = Enumerable.Range(1, 7).ToList();

            List<List<int>>? result = values.Split(3);

            Assert.NotNull(result);
            Assert.Equal(3, result.Count);
            Assert.Equal([1, 2, 3], result[0]);
            Assert.Equal([4, 5, 6], result[1]);
            Assert.Equal([7], result[2]);
        }

        /// <summary>
        /// Tests that a sequence that is only enumerable once (not a List/array) is still split correctly -
        /// guarding against any reintroduction of double enumeration over a single-pass IEnumerable.
        /// </summary>
        [Fact]
        public void Split_SingleUseEnumerable_IsSplitCorrectly()
        {
            IEnumerable<int> SingleUse()
            {
                for (int i = 1; i <= 4; i++)
                {
                    yield return i;
                }
            }

            List<List<int>>? result = Split(SingleUse(), 2);

            Assert.NotNull(result);
            Assert.Equal(2, result.Count);
            Assert.Equal([1, 2], result[0]);
            Assert.Equal([3, 4], result[1]);
        }

        private static List<List<int>>? Split(IEnumerable<int> values, int maxCount)
        {
            return Core.Query.Split(values, maxCount);
        }

        /// <summary>
        /// Tests that null input and an invalid maxCount both return null.
        /// </summary>
        [Fact]
        public void Split_NullOrInvalidMaxCount_ReturnsNull()
        {
            List<int>? nullValues = null;
            Assert.Null(Core.Query.Split(nullValues, 3));

            List<int> values = [1, 2, 3];
            Assert.Null(Core.Query.Split(values, 0));
        }
    }
}
