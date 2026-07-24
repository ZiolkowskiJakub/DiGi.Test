namespace DiGi.Geometry.xUnit
{
    public partial class Facts
    {
        /// <summary>
        /// Tests the Triangle3D copy constructor to verify that vertices are deep-copied and do not share references.
        /// </summary>
        [Fact]
        public void Triangle3D_CopyConstructor_DeepCopiesVertices()
        {
            Spatial.Classes.Point3D point3D_1 = new(0.0, 0.0, 0.0);
            Spatial.Classes.Point3D point3D_2 = new(4.0, 0.0, 0.0);
            Spatial.Classes.Point3D point3D_3 = new(0.0, 3.0, 0.0);

            Spatial.Classes.Triangle3D triangle3D_Orig = new(point3D_1, point3D_2, point3D_3);

            // Copy-construct
            Spatial.Classes.Triangle3D triangle3D_Copy = new(triangle3D_Orig);

            // Verify they have same values
            Spatial.Classes.Point3D? point3D_CopyStart = triangle3D_Copy[0];
            Assert.NotNull(point3D_CopyStart);
            if (point3D_CopyStart is not null)
            {
                Assert.Equal(0.0, point3D_CopyStart.X, 9);
                Assert.Equal(0.0, point3D_CopyStart.Y, 9);
                Assert.Equal(0.0, point3D_CopyStart.Z, 9);
            }

            // Apply a movement to the copy (e.g., translate by (1, 2, 3))
            Spatial.Classes.Vector3D vector3D_Translation = new(1.0, 2.0, 3.0);
            bool bool_Success = triangle3D_Copy.Move(vector3D_Translation);
            Assert.True(bool_Success);

            // Verify copy vertices are translated
            Spatial.Classes.Point3D? point3D_CopyStartMoved = triangle3D_Copy[0];
            Assert.NotNull(point3D_CopyStartMoved);
            if (point3D_CopyStartMoved is not null)
            {
                Assert.Equal(1.0, point3D_CopyStartMoved.X, 9);
                Assert.Equal(2.0, point3D_CopyStartMoved.Y, 9);
                Assert.Equal(3.0, point3D_CopyStartMoved.Z, 9);
            }

            // Verify original vertices are completely unaffected (if references were shared, this would fail!)
            Spatial.Classes.Point3D? point3D_OrigStart = triangle3D_Orig[0];
            Assert.NotNull(point3D_OrigStart);
            if (point3D_OrigStart is not null)
            {
                Assert.Equal(0.0, point3D_OrigStart.X, 9);
                Assert.Equal(0.0, point3D_OrigStart.Y, 9);
                Assert.Equal(0.0, point3D_OrigStart.Z, 9);
            }
        }

        /// <summary>
        /// Tests the GetArea and GetCentroid methods of the Triangle3D class.
        /// </summary>
        [Fact]
        public void Triangle3D_AreaAndCentroid()
        {
            // Right triangle in XY plane with base 4 and height 3. Area should be 0.5 * 4 * 3 = 6.0
            Spatial.Classes.Point3D point3D_1 = new(0.0, 0.0, 0.0);
            Spatial.Classes.Point3D point3D_2 = new(4.0, 0.0, 0.0);
            Spatial.Classes.Point3D point3D_3 = new(0.0, 3.0, 0.0);

            Spatial.Classes.Triangle3D triangle3D_Test = new(point3D_1, point3D_2, point3D_3);

            // Verify Area
            double double_Area = triangle3D_Test.GetArea();
            Assert.Equal(6.0, double_Area, 9);

            // Verify Centroid: average of (0,0,0), (4,0,0), (0,3,0) should be (4/3, 3/3, 0) = (1.333333333, 1.0, 0.0)
            Spatial.Classes.Point3D? point3D_Centroid = triangle3D_Test.GetCentroid();
            Assert.NotNull(point3D_Centroid);
            if (point3D_Centroid is not null)
            {
                Assert.Equal(4.0 / 3.0, point3D_Centroid.X, 9);
                Assert.Equal(1.0, point3D_Centroid.Y, 9);
                Assert.Equal(0.0, point3D_Centroid.Z, 9);
            }
        }

        /// <summary>
        /// Tests the Triangle3D.Inside method to verify correct containment check logic
        /// <para>Checks various edge cases including interior points, exterior points, vertices, edges, and tolerances.</para>
        /// </summary>
        [Fact]
        public void Triangle3D_Inside_EdgeCases()
        {
            Spatial.Classes.Point3D point3D_1 = new(0.0, 0.0, 0.0);
            Spatial.Classes.Point3D point3D_2 = new(4.0, 0.0, 0.0);
            Spatial.Classes.Point3D point3D_3 = new(0.0, 3.0, 0.0);
            Spatial.Classes.Triangle3D triangle3D_Test = new(point3D_1, point3D_2, point3D_3);

            double double_Tol = 1e-5;

            // 1. Inside point
            Spatial.Classes.Point3D point3D_Inside = new(1.0, 1.0, 0.0);
            Assert.True(triangle3D_Test.Inside(point3D_Inside, double_Tol));

            // 2. Outside point
            Spatial.Classes.Point3D point3D_Outside = new(5.0, 5.0, 0.0);
            Assert.False(triangle3D_Test.Inside(point3D_Outside, double_Tol));

            // 3. Exactly on vertex
            Spatial.Classes.Point3D point3D_Vertex = new(4.0, 0.0, 0.0);
            Assert.True(triangle3D_Test.Inside(point3D_Vertex, double_Tol));

            // 4. Exactly on edge
            Spatial.Classes.Point3D point3D_Edge = new(2.0, 0.0, 0.0);
            Assert.True(triangle3D_Test.Inside(point3D_Edge, double_Tol));

            // 5. Outside but within tolerance
            Spatial.Classes.Point3D point3D_NearEdgeInsideTol = new(2.0, -0.5e-5, 0.0);
            Assert.True(triangle3D_Test.Inside(point3D_NearEdgeInsideTol, double_Tol));

            // 6. Outside beyond tolerance
            Spatial.Classes.Point3D point3D_NearEdgeOutsideTol = new(2.0, -4e-5, 0.0);
            Assert.False(triangle3D_Test.Inside(point3D_NearEdgeOutsideTol, double_Tol));

            // 7. Off plane but within tolerance
            Spatial.Classes.Point3D point3D_OffPlaneInsideTol = new(1.0, 1.0, 0.5e-5);
            Assert.True(triangle3D_Test.Inside(point3D_OffPlaneInsideTol, double_Tol));

            // 8. Off plane beyond tolerance
            Spatial.Classes.Point3D point3D_OffPlaneOutsideTol = new(1.0, 1.0, 2e-5);
            Assert.False(triangle3D_Test.Inside(point3D_OffPlaneOutsideTol, double_Tol));
        }
    }
}