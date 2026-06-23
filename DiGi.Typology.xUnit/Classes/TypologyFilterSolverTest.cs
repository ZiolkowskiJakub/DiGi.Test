using DiGi.Typology.Classes;
using DiGi.Typology.Interfaces;

namespace DiGi.Typology.xUnit.Classes
{
    /// <summary>
    /// A concrete typology filter solver implementation used for unit testing.
    /// </summary>
    public class TypologyFilterSolverTest : TypologyFilterSolver<TypologyFilterTest, UniqueObjectTest>
    {
        protected override TypologyItem? GetTypologyItem(TypologyFilterTest? typologyFilter, ITypologyFilterRuleData? typologyFilterRuleData)
        {
            if (typologyFilter is null || typologyFilterRuleData is null)
            {
                return null;
            }

            TypologyItem result = new()
            {
                Name = $"{typologyFilter?.Value ?? string.Empty} {typologyFilterRuleData}",
            };

            return result;
        }

        protected override object? GetValue(TypologyFilterTest? typologyFilter, UniqueObjectTest? uniqueObject)
        {
            if (typologyFilter?.Value is not string name || uniqueObject is null)
            {
                return null;
            }

            return uniqueObject.GetValue(name);
        }
    }
}