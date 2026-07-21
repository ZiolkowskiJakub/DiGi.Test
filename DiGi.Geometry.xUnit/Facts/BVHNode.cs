using DiGi.Geometry.Spatial;
using DiGi.Geometry.Spatial.Classes;
using DiGi.Geometry.Spatial.Interfaces;

namespace DiGi.Geometry.xUnit
{
    public partial class Facts
    {
        /// <summary>
        /// Tests construction, cloning, and serialization of the public <see cref="BVHNode"/> class hierarchy.
        /// </summary>
        [Fact]
        public void BVHNode_Serialization_And_Hierarchy()
        {
            Point3D point3D_Min = new(-2, -2, -2);
            Point3D point3D_Max = new(2, 2, 2);
            BoundingBox3D boundingBox3D = new(point3D_Min, point3D_Max);
            Polyhedron? polyhedron = Create.Polyhedron(boundingBox3D);
            Assert.NotNull(polyhedron);

            if (polyhedron == null)
            {
                return;
            }

            List<IPolygonalFace3D> faces = [];
            for (int i = 0; i < polyhedron.Count; i++)
            {
                if (polyhedron.GetPolygonalFace3D<IPolygonalFace3D>(i) is IPolygonalFace3D face)
                {
                    faces.Add(face);
                }
            }

            Assert.Equal(6, faces.Count);

            // Construct BVHNode
            BVHNode bvhNode = new(faces);
            Assert.NotNull(bvhNode.Box);
            Assert.NotNull(bvhNode.Left);
            Assert.NotNull(bvhNode.Right);
            Assert.NotNull(bvhNode.Faces);
            Assert.Empty(bvhNode.Faces!);

            // Test children leaf nodes
            Assert.NotNull(bvhNode.Left.Faces);
            Assert.NotEmpty(bvhNode.Left.Faces!);
            Assert.NotNull(bvhNode.Right.Faces);
            Assert.NotEmpty(bvhNode.Right.Faces!);
            Assert.Equal(3, bvhNode.Left.Faces!.Count);
            Assert.Equal(3, bvhNode.Right.Faces!.Count);

            // Clone via copy constructor
            BVHNode bvhNode_Copy = new(bvhNode);
            Assert.NotNull(bvhNode_Copy.Box);
            Assert.Equal(bvhNode.Box.Min.X, bvhNode_Copy.Box.Min.X);
            Assert.NotNull(bvhNode_Copy.Left);
            Assert.Equal(3, bvhNode_Copy.Left.Faces?.Count);

            // Clone via Clone() override
            BVHNode? bvhNode_Clone = bvhNode.Clone() as BVHNode;
            Assert.NotNull(bvhNode_Clone);
            Assert.NotNull(bvhNode_Clone.Box);
            Assert.Equal(bvhNode.Box.Max.Z, bvhNode_Clone.Box.Max.Z);

            // Serialization via DiGi.Core.Query.Clone
            BVHNode? bvhNode_SerializedClone = DiGi.Core.Query.Clone(bvhNode);
            Assert.NotNull(bvhNode_SerializedClone);
            Assert.NotNull(bvhNode_SerializedClone.Box);
            Assert.Equal(bvhNode.Box.Min.Y, bvhNode_SerializedClone.Box.Min.Y);
            Assert.NotNull(bvhNode_SerializedClone.Left);
            Assert.Equal(3, bvhNode_SerializedClone.Left.Faces?.Count);
        }

        /// <summary>
        /// Verifies that the public extension method <see cref="Modify.AddOverlappingFaces"/> correctly queries the BVH tree
        /// to filter and collect overlapping faces of a box polyhedron.
        /// </summary>
        [Fact]
        public void BVHNode_AddOverlappingFaces()
        {
            Point3D point3D_Min = new(-2, -2, -2);
            Point3D point3D_Max = new(2, 2, 2);
            BoundingBox3D boundingBox3D = new(point3D_Min, point3D_Max);
            Polyhedron? polyhedron = Create.Polyhedron(boundingBox3D);
            Assert.NotNull(polyhedron);

            if (polyhedron == null)
            {
                return;
            }

            List<IPolygonalFace3D> faces = [];
            for (int i = 0; i < polyhedron.Count; i++)
            {
                if (polyhedron.GetPolygonalFace3D<IPolygonalFace3D>(i) is IPolygonalFace3D face)
                {
                    faces.Add(face);
                }
            }

            BVHNode bvhNode = new(faces);

            // 1. Query with a bounding box that completely overlaps the polyhedron box
            List<IPolygonalFace3D> list_Result1 = [];
            BoundingBox3D bbox_Large = new(new Point3D(-3, -3, -3), new Point3D(3, 3, 3));
            list_Result1.AddOverlappingFaces(bvhNode, bbox_Large);
            Assert.Equal(6, list_Result1.Count);

            // 2. Query with a bounding box that only overlaps the positive Z region (z > 0.1)
            List<IPolygonalFace3D> list_Result2 = [];
            BoundingBox3D bbox_PositiveZ = new(new Point3D(-1, -1, 0.1), new Point3D(1, 1, 3));
            list_Result2.AddOverlappingFaces(bvhNode, bbox_PositiveZ);
            Assert.Single(list_Result2);

            // 3. Query with a bounding box completely disjoint
            List<IPolygonalFace3D> list_Result3 = [];
            BoundingBox3D bbox_Disjoint = new(new Point3D(10, 10, 10), new Point3D(20, 20, 20));
            list_Result3.AddOverlappingFaces(bvhNode, bbox_Disjoint);
            Assert.Empty(list_Result3);
        }
    }
}