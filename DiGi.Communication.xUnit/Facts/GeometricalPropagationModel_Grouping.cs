using DiGi.Communication.Classes;
using DiGi.Geometry.Spatial.Classes;
using System.Collections.Generic;

namespace DiGi.Communication.xUnit
{
    public partial class Facts
    {
        /// <summary>
        /// Tests the grouping mechanism of <see cref="GeometricalPropagationModel"/>: creates scattering objects
        /// with known mesh geometries, groups them via <see cref="GeometricalPropagationModel.Group{T}"/>,
        /// retrieves the groups via <see cref="GeometricalPropagationModel.GetScatteringGroups{T}"/> and
        /// verifies that the bounding box of each group correctly encloses its constituent objects.
        /// <para>Edge cases for null or empty input collections are also verified.</para>
        /// </summary>
        [Fact]
        public void GeometricalPropagationModel_Grouping()
        {
            // Object A: triangle at (0,0,0)-(10,0,0)-(0,5,5) with centroid ≈ (3.33, 1.67, 1.67)
            ScatteringObject scatteringObject_A = new("ObjectA", new Mesh3D(
                [new Point3D(0, 0, 0), new Point3D(10, 0, 0), new Point3D(0, 5, 5)],
                [[0, 1, 2]]));

            // Object B: triangle at (20,0,0)-(30,0,0)-(20,5,5) with centroid ≈ (23.33, 1.67, 1.67)
            ScatteringObject scatteringObject_B = new("ObjectB", new Mesh3D(
                [new Point3D(20, 0, 0), new Point3D(30, 0, 0), new Point3D(20, 5, 5)],
                [[0, 1, 2]]));

            // Object C: triangle at (0,10,0)-(10,10,0)-(0,15,5) with centroid ≈ (3.33, 11.67, 1.67)
            ScatteringObject scatteringObject_C = new("ObjectC", new Mesh3D(
                [new Point3D(0, 10, 0), new Point3D(10, 10, 0), new Point3D(0, 15, 5)],
                [[0, 1, 2]]));

            GeometricalPropagationModel geometricalPropagationModel = new();
            Assert.True(geometricalPropagationModel.Update(scatteringObject_A));
            Assert.True(geometricalPropagationModel.Update(scatteringObject_B));
            Assert.True(geometricalPropagationModel.Update(scatteringObject_C));

            // Group objects A and B together
            ScatteringGroup? scatteringGroup_AB = geometricalPropagationModel.Group<ScatteringObject>("Group_AB", [scatteringObject_A, scatteringObject_B]);
            Assert.NotNull(scatteringGroup_AB);
            Assert.Equal("Group_AB", scatteringGroup_AB.Reference);

            // Group object C alone
            ScatteringGroup? scatteringGroup_C = geometricalPropagationModel.Group<ScatteringObject>("Group_C", [scatteringObject_C]);
            Assert.NotNull(scatteringGroup_C);
            Assert.Equal("Group_C", scatteringGroup_C.Reference);

            // Retrieve all groups
            List<ScatteringGroup>? scatteringGroups = geometricalPropagationModel.GetScatteringGroups<ScatteringGroup>();
            Assert.NotNull(scatteringGroups);
            Assert.Equal(2, scatteringGroups.Count);

            // Verify Group_AB bounding box: union of A (min 0,0,0, max 10,5,5) and B (min 20,0,0, max 30,5,5)
            // Expected union: min (0,0,0), max (30,5,5)
            BoundingBox3D? boundingBox3D_AB = scatteringGroup_AB.BoundingBox3D;
            Assert.NotNull(boundingBox3D_AB);
            Point3D? min_AB = boundingBox3D_AB.Min;
            Point3D? max_AB = boundingBox3D_AB.Max;
            Assert.NotNull(min_AB);
            Assert.NotNull(max_AB);
            Assert.Equal(0.0, min_AB.X, 9);
            Assert.Equal(0.0, min_AB.Y, 9);
            Assert.Equal(0.0, min_AB.Z, 9);
            Assert.Equal(30.0, max_AB.X, 9);
            Assert.Equal(5.0, max_AB.Y, 9);
            Assert.Equal(5.0, max_AB.Z, 9);

            // Verify Group_C bounding box: min (0,10,0), max (10,15,5)
            BoundingBox3D? boundingBox3D_C = scatteringGroup_C.BoundingBox3D;
            Assert.NotNull(boundingBox3D_C);
            Point3D? min_C = boundingBox3D_C.Min;
            Point3D? max_C = boundingBox3D_C.Max;
            Assert.NotNull(min_C);
            Assert.NotNull(max_C);
            Assert.Equal(0.0, min_C.X, 9);
            Assert.Equal(10.0, min_C.Y, 9);
            Assert.Equal(0.0, min_C.Z, 9);
            Assert.Equal(10.0, max_C.X, 9);
            Assert.Equal(15.0, max_C.Y, 9);
            Assert.Equal(5.0, max_C.Z, 9);

            // Geometry check: centroids of constituent objects are inside their group bounding boxes
            double tolerance = 1e-6;
            Assert.True(boundingBox3D_AB.InRange(new Point3D(10.0 / 3.0, 5.0 / 3.0, 5.0 / 3.0), tolerance));
            Assert.True(boundingBox3D_AB.InRange(new Point3D(70.0 / 3.0, 5.0 / 3.0, 5.0 / 3.0), tolerance));
            Assert.True(boundingBox3D_C.InRange(new Point3D(10.0 / 3.0, 35.0 / 3.0, 5.0 / 3.0), tolerance));

            // Object A centroid is outside Group_C bounding box
            Assert.False(boundingBox3D_C.InRange(new Point3D(10.0 / 3.0, 5.0 / 3.0, 5.0 / 3.0), tolerance));

            // Edge case: null input returns null
            ScatteringGroup? scatteringGroup_Null = geometricalPropagationModel.Group<ScatteringObject>(null, null);
            Assert.Null(scatteringGroup_Null);

            // Edge case: empty collection returns null
            ScatteringGroup? scatteringGroup_Empty = geometricalPropagationModel.Group<ScatteringObject>("Empty", []);
            Assert.Null(scatteringGroup_Empty);
        }
    }
}
