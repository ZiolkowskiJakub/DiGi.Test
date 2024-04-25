using System.Text.Json.Nodes;
using System.Windows;
using DiGi.Core.Classes;
using DiGi.Core.Parameters;
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

            TestObject testObject_3 = Core.Create.SerializableObject<TestObject>(jsonObject);

            TestObject testObject_4 = testObject_3.Clone<TestObject>();

            //string a = "";

            //int hashCode = a.GetHashCode();

            //Modify.FromJsonObject(testObject_2, jsonObject);
        }

        public void PathTest()
        {
            Path path = @"Z:\DiGi\Line3Da.txt";

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



            AssociatedTypes associatedTypes = new AssociatedTypes(typeof(Path));

            json = Convert.ToString(associatedTypes);

            AssociatedTypes associatedTypes_Temp = Convert.ToDiGi<AssociatedTypes>(json)?.FirstOrDefault();

            ExternalParameterDefinition externalParameterDefinition = Parameters.Create.ExternalParameterDefinition(Guid.NewGuid(), "Test", "Test description", Parameters.Enums.ParameterType.Double, typeof(Color), nullable: false);

            json = Convert.ToString(externalParameterDefinition);

            ExternalParameterDefinition externalParameterDefinition_Temp = Convert.ToDiGi<ExternalParameterDefinition>(json)?.FirstOrDefault();

        }

        private void Button_Test1_Click(object sender, RoutedEventArgs e)
        {
            AssociatedTypesTest();
        }
    }
}