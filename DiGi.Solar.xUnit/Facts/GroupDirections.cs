using DiGi.Geometry.Spatial.Classes;
using System;
using System.Collections.Generic;

namespace DiGi.Solar.xUnit
{
    public partial class Facts
    {
        /// <summary>
        /// Tests that the GroupDirections method correctly groups vectors that are within the angle tolerance and separates those that are not.
        /// </summary>
        [Fact]
        public void GroupDirections()
        {
            Dictionary<DateTime, Vector3D> dictionary_Directions = new();
            DateTime dateTime_1 = new(2026, 6, 26, 10, 0, 0);
            DateTime dateTime_2 = new(2026, 6, 26, 11, 0, 0);
            DateTime dateTime_3 = new(2026, 6, 26, 12, 0, 0);

            Vector3D vector3D_1 = new(0.0, 0.0, 1.0);
            Vector3D vector3D_2 = new(0.0, 0.0, 1.0);
            Vector3D vector3D_3 = new(1.0, 0.0, 0.0);

            dictionary_Directions[dateTime_1] = vector3D_1;
            dictionary_Directions[dateTime_2] = vector3D_2;
            dictionary_Directions[dateTime_3] = vector3D_3;

            double angleTolerance = 0.1; // in radians (~5.7 degrees)

            List<Tuple<Vector3D, List<DateTime>>>? groups = Query.GroupDirections(dictionary_Directions, angleTolerance);

            Assert.NotNull(groups);
            Assert.Equal(2, groups.Count);

            // Group 1 should contain dateTime_1 and dateTime_2
            Assert.Contains(dateTime_1, groups[0].Item2);
            Assert.Contains(dateTime_2, groups[0].Item2);
            Assert.Equal(2, groups[0].Item2.Count);

            // Group 2 should contain dateTime_3
            Assert.Contains(dateTime_3, groups[1].Item2);
            Assert.Single(groups[1].Item2);
        }
    }
}
