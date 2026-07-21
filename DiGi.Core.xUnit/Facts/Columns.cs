using DiGi.Core.IO.Table.Classes;
using System.Collections.Generic;

namespace DiGi.Core.xUnit
{
    public partial class Facts
    {
        /// <summary>
        /// Tests that <see cref="DiGi.Core.IO.DelimitedData.Create.Columns(IEnumerable{string})"/> still produces correctly indexed and named
        /// columns, in the original order, after removing the Count()+ElementAt(i) double enumeration over a
        /// single-pass IEnumerable.
        /// </summary>
        [Fact]
        public void Columns()
        {
            IEnumerable<string> SingleUseNames()
            {
                yield return "Col0";
                yield return "Col1";
                yield return "Col2";
            }

            List<Column>? columns = IO.DelimitedData.Create.Columns(SingleUseNames());

            Assert.NotNull(columns);
            Assert.Equal(3, columns.Count);

            for (int i = 0; i < columns.Count; i++)
            {
                Column column = columns[i];
                Assert.Equal(i, column.Index);
                Assert.Equal("Col" + i, column.Name);
                Assert.Equal(typeof(string), column.Type);
            }
        }

        /// <summary>
        /// Tests that an empty sequence of names returns an empty (not null) list of columns, and that a null
        /// sequence returns null.
        /// </summary>
        [Fact]
        public void Columns_EmptyOrNullNames()
        {
            List<string> names_Empty = [];
            List<Column>? columns_Empty = IO.DelimitedData.Create.Columns(names_Empty);

            Assert.NotNull(columns_Empty);
            Assert.Empty(columns_Empty);

            List<string>? names_Null = null;
            List<Column>? columns_Null = IO.DelimitedData.Create.Columns(names_Null);

            Assert.Null(columns_Null);
        }
    }
}