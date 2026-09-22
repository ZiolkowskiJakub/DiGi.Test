using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using DiGi.Analytical.Building.HVAC.Classes;
using DiGi.GIS.Enums;
using DiGi.GIS.WebAPI.UI.Controllers;
using DiGi.GIS.WebAPI.UI.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace DiGi.GIS.WebAPI.UI.xUnit
{
    public partial class Facts
    {
        /// <summary>
        /// Tests the year selection of the regulated heat transfer coefficients read (#50): the user's exact year beats the stored prediction, user entries that are bounds alone fall through to the prediction, and no usable entry answers NoContent.
        /// <para>The coefficients answer is a fixed 2014 regulation, so every case is decided by the year built data alone and the residential read of the 2002 regulation never runs.</para>
        /// </summary>
        [Fact]
        public async Task GetRegulatedHeatTransferCoefficientsByReferenceAsync()
        {
            RegulatedHeatTransferCoefficients_2014 regulatedHeatTransferCoefficients_2014 = new((RegulationAct?)null);
            string regulatedHeatTransferCoefficientsBody = Core.Convert.ToSystem_String(regulatedHeatTransferCoefficients_2014) ?? string.Empty;

            // The user's exact year beats the stored prediction.
            DiGi.GIS.Classes.YearBuiltData yearBuiltData_Exact = new("3020");
            yearBuiltData_Exact.SetUserYearBuilt(1938);
            yearBuiltData_Exact.SetPredictedYearBuilt(new DateTime(2026, 1, 1), 2001);
            RouteStubWebApi routeStubWebApi_Exact = new RouteStubWebApi()
                .Answer("yearbuiltdata/itemsbyreference", HttpStatusCode.OK, Core.Convert.ToSystem_String(new List<DiGi.GIS.Classes.YearBuiltData> { yearBuiltData_Exact }) ?? string.Empty)
                .Answer("regulatedheattransfercoefficientsbyyear", HttpStatusCode.OK, regulatedHeatTransferCoefficientsBody);
            IActionResult result_Exact = await new HeatTransferCoefficientController(new HandlerHttpClientFactory(routeStubWebApi_Exact)).GetRegulatedHeatTransferCoefficientsByReferenceAsync("3020", null);
            PartialViewResult partialViewResult_Exact = Assert.IsType<PartialViewResult>(result_Exact);
            RegulatedHeatTransferCoefficientsViewModel regulatedHeatTransferCoefficientsViewModel_Exact = Assert.IsType<RegulatedHeatTransferCoefficientsViewModel>(partialViewResult_Exact.Model);
            Assert.Equal((short)1938, regulatedHeatTransferCoefficientsViewModel_Exact.Year);

            // A user entry that is a bound alone is not an exact year, so the prediction answers it.
            DiGi.GIS.Classes.YearBuiltData yearBuiltData_Bound = new("3020");
            yearBuiltData_Bound.SetUserYearBuilt(new DiGi.GIS.Classes.UserYearBuilt(1900, YearBuiltRelation.AtOrBefore));
            yearBuiltData_Bound.SetPredictedYearBuilt(new DateTime(2026, 1, 1), 2001);
            RouteStubWebApi routeStubWebApi_Bound = new RouteStubWebApi()
                .Answer("yearbuiltdata/itemsbyreference", HttpStatusCode.OK, Core.Convert.ToSystem_String(new List<DiGi.GIS.Classes.YearBuiltData> { yearBuiltData_Bound }) ?? string.Empty)
                .Answer("regulatedheattransfercoefficientsbyyear", HttpStatusCode.OK, regulatedHeatTransferCoefficientsBody);
            IActionResult result_Bound = await new HeatTransferCoefficientController(new HandlerHttpClientFactory(routeStubWebApi_Bound)).GetRegulatedHeatTransferCoefficientsByReferenceAsync("3020", null);
            PartialViewResult partialViewResult_Bound = Assert.IsType<PartialViewResult>(result_Bound);
            RegulatedHeatTransferCoefficientsViewModel regulatedHeatTransferCoefficientsViewModel_Bound = Assert.IsType<RegulatedHeatTransferCoefficientsViewModel>(partialViewResult_Bound.Model);
            Assert.Equal((short)2001, regulatedHeatTransferCoefficientsViewModel_Bound.Year);

            // No usable entry answers NoContent.
            RouteStubWebApi routeStubWebApi_Empty = new RouteStubWebApi()
                .Answer("yearbuiltdata/itemsbyreference", HttpStatusCode.OK, "[]");
            IActionResult result_Empty = await new HeatTransferCoefficientController(new HandlerHttpClientFactory(routeStubWebApi_Empty)).GetRegulatedHeatTransferCoefficientsByReferenceAsync("3020", null);
            Assert.IsType<NoContentResult>(result_Empty);
        }
    }
}
