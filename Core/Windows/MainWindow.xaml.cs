﻿using System.IO;
using System.Text;
using System.Text.Json.Nodes;
using System.Windows;
using DiGi.Core.Classes;
using DiGi.Core.Interfaces;
using DiGi.Core.Parameter.Classes;
using DiGi.Core.Relation.Classes;
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
            BytesTest();
        }

        private void BytesTest()
        {
            BytesObject bytesObject_1 = new BytesObject(new byte[] { 72, 101, 108, 108, 111 });

            JsonObject jsonObject = bytesObject_1.ToJson();

            string json_1 = Convert.ToString(bytesObject_1);

            BytesObject bytesObject_2 = Convert.ToDiGi<BytesObject>(json_1)?.FirstOrDefault();

            bool result = Convert.ToString(bytesObject_2) == json_1;
        }

        private void ReferenceTest()
        {
            GuidReference guidReference = new GuidReference(new TypeReference(typeof(TestObject)), Guid.NewGuid());
            string source = "AAA";

            InstanceRelatedExternalReference instanceRelatedExternalReference = new InstanceRelatedExternalReference(source, guidReference);

            string reference = instanceRelatedExternalReference.ToString();

            if(Query.TryParse(guidReference.ToString(), out IReference reference_Temp))
            {

            }

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

            List<TestObject> testObjects = uniqueObjectRelationCluster.GetValues<TestObject>(oneToOneBidirectionalRelation);
            uniqueObjectRelationCluster.Remove(testObject_1);
        }

        private void BinaryReadWriteTest()
        {
            string directory = @"C:\Users\jakub\Nextcloud\Work\DigiProject\DiGi\";

            string filePath = System.IO.Path.Combine(directory, "Test.dgb");

            TestObject testObject_1 = new TestObject("BBB");
            string json = Convert.ToString(testObject_1);

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
            string json_1 = Convert.ToString(testObject_1, new System.Text.Json.JsonSerializerOptions() { WriteIndented = true });

            TestObject testObject_2 = Convert.ToDiGi<TestObject>(json_1)?.FirstOrDefault();
            string json_2 = Convert.ToString(testObject_2, new System.Text.Json.JsonSerializerOptions() { WriteIndented = true });

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

            string json_1 = Convert.ToString(enumObject_1, new System.Text.Json.JsonSerializerOptions() { WriteIndented = true });

            EnumObject enumObject_2 = Convert.ToDiGi<EnumObject>(json_1)?.FirstOrDefault();
            string json_2 = Convert.ToString(enumObject_2, new System.Text.Json.JsonSerializerOptions() { WriteIndented = true });

            bool similar = json_1 == json_2;
        }

        private void ObjectTest()
        {
            TestObject testObject_5 = new TestObject("CC", 10, 12);
            TestObject testObject_6 = testObject_5.Clone<TestObject>();
            string json = Convert.ToString(testObject_5, new System.Text.Json.JsonSerializerOptions() { WriteIndented = true });
            


            //TestObject testObject = new TestObject("AAA");

            //JsonValue? jsonValue = JsonValue.Create(testObject);

            //JsonObject jsonObject = Convert.ToJson(testObject);

            //TestObject testObject_3 = Create.SerializableObject<TestObject>(jsonObject);

            TestObject testObject_2 = new TestObject("BBB");
            json = Convert.ToString(testObject_2);

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

            json = Convert.ToString(simpleParameterDefinition);

            SimpleParameterDefinition simpleParameterDefinition_Temp = Convert.ToDiGi<SimpleParameterDefinition>(json)?.FirstOrDefault();


            EnumParameterDefinition enumParameterDefinition = new EnumParameterDefinition(Enums.TestParameterDefinition.Test);
            json = Convert.ToString(enumParameterDefinition);

            EnumParameterDefinition enumParameterDefinition_Temp = Convert.ToDiGi<EnumParameterDefinition>(json)?.FirstOrDefault();
        }

        public void AssociatedTypesTest()
        {
            string json;
            ParameterValue parameterValue = new DoubleParameterValue();

            json = Convert.ToString(parameterValue);

            ParameterValue parameterValue_Temp = Convert.ToDiGi<ParameterValue>(json)?.FirstOrDefault();



            AssociatedTypes associatedTypes = new AssociatedTypes(typeof(Core.Classes.Path));

            json = Convert.ToString(associatedTypes);

            AssociatedTypes associatedTypes_Temp = Convert.ToDiGi<AssociatedTypes>(json)?.FirstOrDefault();

            ExternalParameterDefinition externalParameterDefinition = Parameter.Create.ExternalParameterDefinition(Guid.NewGuid(), "Test", "Test description", Parameter.Enums.ParameterType.Double, typeof(Color), nullable: false);

            json = Convert.ToString(externalParameterDefinition);

            ExternalParameterDefinition externalParameterDefinition_Temp = Convert.ToDiGi<ExternalParameterDefinition>(json)?.FirstOrDefault();
        }

        public void ParametrizedObjectTest()
        {
            string json = null;

            ParametrizedObject parametrizedObject = new ParametrizedObject();
            parametrizedObject.SetValue("Test", "Some Text", new SetValueSettings() { CheckAccessType = false });

            ExternalParameterDefinition externalParameterDefinition = Parameter.Create.ExternalParameterDefinition(Guid.NewGuid(), "Test 2", "Test description", Parameter.Enums.ParameterType.Double, typeof(ParametrizedObject), nullable: false);
            parametrizedObject.SetValue(externalParameterDefinition, 20);

            json = Convert.ToString(parametrizedObject);

            ParametrizedObject parametrizedObject_Temp = Convert.ToDiGi<ParametrizedObject>(json)?.FirstOrDefault();

            bool result = json == Convert.ToString(parametrizedObject_Temp);

        }

        private void ParameterTest_2()
        {
            string json_1 = Convert.ToString(Parameter.Create.Parameter("Test", "Some Text"));

            Parameter.Classes.Parameter parameter = Convert.ToDiGi<Parameter.Classes.Parameter>(json_1)?.FirstOrDefault();

            string json_2= Convert.ToString(parameter);

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

            Convert.ToFileInfo((ISerializableObject)serializableObjectCollection, path_JSON);

            using (IO.File.Classes.File file_1 = new IO.File.Classes.File(path_ZIP))
            {
                file_1.AddRange(serializableObjectCollection);
                file_1.Save();
            }

            using (IO.File.Classes.File file_2 = new IO.File.Classes.File(path_ZIP))
            {
                file_2.Open();

                SerializableObjectCollection serializableObjectCollection_3 = file_2.Value;
            }
        }

        private void CollectionTest()
        {
            TestObject testObject_1 = new TestObject("BBB");

            TestObject testObject_2 = new TestObject("AAA");


            SerializableObjectCollection serializableObjectCollection_1 = new SerializableObjectCollection(new ISerializableObject[] { testObject_1, testObject_2 });
            string json_1 = Convert.ToString((ISerializableObject)serializableObjectCollection_1);

            SerializableObjectCollection serializableObjectCollection_2 = Convert.ToDiGi<SerializableObjectCollection>(json_1)?.FirstOrDefault();
            string json_2 = Convert.ToString((ISerializableObject)serializableObjectCollection_2);

            bool result = json_1 == json_2;


        }

    }
}