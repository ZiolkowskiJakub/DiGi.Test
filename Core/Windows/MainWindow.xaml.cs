﻿using System.IO;
using System.Text;
using System.Text.Json.Nodes;
using System.Windows;
using DiGi.Core.Classes;
using DiGi.Core.Parameter.Classes;
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


        private void BinaryReadWriteTest()
        {
            string directory = @"C:\Users\jakub\Nextcloud\Work\DigiProject\DiGi\";

            string filePath = System.IO.Path.Combine(directory, "Test.dgb");

            TestObject testObject_1 = new TestObject("BBB");
            string json = Convert.ToString(testObject_1);

            File.WriteAllText(System.IO.Path.Combine(directory, "Test.json"), json);

            using (var stream = File.Open(filePath, FileMode.Create))
            {
                using (BinaryWriter binaryWriter = new BinaryWriter(stream, Encoding.UTF8, false))
                {
                    binaryWriter.Write(json);
                    binaryWriter.Write(filePath);
                }
            }

            json = null;

            using (var stream = File.Open(filePath, FileMode.Open))
            {
                using (BinaryReader binaryReader = new BinaryReader(stream, Encoding.UTF8, false))
                {
                    json = binaryReader.ReadString();
                }
            }

            TestObject testObject_2 = Convert.ToDiGi<TestObject>(json)?.FirstOrDefault();

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

            FileInfo fileInfo = path.GetFileInfo();
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

        private void Button_Test1_Click(object sender, RoutedEventArgs e)
        {
            BinaryReadWriteTest();
        }

    }
}