using DiGi.Core.Enums;
using DiGi.PostgreSQL.Table.Classes;
using System.Collections.Generic;

namespace DiGi.GIS.WebAPI.UI.xUnit
{
    public static partial class Create
    {
        /// <summary>
        /// Creates a stand-in for the building data column catalog the deployed GIS Web API lists, with one column of each
        /// kind a Typology definition rule distinguishes: an integer, a floating point and a string column. The unique
        /// identifiers are the slugs of the names, as on the live catalog, so <c>Core.IO.Query.UniqueId</c> resolves them.
        /// </summary>
        /// <returns>The columns.</returns>
        public static List<Column> BuildingDataColumns()
        {
            return
            [
                new Column() { Name = "Predicted year built", UniqueId = "predicted_year_built", Index = 2, DataType = DataType.UShort, Category = "Year built" },
                new Column() { Name = "Occupancy", UniqueId = "occupancy", Index = 5, DataType = DataType.String },
                new Column() { Name = "Floor area", UniqueId = "floor_area", Index = 7, DataType = DataType.Double }
            ];
        }
    }
}
