namespace DiGi.Analytical.Test
{
    partial class MainForm
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            Button_Test1 = new Button();
            SuspendLayout();
            // 
            // Button_Test1
            // 
            Button_Test1.Location = new Point(12, 12);
            Button_Test1.Name = "Button_Test1";
            Button_Test1.Size = new Size(94, 29);
            Button_Test1.TabIndex = 0;
            Button_Test1.Text = "Test 1";
            Button_Test1.UseVisualStyleBackColor = true;
            Button_Test1.Click += Button_Test1_Click;
            // 
            // MainForm
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(800, 450);
            Controls.Add(Button_Test1);
            Name = "MainForm";
            Text = "Analytical Test";
            ResumeLayout(false);
        }

        #endregion

        private Button Button_Test1;
    }
}
