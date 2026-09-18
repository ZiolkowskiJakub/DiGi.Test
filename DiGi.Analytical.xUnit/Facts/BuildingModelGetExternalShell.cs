using DiGi.Analytical.Building.Classes;
using DiGi.Analytical.Building.Interfaces;
using DiGi.Analytical.Classes;
using DiGi.Core.Classes;
using DiGi.Core.Interfaces;
using DiGi.Geometry.Core.Enums;
using DiGi.Geometry.Spatial;
using DiGi.Geometry.Spatial.Classes;
using System.Diagnostics;
using System.Reflection;

namespace DiGi.Analytical.xUnit
{
    public partial class Facts
    {
        /// <summary>
        /// Tests that <see cref="BuildingModel.GetExternalShell(Side?, Orientation?, Orientation?, double)"/> turns a wall whose stored face points into the building outward.
        /// <para>The box is closed by six components around one space, and one wall is wound the wrong way so its stored normal points inward. Asked for no side, the method hands the wall back as stored - which is what shows the outward assertion can fail - and asked for <see cref="Side.External"/> it flips exactly that wall, leaves the other five as they are, and every face then carries the reference of its component.</para>
        /// </summary>
        [Fact]
        public void BuildingModelGetExternalShell_InwardNormal()
        {
            List<(IComponent Component, Vector3D Normal)> components = BuildingModelGetExternalShell_Box(0, 0, 0, 10, 10, 3, [2]);

            Space space = new(new Point3D(5, 5, 1.5), "Space");

            BuildingModel buildingModel = new();
            foreach ((IComponent component, Vector3D _) in components)
            {
                Assert.True(buildingModel.Assign(component, space));
            }

            // Without a side the stored orientation is kept, so the reversed wall still points inward: the guard below is a real one.
            Shell? shell_Stored = buildingModel.GetExternalShell();
            Assert.NotNull(shell_Stored);
            Assert.Equal(6, shell_Stored.Count);

            Dictionary<Guid, Vector3D> normals_Stored = BuildingModelGetExternalShell_Normals(shell_Stored);
            Assert.True(normals_Stored[components[2].Component.Guid] * components[2].Normal < 0, "The reversed wall was not stored inward, so the fixture proves nothing.");
            Assert.True(normals_Stored[components[0].Component.Guid] * components[0].Normal > 0);

            Shell? shell = buildingModel.GetExternalShell(Side.External);
            Assert.NotNull(shell);
            Assert.Equal(6, shell.Count);
            Assert.True(shell.IsClosed());

            BuildingModelGetExternalShell_AssertOutward(shell, components);
            BuildingModelGetExternalShell_AssertProbes(shell, 0.1, Core.Constants.Tolerance.Distance);

            PolyhedronNormalizationUpdater<Shell> polyhedronNormalizationUpdater = new(Side.External, null, null)
            {
                Value = shell
            };
            Assert.True(polyhedronNormalizationUpdater.Normalized());

            IUniqueReference? uniqueReference_Shell = shell.UniqueReference;
            Assert.True(uniqueReference_Shell is GuidReference guidReference_Shell && guidReference_Shell.Guid == buildingModel.Guid);

            // The model is left as it was: the wall it stores still points inward.
            List<IWall>? walls = buildingModel.GetComponents<IWall>();
            Assert.NotNull(walls);
            IWall? wall_Reversed = walls.Find(x => x.Guid == components[2].Component.Guid);
            Assert.NotNull(wall_Reversed);
            Vector3D? normal_Model = Building.Query.Geometry3D<PolygonalFace3D>(wall_Reversed)?.Plane?.Normal;
            Assert.NotNull(normal_Model);
            Assert.True(normal_Model * components[2].Normal < 0);
        }

        /// <summary>
        /// Tests the outward orientation of a courtyard building, the concave case where a centroid heuristic fails.
        /// <para>The envelope is a ring - a 20 by 20 footprint with a 10 by 10 hole in the middle, three metres high - so the floor and the roof carry a hole and four of the ten walls face the courtyard. Those four are stored inward. The centroid of the model sits in the courtyard, on the wrong side of every courtyard wall, so "away from the centroid" gives the wrong answer for them; ray parity over the closed envelope gives the right one for all ten faces.</para>
        /// </summary>
        [Fact]
        public void BuildingModelGetExternalShell_Courtyard()
        {
            List<(IComponent Component, Vector3D Normal)> components = BuildingModelGetExternalShell_Ring(0, 0, 0, 20, 20, 3, 5);

            Space space = new(new Point3D(2.5, 10, 1.5), "Space");

            BuildingModel buildingModel = new();
            foreach ((IComponent component, Vector3D _) in components)
            {
                Assert.True(buildingModel.Assign(component, space));
            }

            Shell? shell = buildingModel.GetExternalShell(Side.External);
            Assert.NotNull(shell);
            Assert.Equal(10, shell.Count);
            Assert.True(shell.IsClosed());

            BuildingModelGetExternalShell_AssertOutward(shell, components);
            BuildingModelGetExternalShell_AssertProbes(shell, 0.1, Core.Constants.Tolerance.Distance);

            // The heuristic the issue names would file every courtyard wall the wrong way round.
            Point3D? centroid = buildingModel.GetBoundingBox()?.GetCentroid();
            Assert.NotNull(centroid);

            List<Face>? faces = shell.PolygonalFaces;
            Assert.NotNull(faces);

            int count_CourtyardWalls = 0;
            foreach (Face face in faces)
            {
                Point3D? point3D = face.GetInternalPoint();
                Vector3D? normal = face.Plane?.Normal;
                Assert.NotNull(point3D);
                Assert.NotNull(normal);

                // Courtyard walls are the vertical faces whose internal point lies within the 5 metre ring of the hole.
                if (Math.Abs(normal.Z) > Core.Constants.Tolerance.Distance || point3D.X < 5 - Core.Constants.Tolerance.Distance || point3D.X > 15 + Core.Constants.Tolerance.Distance || point3D.Y < 5 - Core.Constants.Tolerance.Distance || point3D.Y > 15 + Core.Constants.Tolerance.Distance)
                {
                    continue;
                }

                count_CourtyardWalls++;
                Assert.True(normal * (point3D - centroid) < 0, "A courtyard wall does not disagree with the centroid heuristic, so the fixture proves nothing.");
            }

            Assert.Equal(4, count_CourtyardWalls);
        }

        /// <summary>
        /// Tests which components reach the envelope: every component bounding exactly one space, exactly once, and nothing else.
        /// <para>Two boxes are stacked into two spaces. The slab between them bounds both spaces and is an internal partition, and a stray floor stored without any space bounds none; neither may appear. The ten faces that remain - the lower floor, eight walls and the upper roof - close the envelope, and the four upper walls are stored inward to show they are oriented over that envelope rather than left as stored.</para>
        /// </summary>
        [Fact]
        public void BuildingModelGetExternalShell_Coverage()
        {
            List<(IComponent Component, Vector3D Normal)> components_Lower = BuildingModelGetExternalShell_Box(0, 0, 0, 10, 10, 3, []);
            List<(IComponent Component, Vector3D Normal)> components_Upper = BuildingModelGetExternalShell_Box(0, 0, 3, 10, 10, 6, [2, 3, 4, 5]);

            Space space_Lower = new(new Point3D(5, 5, 1.5), "Lower");
            Space space_Upper = new(new Point3D(5, 5, 4.5), "Upper");

            BuildingModel buildingModel = new();

            // The lower box keeps its floor and walls; its roof becomes the slab shared with the upper space.
            for (int i = 0; i < components_Lower.Count; i++)
            {
                if (i == 1)
                {
                    continue;
                }

                Assert.True(buildingModel.Assign(components_Lower[i].Component, space_Lower));
            }

            FaceFloor slab = new(BuildingModel_Footprints_Face([new Point3D(0, 0, 3), new Point3D(10, 0, 3), new Point3D(10, 10, 3), new Point3D(0, 10, 3)]));
            Assert.True(buildingModel.Assign(slab, space_Lower, space_Upper));

            // The upper box keeps its walls and roof; its floor is the slab.
            for (int i = 0; i < components_Upper.Count; i++)
            {
                if (i == 0)
                {
                    continue;
                }

                Assert.True(buildingModel.Assign(components_Upper[i].Component, space_Upper));
            }

            FaceFloor orphan = new(BuildingModel_Footprints_Face([new Point3D(0, 0, -1), new Point3D(0, 10, -1), new Point3D(10, 10, -1), new Point3D(10, 0, -1)]));
            Assert.True(buildingModel.Update(orphan));

            Shell? shell = buildingModel.GetExternalShell(Side.External);
            Assert.NotNull(shell);
            Assert.Equal(10, shell.Count);
            Assert.True(shell.IsClosed());

            List<(IComponent Component, Vector3D Normal)> components_External = [];
            components_External.AddRange(components_Lower.Where((x, i) => i != 1));
            components_External.AddRange(components_Upper.Where((x, i) => i != 0));
            Assert.Equal(10, components_External.Count);

            BuildingModelGetExternalShell_AssertOutward(shell, components_External);
            BuildingModelGetExternalShell_AssertProbes(shell, 0.1, Core.Constants.Tolerance.Distance);

            Dictionary<Guid, Vector3D> normals = BuildingModelGetExternalShell_Normals(shell);
            Assert.False(normals.ContainsKey(slab.Guid), "The slab bounds two spaces and must not reach the envelope.");
            Assert.False(normals.ContainsKey(orphan.Guid), "A component bounding no space must not reach the envelope.");
        }

        /// <summary>
        /// Tests the cases that yield no envelope: a model without any space relation, a space bounded only by non-polygonal geometry, and fewer than the four faces a closed solid needs.
        /// <para>The second case is where <see cref="BuildingModel.GetShells{TSpace}(IEnumerable{TSpace}, Side?, Orientation?, Orientation?, double)"/> throws; the envelope does not visit spaces, so it answers <see langword="null"/> instead.</para>
        /// </summary>
        [Fact]
        public void BuildingModelGetExternalShell_Null()
        {
            BuildingModel buildingModel_Empty = new();
            Assert.Null(buildingModel_Empty.GetExternalShell());
            Assert.Null(buildingModel_Empty.GetExternalShell(Side.External));

            Space space_Air = new(new Point3D(5, 5, 0), "Air");
            BuildingModel buildingModel_Air = new();
            Assert.True(buildingModel_Air.Assign(new PointAir(new Point3D(5, 5, 0)), space_Air));
            Assert.Null(buildingModel_Air.GetExternalShell(Side.External));
            Assert.Throws<InvalidOperationException>(() => buildingModel_Air.GetShells<Space>(Side.External));

            List<(IComponent Component, Vector3D Normal)> components = BuildingModelGetExternalShell_Box(0, 0, 0, 10, 10, 3, []);
            Space space_Open = new(new Point3D(5, 5, 1.5), "Open");
            BuildingModel buildingModel_Open = new();
            for (int i = 0; i < 3; i++)
            {
                Assert.True(buildingModel_Open.Assign(components[i].Component, space_Open));
            }

            Assert.Null(buildingModel_Open.GetExternalShell(Side.External));
        }

        /// <summary>
        /// Tests the envelope of the real building models held in the shared fixtures against the per-space path the consumers use today.
        /// <para>Three of the four stored models are single-space and the fourth is a two-storey split with one slab between its spaces. Over the shells of the spaces, a component on exactly one shell face is external and one on two is the partition, so the envelope has to hold exactly the former, with every normal on the same side as the space shell puts it, and none of the latter. The envelope also has to close at some rung of the tolerance ladder <see cref="Building.Query.IsEnclosed(BuildingModel?, bool, double)"/> uses, and a point just inside each face along its normal is inside the envelope while a point just outside is not.</para>
        /// </summary>
        [Fact]
        public void BuildingModelGetExternalShell_RealModels()
        {
            List<BuildingModel> buildingModels = [];
            foreach (string fileName in new string[] { "BuildingModel_ValidGeometry.json", "buildingmodel_5072294.json" })
            {
                List<BuildingModel>? buildingModels_File = BuildingModel_Footprints_File(fileName);
                Assert.NotNull(buildingModels_File);
                Assert.NotEmpty(buildingModels_File);

                buildingModels.AddRange(buildingModels_File);
            }

            Assert.Equal(4, buildingModels.Count);

            double[] tolerances = [Core.Constants.Tolerance.Distance, 1E-05, 0.0001, Core.Constants.Tolerance.MacroDistance, 0.01, 0.02, 0.05, 0.1, 0.2];

            List<string> lines = [];

            int count_MultiSpace = 0;
            int count_Probed = 0;
            foreach (BuildingModel buildingModel in buildingModels)
            {
                List<Space>? spaces = buildingModel.GetSpaces<Space>();
                Assert.NotNull(spaces);
                Assert.NotEmpty(spaces);

                Shell? shell = buildingModel.GetExternalShell(Side.External);
                Assert.NotNull(shell);

                List<Shell>? shells_Space = buildingModel.GetShells<Space>(Side.External);
                Assert.NotNull(shells_Space);
                Assert.Equal(spaces.Count, shells_Space.Count);

                // A component on one space shell is external, on two it is the partition between them.
                Dictionary<Guid, Vector3D> normals_External = [];
                HashSet<Guid> guids_Partition = [];
                foreach (Shell shell_Space in shells_Space)
                {
                    foreach (KeyValuePair<Guid, Vector3D> keyValuePair in BuildingModelGetExternalShell_Normals(shell_Space))
                    {
                        if (normals_External.Remove(keyValuePair.Key))
                        {
                            guids_Partition.Add(keyValuePair.Key);
                            continue;
                        }

                        normals_External.Add(keyValuePair.Key, keyValuePair.Value);
                    }
                }

                if (spaces.Count > 1)
                {
                    count_MultiSpace++;
                    Assert.NotEmpty(guids_Partition);
                }

                Dictionary<Guid, Vector3D> normals = BuildingModelGetExternalShell_Normals(shell);

                Assert.Equal(normals_External.Count, normals.Count);
                foreach (KeyValuePair<Guid, Vector3D> keyValuePair in normals_External)
                {
                    Assert.True(normals.TryGetValue(keyValuePair.Key, out Vector3D? normal), "A component on one space shell is missing from the envelope.");
                    Assert.True(normal * keyValuePair.Value > 0, "The envelope and the space shell disagree on the side of a face.");
                }

                foreach (Guid guid in guids_Partition)
                {
                    Assert.False(normals.ContainsKey(guid), "A component on two space shells reached the envelope.");
                }

                double? closingTolerance = shell.ClosingTolerance(tolerances);
                List<double?> closingTolerances_Space = shells_Space.ConvertAll(x => x.ClosingTolerance(tolerances));
                lines.Add(string.Format("Model {0}: spaces {1}, envelope faces {2}, partitions {3}, envelope closes at {4}, space shells close at {5}.", buildingModel.Guid, spaces.Count, shell.Count, guids_Partition.Count, closingTolerance?.ToString() ?? "none", string.Join(" / ", closingTolerances_Space.Select(x => x?.ToString() ?? "none"))));

                // Closed space shells give a closed envelope: dropping the partition leaves its two storeys pairing their edges with each other instead.
                // The converse does not hold - a storey split that never closed leaves the envelope open too - and such a model is recorded rather than probed.
                if (closingTolerances_Space.TrueForAll(x => x is not null))
                {
                    Assert.NotNull(closingTolerance);
                }

                if (closingTolerance is null)
                {
                    continue;
                }

                count_Probed++;
                BuildingModelGetExternalShell_AssertProbes(shell, 0.1, closingTolerance.Value);
            }

            string? path_Reports = Core.xUnit.Query.ReportsDirectory(Assembly.GetExecutingAssembly());
            Assert.False(string.IsNullOrWhiteSpace(path_Reports));
            File.WriteAllLines(System.IO.Path.Combine(path_Reports!, "BuildingModelGetExternalShell_RealModels.txt"), lines);

            Assert.Equal(1, count_MultiSpace);
            Assert.True(count_Probed >= 3, string.Format("Only {0} of the stored models closed, so the probes ran on too few of them.", count_Probed));
        }

        /// <summary>
        /// Measures the envelope of the number of building models the 3D view of the largest area it offers has to carry, against the per-space path in use today.
        /// <para>Both paths are warmed up over the whole set, then timed three times each over the same two thousand four hundred models in alternating order, and the ranges are written to the test reports directory. Two of the three stored models are single-space, where the two paths do the same work, and the third is a two-storey split, where the per-space path builds and orients its slab twice; the slowest envelope run is asserted against the threshold.</para>
        /// </summary>
        [Fact]
        public void BuildingModelGetExternalShell_Performance()
        {
            const int count_Models = 2400;

            List<BuildingModel>? buildingModels = BuildingModel_Footprints_File("BuildingModel_ValidGeometry.json");
            Assert.NotNull(buildingModels);
            Assert.NotEmpty(buildingModels);

            List<BuildingModel> buildingModels_Temp = [];
            while (buildingModels_Temp.Count < count_Models)
            {
                foreach (BuildingModel buildingModel in buildingModels)
                {
                    if (Core.Query.Clone(buildingModel) is BuildingModel buildingModel_Temp)
                    {
                        buildingModels_Temp.Add(buildingModel_Temp);
                    }
                }

                Assert.NotEmpty(buildingModels_Temp);
            }

            int count_Faces = 0;

            long Envelope()
            {
                count_Faces = 0;

                Stopwatch stopwatch = Stopwatch.StartNew();
                foreach (BuildingModel buildingModel in buildingModels_Temp)
                {
                    Shell? shell = buildingModel.GetExternalShell(Side.External);
                    Assert.NotNull(shell);
                    count_Faces += shell.Count;
                }
                stopwatch.Stop();

                return stopwatch.ElapsedMilliseconds;
            }

            long Spaces()
            {
                Stopwatch stopwatch = Stopwatch.StartNew();
                foreach (BuildingModel buildingModel in buildingModels_Temp)
                {
                    List<Shell>? shells = buildingModel.GetShells<Space>(Side.External);
                    Assert.NotNull(shells);
                }
                stopwatch.Stop();

                return stopwatch.ElapsedMilliseconds;
            }

            // Warm up both paths over the whole set, so tiered compilation of the shared geometry has settled before either is timed.
            _ = Envelope();
            _ = Spaces();

            List<long> durations_Envelope = [];
            List<long> durations_Spaces = [];

            // The order alternates, so neither path is always the one paying for a cold cache.
            for (int i = 0; i < 3; i++)
            {
                if (i % 2 == 0)
                {
                    durations_Envelope.Add(Envelope());
                    durations_Spaces.Add(Spaces());
                }
                else
                {
                    durations_Spaces.Add(Spaces());
                    durations_Envelope.Add(Envelope());
                }
            }

            string? path_Reports = Core.xUnit.Query.ReportsDirectory(Assembly.GetExecutingAssembly());
            Assert.False(string.IsNullOrWhiteSpace(path_Reports));

            List<string> lines =
            [
                string.Format("Building models {0}, envelope faces {1}, processors {2}.", buildingModels_Temp.Count, count_Faces, Environment.ProcessorCount),
                string.Format("GetExternalShell(Side.External): {0} ms ({1:F1} us per model).", string.Join(" / ", durations_Envelope), durations_Envelope.Max() * 1000.0 / buildingModels_Temp.Count),
                string.Format("GetShells<Space>(Side.External): {0} ms ({1:F1} us per model).", string.Join(" / ", durations_Spaces), durations_Spaces.Max() * 1000.0 / buildingModels_Temp.Count)
            ];

            File.WriteAllLines(System.IO.Path.Combine(path_Reports!, "BuildingModelGetExternalShell_Performance.txt"), lines);

            // Measured 2026-09-18 (Ryzen 9 9950X): 619-680 ms isolated in Release, 1536-1638 ms in-suite in Debug; the threshold clears the latter.
            Assert.True(durations_Envelope.Max() < 5000, string.Format("BuildingModel.GetExternalShell failed threshold! Elapsed: {0} ms.", durations_Envelope.Max()));
        }

        /// <summary>
        /// Builds the six components of an axis-aligned box wound outward, each paired with the outward normal it should end up with; the walls at the given indexes are wound the other way so their stored normal points inward.
        /// <para>Order: floor, roof, then the walls at the minimum Y, maximum X, maximum Y and minimum X.</para>
        /// </summary>
        private static List<(IComponent Component, Vector3D Normal)> BuildingModelGetExternalShell_Box(double xMin, double yMin, double zMin, double xMax, double yMax, double zMax, int[] indexes_Reversed)
        {
            List<(List<Point3D> Point3Ds, Vector3D Normal)> rings =
            [
                ([new Point3D(xMin, yMin, zMin), new Point3D(xMin, yMax, zMin), new Point3D(xMax, yMax, zMin), new Point3D(xMax, yMin, zMin)], new Vector3D(0, 0, -1)),
                ([new Point3D(xMin, yMin, zMax), new Point3D(xMax, yMin, zMax), new Point3D(xMax, yMax, zMax), new Point3D(xMin, yMax, zMax)], new Vector3D(0, 0, 1)),
                ([new Point3D(xMin, yMin, zMin), new Point3D(xMax, yMin, zMin), new Point3D(xMax, yMin, zMax), new Point3D(xMin, yMin, zMax)], new Vector3D(0, -1, 0)),
                ([new Point3D(xMax, yMin, zMin), new Point3D(xMax, yMax, zMin), new Point3D(xMax, yMax, zMax), new Point3D(xMax, yMin, zMax)], new Vector3D(1, 0, 0)),
                ([new Point3D(xMax, yMax, zMin), new Point3D(xMin, yMax, zMin), new Point3D(xMin, yMax, zMax), new Point3D(xMax, yMax, zMax)], new Vector3D(0, 1, 0)),
                ([new Point3D(xMin, yMax, zMin), new Point3D(xMin, yMin, zMin), new Point3D(xMin, yMin, zMax), new Point3D(xMin, yMax, zMax)], new Vector3D(-1, 0, 0))
            ];

            List<(IComponent Component, Vector3D Normal)> result = [];
            for (int i = 0; i < rings.Count; i++)
            {
                List<Point3D> point3Ds = rings[i].Point3Ds;
                if (indexes_Reversed.Contains(i))
                {
                    point3Ds.Reverse();
                }

                PolygonalFace3D polygonalFace3D = BuildingModel_Footprints_Face(point3Ds);

                IComponent component = i switch
                {
                    0 => new FaceFloor(polygonalFace3D),
                    1 => new SurfaceRoof(polygonalFace3D),
                    _ => new SurfaceWall(polygonalFace3D)
                };

                result.Add((component, rings[i].Normal));
            }

            return result;
        }

        /// <summary>
        /// Builds the ten components of a ring-shaped building - a box with a square courtyard cut through it - each paired with its outward normal. The four courtyard walls are wound inward.
        /// <para>Order: floor and roof (each with the courtyard as a hole), the four outer walls, then the four courtyard walls, whose outward side faces the courtyard.</para>
        /// </summary>
        private static List<(IComponent Component, Vector3D Normal)> BuildingModelGetExternalShell_Ring(double xMin, double yMin, double zMin, double xMax, double yMax, double zMax, double thickness)
        {
            double xMin_Hole = xMin + thickness;
            double yMin_Hole = yMin + thickness;
            double xMax_Hole = xMax - thickness;
            double yMax_Hole = yMax - thickness;

            List<(IComponent Component, Vector3D Normal)> result = [];

            Polygon3D? polygon3D_Floor = Geometry.Spatial.Create.Polygon3D([new Point3D(xMin, yMin, zMin), new Point3D(xMin, yMax, zMin), new Point3D(xMax, yMax, zMin), new Point3D(xMax, yMin, zMin)]);
            Polygon3D? polygon3D_Floor_Hole = Geometry.Spatial.Create.Polygon3D([new Point3D(xMin_Hole, yMin_Hole, zMin), new Point3D(xMin_Hole, yMax_Hole, zMin), new Point3D(xMax_Hole, yMax_Hole, zMin), new Point3D(xMax_Hole, yMin_Hole, zMin)]);
            Assert.NotNull(polygon3D_Floor);
            Assert.NotNull(polygon3D_Floor_Hole);

            PolygonalFace3D? polygonalFace3D_Floor = Geometry.Spatial.Create.PolygonalFace3D(polygon3D_Floor, [polygon3D_Floor_Hole]);
            Assert.NotNull(polygonalFace3D_Floor);
            result.Add((new FaceFloor(polygonalFace3D_Floor), new Vector3D(0, 0, -1)));

            Polygon3D? polygon3D_Roof = Geometry.Spatial.Create.Polygon3D([new Point3D(xMin, yMin, zMax), new Point3D(xMax, yMin, zMax), new Point3D(xMax, yMax, zMax), new Point3D(xMin, yMax, zMax)]);
            Polygon3D? polygon3D_Roof_Hole = Geometry.Spatial.Create.Polygon3D([new Point3D(xMin_Hole, yMin_Hole, zMax), new Point3D(xMax_Hole, yMin_Hole, zMax), new Point3D(xMax_Hole, yMax_Hole, zMax), new Point3D(xMin_Hole, yMax_Hole, zMax)]);
            Assert.NotNull(polygon3D_Roof);
            Assert.NotNull(polygon3D_Roof_Hole);

            PolygonalFace3D? polygonalFace3D_Roof = Geometry.Spatial.Create.PolygonalFace3D(polygon3D_Roof, [polygon3D_Roof_Hole]);
            Assert.NotNull(polygonalFace3D_Roof);
            result.Add((new SurfaceRoof(polygonalFace3D_Roof), new Vector3D(0, 0, 1)));

            // Outer walls wound outward.
            result.Add((new SurfaceWall(BuildingModel_Footprints_Face([new Point3D(xMin, yMin, zMin), new Point3D(xMax, yMin, zMin), new Point3D(xMax, yMin, zMax), new Point3D(xMin, yMin, zMax)])), new Vector3D(0, -1, 0)));
            result.Add((new SurfaceWall(BuildingModel_Footprints_Face([new Point3D(xMax, yMin, zMin), new Point3D(xMax, yMax, zMin), new Point3D(xMax, yMax, zMax), new Point3D(xMax, yMin, zMax)])), new Vector3D(1, 0, 0)));
            result.Add((new SurfaceWall(BuildingModel_Footprints_Face([new Point3D(xMax, yMax, zMin), new Point3D(xMin, yMax, zMin), new Point3D(xMin, yMax, zMax), new Point3D(xMax, yMax, zMax)])), new Vector3D(0, 1, 0)));
            result.Add((new SurfaceWall(BuildingModel_Footprints_Face([new Point3D(xMin, yMax, zMin), new Point3D(xMin, yMin, zMin), new Point3D(xMin, yMin, zMax), new Point3D(xMin, yMax, zMax)])), new Vector3D(-1, 0, 0)));

            // Courtyard walls: the outward side faces the courtyard, and they are wound the other way so their stored normal points into the building.
            result.Add((new SurfaceWall(BuildingModel_Footprints_Face([new Point3D(xMin_Hole, yMin_Hole, zMin), new Point3D(xMax_Hole, yMin_Hole, zMin), new Point3D(xMax_Hole, yMin_Hole, zMax), new Point3D(xMin_Hole, yMin_Hole, zMax)])), new Vector3D(0, 1, 0)));
            result.Add((new SurfaceWall(BuildingModel_Footprints_Face([new Point3D(xMax_Hole, yMin_Hole, zMin), new Point3D(xMax_Hole, yMax_Hole, zMin), new Point3D(xMax_Hole, yMax_Hole, zMax), new Point3D(xMax_Hole, yMin_Hole, zMax)])), new Vector3D(-1, 0, 0)));
            result.Add((new SurfaceWall(BuildingModel_Footprints_Face([new Point3D(xMax_Hole, yMax_Hole, zMin), new Point3D(xMin_Hole, yMax_Hole, zMin), new Point3D(xMin_Hole, yMax_Hole, zMax), new Point3D(xMax_Hole, yMax_Hole, zMax)])), new Vector3D(0, -1, 0)));
            result.Add((new SurfaceWall(BuildingModel_Footprints_Face([new Point3D(xMin_Hole, yMax_Hole, zMin), new Point3D(xMin_Hole, yMin_Hole, zMin), new Point3D(xMin_Hole, yMin_Hole, zMax), new Point3D(xMin_Hole, yMax_Hole, zMax)])), new Vector3D(1, 0, 0)));

            return result;
        }

        /// <summary>
        /// Reads the unit normal of every face of a shell, keyed by the identifier the face's component reference carries. Every face has to carry a <see cref="GuidReference"/> and no component may appear twice.
        /// </summary>
        private static Dictionary<Guid, Vector3D> BuildingModelGetExternalShell_Normals(Shell shell)
        {
            List<Face>? faces = shell.PolygonalFaces;
            Assert.NotNull(faces);

            Dictionary<Guid, Vector3D> result = [];
            foreach (Face face in faces)
            {
                // Face.UniqueReference is a fresh clone on every call: read it once and match on the identifier it carries.
                IUniqueReference? uniqueReference = face.UniqueReference;
                Assert.True(uniqueReference is GuidReference, "A face of the envelope carries no GuidReference of its component.");

                Vector3D? normal = face.Plane?.Normal?.Unit;
                Assert.NotNull(normal);

                Assert.True(result.TryAdd(((GuidReference)uniqueReference!).Guid, normal), "A component appears on two faces of the envelope.");
            }

            return result;
        }

        /// <summary>
        /// Asserts that every given component is on the shell exactly once and that its face normal points the way it is expected to.
        /// </summary>
        private static void BuildingModelGetExternalShell_AssertOutward(Shell shell, IEnumerable<(IComponent Component, Vector3D Normal)> components)
        {
            Dictionary<Guid, Vector3D> normals = BuildingModelGetExternalShell_Normals(shell);

            int count = 0;
            foreach ((IComponent component, Vector3D normal_Expected) in components)
            {
                count++;

                Assert.True(normals.TryGetValue(component.Guid, out Vector3D? normal), string.Format("The component {0} is missing from the envelope.", component.Guid));
                Assert.True(normal * normal_Expected > 0.99, string.Format("The face of the component {0} is not oriented outward.", component.Guid));
            }

            Assert.Equal(count, normals.Count);
        }

        /// <summary>
        /// Asserts, for every face of a closed shell, that a point moved from its internal point against the normal lies inside the shell and a point moved along the normal lies outside it.
        /// </summary>
        private static void BuildingModelGetExternalShell_AssertProbes(Shell shell, double offset, double tolerance)
        {
            List<Face>? faces = shell.PolygonalFaces;
            Assert.NotNull(faces);

            foreach (Face face in faces)
            {
                Point3D? point3D = face.GetInternalPoint(tolerance);
                Vector3D? normal = face.Plane?.Normal?.Unit;
                Assert.NotNull(point3D);
                Assert.NotNull(normal);

                Assert.True(shell.Inside(point3D - (normal * offset), tolerance), "A point just behind a face of the envelope is not inside it.");
                Assert.False(shell.Inside(point3D + (normal * offset), tolerance), "A point just in front of a face of the envelope is inside it, so the face points inward.");
            }
        }
    }
}
