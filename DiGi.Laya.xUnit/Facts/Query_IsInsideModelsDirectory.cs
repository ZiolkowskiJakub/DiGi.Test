namespace DiGi.Laya.xUnit
{
    public partial class Facts
    {
        /// <summary>
        /// Verifies that <see cref="Query.IsInsideModelsDirectory(string?)"/> recognises a Laya\models folder, the DiGi.Laya\models software destination (a UNC path included), and anything inside them, case-insensitively and with or without a trailing separator, and not a sibling such as Laya\models_old, a models folder under another parent, the Laya folder itself, or a blank value.
        /// </summary>
        [Fact]
        public void Query_IsInsideModelsDirectory()
        {
            Assert.True(Query.IsInsideModelsDirectory(@"C:\host\bin\Laya\models"));
            Assert.True(Query.IsInsideModelsDirectory(@"C:\host\bin\Laya\models\"));
            Assert.True(Query.IsInsideModelsDirectory(@"C:\host\bin\laya\MODELS\run-1"));
            Assert.True(Query.IsInsideModelsDirectory(@"D:\Laya\models\multilingual\encoder"));

            //The software destination DeployLaya.ps1 deploys to: "Laya" follows a dot there, not a separator
            Assert.True(Query.IsInsideModelsDirectory(@"\\server\Software\DiGi.Laya\models\recipe_classification"));
            Assert.True(Query.IsInsideModelsDirectory(@"C:\Software\digi.laya\Models"));
            Assert.False(Query.IsInsideModelsDirectory(@"C:\Software\DiGi.Laya\venv"));
            Assert.False(Query.IsInsideModelsDirectory(@"C:\Software\XDiGi.Laya\models"));

            Assert.False(Query.IsInsideModelsDirectory(@"D:\Laya\models_old"));
            Assert.False(Query.IsInsideModelsDirectory(@"D:\YOLO\models\run"));
            Assert.False(Query.IsInsideModelsDirectory(@"D:\Laya"));
            Assert.False(Query.IsInsideModelsDirectory(@"D:\Laya\runs\models-test"));
            Assert.False(Query.IsInsideModelsDirectory(" "));
            Assert.False(Query.IsInsideModelsDirectory(null));
        }
    }
}
