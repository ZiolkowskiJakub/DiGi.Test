using DiGi.Analytical.Building;
using DiGi.Analytical.Building.Classes;
using DiGi.Analytical.Building.Interfaces;
using DiGi.Analytical.Classes;
using DiGi.Core.Parameter.Classes;
using DiGi.Geometry.Planar.Classes;
using DiGi.Geometry.Spatial;
using DiGi.Geometry.Spatial.Classes;
using DiGi.GIS.Analytical.Enums;
using DiGi.GIS.Classes;
using System.Collections.Generic;

namespace DiGi.GIS.Analytical.xUnit
{
    public partial class Facts
    {
        /// <summary>
        /// Verifies that the internal point of every storey of an extruded model sits at the mid height of that storey and inside the shell bounding it.
        /// <para>The point is taken from the footprint already projected onto the storey floor, so it starts out carrying the base elevation. Raising it by the absolute mid height rather than by half a storey adds that elevation a second time, which leaves the point above the roof of every building not standing at sea level - and passes unnoticed at an elevation of zero, where the two are the same number.</para>
        /// </summary>
        [Fact]
        public void BuildingModel_FromPolygonalFace3D_SpacePointAtStoreyMidHeight()
        {
            double minElevation = 188.5;
            ushort storeys = 2;
            double storeyHeight = 3.0;

            Plane plane = Geometry.Spatial.Create.Plane(minElevation)!;

            PolygonalFace3D? polygonalFace3D = Geometry.Spatial.Create.PolygonalFace3D(
                plane,
                [
                    new Point2D(0, 0),
                    new Point2D(10, 0),
                    new Point2D(10, 10),
                    new Point2D(0, 10)
                ]);

            Assert.NotNull(polygonalFace3D);

            BuildingModel? buildingModel = Create.BuildingModel(polygonalFace3D, storeys, storeyHeight);
            Assert.NotNull(buildingModel);

            List<Space>? spaces = buildingModel.GetSpaces<Space>();
            Assert.NotNull(spaces);
            Assert.Equal(storeys, spaces.Count);

            for (int i = 0; i < spaces.Count; i++)
            {
                Space space = spaces[i];

                Point3D? point3D = space.Geometry;
                Assert.NotNull(point3D);

                double expectedZ = minElevation + (i * storeyHeight) + (storeyHeight / 2);
                Assert.True(System.Math.Abs(point3D.Z - expectedZ) < Core.Constants.Tolerance.Distance, $"Space '{space.Name}' point sits at {point3D.Z} instead of the storey mid height {expectedZ}");

                Shell? shell = buildingModel.GetShell(space, tolerance: Constants.Tolerance.Enclosure);
                Assert.NotNull(shell);
                Assert.True(shell.IsClosed(Constants.Tolerance.Enclosure), $"Shell of space '{space.Name}' is not closed");
                Assert.True(shell.Inside(point3D, Constants.Tolerance.Enclosure), $"Point of space '{space.Name}' lies outside the shell bounding it");
            }
        }

        /// <summary>
        /// Verifies that a complete building model extruded at a terrain elevation passes every validation check.
        /// </summary>
        [Fact]
        public void BuildingModelValidationResult_Extruded_NoValidationCodes()
        {
            BuildingModel? buildingModel = BuildingModel_Extruded(188.5, 2);
            Assert.NotNull(buildingModel);

            BuildingModelValidationResult? buildingModelValidationResult = Create.BuildingModelValidationResult(buildingModel);
            Assert.NotNull(buildingModelValidationResult);

            Assert.NotNull(buildingModelValidationResult.ValidationCodes);
            Assert.Empty(buildingModelValidationResult.ValidationCodes);
            Assert.True(buildingModelValidationResult.IsValid);

            Assert.Equal(2, buildingModelValidationResult.SpaceCount);
            Assert.Equal(2, buildingModelValidationResult.ShellCount);
            Assert.Equal(2, buildingModelValidationResult.EnclosedShellCount);
            Assert.True(buildingModelValidationResult.MinEnclosingTolerance <= Constants.Tolerance.Enclosure, $"An extruded model needs a tolerance of {buildingModelValidationResult.MinEnclosingTolerance} to close");
            Assert.Equal(188.5, buildingModelValidationResult.MinZ, 6);
            Assert.Equal(194.5, buildingModelValidationResult.MaxZ, 6);

            Core.xUnit.Query.SerializationCheck(buildingModelValidationResult);
        }

        /// <summary>
        /// Verifies that a model left at an elevation of zero is reported, since that is the state a building takes when the terrain service never resolved its elevation rather than a place it was deliberately put.
        /// </summary>
        [Fact]
        public void BuildingModelValidationResult_SeaLevel()
        {
            BuildingModel? buildingModel = BuildingModel_Extruded(0, 1);
            Assert.NotNull(buildingModel);

            BuildingModelValidationResult? buildingModelValidationResult = Create.BuildingModelValidationResult(buildingModel);
            Assert.NotNull(buildingModelValidationResult);

            Assert.NotNull(buildingModelValidationResult.ValidationCodes);
            Assert.Contains(BuildingModelValidationCode.SeaLevel, buildingModelValidationResult.ValidationCodes);

            // Everything other than the elevation is sound, so the enclosure is expected to stand.
            Assert.DoesNotContain(BuildingModelValidationCode.NotEnclosed, buildingModelValidationResult.ValidationCodes);
        }

        /// <summary>
        /// Verifies that a model missing the parameters keying it to a building is reported.
        /// </summary>
        [Fact]
        public void BuildingModelValidationResult_MissingParameters()
        {
            Plane plane = Geometry.Spatial.Create.Plane(188.5)!;

            PolygonalFace3D? polygonalFace3D = Geometry.Spatial.Create.PolygonalFace3D(
                plane,
                [
                    new Point2D(0, 0),
                    new Point2D(10, 0),
                    new Point2D(10, 10),
                    new Point2D(0, 10)
                ]);

            Assert.NotNull(polygonalFace3D);

            BuildingModel? buildingModel = Create.BuildingModel(polygonalFace3D, 1, 3.0);
            Assert.NotNull(buildingModel);

            BuildingModelValidationResult? buildingModelValidationResult = Create.BuildingModelValidationResult(buildingModel);
            Assert.NotNull(buildingModelValidationResult);

            Assert.NotNull(buildingModelValidationResult.ValidationCodes);
            Assert.Contains(BuildingModelValidationCode.MissingReference, buildingModelValidationResult.ValidationCodes);
            Assert.Contains(BuildingModelValidationCode.MissingCode, buildingModelValidationResult.ValidationCodes);
            Assert.Null(buildingModelValidationResult.Reference);
            Assert.Null(buildingModelValidationResult.Code);
        }

        /// <summary>
        /// Verifies that removing one wall from an otherwise sound model is reported as a failure to enclose, and that the model is then reported as closing at no tolerance at all rather than at a coarser one.
        /// </summary>
        [Fact]
        public void BuildingModelValidationResult_NotEnclosed()
        {
            BuildingModel? buildingModel = BuildingModel_Extruded(188.5, 1);
            Assert.NotNull(buildingModel);

            List<IWall>? walls = buildingModel.GetComponents<IWall>();
            Assert.NotNull(walls);
            Assert.NotEmpty(walls);

            Assert.True(buildingModel.Remove(walls[0]));

            BuildingModelValidationResult? buildingModelValidationResult = Create.BuildingModelValidationResult(buildingModel);
            Assert.NotNull(buildingModelValidationResult);

            Assert.NotNull(buildingModelValidationResult.ValidationCodes);
            Assert.Contains(BuildingModelValidationCode.NotEnclosed, buildingModelValidationResult.ValidationCodes);
            Assert.Equal(0, buildingModelValidationResult.EnclosedShellCount);
            Assert.True(double.IsNaN(buildingModelValidationResult.MinEnclosingTolerance), $"An open model reports closing at {buildingModelValidationResult.MinEnclosingTolerance}");
        }

        /// <summary>
        /// Verifies the standalone enclosure predicate against a sound model and against the same model with a wall taken out.
        /// <para>The predicate exists next to <c>Analytical.Building.Query.IsValid</c> as the cheap gate a write path can hold a model to - <see cref="Create.BuildingModelValidationResult(BuildingModel, double)"/> answers the same question in far more detail, at the cost of assembling every shell twice.</para>
        /// </summary>
        [Fact]
        public void BuildingModel_IsEnclosed()
        {
            BuildingModel? buildingModel = BuildingModel_Extruded(188.5, 2);
            Assert.NotNull(buildingModel);
            Assert.True(buildingModel.IsEnclosed(Constants.Tolerance.Enclosure));

            List<IWall>? walls = buildingModel.GetComponents<IWall>();
            Assert.NotNull(walls);
            Assert.NotEmpty(walls);

            Assert.True(buildingModel.Remove(walls[0]));
            Assert.False(buildingModel.IsEnclosed(Constants.Tolerance.Enclosure));

            BuildingModel? buildingModel_Null = null;
            Assert.False(buildingModel_Null.IsEnclosed(Constants.Tolerance.Enclosure));
        }

        /// <summary>
        /// Verifies that a null building model yields no result rather than an empty one that would be counted as a pass.
        /// </summary>
        [Fact]
        public void BuildingModelValidationResult_Null()
        {
            Assert.Null(Create.BuildingModelValidationResult(null));
        }

        /// <summary>
        /// Builds a reference model the way the upload does - a square footprint extruded storey by storey from the given elevation, carrying the reference and the administrative area code.
        /// </summary>
        /// <param name="elevation">The base elevation in meters the footprint is extruded from.</param>
        /// <param name="storeys">The number of storeys to generate.</param>
        /// <returns>The extruded <see cref="BuildingModel"/>.</returns>
        private static BuildingModel? BuildingModel_Extruded(double elevation, ushort storeys)
        {
            Plane plane = Geometry.Spatial.Create.Plane(elevation)!;

            PolygonalFace3D? polygonalFace3D = Geometry.Spatial.Create.PolygonalFace3D(
                plane,
                [
                    new Point2D(250450, 390650),
                    new Point2D(250460, 390650),
                    new Point2D(250460, 390660),
                    new Point2D(250450, 390660)
                ]);

            BuildingModel? result = Create.BuildingModel(polygonalFace3D, storeys, 3.0);
            if (result is not null)
            {
                SetValueSettings setValueSettings = new(true, false);

                result.SetValue(BuildingModelParameter.Reference, "TEST-REFERENCE", setValueSettings);
                result.SetValue(BuildingModelParameter.Code, "0201", setValueSettings);
            }

            return result;
        }
    }
}
