using DiGi.GIS.WebAPI.Classes;
using DiGi.PostgreSQL.Table.Enums;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Reflection;
using System.Text.Json;

namespace DiGi.GIS.WebAPI.xUnit
{
    public partial class Facts
    {
        /// <summary>
        /// Pins DiGi-Organization/DiGi.GIS.WebAPI#59: <see cref="MultivalueAggregateRequestParameter.MaxGroups"/> is a [1, 1000]-bounded int
        /// defaulting to 1000, survives the System.Text.Json web-default round trip the host's input formatter uses, and is accepted in range /
        /// rejected out of range by the same DataAnnotations the [ApiController] model validation runs - the source of the 400 an out-of-range
        /// <c>maxgroups</c> body produces. No database is needed: the annotation is the contract, and it is exercised here directly.
        /// </summary>
        [Fact]
        public void MultivalueAggregateRequestParameter_MaxGroups_RangeDefaultRoundTrip()
        {
            // Present, and defaulting to the 1000 ceiling.
            MultivalueAggregateRequestParameter parameter = new();
            Assert.Equal(1000, parameter.MaxGroups);

            // Bounded [1, 1000] by the annotation itself (not inferred from behaviour).
            PropertyInfo? propertyInfo = typeof(MultivalueAggregateRequestParameter).GetProperty(nameof(MultivalueAggregateRequestParameter.MaxGroups));
            RangeAttribute? rangeAttribute = propertyInfo!.GetCustomAttribute<RangeAttribute>();
            Assert.Equal(1, rangeAttribute!.Minimum);
            Assert.Equal(1000, rangeAttribute.Maximum);

            // Wire shape: the host's input formatter (JsonSerializerOptions.Web) keeps the property and its value.
            MultivalueAggregateRequestParameter parameter_Source = new()
            {
                ColumnUniqueId = "floor_area",
                MultivalueAggregateFunction = MultivalueAggregateFunction.SplitValueDistribution,
                MaxGroups = 20,
            };
            string json = JsonSerializer.Serialize(parameter_Source, JsonSerializerOptions.Web);
            MultivalueAggregateRequestParameter? parameter_RoundTrip = JsonSerializer.Deserialize<MultivalueAggregateRequestParameter>(json, JsonSerializerOptions.Web);
            Assert.NotNull(parameter_RoundTrip);
            Assert.Equal(20, parameter_RoundTrip!.MaxGroups);

            // Accept in range, reject out of range - the same validator [ApiController] runs (the thing that yields the 400).
            MultivalueAggregateRequestParameter parameter_Validate = new();
            ValidationContext validationContext = new(parameter_Validate) { MemberName = nameof(MultivalueAggregateRequestParameter.MaxGroups) };

            List<ValidationResult> results_InRange = [];
            Assert.True(Validator.TryValidateProperty(20, validationContext, results_InRange));
            Assert.Empty(results_InRange);

            List<ValidationResult> results_TooHigh = [];
            Assert.False(Validator.TryValidateProperty(1001, validationContext, results_TooHigh));
            Assert.NotEmpty(results_TooHigh);

            List<ValidationResult> results_TooLow = [];
            Assert.False(Validator.TryValidateProperty(0, validationContext, results_TooLow));
            Assert.NotEmpty(results_TooLow);
        }
    }
}
