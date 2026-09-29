using System.IO;

namespace DiGi.YOLO.xUnit
{
    public partial class Facts
    {
        /// <summary>
        /// Verifies that <see cref="Query.IsInsideModelsDirectory(string?)"/> recognises a YOLO\models folder and everything inside it, whatever the case and with or without a trailing separator, and nothing beside it.
        /// <para>It is the one copy of the rule that keeps every training run - <c>Modify.Train</c>, the runner's <c>--train</c> preflight and the tray task's - out of the folder the frozen weights live in. The sibling <c>models_old</c> is the case a plain prefix test would get wrong, and the relative path is resolved against the current directory, which is placed under a models folder here so the resolution is what decides the answer.</para>
        /// </summary>
        [Fact]
        public void IsInsideModelsDirectory()
        {
            string root = Path.Combine(Path.GetTempPath(), "DiGi_YOLO_IsInsideModelsDirectory");

            Assert.True(Query.IsInsideModelsDirectory(Path.Combine(root, "YOLO", "models")));
            Assert.True(Query.IsInsideModelsDirectory(Path.Combine(root, "YOLO", "models") + Path.DirectorySeparatorChar));
            Assert.True(Query.IsInsideModelsDirectory(Path.Combine(root, "YOLO", "models", "runs", "train9")));
            Assert.True(Query.IsInsideModelsDirectory(Path.Combine(root, "yolo", "MODELS", "base")));

            Assert.False(Query.IsInsideModelsDirectory(Path.Combine(root, "YOLO", "models_old")));
            Assert.False(Query.IsInsideModelsDirectory(Path.Combine(root, "YOLO", "runs")));
            Assert.False(Query.IsInsideModelsDirectory(Path.Combine(root, "models")));
            Assert.False(Query.IsInsideModelsDirectory(root));

            Assert.False(Query.IsInsideModelsDirectory(null));
            Assert.False(Query.IsInsideModelsDirectory(" "));

            string currentDirectory = Directory.GetCurrentDirectory();
            string directory_Models = Path.Combine(root, "YOLO", "models");

            try
            {
                Directory.CreateDirectory(directory_Models);
                Directory.SetCurrentDirectory(directory_Models);

                Assert.True(Query.IsInsideModelsDirectory("train9"));
                Assert.False(Query.IsInsideModelsDirectory(Path.Combine("..", "runs")));
            }
            finally
            {
                Directory.SetCurrentDirectory(currentDirectory);

                if (Directory.Exists(root))
                {
                    Directory.Delete(root, true);
                }
            }
        }
    }
}
