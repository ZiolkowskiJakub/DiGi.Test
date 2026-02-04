using DiGi.Core.Classes;
using System;

namespace DiGi.Core.xUnit
{
    public partial class Classes
    {
        [Fact]
        public void Tag()
        {
            Query.SerializationCheck(new Tag(10.0));
            Query.SerializationCheck(new Tag("AAA"));
            Query.SerializationCheck(new Tag(1));
            Query.SerializationCheck(new Tag(true));
            Query.SerializationCheck(new Tag(DateTime.Now));
        }
    }
}