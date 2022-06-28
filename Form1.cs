using System.Windows.Forms;
using TimePicker;

namespace TimePicker
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            TimePicker tp = new TimePicker(0, 0, 400, 480);
            this.ClientSize = tp.Size;
            this.Controls.Add(tp);
        }
    }
}
