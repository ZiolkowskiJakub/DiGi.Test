using System.Windows;
using DiGi.Geometry.Planar;
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

        private static void OffsetTest()
        {
            //Rectangle2D rectangle2D = new Rectangle2D(new Point2D(0, 0), 5, 5);
            //Polygon2D polygon2D = new Polygon2D(rectangle2D.GetPoints());

            Polygon2D polygon2D = new Polygon2D(new List<Point2D>()
            {
                new Point2D(0, 0) ,
                new Point2D(10, 0),
                new Point2D(10, 10),
                new Point2D(11, 10),
                new Point2D(11, 0),
                new Point2D(11, 0),
                new Point2D(20, 0),
                new Point2D(20, 11),
                new Point2D(0, 11),
            });


            List<Polygon2D> polygon2Ds_Offset = Query.Offset(polygon2D, -1);
        }

        private void Button_Test1_Click(object sender, RoutedEventArgs e)
        {
            OffsetTest();

            //Rectangle2D rectangle2D = new Rectangle2D(new Point2D(8, 8), 5, 5);

            //Point2D point2D = rectangle2D.GetCentroid();

            //Polyline2D polyline2D = new Polyline2D(new List<Point2D>()
            //{
            //    new Point2D(0, 0) ,
            //    new Point2D(10, 0),
            //    new Point2D(10, 10),
            //    new Point2D(5, 10),
            //     new Point2D(5, -10),
            //});


            //List<Point2D> point2Ds = Query.LongestPath(new ISegmentable2D[] { rectangle2D, polyline2D }, new Point2D(5, -10));

            //List<Segment2D> segment2Ds = rectangle2D.AdjacentSegments(polyline2D);

            //int count = segment2Ds.Count;

            //Polyline2D polyline2D = new Polyline2D(new List<Point2D>()
            //{
            //    new Point2D(0, 0) ,
            //    new Point2D(10, 0),
            //    new Point2D(10, 10),
            //    new Point2D(5, 10),
            //     new Point2D(5, -10),
            //});

            //List<Segment2D> segment2Ds = new List<Segment2D>()
            //{
            //    new Segment2D(0,0, 10, 0),
            //    new Segment2D(0,0, 10, 0),
            //    new Segment2D(10,10, 10, 0),
            //    new Segment2D(10,10, 0, 10),
            //};

            //int count = 8;

            //double factor = (2 * Math.PI) / count;

            //double angle = 0;
            //List<Vector2D> vector2Ds = new List<Vector2D>();
            //for(int i=0; i < count; i++)
            //{
            //    vector2Ds.Add(Create.Vector2D(angle));
            //    angle += factor;
            //}

            //Vector2D vector2D = Create.Vector2D(Math.PI);

            //List<Polyline2D> polyline2Ds = Create.Polyline2Ds(segment2Ds, new Point2D(0, 0));

            //List<Polygon2D> polygon2Ds = Planar.Create.Polygon2Ds(polyline2D);

            //bool selfIntersect = Planar.Query.SelfIntersect(polygon2Ds[0]);
            //Orientation orientation = polygon2Ds[0].Orientation();
            //polygon2Ds[0].Orient(Orientation.CounterClockwise);

            //selfIntersect = Planar.Query.SelfIntersect(polyline2D);


            //Vector2D widthDirection = rectangle2D.WidthDirection;
            //Vector2D heightDirection = rectangle2D.HeightDirection;

            //List<Point2D> point2Ds_1 = rectangle2D.GetPoints();
            //rectangle2D.Inverse();

            //List<Point2D> point2Ds_2 = rectangle2D.GetPoints();

            //double s = 0;

            //GeometryCollection2D geometryCollection2D = new GeometryCollection2D();
            //geometryCollection2D.Add(new Segment2D(0,0, 0, 1));
            //geometryCollection2D.Add(new Point2D(0, 0));

            //GeometryCollection2D geometryCollection2D_Temp = new GeometryCollection2D();
            //geometryCollection2D_Temp.Add(new Segment2D(0, 0, 1, 3));
            //geometryCollection2D.Add(geometryCollection2D_Temp);

            //geometryCollection2D.Transform(Transform2D.GetTranslation(1, 1));


            //string json = DiGi.Core.Convert.ToString((ISerializableObject)geometryCollection2D);

            //GeometryCollection2D? geometryCollection2D_Converted = DiGi.Core.Convert.ToDiGi<GeometryCollection2D>(json)?.FirstOrDefault();



            //double s = 1;

            //IntersectionResult2D intersectionResult2D = Geometry.Planar.Create.IntersectionResult2D(new Line2D(new Point2D(0, 0), new Vector2D(0, 1)), new Line2D(new Point2D(1, 0), new Vector2D(0, 1)));

            //Vector2D vector2D_1 = new Vector2D(0, 1);

            //Vector2D vector2D_2 = new Vector2D(0, -6);


            //List<KeyValuePair<double, double>> values = new List<KeyValuePair<double, double>>();
            //values.Add(new KeyValuePair<double, double>(1, 2));
            //values.Add(new KeyValuePair<double, double>(3, 5));

            //JsonValue jsonValue_Temp = JsonValue.Create(values);

            //List<KeyValuePair<double, double>> values_Temp = jsonValue_Temp.GetValue<List<KeyValuePair<double, double>>>();



            //Point2D point2D = new Point2D(1, 1);
            //Vector2D vector2D_3 = new Vector2D(0, 1);
            //Transform2D transform2D = Transform2D.GetRotation(Math.PI);
            ////transform2D = Transform2D.GetTranslation(1, 1);

            //Segment2D segment2D = new Segment2D(new Point2D(0, 0), new Point2D(0, 1));
            //segment2D.Transform(transform2D);

            //point2D.Transform(transform2D);
            //vector2D_3.Transform(transform2D);

            //CoordinateSystem2D coordinateSystem2D = new CoordinateSystem2D();




            //List<int[]> indexes = new List<int[]>() { new int[] {1, 2, 3 }, new int[] { 2, 2, 3 } };

            //JsonValue jsonValue = JsonValue.Create(indexes);

            //List<int[]> indexes_Temp = jsonValue.GetValue<List<int[]>>();

            //bool collinear = (new Line2D(new Point2D(0, 0), vector2D_1)).Collinear(new Line2D(new Point2D(2, 2), vector2D_2));


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