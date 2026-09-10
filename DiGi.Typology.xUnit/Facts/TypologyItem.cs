using DiGi.Typology.Classes;

namespace DiGi.Typology.xUnit
{
    public partial class Facts
    {
        /// <summary>
        /// Tests <see cref="Typology.Classes.TypologyItem"/> construction across all overloads, path cloning
        /// independence, ordering and serialization round-trip.
        /// <para>The sort assertion is the regression guard for the comparison contract: a
        /// path-less item used to report int.MinValue against everything, including another
        /// path-less item, which makes List.Sort reject the comparer as inconsistent.</para>
        /// </summary>
        [Fact]
        public void TypologyItem()
        {
            TypologyPath typologyPath = new([1, 2]);

            TypologyItem typologyItem = new(typologyPath, "CCC", "Test CCC");

            Assert.Equal("CCC", typologyItem.Name);
            Assert.Equal("Test CCC", typologyItem.Description);
            Assert.Equal(typologyPath, typologyItem.TypologyPath);
            Assert.NotSame(typologyPath, typologyItem.TypologyPath);
            Assert.Equal("[1.2] CCC", typologyItem.ToString());

            TypologyItem typologyItem_Values = new([1, 2], "CCC", "Test CCC");

            Assert.Equal(typologyPath, typologyItem_Values.TypologyPath);
            Assert.Equal("CCC", typologyItem_Values.Name);

            TypologyItem typologyItem_Name = new([1, 2], "CCC");

            Assert.Equal(typologyPath, typologyItem_Name.TypologyPath);
            Assert.Null(typologyItem_Name.Description);

            TypologyItem typologyItem_PathName = new(typologyPath, "CCC");

            Assert.Equal(typologyPath, typologyItem_PathName.TypologyPath);

            TypologyItem typologyItem_Copy = new(new TypologyPath([3]), typologyItem);

            Assert.Equal(new TypologyPath([3]), typologyItem_Copy.TypologyPath);
            Assert.Equal("CCC", typologyItem_Copy.Name);
            Assert.Equal("Test CCC", typologyItem_Copy.Description);

            TypologyItem typologyItem_Clone = new(typologyItem);

            Assert.Equal(typologyItem.TypologyPath, typologyItem_Clone.TypologyPath);
            Assert.NotSame(typologyItem.TypologyPath, typologyItem_Clone.TypologyPath);

            TypologyItem typologyItem_Empty = new();

            Assert.Null(typologyItem_Empty.TypologyPath);
            Assert.Equal("???", typologyItem_Empty.ToString());

            TypologyItem typologyItem_NoPath = new((TypologyPath?)null, "AAA");

            Assert.Equal("AAA", typologyItem_NoPath.ToString());

            Assert.Equal(0, typologyItem_Empty.CompareTo(new TypologyItem()));
            Assert.True(typologyItem_Empty.CompareTo(typologyItem) < 0);
            Assert.True(typologyItem.CompareTo(typologyItem_Empty) > 0);
            Assert.True(typologyItem.CompareTo(null!) > 0);
            Assert.Equal(0, typologyItem.CompareTo(typologyItem_Values));

            List<TypologyItem> typologyItems = [typologyItem_Copy, typologyItem_Empty, new(), typologyItem];
            typologyItems.Sort();

            Assert.Equal(4, typologyItems.Count);
            Assert.Null(typologyItems[0].TypologyPath);
            Assert.Null(typologyItems[1].TypologyPath);
            Assert.Equal(typologyPath, typologyItems[2].TypologyPath);

            TypologyItem typologyItem_OtherName = new(typologyPath, "DDD", "Test CCC");

            Assert.True(typologyItem.Equals(typologyItem_Values));
            Assert.True(typologyItem == typologyItem_Values);
            Assert.False(typologyItem != typologyItem_Values);
            Assert.Equal(typologyItem.GetHashCode(), typologyItem_Values.GetHashCode());
            Assert.True(typologyItem_Empty == new TypologyItem());
            Assert.False(typologyItem.Equals(typologyItem_Name));
            Assert.False(typologyItem.Equals(null));
            Assert.False(typologyItem == null);
            Assert.False(null == typologyItem);
            Assert.NotEqual(typologyItem, typologyItem_OtherName);

            Assert.True(typologyItem_Name.CompareTo(typologyItem) < 0);
            Assert.True(typologyItem.CompareTo(typologyItem_OtherName) < 0);
            Assert.True(typologyItem_OtherName.CompareTo(typologyItem) > 0);

            Core.xUnit.Query.SerializationCheck(typologyItem);
        }
    }
}
