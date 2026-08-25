using DiGi.GIS.PostgreSQL.Classes;
using DiGi.GIS.PostgreSQL.Enums;
using DiGi.GIS.PostgreSQL.UI.Windows;
using System;
using System.Collections.Generic;
using System.Threading;

namespace DiGi.GIS.PostgreSQL.UI.xUnit
{
    public partial class Facts
    {
        /// <summary>
        /// Verifies that the building data options window can be built and that it works on a copy of the options it is given, without a database or a running application.
        /// <para>The window is the only way a run can be scoped, and a run left unscoped walks every subdivision in the country. A window whose markup failed to parse, or whose constructor threw on the county list, would not be found until someone opened it to scope a run and could not.</para>
        /// <para>What the constructor does is what this exercises: it parses the markup, wires the item naming of both lists, fills and sorts the counties, and restores the selection from the options. A multi-part county is included because two of its pieces share a code and a name and are told apart only by the identifier the naming callback appends.</para>
        /// <para><b>It does not check what the window looks like.</b> A window is laid out by the handle it gets when it is shown, so measuring one that is never shown reports nothing, and showing one during a test run would put a dialog on screen. The controls are private to the window, so the selection they end up holding cannot be read from here either - that the dialog behaves correctly on screen is still something a person has to look at once.</para>
        /// </summary>
        [Fact]
        public void PostgreSQLBuildingDataUpdateOptionsWindow_Construction()
        {
            Exception? exception = null;
            bool countiesCarried = false;
            bool updateTypesCarried = false;
            bool commandTimeoutCarried = false;
            bool copied = false;

            PostgreSQLBuildingDataUpdateOptions postgreSQLBuildingDataUpdateOptions = new()
            {
                BuildingDataUpdateTypes = [BuildingDataUpdateType.General, BuildingDataUpdateType.Database],
                CountyIds = [4816],
                CommandTimeout = 600
            };

            Thread thread = new(() =>
            {
                try
                {
                    List<AdministrativeAreal2DReference> administrativeAreal2DReferences =
                    [
                        new() { Id = 55417, Code = "1465", Name = "m. St. Warszawa", AdministrativeArealType = AdministrativeArealType.County },
                        new() { Id = 4816, Code = "0201", Name = "boleslawiecki", AdministrativeArealType = AdministrativeArealType.County },
                        // Two pieces of one multi-part county: same code and name, told apart only by the identifier.
                        new() { Id = 22138, Code = "2412", Name = "rybnicki", AdministrativeArealType = AdministrativeArealType.County },
                        new() { Id = 22139, Code = "2412", Name = "rybnicki", AdministrativeArealType = AdministrativeArealType.County }
                    ];

                    PostgreSQLBuildingDataUpdateOptionsWindow postgreSQLBuildingDataUpdateOptionsWindow = new(postgreSQLBuildingDataUpdateOptions, administrativeAreal2DReferences);

                    PostgreSQLBuildingDataUpdateOptions postgreSQLBuildingDataUpdateOptions_Held = postgreSQLBuildingDataUpdateOptionsWindow.PostgreSQLBuildingDataUpdateOptions;

                    // Until OK is pressed the window holds what it was given.
                    countiesCarried = postgreSQLBuildingDataUpdateOptions_Held.CountyIds is HashSet<int> countyIds && countyIds.Count == 1 && countyIds.Contains(4816);
                    updateTypesCarried = postgreSQLBuildingDataUpdateOptions_Held.BuildingDataUpdateTypes is HashSet<BuildingDataUpdateType> buildingDataUpdateTypes && buildingDataUpdateTypes.Count == 2 && buildingDataUpdateTypes.Contains(BuildingDataUpdateType.General);
                    commandTimeoutCarried = postgreSQLBuildingDataUpdateOptions_Held.CommandTimeout == 600;

                    // A cancelled dialog has to leave the caller's options alone, which only holds if the window
                    // took a copy rather than a reference.
                    copied = !ReferenceEquals(postgreSQLBuildingDataUpdateOptions, postgreSQLBuildingDataUpdateOptions_Held);
                    postgreSQLBuildingDataUpdateOptions_Held.CommandTimeout = 30;
                }
                catch (Exception exception_Temp)
                {
                    exception = exception_Temp;
                }
            });

            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();
            thread.Join();

            Assert.Null(exception);
            Assert.True(updateTypesCarried);
            Assert.True(countiesCarried);
            Assert.True(commandTimeoutCarried);
            Assert.True(copied);

            // The caller's own instance is untouched by the edit made to the window's copy above.
            Assert.Equal(600, postgreSQLBuildingDataUpdateOptions.CommandTimeout);
        }
    }
}
