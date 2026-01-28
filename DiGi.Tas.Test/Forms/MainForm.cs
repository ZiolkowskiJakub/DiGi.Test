using DiGi.Analytical.Building.Classes;
using DiGi.Tas.TIC;

namespace DiGi.Tas.Test
{
    public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();
        }

        private void Button_Test1_Click(object sender, EventArgs e)
        {
            ConvertTest();
        }

        private void DuplicateTest()
        {
            string path_1 = @"C:\Users\Public\Documents\Tas Data\Databases\InternalConditions.tic";
            string path_2 = @"C:\Users\jakub\Downloads\Test.tic";

            File.Copy(path_1, path_2, true);

            using (TIC.Classes.Document document_1 = new TIC.Classes.Document(path_1))
            {
                global::TIC.Document document_TIC_1 = document_1.Value;

                using (TIC.Classes.Document document_2 = new TIC.Classes.Document(path_2))
                {
                    global::TIC.Document document_TIC_2 = document_2.Value;
                }
            }
        }

        private void ConvertTest()
        {
            string path_TIC = @"C:\Users\Public\Documents\Tas Data\Databases\InternalConditions.tic";

            List<InternalCondition>? internalConditions = null;
            using (TIC.Classes.Document document_1 = new(path_TIC))
            {
                internalConditions = document_1.InternalConditions();
            }

            if (internalConditions is null)
            {
                return;
            }

            string path_ICF = @"C:\Users\jakub\Downloads\InternalConditions.icf";

            if (File.Exists(path_ICF))
            {
                File.Delete(path_ICF);
            }

            using (InternalConditionsFile internalConditionFile = new(path_ICF))
            {
                internalConditionFile.Open();

                internalConditionFile.Values = internalConditions;
                internalConditionFile.Save();
            }
        }
    }
}