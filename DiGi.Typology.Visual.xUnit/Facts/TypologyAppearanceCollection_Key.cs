using DiGi.Core.Enums;
using DiGi.Typology.Classes;
using DiGi.Typology.Visual.Classes;
using System.Globalization;

namespace DiGi.Typology.Visual.xUnit
{
    public partial class Facts
    {
        /// <summary>
        /// Tests that <see cref="Classes.TypologyAppearanceCollection.Key(object)"/> agrees with <see cref="object.Equals(object)"/>
        /// and does not depend on the current culture, so that a key written by one process is found by another.
        /// <para>Each case pairs the key with the raw <c>ToString()</c> it replaces, showing where that form would have
        /// broken the invariant: a decimal keeps its scale, negative zero prints its sign, a DateTime drops its ticks,
        /// and every floating-point and date form follows the current culture.</para>
        /// </summary>
        [Fact]
        public void TypologyAppearanceCollection_Key()
        {
            // Integers of any width, and the string form, share a key.
            Assert.Equal("2010", Classes.TypologyAppearanceCollection.Key(2010));
            Assert.Equal("2010", Classes.TypologyAppearanceCollection.Key(2010L));
            Assert.Equal("2010", Classes.TypologyAppearanceCollection.Key((short)2010));
            Assert.Equal("210", Classes.TypologyAppearanceCollection.Key((byte)210));
            Assert.Equal("2010", Classes.TypologyAppearanceCollection.Key("2010"));
            Assert.Equal("-5", Classes.TypologyAppearanceCollection.Key(-5));

            // A decimal is keyed without its scale, because 1.10m equals 1.1m.
            Assert.True(1.10m.Equals(1.1m));
            Assert.NotEqual(1.10m.ToString(CultureInfo.InvariantCulture), 1.1m.ToString(CultureInfo.InvariantCulture));
            Assert.Equal("1.1", Classes.TypologyAppearanceCollection.Key(1.10m));
            Assert.Equal(Classes.TypologyAppearanceCollection.Key(1.1m), Classes.TypologyAppearanceCollection.Key(1.10m));
            Assert.Equal("2010", Classes.TypologyAppearanceCollection.Key(2010.00m));

            // Negative zero equals zero, so it is keyed as zero.
            Assert.True((-0.0).Equals(0.0));
            Assert.NotEqual((0.0).ToString(CultureInfo.InvariantCulture), (-0.0).ToString(CultureInfo.InvariantCulture));
            Assert.Equal("0", Classes.TypologyAppearanceCollection.Key(-0.0));
            Assert.Equal(Classes.TypologyAppearanceCollection.Key(0.0), Classes.TypologyAppearanceCollection.Key(-0.0));
            Assert.Equal("0", Classes.TypologyAppearanceCollection.Key(-0.0f));
            Assert.Equal("1.5", Classes.TypologyAppearanceCollection.Key(1.5));
            Assert.Equal("1.5", Classes.TypologyAppearanceCollection.Key(1.5f));
            Assert.Equal("NaN", Classes.TypologyAppearanceCollection.Key(double.NaN));
            Assert.Equal("Infinity", Classes.TypologyAppearanceCollection.Key(double.PositiveInfinity));

            // A DateTime is keyed to the tick with its kind ignored, as its equality is.
            DateTime dateTime_Utc = new(2024, 1, 15, 10, 30, 0, DateTimeKind.Utc);
            DateTime dateTime_Local = new(dateTime_Utc.Ticks, DateTimeKind.Local);
            DateTime dateTime_Tick = dateTime_Utc.AddTicks(1);

            Assert.True(dateTime_Utc.Equals(dateTime_Local));
            Assert.False(dateTime_Utc.Equals(dateTime_Tick));
            Assert.Equal(dateTime_Utc.ToString(CultureInfo.InvariantCulture), dateTime_Tick.ToString(CultureInfo.InvariantCulture));
            Assert.Equal("2024-01-15T10:30:00.0000000", Classes.TypologyAppearanceCollection.Key(dateTime_Utc));
            Assert.Equal(Classes.TypologyAppearanceCollection.Key(dateTime_Utc), Classes.TypologyAppearanceCollection.Key(dateTime_Local));
            Assert.NotEqual(Classes.TypologyAppearanceCollection.Key(dateTime_Utc), Classes.TypologyAppearanceCollection.Key(dateTime_Tick));

            // A DateTimeOffset is keyed as the UTC instant its equality compares.
            DateTimeOffset dateTimeOffset_1 = new(2024, 1, 15, 10, 0, 0, TimeSpan.FromHours(1));
            DateTimeOffset dateTimeOffset_2 = new(2024, 1, 15, 9, 0, 0, TimeSpan.Zero);

            Assert.True(dateTimeOffset_1.Equals(dateTimeOffset_2));
            Assert.NotEqual(dateTimeOffset_1.ToString(CultureInfo.InvariantCulture), dateTimeOffset_2.ToString(CultureInfo.InvariantCulture));
            Assert.Equal("2024-01-15T09:00:00.0000000Z", Classes.TypologyAppearanceCollection.Key(dateTimeOffset_1));
            Assert.Equal(Classes.TypologyAppearanceCollection.Key(dateTimeOffset_1), Classes.TypologyAppearanceCollection.Key(dateTimeOffset_2));

            // Culture-free types keep their ToString form.
            Guid guid = Guid.NewGuid();

            Assert.Equal("True", Classes.TypologyAppearanceCollection.Key(true));
            Assert.Equal(guid.ToString(), Classes.TypologyAppearanceCollection.Key(guid));
            Assert.Equal("Int", Classes.TypologyAppearanceCollection.Key(DataType.Int));
            Assert.Equal("01:30:00", Classes.TypologyAppearanceCollection.Key(TimeSpan.FromMinutes(90)));

            // Null and its documented collision with the literal text "null"; rule data resolves to its value.
            Assert.Equal("null", Classes.TypologyAppearanceCollection.Key(null));
            Assert.Equal(Classes.TypologyAppearanceCollection.Key(null), Classes.TypologyAppearanceCollection.Key("null"));
            Assert.Equal(Classes.TypologyAppearanceCollection.Key(2010), Classes.TypologyAppearanceCollection.Key(new UniqueValueRuleData(2010)));
            Assert.Equal(Classes.TypologyAppearanceCollection.Key(null), Classes.TypologyAppearanceCollection.Key(new UniqueValueRuleData((object?)null)));
            Assert.Equal(Classes.TypologyAppearanceCollection.Key(2010), Classes.TypologyAppearanceCollection.Key(new UniqueValueRuleData(2010).ToString()));

            // The key does not follow the current culture, while the raw ToString does. CurrentCulture is per thread.
            CultureInfo cultureInfo = CultureInfo.CurrentCulture;
            try
            {
                CultureInfo.CurrentCulture = new CultureInfo("pl-PL");

                Assert.NotEqual("1.5", 1.5.ToString());
                Assert.NotEqual("1.5", 1.5m.ToString());
                Assert.NotEqual("Infinity", double.PositiveInfinity.ToString());
                Assert.NotEqual("2024-01-15T10:30:00.0000000", dateTime_Utc.ToString());

                Assert.Equal("1.5", Classes.TypologyAppearanceCollection.Key(1.5));
                Assert.Equal("1.5", Classes.TypologyAppearanceCollection.Key(1.5m));
                Assert.Equal("Infinity", Classes.TypologyAppearanceCollection.Key(double.PositiveInfinity));
                Assert.Equal("2024-01-15T10:30:00.0000000", Classes.TypologyAppearanceCollection.Key(dateTime_Utc));
                Assert.Equal("-5", Classes.TypologyAppearanceCollection.Key(-5));
            }
            finally
            {
                CultureInfo.CurrentCulture = cultureInfo;
            }
        }
    }
}
