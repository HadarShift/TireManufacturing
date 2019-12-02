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
    public partial class WeightReport : Form
    {
        public int EmployeeNum { get; set; }
        public DateTime dateTime { get; set;}
        public WeightReport()
        {
            InitializeComponent();
            EmployeeNum = 0;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            int.TryParse(txt_employee.Text, out int EmployeeNum);
            this.EmployeeNum=EmployeeNum;
            this.dateTime = dateTimePicker1.Value;
            this.Close();
        }
    }
}
