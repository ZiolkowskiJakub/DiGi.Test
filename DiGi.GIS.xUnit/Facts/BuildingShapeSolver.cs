using DiGi.Geometry.Planar.Classes;
using DiGi.Geometry.Planar.Interfaces;
using DiGi.GIS.Classes;
using DiGi.GIS.Enums;
using System;
using System.Collections.Generic;

namespace DiGi.GIS.xUnit
{
    public partial class Facts
    {
        /// <summary>Verifies that the solver classifies canonical hand-built footprints (square, rectangle, L, T, U, H, X and E) into the expected <see cref="BuildingShape"/> using the default factors.</summary>
        [Fact]
        public void BuildingShapeSolver()
        {
            List<Tuple<BuildingShape, Point2D[]>> cases =
            [
                new(BuildingShape.Square, [new(0, 0), new(20, 0), new(20, 20), new(0, 20)]),
                new(BuildingShape.Rectangular, [new(0, 0), new(40, 0), new(40, 12), new(0, 12)]),
                new(BuildingShape.L, [new(0, 0), new(30, 0), new(30, 12), new(12, 12), new(12, 30), new(0, 30)]),
                new(BuildingShape.T, [new(10, 0), new(20, 0), new(20, 20), new(30, 20), new(30, 30), new(0, 30), new(0, 20), new(10, 20)]),
                new(BuildingShape.U, [new(0, 0), new(30, 0), new(30, 30), new(20, 30), new(20, 10), new(10, 10), new(10, 30), new(0, 30)]),
                new(BuildingShape.H, [new(0, 0), new(10, 0), new(10, 10), new(20, 10), new(20, 0), new(30, 0), new(30, 30), new(20, 30), new(20, 20), new(10, 20), new(10, 30), new(0, 30)]),
                new(BuildingShape.X, [new(10, 0), new(20, 0), new(20, 10), new(30, 10), new(30, 20), new(20, 20), new(20, 30), new(10, 30), new(10, 20), new(0, 20), new(0, 10), new(10, 10)]),
                new(BuildingShape.E, [new(0, 0), new(30, 0), new(30, 8), new(10, 8), new(10, 12), new(30, 12), new(30, 18), new(10, 18), new(10, 24), new(30, 24), new(30, 30), new(0, 30)]),
            ];

            foreach (Tuple<BuildingShape, Point2D[]> tuple in cases)
            {
                Building2D building2D = CreateBuilding2D(tuple.Item2);

                BuildingShapeSolver buildingShapeSolver = new()
                {
                    Input = building2D
                };

                Assert.True(buildingShapeSolver.Solve());
                Assert.Equal(tuple.Item1, buildingShapeSolver.Output);
            }
        }

        /// <summary>Verifies that a box-filling footprint that encloses a courtyard (internal edge) is classified as <see cref="BuildingShape.O"/>, while the same footprint without a hole remains Square or Rectangular.</summary>
        [Fact]
        public void BuildingShapeSolver_Courtyard()
        {
            // Square outline with a central hole -> ring -> O; without the hole -> Square.
            Point2D[] squareOutline = [new(0, 0), new(40, 0), new(40, 40), new(0, 40)];
            Point2D[] squareHole = [new(14, 14), new(26, 14), new(26, 26), new(14, 26)];

            AssertShape(BuildingShape.O, CreateBuilding2D(squareOutline, squareHole));
            AssertShape(BuildingShape.Square, CreateBuilding2D(squareOutline));

            // Rectangular outline with a central hole -> ring -> O; without the hole -> Rectangular.
            Point2D[] rectangleOutline = [new(0, 0), new(60, 0), new(60, 40), new(0, 40)];
            Point2D[] rectangleHole = [new(20, 15), new(40, 15), new(40, 25), new(20, 25)];

            AssertShape(BuildingShape.O, CreateBuilding2D(rectangleOutline, rectangleHole));
            AssertShape(BuildingShape.Rectangular, CreateBuilding2D(rectangleOutline));
        }

        /// <summary>Verifies that a near-circular footprint (a regular 36-gon) whose isoperimetric ratio exceeds the thinness ratio is classified as <see cref="BuildingShape.Circural"/>.</summary>
        [Fact]
        public void BuildingShapeSolver_Circular()
        {
            int count = 36;
            double radius = 15;

            Point2D[] point2Ds = new Point2D[count];
            for (int i = 0; i < count; i++)
            {
                double angle = 2 * Math.PI * i / count;
                point2Ds[i] = new Point2D(radius * Math.Cos(angle), radius * Math.Sin(angle));
            }

            Building2D building2D = CreateBuilding2D(point2Ds);

            BuildingShapeSolver buildingShapeSolver = new()
            {
                Input = building2D
            };

            Assert.True(buildingShapeSolver.Solve());
            Assert.Equal(BuildingShape.Circural, buildingShapeSolver.Output);
        }

        /// <summary>Verifies that the thinness ratio threshold governs the boundary between Square and Rectangular by classifying a 100 by 90 rectangle (square thinness ratio 0.9) as Square just below the threshold and as Rectangular just above it.</summary>
        [Fact]
        public void BuildingShapeSolver_ThinnessRatioBoundary()
        {
            Building2D building2D = CreateBuilding2D([new(0, 0), new(100, 0), new(100, 90), new(0, 90)]);

            BuildingShapeSolver buildingShapeSolver_Inside = new(thinnessRatio: 0.89)
            {
                Input = building2D
            };
            Assert.True(buildingShapeSolver_Inside.Solve());
            Assert.Equal(BuildingShape.Square, buildingShapeSolver_Inside.Output);

            BuildingShapeSolver buildingShapeSolver_Outside = new(thinnessRatio: 0.91)
            {
                Input = building2D
            };
            Assert.True(buildingShapeSolver_Outside.Solve());
            Assert.Equal(BuildingShape.Rectangular, buildingShapeSolver_Outside.Output);
        }

        /// <summary>Verifies that the solver reports failure and an Undefined shape when no input is supplied or when the input building has no geometry.</summary>
        [Fact]
        public void BuildingShapeSolver_InvalidInput()
        {
            BuildingShapeSolver buildingShapeSolver_NoInput = new();
            Assert.False(buildingShapeSolver_NoInput.Solve());
            Assert.Equal(BuildingShape.Undefined, buildingShapeSolver_NoInput.Output);

            Building2D building2D = new(Guid.NewGuid(), "reference_NoGeometry", null, 1, BuildingPhase.occupied, BuildingGeneralFunction.residential_buildings, [BuildingSpecificFunction.single_family_building]);

            BuildingShapeSolver buildingShapeSolver_NoGeometry = new()
            {
                Input = building2D
            };
            Assert.False(buildingShapeSolver_NoGeometry.Solve());
            Assert.Equal(BuildingShape.Undefined, buildingShapeSolver_NoGeometry.Output);
        }

        /// <summary>Verifies that the constructor rejects out-of-range factors by throwing an <see cref="ArgumentOutOfRangeException"/> for every argument.</summary>
        [Fact]
        public void BuildingShapeSolver_ConstructorValidation()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new BuildingShapeSolver(offset: 0));
            Assert.Throws<ArgumentOutOfRangeException>(() => new BuildingShapeSolver(thinnessRatio: 0));
            Assert.Throws<ArgumentOutOfRangeException>(() => new BuildingShapeSolver(thinnessRatio: 1.5));
            Assert.Throws<ArgumentOutOfRangeException>(() => new BuildingShapeSolver(microTolerance: 0));
            Assert.Throws<ArgumentOutOfRangeException>(() => new BuildingShapeSolver(macroTolerance: -1));
            Assert.Throws<ArgumentOutOfRangeException>(() => new BuildingShapeSolver(areaFactor: -0.1));
            Assert.Throws<ArgumentOutOfRangeException>(() => new BuildingShapeSolver(aspectRatioFactor: double.NaN));
            Assert.Throws<ArgumentOutOfRangeException>(() => new BuildingShapeSolver(areaFactor: 0, aspectRatioFactor: 0, rectangleThinnessRatioFactor: 0));
            Assert.Throws<ArgumentOutOfRangeException>(() => new BuildingShapeSolver(scoreFactor: 0));
            Assert.Throws<ArgumentOutOfRangeException>(() => new BuildingShapeSolver(scoreFactor: 1.1));
        }

        private static void AssertShape(BuildingShape expected, Building2D building2D)
        {
            BuildingShapeSolver buildingShapeSolver = new()
            {
                Input = building2D
            };

            Assert.True(buildingShapeSolver.Solve());
            Assert.Equal(expected, buildingShapeSolver.Output);
        }

        private static Building2D CreateBuilding2D(Point2D[] point2Ds)
        {
            PolygonalFace2D? polygonalFace2D = Geometry.Planar.Create.PolygonalFace2D(new Polygon2D(point2Ds));
            Assert.NotNull(polygonalFace2D);

            return new Building2D(Guid.NewGuid(), Guid.NewGuid().ToString(), polygonalFace2D, 1, BuildingPhase.occupied, BuildingGeneralFunction.residential_buildings, [BuildingSpecificFunction.single_family_building]);
        }

        private static Building2D CreateBuilding2D(Point2D[] externalPoint2Ds, Point2D[] holePoint2Ds)
        {
            List<IPolygonal2D> internalEdges = [new Polygon2D(holePoint2Ds)];

            PolygonalFace2D? polygonalFace2D = Geometry.Planar.Create.PolygonalFace2D(new Polygon2D(externalPoint2Ds), internalEdges);
            Assert.NotNull(polygonalFace2D);
            Assert.True(polygonalFace2D.InternalEdges is not null && polygonalFace2D.InternalEdges.Count > 0);

            return new Building2D(Guid.NewGuid(), Guid.NewGuid().ToString(), polygonalFace2D, 1, BuildingPhase.occupied, BuildingGeneralFunction.residential_buildings, [BuildingSpecificFunction.single_family_building]);
        }
    }
}
