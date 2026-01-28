namespace DiGi.Core.xUnit
{
    public partial class Classes
    {
        [Fact]
        public void MultiUpdater()
        {
            Core.Classes.MultiUpdater<double> multiUpdater = new Core.Classes.MultiUpdater<double>([new TestUpdater(1), new TestUpdater(2)])
            {
                Value = 10
            };

            Assert.True(multiUpdater.Update());

            Assert.Equal(13, multiUpdater.Value);
        }
    }
}