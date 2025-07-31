using DiGi.Analytical.Building.Classes;
using DiGi.Analytical.Building.HVAC;
using DiGi.Analytical.Building.HVAC.Enums;
using DiGi.Core.Classes;

namespace DiGi.Analytical.Test
{
    public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();
        }

        private void Button_Test1_Click(object sender, EventArgs e)
        {
            ProfileTest();
        }

        private void ProfileTest()
        {
            ProfileType profileType = ProfileType.EquipmentLatentGain;

            Profile profile_1 = Create.Profile([0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23], profileType);

            InternalCondition internalCondition_1 = new InternalCondition("Internal Condition 1") { Description = "Internal Condition 1" };
            internalCondition_1.SetProfile(profile_1);

            Profile profile_2 = Create.Profile([100, 101, 102, 103, 104, 105, 106, 107, 108, 109, 110, 111, 112, 113, 114, 115, 116, 117, 118, 119, 120, 121, 122, 123], profileType);

            InternalCondition internalCondition_2 = new InternalCondition("Internal Condition 2") { Description = "Internal Condition 2" };
            internalCondition_2.SetProfile(profile_2);

            Space space = new Space(new Geometry.Spatial.Classes.Point3D(0, 0, 0), "Space");

            BuildingModel buildingModel = new BuildingModel();

            buildingModel.Assign(space, internalCondition_1, new Range<int>(24, 47));
            buildingModel.Assign(space, internalCondition_2, new Range<int>(48, 48 + 23));

            IndexedDoubles indexDoubles = Building.Query.IndexedDoubles(buildingModel, space, new Range<int>(0, 71), Core.Query.Description(profileType));

            buildingModel = new BuildingModel();

            buildingModel.Assign(space, internalCondition_1);

            indexDoubles = Building.Query.IndexedDoubles(buildingModel, space, new Range<int>(0, 71), Core.Query.Description(profileType));
        }
    }
}
