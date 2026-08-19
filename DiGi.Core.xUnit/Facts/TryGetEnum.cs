using System;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace DiGi.Core.xUnit
{
    public partial class Facts
    {
        /// <summary>
        /// Tests the TryGetEnum and TryConvert_Enum query methods, verifying exact name, exact description, fuzzy, numeric, and failure matching semantics.
        /// </summary>
        [Fact]
        public void TryGetEnum()
        {
            // Exact name matching
            Assert.True(Core.Query.TryGetEnum("Plain", typeof(TestWireToken), out Enum? enum_ExactName));
            Assert.Equal(TestWireToken.Plain, enum_ExactName);

            Assert.True(Core.Query.TryGetEnum("Aliased", typeof(TestWireToken), out Enum? enum_ExactAliased));
            Assert.Equal(TestWireToken.Aliased, enum_ExactAliased);

            // Exact description matching
            Assert.True(Core.Query.TryGetEnum("Two Words", typeof(TestWireToken), out Enum? enum_ExactDescription));
            Assert.Equal(TestWireToken.Aliased, enum_ExactDescription);

            // Fuzzy matching (case-insensitive and whitespace-insensitive)
            Assert.True(Core.Query.TryGetEnum("TwoWords", typeof(TestWireToken), out Enum? enum_FuzzyDescription));
            Assert.Equal(TestWireToken.Aliased, enum_FuzzyDescription);

            Assert.True(Core.Query.TryGetEnum("twowords", typeof(TestWireToken), out Enum? enum_FuzzyDescriptionLower));
            Assert.Equal(TestWireToken.Aliased, enum_FuzzyDescriptionLower);

            Assert.True(Core.Query.TryGetEnum("plain", typeof(TestWireToken), out Enum? enum_FuzzyNameLower));
            Assert.Equal(TestWireToken.Plain, enum_FuzzyNameLower);

            // Numeric string matching
            Assert.True(Core.Query.TryGetEnum("2", typeof(TestWireToken), out Enum? enum_NumericPositive));
            Assert.Equal(TestWireToken.Aliased, enum_NumericPositive);

            Assert.True(Core.Query.TryGetEnum("-1", typeof(TestWireToken), out Enum? enum_NumericNegative));
            Assert.Equal(TestWireToken.Undefined, enum_NumericNegative);

            // Generic overload
            Assert.True(Core.Query.TryGetEnum("Two Words", out TestWireToken enum_Generic));
            Assert.Equal(TestWireToken.Aliased, enum_Generic);

            Assert.True(Core.Query.TryGetEnum("2", out TestWireToken enum_GenericNumeric));
            Assert.Equal(TestWireToken.Aliased, enum_GenericNumeric);

            // Type-prefixed string format ("TypeFullName:MemberName")
            string prefix = typeof(TestWireToken).FullName!;
            Assert.True(Core.Query.TryGetEnum($"{prefix}:Aliased", out Enum? enum_PrefixedName));
            Assert.Equal(TestWireToken.Aliased, enum_PrefixedName);

            Assert.True(Core.Query.TryGetEnum($"{prefix}:Two Words", out Enum? enum_PrefixedDescription));
            Assert.Equal(TestWireToken.Aliased, enum_PrefixedDescription);

            Assert.True(Core.Query.TryGetEnum($"{prefix}:2", out Enum? enum_PrefixedNumeric));
            Assert.Equal(TestWireToken.Aliased, enum_PrefixedNumeric);

            // Failure cases must return false and assign null to out parameter (no Undefined leakage)
            Assert.False(Core.Query.TryGetEnum("Nonsense", typeof(TestWireToken), out Enum? enum_Failure));
            Assert.Null(enum_Failure);

            Assert.False(Core.Query.TryGetEnum("999", typeof(TestWireToken), out Enum? enum_NumericFailure));
            Assert.Null(enum_NumericFailure);

            Assert.False(Core.Query.TryGetEnum(string.Empty, typeof(TestWireToken), out Enum? enum_EmptyFailure));
            Assert.Null(enum_EmptyFailure);

            Assert.False(Core.Query.TryGetEnum(null, typeof(TestWireToken), out Enum? enum_NullFailure));
            Assert.Null(enum_NullFailure);

            Assert.False(Core.Query.TryGetEnum("Aliased", typeof(string), out Enum? enum_NonEnumType));
            Assert.Null(enum_NonEnumType);

            // TryConvert_Enum integration
            Assert.True(Core.Query.TryConvert_Enum("Aliased", out Enum? enum_ConvertString, typeof(TestWireToken)));
            Assert.Equal(TestWireToken.Aliased, enum_ConvertString);

            Assert.True(Core.Query.TryConvert_Enum("Two Words", out Enum? enum_ConvertDesc, typeof(TestWireToken)));
            Assert.Equal(TestWireToken.Aliased, enum_ConvertDesc);

            Assert.True(Core.Query.TryConvert_Enum("2", out Enum? enum_ConvertNumericString, typeof(TestWireToken)));
            Assert.Equal(TestWireToken.Aliased, enum_ConvertNumericString);

            Assert.True(Core.Query.TryConvert_Enum(2, out Enum? enum_ConvertInt, typeof(TestWireToken)));
            Assert.Equal(TestWireToken.Aliased, enum_ConvertInt);

            Assert.True(Core.Query.TryConvert_Enum((long)2, out Enum? enum_ConvertLong, typeof(TestWireToken)));
            Assert.Equal(TestWireToken.Aliased, enum_ConvertLong);

            Assert.True(Core.Query.TryConvert_Enum(TestWireToken.Aliased, out Enum? enum_ConvertEnum, typeof(TestWireToken)));
            Assert.Equal(TestWireToken.Aliased, enum_ConvertEnum);

            JsonNode jsonNode_String = JsonValue.Create("Two Words")!;
            Assert.True(Core.Query.TryConvert_Enum(jsonNode_String, out Enum? enum_ConvertJsonNodeString, typeof(TestWireToken)));
            Assert.Equal(TestWireToken.Aliased, enum_ConvertJsonNodeString);

            JsonNode jsonNode_Number = JsonValue.Create(2)!;
            Assert.True(Core.Query.TryConvert_Enum(jsonNode_Number, out Enum? enum_ConvertJsonNodeNumber, typeof(TestWireToken)));
            Assert.Equal(TestWireToken.Aliased, enum_ConvertJsonNodeNumber);

            using JsonDocument jsonDocument = JsonDocument.Parse("{\"stringVal\":\"Two Words\",\"numVal\":2}");
            JsonElement jsonElement_String = jsonDocument.RootElement.GetProperty("stringVal");
            Assert.True(Core.Query.TryConvert_Enum(jsonElement_String, out Enum? enum_ConvertJsonElementString, typeof(TestWireToken)));
            Assert.Equal(TestWireToken.Aliased, enum_ConvertJsonElementString);

            JsonElement jsonElement_Number = jsonDocument.RootElement.GetProperty("numVal");
            Assert.True(Core.Query.TryConvert_Enum(jsonElement_Number, out Enum? enum_ConvertJsonElementNumber, typeof(TestWireToken)));
            Assert.Equal(TestWireToken.Aliased, enum_ConvertJsonElementNumber);
        }
    }
}
