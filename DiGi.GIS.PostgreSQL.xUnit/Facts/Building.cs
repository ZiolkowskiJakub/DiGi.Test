using DiGi.Core.Parameter.Classes;
using DiGi.GIS.PostgreSQL.Classes;

namespace DiGi.GIS.PostgreSQL.xUnit
{
    public partial class Facts
    {
        /// <summary>
        /// Tests that ToPostgreSQL correctly converts a CityGML building containing Year and LOD parameters.
        /// </summary>
        [Fact]
        public void Building_ToPostgreSQL_YearAndLOD()
        {
            CityGML.Classes.Building building = new("B_001", 0, null);
            SetValueSettings setValueSettings = new(true, false);
            building.SetValue(Enums.BuildingParameter.Year, (short)2024, setValueSettings);
            building.SetValue(Enums.BuildingParameter.LOD, CityGML.Enums.LOD.LOD2, setValueSettings);

            Building? postgresBuilding = building.ToPostgreSQL();
            Assert.NotNull(postgresBuilding);
            Assert.Equal((short)2024, postgresBuilding.Year);
            Assert.Equal(CityGML.Enums.LOD.LOD2, postgresBuilding.LOD);
        }
    }
}
