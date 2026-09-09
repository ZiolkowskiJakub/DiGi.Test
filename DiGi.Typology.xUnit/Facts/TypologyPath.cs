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
            Typology.Classes.TypologyPath typologyPath = new([1, 2, 3]);

            Assert.Equal(3, typologyPath.Count);
            Assert.Equal(3, typologyPath.Index);
            Assert.Equal(2, typologyPath.ParentCount);
            Assert.Equal(1, typologyPath[0]);
            Assert.Equal("1.2.3", typologyPath.ToString());

            Assert.Equal(new Typology.Classes.TypologyPath([1, 2]), typologyPath.Parent);
            Assert.Equal(new Typology.Classes.TypologyPath([1]), typologyPath.GetParent(0));
            Assert.Equal(new Typology.Classes.TypologyPath([1, 2]), typologyPath.GetParent(1));
            Assert.Null(typologyPath.GetParent(2));

            List<Typology.Classes.TypologyPath> typologyPaths = typologyPath.GetTypologyPaths();
            Assert.Equal(2, typologyPaths.Count);

            Typology.Classes.TypologyPath typologyPath_Empty = new((IEnumerable<int>?)null);

            Assert.Equal(0, typologyPath_Empty.Count);
            Assert.Equal(-1, typologyPath_Empty.Index);
            Assert.Equal(0, typologyPath_Empty.ParentCount);
            Assert.Null(typologyPath_Empty.Parent);
            Assert.Empty(typologyPath_Empty.GetTypologyPaths());

            Assert.Equal(new Typology.Classes.TypologyPath([2, 3]), typologyPath.GetTypologyPath(1, 2));
            Assert.Equal(new Typology.Classes.TypologyPath([1]), typologyPath.GetTypologyPath(0, 1));
            Assert.Null(typologyPath.GetTypologyPath(1, 3));
            Assert.Null(typologyPath.GetTypologyPath(-1, 1));
            Assert.Null(typologyPath.GetTypologyPath(0, -1));

            Assert.Equal(new Typology.Classes.TypologyPath([1, 2, 3, 4]), typologyPath + new Typology.Classes.TypologyPath([4]));
            Assert.Equal(typologyPath, typologyPath + (Typology.Classes.TypologyPath?)null);
            Assert.Equal(typologyPath, (Typology.Classes.TypologyPath?)null + typologyPath);
            Assert.Null((Typology.Classes.TypologyPath?)null + (Typology.Classes.TypologyPath?)null);

            Assert.Equal(typologyPath, new Typology.Classes.TypologyPath([1, 2, 3]));
            Assert.Equal(typologyPath.GetHashCode(), new Typology.Classes.TypologyPath([1, 2, 3]).GetHashCode());
            Assert.NotEqual(typologyPath, new Typology.Classes.TypologyPath([1, 2, 4]));
            Assert.NotEqual(typologyPath, new Typology.Classes.TypologyPath([1, 2]));

            Assert.Equal(0, typologyPath.CompareTo(new Typology.Classes.TypologyPath([1, 2, 3])));
            Assert.True(new Typology.Classes.TypologyPath([1, 2]).CompareTo(typologyPath) < 0);
            Assert.True(new Typology.Classes.TypologyPath([1, 3]).CompareTo(typologyPath) > 0);
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

            Typology.Classes.TypologyPath? typologyPath_Temp = Core.Convert.ToDiGi<Typology.Classes.TypologyPath>(json)?.FirstOrDefault();

            Assert.NotNull(typologyPath_Temp);
            Assert.Equal(typologyPath, typologyPath_Temp);

            Core.xUnit.Query.SerializationCheck(typologyPath);
        }
    }
}
