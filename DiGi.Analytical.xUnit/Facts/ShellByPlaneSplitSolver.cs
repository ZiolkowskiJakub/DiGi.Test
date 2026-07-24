using DiGi.Analytical.Classes;
using DiGi.Core.Classes;
using DiGi.Core.Interfaces;
using DiGi.Geometry.Spatial.Classes;
using DiGi.Geometry.Spatial.Interfaces;
using System;
using System.Collections.Generic;

namespace DiGi.Analytical.xUnit
{
    public partial class Facts
    {
        /// <summary>
        /// Tests that <see cref="Classes.ShellByPlaneSplitSolver"/> attributes the faces of the resulting shells to the
        /// faces of the split shell they originate from, and only to those.
        /// <para>The shell is a wedge and the cutting plane is parallel to its sloped face, so that face is not touched
        /// by the split and has to survive intact, keeping its reference on exactly one output face. The face created on
        /// the cutting plane has no source face at all and therefore has to come out with no reference.</para>
        /// <para>The sloped face is the regression guard: its axis aligned bounding box contains the internal point of
        /// the face created on the cutting plane, so a solver that fails to recognize it as a face that survived the
        /// split keeps it among the closest face candidates and hands its reference to that new face - which is what
        /// happened while the faces were matched with == instead of by value.</para>
        /// </summary>
        [Fact]
        public void ShellByPlaneSplitSolver()
        {
            TypeReference typeReference = new(typeof(Face));

            GuidReference guidReference_Bottom = new(typeReference, Guid.NewGuid());
            GuidReference guidReference_Wall = new(typeReference, Guid.NewGuid());
            GuidReference guidReference_Slope = new(typeReference, Guid.NewGuid());
            GuidReference guidReference_End_1 = new(typeReference, Guid.NewGuid());
            GuidReference guidReference_End_2 = new(typeReference, Guid.NewGuid());

            Face CreateFace(IUniqueReference uniqueReference, params Point3D[] point3Ds)
            {
                IPolygonal3D? externalEdge = Geometry.Spatial.Create.Polygon3D(point3Ds);

                PolygonalFace3D? polygonalFace3D = Geometry.Spatial.Create.PolygonalFace3D(externalEdge);

                Assert.NotNull(polygonalFace3D);

                return new Face(uniqueReference, polygonalFace3D);
            }

            // Wedge with a triangular section in the XZ plane, extruded 10 along Y: the sloped face runs from x = 10 up to z = 10
            List<Face> faces =
            [
                CreateFace(guidReference_Bottom, new Point3D(0, 0, 0), new Point3D(10, 0, 0), new Point3D(10, 10, 0), new Point3D(0, 10, 0)),
                CreateFace(guidReference_Wall, new Point3D(0, 0, 0), new Point3D(0, 10, 0), new Point3D(0, 10, 10), new Point3D(0, 0, 10)),
                CreateFace(guidReference_Slope, new Point3D(10, 0, 0), new Point3D(10, 10, 0), new Point3D(0, 10, 10), new Point3D(0, 0, 10)),
                CreateFace(guidReference_End_1, new Point3D(0, 0, 0), new Point3D(0, 0, 10), new Point3D(10, 0, 0)),
                CreateFace(guidReference_End_2, new Point3D(0, 10, 0), new Point3D(10, 10, 0), new Point3D(0, 10, 10))
            ];

            Shell shell = new(new GuidReference(typeReference, Guid.NewGuid()), faces);

            // x + z = 5, parallel to the sloped face x + z = 10
            Plane? plane = Geometry.Spatial.Create.Plane(new Point3D(5, 0, 0), new Point3D(0, 0, 5), new Point3D(0, 10, 5));

            Assert.NotNull(plane);

            ShellByPlaneSplitSolver shellByPlaneSplitSolver = new()
            {
                Input = shell,
                Plane = plane
            };

            Assert.True(shellByPlaneSplitSolver.Solve());

            List<Shell>? shells = shellByPlaneSplitSolver.Outputs;

            Assert.NotNull(shells);
            Assert.NotEmpty(shells);

            List<Face> faces_Output = [];
            foreach (Shell shell_Output in shells)
            {
                faces_Output.AddRange(shell_Output.PolygonalFaces ?? []);
            }

            List<Face> faces_Slope = faces_Output.FindAll(x => Core.Query.Equals(x.UniqueReference, guidReference_Slope));

            Assert.Single(faces_Slope);
            Assert.True(Core.Query.AlmostEquals(100.0 * Math.Sqrt(2.0), faces_Slope[0].GetArea(), Core.Constants.Tolerance.MacroDistance));

            List<Face> faces_CuttingPlane = faces_Output.FindAll(x => x.UniqueReference is null);

            Assert.NotEmpty(faces_CuttingPlane);

            foreach (Face face_CuttingPlane in faces_CuttingPlane)
            {
                Assert.True(Core.Query.AlmostEquals(50.0 * Math.Sqrt(2.0), face_CuttingPlane.GetArea(), Core.Constants.Tolerance.MacroDistance));
            }
        }

        /// <summary>
        /// Tests the two properties of <see cref="Face.UniqueReference"/> that make it impossible to compare with ==:
        /// the accessor returns a fresh clone on every call, so the operands are never the same instance, and both
        /// operands are typed as <see cref="IUniqueReference"/>, so the equality operators declared on
        /// <see cref="SerializableReference"/> do not apply and the comparison degrades to reference equality.
        /// <para>Comparing by value with <see cref="Core.Query.Equals(IReference, IReference)"/> is correct.</para>
        /// </summary>
        [Fact]
        public void Face_UniqueReference()
        {
            Plane? plane = Geometry.Spatial.Create.Plane(0.0);

            PolygonalFace3D? polygonalFace3D = Geometry.Spatial.Create.PolygonalFace3D(plane,
            [
                new Geometry.Planar.Classes.Point2D(0, 0),
                new Geometry.Planar.Classes.Point2D(0, 10),
                new Geometry.Planar.Classes.Point2D(10, 10),
                new Geometry.Planar.Classes.Point2D(10, 0)
            ]);

            TypeReference typeReference = new(typeof(Face));

            Face face = new(new GuidReference(typeReference, Guid.NewGuid()), polygonalFace3D);

            IUniqueReference? uniqueReference_1 = face.UniqueReference;
            IUniqueReference? uniqueReference_2 = face.UniqueReference;

            Assert.NotNull(uniqueReference_1);
            Assert.NotNull(uniqueReference_2);

            Assert.False(ReferenceEquals(uniqueReference_1, uniqueReference_2));
            Assert.False(uniqueReference_1 == uniqueReference_2);

            Assert.True(Core.Query.Equals(uniqueReference_1, uniqueReference_2));
        }
    }
}
