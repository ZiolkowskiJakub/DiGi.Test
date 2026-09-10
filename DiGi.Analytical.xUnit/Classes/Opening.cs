using DiGi.Analytical.Building.Classes;
using DiGi.Analytical.Building.Interfaces;
using DiGi.Geometry.Spatial.Classes;
using System.Text.Json.Nodes;

namespace DiGi.Analytical.xUnit
{
    /// <summary>
    /// Represents an opening used as test data, carrying a plane geometry. No concrete opening class exists in <see cref="DiGi.Analytical.Building.Classes"/> yet.
    /// </summary>
    public class Opening : BuildingGeometry3DObject<Plane>, IOpening
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Opening"/> class with the specified plane.
        /// </summary>
        /// <param name="plane">The plane geometry of the opening.</param>
        public Opening(Plane? plane)
            : base(plane)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Opening"/> class by copying an existing opening.
        /// </summary>
        /// <param name="opening">The source opening to copy from.</param>
        public Opening(Opening? opening)
            : base(opening)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Opening"/> class from the specified JSON object.
        /// </summary>
        /// <param name="jsonObject">The JSON object to initialize from.</param>
        public Opening(JsonObject? jsonObject)
            : base(jsonObject)
        {
        }
    }
}
