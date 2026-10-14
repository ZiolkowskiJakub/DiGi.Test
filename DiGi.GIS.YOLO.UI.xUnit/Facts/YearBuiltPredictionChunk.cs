using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace DiGi.GIS.YOLO.UI.xUnit
{
    public partial class Facts
    {
        /// <summary>
        /// Verifies that YearBuiltPredictionChunk keeps the values it is given, survives its string form and the marker file round trip, clones identically and that its copy constructor copies every member (DiGi-Organization/DiGi.GIS.YOLO.UI#30).
        /// <para>A marker that does not read must come back as null, so a damaged marker costs the chunk a repeat rather than letting it be stepped over.</para>
        /// </summary>
        [Fact]
        public void YearBuiltPredictionChunk()
        {
            DateTimeOffset completed = new(2026, 10, 14, 21, 11, 5, TimeSpan.FromHours(2));

            Classes.YearBuiltPredictionChunk yearBuiltPredictionChunk = new(3, ["0207", "0208", "0209"], ["0207", "0209"], 22, new string('c', 64), 0.1, completed);

            void AssertMembers(Classes.YearBuiltPredictionChunk? yearBuiltPredictionChunk_Actual)
            {
                Assert.NotNull(yearBuiltPredictionChunk_Actual);
                Assert.Equal(3, yearBuiltPredictionChunk_Actual!.Index);
                Assert.Equal(["0207", "0208", "0209"], yearBuiltPredictionChunk_Actual.References!);
                Assert.Equal(["0207", "0209"], yearBuiltPredictionChunk_Actual.ScoredReferences!);
                Assert.Equal(22, yearBuiltPredictionChunk_Actual.ImageCount);
                Assert.Equal(new string('c', 64), yearBuiltPredictionChunk_Actual.ModelSHA256);
                Assert.Equal(0.1, yearBuiltPredictionChunk_Actual.Confidence);
                Assert.Equal(completed, yearBuiltPredictionChunk_Actual.Completed);
            }

            AssertMembers(yearBuiltPredictionChunk);

            string? json = Core.Convert.ToSystem_String(yearBuiltPredictionChunk);
            Assert.False(string.IsNullOrWhiteSpace(json));
            AssertMembers(Core.Convert.ToDiGi<Classes.YearBuiltPredictionChunk>(json)?.FirstOrDefault());

            //SerializationCheck never runs the copy constructor
            AssertMembers(new Classes.YearBuiltPredictionChunk(yearBuiltPredictionChunk));

            Core.xUnit.Query.SerializationCheck(yearBuiltPredictionChunk);

            //The optional members left unset
            Classes.YearBuiltPredictionChunk yearBuiltPredictionChunk_Empty = new(0, [], null, 0, null, 0.25, completed);
            Assert.Null(yearBuiltPredictionChunk_Empty.ModelSHA256);
            Assert.Null(yearBuiltPredictionChunk_Empty.ScoredReferences);
            Assert.Null(new Classes.YearBuiltPredictionChunk(yearBuiltPredictionChunk_Empty).ScoredReferences);
            Core.xUnit.Query.SerializationCheck(yearBuiltPredictionChunk_Empty);

            //The marker file round trip
            string? directory_Reports = Core.xUnit.Query.ReportsDirectory(Assembly.GetExecutingAssembly());
            Assert.False(string.IsNullOrWhiteSpace(directory_Reports));

            string directory = Path.Combine(directory_Reports!, nameof(YearBuiltPredictionChunk));
            if (Directory.Exists(directory))
            {
                Directory.Delete(directory, true);
            }

            string path = Path.Combine(directory, "000003" + Constants.FileName.PredictionChunkExtension);
            Assert.True(yearBuiltPredictionChunk.WriteYearBuiltPredictionChunk(path));
            Assert.False(File.Exists(path + ".tmp"));
            AssertMembers(Query.YearBuiltPredictionChunk(path));

            //Rewritten in place
            Assert.True(yearBuiltPredictionChunk_Empty.WriteYearBuiltPredictionChunk(path));
            Assert.Equal(0, Query.YearBuiltPredictionChunk(path)?.Index);

            //Damaged, empty or missing markers read as none
            File.WriteAllText(path, "{\"_type\": \"DiGi.GIS.YOLO.UI.Classes.YearBuiltPredictionChunk, Di");
            Assert.Null(Query.YearBuiltPredictionChunk(path));

            File.WriteAllText(path, string.Empty);
            Assert.Null(Query.YearBuiltPredictionChunk(path));

            Assert.Null(Query.YearBuiltPredictionChunk(Path.Combine(directory, "missing.json")));
            Assert.Null(Query.YearBuiltPredictionChunk(null));

            Assert.False(Modify.WriteYearBuiltPredictionChunk(null, path));
            Assert.False(yearBuiltPredictionChunk.WriteYearBuiltPredictionChunk(null));
        }

        /// <summary>
        /// Verifies that <see cref="Query.IsResumable(Classes.YearBuiltPredictionChunk?, IEnumerable{string}?, string?, double)"/> steps over a chunk only when its marker names the same buildings in the same order, the same weights and the same confidence (DiGi-Organization/DiGi.GIS.YOLO.UI#30).
        /// <para>The weights are the case that matters: <c>model.pt</c> is replaced in place, so a marker left by an earlier detector would otherwise stand in for the current one. The hash compares without regard to case, since a hex digest means the same in either.</para>
        /// </summary>
        [Fact]
        public void IsResumable()
        {
            string modelSHA256 = new('a', 64);

            Classes.YearBuiltPredictionChunk yearBuiltPredictionChunk = new(0, ["0207", "0208"], ["0207"], 11, modelSHA256, 0.1, DateTimeOffset.Now);

            Assert.True(yearBuiltPredictionChunk.IsResumable(["0207", "0208"], modelSHA256, 0.1));
            Assert.True(yearBuiltPredictionChunk.IsResumable(["0207", "0208"], modelSHA256.ToUpperInvariant(), 0.1));

            //Other weights, another threshold, other buildings, another order
            Assert.False(yearBuiltPredictionChunk.IsResumable(["0207", "0208"], new string('b', 64), 0.1));
            Assert.False(yearBuiltPredictionChunk.IsResumable(["0207", "0208"], modelSHA256, 0.25));
            Assert.False(yearBuiltPredictionChunk.IsResumable(["0207", "0208", "0209"], modelSHA256, 0.1));
            Assert.False(yearBuiltPredictionChunk.IsResumable(["0207"], modelSHA256, 0.1));
            Assert.False(yearBuiltPredictionChunk.IsResumable(["0208", "0207"], modelSHA256, 0.1));

            //Nothing to compare with
            Assert.False(Query.IsResumable(null, ["0207", "0208"], modelSHA256, 0.1));
            Assert.False(yearBuiltPredictionChunk.IsResumable(null, modelSHA256, 0.1));
            Assert.False(yearBuiltPredictionChunk.IsResumable(["0207", "0208"], null, 0.1));

            //A marker that recorded no weights, or no scored buildings, is not one a run can rely on
            Assert.False(new Classes.YearBuiltPredictionChunk(0, ["0207", "0208"], ["0207"], 11, null, 0.1, DateTimeOffset.Now).IsResumable(["0207", "0208"], modelSHA256, 0.1));
            Assert.False(new Classes.YearBuiltPredictionChunk(0, ["0207", "0208"], null, 11, modelSHA256, 0.1, DateTimeOffset.Now).IsResumable(["0207", "0208"], modelSHA256, 0.1));
        }
    }
}
