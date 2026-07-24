using DiGi.WebAPI.Classes;
using System;
using System.Text.Json.Nodes;

namespace DiGi.WebAPI.xUnit
{
    public partial class Facts
    {
        /// <summary>
        /// Tests that <see cref="DiGi.WebAPI.Classes.PostOptions"/> initializes with expected default property values.
        /// </summary>
        [Fact]
        public void PostOptions_Defaults()
        {
            DiGi.WebAPI.Classes.PostOptions postOptions = new();

            Assert.Equal(TimeSpan.FromSeconds(20), postOptions.Delay);
            Assert.True(postOptions.RequestResult);
            Assert.Equal(3, postOptions.RetryCount);
            Assert.Equal(TimeSpan.FromSeconds(2), postOptions.RetryDelay);
        }

        /// <summary>
        /// Tests that the copy constructor of <see cref="DiGi.WebAPI.Classes.PostOptions"/> copies all properties correctly.
        /// </summary>
        [Fact]
        public void PostOptions_CopyConstructor()
        {
            DiGi.WebAPI.Classes.PostOptions postOptions_Source = new()
            {
                Delay = TimeSpan.FromSeconds(10),
                RequestResult = false,
                RetryCount = 5,
                RetryDelay = TimeSpan.FromSeconds(1),
            };

            DiGi.WebAPI.Classes.PostOptions postOptions_Copy = new(postOptions_Source);

            Assert.Equal(postOptions_Source.Delay, postOptions_Copy.Delay);
            Assert.Equal(postOptions_Source.RequestResult, postOptions_Copy.RequestResult);
            Assert.Equal(postOptions_Source.RetryCount, postOptions_Copy.RetryCount);
            Assert.Equal(postOptions_Source.RetryDelay, postOptions_Copy.RetryDelay);
        }

        /// <summary>
        /// Tests that <see cref="DiGi.WebAPI.Classes.PostOptions"/> can be converted to and from a JSON object.
        /// </summary>
        [Fact]
        public void PostOptions_JsonObjectConstructor()
        {
            DiGi.WebAPI.Classes.PostOptions postOptions_Original = new()
            {
                Delay = TimeSpan.FromSeconds(15),
                RequestResult = false,
                RetryCount = 2,
                RetryDelay = TimeSpan.FromSeconds(3),
            };

            JsonObject? jsonObject = postOptions_Original.ToJsonObject();
            Assert.NotNull(jsonObject);

            DiGi.WebAPI.Classes.PostOptions postOptions_Deserialized = new(jsonObject);

            Assert.Equal(postOptions_Original.Delay, postOptions_Deserialized.Delay);
            Assert.Equal(postOptions_Original.RequestResult, postOptions_Deserialized.RequestResult);
            Assert.Equal(postOptions_Original.RetryCount, postOptions_Deserialized.RetryCount);
            Assert.Equal(postOptions_Original.RetryDelay, postOptions_Deserialized.RetryDelay);
        }
    }
}
