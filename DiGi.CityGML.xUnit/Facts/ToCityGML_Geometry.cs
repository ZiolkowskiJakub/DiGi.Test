using DiGi.CityGML.Interfaces;
using DiGi.Geometry.Spatial.Classes;
using DiGi.Geometry.Spatial.Interfaces;
using System;
using System.Collections.Generic;
using System.Xml;

namespace DiGi.CityGML.xUnit
{
    public partial class Facts
    {
        /// <summary>
        /// Builds an XML node from the given markup.
        /// </summary>
        /// <param name="xml">The markup of the node.</param>
        /// <returns>The document element of the parsed markup.</returns>
        private static XmlNode XmlNode_Parse(string xml)
        {
            XmlDocument xmlDocument = new();
            xmlDocument.LoadXml(xml);
            return xmlDocument.DocumentElement!;
        }

        /// <summary>
        /// Builds the markup of a linear ring holding the given position list.
        /// </summary>
        /// <param name="posList">The content of the position list.</param>
        /// <returns>The markup of the ring.</returns>
        private static string Xml_LinearRing(string posList)
        {
            return $"<gml:LinearRing xmlns:gml=\"http://www.opengis.net/gml\"><gml:posList>{posList}</gml:posList></gml:LinearRing>";
        }

        /// <summary>
        /// Builds the markup of a wall surface holding one exterior ring and one interior element per given hole, which is how the holes of a polygon are written.
        /// </summary>
        /// <param name="exterior">The position list of the exterior ring.</param>
        /// <param name="interiors">The position lists of the holes, each written as its own interior element.</param>
        /// <returns>The markup of the surface.</returns>
        private static string Xml_WallSurface(string exterior, params string[] interiors)
        {
            string interior = string.Empty;
            for (int i = 0; i < (interiors?.Length ?? 0); i++)
            {
                interior += $"<gml:interior>{Xml_LinearRing(interiors![i])}</gml:interior>";
            }

            return $"<bldg:WallSurface xmlns:bldg=\"http://www.opengis.net/citygml/building/2.0\" xmlns:gml=\"http://www.opengis.net/gml\" gml:id=\"S1\"><bldg:lod2MultiSurface><gml:MultiSurface><gml:surfaceMember><gml:Polygon><gml:exterior>{Xml_LinearRing(exterior)}</gml:exterior>{interior}</gml:Polygon></gml:surfaceMember></gml:MultiSurface></bldg:lod2MultiSurface></bldg:WallSurface>";
        }

        /// <summary>
        /// Tests that every hole of a polygon survives the conversion, whichever way the holes are distributed over the interior elements.
        /// <para>Regression guard for the holes lost while reading the national 3D building model: each hole of a polygon is written as its own <c>gml:interior</c> element, and the list collecting the rings used to be created inside the loop walking those elements. Every interior element therefore discarded the rings gathered by the previous ones and only the holes of the last element reached the geometry - measured against the source files, 154 of 348 holes of one county were lost this way.</para>
        /// </summary>
        [Fact]
        public void ToCityGML_PolygonalFace3D_MultipleInteriorElements()
        {
            // A 10 x 10 wall in the XZ plane with three separate 1 x 1 holes, each in its own interior element.
            const string exterior = "0 0 0 10 0 0 10 0 10 0 0 10 0 0 0";
            const string hole_1 = "1 0 1 2 0 1 2 0 2 1 0 2 1 0 1";
            const string hole_2 = "4 0 1 5 0 1 5 0 2 4 0 2 4 0 1";
            const string hole_3 = "7 0 1 8 0 1 8 0 2 7 0 2 7 0 1";

            ISurface? surface = Convert.ToCityGML_Surface(XmlNode_Parse(Xml_WallSurface(exterior, hole_1, hole_2, hole_3)));
            Assert.NotNull(surface);

            IPolygonalFace3D? polygonalFace3D = surface.Geometry as IPolygonalFace3D;
            Assert.NotNull(polygonalFace3D);

            List<IPolygonal3D>? edges = polygonalFace3D.Edges;
            Assert.NotNull(edges);

            // One exterior ring plus the three holes.
            Assert.Equal(4, edges.Count);

            // A surface without holes still holds its single exterior ring.
            ISurface? surface_NoHole = Convert.ToCityGML_Surface(XmlNode_Parse(Xml_WallSurface(exterior)));
            Assert.NotNull(surface_NoHole);
            Assert.Single((surface_NoHole.Geometry as IPolygonalFace3D)?.Edges!);
        }

        /// <summary>
        /// Tests that a position list is read whatever whitespace separates its coordinates.
        /// <para>The specification allows any whitespace between the coordinates, while the reader used to split on a single space and index the triples without checking the bounds of the array. A position list separated by anything else therefore threw an <see cref="IndexOutOfRangeException"/> and took down the import of the whole file. The files published so far happen to use single spaces, which is the only reason this was never hit.</para>
        /// </summary>
        [Theory]
        [InlineData("0 0 0 4 0 0 4 0 3 0 0 3 0 0 0")]
        [InlineData("0 0 0 4 0 0 4 0 3 0 0 3 0 0 0 ")]
        [InlineData("0 0 0  4 0 0  4 0 3  0 0 3  0 0 0")]
        [InlineData("0 0 0\n4 0 0\n4 0 3\n0 0 3\n0 0 0")]
        [InlineData("\n      0 0 0\t4 0 0\t4 0 3\t0 0 3\t0 0 0\n    ")]
        public void ToCityGML_Point3Ds_Whitespace(string posList)
        {
            List<Point3D>? point3Ds = Convert.ToCityGML_Point3Ds(XmlNode_Parse(Xml_LinearRing(posList)).FirstChild);
            Assert.NotNull(point3Ds);
            Assert.Equal(5, point3Ds.Count);

            foreach (Point3D point3D in point3Ds)
            {
                Assert.False(double.IsNaN(point3D.X) || double.IsNaN(point3D.Y) || double.IsNaN(point3D.Z));
            }

            // The whole surface has to come through, not just the coordinates.
            ISurface? surface = Convert.ToCityGML_Surface(XmlNode_Parse(Xml_WallSurface(posList)));
            Assert.NotNull(surface);
            Assert.NotNull(surface.Geometry);
        }

        /// <summary>
        /// Tests that a coordinate which cannot be read leaves the position undefined instead of substituting a not-a-number value.
        /// <para>A not-a-number coordinate is not detectable as an error further down: it travels into the plane of the surface, from there into every point projected onto it, and is finally stored - by which point it can no longer be traced to the file it came from.</para>
        /// </summary>
        [Fact]
        public void ToCityGML_Point3D_UnreadableCoordinate()
        {
            XmlNode xmlNode_Valid = XmlNode_Parse("<gml:pos xmlns:gml=\"http://www.opengis.net/gml\">1 2 3</gml:pos>");
            Point3D? point3D_Valid = Convert.ToCityGML_Point3D(xmlNode_Valid);
            Assert.NotNull(point3D_Valid);
            Assert.Equal(1, point3D_Valid.X, 9);
            Assert.Equal(2, point3D_Valid.Y, 9);
            Assert.Equal(3, point3D_Valid.Z, 9);

            Assert.Null(Convert.ToCityGML_Point3D(XmlNode_Parse("<gml:pos xmlns:gml=\"http://www.opengis.net/gml\">1 abc 3</gml:pos>")));
            Assert.Null(Convert.ToCityGML_Point3D(XmlNode_Parse("<gml:pos xmlns:gml=\"http://www.opengis.net/gml\">1 2</gml:pos>")));
        }
    }
}
