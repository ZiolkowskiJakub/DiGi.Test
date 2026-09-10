using DiGi.Analytical.Building.Classes;
using DiGi.Geometry.Spatial.Classes;
using System.Text.Json.Nodes;

namespace DiGi.Analytical.xUnit
{
    /// <summary>
    /// Represents an air component used as test data, carrying a point geometry that yields no <see cref="PolygonalFace3D"/> and therefore no shell face.
    /// </summary>
    public class PointAir : Air<Point3D>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PointAir"/> class with the specified point geometry.
        /// </summary>
        /// <param name="point3D">The point geometry of the air component.</param>
        public PointAir(Point3D? point3D)
            : base(point3D)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PointAir"/> class from the specified JSON object.
        /// </summary>
        /// <param name="jsonObject">The JSON object to initialize from.</param>
        public PointAir(JsonObject? jsonObject)
            : base(jsonObject)
        {
        }

        /// <summary>
        /// Gets the axis-aligned bounding box of the component. A point air carries none.
        /// </summary>
        /// <returns>Always null.</returns>
        public override BoundingBox3D? GetBoundingBox()
        {
            return null;
        }
    }
}
