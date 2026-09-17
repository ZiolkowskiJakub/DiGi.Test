using DiGi.Analytical.Building.Interfaces;
using DiGi.Core.IO.Table.Classes;
using DiGi.Core.Parameter.Classes;
using DiGi.GIS.Analytical.Enums;
using DiGi.GIS.PostgreSQL.Classes;
using DiGi.GIS.PostgreSQL.Enums;
using DiGi.Geometry.Planar.Classes;
using DiGi.Geometry.Spatial.Classes;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DiGi.GIS.PostgreSQL.xUnit
{
    public partial class Facts
    {
        /// <summary>
        /// The names of the 35 external components area columns, in the order the method under test declares them: the eight wall sectors, the flat roof, the 24 tilted-roof bands, the floor, and the total last.
        /// <para>The names are declared here rather than taken from the shared GIS.IO constants, for the same reason as in the typology facts: this project does not reference that assembly, and only a column's name matters to address a cell.</para>
        /// </summary>
        private static readonly string[] ColumnNames_External =
        [
            "External north wall area",
            "External northeast wall area",
            "External east wall area",
            "External southeast wall area",
            "External south wall area",
            "External southwest wall area",
            "External west wall area",
            "External northwest wall area",
            "External flat roof area",
            "External north tilted roof area up to 20",
            "External northeast tilted roof area up to 20",
            "External east tilted roof area up to 20",
            "External southeast tilted roof area up to 20",
            "External south tilted roof area up to 20",
            "External southwest tilted roof area up to 20",
            "External west tilted roof area up to 20",
            "External northwest tilted roof area up to 20",
            "External north tilted roof area between 20 and 45",
            "External northeast tilted roof area between 20 and 45",
            "External east tilted roof area between 20 and 45",
            "External southeast tilted roof area between 20 and 45",
            "External south tilted roof area between 20 and 45",
            "External southwest tilted roof area between 20 and 45",
            "External west tilted roof area between 20 and 45",
            "External northwest tilted roof area between 20 and 45",
            "External north tilted roof area above 45",
            "External northeast tilted roof area above 45",
            "External east tilted roof area above 45",
            "External southeast tilted roof area above 45",
            "External south tilted roof area above 45",
            "External southwest tilted roof area above 45",
            "External west tilted roof area above 45",
            "External northwest tilted roof area above 45",
            "External floor area",
            "External components area"
        ];

        /// <summary>
        /// Builds a polygonal face from a plane and the face's local 2D points.
        /// <para>The plane's origin and normal define the face's position and side in 3D; the points address the face within the plane, so an assertion on the stored normal and the area stays independent of how the plane derives its local basis.</para>
        /// </summary>
        /// <param name="plane">The plane the face lies on.</param>
        /// <param name="points">The face's points in the plane's local 2D coordinates.</param>
        /// <returns>The polygonal face.</returns>
        private static PolygonalFace3D Face3D(Plane plane, params Point2D[] points)
        {
            PolygonalFace3D? face = DiGi.Geometry.Spatial.Create.PolygonalFace3D(plane, points);
            Assert.NotNull(face);

            return face!;
        }

        /// <summary>
        /// Builds a floor of the given edge size, centred on the origin in the plane z = 0.
        /// <para>The floor's internal point is the interior reference the wall normals are oriented against, so it is deliberately the centre of the building footprint - at the origin - while the walls are placed a fixed distance away from it.</para>
        /// </summary>
        /// <param name="size">The edge length of the square floor in metres.</param>
        /// <returns>The floor component.</returns>
        private static DiGi.Analytical.Building.Classes.FaceFloor Floor(double size)
        {
            Plane plane = new(new Point3D(0, 0, 0), new Vector3D(0, 0, 1));
            PolygonalFace3D face = Face3D(
                plane,
                new Point2D(-size / 2, -size / 2),
                new Point2D(size / 2, -size / 2),
                new Point2D(size / 2, size / 2),
                new Point2D(-size / 2, size / 2));

            DiGi.Analytical.Building.Classes.FaceFloor? floor = DiGi.Analytical.Building.Create.FaceFloor(face);
            Assert.NotNull(floor);

            return floor!;
        }

        /// <summary>
        /// Builds a 1 x 1 metre wall whose outward normal has the given azimuth, stored with either the outward or the inward normal.
        /// <para>The wall plane is offset from the origin - the floor's internal point - in the outward direction, so the interior of the building sits strictly on one side of it and the orientation test the method under test performs is unambiguous: the stored normal is the outward one when <paramref name="outward"/> is true and its inverse when it is false.</para>
        /// </summary>
        /// <param name="azimuthDegrees">The azimuth of the wall's outward normal, in degrees clockwise from north.</param>
        /// <param name="outward">Whether the stored normal points away from the building interior or into it.</param>
        /// <returns>The wall component.</returns>
        private static DiGi.Analytical.Building.Classes.SurfaceWall Wall(double azimuthDegrees, bool outward)
        {
            double radians = azimuthDegrees * Math.PI / 180.0;
            double sign = outward ? 1.0 : -1.0;

            Vector3D normal = new(sign * Math.Sin(radians), sign * Math.Cos(radians), 0);
            Plane plane = new(new Point3D(5 * Math.Sin(radians), 5 * Math.Cos(radians), 0), normal);
            PolygonalFace3D face = Face3D(
                plane,
                new Point2D(0, 0),
                new Point2D(1, 0),
                new Point2D(1, 1),
                new Point2D(0, 1));

            DiGi.Analytical.Building.Classes.SurfaceWall? wall = DiGi.Analytical.Building.Create.SurfaceWall(face);
            Assert.NotNull(wall);

            return wall!;
        }

        /// <summary>
        /// Builds a square roof of the given edge size with the given tilt from vertical and the given azimuth of its horizontal normal projection.
        /// </summary>
        /// <param name="tiltDegrees">The roof's tilt from vertical, in degrees.</param>
        /// <param name="azimuthDegrees">The azimuth of the roof normal's horizontal projection, in degrees clockwise from north.</param>
        /// <param name="size">The edge length of the square roof in metres.</param>
        /// <returns>The roof component.</returns>
        private static DiGi.Analytical.Building.Classes.SurfaceRoof Roof(double tiltDegrees, double azimuthDegrees, double size)
        {
            double tilt = tiltDegrees * Math.PI / 180.0;
            double azimuth = azimuthDegrees * Math.PI / 180.0;

            Vector3D normal = new(Math.Sin(azimuth) * Math.Sin(tilt), Math.Cos(azimuth) * Math.Sin(tilt), Math.Cos(tilt));
            Plane plane = new(new Point3D(0, 0, 10), normal);
            PolygonalFace3D face = Face3D(
                plane,
                new Point2D(0, 0),
                new Point2D(size, 0),
                new Point2D(size, size),
                new Point2D(0, size));

            DiGi.Analytical.Building.Classes.SurfaceRoof? roof = DiGi.Analytical.Building.Create.SurfaceRoof(face);
            Assert.NotNull(roof);

            return roof!;
        }

        /// <summary>
        /// Builds a stored building model envelope: the reference is set, every component is added, and the result is converted for storage with the given county identifier.
        /// </summary>
        /// <param name="reference">The building reference the model is stored under.</param>
        /// <param name="countyId">The county identifier the model is stored under.</param>
        /// <param name="components">The components the model carries; it is the caller's responsibility to include a floor, because the method under test refuses a model without one.</param>
        /// <returns>The storage envelope.</returns>
        private static BuildingModel Model(string reference, int countyId, params IComponent[] components)
        {
            DiGi.Analytical.Building.Classes.BuildingModel buildingModel = new();
            Assert.True(buildingModel.SetValue(BuildingModelParameter.Reference, reference, new SetValueSettings(true, false)));

            foreach (IComponent component in components)
            {
                Assert.True(buildingModel.Update(component));
            }

            BuildingModel? envelope = buildingModel.ToPostgreSQL(countyId);
            Assert.NotNull(envelope);

            return envelope!;
        }

        /// <summary>
        /// Reads the value the method under test wrote into a table cell, addressed by the column's name.
        /// </summary>
        /// <param name="table">The table the row belongs to.</param>
        /// <param name="row">The row to read.</param>
        /// <param name="columnName">The name of the column to read.</param>
        /// <returns>The cell's value; 0 when the cell was never written.</returns>
        private static double Value(Table table, Row row, string columnName)
        {
            Assert.True(table.TryGetColumn(columnName, out Column? column_Table), $"column {columnName} is missing from the table");
            Assert.NotNull(column_Table);
            Assert.NotNull(row);

            return row.GetValue<float>(column_Table.Index, 0f);
        }

        /// <summary>
        /// Verifies that a wall is filed under the sector its outward normal's azimuth belongs to, on both sides of every sector boundary.
        /// <para>Each boundary is crossed twice - 0.0001° either side - so the wall just inside a sector and the one just across it land in different columns, which is what the boundary is for. The margin is deliberately far above the precision of the azimuth computation, because an azimuth sitting exactly on a boundary has no claim to either side in floating point. 350° and 10° cover the north wrap. Half the walls are stored with their inward normal, so both directions of the stored normal reach the outward-orientation logic, and the sector they land in is the same.</para>
        /// </summary>
        [Fact]
        public void Update_ExternalComponentsArea_WallSectorBoundaries()
        {
            // (azimuth of the outward normal, stored outward) in the order the walls are added: both sides of every boundary, then the north wrap.
            (double Azimuth, bool Outward)[] walls =
            [
                (22.4999, true), (22.5001, false),
                (67.4999, true), (67.5001, false),
                (112.4999, true), (112.5001, false),
                (157.4999, true), (157.5001, false),
                (202.4999, true), (202.5001, false),
                (247.4999, true), (247.5001, false),
                (292.4999, true), (292.5001, false),
                (337.4999, true), (337.5001, false),
                (350.0, true), (10.0, false)
            ];

            List<IComponent> components = [];
            foreach ((double azimuth, bool outward) in walls)
            {
                components.Add(Wall(azimuth, outward));
            }

            components.Add(Floor(10));
            BuildingModel model = Model("ref_walls", 5, [.. components]);

            Table table = new();
            long skipped = Modify.Update_ExternalComponentsArea(table, [model]);

            Assert.Equal(0, skipped);
            Assert.Equal(1, table.RowCount);

            Row? row = table.GetRow(0);
            Assert.NotNull(row);

            // North: 22.4999, 337.5001, 350 and 10. Every other sector: the pair on either side of its boundary.
            Assert.Equal(4.0, Value(table, row, ColumnNames_External[0]), 1e-3);
            Assert.Equal(2.0, Value(table, row, ColumnNames_External[1]), 1e-3);
            Assert.Equal(2.0, Value(table, row, ColumnNames_External[2]), 1e-3);
            Assert.Equal(2.0, Value(table, row, ColumnNames_External[3]), 1e-3);
            Assert.Equal(2.0, Value(table, row, ColumnNames_External[4]), 1e-3);
            Assert.Equal(2.0, Value(table, row, ColumnNames_External[5]), 1e-3);
            Assert.Equal(2.0, Value(table, row, ColumnNames_External[6]), 1e-3);
            Assert.Equal(2.0, Value(table, row, ColumnNames_External[7]), 1e-3);

            // Nothing of a roof may leak into a wall's buckets and vice versa.
            Assert.Equal(0.0, Value(table, row, ColumnNames_External[8]), 1e-9);
            Assert.Equal(100.0, Value(table, row, ColumnNames_External[33]), 1e-3);
            Assert.Equal(118.0, Value(table, row, ColumnNames_External[34]), 1e-3);
        }

        /// <summary>
        /// Verifies that a roof is filed under its tilt band, on both sides of every band boundary.
        /// <para>Flat is strictly below 5°, the first tilted band is [5°, 20°], the second is (20°, 45°] and the third is above 45°, exactly as the column descriptions state - so the roof just under a boundary and the one just over it must land in different columns. Each boundary is crossed 0.0001° at a time, far above the precision of the tilt computation, because a tilt sitting exactly on a boundary has no claim to either side in floating point. Every roof faces east, so the sector is constant and only the band can move a roof between columns.</para>
        /// </summary>
        [Fact]
        public void Update_ExternalComponentsArea_RoofTiltBands()
        {
            BuildingModel model = Model(
                "ref_roofs",
                5,
                Roof(4.9999, 90, 10),
                Roof(5.0001, 90, 10),
                Roof(19.9999, 90, 10),
                Roof(20.0001, 90, 10),
                Roof(44.9999, 90, 10),
                Roof(45.0001, 90, 10),
                Floor(10));

            Table table = new();
            long skipped = Modify.Update_ExternalComponentsArea(table, [model]);

            Assert.Equal(0, skipped);
            Assert.Equal(1, table.RowCount);

            Row? row = table.GetRow(0);
            Assert.NotNull(row);

            // 4.9999 flat, 5.0001 and 19.9999 in the first band, 20.0001 and 44.9999 in the second, 45.0001 in the third.
            Assert.Equal(100.0, Value(table, row, ColumnNames_External[8]), 1e-3);
            Assert.Equal(200.0, Value(table, row, ColumnNames_External[11]), 1e-3);
            Assert.Equal(200.0, Value(table, row, ColumnNames_External[19]), 1e-3);
            Assert.Equal(100.0, Value(table, row, ColumnNames_External[27]), 1e-3);

            // The north sector's tilted bands and the wall buckets stay empty.
            Assert.Equal(0.0, Value(table, row, ColumnNames_External[9]), 1e-9);
            Assert.Equal(0.0, Value(table, row, ColumnNames_External[17]), 1e-9);
            Assert.Equal(0.0, Value(table, row, ColumnNames_External[25]), 1e-9);
            Assert.Equal(0.0, Value(table, row, ColumnNames_External[0]), 1e-9);

            Assert.Equal(100.0, Value(table, row, ColumnNames_External[33]), 1e-3);
            Assert.Equal(700.0, Value(table, row, ColumnNames_External[34]), 1e-3);
        }

        /// <summary>
        /// Verifies that a roof flat enough to be flat is filed under the flat column regardless of which direction it faces.
        /// <para>The flat band has no directional split by design, so a 4° roof facing east must not leak into any of the east tilted-roof columns, and it must not be mistaken for a wall either.</para>
        /// </summary>
        [Fact]
        public void Update_ExternalComponentsArea_FlatRoofRegardlessOfAzimuth()
        {
            BuildingModel model = Model(
                "ref_flatroof",
                5,
                Roof(4.0, 90, 10),
                Floor(10));

            Table table = new();
            long skipped = Modify.Update_ExternalComponentsArea(table, [model]);

            Assert.Equal(0, skipped);
            Assert.Equal(1, table.RowCount);

            Row? row = table.GetRow(0);
            Assert.NotNull(row);

            Assert.Equal(100.0, Value(table, row, ColumnNames_External[8]), 1e-3);

            // Every tilted-roof band stays empty, in every sector.
            for (int i = 9; i <= 32; i++)
            {
                Assert.Equal(0.0, Value(table, row, ColumnNames_External[i]), 1e-9);
            }

            Assert.Equal(100.0, Value(table, row, ColumnNames_External[33]), 1e-3);
            Assert.Equal(200.0, Value(table, row, ColumnNames_External[34]), 1e-3);
        }

        /// <summary>
        /// Verifies that a model made of floors only is filed under the floor column and nowhere else.
        /// <para>Floors carry no direction and no tilt by construction, so every directional and tilt bucket must stay empty - a leak into one would mean the classification is reading a component's type wrong.</para>
        /// </summary>
        [Fact]
        public void Update_ExternalComponentsArea_FloorOnly()
        {
            BuildingModel model = Model("ref_floor", 5, Floor(10));

            Table table = new();
            long skipped = Modify.Update_ExternalComponentsArea(table, [model]);

            Assert.Equal(0, skipped);
            Assert.Equal(1, table.RowCount);

            Row? row = table.GetRow(0);
            Assert.NotNull(row);

            Assert.Equal(100.0, Value(table, row, ColumnNames_External[33]), 1e-3);
            Assert.Equal(100.0, Value(table, row, ColumnNames_External[34]), 1e-3);

            // The eight wall sectors and the flat roof stay empty.
            for (int i = 0; i <= 8; i++)
            {
                Assert.Equal(0.0, Value(table, row, ColumnNames_External[i]), 1e-9);
            }
        }

        /// <summary>
        /// Verifies that the total column equals the sum of the 34 breakdown columns it is derived from, for a model that populates every kind of bucket at once.
        /// <para>The total is computed from the same accumulators as the breakdowns, so the equality must hold on the stored values - a total that drifted from the sum would mean the two were measured separately and can disagree.</para>
        /// </summary>
        [Fact]
        public void Update_ExternalComponentsArea_TotalEqualsSumOfBreakdowns()
        {
            BuildingModel model = Model(
                "ref_total",
                5,
                Wall(0, true),
                Wall(135, false),
                Roof(2.0, 45, 10),
                Roof(10.0, 180, 10),
                Roof(30.0, 90, 5),
                Floor(8));

            Table table = new();
            long skipped = Modify.Update_ExternalComponentsArea(table, [model]);

            Assert.Equal(0, skipped);
            Assert.Equal(1, table.RowCount);

            Row? row = table.GetRow(0);
            Assert.NotNull(row);

            // One component per bucket, so each breakdown column carries its component's area and nothing else.
            Assert.Equal(1.0, Value(table, row, ColumnNames_External[0]), 1e-3);
            Assert.Equal(1.0, Value(table, row, ColumnNames_External[3]), 1e-3);
            Assert.Equal(100.0, Value(table, row, ColumnNames_External[8]), 1e-3);
            Assert.Equal(100.0, Value(table, row, ColumnNames_External[13]), 1e-3);
            Assert.Equal(25.0, Value(table, row, ColumnNames_External[19]), 1e-3);
            Assert.Equal(64.0, Value(table, row, ColumnNames_External[33]), 1e-3);

            double sumOfBreakdowns = 0;

            for (int i = 0; i < ColumnNames_External.Length - 1; i++)
            {
                sumOfBreakdowns += Value(table, row, ColumnNames_External[i]);
            }

            double total = Value(table, row, ColumnNames_External[^1]);
            Assert.Equal(sumOfBreakdowns, total, 3);
            Assert.Equal(291.0, total, 1e-3);
        }

        /// <summary>
        /// Verifies that a component the method cannot classify is refused with an exception rather than skipped, and that a model without a floor is refused the same way.
        /// <para>A curved wall without a base curve yields no polygonal face, so it cannot be classified - the method must throw and name the building, because a silently missing wall area is a defect the caller cannot see. A model without a floor has no interior reference point the wall normals can be oriented against, so it is refused before any of its walls are read.</para>
        /// </summary>
        [Fact]
        public void Update_ExternalComponentsArea_NonPlanarThrows()
        {
            BuildingModel model_WallWithoutFace = Model(
                "ref_nonplanar",
                5,
                Wall(90, true),
                new DiGi.Analytical.Building.Classes.CurveWall(null, 5.0),
                Floor(10));

            Table table = new();
            InvalidOperationException? exception = Assert.Throws<InvalidOperationException>(() => Modify.Update_ExternalComponentsArea(table, [model_WallWithoutFace]));

            Assert.Contains("ref_nonplanar", exception!.Message);
            Assert.Contains("wall", exception.Message);

            BuildingModel model_WithoutFloor = Model(
                "ref_nofloor",
                5,
                Wall(90, true));

            Table table_Floorless = new();
            InvalidOperationException? exception_Floorless = Assert.Throws<InvalidOperationException>(() => Modify.Update_ExternalComponentsArea(table_Floorless, [model_WithoutFloor]));

            Assert.Contains("ref_nofloor", exception_Floorless!.Message);
            Assert.Contains("floor", exception_Floorless.Message);
        }

        /// <summary>
        /// Verifies that when the same building arrives with several stored model versions, the first one - the latest, the converter's order of preference - is the one written, and a different building still gets its own row.
        /// <para>The first model's north wall and 10 x 10 floor are what must be in the row, while the second model's east wall and 8 x 8 floor must not have leaked in; the third envelope, a different reference, must produce a second row with its own areas.</para>
        /// </summary>
        [Fact]
        public void Update_ExternalComponentsArea_LatestModelWins()
        {
            BuildingModel model_First = Model("ref_latest", 5, Wall(0, true), Floor(10));
            BuildingModel model_Second = Model("ref_latest", 5, Wall(90, true), Floor(8));
            BuildingModel model_Other = Model("ref_other", 5, Wall(180, true), Floor(6));

            Table table = new();
            long skipped = Modify.Update_ExternalComponentsArea(table, [model_First, model_Second, model_Other]);

            Assert.Equal(0, skipped);
            Assert.Equal(2, table.RowCount);

            int index_Reference = table.GetColumnIndex("Reference");
            Assert.True(index_Reference >= 0);

            Row? row_First = null;
            Row? row_Other = null;

            for (int i = 0; i < table.RowCount; i++)
            {
                Row? row_Temp = table.GetRow(i);
                Assert.NotNull(row_Temp);

                string? reference = row_Temp.GetValue<string>(index_Reference, string.Empty);

                if (reference == "ref_latest")
                {
                    row_First = row_Temp;
                }
                else if (reference == "ref_other")
                {
                    row_Other = row_Temp;
                }
            }

            Assert.NotNull(row_First);
            Assert.NotNull(row_Other);

            Assert.Equal(1.0, Value(table, row_First!, ColumnNames_External[0]), 1e-3);
            Assert.Equal(0.0, Value(table, row_First, ColumnNames_External[2]), 1e-9);
            Assert.Equal(100.0, Value(table, row_First, ColumnNames_External[33]), 1e-3);
            Assert.Equal(101.0, Value(table, row_First, ColumnNames_External[34]), 1e-3);

            Assert.Equal(1.0, Value(table, row_Other!, ColumnNames_External[4]), 1e-3);
            Assert.Equal(36.0, Value(table, row_Other, ColumnNames_External[33]), 1e-3);
            Assert.Equal(37.0, Value(table, row_Other, ColumnNames_External[34]), 1e-3);
        }

        /// <summary>
        /// Verifies that a building with a stored model gets zeros in its empty buckets while a building the method cannot address gets no row at all.
        /// <para>The zero-versus-null distinction is what lets a reader tell "the building has no external walls" from "the building has no stored model": the first writes a row of zeros, the second leaves the columns untouched. Envelopes with a blank reference or a missing county identifier are skipped before anything is read from them, so they cannot fail the run either.</para>
        /// </summary>
        [Fact]
        public void Update_ExternalComponentsArea_ZeroVsNull()
        {
            BuildingModel model_FloorOnly = Model("ref_zeros", 5, Floor(10));

            Table table = new();
            long skipped = Modify.Update_ExternalComponentsArea(table, [model_FloorOnly]);

            Assert.Equal(0, skipped);
            Assert.Equal(1, table.RowCount);

            Row? row = table.GetRow(0);
            Assert.NotNull(row);

            // Every wall and roof bucket is present and zero, which is what distinguishes the row from an absent one.
            for (int i = 0; i <= 32; i++)
            {
                Assert.Equal(0.0, Value(table, row, ColumnNames_External[i]), 1e-9);
            }

            double floorArea = Value(table, row, ColumnNames_External[33]);
            Assert.True(floorArea > 0);
            Assert.Equal(floorArea, Value(table, row, ColumnNames_External[34]), 3);

            // Envelopes the method cannot address are stepped over without a row and without a skip count.
            BuildingModel envelope_BlankReference = new() { Reference = "   ", CountyId = 5 };
            BuildingModel envelope_MissingCounty = new() { Reference = "ref_orphan", CountyId = null };
            BuildingModel model_Valid = Model("ref_valid", 5, Floor(10));

            Table table_Skipped = new();
            long skipped_Skipped = Modify.Update_ExternalComponentsArea(table_Skipped, [envelope_BlankReference, envelope_MissingCounty, model_Valid]);

            Assert.Equal(0, skipped_Skipped);
            Assert.Equal(1, table_Skipped.RowCount);
        }

        /// <summary>
        /// Verifies that the classification holds for the currently deployed <c>BuildingModel</c>s saved in PostgreSQL - the development database behind <c>GIS_PostgreSQL_Storage.conf</c>.
        /// <para>This is the end-to-end proof the maintainer's face and internal-point contract is sound on real data: every stored component must yield a polygonal face with a resolvable internal point, and every model must carry a floor. A single throw here is a data finding to report, not a test to loosen.</para>
        /// <para>The fact is read-only: it classifies the models into an in-memory table and checks the result, but never writes to the database. Skipped by default, because it needs the conf pointing at a database.</para>
        /// </summary>
        [Fact(Skip = "Runs against the deployed BuildingModels. Point GIS_PostgreSQL_Storage.conf at a database before running.")]
        public async Task Update_ExternalComponentsArea_DeployedBuildingModels()
        {
            const int commandTimeout = 600;

            GISPostgreSQLConverterManager? gISPostgreSQLConverterManager = Create.GISPostgreSQLConverterManager();
            Assert.NotNull(gISPostgreSQLConverterManager);

            BuildingModelPostgreSQLConverter? buildingModelPostgreSQLConverter = gISPostgreSQLConverterManager.GetPostgreSQLConverter<BuildingModelPostgreSQLConverter>();
            Assert.NotNull(buildingModelPostgreSQLConverter);

            AdministrativeAreal2DPostgreSQLConverter? administrativeAreal2DPostgreSQLConverter = gISPostgreSQLConverterManager.GetPostgreSQLConverter<AdministrativeAreal2DPostgreSQLConverter>();
            Assert.NotNull(administrativeAreal2DPostgreSQLConverter);

            List<AdministrativeAreal2DReference>? countyReferences = await administrativeAreal2DPostgreSQLConverter.GetAdministrativeAreal2DReferencesByAdministrativeArealTypeAsync(AdministrativeArealType.County, commandTimeout: commandTimeout);
            Assert.NotNull(countyReferences);

            // The first county that actually carries stored building models.
            int? countyId = null;

            foreach (AdministrativeAreal2DReference? countyReference in countyReferences)
            {
                if (countyReference is null)
                {
                    continue;
                }

                HashSet<string>? references_County = await buildingModelPostgreSQLConverter.GetReferencesAsync(countyReference.Id, commandTimeout: commandTimeout);

                if (references_County is not null && references_County.Count > 0)
                {
                    countyId = countyReference.Id;
                    break;
                }
            }

            Assert.NotNull(countyId);

            HashSet<string>? references = await buildingModelPostgreSQLConverter.GetReferencesAsync(countyId, commandTimeout: commandTimeout);
            Assert.NotNull(references);
            Assert.True(references.Count > 0);

            List<BuildingModel>? models = await buildingModelPostgreSQLConverter.GetItemsByReferencesAsync([.. references], countyId, fallbackByReference: false, commandTimeout: commandTimeout);
            Assert.NotNull(models);
            Assert.True(models.Count > 0);

            // The read is ordered created_at DESC, id DESC, so the first record of a reference is the latest stored version.
            HashSet<string> references_Seen = [];
            List<BuildingModel> models_Latest = [];

            foreach (BuildingModel? model in models)
            {
                if (model is null || model.Reference is not string reference_Model || string.IsNullOrWhiteSpace(reference_Model))
                {
                    continue;
                }

                if (references_Seen.Add(reference_Model))
                {
                    models_Latest.Add(model);
                }
            }

            Assert.True(models_Latest.Count > 0);

            // The call must not throw: every stored component is a planar face with a resolvable internal point, and every model carries a floor.
            Table table = new();
            long skipped = Modify.Update_ExternalComponentsArea(table, models_Latest);

            Assert.Equal(0, skipped);
            Assert.True(table.RowCount > 0);

            bool sawFloor = false;
            bool sawWallOrRoof = false;

            for (int i = 0; i < table.RowCount; i++)
            {
                Row? row = table.GetRow(i);
                Assert.NotNull(row);

                double sumOfBreakdowns = 0;

                for (int j = 0; j < ColumnNames_External.Length - 1; j++)
                {
                    double value = Value(table, row, ColumnNames_External[j]);
                    sumOfBreakdowns += value;

                    // The wall and roof buckets run from the first tilted band up to the last one; the floor and the total have their own checks.
                    if (j > 7 && j < ColumnNames_External.Length - 2)
                    {
                        sawWallOrRoof |= value > 0;
                    }
                }

                sawFloor |= Value(table, row, ColumnNames_External[33]) > 0;

                double total = Value(table, row, ColumnNames_External[^1]);
                double tolerance = Math.Max(0.01, 1e-5 * Math.Abs(total));
                Assert.InRange(Math.Abs(total - sumOfBreakdowns), 0, tolerance);
            }

            Assert.True(sawFloor);
            Assert.True(sawWallOrRoof);
        }
    }
}
