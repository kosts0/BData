namespace WinFormDataReciver
{
    partial class Form1
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
            this.GetParsedSolution = new System.Windows.Forms.Button();
            this.UploadUsersToEs = new System.Windows.Forms.Button();
            this.LoadToEsDataButton = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // GetParsedSolution
            // 
            this.GetParsedSolution.Location = new System.Drawing.Point(22, 330);
            this.GetParsedSolution.Name = "GetParsedSolution";
            this.GetParsedSolution.Size = new System.Drawing.Size(140, 67);
            this.GetParsedSolution.TabIndex = 0;
            this.GetParsedSolution.Text = "Парсинг кода решения";
            this.GetParsedSolution.UseVisualStyleBackColor = true;
            this.GetParsedSolution.Click += new System.EventHandler(this.GetParsedSolution_Click);
            // 
            // UploadUsersToEs
            // 
            this.UploadUsersToEs.Location = new System.Drawing.Point(196, 309);
            this.UploadUsersToEs.Name = "UploadUsersToEs";
            this.UploadUsersToEs.Size = new System.Drawing.Size(140, 108);
            this.UploadUsersToEs.TabIndex = 1;
            this.UploadUsersToEs.Text = "Загрузка колекции пользователей в Es";
            this.UploadUsersToEs.UseVisualStyleBackColor = true;
            this.UploadUsersToEs.Click += new System.EventHandler(this.UploadUsersToEs_Click);
            // 
            // LoadToEsDataButton
            // 
            this.LoadToEsDataButton.Location = new System.Drawing.Point(370, 309);
            this.LoadToEsDataButton.Name = "LoadToEsDataButton";
            this.LoadToEsDataButton.Size = new System.Drawing.Size(159, 106);
            this.LoadToEsDataButton.TabIndex = 2;
            this.LoadToEsDataButton.Text = "Загрузка попыток (основные данные) в Es";
            this.LoadToEsDataButton.UseVisualStyleBackColor = true;
            this.LoadToEsDataButton.Click += new System.EventHandler(this.LoadToEsDataButton_Click);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Controls.Add(this.LoadToEsDataButton);
            this.Controls.Add(this.UploadUsersToEs);
            this.Controls.Add(this.GetParsedSolution);
            this.Name = "Form1";
            this.Text = "Form1";
            this.ResumeLayout(false);

        }

        #endregion

        private Button GetParsedSolution;
        private Button UploadUsersToEs;
        private Button LoadToEsDataButton;
    }
}