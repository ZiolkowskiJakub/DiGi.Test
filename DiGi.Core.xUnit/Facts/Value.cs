using DiGi.Core.Classes;
using System;
using System.Linq;
using System.Text.Json.Nodes;

namespace DiGi.Core.xUnit
{
    public partial class Facts
    {
        /// <summary>
        /// Tests the serialization, cloning, and type wrapping of all 18 primitive/system scalar types, including copy constructors and GetValue behavior.
        /// </summary>
        [Fact]
        public void Value()
        {
            double doubleVal = 10.5;
            Value value_Double = new(doubleVal);
            Assert.Equal(typeof(double), value_Double.ValueType);
            Assert.Equal(doubleVal, value_Double.GetValue<double>());
            Query.SerializationCheck(value_Double);

            DateTime dateTime_Val = DateTime.Now;
            Value value_DateTime = new(dateTime_Val);
            Assert.Equal(typeof(DateTime), value_DateTime.ValueType);
            Assert.Equal(dateTime_Val, value_DateTime.GetValue<DateTime>());
            Query.SerializationCheck(value_DateTime);

            int intVal = 10;
            Value value_Int = new(intVal);
            Assert.Equal(typeof(int), value_Int.ValueType);
            Assert.Equal(intVal, value_Int.GetValue<int>());
            Query.SerializationCheck(value_Int);

            uint uintVal = 10U;
            Value value_Uint = new(uintVal);
            Assert.Equal(typeof(uint), value_Uint.ValueType);
            Assert.Equal(uintVal, value_Uint.GetValue<uint>());
            Query.SerializationCheck(value_Uint);

            sbyte sbyteVal = (sbyte)10;
            Value value_Sbyte = new(sbyteVal);
            Assert.Equal(typeof(sbyte), value_Sbyte.ValueType);
            Assert.Equal(sbyteVal, value_Sbyte.GetValue<sbyte>());
            Query.SerializationCheck(value_Sbyte);

            short shortVal = (short)10;
            Value value_Short = new(shortVal);
            Assert.Equal(typeof(short), value_Short.ValueType);
            Assert.Equal(shortVal, value_Short.GetValue<short>());
            Query.SerializationCheck(value_Short);

            ushort ushortVal = (ushort)10;
            Value value_Ushort = new(ushortVal);
            Assert.Equal(typeof(ushort), value_Ushort.ValueType);
            Assert.Equal(ushortVal, value_Ushort.GetValue<ushort>());
            Query.SerializationCheck(value_Ushort);

            ulong ulongVal = 10UL;
            Value value_Ulong = new(ulongVal);
            Assert.Equal(typeof(ulong), value_Ulong.ValueType);
            Assert.Equal(ulongVal, value_Ulong.GetValue<ulong>());
            Query.SerializationCheck(value_Ulong);

            decimal decimalVal = 10.5M;
            Value value_Decimal = new(decimalVal);
            Assert.Equal(typeof(decimal), value_Decimal.ValueType);
            Assert.Equal(decimalVal, value_Decimal.GetValue<decimal>());
            Query.SerializationCheck(value_Decimal);

            char charVal = 'a';
            Value value_Char = new(charVal);
            Assert.Equal(typeof(char), value_Char.ValueType);
            Assert.Equal(charVal, value_Char.GetValue<char>());
            Query.SerializationCheck(value_Char);

            DateTimeOffset dateTimeOffset_Val = DateTimeOffset.UtcNow;
            Value value_DateTimeOffset = new(dateTimeOffset_Val);
            Assert.Equal(typeof(DateTimeOffset), value_DateTimeOffset.ValueType);
            Assert.Equal(dateTimeOffset_Val, value_DateTimeOffset.GetValue<DateTimeOffset>());
            Query.SerializationCheck(value_DateTimeOffset);

            System.TimeSpan timeSpan_Val = System.TimeSpan.FromSeconds(10);
            Value value_TimeSpan = new(timeSpan_Val);
            Assert.Equal(typeof(System.TimeSpan), value_TimeSpan.ValueType);
            Assert.Equal(timeSpan_Val, value_TimeSpan.GetValue<System.TimeSpan>());
            Query.SerializationCheck(value_TimeSpan);

            bool boolVal = true;
            Value value_Bool = new(boolVal);
            Assert.Equal(typeof(bool), value_Bool.ValueType);
            Assert.Equal(boolVal, value_Bool.GetValue<bool>());
            Query.SerializationCheck(value_Bool);

            byte byteVal = (byte)10;
            Value value_Byte = new(byteVal);
            Assert.Equal(typeof(byte), value_Byte.ValueType);
            Assert.Equal(byteVal, value_Byte.GetValue<byte>());
            Query.SerializationCheck(value_Byte);

            float floatVal = 10.5F;
            Value value_Float = new(floatVal);
            Assert.Equal(typeof(float), value_Float.ValueType);
            Assert.Equal(floatVal, value_Float.GetValue<float>());
            Query.SerializationCheck(value_Float);

            long longVal = 10L;
            Value value_Long = new(longVal);
            Assert.Equal(typeof(long), value_Long.ValueType);
            Assert.Equal(longVal, value_Long.GetValue<long>());
            Query.SerializationCheck(value_Long);

            Guid guid_Val = Guid.NewGuid();
            Value value_Guid = new(guid_Val);
            Assert.Equal(typeof(Guid), value_Guid.ValueType);
            Assert.Equal(guid_Val, value_Guid.GetValue<Guid>());
            Query.SerializationCheck(value_Guid);

            string stringVal = "abc";
            Value value_String = new(stringVal);
            Assert.Equal(typeof(string), value_String.ValueType);
            Assert.Equal(stringVal, value_String.GetValue<string>());
            Query.SerializationCheck(value_String);

            // Type constructor
            Type type_Val = typeof(double);
            Value value_Type = new(type_Val);
            Assert.True(typeof(Type).IsAssignableFrom(value_Type.ValueType));
            Assert.Equal(type_Val, value_Type.GetValue<Type>());
            Query.SerializationCheck(value_Type);

            // ISerializableObject constructor
            Address address_Val = new("street", "city", "postal", Enums.CountryCode.PL);
            Value value_Serializable = new(address_Val);
            Assert.Equal(typeof(Address), value_Serializable.ValueType);
            Assert.Equal(address_Val, value_Serializable.GetValue<Address>());
            Query.SerializationCheck(value_Serializable);

            // JsonObject constructor
            JsonObject jsonObject_Val = [];
            Value value_JsonObject = new(jsonObject_Val);
            Query.SerializationCheck(value_JsonObject);

            // Copy constructor
            Value value_Copy = new(value_Double);
            Assert.Equal(typeof(double), value_Copy.ValueType);
            Assert.Equal(doubleVal, value_Copy.GetValue<double>());
            Query.SerializationCheck(value_Copy);
        }

        /// <summary>
        /// Tests the serialization, cloning, and type wrapping of all 18 supported array types.
        /// </summary>
        [Fact]
        public void Value_Arrays()
        {
            string[] strings = ["A", "B"];
            Value value_Strings = new(strings);
            Assert.Equal(typeof(string[]), value_Strings.ValueType);
            Assert.Equal(strings, value_Strings.GetValue<string[]>());
            Query.SerializationCheck(value_Strings);

            double[] doubles = [1.0, 2.0];
            Value value_Doubles = new(doubles);
            Assert.Equal(typeof(double[]), value_Doubles.ValueType);
            Assert.Equal(doubles, value_Doubles.GetValue<double[]>());
            Query.SerializationCheck(value_Doubles);

            long[] longs = [10L, 20L];
            Value value_Longs = new(longs);
            Assert.Equal(typeof(long[]), value_Longs.ValueType);
            Assert.Equal(longs, value_Longs.GetValue<long[]>());
            Query.SerializationCheck(value_Longs);

            Guid[] guids = [Guid.NewGuid(), Guid.NewGuid()];
            Value value_Guids = new(guids);
            Assert.Equal(typeof(Guid[]), value_Guids.ValueType);
            Assert.Equal(guids, value_Guids.GetValue<Guid[]>());
            Query.SerializationCheck(value_Guids);

            int[] ints = [1, 2];
            Value value_Ints = new(ints);
            Assert.Equal(typeof(int[]), value_Ints.ValueType);
            Assert.Equal(ints, value_Ints.GetValue<int[]>());
            Query.SerializationCheck(value_Ints);

            uint[] uints = [1U, 2U];
            Value value_Uints = new(uints);
            Assert.Equal(typeof(uint[]), value_Uints.ValueType);
            Assert.Equal(uints, value_Uints.GetValue<uint[]>());
            Query.SerializationCheck(value_Uints);

            sbyte[] sbytes = [(sbyte)1, (sbyte)2];
            Value value_Sbytes = new(sbytes);
            Assert.Equal(typeof(sbyte[]), value_Sbytes.ValueType);
            Assert.Equal(sbytes, value_Sbytes.GetValue<sbyte[]>());
            Query.SerializationCheck(value_Sbytes);

            short[] shorts = [(short)1, (short)2];
            Value value_Shorts = new(shorts);
            Assert.Equal(typeof(short[]), value_Shorts.ValueType);
            Assert.Equal(shorts, value_Shorts.GetValue<short[]>());
            Query.SerializationCheck(value_Shorts);

            ushort[] ushorts = [(ushort)1, (ushort)2];
            Value value_Ushorts = new(ushorts);
            Assert.Equal(typeof(ushort[]), value_Ushorts.ValueType);
            Assert.Equal(ushorts, value_Ushorts.GetValue<ushort[]>());
            Query.SerializationCheck(value_Ushorts);

            ulong[] ulongs = [10UL, 20UL];
            Value value_Ulongs = new(ulongs);
            Assert.Equal(typeof(ulong[]), value_Ulongs.ValueType);
            Assert.Equal(ulongs, value_Ulongs.GetValue<ulong[]>());
            Query.SerializationCheck(value_Ulongs);

            decimal[] decimals = [1.5M, 2.5M];
            Value value_Decimals = new(decimals);
            Assert.Equal(typeof(decimal[]), value_Decimals.ValueType);
            Assert.Equal(decimals, value_Decimals.GetValue<decimal[]>());
            Query.SerializationCheck(value_Decimals);

            char[] chars = ['a', 'b'];
            Value value_Chars = new(chars);
            Assert.Equal(typeof(char[]), value_Chars.ValueType);
            Assert.Equal(chars, value_Chars.GetValue<char[]>());
            Query.SerializationCheck(value_Chars);

            DateTimeOffset[] dateTimeOffsets = [DateTimeOffset.UtcNow, DateTimeOffset.UtcNow.AddMinutes(1)];
            Value value_DateTimeOffsets = new(dateTimeOffsets);
            Assert.Equal(typeof(DateTimeOffset[]), value_DateTimeOffsets.ValueType);
            Assert.Equal(dateTimeOffsets, value_DateTimeOffsets.GetValue<DateTimeOffset[]>());
            Query.SerializationCheck(value_DateTimeOffsets);

            System.TimeSpan[] timeSpans = [System.TimeSpan.FromSeconds(10), System.TimeSpan.FromSeconds(20)];
            Value value_TimeSpans = new(timeSpans);
            Assert.Equal(typeof(System.TimeSpan[]), value_TimeSpans.ValueType);
            Assert.Equal(timeSpans, value_TimeSpans.GetValue<System.TimeSpan[]>());
            Query.SerializationCheck(value_TimeSpans);

            byte[] bytes = [(byte)1, (byte)2];
            Value value_Bytes = new(bytes);
            Assert.Equal(typeof(byte[]), value_Bytes.ValueType);
            Assert.Equal(bytes, value_Bytes.GetValue<byte[]>());
            Query.SerializationCheck(value_Bytes);

            DateTime[] dateTimes = [DateTime.Now, DateTime.Now.AddMinutes(1)];
            Value value_DateTimes = new(dateTimes);
            Assert.Equal(typeof(DateTime[]), value_DateTimes.ValueType);
            Assert.Equal(dateTimes, value_DateTimes.GetValue<DateTime[]>());
            Query.SerializationCheck(value_DateTimes);

            float[] floats = [1.5F, 2.5F];
            Value value_Floats = new(floats);
            Assert.Equal(typeof(float[]), value_Floats.ValueType);
            Assert.Equal(floats, value_Floats.GetValue<float[]>());
            Query.SerializationCheck(value_Floats);

            bool[] bools = [true, false];
            Value value_Bools = new(bools);
            Assert.Equal(typeof(bool[]), value_Bools.ValueType);
            Assert.Equal(bools, value_Bools.GetValue<bool[]>());
            Query.SerializationCheck(value_Bools);
        }

        /// <summary>
        /// Tests type conversion, generic casting rules, invalid casting fallback, and explicit JSON object casting operators.
        /// </summary>
        [Fact]
        public void Value_Conversions()
        {
            Value value_Int = new(42);

            // Valid cast
            int int_Success = value_Int.GetValue<int>();
            Assert.Equal(42, int_Success);

            // Invalid cast to struct returns default value of struct
            double double_Failure = value_Int.GetValue<double>();
            Assert.Equal(0.0, double_Failure);

            // Invalid cast to class returns null
            string? string_Failure = value_Int.GetValue<string>();
            Assert.Null(string_Failure);

            // Explicit operator conversions to/from JsonObject
            JsonObject? jsonObject_Val = (JsonObject?)value_Int;
            Assert.NotNull(jsonObject_Val);

            // Round-trip through string to serialize correctly in system types
            string string_Json = jsonObject_Val.ToJsonString();
            JsonObject? jsonObject_Parsed = JsonNode.Parse(string_Json)?.AsObject();
            Assert.NotNull(jsonObject_Parsed);

            SerializableObject? serializableObject_Val = (SerializableObject?)jsonObject_Parsed;
            Assert.NotNull(serializableObject_Val);

            Value? value_Deserialized = serializableObject_Val as Value;
            Assert.NotNull(value_Deserialized);
            Assert.Equal(42, value_Deserialized.GetValue<int>());

            // ValueType check before and after serialization/deserialization
            // Scalar Int
            Value value_IntType = new(10);
            Assert.Equal(typeof(int), value_IntType.ValueType);
            string? string_JsonInt = Convert.ToSystem_String(value_IntType);
            Value? value_DeserializedInt = Convert.ToDiGi<Value>(string_JsonInt)?.FirstOrDefault();
            Assert.NotNull(value_DeserializedInt);
            Assert.Equal(typeof(int), value_DeserializedInt.ValueType);

            // Scalar SByte
            Value value_SbyteType = new((sbyte)10);
            Assert.Equal(typeof(sbyte), value_SbyteType.ValueType);
            string? string_JsonSbyte = Convert.ToSystem_String(value_SbyteType);
            Value? value_DeserializedSbyte = Convert.ToDiGi<Value>(string_JsonSbyte)?.FirstOrDefault();
            Assert.NotNull(value_DeserializedSbyte);
            Assert.Equal(typeof(sbyte), value_DeserializedSbyte.ValueType);

            // Array type
            Guid[] guids = [Guid.NewGuid(), Guid.NewGuid()];
            Value value_GuidsType = new(guids);
            Assert.Equal(typeof(Guid[]), value_GuidsType.ValueType);
            string? string_JsonGuids = Convert.ToSystem_String(value_GuidsType);
            Value? value_DeserializedGuids = Convert.ToDiGi<Value>(string_JsonGuids)?.FirstOrDefault();
            Assert.NotNull(value_DeserializedGuids);
            Assert.Equal(typeof(Guid[]), value_DeserializedGuids.ValueType);
        }
    }
}