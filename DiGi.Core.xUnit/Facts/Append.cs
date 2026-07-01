using System.Text;

namespace DiGi.Core.xUnit
{
    public partial class Facts
    {
        /// <summary>
        /// Tests the Append method in the Modify class, verifying that type names are correctly formatted
        /// and appended to a StringBuilder instance, including handling of null inputs.
        /// </summary>
        [Fact]
        public void Append()
        {
            // 1. Test Null inputs
            StringBuilder? stringBuilder_Null = null;
            System.Type type_Int = typeof(int);
            Modify.Append(stringBuilder_Null, type_Int);

            StringBuilder stringBuilder_Valid = new StringBuilder();
            Modify.Append(stringBuilder_Valid, null);
            Assert.Equal(0, stringBuilder_Valid.Length);

            // 2. Test valid type formatting and appending
            StringBuilder stringBuilder_Int = new StringBuilder();
            Modify.Append(stringBuilder_Int, typeof(int));

            string? string_IntAssemblyFullName = typeof(int).Assembly.FullName;
            Assert.NotNull(string_IntAssemblyFullName);
            int int_IntCommaIndex = string_IntAssemblyFullName.IndexOf(',');
            string string_IntAssemblyShortName = int_IntCommaIndex > 0 ? string_IntAssemblyFullName.Substring(0, int_IntCommaIndex) : string_IntAssemblyFullName;
            string string_ExpectedIntName = "System.Int32," + string_IntAssemblyShortName;
            Assert.Equal(string_ExpectedIntName, stringBuilder_Int.ToString());
        }
    }
}