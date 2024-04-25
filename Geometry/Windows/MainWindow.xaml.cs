﻿using System.Windows;
using DiGi.Geometry.Planar.Classes;
using DiGi.Geometry.Spatial;
using DiGi.Geometry.Spatial.Classes;
using DiGi.Geometry.Spatial.Interfaces;

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


            List<Polygon2D> polygon2Ds_Offset = Planar.Query.Offset(polygon2D, -1);
        }

        private static void VolatileObjectTest()
        {
            Polygon2D polygon2D = new Polygon2D(new List<Point2D>()
            {
                new Point2D(0, 0) ,
                new Point2D(10, 0),
                new Point2D(10, 10),
                new Point2D(0, 10)
            });

            PolygonalFace2D polygonalFace2D = Planar.Create.PolygonalFace2D(polygon2D);

            PolygonalFace3D polygonalFace3D = new PolygonalFace3D(Constans.Plane.WorldZ, polygonalFace2D);

            string json = DiGi.Core.Convert.ToString(polygonalFace2D);

            VolatilePolygonalFace3D volatilePolygonalFace3D = new VolatilePolygonalFace3D(polygonalFace3D);

            string json_Volatile = DiGi.Core.Convert.ToString(polygonalFace2D);

            bool equals = json == json_Volatile;
        }

        private static void IntersectionTest()
        {
            Polygon2D polygon2D = new Polygon2D(new List<Point2D>()
            {
                new Point2D(0, 0) ,
                new Point2D(10, 0),
                new Point2D(10, 10),
                new Point2D(0, 10)
            });

            PolygonalFace3D polygonalFace3D = new PolygonalFace3D(Constans.Plane.WorldZ, Planar.Create.PolygonalFace2D(polygon2D));

            PlanarIntersectionResult planarIntersectionResult = Create.PlanarIntersectionResult(polygonalFace3D, (ILinear3D)new Segment3D(new Point3D(-1, -1, 0), new Point3D(10, 10, 0)));
        }

        private static void PlanarIntersectionTest_1() 
        {
            DateTime dateTime = DateTime.Now;

            Polygon2D polygon2D = new Polygon2D(new List<Point2D>()
            {
                new Point2D(0, 0) ,
                new Point2D(10, 0),
                new Point2D(10, 10),
                new Point2D(0, 10)
            });

            PolygonalFace3D polygonalFace3D = new PolygonalFace3D(Constans.Plane.WorldZ, Planar.Create.PolygonalFace2D(polygon2D));

            Vector3D vector3D = new Vector3D(0, 0, 10);

            Polyhedron polyhedron = Create.Polyhedron(polygonalFace3D, vector3D);

            VolatilePolyhedron volatilePolyhedron = polyhedron;

            for (int i = 0; i < 10000; i++)
            {
                PlanarIntersectionResult planarIntersectionResult = Create.PlanarIntersectionResult(new Plane(new Point3D(0, 0, 1), Constans.Vector3D.WorldZ), volatilePolyhedron);
            }

            double seconds = (DateTime.Now - dateTime).TotalSeconds;

            MessageBox.Show(seconds.ToString());

        }

        private static void PlanarIntersectionTest_2()
        {
            DateTime dateTime = DateTime.Now;

            Polygon2D polygon2D = null;

            polygon2D = new Polygon2D(new List<Point2D>()
            {
                new Point2D(0, 0) ,
                new Point2D(10, 0),
                new Point2D(10, 10),
                new Point2D(0, 10)
            });

            PolygonalFace3D polygonalFace3D_1 = new PolygonalFace3D(Constans.Plane.WorldZ, Planar.Create.PolygonalFace2D(polygon2D));

            polygon2D = new Polygon2D(new List<Point2D>()
            {
                new Point2D(5, 5) ,
                new Point2D(20, 5),
                new Point2D(20, 20),
                new Point2D(5, 20)
            });

            Polygon3D polygon3D = Create.Polygon3D(new List<Point3D>() 
            {
                new Point3D(2, 5, -1),
                new Point3D(2, 20, -1),
                new Point3D(2, 20, 1),
                new Point3D(2, 5, 1)
            });

            PolygonalFace3D polygonalFace3D_2 = Create.PolygonalFace3D(polygon3D);

             polygon3D = Create.Polygon3D(new List<Point3D>()
            {
                new Point3D(2, 5, -1),
                new Point3D(2, 5, 1),
                new Point3D(10, 5, 1),
                new Point3D(10, 5, -1)
            });

            PolygonalFace3D polygonalFace3D_3 = Create.PolygonalFace3D(polygon3D);

            for (int i = 0; i < 10000; i++)
            {
                //PlanarIntersectionResult planarIntersectionResult = Spatial.Create.PlanarIntersectionResult((VolatilePolygonalFace3D)polygonalFace3D_1, new VolatilePolygonalFace3D[] { polygonalFace3D_2, polygonalFace3D_3 });

                List<VolatilePolygonalFace3D> volatilePolygonalFace3Ds = Query.Split(polygonalFace3D_1, new VolatilePolygonalFace3D[] { polygonalFace3D_2, polygonalFace3D_3 });

                //List<IGeometry3D> geometry3Ds = planarIntersectionResult.GetGeometry3Ds<IGeometry3D>();
            }

            double seconds = (DateTime.Now - dateTime).TotalSeconds;

            MessageBox.Show(seconds.ToString());

        }

        private static void PolyhedronTest() 
        {

            DateTime dateTime = DateTime.Now;

            Polygon2D polygon2D = new Polygon2D(new List<Point2D>()
                {
                    new Point2D(0, 0) ,
                    new Point2D(10, 0),
                    new Point2D(10, 10),
                    new Point2D(0, 10)
                });

            PolygonalFace3D polygonalFace3D = new PolygonalFace3D(Constans.Plane.WorldZ, Planar.Create.PolygonalFace2D(polygon2D));

            Vector3D vector3D = new Vector3D(0, 0, 10);

            Polyhedron polyhedron = Create.Polyhedron(polygonalFace3D, vector3D);

            VolatilePolyhedron volatilePolyhedron = polyhedron;

            List<bool> @bools = new List<bool>();
            for (int i = 0; i < 10000; i++)
            {
                IntersectionResult3D intersectionResult3D = Create.IntersectionResult3D(volatilePolyhedron, new Segment3D(-1, 5, 5, 11, 5, 5));

                PlanarIntersectionResult planarIntersectionResult = Create.PlanarIntersectionResult(new Plane(new Point3D(0, 0, 5), Constans.Vector3D.WorldZ), volatilePolyhedron);

                //string text = DiGi.Core.Convert.ToString(polyhedron);

                //Polyhedron polyhedron_1 = DiGi.Core.Convert.ToDiGi<Polyhedron>(text)?.FirstOrDefault();


                //@bools.Add(text == DiGi.Core.Convert.ToString(polyhedron));
            }

            double seconds = (DateTime.Now - dateTime).TotalSeconds;

            MessageBox.Show(seconds.ToString());
        }

        private static void RangeTest()
        {
            BoundingBox3D boundingBox3D = new BoundingBox3D(new Point3D(0, 0, 0), new Point3D(10, 10, 10));

            Point3D point3D = new Point3D(5, 5, 5);

            bool inRange = boundingBox3D.InRange(point3D);
            bool on = boundingBox3D.On(point3D);
            bool inside = boundingBox3D.Inside(point3D);
        }

        private static void RandomPolygon2DTest()
        {
            Polygon2D polygon2D = Planar.Random.Create.Polygon2D(new BoundingBox2D(new Point2D(0, 0), new Point2D(10, 10)), 4);
        }

        private static void RandomPolyhedronTest()
        {
            int seed = 1;

            Polyhedron polyhedron = Spatial.Random.Create.Polyhedron(new BoundingBox3D(new Point3D(0, 0, 0), new Point3D(10, 10, 10)), 4, seed);
        }

        public static void ConversionTest()
        {
            Plane? plane = DiGi.Core.Convert.ToDiGi<Plane>(System.IO.File.ReadAllText(@"Z:\DiGi\Plane.txt"))?.FirstOrDefault();
            Line3D? line3D = DiGi.Core.Convert.ToDiGi<Line3D>(System.IO.File.ReadAllText(@"Z:\DiGi\Line3D.txt"))?.FirstOrDefault();

            Line2D line2D = plane.Convert(line3D);

            Line3D line3D_Temp = plane.Convert(line2D);

            bool valid = DiGi.Core.Convert.ToString(line3D) == DiGi.Core.Convert.ToString(line3D_Temp);
        }

        private void Button_Test1_Click(object sender, RoutedEventArgs e)
        {

            ConversionTest();
            //RandomPolyhedronTest();
            //PlanarIntersectionTest_2();
            //PolyhedronTest();

            //VolatileObjectTest();

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