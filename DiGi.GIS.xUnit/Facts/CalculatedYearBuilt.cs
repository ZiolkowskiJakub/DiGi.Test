using System;

namespace DiGi.GIS.xUnit
{
    public partial class Facts
    {
        /// <summary>
        /// Verifies that the calculated year built prefers the user-entered year over the prediction.
        /// </summary>
        [Fact]
        public void CalculatedYearBuilt_UserFirst()
        {
            Classes.YearBuiltData yearBuiltData_User = new("b_ref_001");
            yearBuiltData_User.SetUserYearBuilt(1975);

            Classes.YearBuiltData yearBuiltData_Predicted_1 = new("b_ref_001");
            yearBuiltData_Predicted_1.SetPredictedYearBuilt(new DateTime(2025, 1, 1), 2001);

            Classes.YearBuiltData yearBuiltData_Predicted_2 = new("b_ref_001");
            yearBuiltData_Predicted_2.SetPredictedYearBuilt(new DateTime(2025, 6, 1), 2001);

            Assert.Equal((short)1975, Query.CalculatedYearBuilt([yearBuiltData_User, yearBuiltData_Predicted_1, yearBuiltData_Predicted_2]));
        }

        /// <summary>
        /// Verifies that bounds-only user entries fall through to the prediction, so the calculated year is the predicted one.
        /// </summary>
        [Fact]
        public void CalculatedYearBuilt_FallsThroughBounds()
        {
            Classes.YearBuiltData yearBuiltData_Bound = new("b_ref_001");
            yearBuiltData_Bound.SetUserYearBuilt(new Classes.UserYearBuilt((short)1975, Enums.YearBuiltRelation.AtOrBefore));

            Classes.YearBuiltData yearBuiltData_Predicted_1 = new("b_ref_001");
            yearBuiltData_Predicted_1.SetPredictedYearBuilt(new DateTime(2025, 1, 1), 2001);

            Classes.YearBuiltData yearBuiltData_Predicted_2 = new("b_ref_001");
            yearBuiltData_Predicted_2.SetPredictedYearBuilt(new DateTime(2025, 6, 1), 2001);

            Assert.Equal((short)2001, Query.CalculatedYearBuilt([yearBuiltData_Bound, yearBuiltData_Predicted_1, yearBuiltData_Predicted_2]));
        }

        /// <summary>
        /// Verifies that null, empty and entry-less input answers null.
        /// </summary>
        [Fact]
        public void CalculatedYearBuilt_Empty()
        {
            Assert.Null(Query.CalculatedYearBuilt(null));
            Assert.Null(Query.CalculatedYearBuilt([]));

            Classes.YearBuiltData yearBuiltData = new("b_ref_001");
            Assert.Null(Query.CalculatedYearBuilt([yearBuiltData]));
        }
    }
}
