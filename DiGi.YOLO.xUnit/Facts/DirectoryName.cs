namespace DiGi.YOLO.xUnit
{
    public partial class Facts
    {
        /// <summary>
        /// Verifies that <see cref="Query.DirectoryName(Enums.Category)"/> returns the expected folder name for each dataset split category and <c>null</c> for unmapped values.
        /// </summary>
        [Fact]
        public void DirectoryName()
        {
            Assert.Equal("train", Enums.Category.Train.DirectoryName());
            Assert.Equal("val", Enums.Category.Validate.DirectoryName());
            Assert.Equal("test", Enums.Category.Test.DirectoryName());

            Assert.Equal("train", Query.DirectoryName(Enums.Category.Train));
            Assert.Equal("val", Query.DirectoryName(Enums.Category.Validate));
            Assert.Equal("test", Query.DirectoryName(Enums.Category.Test));

            Enums.Category invalidCategory = (Enums.Category)999;
            Assert.Null(invalidCategory.DirectoryName());
            Assert.Null(Query.DirectoryName(invalidCategory));
        }
    }
}
