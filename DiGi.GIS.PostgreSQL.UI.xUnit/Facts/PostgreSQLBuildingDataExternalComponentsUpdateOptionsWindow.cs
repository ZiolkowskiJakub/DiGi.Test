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
        /// Verifies that the external components area options window can be built, that it works on a copy of the options it is given, and that a county set it was not given stays a county set of nothing, all without a database or a running application.
        /// <para>The window is the only way a run can be scoped, and a run left unscoped reads every stored building model in the country. A window whose markup failed to parse, or whose constructor threw on the county list, would not be found until someone opened it to scope a run and could not.</para>
        /// <para>What the constructor does is what this exercises: it parses the markup, wires the item naming of the county list, fills and sorts the counties, and decides the selection - the given county set when the options carry one, the default of every county when they do not. A multi-part county is included because two of its pieces share a code and a name and are told apart only by the identifier the naming callback appends.</para>
        /// <para><b>It does not check what the window looks like.</b> A window is laid out by the handle it gets when it is shown, so measuring one that is never shown reports nothing, and showing one during a test run would put a dialog on screen. The controls are private to the window, so the selection they end up holding cannot be read from here either - that the default selection is the whole country, and that the dialog behaves correctly on screen, is something a person has to look at once.</para>
        /// </summary>
        [Fact]
        public void PostgreSQLBuildingDataExternalComponentsUpdateOptionsWindow_Construction()
        {
            Exception? exception = null;
            bool countiesCarried = false;
            bool commandTimeoutCarried = false;
            bool copied = false;
            bool countySetKeptAbsent = false;

            PostgreSQLBuildingDataExternalComponentsUpdateOptions postgreSQLBuildingDataExternalComponentsUpdateOptions = new()
            {
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

                    PostgreSQLBuildingDataExternalComponentsUpdateOptionsWindow postgreSQLBuildingDataExternalComponentsUpdateOptionsWindow = new(postgreSQLBuildingDataExternalComponentsUpdateOptions, administrativeAreal2DReferences);

                    PostgreSQLBuildingDataExternalComponentsUpdateOptions postgreSQLBuildingDataExternalComponentsUpdateOptions_Held = postgreSQLBuildingDataExternalComponentsUpdateOptionsWindow.PostgreSQLBuildingDataExternalComponentsUpdateOptions;

                    // Until OK is pressed the window holds what it was given.
                    countiesCarried = postgreSQLBuildingDataExternalComponentsUpdateOptions_Held.CountyIds is HashSet<int> countyIds && countyIds.Count == 1 && countyIds.Contains(4816);
                    commandTimeoutCarried = postgreSQLBuildingDataExternalComponentsUpdateOptions_Held.CommandTimeout == 600;

                    // A cancelled dialog has to leave the caller's options alone, which only holds if the window
                    // took a copy rather than a reference.
                    copied = !ReferenceEquals(postgreSQLBuildingDataExternalComponentsUpdateOptions, postgreSQLBuildingDataExternalComponentsUpdateOptions_Held);
                    postgreSQLBuildingDataExternalComponentsUpdateOptions_Held.CommandTimeout = 30;

                    // No county set in the options is the "every county" scope: the window selects all the counties
                    // it shows, but the options it holds stay as they were opened with until OK is pressed - so the
                    // held county set must still be absent here, and that is what keeps a cancelled dialog from
                    // turning a scoped run into a national one.
                    PostgreSQLBuildingDataExternalComponentsUpdateOptions postgreSQLBuildingDataExternalComponentsUpdateOptions_Default = new();

                    PostgreSQLBuildingDataExternalComponentsUpdateOptionsWindow postgreSQLBuildingDataExternalComponentsUpdateOptionsWindow_Default = new(postgreSQLBuildingDataExternalComponentsUpdateOptions_Default, administrativeAreal2DReferences);

                    countySetKeptAbsent = postgreSQLBuildingDataExternalComponentsUpdateOptionsWindow_Default.PostgreSQLBuildingDataExternalComponentsUpdateOptions.CountyIds is null;
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
            Assert.True(countiesCarried);
            Assert.True(commandTimeoutCarried);
            Assert.True(copied);
            Assert.True(countySetKeptAbsent);

            // The caller's own instance is untouched by the edit made to the window's copy above.
            Assert.Equal(600, postgreSQLBuildingDataExternalComponentsUpdateOptions.CommandTimeout);
        }
    }
}
