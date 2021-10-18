
namespace JMCAudioPlayerServer
{
    partial class PipeServerForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
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
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.ListBoxUsers = new System.Windows.Forms.ListBox();
            this.RichTextBoxConsole = new System.Windows.Forms.RichTextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.TextBoxPipeName = new System.Windows.Forms.TextBox();
            this.ButtonStart = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // ListBoxUsers
            // 
            this.ListBoxUsers.FormattingEnabled = true;
            this.ListBoxUsers.Location = new System.Drawing.Point(8, 32);
            this.ListBoxUsers.Name = "ListBoxUsers";
            this.ListBoxUsers.Size = new System.Drawing.Size(464, 173);
            this.ListBoxUsers.TabIndex = 0;
            // 
            // RichTextBoxConsole
            // 
            this.RichTextBoxConsole.Location = new System.Drawing.Point(8, 216);
            this.RichTextBoxConsole.Name = "RichTextBoxConsole";
            this.RichTextBoxConsole.ReadOnly = true;
            this.RichTextBoxConsole.Size = new System.Drawing.Size(464, 192);
            this.RichTextBoxConsole.TabIndex = 1;
            this.RichTextBoxConsole.Text = "";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(8, 12);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(93, 13);
            this.label1.TabIndex = 2;
            this.label1.Text = "Pipe Server Name";
            // 
            // TextBoxPipeName
            // 
            this.TextBoxPipeName.Location = new System.Drawing.Point(104, 8);
            this.TextBoxPipeName.Name = "TextBoxPipeName";
            this.TextBoxPipeName.ReadOnly = true;
            this.TextBoxPipeName.Size = new System.Drawing.Size(122, 20);
            this.TextBoxPipeName.TabIndex = 3;
            // 
            // ButtonStart
            // 
            this.ButtonStart.Location = new System.Drawing.Point(232, 6);
            this.ButtonStart.Name = "ButtonStart";
            this.ButtonStart.Size = new System.Drawing.Size(75, 23);
            this.ButtonStart.TabIndex = 4;
            this.ButtonStart.Text = "Start Server";
            this.ButtonStart.UseVisualStyleBackColor = true;
            this.ButtonStart.Click += new System.EventHandler(this.ButtonStart_Click);
            // 
            // PipeServerForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(485, 417);
            this.Controls.Add(this.ButtonStart);
            this.Controls.Add(this.TextBoxPipeName);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.RichTextBoxConsole);
            this.Controls.Add(this.ListBoxUsers);
            this.Name = "PipeServerForm";
            this.Text = "Form1";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ListBox ListBoxUsers;
        private System.Windows.Forms.RichTextBox RichTextBoxConsole;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox TextBoxPipeName;
        private System.Windows.Forms.Button ButtonStart;
    }
}

