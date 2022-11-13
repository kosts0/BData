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
            this.UploadSolutionCodeButton = new System.Windows.Forms.Button();
            this.CodeParseTextBox = new System.Windows.Forms.TextBox();
            this.textBox1 = new System.Windows.Forms.TextBox();
            this.textBox2 = new System.Windows.Forms.TextBox();
            this.button1 = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // GetParsedSolution
            // 
            this.GetParsedSolution.Location = new System.Drawing.Point(45, 329);
            this.GetParsedSolution.Name = "GetParsedSolution";
            this.GetParsedSolution.Size = new System.Drawing.Size(140, 67);
            this.GetParsedSolution.TabIndex = 0;
            this.GetParsedSolution.Text = "Парсинг кода решения";
            this.GetParsedSolution.UseVisualStyleBackColor = true;
            this.GetParsedSolution.Click += new System.EventHandler(this.GetParsedSolution_Click);
            // 
            // UploadUsersToEs
            // 
            this.UploadUsersToEs.Location = new System.Drawing.Point(440, 320);
            this.UploadUsersToEs.Name = "UploadUsersToEs";
            this.UploadUsersToEs.Size = new System.Drawing.Size(140, 108);
            this.UploadUsersToEs.TabIndex = 1;
            this.UploadUsersToEs.Text = "Загрузка колекции пользователей в Es";
            this.UploadUsersToEs.UseVisualStyleBackColor = true;
            this.UploadUsersToEs.Click += new System.EventHandler(this.UploadUsersToEs_Click);
            // 
            // LoadToEsDataButton
            // 
            this.LoadToEsDataButton.Location = new System.Drawing.Point(237, 311);
            this.LoadToEsDataButton.Name = "LoadToEsDataButton";
            this.LoadToEsDataButton.Size = new System.Drawing.Size(159, 106);
            this.LoadToEsDataButton.TabIndex = 2;
            this.LoadToEsDataButton.Text = "Обновление коллекции попыток в ES (основные данные)";
            this.LoadToEsDataButton.UseVisualStyleBackColor = true;
            this.LoadToEsDataButton.Click += new System.EventHandler(this.LoadToEsDataButton_Click);
            // 
            // UploadSolutionCodeButton
            // 
            this.UploadSolutionCodeButton.Location = new System.Drawing.Point(605, 332);
            this.UploadSolutionCodeButton.Name = "UploadSolutionCodeButton";
            this.UploadSolutionCodeButton.Size = new System.Drawing.Size(149, 85);
            this.UploadSolutionCodeButton.TabIndex = 3;
            this.UploadSolutionCodeButton.Text = "Обновить записи с кодом решения в Es";
            this.UploadSolutionCodeButton.UseVisualStyleBackColor = true;
            this.UploadSolutionCodeButton.Click += new System.EventHandler(this.UploadSolutionCodeButton_Click);
            // 
            // CodeParseTextBox
            // 
            this.CodeParseTextBox.Location = new System.Drawing.Point(12, 12);
            this.CodeParseTextBox.Multiline = true;
            this.CodeParseTextBox.Name = "CodeParseTextBox";
            this.CodeParseTextBox.ReadOnly = true;
            this.CodeParseTextBox.Size = new System.Drawing.Size(201, 307);
            this.CodeParseTextBox.TabIndex = 4;
            // 
            // textBox1
            // 
            this.textBox1.Location = new System.Drawing.Point(237, 278);
            this.textBox1.Name = "textBox1";
            this.textBox1.ReadOnly = true;
            this.textBox1.Size = new System.Drawing.Size(159, 27);
            this.textBox1.TabIndex = 5;
            // 
            // textBox2
            // 
            this.textBox2.Location = new System.Drawing.Point(440, 12);
            this.textBox2.Multiline = true;
            this.textBox2.Name = "textBox2";
            this.textBox2.ReadOnly = true;
            this.textBox2.Size = new System.Drawing.Size(125, 148);
            this.textBox2.TabIndex = 6;
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(440, 166);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(125, 48);
            this.button1.TabIndex = 7;
            this.button1.Text = "Загрузка данных с API Cf";
            this.button1.UseVisualStyleBackColor = true;
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.textBox2);
            this.Controls.Add(this.textBox1);
            this.Controls.Add(this.CodeParseTextBox);
            this.Controls.Add(this.UploadSolutionCodeButton);
            this.Controls.Add(this.LoadToEsDataButton);
            this.Controls.Add(this.UploadUsersToEs);
            this.Controls.Add(this.GetParsedSolution);
            this.Name = "Form1";
            this.Text = "Form1";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private Button GetParsedSolution;
        private Button UploadUsersToEs;
        private Button LoadToEsDataButton;
        private Button UploadSolutionCodeButton;
        private TextBox CodeParseTextBox;
        private TextBox textBox1;
        private TextBox textBox2;
        private Button button1;
    }
}