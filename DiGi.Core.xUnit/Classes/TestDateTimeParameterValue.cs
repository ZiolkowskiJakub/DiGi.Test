using DiGi.Core.Interfaces;
using DiGi.Core.Parameter.Classes;
using DiGi.Core.Parameter.Enums;

namespace DiGi.Core.xUnit
{
    public partial class Classes
    {
        /// <summary>
        /// Minimal concrete <see cref="ParameterValue"/> reporting <see cref="ParameterType.DateTime"/>, used to
        /// exercise the base ParameterValue.TryConvert DateTime branch. No concrete DateTimeParameterValue class
        /// exists yet in DiGi.Core.Parameter, so this test-only subclass stands in for one.
        /// </summary>
        public class TestDateTimeParameterValue : ParameterValue
        {
            public override ParameterType ParameterType => ParameterType.DateTime;

            public override ISerializableObject? Clone()
            {
                return new TestDateTimeParameterValue();
            }
        }
    }
}