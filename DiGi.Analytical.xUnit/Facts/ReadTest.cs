using DiGi.Core.Interfaces;

namespace DiGi.Analytical.xUnit
{
    public partial class Facts
    {
        /// <summary>
        /// Tests reading serializable objects from a specified file path.
        /// </summary>
        [Fact]
        public void ReadTest()
        {
            string path = @"C:\Users\jakub\Downloads\InternalGain.txt";

            if (File.Exists(path))
            {
                List<ISerializableObject>? serializableObjects = Core.Convert.ToDiGi<ISerializableObject>(new Core.Classes.Path(path));
                Assert.NotNull(serializableObjects);
            }
        }
    }
}