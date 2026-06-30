using DiGi.GIS.Classes;
using System.Collections.Generic;
using System.Reflection;

namespace DiGi.GIS.xUnit
{
    public partial class Facts
    {
        /// <summary>Loads a real GIS model from the 0207_GML.gmf sample file and recalculates occupancy, verifying that the calculation completes without throwing and assigns occupancy results to the model's administrative subdivisions.</summary>
        [Fact]
        public void CalculateOccupancy()
        {
            string? path = Core.xUnit.Query.FilePath(Assembly.GetExecutingAssembly(), "0207_GML.gmf");
            Assert.False(string.IsNullOrWhiteSpace(path));
            Assert.True(System.IO.File.Exists(path));

            GISModel? gISModel = null;
            using (GISModelFile gISModelFile = new(path))
            {
                Assert.True(gISModelFile.Open());
                gISModel = gISModelFile.Value;
            }

            Assert.NotNull(gISModel);

            List<Building2D>? building2Ds = gISModel.GetObjects<Building2D>();
            Assert.NotNull(building2Ds);
            Assert.NotEmpty(building2Ds);

            List<AdministrativeSubdivision>? administrativeSubdivisions = gISModel.GetObjects<AdministrativeSubdivision>();
            Assert.NotNull(administrativeSubdivisions);
            Assert.NotEmpty(administrativeSubdivisions);

            gISModel.CalculateOccupancy();

            int occupancyResultCount = 0;
            foreach (AdministrativeSubdivision administrativeSubdivision in administrativeSubdivisions)
            {
                if (gISModel.GetRelatedObject<OccupancyCalculationResult>(administrativeSubdivision) is not null)
                {
                    occupancyResultCount++;
                }
            }

            Assert.True(occupancyResultCount > 0);
        }
    }
}
