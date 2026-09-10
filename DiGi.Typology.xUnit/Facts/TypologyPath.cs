using DiGi.Typology.Classes;

namespace DiGi.Typology.xUnit
{
    public partial class Facts
    {
        /// <summary>
        /// Tests <see cref="Typology.Classes.TypologyPath"/> construction, navigation, range extraction,
        /// concatenation, equality, ordering and serialization round-trip.
        /// <para>The serialization assertions pin the slim wire format: the computed members
        /// (Count, Index, Parent, ParentCount) must not be written, because a recursive Parent
        /// chain makes the JSON of a path quadratic in its depth.</para>
        /// </summary>
        [Fact]
        public void TypologyPath()
        {
            TypologyPath typologyPath = new([1, 2, 3]);

            Assert.Equal(3, typologyPath.Count);
            Assert.Equal(3, typologyPath.Index);
            Assert.Equal(2, typologyPath.ParentCount);
            Assert.Equal(1, typologyPath[0]);
            Assert.Equal("1.2.3", typologyPath.ToString());

            Assert.Equal(new TypologyPath([1, 2]), typologyPath.Parent);
            Assert.Equal(new TypologyPath([1]), typologyPath.GetParent(0));
            Assert.Equal(new TypologyPath([1, 2]), typologyPath.GetParent(1));
            Assert.Null(typologyPath.GetParent(2));

            List<TypologyPath> typologyPaths = typologyPath.GetTypologyPaths();
            Assert.Equal(2, typologyPaths.Count);

            TypologyPath typologyPath_Empty = new((IEnumerable<int>?)null);

            Assert.Equal(0, typologyPath_Empty.Count);
            Assert.Equal(-1, typologyPath_Empty.Index);
            Assert.Equal(0, typologyPath_Empty.ParentCount);
            Assert.Null(typologyPath_Empty.Parent);
            Assert.Empty(typologyPath_Empty.GetTypologyPaths());

            Assert.Equal(new TypologyPath([2, 3]), typologyPath.GetTypologyPath(1, 2));
            Assert.Equal(new TypologyPath([1]), typologyPath.GetTypologyPath(0, 1));
            Assert.Null(typologyPath.GetTypologyPath(1, 3));
            Assert.Null(typologyPath.GetTypologyPath(-1, 1));
            Assert.Null(typologyPath.GetTypologyPath(0, -1));

            Assert.Equal(new TypologyPath([1, 2, 3, 4]), typologyPath + new TypologyPath([4]));
            Assert.Equal(typologyPath, typologyPath + (TypologyPath?)null);
            Assert.Equal(typologyPath, (TypologyPath?)null + typologyPath);
            Assert.Null((TypologyPath?)null + (TypologyPath?)null);

            Assert.Equal(typologyPath, new TypologyPath([1, 2, 3]));
            Assert.Equal(typologyPath.GetHashCode(), new TypologyPath([1, 2, 3]).GetHashCode());
            Assert.NotEqual(typologyPath, new TypologyPath([1, 2, 4]));
            Assert.NotEqual(typologyPath, new TypologyPath([1, 2]));

            Assert.Equal(0, typologyPath.CompareTo(new TypologyPath([1, 2, 3])));
            Assert.True(new TypologyPath([1, 2]).CompareTo(typologyPath) < 0);
            Assert.True(new TypologyPath([1, 3]).CompareTo(typologyPath) > 0);
            Assert.True(typologyPath.CompareTo(null!) > 0);

            List<int>? values = (List<int>?)typologyPath;

            Assert.NotNull(values);
            Assert.Equal(3, values.Count);
            Assert.Equal(6, typologyPath.Sum());

            string? json = Core.Convert.ToSystem_String(typologyPath);

            Assert.NotNull(json);
            Assert.DoesNotContain("\"Parent\"", json);
            Assert.DoesNotContain("\"ParentCount\"", json);
            Assert.DoesNotContain("\"Count\"", json);
            Assert.DoesNotContain("\"Index\"", json);
            Assert.Contains("\"Values\"", json);

            TypologyPath? typologyPath_Temp = Core.Convert.ToDiGi<TypologyPath>(json)?.FirstOrDefault();

            Assert.NotNull(typologyPath_Temp);
            Assert.Equal(typologyPath, typologyPath_Temp);

            Core.xUnit.Query.SerializationCheck(typologyPath);
        }

        /// <summary>
        /// Tests that the == and != operators and IEquatable route to value equality rather than
        /// reference equality, so two distinct but value-equal paths compare equal.
        /// <para>Reproduction guard: without the operators, == compiled to reference equality and
        /// returned false for equal-but-distinct instances.</para>
        /// </summary>
        [Fact]
        public void TypologyPath_EqualityOperators()
        {
            TypologyPath typologyPath_1 = new([1, 2, 3]);
            TypologyPath typologyPath_2 = new([1, 2, 3]);
            TypologyPath typologyPath_3 = new([1, 2, 4]);

            Assert.True(typologyPath_1 == typologyPath_2);
            Assert.False(typologyPath_1 != typologyPath_2);
            Assert.True(typologyPath_1 != typologyPath_3);
            Assert.False(typologyPath_1 == typologyPath_3);

            Assert.True(typologyPath_1.Equals(typologyPath_2));
            Assert.False(typologyPath_1.Equals(typologyPath_3));
            Assert.False(typologyPath_1.Equals((TypologyPath?)null));

            Assert.True(EqualityComparer<TypologyPath>.Default.Equals(typologyPath_1, typologyPath_2));
            Assert.False(EqualityComparer<TypologyPath>.Default.Equals(typologyPath_1, typologyPath_3));

            TypologyPath? typologyPath_Null = null;

            Assert.True(typologyPath_Null == null);
            Assert.False(typologyPath_Null == typologyPath_1);
            Assert.True(typologyPath_Null != typologyPath_1);
            Assert.False(typologyPath_Null != null);
        }
    }
}
