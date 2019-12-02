using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TireManufacturing
{
    public partial class BeforeReleaseVersion : Form
    {
        int Couner = 180;//3 min
        public BeforeReleaseVersion()
        {
            InitializeComponent();
        }

        private void BeforeReleaseVersion_Load(object sender, EventArgs e)
        {
            timer1.Start();
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            Couner--;
            if (Couner == 0)
            {
                timer1.Stop();
                Application.Exit();
            }
            TimeSpan time = TimeSpan.FromSeconds(Couner);
            lblTimer.Text = time.ToString(@"mm\:ss");
            //this.Close();
            //Application.Exit();
        }


        private void BeforeReleaseVersion_FormClosing(object sender, FormClosingEventArgs e)
        {
            if(Couner!=0)
                e.Cancel = true;
        }
    }
}
