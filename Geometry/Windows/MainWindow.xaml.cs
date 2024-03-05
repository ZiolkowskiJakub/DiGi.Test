﻿using System.Text.Json.Nodes;
using System.Windows;
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

            Vector2D vector2D_1 = new Vector2D(0, 1);

            Vector2D vector2D_2 = new Vector2D(0, -6);


            List<KeyValuePair<double, double>> values = new List<KeyValuePair<double, double>>();
            values.Add(new KeyValuePair<double, double>(1, 2));
            values.Add(new KeyValuePair<double, double>(3, 5));

            JsonValue jsonValue_Temp = JsonValue.Create(values);

            List<KeyValuePair<double, double>> values_Temp = jsonValue_Temp.GetValue<List<KeyValuePair<double, double>>>();



            Point2D point2D = new Point2D(1, 1);
            Vector2D vector2D_3 = new Vector2D(0, 1);
            Transform2D transform2D = Transform2D.GetRotation(Math.PI);

            point2D.Transform(transform2D);
            vector2D_3.Transform(transform2D);






            List<int[]> indexes = new List<int[]>() { new int[] {1, 2, 3 }, new int[] { 2, 2, 3 } };

            JsonValue jsonValue = JsonValue.Create(indexes);

            List<int[]> indexes_Temp = jsonValue.GetValue<List<int[]>>();

            bool collinear = (new Line2D(new Point2D(0, 0), vector2D_1)).Collinear(new Line2D(new Point2D(2, 2), vector2D_2));


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