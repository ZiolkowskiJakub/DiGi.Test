using System.IO;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
using System.Web;
using System.Windows;
using DiGi.Core.Classes;
using DiGi.Core.Interfaces;
using DiGi.Core.IO.File.Classes;
using DiGi.Core.IO.Interfaces;
using DiGi.Core.Parameter.Classes;
using DiGi.Core.Relation.Classes;
using DiGi.Core.Relation.Enums;
using DiGi.Core.Relation.Interfaces;
using DiGi.Core.Test.Classes;

namespace DiGi.Core.Test
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void Button_Test1_Click(object sender, RoutedEventArgs e)
        {
            ReferenceTest();
        }

        private void LowerLimitTest()
        {
            SortedDictionary<DateTime, int> sortedDictionary = new SortedDictionary<DateTime, int>();
            sortedDictionary.Add(new DateTime(2018, 1, 10), -1);
            sortedDictionary.Add(new DateTime(2021, 1, 10), 0);
            sortedDictionary.Add(new DateTime(2023, 1, 10), 1);
            sortedDictionary.Add(new DateTime(2025, 1, 10), 2);

            int value;
            bool result;

            result = Query.TryGetLowerValue(sortedDictionary, new DateTime(2017, 1, 1), out value, false, false);

            result = Query.TryGetLowerValue(sortedDictionary, new DateTime(2019, 1, 1), out value);

            result = Query.TryGetLowerValue(sortedDictionary, new DateTime(2026, 1, 1), out value, false, false);

            result = Query.TryGetLowerValue(sortedDictionary, new DateTime(2024, 1, 1), out value);
        }

        private void ObjectPathTest()
        {
            ObjectPath objectPath = new ObjectPath(new List<string>() { "AAA", "BBB", "CCC" });
            string value = objectPath.ToString();

            ObjectPath objectPath_2 = new ObjectPath(new List<string>() { "AAA", "BBB", "CCC" });


            bool equals = objectPath_2.Equals(objectPath);


            string value_2 = objectPath.ToJsonObject().ToString();

            //            string csvContent = @"""Name"",""Comment""
            //""John Doe"",""He said, """"Hello!"""" to everyone.""
            //""Jane Doe"",""She replied, """"Hi there!""""""";



            string input = "\"John Doe\",\"He said, \"\"Hello!\"\" to everyone.\",\"Another field\"";
            List<string> values = Query.QuotedStrings(input);
            CategoryPath categoryPath_1 = new CategoryPath(values);

            input = categoryPath_1.ToString();

            CategoryPath categoryPath_2 = Create.CategoryPath(input);

            equals = categoryPath_2.Equals(categoryPath_1);
        }

        private void TableTest()
        {
            string path = @"C:\Users\jakub\Downloads\DiGi Test\4.2.3.portfolio";

            IO.Table.Classes.Table table = IO.DelimitedData.Create.Table(path, IO.DelimitedData.Enums.DelimitedDataSeparator.Tab);
        }

        private void NaNSerializationTest()
        {
            NaNTestObject naNTestObject_1 = new NaNTestObject();

            string json = naNTestObject_1.ToSystem_String();

            NaNTestObject naNTestObject_2 = Convert.ToDiGi<NaNTestObject>(json)?.FirstOrDefault();
        }

        private void StorageFileTest()
        {

            string reference = "DiGi.GIS.Classes.OrtoDatas,DiGi.GIS::7094b4255ada4bc69b047987ed1c0e9c";

            Query.TryParse(reference, out UniqueReference uniqueReference);

            string encode = IO.File.Query.Encode(uniqueReference);

            UniqueReference uniqueReference_Temp = IO.File.Query.Decode(encode);

            bool equals = uniqueReference.Equals(uniqueReference_Temp);
            equals = uniqueReference == uniqueReference_Temp;

            string reference_Encode = HttpUtility.UrlEncode(reference);
            string reference_Decode = HttpUtility.UrlDecode(reference);

            byte[] bytes = HttpUtility.UrlEncodeToBytes(reference);
            string reference_Temp = HttpUtility.UrlDecode(bytes, Encoding.UTF8);

            using (StorageFile storageFile = new StorageFile(@"C:\Users\jakub\Downloads\GIS Test\3262_GML.odf"))
            {
                int count = storageFile.Count;

                ISerializableObject serializableObject = storageFile.GetValue(count - 1);
            }
        }

        private void RelativePathTest()
        {
            string relativePath = IO.Query.RelativePath(@"C:\Users\jakub", @"C:\Users\jakub\Downloads\GIS Test\Test.txt");

            string relativePath_2 = System.IO.Path.GetRelativePath(@"C:\Users\jakub\", @"C:\Users\jakub\Downloads\GIS Test\Test.txt");

            bool bool_1 = IO.Query.IsPathFullyQualified(relativePath_2);

            string absolutePath_1 = IO.Query.AbsolutePath(@"C:\Users\jakub", relativePath);

            //string text = System.IO.File.ReadAllText(absolutePath);

            string relativePath_3 = IO.Query.RelativePath(@"C:\Users\jakub", @"C:\Users\jakub\Downloads\GIS Test\Test.txt");

            string relativePath_4 = IO.Query.RelativePath(@"C:\Users\jakub\Downloads\GIS Test", @"C:\Users\jakub\Test.txt");

            string absolutePath_2 = IO.Query.AbsolutePath(@"C:\Users\jakub\Downloads\GIS Test", relativePath_4);
        }

        static string GetAbsolutePath(string basePath, string relativePath)
        {
            return System.IO.Path.GetFullPath(System.IO.Path.Combine(basePath, relativePath));
        }

        private void RangeTest()
        {
            Range<int> range = new Range<int>(0, 10);

            List<int> values = range.ToSystem(1);
        }

        private void ValueFileTest()
        {
            BytesObject bytesObject_1 = new BytesObject(new byte[] { 72, 101, 108, 108, 111 });

            string path = @"C:\Users\jakub\Downloads\GIS Test\teststoragefile.dgs";


            //using (ValueFile valueFile = new ValueFile(path))
            //{
            //    valueFile.Value = bytesObject_1;
            //    valueFile.Save();
            //}

            //BytesObject bytesObject_2 = null;

            //using (ValueFile valueFile = new ValueFile(path))
            //{
            //    valueFile.Open();
            //    bytesObject_2 = valueFile.Value as BytesObject;
            //}

            //bool result = Convert.ToString(bytesObject_1) == Convert.ToString(bytesObject_2);

            using (ValuesFile valuesFile = new ValuesFile(path))
            {
                valuesFile.Values = new List<ISerializableObject>() { bytesObject_1, bytesObject_1 };
                valuesFile.SetMetadata(new TestMetadata("AAA"));
                valuesFile.Save();
            }

            BytesObject bytesObject_3 = null;

            using (ValuesFile valueFile = new ValuesFile(path))
            {
                valueFile.Open();
                IEnumerable<ISerializableObject> serializableObjects= valueFile.Values;

                IMetadata metadata = valueFile.GetMetadata<TestMetadata>();
            }

            UniqueReference uniqueReference = null;

            path = @"C:\Users\jakub\Downloads\GIS Test\teststoragefile_2.dgs";
            using (StorageFile storageFile = new StorageFile(path))
            {
                storageFile.Open();
                uniqueReference = storageFile.AddValue(bytesObject_1);
                storageFile.Save();
            }


            using (StorageFile storageFile = new StorageFile(path))
            {
                storageFile.Open();
                BytesObject bytesObject_Temp = storageFile.GetValue<BytesObject>(uniqueReference);
            }
        }

        private void BytesTest()
        {
            BytesObject bytesObject_1 = new BytesObject(new byte[] { 72, 101, 108, 108, 111 });

            JsonObject jsonObject = bytesObject_1.ToJson();

            string json_1 = Convert.ToSystem_String(bytesObject_1);

            BytesObject bytesObject_2 = Convert.ToDiGi<BytesObject>(json_1)?.FirstOrDefault();

            bool result = Convert.ToSystem_String(bytesObject_2) == json_1;
        }

        private void ReferenceTest()
        {
            Guid guid = Guid.NewGuid();

            GuidReference guidReference_1 = new GuidReference(new TypeReference(typeof(TestObject)), guid);

            GuidReference guidReference_2 = new GuidReference(new TypeReference(typeof(TestObject)), guid);

            List<bool> results = new List<bool>();

            results.Add(guidReference_1 == guidReference_2);
            results.Add(guidReference_1 == (IUniqueReference)guidReference_2);
            results.Add((IUniqueReference)guidReference_1 == guidReference_2);
            results.Add((dynamic)guidReference_1 == (dynamic)guidReference_2);
            results.Add(((IUniqueReference)guidReference_1).Equals((IUniqueReference)guidReference_2));
            results.Add((IUniqueReference)guidReference_1 == (IUniqueReference)guidReference_2);

            //GuidReference guidReference = new GuidReference(new TypeReference(typeof(TestObject)), Guid.NewGuid());
            //string source = "AAA";

            //InstanceRelatedExternalReference instanceRelatedExternalReference = new InstanceRelatedExternalReference(source, guidReference);

            //string reference = instanceRelatedExternalReference.ToString();

            //if(Query.TryParse(guidReference.ToString(), out IReference reference_Temp))
            //{

            //}

        }

        private void ThinnesRatioTest()
        {
            double area = 2;
            double perimeter = 10;

            double thinnessRatio = 4 * Math.Sqrt(area) / perimeter;



            //DiGi.Core.IO.Wrapper.Classes.

            //TestObject testObject_1 = new TestObject("CC", 10, 12);
            //TestObject testObject_2 = new TestObject("CC", 100, 12);

            //WrapperNodeCluster wrapperNodeCluster = new WrapperNodeCluster();
            //IWrapperReference wrapperReference_1 = wrapperNodeCluster.Add(testObject_1);
            //IWrapperReference wrapperReference_2 = wrapperNodeCluster.Add(testObject_2);

            //wrapperNodeCluster.Wrap();

        }

        private void JsonValueTest()
        {
            int value_1;
            double value_2;
            object value_3;

            JsonValue jsonValue_1 = JsonValue.Create(2.0);
            if (!jsonValue_1.TryGetValue(out value_1))
            {
                jsonValue_1.TryGetValue(out value_2);
            }

            if (!jsonValue_1.TryGetValue(out value_3))
            {

            }

            JsonValue jsonValue_2 = JsonValue.Create(2);
            if (!jsonValue_2.TryGetValue(out value_1))
            {
                jsonValue_2.TryGetValue(out value_2);
            }

            if (!jsonValue_2.TryGetValue(out value_3))
            {

            }
        }

        private void ClusterTest()
        {
            TestObject testObject_1 = new TestObject("AAA");
            TestObject testObject_2 = new TestObject("BBB");

            OneToOneBidirectionalRelation oneToOneBidirectionalRelation = new OneToOneBidirectionalRelation(testObject_1, testObject_2);

            UniqueObjectRelationCluster<IUniqueObject, IRelation> uniqueObjectRelationCluster = new UniqueObjectRelationCluster<IUniqueObject, IRelation>();
            uniqueObjectRelationCluster.Add(testObject_1);
            uniqueObjectRelationCluster.Add(testObject_2);

            uniqueObjectRelationCluster.AddRelation(oneToOneBidirectionalRelation);

            List<IRelation> relations = uniqueObjectRelationCluster.GetRelations<IRelation>(testObject_1);

            List<TestObject> testObjects = uniqueObjectRelationCluster.GetValues<TestObject>(oneToOneBidirectionalRelation, RelationSide.Undefined);
            uniqueObjectRelationCluster.Remove(testObject_1);
        }

        private void BinaryReadWriteTest()
        {
            string directory = @"C:\Users\jakub\Nextcloud\Work\DigiProject\DiGi\";

            string filePath = System.IO.Path.Combine(directory, "Test.dgb");

            TestObject testObject_1 = new TestObject("BBB");
            string json = Convert.ToSystem_String(testObject_1);

            System.IO.File.WriteAllText(System.IO.Path.Combine(directory, "Test.json"), json);

            using (var stream = System.IO.File.Open(filePath, FileMode.Create))
            {
                using (BinaryWriter binaryWriter = new BinaryWriter(stream, Encoding.UTF8, false))
                {
                    binaryWriter.Write(json);
                    binaryWriter.Write(filePath);
                }
            }

            json = null;

            using (var stream = System.IO.File.Open(filePath, FileMode.Open))
            {
                using (BinaryReader binaryReader = new BinaryReader(stream, Encoding.UTF8, false))
                {
                    json = binaryReader.ReadString();
                }
            }

            TestObject testObject_2 = Convert.ToDiGi<TestObject>(json)?.FirstOrDefault();

        }

        private void NullableObjectTest()
        {
            TestObject testObject_1 = new TestObject("CC", 10, 12);
            testObject_1.TestEnum = Enums.TestEnum.Test1;
            string json_1 = Convert.ToSystem_String(testObject_1, new System.Text.Json.JsonSerializerOptions() { WriteIndented = true });

            TestObject testObject_2 = Convert.ToDiGi<TestObject>(json_1)?.FirstOrDefault();
            string json_2 = Convert.ToSystem_String(testObject_2, new System.Text.Json.JsonSerializerOptions() { WriteIndented = true });

            bool similar = json_1 == json_2;
        }

        private void EnumTest()
        {
            EnumObject enumObject_1 = new EnumObject() 
            {
                TestEnum = Enums.TestEnum.Test1, 
                TestEnums_1 = new List<Enums.TestEnum?>() { Enums.TestEnum.Test1 },
                TestEnums_2 = new HashSet<Enums.TestEnum?>() { Enums.TestEnum.Test2 },
                TestEnums_3 = new HashSet<Enums.TestEnum>() { Enums.TestEnum.Test1, Enums.TestEnum.Test2 },
            };

            string json_1 = Convert.ToSystem_String(enumObject_1, new System.Text.Json.JsonSerializerOptions() { WriteIndented = true });

            EnumObject enumObject_2 = Convert.ToDiGi<EnumObject>(json_1)?.FirstOrDefault();
            string json_2 = Convert.ToSystem_String(enumObject_2, new System.Text.Json.JsonSerializerOptions() { WriteIndented = true });

            bool similar = json_1 == json_2;
        }

        private void ObjectTest()
        {
            TestObject testObject_5 = new TestObject("CC", 10, 12);
            TestObject testObject_6 = testObject_5.Clone<TestObject>();
            string json = Convert.ToSystem_String(testObject_5, new System.Text.Json.JsonSerializerOptions() { WriteIndented = true });
            


            //TestObject testObject = new TestObject("AAA");

            //JsonValue? jsonValue = JsonValue.Create(testObject);

            //JsonObject jsonObject = Convert.ToJson(testObject);

            //TestObject testObject_3 = Create.SerializableObject<TestObject>(jsonObject);

            TestObject testObject_2 = new TestObject("BBB");
            json = Convert.ToSystem_String(testObject_2);

            JsonObject jsonObject = JsonNode.Parse(json).AsObject();

            TestObject testObject_3 = Create.SerializableObject<TestObject>(jsonObject);

            TestObject testObject_4 = testObject_3.Clone<TestObject>();

            //string a = "";

            //int hashCode = a.GetHashCode();

            //Modify.FromJsonObject(testObject_2, jsonObject);
        }

        public void PathTest()
        {
            Core.Classes.Path path = @"Z:\DiGi\Line3Da.txt";

            //Path path = @"Z:\DiGi";

            string extension = path.Extension;

            bool valid = path.IsValid();

            System.IO.FileInfo fileInfo = path.GetFileInfo();
        }

        public void ParameterTest()
        {
            string json = null;

            SimpleParameterDefinition simpleParameterDefinition = new SimpleParameterDefinition("Test");

            json = Convert.ToSystem_String(simpleParameterDefinition);

            SimpleParameterDefinition simpleParameterDefinition_Temp = Convert.ToDiGi<SimpleParameterDefinition>(json)?.FirstOrDefault();


            EnumParameterDefinition enumParameterDefinition = new EnumParameterDefinition(Enums.TestParameterDefinition.Test);
            json = Convert.ToSystem_String(enumParameterDefinition);

            EnumParameterDefinition enumParameterDefinition_Temp = Convert.ToDiGi<EnumParameterDefinition>(json)?.FirstOrDefault();
        }

        public void AssociatedTypesTest()
        {
            string json;
            ParameterValue parameterValue = new DoubleParameterValue();

            json = Convert.ToSystem_String(parameterValue);

            ParameterValue parameterValue_Temp = Convert.ToDiGi<ParameterValue>(json)?.FirstOrDefault();



            AssociatedTypes associatedTypes = new AssociatedTypes(typeof(Core.Classes.Path));

            json = Convert.ToSystem_String(associatedTypes);

            AssociatedTypes associatedTypes_Temp = Convert.ToDiGi<AssociatedTypes>(json)?.FirstOrDefault();

            ExternalParameterDefinition externalParameterDefinition = Parameter.Create.ExternalParameterDefinition(Guid.NewGuid(), "Test", "Test description", Parameter.Enums.ParameterType.Double, typeof(Color), nullable: false);

            json = Convert.ToSystem_String(externalParameterDefinition);

            ExternalParameterDefinition externalParameterDefinition_Temp = Convert.ToDiGi<ExternalParameterDefinition>(json)?.FirstOrDefault();
        }

        public void ParametrizedObjectTest()
        {
            string json = null;

            ParametrizedObject parametrizedObject = new ParametrizedObject();
            parametrizedObject.SetValue("Test", "Some Text", new SetValueSettings() { CheckAccessType = false });

            ExternalParameterDefinition externalParameterDefinition = Parameter.Create.ExternalParameterDefinition(Guid.NewGuid(), "Test 2", "Test description", Parameter.Enums.ParameterType.Double, typeof(ParametrizedObject), nullable: false);
            parametrizedObject.SetValue(externalParameterDefinition, 20);

            json = Convert.ToSystem_String(parametrizedObject);

            ParametrizedObject parametrizedObject_Temp = Convert.ToDiGi<ParametrizedObject>(json)?.FirstOrDefault();

            bool result = json == Convert.ToSystem_String(parametrizedObject_Temp);

        }

        private void ParameterTest_2()
        {
            string json_1 = Convert.ToSystem_String(Parameter.Create.Parameter("Test", "Some Text"));

            Parameter.Classes.Parameter parameter = Convert.ToDiGi<Parameter.Classes.Parameter>(json_1)?.FirstOrDefault();

            string json_2= Convert.ToSystem_String(parameter);

            bool result = json_1 == json_2;
        }

        private void CategoryTest()
        {
            Category category = new Category("AAA");
            Category subCategory = category.Add("BBB");

            Category category_Temp = category.Clone<Category>();

            bool equals = category.Equals(category_Temp);
        }

        private void EscapeTest()
        {
            string text = "aaa\n aaa";

            text = "<>:\"|\\?*";

            text = ",";

            string escape = System.Text.RegularExpressions.Regex.Escape(text);
            string unescape = System.Text.RegularExpressions.Regex.Unescape(escape);
        }

        private void NameTest()
        {
            string name = nameof(Enums.TestParameterDefinition.Test);
        }

        private void TagTest()
        {
            Tag tag = new Tag(new Tag(10.1));

            JsonObject jsonObject = tag.ToJsonObject();

            tag = Convert.ToDiGi<Tag>(jsonObject.ToString()).FirstOrDefault();


        }

        private void FileTest()
        {

            string path_ZIP = @"C:\Users\jakub\Downloads\FileTest.zip";
            string path_JSON = @"C:\Users\jakub\Downloads\FileTest.json";

            SerializableObjectCollection serializableObjectCollection = new SerializableObjectCollection();
            for (int i = 0; i < 10000; i++)
            {
                serializableObjectCollection.Add(new TestObject(i.ToString()));
            }

            Convert.ToSystem_FileInfo((ISerializableObject)serializableObjectCollection, path_JSON);

            //using (IO.File.Classes.File file_1 = new IO.File.Classes.File(path_ZIP))
            //{
            //    file_1.AddRange(serializableObjectCollection);
            //    file_1.Save();
            //}

            //using (IO.File.Classes.File file_2 = new IO.File.Classes.File(path_ZIP))
            //{
            //    file_2.Open();

            //    SerializableObjectCollection serializableObjectCollection_3 = file_2.Value;
            //}
        }

        private void CollectionTest()
        {
            TestObject testObject_1 = new TestObject("BBB");

            TestObject testObject_2 = new TestObject("AAA");


            SerializableObjectCollection serializableObjectCollection_1 = new SerializableObjectCollection(new ISerializableObject[] { testObject_1, testObject_2 });
            string json_1 = Convert.ToSystem_String((ISerializableObject)serializableObjectCollection_1);

            SerializableObjectCollection serializableObjectCollection_2 = Convert.ToDiGi<SerializableObjectCollection>(json_1)?.FirstOrDefault();
            string json_2 = Convert.ToSystem_String((ISerializableObject)serializableObjectCollection_2);

            bool result = json_1 == json_2;


        }

    }
}