using DiGi.GIS.PostgreSQL.Enums;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace DiGi.WebAPI.WindowsService.xUnit
{
    public partial class Facts
    {
        /// <summary>
        /// Serves <see cref="EnumSerializableObjectFixture"/> so that a whole document can be generated through the host's configuration: an enum query parameter, which binds the member name or the integer, beside a DiGi payload whose enum members travel as integers.
        /// <para>Nested, so MVC does not discover it on its own; <see cref="ControllerFeatureProviderFixture"/> adds it.</para>
        /// </summary>
        [ApiController]
        [Route("fixture")]
        public sealed class EnumControllerFixture : ControllerBase
        {
            /// <summary>
            /// Returns a fixture payload at the requested administrative level.
            /// </summary>
            /// <param name="administrativeArealType">The administrative level to return.</param>
            /// <returns>The fixture payload.</returns>
            [HttpGet("item")]
            [ProducesResponseType(typeof(EnumSerializableObjectFixture), StatusCodes.Status200OK)]
            public IActionResult Item([FromQuery(Name = "administrativearealtype")] AdministrativeArealType administrativeArealType)
            {
                return Ok(new EnumSerializableObjectFixture(administrativeArealType, null, null, FlagsEnumFixture.None, LongEnumFixture.Small));
            }
        }
    }
}
