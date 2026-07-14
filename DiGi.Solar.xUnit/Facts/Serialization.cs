using DiGi.Core.Classes;
using DiGi.Geometry.Planar.Classes;
using DiGi.Geometry.Planar.Interfaces;
using DiGi.Geometry.Spatial.Classes;
using DiGi.Geometry.Spatial.Interfaces;
using DiGi.Solar.Classes;
using DiGi.Solar.ComputeSharp.Classes;
using DiGi.Solar.Enums;
using DiGi.Solar.Interfaces;
using System;
using System.Collections.Generic;
using System.Runtime.Versioning;

namespace DiGi.Solar.xUnit
{
    public partial class Facts
    {
        /// <summary>
        /// Tests the serialization round-trip and cloning of GeometricalShadingSolverResult.
        /// </summary>
        [Fact]
        public void GeometricalShadingSolverResult_Serialization()
        {
            DateTime dateTime = new(2026, 6, 26, 12, 0, 0);
            Plane plane = Geometry.Spatial.Constants.Plane.WorldZ;
            Polygon2D externalEdge = new([new Point2D(0.0, 0.0), new Point2D(10.0, 0.0), new Point2D(10.0, 10.0), new Point2D(0.0, 10.0)]);
            PolygonalFace2D polygonalFace2D = DiGi.Geometry.Planar.Create.PolygonalFace2D(externalEdge, [])!;
            List<IPolygonalFace2D> polygonalFace2Ds = [polygonalFace2D];

            GeometricalShadingSolverResult geometricalShadingSolverResult = new(dateTime, plane, polygonalFace2Ds);

            Assert.Equal(dateTime, geometricalShadingSolverResult.DateTime);
            Assert.NotNull(geometricalShadingSolverResult.Plane);
            Assert.NotNull(geometricalShadingSolverResult.PolygonalFace2Ds);

            Core.xUnit.Query.SerializationCheck(geometricalShadingSolverResult);
        }

        /// <summary>
        /// Tests the serialization round-trip and cloning of NumericalShadingSolverResult.
        /// </summary>
        [Fact]
        public void NumericalShadingSolverResult_Serialization()
        {
            DateTime dateTime = new(2026, 6, 26, 12, 0, 0);
            double area = 15.5;

            NumericalShadingSolverResult numericalShadingSolverResult = new(dateTime, area);

            Assert.Equal(dateTime, numericalShadingSolverResult.DateTime);
            Assert.Equal(area, numericalShadingSolverResult.Area);

            Core.xUnit.Query.SerializationCheck(numericalShadingSolverResult);
        }

        /// <summary>
        /// Tests the serialization round-trip and cloning of ShadingElement.
        /// </summary>
        [Fact]
        public void ShadingElement_Serialization()
        {
            Plane plane = Geometry.Spatial.Constants.Plane.WorldZ;
            IPolygonalFace3D polygonalFace3D = Geometry.Spatial.Create.PolygonalFace3D(plane, new Point2D(0.0, 0.0), new Point2D(10.0, 0.0), new Point2D(10.0, 10.0), new Point2D(0.0, 10.0))!;
            Guid guid = Guid.NewGuid();
            string reference = "Shading-Element-1";

            ShadingElement shadingElement = new(guid, reference, polygonalFace3D, true);

            Assert.Equal(guid, shadingElement.Guid);
            Assert.Equal(reference, shadingElement.Reference);
            Assert.NotNull(shadingElement.PolygonalFace3D);
            Assert.True(shadingElement.ShadingOnly);

            Core.xUnit.Query.SerializationCheck(shadingElement);
        }

        /// <summary>
        /// Tests the serialization round-trip and cloning of ShadingModel and its relation cluster components.
        /// </summary>
        [Fact]
        public void ShadingModel_Serialization()
        {
            Coordinates coordinates = new(50.0, 20.0);
            ShadingModel shadingModel = new(Core.Enums.UTC.Plus0100, coordinates);

            Plane plane = Geometry.Spatial.Constants.Plane.WorldZ;
            IPolygonalFace3D polygonalFace3D = Geometry.Spatial.Create.PolygonalFace3D(plane, new Point2D(0.0, 0.0), new Point2D(10.0, 0.0), new Point2D(10.0, 10.0), new Point2D(0.0, 10.0))!;
            ShadingElement shadingElement = new(Guid.NewGuid(), "Shading-Element-1", polygonalFace3D, false);
            NumericalShadingSolverResult numericalShadingSolverResult = new(new DateTime(2026, 6, 26, 12, 0, 0), 15.5);

            bool isAssigned = shadingModel.Assign(shadingElement, [numericalShadingSolverResult]);
            Assert.True(isAssigned);

            Assert.NotNull(shadingModel.Coordinates);
            Assert.Equal(Core.Enums.UTC.Plus0100, shadingModel.UTC);

            Core.xUnit.Query.SerializationCheck(shadingModel);
        }

        /// <summary>
        /// Tests the serialization round-trip and cloning of ShadingRelationCluster.
        /// </summary>
        [Fact]
        public void ShadingRelationCluster_Serialization()
        {
            Plane plane = Geometry.Spatial.Constants.Plane.WorldZ;
            IPolygonalFace3D polygonalFace3D = Geometry.Spatial.Create.PolygonalFace3D(plane, new Point2D(0.0, 0.0), new Point2D(10.0, 0.0), new Point2D(10.0, 10.0), new Point2D(0.0, 10.0))!;
            ShadingElement shadingElement = new(Guid.NewGuid(), "Shading-Element-1", polygonalFace3D, false);
            NumericalShadingSolverResult numericalShadingSolverResult = new(new DateTime(2026, 6, 26, 12, 0, 0), 15.5);

            ShadingRelationCluster shadingRelationCluster = [];
            shadingRelationCluster.Add(shadingElement);
            shadingRelationCluster.Add(numericalShadingSolverResult);
            shadingRelationCluster.AddRelation(shadingElement, [numericalShadingSolverResult]);

            Core.xUnit.Query.SerializationCheck(shadingRelationCluster);
        }

        /// <summary>
        /// Tests the serialization round-trip and cloning of ShadingSolverResultRelation.
        /// </summary>
        [Fact]
        public void ShadingSolverResultRelation_Serialization()
        {
            Plane plane = Geometry.Spatial.Constants.Plane.WorldZ;
            IPolygonalFace3D polygonalFace3D = Geometry.Spatial.Create.PolygonalFace3D(plane, new Point2D(0.0, 0.0), new Point2D(10.0, 0.0), new Point2D(10.0, 10.0), new Point2D(0.0, 10.0))!;
            ShadingElement shadingElement = new(Guid.NewGuid(), "Shading-Element-1", polygonalFace3D, false);
            NumericalShadingSolverResult numericalShadingSolverResult = new(new DateTime(2026, 6, 26, 12, 0, 0), 15.5);

            ShadingSolverResultRelation shadingSolverResultRelation = new(shadingElement, numericalShadingSolverResult);

            Core.xUnit.Query.SerializationCheck(shadingSolverResultRelation);
        }

        /// <summary>
        /// Tests the serialization round-trip and cloning of ShadingSolverOptions.
        /// </summary>
        [Fact]
        [SupportedOSPlatform("windows")]
        public void ShadingSolverOptions_Serialization()
        {
            ShadingSolverOptions shadingSolverOptions = new();
            shadingSolverOptions.AngleTolerance = 0.05;
            shadingSolverOptions.ShadingSolverType = ShadingSolverType.Geometrical;
            shadingSolverOptions.Tolerance = 1e-4;
            shadingSolverOptions.TimeSeries = new DateTimeSeries(new DateTime(2026, 6, 26, 12, 0, 0));

            Assert.Equal(0.05, shadingSolverOptions.AngleTolerance);
            Assert.Equal(ShadingSolverType.Geometrical, shadingSolverOptions.ShadingSolverType);
            Assert.Equal(1e-4, shadingSolverOptions.Tolerance);
            Assert.NotNull(shadingSolverOptions.TimeSeries);

            Core.xUnit.Query.SerializationCheck(shadingSolverOptions);
        }
    }
}
