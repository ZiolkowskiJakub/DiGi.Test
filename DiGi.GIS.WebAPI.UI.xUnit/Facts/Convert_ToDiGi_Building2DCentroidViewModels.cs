using DiGi.GIS.WebAPI.UI.ViewModels;
using System.Collections.Generic;

namespace DiGi.GIS.WebAPI.UI.xUnit
{
    public partial class Facts
    {
        /// <summary>
        /// Verifies <see cref="Convert.ToDiGi_Building2DCentroidViewModels(string?)"/>, the reader of the compact centroid answer (DiGi.GIS.WebAPI.UI#51, DiGi.GIS.WebAPI#40).
        /// <para>The four arrays become view models in body order, and four empty arrays an empty list. A body that is not the compact shape is refused as a whole: not JSON, a missing or non-array member, arrays of unequal length, an entry of the wrong kind, a blank reference, or the full endpoint's object array. Null and blank bodies answer null.</para>
        /// </summary>
        [Fact]
        public void Convert_ToDiGi_Building2DCentroidViewModels()
        {
            List<Building2DCentroidViewModel>? building2DCentroidViewModels = Convert.ToDiGi_Building2DCentroidViewModels("{\"References\":[\"R1\",\"R2\"],\"CountyIds\":[78244,5],\"X\":[474823.58,-10.13],\"Y\":[251870,0.01]}");
            Assert.NotNull(building2DCentroidViewModels);
            Assert.Equal(2, building2DCentroidViewModels.Count);
            Assert.Equal("R1", building2DCentroidViewModels[0].Reference);
            Assert.Equal(78244, building2DCentroidViewModels[0].CountyId);
            Assert.Equal(474823.58, building2DCentroidViewModels[0].X);
            Assert.Equal(251870.0, building2DCentroidViewModels[0].Y);
            Assert.Equal("R2", building2DCentroidViewModels[1].Reference);
            Assert.Equal(5, building2DCentroidViewModels[1].CountyId);
            Assert.Equal(-10.13, building2DCentroidViewModels[1].X);

            List<Building2DCentroidViewModel>? building2DCentroidViewModels_Empty = Convert.ToDiGi_Building2DCentroidViewModels("{\"References\":[],\"CountyIds\":[],\"X\":[],\"Y\":[]}");
            Assert.NotNull(building2DCentroidViewModels_Empty);
            Assert.Empty(building2DCentroidViewModels_Empty);

            Assert.Null(Convert.ToDiGi_Building2DCentroidViewModels(null));
            Assert.Null(Convert.ToDiGi_Building2DCentroidViewModels("   "));
            Assert.Null(Convert.ToDiGi_Building2DCentroidViewModels("not json"));
            Assert.Null(Convert.ToDiGi_Building2DCentroidViewModels("{\"References\":[\"R1\"],\"CountyIds\":[1],\"X\":[1]}"));
            Assert.Null(Convert.ToDiGi_Building2DCentroidViewModels("{\"References\":[\"R1\"],\"CountyIds\":[1,2],\"X\":[1],\"Y\":[1]}"));
            Assert.Null(Convert.ToDiGi_Building2DCentroidViewModels("{\"References\":[\"R1\"],\"CountyIds\":[\"1\"],\"X\":[1],\"Y\":[1]}"));
            Assert.Null(Convert.ToDiGi_Building2DCentroidViewModels("{\"References\":[1],\"CountyIds\":[1],\"X\":[1],\"Y\":[1]}"));
            Assert.Null(Convert.ToDiGi_Building2DCentroidViewModels("{\"References\":[\" \"],\"CountyIds\":[1],\"X\":[1],\"Y\":[1]}"));
            Assert.Null(Convert.ToDiGi_Building2DCentroidViewModels("{\"References\":\"R1\",\"CountyIds\":[1],\"X\":[1],\"Y\":[1]}"));

            // The full endpoint's answer is not the compact shape - the caller then falls back to it by its own reader.
            Assert.Null(Convert.ToDiGi_Building2DCentroidViewModels("[{\"_type\":\"DiGi.GIS.PostgreSQL.Classes.Building2DCentroid,DiGi.GIS.PostgreSQL\",\"Reference\":\"R1\",\"CountyId\":5,\"X\":1,\"Y\":2}]"));
        }
    }
}
