using DiGi.Core.Classes;
using System;
using System.ComponentModel;

namespace DiGi.Core.xUnit
{
    public partial class Facts
    {
        /// <summary>
        /// Verifies that an enum carrying <see cref="DescriptionEnumConverter"/> binds a member by its <see cref="DescriptionAttribute"/> text as well as by its name.
        /// <para>ASP.NET Core converts a query, route or form value through <see cref="TypeDescriptor.GetConverter(Type)"/>, so this is the same code path a request takes. A plain <see cref="EnumConverter"/> matches names only, which is what the attribute exists to widen - a member whose accepted wire token differs from its identifier stays bindable without renaming it.</para>
        /// <para>An unknown token still has to throw: binding it to a member would turn a typo into a silently wrong query instead of a 400.</para>
        /// </summary>
        [Fact]
        public void DescriptionEnumConverter_BindsNameAndDescription()
        {
            TypeConverter typeConverter = TypeDescriptor.GetConverter(typeof(TestWireToken));

            Assert.IsType<DescriptionEnumConverter>(typeConverter);

            // The description, the member name, and the description with its spacing restored.
            Assert.Equal(TestWireToken.Aliased, Assert.IsType<TestWireToken>(typeConverter.ConvertFromString("Two Words")));
            Assert.Equal(TestWireToken.Aliased, Assert.IsType<TestWireToken>(typeConverter.ConvertFromString("TwoWords")));
            Assert.Equal(TestWireToken.Aliased, Assert.IsType<TestWireToken>(typeConverter.ConvertFromString("twowords")));
            Assert.Equal(TestWireToken.Aliased, Assert.IsType<TestWireToken>(typeConverter.ConvertFromString("Aliased")));

            // Members whose description equals their name keep binding exactly as the base converter bound them.
            Assert.Equal(TestWireToken.Plain, Assert.IsType<TestWireToken>(typeConverter.ConvertFromString("Plain")));
            Assert.Equal(TestWireToken.Plain, Assert.IsType<TestWireToken>(typeConverter.ConvertFromString("plain")));

            // Numeric text falls through to the base converter, which is the only thing that handles it.
            Assert.Equal(TestWireToken.Aliased, Assert.IsType<TestWireToken>(typeConverter.ConvertFromString("2")));
            Assert.Equal(TestWireToken.Undefined, Assert.IsType<TestWireToken>(typeConverter.ConvertFromString("-1")));

            Assert.ThrowsAny<Exception>(() => typeConverter.ConvertFromString("Nonsense"));
        }

        /// <summary>
        /// Enum whose middle member carries a description that differs from its name, so the converter has something to resolve.
        /// </summary>
        [TypeConverter(typeof(DescriptionEnumConverter))]
        private enum TestWireToken
        {
            /// <summary>Undefined member.</summary>
            [Description("Undefined")] Undefined = -1,

            /// <summary>Member whose description repeats its name.</summary>
            [Description("Plain")] Plain = 1,

            /// <summary>Member reachable by a description that is not its name.</summary>
            [Description("Two Words")] Aliased = 2,
        }
    }
}
