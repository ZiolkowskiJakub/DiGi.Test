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
            string path = @"C:\Users\Public\Documents\Tas Data\Databases\InternalConditions.tic";

            using (TIC.Classes.Document document = new TIC.Classes.Document(path))
            {
                global::TIC.Document document_TIC = document.Value;

                document.Save(@"C:\Users\jakub\Downloads\Test.tic");
            }


        }
    }
}
