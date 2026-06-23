using System.Collections.Generic;
using DiGi.Core.Classes;

namespace DiGi.Core.xUnit
{
    public partial class Facts
    {
        /// <summary>
        /// Tests the Category class, verifying hierarchical construction, deep copying, and value-based equality.
        /// </summary>
        [Fact]
        public void Category_HierarchicalAndClone()
        {
            // 1. Hierarchical Construction
            Category category_Root = new("Root");
            Category category_Sub1 = new("Sub1");
            Category category_Sub2 = new("Sub2");

            Assert.True(category_Root.Add(category_Sub1));
            Assert.True(category_Root.Add(category_Sub2));

            // Verify count of subcategories using collection naming guidelines (plural suffix, no list prefix)
            List<Category>? categories_Sub = category_Root.SubCategories;
            Assert.NotNull(categories_Sub);
            Assert.Equal(2, categories_Sub.Count);

            // 2. Deep Copying via Clone() (Verifying the fix for Bug 1)
            Category? category_Clone = category_Root.Clone() as Category;
            Assert.NotNull(category_Clone);
            Assert.Equal("Root", category_Clone.Name);

            // Verify that the original category's subcategories were NOT destroyed/cleared by Clone()
            List<Category>? categories_OriginalSub = category_Root.SubCategories;
            Assert.NotNull(categories_OriginalSub);
            Assert.Equal(2, categories_OriginalSub.Count);

            // Verify that the clone successfully copied the subcategories
            List<Category>? categories_CloneSub = category_Clone.SubCategories;
            Assert.NotNull(categories_CloneSub);
            Assert.Equal(2, categories_CloneSub.Count);

            // 3. Value-Based Equality (Verifying the refactoring for Bug 2)
            Category category_Root2 = new("Root");
            Category category_Root2Sub1 = new("Sub1");
            Category category_Root2Sub2 = new("Sub2");
            category_Root2.Add(category_Root2Sub1);
            category_Root2.Add(category_Root2Sub2);

            Assert.True(category_Root.Equals(category_Root2));
            Assert.True(category_Root == category_Root2);

            // Modify one subcategory in category_Root3 and verify they are no longer equal
            Category category_Root3 = new("Root");
            Category category_Root3Sub1 = new("Sub1");
            Category category_Root3Sub3 = new("Sub3");
            category_Root3.Add(category_Root3Sub1);
            category_Root3.Add(category_Root3Sub3);

            Assert.False(category_Root.Equals(category_Root3));
            Assert.True(category_Root != category_Root3);
        }
    }
}
