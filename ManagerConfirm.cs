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
    /// <summary>
    /// חלונית אישור מנהל עבור שלב ב'-בשלב א לא נשקל
    /// </summary>
    public partial class ManagerConfirm : Form
    {
        List<string> Managers = new List<string>();
        public bool TheMangerNumExist { get; set; }
        public int Dna { get; set; }
        public string DnaWithLetter { get; set; }
        public bool DnaAndSrialOk { get; set; }
        public int SerialNum { get; set; }
        public string CatalogNum { get; set; }

        ActualTire A = new ActualTire();
        public string ManagerId { get; set; }//מספר מנהל מאשר

        /// <summary>
        /// אישור מנהל שהתווית לא עברה שקיל
        /// </summary>
        public ManagerConfirm(List<string> Manager, string catalognum,string DnaWithLetter,string Specification)
        {
            InitializeComponent();
            this.Managers = Manager;
            A.Specifications = Specification;
            TheMangerNumExist = false;//רק שיזין מנהל שקיים ישתנה לtrue
            CatalogNum = catalognum;
            txtCatalogNum.Text = CatalogNum;
            txtCatalogNum.Enabled = false;
            gpo_ManagerConfirm1.BringToFront();
            DnaAndSrialOk = false;//הדיאנאיי וסריאלי עברו בדיקה
            LogWaveClass.LogWave("נפתח חלון אישור מנהל");
        }


        private void btn_confirm_Click_1(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(txt_id_manager.Text)) return;
            try
            {
                if (!(Managers.Contains(txt_id_manager.Text)))
                {
                    MessageBox.Show("לא קיים מנהל עם מספר עובד זה");
                    return;
                }
                else
                {
                    ManagerId = txt_id_manager.Text;
                    gpo_ManagerConfirm1.Visible = false;
                    int x = Convert.ToInt32((this.Size.Width) * 0.07);
                    int y = Convert.ToInt32((this.Size.Height) * 0.07);
                    gpo_ManagerConfirm2.Location = new Point(x, y);
                    gpo_ManagerConfirm2.Visible = true;
                    txtDNA.Focus();
                }
            }

            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
          
        }


        private void TxtDNA_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)13)
            {
                if (txtDNA.Text == null)
                {
                    MessageBox.Show(
                        "Specification Not Loaded",
                        "Alliance Tire Manufacturing",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error,
                        MessageBoxDefaultButton.Button1,
                        (MessageBoxOptions)0x40000);
                    this.txtDNA.Text = string.Empty;
                    return;
                }

                bool Check = CheckDnaBarCode(txtDNA.Text);//בדיקה שהדיאנאיי הוכנס כמו שצריך
                A.PutDnaField(txtDNA.Text);
                Check = A.CheckIfDnaUsed();//בדיקה אם היה שימוש בDNA
                if (!Check)
                {
                    txtDNA.Text = "";
                    txtDNA.Focus();
                }
                else
                {
                    this.txtSerial.Focus();
                    txtDNA.BackColor = Color.YellowGreen;
                    txtDNA.Enabled = false;
                    Dna = A.Dna;
                    DnaWithLetter = A.DnaWithLetter;

                }

                if (txtSerial.Enabled == false && txtDNA.Enabled == false)//שניהם עברו בדיקה
                {
                    DnaAndSrialOk = true;
                    this.Close();
                }
            }
        }





        private bool CheckDnaBarCode(string txtDNA)
        {
  
            if (txtDNA == "")
            {
                MessageBox.Show($@"הכנס {lblDnaNum.Text}");
                return false;
            }

            try
            {
                if (txtDNA.Length == 6)
                    int.Parse(txtDNA.Substring(0, 5));//חמישה תווים ראשונים חייבים להיות מספר
                else
                {
                    MessageBox.Show("וודא שמספר DNA מכיל 6 ספרות");
                    return false;
                }
                if (!char.IsUpper(txtDNA[txtDNA.Length - 1]) && !char.IsDigit(txtDNA[txtDNA.Length - 1]))//אם התו האחרון לא מספר ולא אות גדולה
                {
                    MessageBox.Show("תו אחרון חייב להכיל מספר או אות גדולה");
                    return false;
                }
            }

            catch (Exception)
            {
                MessageBox.Show("שדה " + lblDnaNum.Text + " יכול להכיל רק מספרים");
                return false;
            }


            return true;
        }

        private void txtSerial_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)13)
            {
                bool Check = CheckSerialNumBarCode(txtSerial.Text);
                if (!Check)
                {
                    txtSerial.Text = "";
                    txtSerial.Focus();
                }
                else
                {
                    A.SerialNumber = int.Parse(txtSerial.Text.Substring(15, 6));
                    A.CatalogNum = txtSerial.Text.Substring(0, 15);
                    Check = A.CheckLevelSerialNumber("שלב א");//בדיקה שלא היה שימוש במספר סריאלי בשלב קרקס
                    if (!Check)
                    {
                        txtSerial.Text = "";
                        txtSerial.Focus();
                    }
                    else
                    {
                        SerialNum = A.SerialNumber;
                        txtSerial.Enabled = false;
                    }
                }

                if (txtSerial.Enabled == false && txtDNA.Enabled == false)//שניהם עברו בדיקה
                {
                    DnaAndSrialOk = true;
                    this.Close();
                }
            }

        }

        private bool CheckSerialNumBarCode(string txtSerial)
        {

            if (txtSerial == "")
            {
                MessageBox.Show($@"הכנס {lblSerialNum.Text}");
                return false;
            }
        
            
            if (txtSerial.Length != 21)
            {
                MessageBox.Show("מספר סידורי לא הוכנס בהתאם");
                return false;
            }
            return true;
        }


        private void gpo_ManagerConfirm1_Enter(object sender, EventArgs e)
        {

        }
    }
}
