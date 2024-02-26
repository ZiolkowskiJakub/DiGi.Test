using System.Windows;
using DiGi.Core;
using DiGi.Geometry.Planar.Classes;

namespace DiGi.Geometry.Test
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
            IntersectionResult2D intersectionResult2D = Geometry.Planar.Create.IntersectionResult2D(new Line2D(new Point2D(0, 0), new Vector2D(0, 1)), new Line2D(new Point2D(1, 0), new Vector2D(0, 1)));


            //Vector2D vector2D = new Vector2D(10, 11);

            //string json = DiGi.Core.Convert.ToString(new List<Vector2D> { vector2D, vector2D });

            //List<Vector2D> vector2Ds = DiGi.Core.Convert.ToDiGi<Vector2D>(json);

            //List<Point2D> point2Ds = new List<Point2D>() { new Point2D(0, 0), new Point2D(0, 5), new Point2D(5, 5), new Point2D(5, 0) };

            //Point2D point2D = new Point2D(0, 5);

            //double parameter = Planar.Query.Parameter(point2Ds, point2D, out Point2D point2D_Closest, out double distance);

            //Point2D point2D_Temp = Planar.Query.Point(point2Ds, parameter);

            //JsonNode jsonNode = JsonNode.Parse(json);


            //ISerializableObject serializableObject = Create.SerializableObject<ISerializableObject>(jsonNode.AsObject());
        }
    }
}