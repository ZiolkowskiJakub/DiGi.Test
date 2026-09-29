using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace DiGi.GIS.YOLO.UI.xUnit
{
    public partial class Facts
    {
        /// <summary>
        /// Verifies a dataset building: it keeps what it is given, survives its string form and its manifest row, clones identically, and the manifest reader skips a torn last line.
        /// <para>The manifest is appended to as a build goes, so a stopped run can leave a half-written last row; the reader has to drop it rather than fail the whole manifest, and the building on it is then rebuilt. A reference named twice keeps its first row.</para>
        /// </summary>
        [Fact]
        public void DatasetReference()
        {
            Classes.DatasetReference datasetReference = new("REF_A_1", 73485, DiGi.YOLO.Enums.Category.Test, 1987, Enums.LegacySource.Both);

            Assert.Equal("REF_A_1", datasetReference.Reference);
            Assert.Equal(73485, datasetReference.CountyId);
            Assert.Equal(DiGi.YOLO.Enums.Category.Test, datasetReference.Category);
            Assert.Equal(1987, datasetReference.Label);
            Assert.Equal(Enums.LegacySource.Both, datasetReference.LegacySource);
            Assert.True(datasetReference.Legacy);

            string? json = Core.Convert.ToSystem_String(datasetReference);
            Assert.False(string.IsNullOrWhiteSpace(json));

            Classes.DatasetReference? datasetReference_Json = Core.Convert.ToDiGi<Classes.DatasetReference>(json)?.FirstOrDefault();
            Assert.NotNull(datasetReference_Json);
            Assert.Equal(datasetReference.Category, datasetReference_Json!.Category);
            Assert.Equal(datasetReference.LegacySource, datasetReference_Json.LegacySource);

            Classes.DatasetReference datasetReference_Copy = new(datasetReference);
            Assert.Equal(datasetReference.Reference, datasetReference_Copy.Reference);
            Assert.Equal(datasetReference.CountyId, datasetReference_Copy.CountyId);
            Assert.Equal(datasetReference.Category, datasetReference_Copy.Category);
            Assert.Equal(datasetReference.Label, datasetReference_Copy.Label);
            Assert.Equal(datasetReference.LegacySource, datasetReference_Copy.LegacySource);

            Core.xUnit.Query.SerializationCheck(datasetReference);

            // The manifest row, and back.
            string? line = Convert.ToTSV(datasetReference);
            Assert.Equal("REF_A_1\t73485\tTest\t1987\ttrue\tboth", line);

            Classes.DatasetReference? datasetReference_Line = Convert.ToDiGi_DatasetReference(line);
            Assert.NotNull(datasetReference_Line);
            Assert.Equal(datasetReference.Reference, datasetReference_Line!.Reference);
            Assert.Equal(datasetReference.CountyId, datasetReference_Line.CountyId);
            Assert.Equal(datasetReference.Category, datasetReference_Line.Category);
            Assert.Equal(datasetReference.Label, datasetReference_Line.Label);
            Assert.Equal(datasetReference.LegacySource, datasetReference_Line.LegacySource);

            // A clean building writes an empty source; an unknown one says so.
            Assert.Equal("B\t1\tTrain\t2000\tfalse\t", Convert.ToTSV(new Classes.DatasetReference("B", 1, DiGi.YOLO.Enums.Category.Train, 2000, Enums.LegacySource.None)));
            Assert.Equal(Enums.LegacySource.None, Convert.ToDiGi_DatasetReference("B\t1\tTrain\t2000\tfalse\t")!.LegacySource);
            Assert.Equal(Enums.LegacySource.Unknown, Convert.ToDiGi_DatasetReference("B\t1\tTest\t2000\ttrue\tunknown")!.LegacySource);

            Assert.Null(Convert.ToDiGi_DatasetReference(Constants.Header.DatasetReferences));
            Assert.Null(Convert.ToDiGi_DatasetReference("B\t1\tTrain"));
            Assert.Null(Convert.ToDiGi_DatasetReference("B\t1\tNowhere\t2000\tfalse\t"));
            Assert.Null(Convert.ToDiGi_DatasetReference("B\t1\tTrain\t2000\tfalse\tsomething"));

            // The reader: a duplicate keeps its first row, a torn last line is dropped.
            string? directory_Reports = Core.xUnit.Query.ReportsDirectory(Assembly.GetExecutingAssembly());
            Assert.False(string.IsNullOrWhiteSpace(directory_Reports));

            string path = Path.Combine(directory_Reports!, "dataset_references_DatasetReference.tsv");
            File.WriteAllLines(path, [Constants.Header.DatasetReferences, line!, "B\t1\tTrain\t2000\tfalse\t", "REF_A_1\t2\tTrain\t1999\tfalse\t", "C\t1\tTr"]);

            List<Classes.DatasetReference>? datasetReferences = Query.DatasetReferences(path);
            Assert.NotNull(datasetReferences);
            Assert.Equal(2, datasetReferences!.Count);
            Assert.Equal(73485, datasetReferences[0].CountyId);
            Assert.Equal("B", datasetReferences[1].Reference);

            // A file that is not a manifest is not read as an empty one.
            File.WriteAllLines(path, ["Reference\tSomething", "B"]);
            Assert.Null(Query.DatasetReferences(path));
            Assert.Null(Query.DatasetReferences(Path.Combine(directory_Reports!, "missing_dataset_references.tsv")));
        }
    }
}
