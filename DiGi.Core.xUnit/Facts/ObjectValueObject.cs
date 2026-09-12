using System.Linq;

namespace DiGi.Core.xUnit
{
    public partial class Facts
    {
        /// <summary>
        /// Pins how a bare <see cref="object"/> member holding a value survives the clone leg (no text parse) versus the
        /// text leg, the surface of ZiolkowskiJakub/DiGi.Core#6. The clone leg must keep the exact CLR type it was
        /// handed (an int stays an int), and the text leg must canonicalize a JSON number to the narrowest CLR type that
        /// round-trips, so a whole number reads back as an int rather than a double.
        /// <para>Per value this states which CLR type each leg hands back: int and long stay whole-numbered, double and a
        /// fractional decimal read back as a double, and string, bool and null are unchanged.</para>
        /// </summary>
        [Fact]
        public void ObjectValueObject_NumberRoundTrip()
        {
            object? CloneValue(ObjectValueObject objectValueObject)
            {
                return Assert.IsType<ObjectValueObject>(objectValueObject.Clone()).Value;
            }

            object? TextValue(ObjectValueObject objectValueObject)
            {
                string? text = Convert.ToSystem_String(objectValueObject);
                ObjectValueObject? objectValueObject_Reloaded = Convert.ToDiGi<ObjectValueObject>(text)?.FirstOrDefault();
                Assert.NotNull(objectValueObject_Reloaded);
                return objectValueObject_Reloaded.Value;
            }

            // int - a whole number in the int range: an int on both legs.
            ObjectValueObject intValue = new(2010);
            Query.SerializationCheck(intValue);
            Assert.IsType<int>(CloneValue(intValue));
            Assert.IsType<int>(TextValue(intValue));

            // long - a whole number above the int range: a long on both legs.
            ObjectValueObject longValue = new(2_147_483_648L);
            Query.SerializationCheck(longValue);
            Assert.IsType<long>(CloneValue(longValue));
            Assert.IsType<long>(TextValue(longValue));

            // A whole-number double - the clone leg keeps the double, but the text leg canonicalizes the whole number to an int.
            ObjectValueObject wholeNumberDoubleValue = new(2010.0);
            Query.SerializationCheck(wholeNumberDoubleValue);
            Assert.IsType<double>(CloneValue(wholeNumberDoubleValue));
            Assert.IsType<int>(TextValue(wholeNumberDoubleValue));

            // double - a fractional number: a double on both legs.
            ObjectValueObject doubleValue = new(2010.5);
            Query.SerializationCheck(doubleValue);
            Assert.IsType<double>(CloneValue(doubleValue));
            Assert.IsType<double>(TextValue(doubleValue));

            // decimal - the clone leg keeps the decimal; the text leg has no decimal to recover, so it canonicalizes to a double.
            ObjectValueObject decimalValue = new(2010.5m);
            Query.SerializationCheck(decimalValue);
            Assert.IsType<decimal>(CloneValue(decimalValue));
            Assert.IsType<double>(TextValue(decimalValue));

            // string and bool are not numbers and are unchanged on both legs.
            ObjectValueObject stringValue = new("Residential");
            Query.SerializationCheck(stringValue);
            Assert.IsType<string>(CloneValue(stringValue));
            Assert.IsType<string>(TextValue(stringValue));

            ObjectValueObject boolValue = new(true);
            Query.SerializationCheck(boolValue);
            Assert.IsType<bool>(CloneValue(boolValue));
            Assert.IsType<bool>(TextValue(boolValue));

            // null is null on both legs.
            ObjectValueObject nullValue = new((object?)null);
            Query.SerializationCheck(nullValue);
            Assert.Null(CloneValue(nullValue));
            Assert.Null(TextValue(nullValue));
        }
    }
}
