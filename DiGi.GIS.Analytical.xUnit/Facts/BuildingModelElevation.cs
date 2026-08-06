using DiGi.Analytical.Building.Classes;
using DiGi.CityGML.Classes;
using DiGi.Geometry.Spatial.Classes;
using DiGi.GIS.Classes;
using DiGi.GIS.Enums;
using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace DiGi.GIS.Analytical.xUnit
{
    public partial class Facts
    {
        /// <summary>
        /// An HTTP message handler standing in for an unreachable terrain service.
        /// <para>Every request throws, which is what a name that does not resolve or a service that is down amounts to for the caller, and keeps the fact using it off the network.</para>
        /// </summary>
        private sealed class UnreachableHandler : HttpMessageHandler
        {
            /// <summary>
            /// Fails every request.
            /// </summary>
            /// <param name="request">The request that would have been sent.</param>
            /// <param name="cancellationToken">The token used to cancel the request.</param>
            /// <returns>Never returns - always throws.</returns>
            protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            {
                throw new HttpRequestException("The terrain service is unreachable.");
            }
        }

        /// <summary>
        /// Tests that the base elevation is carried into the model extruded from a 2D building.
        /// <para>Every footprint used to be extruded from the world XY plane, so a model built from a footprint floated at sea level while one built from CityGML sat on real terrain and the two could not be shown in one scene. A ten by ten footprint of two storeys raised to a hundred metres has to span exactly a hundred to a hundred and six metres.</para>
        /// </summary>
        [Fact]
        public void BuildingModel_Building2D_Elevation()
        {
            double elevation = 100;
            double storeyHeight = 3.0;
            ushort storeys = 2;

            Building2D building2D = Building2D_Rectangle(10, 10, storeys, BuildingGeneralFunction.residential_buildings);

            BuildingModel? buildingModel = Create.BuildingModel(building2D, elevation, storeyHeight);

            Assert.NotNull(buildingModel);

            BoundingBox3D? boundingBox3D = buildingModel.GetBoundingBox();
            Assert.NotNull(boundingBox3D);

            double maxZ_Expected = elevation + (storeys * storeyHeight);

            Assert.True(Math.Abs(boundingBox3D.MinZ - elevation) < Core.Constants.Tolerance.Distance, $"MinZ is {boundingBox3D.MinZ} instead of {elevation}.");
            Assert.True(Math.Abs(boundingBox3D.MaxZ - maxZ_Expected) < Core.Constants.Tolerance.Distance, $"MaxZ is {boundingBox3D.MaxZ} instead of {maxZ_Expected}.");

            Core.xUnit.Query.SerializationCheck(buildingModel);
        }

        /// <summary>
        /// Tests that the default elevation leaves the model on the world XY plane.
        /// <para>The elevation was inserted ahead of the storey height and the tolerance, so the default has to reproduce the behaviour of every caller written before it existed.</para>
        /// </summary>
        [Fact]
        public void BuildingModel_Building2D_ElevationDefault()
        {
            Building2D building2D = Building2D_Rectangle(10, 10, 2, BuildingGeneralFunction.residential_buildings);

            BuildingModel? buildingModel = Create.BuildingModel(building2D);

            Assert.NotNull(buildingModel);

            BoundingBox3D? boundingBox3D = buildingModel.GetBoundingBox();
            Assert.NotNull(boundingBox3D);

            Assert.True(Math.Abs(boundingBox3D.MinZ) < Core.Constants.Tolerance.Distance, $"MinZ is {boundingBox3D.MinZ} instead of 0.");
            Assert.True(Math.Abs(boundingBox3D.Height - (2 * 3.0)) < Core.Constants.Tolerance.Distance, $"The model is {boundingBox3D.Height} high instead of {2 * 3.0}.");
        }

        /// <summary>
        /// Tests that a not-a-number elevation refuses to extrude the 2D building.
        /// <para>Not-a-number means that no elevation is known, and the creator returns null rather than placing the building at a guessed height. The asynchronous creator relies on that signal to decide whether the terrain service has to be queried, so it is a contract rather than an implementation detail.</para>
        /// </summary>
        [Fact]
        public void BuildingModel_Building2D_ElevationNaN()
        {
            Building2D building2D = Building2D_Rectangle(10, 10, 2, BuildingGeneralFunction.residential_buildings);

            Assert.Null(Create.BuildingModel(building2D, double.NaN));
        }

        /// <summary>
        /// Tests that a not-a-number elevation refuses the extruded fallback of the overload taking a CityGML building.
        /// <para>The CityGML building is null here, so the fallback is the only path left and the model may not be created.</para>
        /// </summary>
        [Fact]
        public void BuildingModel_BuildingAndBuilding2D_ElevationNaN_NullBuilding()
        {
            Building? building = null;

            Building2D building2D = Building2D_Rectangle(10, 10, 2, BuildingGeneralFunction.residential_buildings);

            Assert.Null(Create.BuildingModel(building, building2D, double.NaN));
        }

        /// <summary>
        /// Tests that the elevation is ignored when the CityGML geometry converts.
        /// <para>The 3D geometry carries its own elevations, so the elevation passed alongside it is read only on the extruded fallback. A building modelled between zero and nine metres has to stay there whatever elevation accompanies it, otherwise a terrain query would silently move buildings that never needed one.</para>
        /// </summary>
        [Fact]
        public void BuildingModel_BuildingAndBuilding2D_ElevationIgnored()
        {
            Building building = CityGML_Building(new BoundingBox3D(new Point3D(0, 0, 0), new Point3D(10, 10, 9)), "Building 1");

            Building2D building2D = Building2D_Rectangle(10, 10, 3, BuildingGeneralFunction.residential_buildings);

            BuildingModel? buildingModel = Create.BuildingModel(building, building2D, 100);

            Assert.NotNull(buildingModel);

            BoundingBox3D? boundingBox3D = buildingModel.GetBoundingBox();
            Assert.NotNull(boundingBox3D);

            Assert.True(Math.Abs(boundingBox3D.MinZ) < Core.Constants.Tolerance.Distance, $"MinZ is {boundingBox3D.MinZ} - the elevation moved a model built from 3D geometry.");
            Assert.True(Math.Abs(boundingBox3D.MaxZ - 9) < Core.Constants.Tolerance.Distance, $"MaxZ is {boundingBox3D.MaxZ} instead of 9.");
        }

        /// <summary>
        /// Tests the null handling of both asynchronous creators.
        /// <para>A null client and a 2D building carrying no footprint are caller errors rather than service failures, so they return null without reaching the network.</para>
        /// </summary>
        [Fact]
        public async Task BuildingModelAsync_NullInputs()
        {
            HttpClient? httpClient_Null = null;
            Building2D? building2D_Null = null;
            Building? building_Null = null;

            Assert.Null(await httpClient_Null.BuildingModelAsync(building2D_Null));
            Assert.Null(await httpClient_Null.BuildingModelAsync(building_Null, building2D_Null));

            using HttpClient httpClient = new(new UnreachableHandler());

            Assert.Null(await httpClient.BuildingModelAsync(building2D_Null));

            Building2D building2D = Building2D_Rectangle(10, 10, 2, BuildingGeneralFunction.residential_buildings);

            Assert.Null(await httpClient_Null.BuildingModelAsync(building2D));
        }

        /// <summary>
        /// Tests that an unreachable terrain service still yields a model, extruded from an elevation of zero.
        /// <para>The elevation enriches the model, it is not a precondition. The bulk import treats a null result as a building it cannot upload, so returning null on a failed terrain query would let an outage of a third party service quietly empty a whole county.</para>
        /// </summary>
        [Fact]
        public async Task BuildingModelAsync_ElevationUnavailable()
        {
            Building2D building2D = Building2D_Rectangle(10, 10, 2, BuildingGeneralFunction.residential_buildings);

            using HttpClient httpClient = new(new UnreachableHandler());

            BuildingModel? buildingModel = await httpClient.BuildingModelAsync(building2D, storeyHeight: 3.0);

            Assert.NotNull(buildingModel);

            BoundingBox3D? boundingBox3D = buildingModel.GetBoundingBox();
            Assert.NotNull(boundingBox3D);

            Assert.True(Math.Abs(boundingBox3D.MinZ) < Core.Constants.Tolerance.Distance, $"MinZ is {boundingBox3D.MinZ} instead of 0.");
            Assert.True(Math.Abs(boundingBox3D.Height - 6) < Core.Constants.Tolerance.Distance, $"The model is {boundingBox3D.Height} high instead of 6.");
        }

        /// <summary>
        /// Tests that an unreachable terrain service still yields a model on the overload taking a CityGML building.
        /// <para>The CityGML building is null, so the extruded fallback is the only path left and it has to survive the failed terrain query rather than propagate the null the first attempt returns by design.</para>
        /// </summary>
        [Fact]
        public async Task BuildingModelAsync_ElevationUnavailable_NullBuilding()
        {
            Building? building = null;

            Building2D building2D = Building2D_Rectangle(10, 10, 2, BuildingGeneralFunction.residential_buildings);

            using HttpClient httpClient = new(new UnreachableHandler());

            BuildingModel? buildingModel = await httpClient.BuildingModelAsync(building, building2D);

            Assert.NotNull(buildingModel);

            BoundingBox3D? boundingBox3D = buildingModel.GetBoundingBox();
            Assert.NotNull(boundingBox3D);

            Assert.True(Math.Abs(boundingBox3D.MinZ) < Core.Constants.Tolerance.Distance, $"MinZ is {boundingBox3D.MinZ} instead of 0.");
            Assert.True(Math.Abs(boundingBox3D.Height - (2 * Constants.StoreyHeight.Default)) < Core.Constants.Tolerance.Distance, $"The model is {boundingBox3D.Height} high instead of {2 * Constants.StoreyHeight.Default}.");
        }
    }
}
