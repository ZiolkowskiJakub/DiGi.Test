using DiGi.Analytical.Building.Classes;
using DiGi.Analytical.Building.HVAC;

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

            BuildingModel buildingModel = new BuildingModel();

            Profile profile = new Profile() { Name = "Profile 1", Description = "Test profile" };

            InternalCondition internalCondition_1 = new InternalCondition("Internal Condition 1") { Description = "Test Internal Condition" };
            InternalCondition internalCondition_2 = new InternalCondition("Internal Condition 2") { Description = "Test Internal Condition" };

            buildingModel.Assign(internalCondition_1, profile, Building.HVAC.Enums.ProfileType.Infiltration);
            buildingModel.Assign(internalCondition_2, profile, Building.HVAC.Enums.ProfileType.Infiltration);

            List<Profile> profiles = buildingModel.GetProfiles<Profile>(internalCondition_1);

            List<InternalCondition> internalConditions = buildingModel.GetInternalConditions<InternalCondition>(profile);

            Profile profile_1 = buildingModel.Profile<Profile>(internalCondition_1, Building.HVAC.Enums.ProfileType.Infiltration);

        }
    }
}
