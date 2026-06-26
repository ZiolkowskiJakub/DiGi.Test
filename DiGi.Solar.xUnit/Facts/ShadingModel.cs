using DiGi.Core.Classes;
using DiGi.Geometry.Spatial;
using DiGi.Geometry.Spatial.Classes;
using DiGi.Geometry.Spatial.Interfaces;
using DiGi.Solar.Classes;
using DiGi.Solar.Interfaces;
using System;
using System.Collections.Generic;

namespace DiGi.Solar.xUnit
{
    public partial class Facts
    {
        /// <summary>
        /// Tests that the shading model correctly assigns elements and solver results, and correctly evaluates exact shading factors and performs linear interpolation.
        /// </summary>
        [Fact]
        public void ShadingModel_TryGetShadingFactor()
        {
            Coordinates coordinates = new(50.0, 20.0);
            ShadingModel shadingModel = new(Core.Enums.UTC.Plus0100, coordinates);

            Plane plane_WorldZ = DiGi.Geometry.Spatial.Constants.Plane.WorldZ;
            List<Point3D> point3Ds = [new Point3D(0, 0, 0), new Point3D(10, 0, 0), new Point3D(10, 10, 0), new Point3D(0, 10, 0)];
            Polygon3D polygon3D_ExternalEdge = new(plane_WorldZ, point3Ds.ConvertAll(plane_WorldZ.Convert)!);
            IPolygonalFace3D? polygonalFace3D = DiGi.Geometry.Spatial.Create.PolygonalFace3D(polygon3D_ExternalEdge, []);
            Assert.NotNull(polygonalFace3D);

            ShadingElement shadingElement = new(polygonalFace3D, false);

            DateTime dateTime_Start = new(2026, 6, 26, 10, 0, 0);
            DateTime dateTime_Middle = new(2026, 6, 26, 11, 0, 0);
            DateTime dateTime_End = new(2026, 6, 26, 12, 0, 0);

            NumericalShadingSolverResult numericalShadingSolverResult_1 = new(dateTime_Start, 20.0);
            NumericalShadingSolverResult numericalShadingSolverResult_2 = new(dateTime_End, 80.0);

            List<IShadingSolverResult> shadingSolverResults = [numericalShadingSolverResult_1, numericalShadingSolverResult_2];

            bool isAssigned = shadingModel.Assign(shadingElement, shadingSolverResults);
            Assert.True(isAssigned);

            // Test exact match at start
            bool hasFactor_Start = shadingModel.TryGetShadingFactor(shadingElement, dateTime_Start, out double factor_Start);
            Assert.True(hasFactor_Start);
            Assert.Equal(0.2, factor_Start, 5);

            // Test exact match at end
            bool hasFactor_End = shadingModel.TryGetShadingFactor(shadingElement, dateTime_End, out double factor_End);
            Assert.True(hasFactor_End);
            Assert.Equal(0.8, factor_End, 5);

            // Test linear interpolation in the middle
            bool hasFactor_Middle = shadingModel.TryGetShadingFactor(shadingElement, dateTime_Middle, out double factor_Middle);
            Assert.True(hasFactor_Middle);
            Assert.Equal(0.5, factor_Middle, 5);

            // Test out of bounds (before start)
            DateTime dateTime_BeforeStart = new(2026, 6, 26, 9, 59, 59);
            bool hasFactor_BeforeStart = shadingModel.TryGetShadingFactor(shadingElement, dateTime_BeforeStart, out double factor_BeforeStart);
            Assert.False(hasFactor_BeforeStart);
            Assert.True(double.IsNaN(factor_BeforeStart));

            // Test out of bounds (after end)
            DateTime dateTime_AfterEnd = new(2026, 6, 26, 12, 0, 1);
            bool hasFactor_AfterEnd = shadingModel.TryGetShadingFactor(shadingElement, dateTime_AfterEnd, out double factor_AfterEnd);
            Assert.False(hasFactor_AfterEnd);
            Assert.True(double.IsNaN(factor_AfterEnd));

            // Test with interpolation disabled
            bool hasFactor_NoInterpolation = shadingModel.TryGetShadingFactor(shadingElement, dateTime_Middle, out double factor_NoInterpolation, false);
            Assert.False(hasFactor_NoInterpolation);
            Assert.True(double.IsNaN(factor_NoInterpolation));

            // Test that a NaN area result is ignored and does not corrupt interpolation
            DateTime dateTime_NaN = new(2026, 6, 26, 11, 30, 0);
            NumericalShadingSolverResult numericalShadingSolverResult_NaN = new(dateTime_NaN, double.NaN);

            List<IShadingSolverResult> shadingSolverResults_WithNaN = [numericalShadingSolverResult_1, numericalShadingSolverResult_NaN, numericalShadingSolverResult_2];
            bool isAssigned_WithNaN = shadingModel.Assign(shadingElement, shadingSolverResults_WithNaN);
            Assert.True(isAssigned_WithNaN);

            bool hasFactor_MiddleAfterNaN = shadingModel.TryGetShadingFactor(shadingElement, dateTime_Middle, out double factor_MiddleAfterNaN);
            Assert.True(hasFactor_MiddleAfterNaN);
            Assert.Equal(0.5, factor_MiddleAfterNaN, 5);
        }
    }
}
