using DiGi.Core.Classes;
using System.Collections.Generic;

namespace DiGi.Core.xUnit
{
    public partial class Facts
    {
        /// <summary>
        /// Tests that escaping is invertible for every character class that matters: the grammar metacharacters, the
        /// decorations the old format used to depend on, unicode, and the empty string.
        /// </summary>
        [Fact]
        public void ReferenceEscaping_Escaped_Unescaped_RoundTrip()
        {
            string[] values =
            [
                "plain",
                "A::B",
                "A\"B",
                "[A]",
                "a->b",
                "A\\B",
                "A(B)C",
                "\\0",
                "\\:\\:",
                ":",
                "(",
                ")",
                "\\",
                "Zażółć gęślą jaźń",
                "日本語",
                string.Empty,
            ];

            foreach (string value in values)
            {
                Assert.Equal(value, Core.Query.Unescaped(Core.Query.Escaped(value)));
            }
        }

        /// <summary>
        /// Tests that null and the empty string escape to different tokens and both survive the round trip, so a
        /// missing value stays distinguishable from a present but empty one.
        /// </summary>
        [Fact]
        public void ReferenceEscaping_NullVersusEmpty()
        {
            Assert.Equal(Constants.Reference.Null, Core.Query.Escaped(null));
            Assert.Equal(string.Empty, Core.Query.Escaped(string.Empty));

            Assert.Null(Core.Query.Unescaped(Constants.Reference.Null));
            Assert.Equal(string.Empty, Core.Query.Unescaped(string.Empty));
        }

        /// <summary>
        /// Tests that a payload consisting literally of the null token is not read back as null. A literal escape
        /// character is always doubled, so a real value can never masquerade as the token.
        /// </summary>
        [Fact]
        public void ReferenceEscaping_LiteralNullTokenPayload()
        {
            string value = Constants.Reference.Null;

            Assert.NotEqual(Constants.Reference.Null, Core.Query.Escaped(value));
            Assert.Equal(value, Core.Query.Unescaped(Core.Query.Escaped(value)));
        }

        /// <summary>
        /// Tests that a unique identifier containing the separator survives a full reference round trip and does not
        /// split the enclosing string. Nothing was escaped in the old format, so such a value was unrecoverable.
        /// </summary>
        [Fact]
        public void ReferenceEscaping_UniqueIdContainingSeparator()
        {
            TypeReference typeReference = new(typeof(TestObject));
            UniqueIdReference uniqueIdReference = new(typeReference, "A::B");

            Assert.True(Core.Query.TryParse(uniqueIdReference.ToString(), out UniqueIdReference? uniqueIdReference_Parsed));
            Assert.NotNull(uniqueIdReference_Parsed);
            Assert.Equal("A::B", uniqueIdReference_Parsed.UniqueId);

            Assert.True(Core.Query.TryGetDiscriminator(uniqueIdReference.ToString(), out string? discriminator, out string? body));
            Assert.Equal(Constants.Reference.Kind.UniqueId, discriminator);

            List<string>? segments = Core.Query.Segments(body);
            Assert.NotNull(segments);
            Assert.Equal(2, segments.Count);
        }

        /// <summary>
        /// Tests that the characters the old format reserved as decorations - quotes, brackets and the arrow the
        /// PostgreSQL partition references used - are ordinary payload now and survive a round trip untouched.
        /// </summary>
        [Fact]
        public void ReferenceEscaping_UniqueIdContainingQuotesAndBrackets()
        {
            TypeReference typeReference = new(typeof(TestObject));

            foreach (string uniqueId in new[] { "A\"B[C]->D", "\"quoted\"", "[bracketed]", "A(B)C", "A\\B" })
            {
                UniqueIdReference uniqueIdReference = new(typeReference, uniqueId);

                Assert.True(Core.Query.TryParse(uniqueIdReference.ToString(), out UniqueIdReference? uniqueIdReference_Parsed), uniqueId);
                Assert.NotNull(uniqueIdReference_Parsed);
                Assert.Equal(uniqueId, uniqueIdReference_Parsed.UniqueId);
            }
        }

        /// <summary>
        /// Tests that a nested reference whose payload contains the separator survives two levels of nesting, so
        /// escaping composes rather than only working at the outermost level.
        /// </summary>
        [Fact]
        public void ReferenceEscaping_SurvivesTwoNestingLevels()
        {
            TypeReference typeReference = new(typeof(TestObject));
            UniqueIdReference uniqueIdReference = new(typeReference, "A::B(C)\\D");
            UniqueIdPropertyReference uniqueIdPropertyReference = new(uniqueIdReference, "P::Q");

            ComplexReference complexReference = new([uniqueIdPropertyReference]);

            Assert.True(Core.Query.TryParse(complexReference.ToString(), out ComplexReference? complexReference_Parsed));
            Assert.NotNull(complexReference_Parsed);

            UniqueIdPropertyReference? uniqueIdPropertyReference_Parsed = complexReference_Parsed[0] as UniqueIdPropertyReference;
            Assert.NotNull(uniqueIdPropertyReference_Parsed);
            Assert.Equal("P::Q", uniqueIdPropertyReference_Parsed.PropertyName);
            Assert.Equal("A::B(C)\\D", uniqueIdPropertyReference_Parsed.Reference?.UniqueId);
        }

        /// <summary>
        /// Tests that <see cref="Core.Query.Segments(string?)"/> preserves empty segments and rejects malformed
        /// bodies, rather than silently dropping information as a RemoveEmptyEntries split did.
        /// </summary>
        [Fact]
        public void ReferenceEscaping_Segments_PreservesEmptyAndRejectsMalformed()
        {
            List<string>? segments = Core.Query.Segments("a::::b");
            Assert.NotNull(segments);
            Assert.Equal(3, segments.Count);
            Assert.Equal(string.Empty, segments[1]);

            Assert.Null(Core.Query.Segments("(unbalanced"));
            Assert.Null(Core.Query.Segments("unbalanced)"));
            Assert.Null(Core.Query.Segments("trailing\\"));
        }
    }
}