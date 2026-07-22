using DiGi.Analytical.Classes;
using DiGi.Core.Classes;
using DiGi.Core.Interfaces;
using DiGi.Geometry.Planar.Classes;
using DiGi.Geometry.Spatial.Classes;
using DiGi.Geometry.Spatial.Interfaces;

namespace DiGi.Analytical.xUnit
{
    public partial class Facts
    {
        /// <summary>
        /// Creates a box <see cref="Shell"/> from the given <see cref="BoundingBox3D"/> where the shell and each of its faces carry a distinct <see cref="GuidReference"/>.
        /// </summary>
        /// <param name="boundingBox3D">The <see cref="BoundingBox3D"/> defining the extents of the box.</param>
        /// <param name="uniqueReference">The <see cref="IUniqueReference"/> to be assigned to the created <see cref="Shell"/>.</param>
        /// <returns>A <see cref="Shell"/> built from the six faces of the box, or null if the box could not be converted.</returns>
        private static Shell? BoxShell(BoundingBox3D boundingBox3D, IUniqueReference? uniqueReference)
        {
            Polyhedron? polyhedron = Geometry.Spatial.Create.Polyhedron(boundingBox3D);
            if (polyhedron?.PolygonalFaces is not List<IPolygonalFace3D> polygonalFace3Ds)
            {
                return null;
            }

            List<Face> faces = [];
            for (int i = 0; i < polygonalFace3Ds.Count; i++)
            {
                if (polygonalFace3Ds[i] is not PolygonalFace3D polygonalFace3D)
                {
                    continue;
                }

                faces.Add(new Face(new GuidReference(new TypeReference(typeof(Face)), Guid.NewGuid()), polygonalFace3D));
            }

            return new Shell(uniqueReference, faces);
        }

        /// <summary>
        /// Tests splitting a <see cref="Face"/> by non coplanar faces and verifies that every resulting face inherits the <see cref="IUniqueReference"/> of the split face.
        /// </summary>
        [Fact]
        public void TrySplit_Face_ByFaces()
        {
            GuidReference guidReference_Face = new(new TypeReference(typeof(Face)), Guid.NewGuid());

            PolygonalFace3D? polygonalFace3D = Geometry.Spatial.Create.PolygonalFace3D(Geometry.Spatial.Constants.Plane.WorldZ,
            [
                new Point2D(0, 0),
                new Point2D(10, 0),
                new Point2D(10, 10),
                new Point2D(0, 10)
            ]);

            Assert.NotNull(polygonalFace3D);

            Face face = new(guidReference_Face, polygonalFace3D);

            Polygon3D? polygon3D_Cutting = Geometry.Spatial.Create.Polygon3D(
            [
                new Point3D(5, -5, -1),
                new Point3D(5, 15, -1),
                new Point3D(5, 15, 1),
                new Point3D(5, -5, 1)
            ]);

            Assert.NotNull(polygon3D_Cutting);

            PolygonalFace3D? polygonalFace3D_Cutting = Geometry.Spatial.Create.PolygonalFace3D(polygon3D_Cutting);

            Assert.NotNull(polygonalFace3D_Cutting);

            Face face_Cutting = new(new GuidReference(new TypeReference(typeof(Face)), Guid.NewGuid()), polygonalFace3D_Cutting);

            Assert.True(Query.TrySplit(face, [face_Cutting], out List<Face>? faces));

            Assert.NotNull(faces);
            Assert.True(faces.Count > 1);

            for (int i = 0; i < faces.Count; i++)
            {
                IUniqueReference? uniqueReference = faces[i].UniqueReference;

                Assert.NotNull(uniqueReference);
                Assert.True(guidReference_Face.Equals(uniqueReference));
            }
        }

        /// <summary>
        /// Tests splitting a <see cref="Face"/> by a coplanar <see cref="Face"/> and verifies that fragments covered by the coplanar cutting face inherit its <see cref="IUniqueReference"/> while the remaining fragments keep the reference of the split face.
        /// </summary>
        [Fact]
        public void TrySplit_Face_ByCoplanarFaces()
        {
            GuidReference guidReference_Face = new(new TypeReference(typeof(Face)), Guid.NewGuid());
            GuidReference guidReference_Face_Cutting = new(new TypeReference(typeof(Face)), Guid.NewGuid());

            PolygonalFace3D? polygonalFace3D = Geometry.Spatial.Create.PolygonalFace3D(Geometry.Spatial.Constants.Plane.WorldZ,
            [
                new Point2D(0, 0),
                new Point2D(10, 0),
                new Point2D(10, 10),
                new Point2D(0, 10)
            ]);

            Assert.NotNull(polygonalFace3D);

            Face face = new(guidReference_Face, polygonalFace3D);

            PolygonalFace3D? polygonalFace3D_Cutting = Geometry.Spatial.Create.PolygonalFace3D(Geometry.Spatial.Constants.Plane.WorldZ,
            [
                new Point2D(-5, -5),
                new Point2D(5, -5),
                new Point2D(5, 15),
                new Point2D(-5, 15)
            ]);

            Assert.NotNull(polygonalFace3D_Cutting);

            Face face_Cutting = new(guidReference_Face_Cutting, polygonalFace3D_Cutting);

            Assert.True(Query.TrySplit(face, [face_Cutting], out List<Face>? faces));

            Assert.NotNull(faces);
            Assert.Equal(2, faces.Count);

            int count_Face = 0;
            int count_Face_Cutting = 0;

            for (int i = 0; i < faces.Count; i++)
            {
                IUniqueReference? uniqueReference = faces[i].UniqueReference;

                Assert.NotNull(uniqueReference);

                if (guidReference_Face_Cutting.Equals(uniqueReference))
                {
                    count_Face_Cutting++;
                    continue;
                }

                Assert.True(guidReference_Face.Equals(uniqueReference));
                count_Face++;
            }

            Assert.Equal(1, count_Face_Cutting);
            Assert.Equal(1, count_Face);
        }

        /// <summary>
        /// Tests splitting a <see cref="Shell"/> by another <see cref="Shell"/> and verifies that the resulting shell keeps the reference of the split shell while every resulting face keeps a reference of one of the source faces.
        /// </summary>
        [Fact]
        public void TrySplit_Shell_ByShells()
        {
            GuidReference guidReference_Shell = new(new TypeReference(typeof(Shell)), Guid.NewGuid());
            GuidReference guidReference_Shell_Cutting = new(new TypeReference(typeof(Shell)), Guid.NewGuid());

            Shell? shell = BoxShell(new BoundingBox3D(new Point3D(0, 0, 0), new Point3D(10, 10, 10)), guidReference_Shell);
            Shell? shell_Cutting = BoxShell(new BoundingBox3D(new Point3D(5, -5, -5), new Point3D(15, 5, 5)), guidReference_Shell_Cutting);

            Assert.NotNull(shell);
            Assert.NotNull(shell_Cutting);

            List<IUniqueReference> uniqueReferences_Source = [];

            List<Face>? faces_Source = shell.PolygonalFaces;
            Assert.NotNull(faces_Source);

            for (int i = 0; i < faces_Source.Count; i++)
            {
                IUniqueReference? uniqueReference = faces_Source[i].UniqueReference;

                Assert.NotNull(uniqueReference);
                uniqueReferences_Source.Add(uniqueReference);
            }

            List<Face>? faces_Cutting = shell_Cutting.PolygonalFaces;
            Assert.NotNull(faces_Cutting);

            for (int i = 0; i < faces_Cutting.Count; i++)
            {
                IUniqueReference? uniqueReference = faces_Cutting[i].UniqueReference;

                Assert.NotNull(uniqueReference);
                uniqueReferences_Source.Add(uniqueReference);
            }

            Assert.True(Query.TrySplit(shell, [shell_Cutting], out Shell? shell_Result));

            Assert.NotNull(shell_Result);
            Assert.True(guidReference_Shell.Equals(shell_Result.UniqueReference));

            // The source box had six faces, the cutting box splits some of them into multiple fragments
            Assert.True(shell_Result.Count > 6);

            List<Face>? faces_Result = shell_Result.PolygonalFaces;
            Assert.NotNull(faces_Result);

            for (int i = 0; i < faces_Result.Count; i++)
            {
                IUniqueReference? uniqueReference = faces_Result[i].UniqueReference;

                Assert.NotNull(uniqueReference);
                Assert.True(uniqueReferences_Source.Exists(x => x.Equals(uniqueReference)));
            }

            Core.xUnit.Query.SerializationCheck(shell_Result);
        }

        /// <summary>
        /// Tests splitting a collection of shells and verifies that shells which are not split are preserved together with their references.
        /// </summary>
        [Fact]
        public void TrySplit_Shells()
        {
            GuidReference guidReference_Shell_1 = new(new TypeReference(typeof(Shell)), Guid.NewGuid());
            GuidReference guidReference_Shell_2 = new(new TypeReference(typeof(Shell)), Guid.NewGuid());
            GuidReference guidReference_Shell_3 = new(new TypeReference(typeof(Shell)), Guid.NewGuid());

            Shell? shell_1 = BoxShell(new BoundingBox3D(new Point3D(0, 0, 0), new Point3D(10, 10, 10)), guidReference_Shell_1);
            Shell? shell_2 = BoxShell(new BoundingBox3D(new Point3D(5, 5, -5), new Point3D(15, 15, 5)), guidReference_Shell_2);
            Shell? shell_3 = BoxShell(new BoundingBox3D(new Point3D(100, 100, 100), new Point3D(110, 110, 110)), guidReference_Shell_3);

            Assert.NotNull(shell_1);
            Assert.NotNull(shell_2);
            Assert.NotNull(shell_3);

            List<Shell> shells = [shell_1, shell_2, shell_3];

            Assert.True(Query.TrySplit(shells, out List<Shell>? shells_Result));

            Assert.NotNull(shells_Result);
            Assert.Equal(3, shells_Result.Count);

            Assert.Contains(shells_Result, x => guidReference_Shell_1.Equals(x.UniqueReference));
            Assert.Contains(shells_Result, x => guidReference_Shell_2.Equals(x.UniqueReference));
            Assert.Contains(shells_Result, x => guidReference_Shell_3.Equals(x.UniqueReference));
        }

        /// <summary>
        /// Tests splitting a <see cref="Shell"/> by a <see cref="Plane"/> and verifies that both resulting shells keep the reference of the split shell, that faces created on the cutting plane are left without a reference and that all remaining faces keep the reference of the face they originate from.
        /// </summary>
        [Fact]
        public void TrySplit_Plane_Shell()
        {
            double tolerance = Core.Constants.Tolerance.Distance;

            GuidReference guidReference_Shell = new(new TypeReference(typeof(Shell)), Guid.NewGuid());

            Shell? shell = BoxShell(new BoundingBox3D(new Point3D(0, 0, 0), new Point3D(10, 10, 10)), guidReference_Shell);

            Assert.NotNull(shell);

            List<Face>? faces_Source = shell.PolygonalFaces;
            Assert.NotNull(faces_Source);

            List<IUniqueReference> uniqueReferences_Source = [];
            for (int i = 0; i < faces_Source.Count; i++)
            {
                IUniqueReference? uniqueReference = faces_Source[i].UniqueReference;

                Assert.NotNull(uniqueReference);
                uniqueReferences_Source.Add(uniqueReference);
            }

            Plane plane = new(new Point3D(0, 0, 5), Geometry.Spatial.Constants.Vector3D.WorldZ);

            Assert.True(Query.TrySplit(plane, shell, out List<Shell>? shells_Result));

            Assert.NotNull(shells_Result);
            Assert.Equal(2, shells_Result.Count);

            for (int i = 0; i < shells_Result.Count; i++)
            {
                Shell shell_Result = shells_Result[i];

                Assert.True(guidReference_Shell.Equals(shell_Result.UniqueReference));

                List<Face>? faces_Result = shell_Result.PolygonalFaces;
                Assert.NotNull(faces_Result);

                int count_Plane = 0;

                for (int j = 0; j < faces_Result.Count; j++)
                {
                    Face face = faces_Result[j];

                    Assert.True(face.GetBoundingBox() is BoundingBox3D);

                    BoundingBox3D boundingBox3D = face.GetBoundingBox()!;

                    IUniqueReference? uniqueReference = face.UniqueReference;

                    if (boundingBox3D.MinZ > 5 - tolerance && boundingBox3D.MaxZ < 5 + tolerance)
                    {
                        // Face created on the cutting plane has no source face and therefore no reference
                        Assert.Null(uniqueReference);
                        count_Plane++;
                        continue;
                    }

                    Assert.NotNull(uniqueReference);
                    Assert.True(uniqueReferences_Source.Exists(x => x.Equals(uniqueReference)));
                }

                Assert.True(count_Plane > 0);
            }
        }

        /// <summary>
        /// Tests splitting a <see cref="Face"/> by a <see cref="Plane"/> and verifies that both resulting faces inherit the reference of the split face and that a plane which does not intersect the face returns false.
        /// </summary>
        [Fact]
        public void TrySplit_Plane_Face()
        {
            GuidReference guidReference_Face = new(new TypeReference(typeof(Face)), Guid.NewGuid());

            PolygonalFace3D? polygonalFace3D = Geometry.Spatial.Create.PolygonalFace3D(Geometry.Spatial.Constants.Plane.WorldZ,
            [
                new Point2D(0, 0),
                new Point2D(10, 0),
                new Point2D(10, 10),
                new Point2D(0, 10)
            ]);

            Assert.NotNull(polygonalFace3D);

            Face face = new(guidReference_Face, polygonalFace3D);

            Plane plane_Cutting = new(new Point3D(5, 0, 0), Geometry.Spatial.Constants.Vector3D.WorldX);

            Assert.True(Query.TrySplit(plane_Cutting, face, out List<Face>? faces));

            Assert.NotNull(faces);
            Assert.Equal(2, faces.Count);

            for (int i = 0; i < faces.Count; i++)
            {
                Assert.True(guidReference_Face.Equals(faces[i].UniqueReference));
            }

            // Plane parallel to the face does not split it
            Plane plane_Parallel = new(new Point3D(0, 0, 5), Geometry.Spatial.Constants.Vector3D.WorldZ);

            Assert.False(Query.TrySplit(plane_Parallel, face, out List<Face>? faces_Parallel));
            Assert.Null(faces_Parallel);
        }

        /// <summary>
        /// Tests that all TrySplit overloads return false and a null result for null and empty inputs.
        /// </summary>
        [Fact]
        public void TrySplit_NullInputs()
        {
            GuidReference guidReference_Shell = new(new TypeReference(typeof(Shell)), Guid.NewGuid());

            Shell? shell = BoxShell(new BoundingBox3D(new Point3D(0, 0, 0), new Point3D(10, 10, 10)), guidReference_Shell);
            Assert.NotNull(shell);

            List<Face>? faces_Source = shell.PolygonalFaces;
            Assert.NotNull(faces_Source);
            Assert.NotEmpty(faces_Source);

            Face face = faces_Source[0];

            Plane plane = new(new Point3D(0, 0, 5), Geometry.Spatial.Constants.Vector3D.WorldZ);

            Assert.False(Query.TrySplit(null as Face, [face], out List<Face>? faces_NullFace));
            Assert.Null(faces_NullFace);

            Assert.False(Query.TrySplit(face, null as IEnumerable<Face>, out List<Face>? faces_NullCutting));
            Assert.Null(faces_NullCutting);

            Assert.False(Query.TrySplit(face, [], out List<Face>? faces_EmptyCutting));
            Assert.Null(faces_EmptyCutting);

            Assert.False(Query.TrySplit(null as Shell, [shell], out Shell? shell_NullShell));
            Assert.Null(shell_NullShell);

            Assert.False(Query.TrySplit(shell, null as IEnumerable<Shell>, out Shell? shell_NullCutting));
            Assert.Null(shell_NullCutting);

            Assert.False(Query.TrySplit(null as IEnumerable<Shell>, out List<Shell>? shells_Null));
            Assert.Null(shells_Null);

            Assert.False(Query.TrySplit([shell], out List<Shell>? shells_Single));
            Assert.Null(shells_Single);

            Assert.False(Query.TrySplit(null as Plane, shell, out List<Shell>? shells_NullPlane));
            Assert.Null(shells_NullPlane);

            Assert.False(Query.TrySplit(plane, null as Shell, out List<Shell>? shells_NullShell));
            Assert.Null(shells_NullShell);

            Assert.False(Query.TrySplit(null as Plane, face, out List<Face>? faces_NullPlane));
            Assert.Null(faces_NullPlane);

            Assert.False(Query.TrySplit(plane, null as Face, out List<Face>? faces_NullFace_Plane));
            Assert.Null(faces_NullFace_Plane);
        }
    }
}
