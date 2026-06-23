using DiGi.Core.Parameter.Classes;

namespace DiGi.Core.xUnit
{
    public partial class Facts
    {
        [Fact]
        public void ParameterGroup_BasicOperations()
        {
            var group = new ParameterGroup("General");

            Assert.Equal("General", group.Name);

            // Add/Set values
            bool set1 = group.SetValue("Param1", 42);
            bool set2 = group.SetValue("Param2", "TestValue");

            Assert.True(set1);
            Assert.True(set2);

            // Retrieve values
            var val1 = group.GetValue<int>("Param1");
            var val2 = group.GetValue<string>("Param2");

            Assert.Equal(42, val1);
            Assert.Equal("TestValue", val2);

            // Check existence
            Assert.True(group.Contains("Param1"));
            Assert.False(group.Contains("NonExistent"));

            // Remove
            bool removed = group.Remove("Param1");
            Assert.True(removed);
            Assert.False(group.Contains("Param1"));
        }

        [Fact]
        public void ParameterGroup_Clone_ShouldPerformDeepCopy()
        {
            var group = new ParameterGroup("OriginalGroup");
            group.SetValue("Param1", 100);

            var clonedGroup = (ParameterGroup?)group.Clone();
            Assert.NotNull(clonedGroup);
            Assert.Equal("OriginalGroup", clonedGroup.Name);

            // Verify the cloned parameter value is correct
            Assert.Equal(100, clonedGroup.GetValue<int>("Param1"));

            // Modify parameter in original group
            group.SetValue("Param1", 200);

            // Cloned group parameter must remain unaffected
            Assert.Equal(100, clonedGroup.GetValue<int>("Param1"));
            Assert.Equal(200, group.GetValue<int>("Param1"));
        }

        [Fact]
        public void ParameterGroupCollection_BasicOperations()
        {
            var collection = new ParameterGroupCollection();

            // SetValue automatically resolves or creates groups
            bool set1 = collection.SetValue("Param1", 99.99); // Will default to default group name if GroupName is null/default
            Assert.True(set1);

            // Verify contains
            Assert.True(collection.Contains("Param1"));

            // GetValue
            double val = (double)collection.GetValue("Param1")!;
            Assert.Equal(99.99, val);

            // Clone collection
            var clonedCollection = (ParameterGroupCollection?)collection.Clone();
            Assert.NotNull(clonedCollection);
            Assert.True(clonedCollection.Contains("Param1"));

            // Modify parameter in original collection
            collection.SetValue("Param1", 11.11);

            // Verify deep copy isolation
            Assert.Equal(99.99, (double)clonedCollection.GetValue("Param1")!);
            Assert.Equal(11.11, (double)collection.GetValue("Param1")!);
        }
    }
}