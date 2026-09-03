using DiGi.GIS.PostgreSQL.Classes;
using DiGi.GIS.PostgreSQL.UI.Classes;
using DiGi.GIS.PostgreSQL.UI.Enums;
using DiGi.GIS.WebAPI.Classes;
using DiGi.UI.WPF.Interfaces;
using System.Collections.Generic;

namespace DiGi.GIS.PostgreSQL.UI.xUnit
{
    public partial class Facts
    {
        /// <summary>
        /// Tests that <see cref="Create.VisualBackgroundTasks(GISPostgreSQLConverterManager?, GISWebAPIManager?, Mode)"/> handles null managers without throwing exceptions, ensuring that a missing WebAPI configuration does not crash the server task list.
        /// </summary>
        [Fact]
        public void VisualBackgroundTasks()
        {
            // Null converter manager and null WebAPI manager should return empty list across all modes
            List<IVisualBackgroundTask>? visualBackgroundTasks_NullManagers_Server = Create.VisualBackgroundTasks(null, null, Mode.Server);
            Assert.NotNull(visualBackgroundTasks_NullManagers_Server);
            Assert.Empty(visualBackgroundTasks_NullManagers_Server);

            List<IVisualBackgroundTask>? visualBackgroundTasks_NullManagers_Client = Create.VisualBackgroundTasks(null, null, Mode.Client);
            Assert.NotNull(visualBackgroundTasks_NullManagers_Client);
            Assert.Empty(visualBackgroundTasks_NullManagers_Client);

            List<IVisualBackgroundTask>? visualBackgroundTasks_NullManagers_Both = Create.VisualBackgroundTasks(null, null, Mode.ServerAndCient);
            Assert.NotNull(visualBackgroundTasks_NullManagers_Both);
            Assert.Empty(visualBackgroundTasks_NullManagers_Both);

            // With converter manager but null WebAPI manager (the issue #1 scenario)
            GISPostgreSQLConverterManager gISPostgreSQLConverterManager = new();

            List<IVisualBackgroundTask>? visualBackgroundTasks_Server = Create.VisualBackgroundTasks(gISPostgreSQLConverterManager, null, Mode.Server);
            Assert.NotNull(visualBackgroundTasks_Server);
            Assert.NotEmpty(visualBackgroundTasks_Server);

            // OrtoDatasTask requires GISWebAPIManager and must be excluded when it is null
            Assert.DoesNotContain(visualBackgroundTasks_Server, x => x.TypeName == typeof(OrtoDatasTask).Name);

            // Client mode with null WebAPI manager should return empty list
            List<IVisualBackgroundTask>? visualBackgroundTasks_Client = Create.VisualBackgroundTasks(gISPostgreSQLConverterManager, null, Mode.Client);
            Assert.NotNull(visualBackgroundTasks_Client);
            Assert.Empty(visualBackgroundTasks_Client);
        }

        /// <summary>
        /// Tests that the Year Built prediction task is offered where it can actually run, and nowhere else.
        /// <para>The task reads the county rows the dialog is scoped from over the Web API, so a null manager has to leave it out rather than produce a row that throws when it is clicked - the failure ZiolkowskiJakub/DiGi.GIS.PostgreSQL.UI#1 was about. It belongs to the client side for the same reason: it holds no PostgreSQL converter and reaches the estate only through the API.</para>
        /// <para>It is registered rather than constructed here, because constructing it proves nothing - the registration is what decides whether the row appears at all, and a task added under the wrong mode builds and tests green while being invisible in the tab an operator opens.</para>
        /// </summary>
        [Fact]
        public void VisualBackgroundTasks_UIYearBuiltPredictionsTask()
        {
            // Any non-empty key builds a manager. Nothing here reaches the network - the task is registered, never started.
            GISWebAPIManager? gISWebAPIManager = DiGi.GIS.WebAPI.Create.GISWebAPIManager("00000000-0000-0000-0000-000000000000");
            Assert.NotNull(gISWebAPIManager);

            List<IVisualBackgroundTask>? visualBackgroundTasks_Client = Create.VisualBackgroundTasks(null, gISWebAPIManager, Mode.Client);
            Assert.NotNull(visualBackgroundTasks_Client);
            Assert.Contains(visualBackgroundTasks_Client, x => x.TypeName == typeof(UIYearBuiltPredictionsTask).Name);

            // The server tab holds the tasks driven by a PostgreSQL converter; this one is not among them.
            List<IVisualBackgroundTask>? visualBackgroundTasks_Server = Create.VisualBackgroundTasks(new GISPostgreSQLConverterManager(), gISWebAPIManager, Mode.Server);
            Assert.NotNull(visualBackgroundTasks_Server);
            Assert.DoesNotContain(visualBackgroundTasks_Server, x => x.TypeName == typeof(UIYearBuiltPredictionsTask).Name);

            // Without a manager there is nothing to read the counties with, so the row must not be offered at all.
            List<IVisualBackgroundTask>? visualBackgroundTasks_NoManager = Create.VisualBackgroundTasks(new GISPostgreSQLConverterManager(), null, Mode.ServerAndCient);
            Assert.NotNull(visualBackgroundTasks_NoManager);
            Assert.DoesNotContain(visualBackgroundTasks_NoManager, x => x.TypeName == typeof(UIYearBuiltPredictionsTask).Name);
        }
    }
}
