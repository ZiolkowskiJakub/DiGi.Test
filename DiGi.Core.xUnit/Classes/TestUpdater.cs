using DiGi.Core.Classes;
using DiGi.Core.Interfaces;
using DiGi.Core.xUnit.Enums;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

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