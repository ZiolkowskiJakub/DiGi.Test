using DiGi.Core.Classes;
using DiGi.Core.Interfaces;
using DiGi.SQLite.Classes;
using DiGi.SQLite.Interfaces;
using DiGi.SQLite.Test.Classes;
using System.Text.Json.Nodes;
using System.Windows;

namespace DiGi.SQLite.Test
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

        private void Button_Test_1_Click(object sender, RoutedEventArgs e)
        {
            SQLiteModelTest();
        }

        private void SQLiteTest()
        {
            TestClass1 testClass1 = new TestClass1() { Parameter1 = "AAA" };
            TestClass2 testClass2_1 = new TestClass2() { Parameter1 = 10, TestClass1 = testClass1 };
            TestClass2 testClass2_2 = new TestClass2() { Parameter1 = 11, Parent = testClass2_1 };

            TestClass3 testClass3 = new TestClass3() { Parent = testClass2_2 };
            testClass3.TestClasses = new List<TestClass1>() { testClass1 , testClass1 };

            JsonObject jsonObject = testClass3.ToJsonObject();

            List<SQLiteProperty> sQLiteProperties_1 = SQLite.Query.SQLiteProperties<SQLiteDataValue>(jsonObject, true);

            List<SQLiteProperty> sQLiteProperties_2 = SQLite.Query.SQLiteProperties<SQLiteDataObject>(jsonObject, true);

            foreach(SQLiteProperty sQLiteProperty in sQLiteProperties_2)
            {
                SQLiteDataObject sQLiteDataObject =  sQLiteProperty.GetSQLiteData<SQLiteDataObject>();
                if(sQLiteDataObject == null)
                {
                    continue;
                }

                UniqueIdReference uniqueIdReference = sQLiteDataObject.UniqueIdReference;
            }

        }

        private void ReferenceTest()
        {
            List<TestClass3> testClass3s = new List<TestClass3>() { new TestClass3() , new TestClass3() , new TestClass3() };

            List<UniqueReference> uniqueReferences = new List<UniqueReference>();
            uniqueReferences.AddRange(testClass3s.ConvertAll(x => new GuidReference(x)));
            uniqueReferences.RemoveAt(0);

            uniqueReferences.Add(new UniqueIdReference(uniqueReferences[0].TypeReference.FullTypeName, Core.Query.UniqueId(testClass3s[0].Guid)));

            uniqueReferences.Remove(uniqueReferences[0]);

        }

        private void SQLiteModelTest()
        {
            TestClass1 testClass1 = new TestClass1() { Parameter1 = "AAA" };


            ISQLiteData sQLiteData = Create.SQLiteData(testClass1.ToJsonObject());
            UniqueIdReference uniqueIdReference =  sQLiteData.UniqueIdReference;


            TestClass2 testClass2_1 = new TestClass2() { Parameter1 = 10, TestClass1 = null };
            TestClass2 testClass2_2 = new TestClass2() { Parameter1 = 11, Parent = testClass2_1 };

            TestClass3 testClass3 = new TestClass3() { Parent = testClass2_2 };
            testClass3.TestClasses = new List<TestClass1>() { testClass1, testClass1 };

            List<ISerializableObject> serializableObjects = new List<ISerializableObject>()
            {
                //testClass1,
                testClass2_1,
                testClass2_2,
                //testClass3
            };

            SQLiteModel model = new SQLiteModel();
            model.AddRange(serializableObjects);


            //model.JsonNodes = serializableObjects.ConvertAll(x => (JsonNode) x.ToJsonObject());

            Convert.ToSQLite(model, @"C:\Users\jakub\Downloads\test");
        }
    }
}