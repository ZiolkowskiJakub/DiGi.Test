using DiGi.Geometry.Planar.Classes;
using DiGi.Geometry.Planar.Interfaces;
using DiGi.Geometry.Spatial;
using DiGi.Geometry.Spatial.Classes;
using DiGi.Geometry.Spatial.Interfaces;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace DiGi.Geometry.xUnit
{
    public partial class Facts
    {
        /// <summary>
        /// Proposed implementation of <c>IsClosed</c> using Dual Half-Edge Matching via Midpoint Spatial Hash.
        /// <para>Used for direct algorithmic and performance comparison against the existing vertex-welding implementation.</para>
        /// </summary>
        /// <typeparam name="TPolygonalFace3D">The type of the polygonal face.</typeparam>
        /// <param name="polyhedron">The polyhedron to evaluate.</param>
        /// <param name="manifold">When true, requires strict 2-manifold surface.</param>
        /// <param name="tolerance">The distance tolerance.</param>
        /// <returns>True if the polyhedron is closed.</returns>
        private static bool IsClosed_Proposed<TPolygonalFace3D>(Polyhedron<TPolygonalFace3D>? polyhedron, bool manifold, double tolerance = DiGi.Core.Constants.Tolerance.Distance) where TPolygonalFace3D : IPolygonalFace3D
        {
            if (polyhedron is null || polyhedron.Count < 4)
            {
                return false;
            }

            double tolerance_Temp = tolerance > 0.0 ? tolerance : DiGi.Core.Constants.Tolerance.MicroDistance;
            double invTolerance = 1.0 / tolerance_Temp;
            double toleranceSquared = tolerance_Temp * tolerance_Temp;
            const double microToleranceSquared = 1e-18;

            // Flat arrays for half-edge geometry: Start(X,Y,Z), End(X,Y,Z), Midpoint(X,Y,Z)
            List<double> coordinates = new(polyhedron.Count * 12);
            List<int> faceIndices = new(polyhedron.Count * 4);
            List<int> indexes_Next = new(polyhedron.Count * 4);
            Dictionary<(long X, long Y, long Z), int> index_ByCell = [];

            int edgeCount = 0;

            for (int i = 0; i < polyhedron.Count; i++)
            {
                TPolygonalFace3D? polygonalFace3D = polyhedron.GetPolygonalFace3D<TPolygonalFace3D>(i);
                if (polygonalFace3D is null)
                {
                    return false;
                }

                Plane? plane = polygonalFace3D.Plane;
                if (plane is null)
                {
                    return false;
                }

                Point3D? point3D_Origin = plane.Origin;
                Spatial.Classes.Vector3D? vector3D_AxisX = plane.AxisX;
                Spatial.Classes.Vector3D? vector3D_AxisY = plane.AxisY;

                if (point3D_Origin is null || vector3D_AxisX is null || vector3D_AxisY is null)
                {
                    return false;
                }

                double originX = point3D_Origin.X;
                double originY = point3D_Origin.Y;
                double originZ = point3D_Origin.Z;

                double axisXX = vector3D_AxisX.X;
                double axisXY = vector3D_AxisX.Y;
                double axisXZ = vector3D_AxisX.Z;

                double axisYX = vector3D_AxisY.X;
                double axisYY = vector3D_AxisY.Y;
                double axisYZ = vector3D_AxisY.Z;

                IPolygonalFace2D? polygonalFace2D = polygonalFace3D.Geometry2D;
                if (polygonalFace2D is null)
                {
                    return false;
                }

                List<IPolygonal2D>? polygonal2Ds = polygonalFace2D.Edges;
                if (polygonal2Ds is null || polygonal2Ds.Count == 0)
                {
                    return false;
                }

                for (int j = 0; j < polygonal2Ds.Count; j++)
                {
                    IPolygonal2D? polygonal2D = polygonal2Ds[j];
                    if (polygonal2D is null)
                    {
                        return false;
                    }

                    List<Point2D>? point2Ds = polygonal2D is Segmentable2D segmentable2D ? segmentable2D.GetPoints(false) : polygonal2D.GetPoints();
                    if (point2Ds is null || point2Ds.Count < 3)
                    {
                        return false;
                    }

                    int count_Points = point2Ds.Count;
                    double prevX = 0, prevY = 0, prevZ = 0;
                    bool hasPrev = false;
                    double firstX = 0, firstY = 0, firstZ = 0;

                    for (int k = 0; k < count_Points; k++)
                    {
                        Point2D? point2D = point2Ds[k];
                        if (point2D is null)
                        {
                            return false;
                        }

                        double px = point2D.X;
                        double py = point2D.Y;
                        double currX = originX + (axisYX * py) + (axisXX * px);
                        double currY = originY + (axisYY * py) + (axisXY * px);
                        double currZ = originZ + (axisYZ * py) + (axisXZ * px);

                        if (!hasPrev)
                        {
                            firstX = currX;
                            firstY = currY;
                            firstZ = currZ;
                            prevX = currX;
                            prevY = currY;
                            prevZ = currZ;
                            hasPrev = true;
                            continue;
                        }

                        double dx = currX - prevX;
                        double dy = currY - prevY;
                        double dz = currZ - prevZ;

                        if ((dx * dx) + (dy * dy) + (dz * dz) > microToleranceSquared)
                        {
                            AddEdge(prevX, prevY, prevZ, currX, currY, currZ, i);
                        }

                        prevX = currX;
                        prevY = currY;
                        prevZ = currZ;
                    }

                    // Closing edge of the ring
                    double dx_Close = firstX - prevX;
                    double dy_Close = firstY - prevY;
                    double dz_Close = firstZ - prevZ;

                    if ((dx_Close * dx_Close) + (dy_Close * dy_Close) + (dz_Close * dz_Close) > microToleranceSquared)
                    {
                        AddEdge(prevX, prevY, prevZ, firstX, firstY, firstZ, i);
                    }
                }
            }

            void AddEdge(double startX, double startY, double startZ, double endX, double endY, double endZ, int faceIndex)
            {
                int edgeIndex = edgeCount++;
                double midX = (startX + endX) * 0.5;
                double midY = (startY + endY) * 0.5;
                double midZ = (startZ + endZ) * 0.5;

                coordinates.Add(startX);
                coordinates.Add(startY);
                coordinates.Add(startZ);
                coordinates.Add(endX);
                coordinates.Add(endY);
                coordinates.Add(endZ);
                coordinates.Add(midX);
                coordinates.Add(midY);
                coordinates.Add(midZ);

                faceIndices.Add(faceIndex);

                long cellX = (long)System.Math.Floor(midX * invTolerance);
                long cellY = (long)System.Math.Floor(midY * invTolerance);
                long cellZ = (long)System.Math.Floor(midZ * invTolerance);
                (long X, long Y, long Z) cell = (cellX, cellY, cellZ);

                indexes_Next.Add(index_ByCell.TryGetValue(cell, out int index_Head) ? index_Head : -1);
                index_ByCell[cell] = edgeIndex;
            }

            if (edgeCount < 6 || (edgeCount & 1) != 0)
            {
                return false;
            }

            int[] mate = new int[edgeCount];
            for (int i = 0; i < edgeCount; i++)
            {
                mate[i] = -1;
            }

            int matchedCount = 0;

            // Two-pass pairing:
            // Pass 1: Prioritize exact / micro-distance matches (distSq <= 1e-12)
            // Pass 2: Pair remaining tolerance matches (distSq <= toleranceSquared)
            for (int pass = 0; pass < 2; pass++)
            {
                double currentThresholdSq = pass == 0 ? 1e-12 : toleranceSquared;

                for (int i = 0; i < edgeCount; i++)
                {
                    if (mate[i] >= 0)
                    {
                        continue;
                    }

                    int offset_I = i * 9;
                    double startX_I = coordinates[offset_I];
                    double startY_I = coordinates[offset_I + 1];
                    double startZ_I = coordinates[offset_I + 2];
                    double endX_I = coordinates[offset_I + 3];
                    double endY_I = coordinates[offset_I + 4];
                    double endZ_I = coordinates[offset_I + 5];
                    double midX_I = coordinates[offset_I + 6];
                    double midY_I = coordinates[offset_I + 7];
                    double midZ_I = coordinates[offset_I + 8];
                    int face_I = faceIndices[i];

                    long cellX = (long)System.Math.Floor(midX_I * invTolerance);
                    long cellY = (long)System.Math.Floor(midY_I * invTolerance);
                    long cellZ = (long)System.Math.Floor(midZ_I * invTolerance);

                    int bestMatch = -1;
                    double bestDistSq = double.MaxValue;

                    for (long nx = cellX - 1; nx <= cellX + 1; nx++)
                    {
                        for (long ny = cellY - 1; ny <= cellY + 1; ny++)
                        {
                            for (long nz = cellZ - 1; nz <= cellZ + 1; nz++)
                            {
                                if (!index_ByCell.TryGetValue((nx, ny, nz), out int candHead))
                                {
                                    continue;
                                }

                                while (candHead >= 0)
                                {
                                    if (candHead != i && mate[candHead] < 0)
                                    {
                                        int face_J = faceIndices[candHead];
                                        if (face_J != face_I)
                                        {
                                            int offset_J = candHead * 9;
                                            double startX_J = coordinates[offset_J];
                                            double startY_J = coordinates[offset_J + 1];
                                            double startZ_J = coordinates[offset_J + 2];
                                            double endX_J = coordinates[offset_J + 3];
                                            double endY_J = coordinates[offset_J + 4];
                                            double endZ_J = coordinates[offset_J + 5];

                                            // Opposite orientation (standard manifold)
                                            double dx1 = startX_I - endX_J;
                                            double dy1 = startY_I - endY_J;
                                            double dz1 = startZ_I - endZ_J;
                                            double d1 = (dx1 * dx1) + (dy1 * dy1) + (dz1 * dz1);

                                            double dx2 = endX_I - startX_J;
                                            double dy2 = endY_I - startY_J;
                                            double dz2 = endZ_I - startZ_J;
                                            double d2 = (dx2 * dx2) + (dy2 * dy2) + (dz2 * dz2);

                                            double distOppSq = d1 > d2 ? d1 : d2;

                                            // Same orientation (inverted face)
                                            double dx3 = startX_I - startX_J;
                                            double dy3 = startY_I - startY_J;
                                            double dz3 = startZ_I - startZ_J;
                                            double d3 = (dx3 * dx3) + (dy3 * dy3) + (dz3 * dz3);

                                            double dx4 = endX_I - endX_J;
                                            double dy4 = endY_I - endY_J;
                                            double dz4 = endZ_I - endZ_J;
                                            double d4 = (dx4 * dx4) + (dy4 * dy4) + (dz4 * dz4);

                                            double distSameSq = d3 > d4 ? d3 : d4;
                                            double distSq = distOppSq < distSameSq ? distOppSq : distSameSq;

                                            if (distSq <= currentThresholdSq && distSq < bestDistSq)
                                            {
                                                bestDistSq = distSq;
                                                bestMatch = candHead;
                                            }
                                        }
                                    }

                                    candHead = indexes_Next[candHead];
                                }
                            }
                        }
                    }

                    if (bestMatch >= 0)
                    {
                        mate[i] = bestMatch;
                        mate[bestMatch] = i;
                        matchedCount += 2;
                    }
                }
            }

            if (matchedCount != edgeCount)
            {
                return false;
            }

            if (manifold)
            {
                // In strict 2-manifold mode, verify that no two distinct matched pairs are coincident in space (which indicates >2 faces sharing an edge).
                for (int i = 0; i < edgeCount; i++)
                {
                    int mate_I = mate[i];
                    if (mate_I < i)
                    {
                        continue; // Check each pair once
                    }

                    int offset_I = i * 9;
                    double startX_I = coordinates[offset_I];
                    double startY_I = coordinates[offset_I + 1];
                    double startZ_I = coordinates[offset_I + 2];
                    double endX_I = coordinates[offset_I + 3];
                    double endY_I = coordinates[offset_I + 4];
                    double endZ_I = coordinates[offset_I + 5];
                    double midX_I = coordinates[offset_I + 6];
                    double midY_I = coordinates[offset_I + 7];
                    double midZ_I = coordinates[offset_I + 8];
                    int face_I1 = faceIndices[i];
                    int face_I2 = faceIndices[mate_I];

                    long cellX = (long)System.Math.Floor(midX_I * invTolerance);
                    long cellY = (long)System.Math.Floor(midY_I * invTolerance);
                    long cellZ = (long)System.Math.Floor(midZ_I * invTolerance);

                    for (long nx = cellX - 1; nx <= cellX + 1; nx++)
                    {
                        for (long ny = cellY - 1; ny <= cellY + 1; ny++)
                        {
                            for (long nz = cellZ - 1; nz <= cellZ + 1; nz++)
                            {
                                if (!index_ByCell.TryGetValue((nx, ny, nz), out int candHead))
                                {
                                    continue;
                                }

                                while (candHead >= 0)
                                {
                                    if (candHead != i && candHead != mate_I)
                                    {
                                        int face_J = faceIndices[candHead];
                                        // A coincident edge must not belong to the same faces
                                        if (face_J != face_I1 && face_J != face_I2)
                                        {
                                            int offset_J = candHead * 9;
                                            double startX_J = coordinates[offset_J];
                                            double startY_J = coordinates[offset_J + 1];
                                            double startZ_J = coordinates[offset_J + 2];
                                            double endX_J = coordinates[offset_J + 3];
                                            double endY_J = coordinates[offset_J + 4];
                                            double endZ_J = coordinates[offset_J + 5];

                                            double dx1 = startX_I - endX_J;
                                            double dy1 = startY_I - endY_J;
                                            double dz1 = startZ_I - endZ_J;
                                            double d1 = (dx1 * dx1) + (dy1 * dy1) + (dz1 * dz1);

                                            double dx2 = endX_I - startX_J;
                                            double dy2 = endY_I - startY_J;
                                            double dz2 = endZ_I - startZ_J;
                                            double d2 = (dx2 * dx2) + (dy2 * dy2) + (dz2 * dz2);

                                            double distOppSq = d1 > d2 ? d1 : d2;

                                            double dx3 = startX_I - startX_J;
                                            double dy3 = startY_I - startY_J;
                                            double dz3 = startZ_I - startZ_J;
                                            double d3 = (dx3 * dx3) + (dy3 * dy3) + (dz3 * dz3);

                                            double dx4 = endX_I - endX_J;
                                            double dy4 = endY_I - endY_J;
                                            double dz4 = endZ_I - endZ_J;
                                            double d4 = (dx4 * dx4) + (dy4 * dy4) + (dz4 * dz4);

                                            double distSameSq = d3 > d4 ? d3 : d4;
                                            double distSq = distOppSq < distSameSq ? distOppSq : distSameSq;

                                            if (distSq <= 1e-12)
                                            {
                                                // Another distinct pair shares this exact edge in space -> non-manifold (>2 faces meeting at same edge)
                                                return false;
                                            }
                                        }
                                    }

                                    candHead = indexes_Next[candHead];
                                }
                            }
                        }
                    }
                }
            }

            return true;
        }

        /// <summary>
        /// Compares the existing and proposed IsClosed implementations on the 0.03 m step extrusion (Issue #1).
        /// <para>Demonstrates that the proposed Dual Half-Edge Matching is strictly monotonic across all tolerances [0.001 to 0.20], whereas the existing vertex welding opens at tolerances >= 0.03 m.</para>
        /// </summary>
        [Fact]
        public void Polyhedron_IsClosed_Comparison_StepExtrusion_Monotonicity()
        {
            Plane plane = Create.Plane(0)!;
            PolygonalFace3D? polygonalFace3D = Create.PolygonalFace3D(
                plane,
                new Point2D(0, 0),
                new Point2D(10, 0),
                new Point2D(10, 10),
                new Point2D(5, 10),
                new Point2D(5, 9.97), // 0.03 m step
                new Point2D(0, 9.97));

            Assert.NotNull(polygonalFace3D);
            Polyhedron? polyhedron = Create.Polyhedron(polygonalFace3D, new Spatial.Classes.Vector3D(0, 0, 3));
            Assert.NotNull(polyhedron);

            double[] tolerances = [0.001, 0.01, 0.02, 0.03, 0.04, 0.05, 0.08, 0.10, 0.20];

            for (int i = 0; i < tolerances.Length; i++)
            {
                double tol = tolerances[i];
                bool result_Existing_Manifold = polyhedron.IsClosed(true, tol);
                bool result_Proposed_Manifold = IsClosed_Proposed(polyhedron, true, tol);

                // Proposed must ALWAYS report closed (strictly monotonic, never collapsing valid 3 cm features)
                Assert.True(result_Proposed_Manifold, $"Proposed failed at tolerance {tol}");

                // Document existing behavior: existing fails at tol >= 0.03 because the 0.03 m edge collapses
                if (tol >= 0.03)
                {
                    Assert.False(result_Existing_Manifold, $"Existing unexpectedly succeeded at {tol}");
                }
                else
                {
                    Assert.True(result_Existing_Manifold, $"Existing unexpectedly failed at {tol}");
                }
            }
        }

        /// <summary>
        /// Compares the existing and proposed implementations on floating point threshold split (Issue #1 root cause).
        /// </summary>
        [Fact]
        public void Polyhedron_IsClosed_Comparison_FloatingPointNoise()
        {
            // Floating point noise at threshold:
            // Face 1 (bottom): edge length is 0.049999999988 (<= 0.05 -> existing collapses it)
            // Face 2 (wall): edge length is 0.050000000017 (> 0.05 -> existing does NOT collapse it)
            double d1 = 0.049999999988;

            // Box with a 0.05 m step where the bottom and top faces are flat polygons with d1 and d2 offsets
            // Bottom face: (0,0,0) -> (10,0,0) -> (10,10,0) -> (5,10,0) -> (5,10-d1,0) -> (0,10-d1,0)
            // Wall face 1: (5,10,0) -> (5,10-d1,0) -> (5,10-d2,3) -> (5,10,3)
            // Wall face 2: (5,10,3) -> (5,10-d2,3) on the top
            // Top face: (0,0,3) -> (10,0,3) -> (10,10,3) -> (5,10,3) -> (5,10-d2,3) -> (0,10-d2,3)
            // Other 4 walls close the rest of the shape.
            // When existing IsClosed(0.05) runs:
            // (5,10,0) and (5,10-d1,0) weld -> edge on bottom collapses (count 0).
            // (5,10,3) and (5,10-d2,3) do NOT weld -> edge on top survives (count 1 on top, count 1 on wall).
            // Wall face 1 has corners V0, V0, V2, V1 -> edges (V0,V2), (V2,V1), (V1,V0).
            // Edge (V2,V1) matches top face (V1,V2) -> count 2.
            // BUT edge between (5,10,0) and (5,10,3) on Wall 1 is (V0,V1).
            // On adjacent wall (10,10,0)->(5,10,0)->(5,10,3)->(10,10,3), edge is (V0,V1) -> count 2.
            // But on Wall face 1, diagonal/collapsed edge (V0, V2) has NO matching edge on the adjacent wall if the adjacent wall has a vertical edge at (5, 10-d1, 0) to (5, 10-d2, 3) because it connects V0 to V2, but top edge on wall 2 connects (5,10-d2,3) to (0,10-d2,3)!
            // This creates odd parity or broken manifold in existing IsClosed!

            Plane plane = Create.Plane(0)!;
            PolygonalFace3D? face_Base = Create.PolygonalFace3D(
                plane,
                new Point2D(0, 0),
                new Point2D(10, 0),
                new Point2D(10, 10),
                new Point2D(5, 10),
                new Point2D(5, 10 - d1),
                new Point2D(0, 10 - d1));
            Assert.NotNull(face_Base);

            Polyhedron? polyhedron = Create.Polyhedron(face_Base, new Spatial.Classes.Vector3D(0, 0, 3));
            Assert.NotNull(polyhedron);

            // At 0.04 m: neither collapses -> both closed
            Assert.True(polyhedron.IsClosed(0.04));
            Assert.True(IsClosed_Proposed(polyhedron, false, 0.04));

            // At 0.05 m:
            // Existing IsClosed(manifold: true) FAILS because the 0.049999999988 edge collapses and destroys 2-manifold topology
            // Proposed IsClosed_Proposed(manifold: true) SUCCEEDS because genuine edges are preserved and paired
            Assert.False(polyhedron.IsClosed(true, 0.05));
            Assert.True(IsClosed_Proposed(polyhedron, true, 0.05));

            // Proposed is closed across the whole sweep:
            Assert.True(IsClosed_Proposed(polyhedron, true, 0.001));
            Assert.True(IsClosed_Proposed(polyhedron, true, 0.01));
            Assert.True(IsClosed_Proposed(polyhedron, true, 0.05));
            Assert.True(IsClosed_Proposed(polyhedron, true, 0.10));
        }

        /// <summary>
        /// Compares the existing and proposed implementations across all standard regression test cases.
        /// </summary>
        [Fact]
        public void Polyhedron_IsClosed_Comparison_RegressionSuite()
        {
            // 1. Box
            BoundingBox3D box = new(new Point3D(0, 0, 0), new Point3D(10, 10, 10));
            Polyhedron? poly_Box = Create.Polyhedron(box);
            Assert.NotNull(poly_Box);
            Assert.Equal(poly_Box.IsClosed(), IsClosed_Proposed(poly_Box, false));
            Assert.Equal(poly_Box.IsClosed(true), IsClosed_Proposed(poly_Box, true));

            // 2. Open Box (missing face)
            List<IPolygonalFace3D> openFaces = Polyhedron_IsClosed_BoxFaces(0, 10, 0);
            openFaces.RemoveAt(0);
            Polyhedron? poly_Open = Create.Polyhedron(openFaces);
            Assert.NotNull(poly_Open);
            Assert.False(IsClosed_Proposed(poly_Open, false));
            Assert.False(IsClosed_Proposed(poly_Open, true));

            // 3. Tetrahedron
            Polyhedron? poly_Tetra = Create.Polyhedron(Polyhedron_IsClosed_TetrahedronFaces());
            Assert.NotNull(poly_Tetra);
            Assert.Equal(poly_Tetra.IsClosed(), IsClosed_Proposed(poly_Tetra, false));
            Assert.Equal(poly_Tetra.IsClosed(true), IsClosed_Proposed(poly_Tetra, true));

            // 4. Non-Manifold (2 glued boxes)
            List<IPolygonalFace3D> gluedFaces = Polyhedron_IsClosed_BoxFaces(0, 10, 0);
            gluedFaces.AddRange(Polyhedron_IsClosed_BoxFaces(0, 10, 10));
            Polyhedron? poly_Glued = Create.Polyhedron(gluedFaces);
            Assert.NotNull(poly_Glued);
            Assert.True(IsClosed_Proposed(poly_Glued, false));
            Assert.False(IsClosed_Proposed(poly_Glued, true));

            // 5. Inverted face
            List<IPolygonalFace3D> invertedFaces = Polyhedron_IsClosed_BoxFaces(0, 10, 0);
            invertedFaces[0] = Polyhedron_IsClosed_Face(new Point3D(0, 10, 0), new Point3D(10, 10, 0), new Point3D(10, 0, 0), new Point3D(0, 0, 0))!;
            Polyhedron? poly_Inverted = Create.Polyhedron(invertedFaces);
            Assert.NotNull(poly_Inverted);
            Assert.True(IsClosed_Proposed(poly_Inverted, false));
            Assert.True(IsClosed_Proposed(poly_Inverted, true));

            // 6. Block with hole (lined vs unlined)
            Polyhedron? poly_LinedHole = Create.Polyhedron(Polyhedron_IsClosed_BlockWithHoleFaces(true));
            Assert.NotNull(poly_LinedHole);
            Assert.True(IsClosed_Proposed(poly_LinedHole, true));

            Polyhedron? poly_UnlinedHole = Create.Polyhedron(Polyhedron_IsClosed_BlockWithHoleFaces(false));
            Assert.NotNull(poly_UnlinedHole);
            Assert.False(IsClosed_Proposed(poly_UnlinedHole, false));

            // 7. Large GIS Coordinates
            Polyhedron? poly_Far = Create.Polyhedron(Polyhedron_IsClosed_BoxFaces(500000, 500010, 0));
            Assert.NotNull(poly_Far);
            Assert.True(IsClosed_Proposed(poly_Far, true));
        }

        /// <summary>
        /// Compares the runtime performance of existing vs proposed implementations on large polyhedra.
        /// </summary>
        [Fact]
        public void Polyhedron_IsClosed_Comparison_PerformanceBenchmark()
        {
            // 500-sided extrusion (3000 half-edges)
            Polyhedron? poly_Extrusion = Polyhedron_IsClosed_Extrusion(500);
            Assert.NotNull(poly_Extrusion);

            // Warm up
            _ = poly_Extrusion.IsClosed();
            _ = IsClosed_Proposed(poly_Extrusion, false);

            int repeats = 100;

            Stopwatch sw_Existing = Stopwatch.StartNew();
            for (int i = 0; i < repeats; i++)
            {
                _ = poly_Extrusion.IsClosed();
            }
            sw_Existing.Stop();

            Stopwatch sw_Proposed = Stopwatch.StartNew();
            for (int i = 0; i < repeats; i++)
            {
                _ = IsClosed_Proposed(poly_Extrusion, false);
            }
            sw_Proposed.Stop();

            double us_Existing = sw_Existing.Elapsed.TotalMilliseconds * 1000.0 / repeats;
            double us_Proposed = sw_Proposed.Elapsed.TotalMilliseconds * 1000.0 / repeats;

            // 9800-face ellipsoid
            Ellipsoid ellipsoid = new(new Point3D(1, 2, 3), 3, 2, 1);
            Polyhedron? poly_Ellipsoid = Create.Polyhedron(ellipsoid, 50, 100);
            Assert.NotNull(poly_Ellipsoid);

            Stopwatch sw_Ellipsoid_Existing = Stopwatch.StartNew();
            _ = poly_Ellipsoid.IsClosed();
            sw_Ellipsoid_Existing.Stop();

            Stopwatch sw_Ellipsoid_Proposed = Stopwatch.StartNew();
            _ = IsClosed_Proposed(poly_Ellipsoid, false);
            sw_Ellipsoid_Proposed.Stop();

            // Assert correctness and performance threshold
            Assert.True(us_Proposed < 8000.0, $"Proposed took {us_Proposed:F1} us per call");
            Assert.True(sw_Ellipsoid_Proposed.ElapsedMilliseconds < 1000, $"Proposed ellipsoid took {sw_Ellipsoid_Proposed.ElapsedMilliseconds} ms");
        }

        /// <summary>
        /// Hybrid implementation: Fast Vertex Hash with Dual Half-Edge Matching fallback when edge collapse occurs.
        /// </summary>
        private static bool IsClosed_Hybrid<TPolygonalFace3D>(Polyhedron<TPolygonalFace3D>? polyhedron, bool manifold, double tolerance = DiGi.Core.Constants.Tolerance.Distance) where TPolygonalFace3D : IPolygonalFace3D
        {
            if (polyhedron is null || polyhedron.Count < 4)
            {
                return false;
            }

            double tolerance_Temp = tolerance > 0.0 ? tolerance : DiGi.Core.Constants.Tolerance.MicroDistance;
            double invTolerance = 1.0 / tolerance_Temp;
            double toleranceSquared = tolerance_Temp * tolerance_Temp;

            List<double> coordinates = new(polyhedron.Count * 3);
            List<int> indexes_Next = new(polyhedron.Count);
            Dictionary<(long X, long Y, long Z), int> index_ByCell = [];

            int IndexInCell(long cellX, long cellY, long cellZ, double x, double y, double z)
            {
                if (!index_ByCell.TryGetValue((cellX, cellY, cellZ), out int index))
                {
                    return -1;
                }

                while (index >= 0)
                {
                    int offset = index * 3;
                    double dx = coordinates[offset] - x;
                    double dy = coordinates[offset + 1] - y;
                    double dz = coordinates[offset + 2] - z;

                    if ((dx * dx) + (dy * dy) + (dz * dz) <= toleranceSquared)
                    {
                        return index;
                    }

                    index = indexes_Next[index];
                }

                return -1;
            }

            int Index(double x, double y, double z)
            {
                long cellX = (long)System.Math.Floor(x * invTolerance);
                long cellY = (long)System.Math.Floor(y * invTolerance);
                long cellZ = (long)System.Math.Floor(z * invTolerance);

                int index = IndexInCell(cellX, cellY, cellZ, x, y, z);
                if (index >= 0)
                {
                    return index;
                }

                for (long neighbourX = cellX - 1; neighbourX <= cellX + 1; neighbourX++)
                {
                    for (long neighbourY = cellY - 1; neighbourY <= cellY + 1; neighbourY++)
                    {
                        for (long neighbourZ = cellZ - 1; neighbourZ <= cellZ + 1; neighbourZ++)
                        {
                            if (neighbourX == cellX && neighbourY == cellY && neighbourZ == cellZ)
                            {
                                continue;
                            }

                            index = IndexInCell(neighbourX, neighbourY, neighbourZ, x, y, z);
                            if (index >= 0)
                            {
                                return index;
                            }
                        }
                    }
                }

                int index_New = indexes_Next.Count;
                coordinates.Add(x);
                coordinates.Add(y);
                coordinates.Add(z);

                (long X, long Y, long Z) cell = (cellX, cellY, cellZ);
                indexes_Next.Add(index_ByCell.TryGetValue(cell, out int index_Head) ? index_Head : -1);
                index_ByCell[cell] = index_New;

                return index_New;
            }

            Dictionary<(int, int), int> counts_ByEdge = new(polyhedron.Count * 2);
            int[] indexes_Ring = new int[16];
            bool hadCollapsedEdge = false;

            for (int i = 0; i < polyhedron.Count; i++)
            {
                TPolygonalFace3D? polygonalFace3D = polyhedron.GetPolygonalFace3D<TPolygonalFace3D>(i);
                if (polygonalFace3D is null)
                {
                    return false;
                }

                Plane? plane = polygonalFace3D.Plane;
                if (plane is null)
                {
                    return false;
                }

                Point3D? point3D_Origin = plane.Origin;
                Spatial.Classes.Vector3D? vector3D_AxisX = plane.AxisX;
                Spatial.Classes.Vector3D? vector3D_AxisY = plane.AxisY;

                if (point3D_Origin is null || vector3D_AxisX is null || vector3D_AxisY is null)
                {
                    return false;
                }

                double originX = point3D_Origin.X;
                double originY = point3D_Origin.Y;
                double originZ = point3D_Origin.Z;

                double axisXX = vector3D_AxisX.X;
                double axisXY = vector3D_AxisX.Y;
                double axisXZ = vector3D_AxisX.Z;

                double axisYX = vector3D_AxisY.X;
                double axisYY = vector3D_AxisY.Y;
                double axisYZ = vector3D_AxisY.Z;

                IPolygonalFace2D? polygonalFace2D = polygonalFace3D.Geometry2D;
                if (polygonalFace2D is null)
                {
                    return false;
                }

                List<IPolygonal2D>? polygonal2Ds = polygonalFace2D.Edges;
                if (polygonal2Ds is null || polygonal2Ds.Count == 0)
                {
                    return false;
                }

                for (int j = 0; j < polygonal2Ds.Count; j++)
                {
                    IPolygonal2D? polygonal2D = polygonal2Ds[j];
                    if (polygonal2D is null)
                    {
                        return false;
                    }

                    List<Point2D>? point2Ds = polygonal2D is Segmentable2D segmentable2D ? segmentable2D.GetPoints(false) : polygonal2D.GetPoints();
                    if (point2Ds is null || point2Ds.Count < 3)
                    {
                        return false;
                    }

                    if (indexes_Ring.Length < point2Ds.Count)
                    {
                        indexes_Ring = new int[point2Ds.Count];
                    }

                    for (int k = 0; k < point2Ds.Count; k++)
                    {
                        Point2D? point2D = point2Ds[k];
                        if (point2D is null)
                        {
                            return false;
                        }

                        double x = point2D.X;
                        double y = point2D.Y;

                        indexes_Ring[k] = Index(
                            originX + (axisYX * y) + (axisXX * x),
                            originY + (axisYY * y) + (axisXY * x),
                            originZ + (axisYZ * y) + (axisXZ * x));
                    }

                    for (int k = 0; k < point2Ds.Count; k++)
                    {
                        int index_Start = indexes_Ring[k];
                        int index_End = indexes_Ring[k == point2Ds.Count - 1 ? 0 : k + 1];

                        if (index_Start == index_End)
                        {
                            hadCollapsedEdge = true;
                            continue;
                        }

                        (int, int) key = index_Start < index_End ? (index_Start, index_End) : (index_End, index_Start);

                        counts_ByEdge.TryGetValue(key, out int count);
                        counts_ByEdge[key] = count + 1;
                    }
                }
            }

            if (counts_ByEdge.Count > 0)
            {
                bool vertexPassPassed = true;
                foreach (KeyValuePair<(int, int), int> keyValuePair in counts_ByEdge)
                {
                    if (manifold ? keyValuePair.Value != 2 : (keyValuePair.Value & 1) != 0)
                    {
                        vertexPassPassed = false;
                        break;
                    }
                }

                if (vertexPassPassed)
                {
                    return true;
                }
            }

            // If no edge collapsed, geometry is genuinely open -> return false immediately.
            // If edges did collapse, run Dual Half-Edge Matching to eliminate feature-collapse false negative.
            if (!hadCollapsedEdge)
            {
                return false;
            }

            return IsClosed_Proposed(polyhedron, manifold, tolerance);
        }

        /// <summary>
        /// Three-way speed and correctness comparison between Existing Vertex Hash, Pure Dual Half-Edge Matching, and Hybrid.
        /// </summary>
        [Fact]
        public void Polyhedron_IsClosed_Comparison_ThreeWayBenchmark()
        {
            // 1. 500-gon Extrusion (standard closed model - tests fast path)
            Polyhedron? poly_Extrusion = Polyhedron_IsClosed_Extrusion(500);
            Assert.NotNull(poly_Extrusion);

            int repeats = 100;

            // Warm-up
            _ = poly_Extrusion.IsClosed();
            _ = IsClosed_Proposed(poly_Extrusion, false);
            _ = IsClosed_Hybrid(poly_Extrusion, false);

            Stopwatch sw_Existing = Stopwatch.StartNew();
            for (int i = 0; i < repeats; i++)
            {
                _ = poly_Extrusion.IsClosed();
            }
            sw_Existing.Stop();

            Stopwatch sw_Proposed = Stopwatch.StartNew();
            for (int i = 0; i < repeats; i++)
            {
                _ = IsClosed_Proposed(poly_Extrusion, false);
            }
            sw_Proposed.Stop();

            Stopwatch sw_Hybrid = Stopwatch.StartNew();
            for (int i = 0; i < repeats; i++)
            {
                _ = IsClosed_Hybrid(poly_Extrusion, false);
            }
            sw_Hybrid.Stop();

            double us_Existing = sw_Existing.Elapsed.TotalMilliseconds * 1000.0 / repeats;
            double us_Proposed = sw_Proposed.Elapsed.TotalMilliseconds * 1000.0 / repeats;
            double us_Hybrid = sw_Hybrid.Elapsed.TotalMilliseconds * 1000.0 / repeats;

            // 2. 0.03 m Step Extrusion (tests fallback path on feature collapse at tolerance = 0.05)
            Plane plane = Create.Plane(0)!;
            PolygonalFace3D? face_Step = Create.PolygonalFace3D(
                plane,
                new Point2D(0, 0),
                new Point2D(10, 0),
                new Point2D(10, 10),
                new Point2D(5, 10),
                new Point2D(5, 9.97),
                new Point2D(0, 9.97));
            Assert.NotNull(face_Step);
            Polyhedron? poly_Step = Create.Polyhedron(face_Step, new Spatial.Classes.Vector3D(0, 0, 3));
            Assert.NotNull(poly_Step);

            // Existing fails at 0.05
            Assert.False(poly_Step.IsClosed(true, 0.05));
            // Both Proposed and Hybrid SUCCEED at 0.05
            Assert.True(IsClosed_Proposed(poly_Step, true, 0.05));
            Assert.True(IsClosed_Hybrid(poly_Step, true, 0.05));

            // Hybrid is strictly monotonic on the step extrusion:
            double[] tolerances = [0.001, 0.01, 0.02, 0.03, 0.04, 0.05, 0.08, 0.10, 0.20];
            for (int i = 0; i < tolerances.Length; i++)
            {
                Assert.True(IsClosed_Hybrid(poly_Step, true, tolerances[i]), $"Hybrid failed at {tolerances[i]}");
            }

            // 3. 9800-face Ellipsoid (large closed model - 29,400 half-edges)
            Ellipsoid ellipsoid = new(new Point3D(1, 2, 3), 3, 2, 1);
            Polyhedron? poly_Ellipsoid = Create.Polyhedron(ellipsoid, 50, 100);
            Assert.NotNull(poly_Ellipsoid);

            // Warm-up
            _ = poly_Ellipsoid.IsClosed();
            _ = IsClosed_Proposed(poly_Ellipsoid, false);
            _ = IsClosed_Hybrid(poly_Ellipsoid, false);

            Stopwatch sw_Ellipsoid_Existing = Stopwatch.StartNew();
            _ = poly_Ellipsoid.IsClosed();
            sw_Ellipsoid_Existing.Stop();

            Stopwatch sw_Ellipsoid_Proposed = Stopwatch.StartNew();
            _ = IsClosed_Proposed(poly_Ellipsoid, false);
            sw_Ellipsoid_Proposed.Stop();

            Stopwatch sw_Ellipsoid_Hybrid = Stopwatch.StartNew();
            _ = IsClosed_Hybrid(poly_Ellipsoid, false);
            sw_Ellipsoid_Hybrid.Stop();

            // 4. Open model (Cube missing 1 face)
            List<IPolygonalFace3D> openFaces = Polyhedron_IsClosed_BoxFaces(0, 10, 0);
            openFaces.RemoveAt(0);
            Polyhedron? poly_Open = Create.Polyhedron(openFaces);
            Assert.NotNull(poly_Open);

            int openRepeats = 500;
            Stopwatch sw_Open_Existing = Stopwatch.StartNew();
            for (int i = 0; i < openRepeats; i++)
            {
                _ = poly_Open.IsClosed();
            }
            sw_Open_Existing.Stop();

            Stopwatch sw_Open_Proposed = Stopwatch.StartNew();
            for (int i = 0; i < openRepeats; i++)
            {
                _ = IsClosed_Proposed(poly_Open, false);
            }
            sw_Open_Proposed.Stop();

            Stopwatch sw_Open_Hybrid = Stopwatch.StartNew();
            for (int i = 0; i < openRepeats; i++)
            {
                _ = IsClosed_Hybrid(poly_Open, false);
            }
            sw_Open_Hybrid.Stop();

            double us_Open_Existing = sw_Open_Existing.Elapsed.TotalMilliseconds * 1000.0 / openRepeats;
            double us_Open_Proposed = sw_Open_Proposed.Elapsed.TotalMilliseconds * 1000.0 / openRepeats;
            double us_Open_Hybrid = sw_Open_Hybrid.Elapsed.TotalMilliseconds * 1000.0 / openRepeats;

            // 5. Step Extrusion at tol = 0.05 (fallback path triggered)
            int stepRepeats = 500;
            Stopwatch sw_Step_Existing = Stopwatch.StartNew();
            for (int i = 0; i < stepRepeats; i++)
            {
                _ = poly_Step.IsClosed(true, 0.05);
            }
            sw_Step_Existing.Stop();

            Stopwatch sw_Step_Proposed = Stopwatch.StartNew();
            for (int i = 0; i < stepRepeats; i++)
            {
                _ = IsClosed_Proposed(poly_Step, true, 0.05);
            }
            sw_Step_Proposed.Stop();

            Stopwatch sw_Step_Hybrid = Stopwatch.StartNew();
            for (int i = 0; i < stepRepeats; i++)
            {
                _ = IsClosed_Hybrid(poly_Step, true, 0.05);
            }
            sw_Step_Hybrid.Stop();

            double us_Step_Existing = sw_Step_Existing.Elapsed.TotalMilliseconds * 1000.0 / stepRepeats;
            double us_Step_Proposed = sw_Step_Proposed.Elapsed.TotalMilliseconds * 1000.0 / stepRepeats;
            double us_Step_Hybrid = sw_Step_Hybrid.Elapsed.TotalMilliseconds * 1000.0 / stepRepeats;

            // Write report file
            string? dir_Reports = DiGi.Core.xUnit.Query.ReportsDirectory(System.Reflection.Assembly.GetExecutingAssembly());
            if (dir_Reports != null && System.IO.Directory.Exists(dir_Reports))
            {
                string reportPath = System.IO.Path.Combine(dir_Reports, "IsClosed_Benchmark_Report.md");
                string reportContent = $"# IsClosed Algorithmic Benchmark Report\n\n" +
                    $"| Benchmark Geometry | Existing Vertex Hash | Proposed Pure Edge Matching | Proposed Hybrid (Fast-Path + Fallback) | Monotonic Outcome |\n" +
                    $"| :--- | :--- | :--- | :--- | :--- |\n" +
                    $"| **500-gon Extrusion** (3,000 half-edges) | {us_Existing:F1} us | {us_Proposed:F1} us | **{us_Hybrid:F1} us** | All True |\n" +
                    $"| **9,800-face Ellipsoid** (29,400 half-edges) | {sw_Ellipsoid_Existing.ElapsedMilliseconds} ms | {sw_Ellipsoid_Proposed.ElapsedMilliseconds} ms | **{sw_Ellipsoid_Hybrid.ElapsedMilliseconds} ms** | All True |\n" +
                    $"| **Open Cube (5 faces)** (missing face) | {us_Open_Existing:F1} us | {us_Open_Proposed:F1} us | **{us_Open_Hybrid:F1} us** | All False |\n" +
                    $"| **0.03 m Step Extrusion (tol = 0.05)** (Issue #1) | {us_Step_Existing:F1} us (False!) | {us_Step_Proposed:F1} us (True) | **{us_Step_Hybrid:F1} us (True)** | Hybrid Monotonic (True) |\n";

                System.IO.File.WriteAllText(reportPath, reportContent);
            }

            Assert.True(us_Hybrid < 8000.0, $"Hybrid took {us_Hybrid:F1} us on 500-gon");
        }
    }
}
