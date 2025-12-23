using DiGi.Core.Interfaces;

namespace DiGi.Core.xUnit
{
    public class TestUpdater : IUpdater<double>
    {
        public double Addend { get; set; }

        public double Value { get; set; }

        public TestUpdater(double addend) 
        {
            Addend = addend;
        }

        public bool Update()
        {
            Value = Value + Addend;
            return true;
        }
    }
}