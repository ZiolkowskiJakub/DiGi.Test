using DiGi.GIS.WebAPI.Classes;
using System.Collections.Generic;

namespace DiGi.GIS.WebAPI.xUnit
{
    public partial class Facts
    {
        /// <summary>
        /// Verifies that the <see cref="Building2DReferencesByPagingParameter"/> copy constructor copies all properties correctly.
        /// </summary>
        [Fact]
        public void Building2DReferencesByPagingParameter_CopyConstructor()
        {
            Building2DReferencesByPagingParameter parameter_Source = new()
            {
                CountyId = 10365,
                SubdivisionId = 1035,
                PageSize = 500,
                Cursor = "BLDG-12345"
            };

            Building2DReferencesByPagingParameter parameter_Copy = new(parameter_Source);

            Assert.Equal(parameter_Source.CountyId, parameter_Copy.CountyId);
            Assert.Equal(parameter_Source.SubdivisionId, parameter_Copy.SubdivisionId);
            Assert.Equal(parameter_Source.PageSize, parameter_Copy.PageSize);
            Assert.Equal(parameter_Source.Cursor, parameter_Copy.Cursor);
        }

        /// <summary>
        /// Verifies that the <see cref="Building2DReferencesByPagingParameter"/> copy constructor handles null source without throwing.
        /// </summary>
        [Fact]
        public void Building2DReferencesByPagingParameter_CopyConstructor_NullSource()
        {
            Building2DReferencesByPagingParameter parameter_Copy = new((Building2DReferencesByPagingParameter)null!);

            Assert.Equal(0, parameter_Copy.CountyId);
            Assert.Null(parameter_Copy.SubdivisionId);
            Assert.Equal(250, parameter_Copy.PageSize);
            Assert.Null(parameter_Copy.Cursor);
        }

        /// <summary>
        /// Verifies that the <see cref="BuildingDataByReferencesParameter"/> copy constructor copies all properties correctly.
        /// </summary>
        [Fact]
        public void BuildingDataByReferencesParameter_CopyConstructor()
        {
            BuildingDataByReferencesParameter parameter_Source = new()
            {
                CountyId = 2001,
                References = ["REF-1", "REF-2"],
                ColumnUniqueIds = ["col_1", "col_2"]
            };

            BuildingDataByReferencesParameter parameter_Copy = new(parameter_Source);

            Assert.Equal(parameter_Source.CountyId, parameter_Copy.CountyId);
            Assert.Equal(parameter_Source.References, parameter_Copy.References);
            Assert.Equal(parameter_Source.ColumnUniqueIds, parameter_Copy.ColumnUniqueIds);
        }

        /// <summary>
        /// Verifies that the <see cref="BuildingDataBySubdivisionIdsParameter"/> copy constructor copies all properties correctly.
        /// </summary>
        [Fact]
        public void BuildingDataBySubdivisionIdsParameter_CopyConstructor()
        {
            BuildingDataBySubdivisionIdsParameter parameter_Source = new()
            {
                SubdivisionIds = [101, 102],
                ColumnUniqueIds = ["col_1", "col_2"]
            };

            BuildingDataBySubdivisionIdsParameter parameter_Copy = new(parameter_Source);

            Assert.Equal(parameter_Source.SubdivisionIds, parameter_Copy.SubdivisionIds);
            Assert.Equal(parameter_Source.ColumnUniqueIds, parameter_Copy.ColumnUniqueIds);
        }

        /// <summary>
        /// Verifies that the <see cref="CountByAdministrativeAreal2DIdsParameter"/> copy constructor copies all properties correctly.
        /// </summary>
        [Fact]
        public void CountByAdministrativeAreal2DIdsParameter_CopyConstructor()
        {
            CountByAdministrativeAreal2DIdsParameter parameter_Source = new()
            {
                AdministrativeAreal2DIds = [1, 2, 3]
            };

            CountByAdministrativeAreal2DIdsParameter parameter_Copy = new(parameter_Source);

            Assert.Equal(parameter_Source.AdministrativeAreal2DIds, parameter_Copy.AdministrativeAreal2DIds);
        }
    }
}
