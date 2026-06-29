using DiGi.Core.Parameter.Classes;
using DiGi.Core.Parameter.Interfaces;

namespace DiGi.Core.xUnit
{
    public partial class Facts
    {
        /// <summary>
        /// Tests basic operations of the ParameterGroup class, including setting, retrieving, checking existence, and removing parameter values.
        /// </summary>
        [Fact]
        public void ParameterGroup_BasicOperations()
        {
            ParameterGroup group = new("General");

            Assert.Equal("General", group.Name);

            // Add/Set values
            bool set1 = group.SetValue("Param1", 42);
            bool set2 = group.SetValue("Param2", "TestValue");

            Assert.True(set1);
            Assert.True(set2);

            // Retrieve values
            int val1 = group.GetValue<int>("Param1");
            string? val2 = group.GetValue<string>("Param2");

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

        /// <summary>
        /// Tests that cloning a ParameterGroup performs a deep copy, isolating changes in the original group from the cloned one.
        /// </summary>
        [Fact]
        public void ParameterGroup_Clone_ShouldPerformDeepCopy()
        {
            ParameterGroup group = new("OriginalGroup");
            group.SetValue("Param1", 100);

            ParameterGroup? clonedGroup = (ParameterGroup?)group.Clone();
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

        /// <summary>
        /// Tests basic operations of the ParameterGroupCollection class, including value assignment, retrieval, cloning, and deep copy isolation.
        /// </summary>
        [Fact]
        public void ParameterGroupCollection_BasicOperations()
        {
            ParameterGroupCollection collection = new();

            // SetValue automatically resolves or creates groups
            bool set1 = collection.SetValue("Param1", 99.99); // Will default to default group name if GroupName is null/default
            Assert.True(set1);

            // Verify contains
            Assert.True(collection.Contains("Param1"));

            // GetValue
            double val = (double)collection.GetValue("Param1")!;
            Assert.Equal(99.99, val);

            // Clone collection
            ParameterGroupCollection? clonedCollection = (ParameterGroupCollection?)collection.Clone();
            Assert.NotNull(clonedCollection);
            Assert.True(clonedCollection.Contains("Param1"));

            // Modify parameter in original collection
            collection.SetValue("Param1", 11.11);

            // Verify deep copy isolation
            Assert.Equal(99.99, (double)clonedCollection.GetValue("Param1")!);
            Assert.Equal(11.11, (double)collection.GetValue("Param1")!);
        }

        /// <summary>
        /// Tests that querying non-existent keys on a ParameterGroup using GetValue or TryGetValue returns null, default, or false without throwing KeyNotFoundExceptions.
        /// </summary>
        [Fact]
        public void ParameterGroup_MissingKeys_ShouldReturnDefaultOrNullWithoutThrowing()
        {
            ParameterGroup group = new("General");

            // Direct GetValue should return null for missing keys
            object? val_Obj = group.GetValue("NonExistentKey");
            Assert.Null(val_Obj);

            // Generic GetValue<T> should return default(T)
            int val_Int = group.GetValue<int>("NonExistentKey");
            Assert.Equal(0, val_Int);

            string? val_Str = group.GetValue<string>("NonExistentKey");
            Assert.Null(val_Str);

            // TryGetValue should safely return false and output null/default
            bool result_Try = group.TryGetValue("NonExistentKey", out object? val_Try);
            Assert.False(result_Try);
            Assert.Null(val_Try);
        }

        /// <summary>
        /// Tests that <see cref="ParameterGroupCollection.GetValue(IParameterDefinition, GetValueSettings)"/> and
        /// <see cref="ParameterGroupCollection.GetValue{T}(IParameterDefinition, GetValueSettings)"/> still return
        /// the correct value after replacing the Contains()+GetValue() double dictionary lookup with a single
        /// TryGetValue() call.
        /// </summary>
        [Fact]
        public void ParameterGroupCollection_GetValue_ByDefinition_ReturnsCorrectValue()
        {
            ParameterGroupCollection collection = new();

            IParameterDefinition parameterDefinition = new SimpleParameterDefinition("Param1");

            Assert.True(collection.SetValue(parameterDefinition, 42));

            object? value_Object = collection.GetValue(parameterDefinition);
            Assert.Equal(42, value_Object);

            int value_Typed = collection.GetValue<int>(parameterDefinition);
            Assert.Equal(42, value_Typed);
        }

        /// <summary>
        /// Tests that <see cref="ParameterGroupCollection.GetValue(IParameterDefinition, GetValueSettings)"/> and
        /// <see cref="ParameterGroupCollection.GetValue{T}(IParameterDefinition, GetValueSettings)"/> return
        /// null/default (not throw) for a definition that was never added, and for a null definition.
        /// </summary>
        [Fact]
        public void ParameterGroupCollection_GetValue_ByDefinition_MissingOrNull_ReturnsDefault()
        {
            ParameterGroupCollection collection = new();

            IParameterDefinition parameterDefinition_Missing = new SimpleParameterDefinition("NeverAdded");

            Assert.Null(collection.GetValue(parameterDefinition_Missing));
            Assert.Equal(0, collection.GetValue<int>(parameterDefinition_Missing));

            IParameterDefinition? parameterDefinition_Null = null;
            Assert.Null(collection.GetValue(parameterDefinition_Null));
            Assert.Equal(0, collection.GetValue<int>(parameterDefinition_Null));
        }

        /// <summary>
        /// Tests that <see cref="ParameterGroupCollection.GetValue(IParameterDefinition, GetValueSettings)"/> correctly
        /// distinguishes between multiple parameters in the collection, returning the value for the requested
        /// definition specifically rather than e.g. the first one found.
        /// </summary>
        [Fact]
        public void ParameterGroupCollection_GetValue_ByDefinition_MultipleParameters()
        {
            ParameterGroupCollection collection = new();

            IParameterDefinition parameterDefinition_1 = new SimpleParameterDefinition("Param1");
            IParameterDefinition parameterDefinition_2 = new SimpleParameterDefinition("Param2");

            Assert.True(collection.SetValue(parameterDefinition_1, "Value1"));
            Assert.True(collection.SetValue(parameterDefinition_2, "Value2"));

            Assert.Equal("Value1", collection.GetValue(parameterDefinition_1));
            Assert.Equal("Value2", collection.GetValue(parameterDefinition_2));
        }
    }
}