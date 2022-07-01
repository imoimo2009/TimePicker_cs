using System.Windows.Forms;
using TimePicker;

namespace TimePicker
{
    public partial class Form1 : Form
    {
        TimePicker Tp;
        public Form1()
        {
            InitializeComponent();
            Tp = new TimePicker(0, 0, 400, 480);
            Tp.AutoNext = true;
            Tp.Visible = false;
            Tp.VisibleChanged += Tp_VisibleChanged;

            this.ClientSize = Tp.Size;
            textBox1.Top = this.ClientSize.Height / 2 - textBox1.Height;
            textBox1.Left = this.ClientSize.Width / 2 - textBox1.Width / 2;
            button1.Top = this.ClientSize.Height / 2 + 16;
            button1.Left = this.ClientSize.Width / 2 - button1.Width / 2;
            this.Controls.Add(Tp);
            Tp.BringToFront();
        }

        private void Tp_VisibleChanged(object sender, System.EventArgs e)
        {
            if (!Tp.Visible)
            {
                textBox1.Text = Tp.Text;
            }
        }

        private void button1_Click(object sender, System.EventArgs e)
        {
            Tp.Visible = true;
        }
    }
}
