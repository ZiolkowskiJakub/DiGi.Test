using DiGi.Core;
using DiGi.Core.Classes;
using DiGi.Core.Interfaces;
using DiGi.SQLite.Classes;
using DiGi.SQLite.Test.Classes;
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
            SQLiteWrapperTest();
        }

        private void SQLiteWrapperTest()
        {
            UniqueIdReference uniqueIdReference = new UniqueIdReference(new TypeReference(typeof(IEnumerable<TestClass1>)), "BBB");
            GuidReference guidReference = new GuidReference("AAA", Guid.NewGuid());

            string string_1 = uniqueIdReference.ToString();
            string string_2 = guidReference.ToString();


            string path = @"C:\Users\jakub\Downloads\SQLite\test.sqlite";

            TestClass1 testClass1 = new TestClass1() { Parameter1 = "AAA" };
            TestClass2 testClass2_1 = new TestClass2() { Parameter1 = 10, TestClass1 = testClass1 };
            TestClass2 testClass2_2 = new TestClass2() { Parameter1 = 11, Parent = testClass2_1 };

            TestClass3 testClass3 = new TestClass3() { Parent = testClass2_2 };
            testClass3.TestClasses = new List<TestClass1>() { testClass1 , testClass1 };

            List<ISerializableObject> serializableObjects = new List<ISerializableObject>
            {
                testClass1,
                testClass2_1,
                testClass2_2,
                testClass3,
            };

            Dictionary<UniqueReference, ISerializableObject> dictionary = new Dictionary<UniqueReference, ISerializableObject>();
            foreach(ISerializableObject serializableObject in serializableObjects)
            {
                dictionary[Create.UniqueReference(serializableObject)] = serializableObject;
            }


            using (SQLiteWrapper sQLiteWrapper = new SQLiteWrapper())
            {
                sQLiteWrapper.ConnectionString = Query.ConnectionString(path);

                foreach(ISerializableObject serializableObject in dictionary.Values)
                {
                    sQLiteWrapper.Add(serializableObject);
                }
                
                sQLiteWrapper.Write(dictionary.Keys);

                sQLiteWrapper.Clear();

                List<ISerializableObject> serializableObjects_Temp = sQLiteWrapper.Read<ISerializableObject>(dictionary.Keys);
                if (serializableObjects_Temp != null && serializableObjects_Temp.Count != 0)
                {
                    foreach(ISerializableObject serializableObject_Temp in serializableObjects_Temp)
                    {
                        ISerializableObject serializableObject = dictionary[Create.UniqueReference(serializableObject_Temp)];

                        if(Core.Convert.ToString(serializableObject) != Core.Convert.ToString(serializableObject_Temp))
                        {
                            throw new Exception();
                        }
                    }
                }
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
    }
}