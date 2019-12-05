using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO.Ports;
using System.Text.RegularExpressions;
using System.Configuration;

namespace TireManufacturing
{
    public partial class Form1 : Form
    {
        static Machine machine = new Machine();//קבלת נתוני מכונה
        static WorkPlan workPlan = new WorkPlan();//קבלת תוכנית עבודה
        static TireSpecifications tireSpecifications = new TireSpecifications();//מחלקת מפרטים
        static FormStopReason stopReason = new FormStopReason(machine.AS400User, machine.AS400Pass);//שולח רשימת סיבות עצירה
        SelfInspection selfInspection;
        ActualTire actualTire= new ActualTire();//אובייקט נתוני גלגל בפועל כולל dna 
         List<int> Managers = new List<int>();//רשימת מנהלים מאשרים
        int PreviousEmployeeSelfInspection;//העובד הקודם שעשה בקרה עצמית
        string PreviousSpecificationSelfInspection;//מפרט קודם עבור בקרה עצמית-בשביל לבדוק אם מהתחלה או לא
        DataTable dataTableWorkPlan = new DataTable();//תוכנית עבודה                     
        string CatalogNumNow = "";
        ReminderBandages reminderBandages;//הנחת תחבושת אחרי שקילה
        BeforeReleaseVersion beforeReleaseVersion = new BeforeReleaseVersion();//התראה לפני סגירת תוכנית לשם עדכון
        bool beforeReleaseVersionExist=false;//יכול להיות שפתוח כבר החלון
        public Form1()
        {
  
            try
            {
                LogWaveClass.LogWave("אפליקציה התחילה");
                InitializeComponent();
                LogWaveClass.LogWave("אפליקציה הועלתה");
                timer_RecSave.Start();
                //txtDNA.Text = "905055";
                //txtSerial.Text = "38022999-100  S205783";
                //List<string> ListWorkPlan = new List<string>();//רשימת סטרינגים עבור תוכנית עבודה
                
                GetWorkPlan();//תוכנית עבודה
                LogWaveClass.LogWave("קיבל תוכנית עבודה");
                ShowStart();//תצוגה התחלתית
                LogWaveClass.LogWave("סיים תצוגה התחלתית");
                ShowManagers();//רשימת מנהלים מאשרים      
                LogWaveClass.LogWave("סיים שליפת רשימת מנהלים מאשרים");
                stopReason.CheckIFStopExist();//בודק אם קיימת עצירה על המכונה
                if (stopReason.ExistStop)//אם כן ישר רושם ועושה כפתור אדום
                {
                    Btn_StopReason.Text = stopReason.DescriptionStop;
                    Btn_StopReason.BackColor = Color.OrangeRed;
                }

            }

            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
      
        }



        private void ShowStart()
        {
            txtDNA.Enabled = false;
            txtSerial.Enabled = false;
            LblMachine.Text = machine.MachineID;
            LblDept.Text = machine.DepratmentId;
            lbl_MachineWrk.Text = $@"מכונה {machine.MachineID}";
            //אפשרויות המכונה
            if (machine.Karkas) Cbo_Level.Items.Add("שלב א");
            if(machine.LevelB) Cbo_Level.Items.Add("שלב ב");
            if (machine.Tire) Cbo_Level.Items.Add("צמיג מלא");

            if (Cbo_Level.Items.Count == 1) Cbo_Level.SelectedIndex = 0;//אם יש רק אופצייה אחת במכונה היא תופיע דיפולטיבית
            //VisibleWeightFields();
        }
        


        /// <summary>
        /// רשימת מנהלים מאשרים
        /// </summary>
        private void ShowManagers()
        {
            DbServiceSQL dbServiceSQL = new DbServiceSQL();
            DataTable dataTable = new DataTable();
            string query =
                $@"  SELECT tbl_Approvers.Manager_Code
                     FROM tbl_Approvers
                     WHERE tbl_Approvers.App_Key='Building_Machine'";
            LogWaveClass.LogWave("קבלת רשימת מנהלים מאשרים " + query);
            dataTable = dbServiceSQL.executeSelectQueryNoParam(query);
            for (int i = 0; i < dataTable.Rows.Count; i++)
            {
                Managers.Add(int.Parse(dataTable.Rows[i]["Manager_Code"].ToString()));
            }
        }


        private void Form1_Load(object sender, EventArgs e)
        {
            this.FormBorderStyle = FormBorderStyle.Fixed3D; 
            this.WindowState = FormWindowState.Maximized;
            this.lbl_CurTime.Text = DateTime.Now.ToString("HH:mm:ss");
        }





        /// <summary>
        /// סידור עבודה
        /// </summary>
        private void Btn_GetWorkPlan_Click(object sender, EventArgs e)
        {
            GetWorkPlan();   
        }

        /// <summary>
        /// קבלת סידור עבודה
        /// </summary>
        private void GetWorkPlan()
        {
            DBService DBS = new DBService();
            dataTableWorkPlan = workPlan.GetWorkingPlane(false);//מתוך אובייקט work plan שנוצר למעלה
            CBox_WorkPlan.SelectedIndex = -1;
        }

        /// <summary>
        /// טעינת מפרט
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Btn_Load_Click(object sender, EventArgs e)
        {
            try
            {
                if (Btn_Load.Text == "טען מפרט")
                {
                    Cursor.Current = Cursors.WaitCursor;
                    if (CBox_WorkPlan.Text == "") throw new Exception("לא בחרת מפרט");
                    if (string.IsNullOrEmpty(Txt_Emp.Text as string)) throw new Exception("לא הכנסת מספר עובד");
                    if (Txt_Emp.Text.Length != 5) throw new Exception("מספר עובד לא תקין");
                    if (Cbo_Level.Text == "") throw new Exception("בחר סוג צמיג עבור המכונה");
                    bool EmployeeExist = CheckEmployeeId(int.Parse(Txt_Emp.Text));//בודק אם מספר עובד קיים
                    if (!EmployeeExist) return;

           

                    LogWaveClass.LogWave("מפרט הוטען");
                    //בדיקה כמה מוכנים               
                    workPlan.ReadyTires(CBox_WorkPlan.SelectedIndex,CBox_WorkPlan.Text);//שולח את מיקום המפרט עליו אנחנו נרצה לעבוד
                    Lbl_PlanedProduced.Text = workPlan.HowManyPlanned + " // " + workPlan.HowManyReady;
                    LogWaveClass.LogWave("סיים לכתוב כמה צמיגים מוכנים מתוך כמה מתוכניים");
                    string str = CBox_WorkPlan.Text;//מקבל את המפרט
                    char delimiter = '-';
                    string[] SubstringsSpecifications = str.Split(delimiter);//פיצול מפרט ל3 חלקים עבור השאילתה

                    actualTire.LevelWork = Cbo_Level.SelectedItem.ToString();
                    actualTire.Specifications = CBox_WorkPlan.Text;//הכנסת מפרט לצמיג נוכחי ישמש לבדיקות התוויות
                    actualTire.Managers = Managers;//הכנסת רשימת מנהלים
                    actualTire.TireNumber = workPlan.HowManyReady + 1;//איזה צמיג כרגע בסדרת ייצור

                    tireSpecifications.GetSpecificationBaseData(SubstringsSpecifications);//מקבל את נתוני המפרט 
                    tireSpecifications.CheckSpecificationAndGetCatalog(str, Cbo_Level.SelectedItem.ToString(),workPlan.Shift);//בודק את תקינות המפרט
                    LogWaveClass.LogWave("סיים קבלת ובדיקת מפרט");

                    ///בדיקה שלא עובר את מקסימום ראש שקילה שניתן להכיל,והכנסת מקט רלוונטי בהתאם לשלב העבודה
                    switch (Cbo_Level.SelectedItem.ToString())
                    {
                        case "שלב א":
                            tireSpecifications.CatalogNumberUsedNow = tireSpecifications.CaracasCatalogNum;
                            if (tireSpecifications.CarcasWeight / 2.2046 > machine.MaxWeightingCap)
                            {
                                MessageBox.Show("משקל קרקס גדול מקיבולת ראש המשקל", "הודעת שגיאה",MessageBoxButtons.OK, MessageBoxIcon.Exclamation,MessageBoxDefaultButton.Button1);
                                return;
                            }
                            break;

                        case "שלב ב":
                            tireSpecifications.CatalogNumberUsedNow = tireSpecifications.GreenCatalogNum;
                            if (tireSpecifications.WeightLevelB/2.2046 > machine.MaxWeightingCap)
                            {
                                MessageBox.Show("משקל קרקס גדול מקיבולת ראש המשקל", "הודעת שגיאה", MessageBoxButtons.OK, MessageBoxIcon.Exclamation, MessageBoxDefaultButton.Button1);
                                return;
                            }
                            break;

                        case "צמיג מלא":
                            tireSpecifications.CatalogNumberUsedNow = tireSpecifications.GreenCatalogNum;
                            if (tireSpecifications.TireWeight / 2.2046 > machine.MaxWeightingCap)
                            {
                                MessageBox.Show("משקל קרקס גדול מקיבולת ראש המשקל", "הודעת שגיאה", MessageBoxButtons.OK, MessageBoxIcon.Exclamation, MessageBoxDefaultButton.Button1);
                                return;
                            }
                            break;

                    }

                    lbl_WrappingId.Text = tireSpecifications.WrappingId;
                    lbl_Size.Text = tireSpecifications.Size;
                    lbl_Weight1.Text = tireSpecifications.CarcasWeight.ToString();
                    lbl_GreenWeight.Text = tireSpecifications.TireWeight.ToString();
                    lbl_Mixture.Text = tireSpecifications.Mixture;
                    lbl_PR.Text = tireSpecifications.Pr.ToString();
                    lbl_Weight2.Text = tireSpecifications.WeightLevelB.ToString();
                    LogWaveClass.LogWave("הכנסת ערכי מפרט למסך");
                    actualTire.CatalogNum = tireSpecifications.CatalogNumberUsedNow;//הכנסת מק"ט רלוונטי למחלקת צמיג בפועל
                    workPlan.CheckIfTheSameEmployee(CBox_WorkPlan.Text,actualTire.CatalogNum);//בודק אם יש צורך לרשומה חדשה בכרטיס עבודה בתוכנית עבודה אם מספר עובד השתנה
                    LogWaveClass.LogWave("סיים workPlan.CheckIfTheSameEmployee");
                    LogWaveClass.LogWave("בא לשים תמונת עיגול ירוק כסטטוס פעיל");
                    LblStatus.Image = new Bitmap(@"\\nas84bk\pub_dep\Application\C#\TireManufacturing\green.jpg");//סימן ירוק בסטטוס

                    //נעילת שדות
                    CBox_WorkPlan.Enabled = false;
                    Cbo_Level.Enabled = false;
                    Txt_Emp.Enabled = false;
                    txtDNA.Enabled = true;
                    txtSerial.Enabled = true;
                    btn_SelfInspection.Enabled = true;
                    LogWaveClass.LogWave("לפני יצירת אובייקט בקרה עצמית");
                    selfInspection = new SelfInspection(tireSpecifications.CatalogNumberUsedNow, Cbo_Level.Text, CBox_WorkPlan.Text, Managers, int.Parse(Txt_Emp.Text), workPlan.Shift, PreviousEmployeeSelfInspection, PreviousSpecificationSelfInspection, machine.AS400User, machine.AS400Pass);//שולח לטופס בקרה גם את השלב בו אנו עובדים                                                                                                                                                                                                                                                         //בדיקת צבע סטטוס בקרה עצמית
                    switch ((int)(selfInspection.colorSelfInspection))
                    {
                        case 0:
                            btn_SelfInspection.BackColor = Color.Crimson;
                            break;

                        case 1:
                            btn_SelfInspection.BackColor = Color.Yellow;
                            break;

                        case 2:
                            btn_SelfInspection.BackColor = Color.GreenYellow;
                            break;
                    }
                    LogWaveClass.LogWave("אחרי יצירת אובייקט בקרה עצמית");

                    Btn_StopReason.Enabled = true;//כעת יהיה אפשר לדווח עצירה במידה ויש
                    stopReason.ShowReasons(actualTire.EmployeeId, actualTire.CatalogNum, workPlan.Shift, actualTire.WorkCenter);//כעת יהיה אפשר לדווח עצירה במידה ויש
                    Btn_GetWorkPlan.Enabled = false;//אי אפשר לרענן סידור עבודה


                    txtDNA.Focus();
                    Cursor.Current = Cursors.Default;
                }

                if (Btn_Load.Text == "הפסק")
                {
                    Btn_Load.Text = "טען מפרט";
                    CBox_WorkPlan.Enabled = true;
                    Cbo_Level.Enabled = true;
                    Txt_Emp.Enabled = true;
                    txtDNA.Enabled = true;
                    txtSerial.Enabled = true;
                    txtDNA.Enabled = false;
                    txtSerial.Enabled = false;
                    btn_SelfInspection.Enabled = false;
                    ClearScreen();
                    Btn_GetWorkPlan.Enabled = true;// אפשר לרענן סידור עבודה
                    srlWeighingHead.Close();
                }
                else
                {
                    Btn_Load.Text = "הפסק";
                    srlWeighingHead.PortName = machine.ComPort1;
                    srlWeighingHead.Open();
                }

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                LogWaveClass.ErrorLogWave("Error: " + ex.Message);
            }

        }


        ///בודק אם נעשה כבר טופס בקרה עצמית על אותו מפרט
        private void CheckIfWasSelfInspection()
        {
            string qry = $@"SELECT *
                          FROM tbl_Spec_Components
                          WHERE Spec_No='{actualTire.Specifications}' and Emp_No={actualTire.EmployeeId}";
            DbServiceSQL dbServiceSQL = new DbServiceSQL();
            DataTable dataTable = new DataTable();
            dataTable= dbServiceSQL.executeSelectQueryNoParam(qry);

        }

        /// <summary>
        /// ניקוי מסך אחרי הפסקת טעינה
        /// </summary>
        private void ClearScreen()
        {
            Action<Control.ControlCollection> func = null;
            func = (controls) =>
            {
                foreach (Control control in controls)
                {
                    if (control.AccessibleName != null)
                    {
                        if (control.AccessibleName == "LabelsToDeleteAfter")
                            control.Text = "";
                    }
                    else
                        func(control.Controls);
                }
            };
            txtDNA.BackColor = Color.White;
            txtSerial.BackColor = Color.White;
            LblStatus.Image = new Bitmap(@"\\nas84bk\pub_dep\Application\C#\TireManufacturing\glossy-red-icon-angle-md.png");//סימן אדום בסטטוס



            func(Controls);
            tireSpecifications = new TireSpecifications();

        }

        /// <summary>
        /// איפוס שקילה וניקוי שדות רלוונטיים
        /// </summary>
        private void NewRound()
        {
            try
            {
                Action<Control.ControlCollection> func = null;
                func = (controls) =>
                {
                    foreach (Label control in controls)
                    {
                        if (control.AccessibleName != null)
                        {
                            if (control.AccessibleDescription == "Weight")//מוחק את כל הלייבלי שקילה
                            {
                                control.Enabled = true;
                                control.Text = "";
                            }
                        }
                        else
                            func(control.Controls);
                    }
                };
                foreach (Control lbl in flowLayoutPanel2.Controls)
                {
                    if (lbl.AccessibleDescription == "Weight")
                    {
                        lbl.Text = String.Empty;
                        lbl.BackColor = SystemColors.ControlLightLight;
                    }
                }

                txtDNA.BackColor = Color.White;
                txtDNA.Text = "";
                txtDNA.Enabled = true;
                txtSerial.BackColor = Color.White;
                txtSerial.Text = "";
                txtSerial.Enabled = true;
                this.BackColor = SystemColors.GradientInactiveCaption;

                LogWaveClass.LogWave("איפוס סבב חדש יצירת אובייקט חדש");
                CatalogNumNow = actualTire.CatalogNum;
                actualTire = new ActualTire(actualTire.EmployeeId, CBox_WorkPlan.Text, Managers, actualTire.CatalogNum, actualTire.LevelWork);
                //actualTire.Specifications = CBox_WorkPlan.Text;//הכנסת מפרט לצמיג נוכחי ישמש לבדיקות התוויות
                //actualTire.Managers = Managers;//הכנסת רשימת מנהלים
                //actualTire.CatalogNum = CatalogNumNow;
                workPlan.ConfigureShift();
                dataTableWorkPlan = workPlan.GetWorkingPlane(true);
                workPlan.ReadyTires(CBox_WorkPlan.SelectedIndex,CBox_WorkPlan.Text);//בודק איזה צמיג בסדרה עכשיו. אמור להיות פלוס 1 מהסבב הקודם
                actualTire.TireNumber = workPlan.HowManyReady + 1;//איזה צמיג כרגע בסדרת ייצור
                Lbl_PlanedProduced.Text = workPlan.HowManyPlanned + " // " + workPlan.HowManyReady;

                //bool EmployeeExist = CheckEmployeeId(int.Parse(Txt_Emp.Text));//בודק אם מספר עובד קיים
                txtDNA.Focus();
            }

            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                LogWaveClass.ErrorLogWave("Error: " + ex.Message);
            }

        }

        /// <summary>
        /// בודק אם מספר עובד קיים
        /// </summary>
        /// <param name="EmployeeId"></param>
        private bool CheckEmployeeId(int EmployeeId)
        {
            DBService DBS = new DBService();
            DataTable dataTable = new DataTable();
            string query =
           $@" SELECT right(trim(T.OVED),5) as Number,L.PRATI as FirstName, L.FAMILY as LastName
            FROM isufkv.isav as T left join isufkv.isavl10 as L on T.OVED = L.OVED
            WHERE right(trim(T.OVED),5)={EmployeeId} ";
            dataTable = DBS.executeSelectQueryNoParam(query);
            if (dataTable.Rows.Count==0) return false;
            else
            {
                LblEmpName.Text = dataTable.Rows[0]["FirstName"].ToString() + " " + dataTable.Rows[0]["LastName"].ToString();
                if (actualTire.EmployeeId != EmployeeId)//אם הוחלף מספר עובד או עכשיו הוכנס לראשונה
                {
                    actualTire.EmployeeId = EmployeeId;
                    workPlan.EmployeeId = EmployeeId;
                    btn_SelfInspection.BackColor = Color.Crimson;//צריך לעשות טופס בקרה מחדש
                }
                return true;

            }


        }

        private void Btn_OpenPDF_Click(object sender, EventArgs e)
        {
            OpenPDF();
        }

        private void CBox_WorkPlan_TextChanged(object sender, EventArgs e)
        {
            Btn_Load.Enabled = true;
        }

        private void OpenPDF()
        {
            string fName = CBox_WorkPlan.Text;
            var filePath = @"\\nas84bk\pub_dep\Build layout pdf\";
            var filePathA = @"\\nas84bk\pdfarchive\";

            var tempFolder = @"C:\TmpSpec";

            var lfp = tempFolder + "\\" + fName + ".pdf";
            var sfp = filePath + fName;
            if (!Directory.Exists(tempFolder)) Directory.CreateDirectory(tempFolder);

            if (File.Exists(filePath + "BL " + fName + ".pdf")) sfp = filePath + "BL " + fName + ".pdf";
            else if (File.Exists(filePathA + "mifrat_ " + fName.Replace('-', ' ') + ".pdf"))
                sfp = filePathA + "mifrat_ " + fName.Replace('-', ' ') + ".pdf";

            if (File.Exists(sfp))
                try
                {
                    foreach (var clsProcess in Process.GetProcesses())
                        if (clsProcess.ProcessName.StartsWith("AcroRd32")) clsProcess.Kill();

                    Thread.Sleep(1000);

                    foreach (var file in new DirectoryInfo(tempFolder).GetFiles()) file.Delete();

                    // העתקת קובץ לתיקיה מקומית
                    File.Copy(sfp, lfp);

                    // פתיחת קובץ
                    Process.Start(lfp);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error : During Open File. " + ex.Message, "Weighing Application");
                }
            else MessageBox.Show("File Not Exist.", "Weighing Application");
        }


        /// <summary>
        /// קורא ראש משקל
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void srlWeighingHead_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            LogWaveClass.LogWave("נכנס לפונקציית שקילה");
            var sp = (SerialPort)sender;

            // sp.Encoding = Encoding.GetEncoding("utf-8");
            var indata = sp.ReadExisting();
            LogWaveClass.LogWave("קריאת שקילה" +indata);
            var regex = new Regex(@"-?[0-9]*\.?[0-9]+");

            if (regex.IsMatch(indata))
            {
                LogWaveClass.LogWave("לפני השמת שדה משקל");
                var weight = 0.0;
                double.TryParse(regex.Match(indata).Value, out weight);
                weight = Math.Abs(weight);
                LogWaveClass.LogWave("אחרי השמת שדה משקל");

                if (weight > 0)
                {
                    //if ((weight < Convert.ToDouble(Lbl_BandSpecWeight.Text.Trim()) * 1.15) && (weight > Convert.ToDouble(Lbl_BandSpecWeight.Text.Trim()) * 0.85))
                    LogWaveClass.LogWave("לפני קריאה לPROCESS SCALE");
                    this.Process_Scale(weight);                   
                }
            }

        }

        /// <summary>
        /// שקילה ומדידת סטיות
        /// </summary>
        /// <param name="ActualWeight"></param>
        private void Process_Scale(double ActualWeight)
        {
            if (txtDNA.Enabled == false && txtSerial.Enabled == false && btn_SelfInspection.BackColor!=Color.Crimson)// רק כאשר דיאינאיי ומספר סידורי הוכנסו כמו שצריך(נחסמו מעריכה)ונעשתה בקרה עצמית ניתן לעשות פעולת שקילה
            {
                //שתי השורות הבאות מתייחסות לאם הבנאי לחץ פעם נוספת על כפתור השקילה
                if(!actualTire.BlockWeightPrint)//רק אם לא חסום האופציה ללחוץ על כפתור שקילה
                    actualTire.timeUntilSaveWeight = 60;//מאפס שוב את המונה שניות על מנת שהשקילה האחרונה תהיה הרלוונטית

                LogWaveClass.LogWave("התחילה פעולת שקילה");
                //ActualWeight = ActualWeight / 2.2046;//המרה מליברות לק"ג
                stopReason.CheckIFStopExist();//בודק אם קיימת עצירה על המכונה
                if (stopReason.ExistStop)//אם כן עוצר אותה עם קוד תקלה 95
                {
                    stopReason.UpdateStopTables("95");
                    Btn_StopReason.Text = "דיווח סיבת עצירה";
                    Btn_StopReason.BackColor = Color.Gainsboro;
                    LogWaveClass.LogWave("סיום סיבת עצירה כתוצאה משקילה ");
                }

                //ActualWeight = 810;
                //להבין באיזה שלב אנחנו בהתאם לסטייה
                LogWaveClass.LogWave("לפני זיהוי באיזה מצב אנחנו בשקילה");
                string WhichCase = "";
                if ((ActualWeight < Convert.ToDouble(lbl_Weight1.Text.Trim()) * 1.15) && (ActualWeight > Convert.ToDouble(lbl_Weight1.Text.Trim()) * 0.85))//אם לא חורג ממש מהמשקל של המפרט אז המשקל רלוונטי לקרקס
                    WhichCase = "שלב א";
                else if ((ActualWeight < Convert.ToDouble(lbl_GreenWeight.Text.Trim()) * 1.15) && (ActualWeight > Convert.ToDouble(lbl_GreenWeight.Text.Trim()) * 0.85))//אם לא חורג ממש מהמשקל של המפרט אז המשקל רלוונטי
                    WhichCase = "צמיג מלא";
                else if (ActualWeight >= 5)// רק מעל 5 ליברות ההודעה רלוונטית
                {
                    MessageBox.Show("שגיאה: המשקל חורג ממשקל המפרט", "הודעת שגיאה", MessageBoxButtons.OK, MessageBoxIcon.Exclamation, MessageBoxDefaultButton.Button1);
                    return;
                }
                    
                actualTire.WhichCase = WhichCase;
                double Deviation, WeightNorm;//משתנה סטייה ומשקל לפי התקן           
                Deviation = 0;

                LogWaveClass.LogWave("אחרי זיהוי באיזה מצב אנחנו בשקילה");
                string Colorr="";
                switch (WhichCase)//איזה שלב עבודה נבחר מאופציות המכונה
                {
                    case "שלב א":
                        actualTire.WeightKarkas = ActualWeight;//שומר את משקל קרקס בפועל במחלקת גלגל בפועל
                        lbl_WeightA.Text = ActualWeight.ToString();//הצגת משקל בפועל                                    
                        WeightNorm = double.Parse(lbl_Weight1.Text);//משקל תקן
                        Deviation = (ActualWeight / WeightNorm - 1) * 100;
                        actualTire.DeviationKarkas = Deviation;//שומר סטייה במחלקה
                        lbl_DeviationA.Text = Deviation.ToString("N2");
                        LogWaveClass.LogWave("אחרי שקילת שלב א-הכנסה ללייבלים");
                        Colorr = CheckDeviation(Deviation);
                        ColorControl(lbl_WeightA);
                        ColorControl(lbl_DeviationA);
                        break;


                    case "צמיג מלא":
                        if(Cbo_Level.Text=="שלב ב")
                        {
                            //מקרה קצה-אם לא חייב לשקול קרקס לפני יקח מתוך נתונים קיימים כבר
                            if (!machine.MustCaracasWeight)
                            {
                                if (actualTire.WeightKarkasFromPast != 0)//מכניס את המשקל מהעבר לשדה משקל קרקס
                                    actualTire.WeightKarkas = actualTire.WeightKarkasFromPast;
                                else //אם לא קיים כזה מכניס משקל מפרט
                                    actualTire.WeightKarkas = tireSpecifications.CarcasWeight;
                                lbl_WeightA.Text = actualTire.WeightKarkas.ToString("N2");//הצגת משקל בפועל                                    
                                WeightNorm = double.Parse(lbl_Weight1.Text);//משקל תקן
                                Deviation = (actualTire.WeightKarkas / WeightNorm - 1) * 100;
                                actualTire.DeviationKarkas = Deviation;//שומר סטייה במחלקה
                                lbl_DeviationA.Text = Deviation.ToString("N2");
                                LogWaveClass.LogWave("מקרה קצה אם לא חייב לשקול קרקס בשלב ב");
                                Colorr = CheckDeviation(Deviation);
                                ColorControl(lbl_WeightA);
                                ColorControl(lbl_DeviationA);

                            }

                                if (actualTire.WeightKarkas==0)//לחסום שקילת צמיג מלא אם לא נשקל לפני משקל קרקס
                            {
                                MessageBox.Show("יש לשקול משקל קרקס", "הודעת שגיאה", MessageBoxButtons.OK, MessageBoxIcon.Exclamation, MessageBoxDefaultButton.Button1);
                                return;
                            }
                        }
                        actualTire.WeightGreenTire = ActualWeight;//שומר את משקל בפועל של צמיג ירוק במחלקת גלגל בפועל
                        lbl_WeightFull.Text = ActualWeight.ToString();
                        WeightNorm = double.Parse(lbl_GreenWeight.Text);//משקל תקן
                        Deviation = (ActualWeight / WeightNorm - 1) * 100;
                        actualTire.DeviationGreenTire = Deviation;
                        lbl_DeviationFull.Text = Deviation.ToString("N2");
                        LogWaveClass.LogWave("אחרי שקילת צמיג שלם-הכנסה ללייבלים ולפני בדיקת שליחת הודעה");
                        actualTire.CheckIFSmsDeviation(ActualWeight, Deviation, "צמיג מלא");
                        LogWaveClass.LogWave("אחרי בדיקת שליחת הודעה");
                        Colorr = CheckDeviation(Deviation);
                        ColorControl(lbl_WeightFull);
                        ColorControl(lbl_DeviationFull);
                        break;


                }


                //פונקציית צביעת רקע או לייבל
                void ColorControl(Control control)
                {
                    if (Colorr == "red")
                    {
                        control.BackColor = Color.Red;
                    }
                    else if (Colorr == "yellow")
                    {
                        control.BackColor = Color.Yellow;
                    }
                    else
                    {
                        control.BackColor = Color.YellowGreen;
                    }

                }

                if (WhichCase != "")
                {
                    LogWaveClass.LogWave("לפני צביעת רקע שקילה");
                    //בדיקת סטיה וצבע רקע
                    ColorControl(this);

 

                    if (lbl_WeightA.Text != "" && lbl_WeightFull.Text != "")// ואנחנו בשלב ב'-רק שמופיעים נתוני שקילה של קרקס וצמיג שלם
                    {
                        actualTire.WeightLevelB = actualTire.WeightGreenTire - actualTire.WeightKarkas;//משקל שלב ב- צמיג פחות קרקס
                        lbl_WeightB.Text = actualTire.WeightLevelB.ToString("N2");
                        actualTire.DeviationLevelB = actualTire.DeviationGreenTire - actualTire.DeviationKarkas;
                        lbl_DeviationB.Text = actualTire.DeviationLevelB.ToString("N2");
                        Colorr= CheckDeviation(actualTire.DeviationLevelB);
                        ColorControl(lbl_WeightB);
                        ColorControl(lbl_DeviationB);
                    }
                    LogWaveClass.LogWave("אחרי צביעת רקע שקילה");

                    //actualTire.CheckAlert(WhichCase, Deviation, ActualWeight);//חדש מה7.4 בדיקת התראות למנהלים

                    LogWaveClass.LogWave("לפני שחרור שדה שמירת שקילה");

                    bool ScaleCarcasBeforeInLevelB = false;//שקילת כרכס בשלב ב' לפני בניית צמיג מלא-במקרה זה לא יהיה שידור שקילה
                    if (actualTire.LevelWork == "שלב ב" && WhichCase == "שלב א" && actualTire.DnaMangerConfirm == 0)
                        ScaleCarcasBeforeInLevelB = true;

                    reminderBandages = new ReminderBandages();//תזכורת הנחת תחבושת
                    if (!ScaleCarcasBeforeInLevelB )
                    {
                        actualTire.WeighingSave = true;//משחרר את האופציה לשמור שקילה
                        Rectangle workingArea = Screen.GetWorkingArea(this);
                        reminderBandages.Location = new Point((Screen.PrimaryScreen.WorkingArea.Width - reminderBandages.Width) / 2,
                              (Screen.PrimaryScreen.WorkingArea.Height - reminderBandages.Height) / 2 + 200);
                        reminderBandages.ShowDialog();
                        txtDNA.Focus();
                    }
                    LogWaveClass.LogWave("אחרי שחרור שדה שמירת שקילה");
                }

         
            }
            else
            {
                if (txtDNA.Enabled != false)
                {
                    MessageBox.Show("לא הכנסת DNA", "הודעת שגיאה", MessageBoxButtons.OK, MessageBoxIcon.Exclamation, MessageBoxDefaultButton.Button1);
                    txtDNA.Focus();
                }
                else if (txtSerial.Enabled != false)
                {
                    MessageBox.Show("לא הכנסת מספר סריאלי", "הודעת שגיאה", MessageBoxButtons.OK, MessageBoxIcon.Exclamation, MessageBoxDefaultButton.Button1);    
                    txtSerial.Focus();
                }
                else if (btn_SelfInspection.BackColor == Color.Crimson)
                    MessageBox.Show("לא סיימת לבצע בקרה עצמית", "הודעת שגיאה", MessageBoxButtons.OK, MessageBoxIcon.Exclamation, MessageBoxDefaultButton.Button1);
            }
        }

        




        /// <summary>
        /// בדיקות דינאנאיי לפני המשך התהליך
        /// </summary>
        private bool CheckStickerDnaBeoforeContinue()
        {
            txtDNA.CharacterCasing = CharacterCasing.Upper;
            bool ContinueNext = CheckDnaBarCode(txtDNA.Text);//בודק שהברקודים הוכנסו בהתאם*/
            if (!ContinueNext)
            {
                txtDNA.Focus();
                return false;
            }
            ContinueNext=actualTire.PutDnaField(txtDNA.Text);//השמה לשדה dna-בודק גם אותיות במידה ודיאנאיי שלילי
            if (!ContinueNext)
            {
                txtDNA.Focus();
                return false;
            }
            LogWaveClass.LogWave("עבר בדיקות טכניות של הכנסת דיאנאיי");
            if (Cbo_Level.Text == "שלב ב")
            {
                LogWaveClass.LogWave("לפני בדיקות דיאנאיי של שלב ב");
                ContinueNext = actualTire.CheckIfLevelBDnaUsed(tireSpecifications.CaracasCatalogNum);//בדיקה של שלב ב'
                LogWaveClass.LogWave("לפני בדיקת אישור מנהל אחרי בדיקות דיאנאיי של שלב ב");

                if (!string.IsNullOrEmpty(actualTire.DnaMangerConfirm.ToString() as string) && !string.IsNullOrEmpty(actualTire.CatalogNumMangerConfirm.ToString() as string))//תנאי שבודק אם היה צורך באישור מנהל
                {
                    txtDNA.Text = actualTire.DnaMangerConfirmWithLetter;//מכניס את מספר הdna שהמנהל יכניס אליו את נתוני שקילת קרקס
                    txtSerial.Text = actualTire.CatalogNumMangerConfirm.ToString()+actualTire.SerialNumMangerConfirm.ToString();
                    actualTire.SerialNumber = actualTire.SerialNumMangerConfirm;//הכנסת מספר סריאלי לפי מה שהכניס המנהל ואושר לו
                    actualTire.CatalogNum = actualTire.CatalogNumMangerConfirm;
                    actualTire.Dna = actualTire.Dna;
                    actualTire.DnaWithLetter = actualTire.DnaMangerConfirmWithLetter;
                    txtSerial.Enabled = false;//סיימנו גם את בדיקת לייבל סידורי כי המנהל הכניס כבר
                    txtSerial.BackColor = Color.YellowGreen;
                    LogWaveClass.LogWave("אחרי בדיקת אישור מנהל ");
                }
            }
            else //שלב א /צמיג שלם
            {
                LogWaveClass.LogWave("לפני בדיקות דיאנאיי של צמיג מלא/שלב א");
                ContinueNext = actualTire.CheckIfDnaUsed();//בודק אם היה שימוש בעבר לדיאנאיי
                LogWaveClass.LogWave("אחרי בדיקות דיאנאיי של צמיג מלא/שלב א");
            }
            if (!ContinueNext)
            {
                return false;
            }
     
            return true;//עבר את הבדיקות בהצלחה
        }




        /// <summary>
        /// בדיקות מספר סריאלי לפני המשך תהליך
        /// </summary>
        /// <returns></returns>
        private bool CheckStickerSerialNumBeoforeContinue()
        {
            txtSerial.CharacterCasing = CharacterCasing.Upper;//מחליף הכל לאותיות גדולות
            bool ContinueNext = CheckSerialNumBarCode(txtSerial.Text);
            if (!ContinueNext)
            {
                txtSerial.Focus();
                return false;
            }
            LogWaveClass.LogWave("סיים בדיקות טכניות בהכנסת מספר סריאלי");
            actualTire.SerialNumber = int.Parse(txtSerial.Text.Substring(15, 6));//הכנסת מספר סריאלי
            if (txtSerial.Text.Substring(0, 15).Trim() != actualTire.CatalogNum)//אם מספר קטלוגי שהוכנס שונה מזה שמוחזק אצלי מתוך המפרט
            {
                MessageBox.Show($@"מספר קטלוגי לא תואם את הנוכחי", "הודעת שגיאה", MessageBoxButtons.OK, MessageBoxIcon.Exclamation, MessageBoxDefaultButton.Button1);
                return false;
            }
            LogWaveClass.LogWave("לפני בדיקת מספר סריאלי-אם היה בשימוש");
            if (tireSpecifications.CaracasCatalogNum != null)//במידה והוא לא טען מפרט עדיין זה לא יכנס לבדיקה הבאה
            {
                ContinueNext = actualTire.CheckLevelSerialNumber(Cbo_Level.SelectedItem.ToString());//שולח את שלב העבודה הרלוונטי,בודק אם היה שימוש לתוויות בעבר
                LogWaveClass.LogWave("אחרי בדיקת מספר סריאלי-אם היה בשימוש");
                if (!ContinueNext)
                {                  
                    return false;
                }
            }

            return true;//עבר את הבדיקות בהצלחה

        }


        /// <summary>
        /// בדיקות שהdna הוכנס כמו שצריך
        /// </summary>
        private bool CheckDnaBarCode(string txtDNA)
        {
            LogWaveClass.LogWave("בדיקות שדיאנאיי הוכנס כמו שצריך " + txtDNA);
            if (txtDNA == "" )
            {
                MessageBox.Show($@"הכנס {lblDnaNum.Text}", "הודעת שגיאה", MessageBoxButtons.OK, MessageBoxIcon.Exclamation, MessageBoxDefaultButton.Button1);
                return false;
            }
      
            try
            {
                if (txtDNA.Length > 7)
                {
                    MessageBox.Show($@"הכנס {lblDnaNum.Text}", "הודעת שגיאה", MessageBoxButtons.OK, MessageBoxIcon.Exclamation, MessageBoxDefaultButton.Button1);
                    return false;
                }
                if (txtDNA.Length == 6)
                    int.Parse(txtDNA.Substring(0,5));//חמישה תווים ראשונים חייבים להיות מספר
                else
                {
                    MessageBox.Show("וודא שמספר DNA מכיל 6 ספרות", "הודעת שגיאה", MessageBoxButtons.OK, MessageBoxIcon.Exclamation, MessageBoxDefaultButton.Button1);
                    return false;
                }
                if (!char.IsUpper(txtDNA[txtDNA.Length - 1]) && !char.IsDigit(txtDNA[txtDNA.Length - 1]))//אם התו האחרון לא מספר ולא אות גדולה
                {
                    MessageBox.Show("תו אחרון חייב להכיל מספר או אות גדולה", "הודעת שגיאה", MessageBoxButtons.OK, MessageBoxIcon.Exclamation, MessageBoxDefaultButton.Button1);
                    return false;
                } 
            }
          
            catch (Exception)
            {
                MessageBox.Show("שדה " + lblDnaNum.Text + " יכול להכיל רק מספרים", "הודעת שגיאה", MessageBoxButtons.OK, MessageBoxIcon.Exclamation, MessageBoxDefaultButton.Button1);
                return false;
            }
               
        
            return true;
        }


        /// <summary>
        /// בדיקה שסידורי הוכנס כמו שצריך
        /// </summary>
        private bool CheckSerialNumBarCode(string txtSerial)
        {

            LogWaveClass.LogWave("בדיקות שסריאלי הוכנס כמו שצריך- "+txtSerial);

            if (txtSerial == "")
                {
                        MessageBox.Show($@"הכנס {lblSerialNum.Text}", "הודעת שגיאה", MessageBoxButtons.OK, MessageBoxIcon.Exclamation, MessageBoxDefaultButton.Button1);
                        return false;
                }

            if (txtSerial.Length != 21)
            {
                MessageBox.Show("מקט לא הוכנס בהתאם", "הודעת שגיאה", MessageBoxButtons.OK, MessageBoxIcon.Exclamation, MessageBoxDefaultButton.Button1);
                return false;
            }
            if (tireSpecifications.CatalogNumberUsedNow != null)//במידה והוא לא טען מפרט עדיין זה לא יכנס לבדיקה הבאה
            {
                //בדיקה שהמקט תואם את זה שהוכנס
                //להחזיר
                if (tireSpecifications.CatalogNumberUsedNow != txtSerial.Substring(0, 15))
                {
                    //MessageBox.Show("מקט לא תואם את המפרט");
                    //return false;
                }
            }

            return true;
        }

        /// <summary>
        ///בודק סטייה ומחזיר צבע רקע בהתאם
        /// </summary>
        private string CheckDeviation(double Deviation) 
        {
           
            string color=null;
            if ((Deviation <= 1.5 && Deviation >= 0)|| (Deviation <= 0 && Deviation >= -1.5)) color = "green";
            else if ((Deviation <= 3 && Deviation > 1.5) || (Deviation <= -1.5 && Deviation > -3)) color = "yellow";
            else color = "red";
            return color;
        }

        private void ResetWeight()
        {
        //    Lbl_ActualWeight.Text = "";
        //    Lbl_Dev.Text = "";
            this.BackColor = Color.LightSkyBlue;

           
        }

        private void txtDNA_KeyPress(object sender, KeyPressEventArgs e)
        {
            try
            {
                if (e.KeyChar == (char)13)
                {
                    if (CBox_WorkPlan.Text == null)
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
                    LogWaveClass.LogWave("הוכנס דיאנאיי למסך ראשי");
                    stopReason.CheckIFStopExist();//בודק אם קיימת עצירה על המכונה
                    if (stopReason.ExistStop)//אם כן עוצר אותה עם קוד תקלה 95
                    {
                        stopReason.UpdateStopTables("95");
                        Btn_StopReason.Text = "דיווח סיבת עצירה";
                        Btn_StopReason.BackColor = Color.Gainsboro;
                        LogWaveClass.LogWave("הופסקה תקלה אם הייתה דרך הכנסת DNA");
                    }


                    bool Check = CheckStickerDnaBeoforeContinue();//בדיקה שהדיאנאיי הוכנס כמו שצריך
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

                    }
                    LogWaveClass.LogWave("סיים הכנסת דיאנאיי במסך ראשי בהצלחה");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                LogWaveClass.ErrorLogWave("Error: " + ex.Message);
            }

        }

        private void txtSerial_KeyPress(object sender, KeyPressEventArgs e)
        {
            try
            {
                if (e.KeyChar == (char)13)
                {
                    if (CBox_WorkPlan.Text == null)
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
                    LogWaveClass.LogWave("הוכנס מספר סריאלי למסך ראשי");
                    stopReason.CheckIFStopExist();//בודק אם קיימת עצירה על המכונה
                    if (stopReason.ExistStop)//אם כן עוצר אותה עם קוד תקלה 95
                    {
                        stopReason.UpdateStopTables("95");
                        Btn_StopReason.Text = "דיווח סיבת עצירה";
                        Btn_StopReason.BackColor = Color.Gainsboro;
                        LogWaveClass.LogWave("הופסקה תקלה אם הייתה דרך הכנסת סריאלי");
                    }

                    bool Check = CheckStickerSerialNumBeoforeContinue();//בדיקה שמספר סריאלי הוכנס כמו שצריך
                    if (!Check)
                    {
                        txtSerial.Text = "";
                        txtSerial.Focus();
                    }
                    else
                    {
                        txtSerial.Enabled = false;
                        txtSerial.BackColor = Color.YellowGreen;
                        lbl_WrappingId.Focus();
                    }
                    LogWaveClass.LogWave("סיים הכנסת סריאלי במסך ראשי בהצלחה");

                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                LogWaveClass.ErrorLogWave("Error: " + ex.Message);
            }

        }

        /// <summary>
        /// כפתור שקילה
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button2_Click(object sender, EventArgs e)
        {
            Process_Scale(51);
        }
        private void button1_Click(object sender, EventArgs e)
        {
            Process_Scale(144.0);

        }
        private void Btn_StopReason_Click(object sender, EventArgs e)
        {
            try
            {
                LogWaveClass.LogWave("לחץ על סיבת עצירה");
                LogWaveClass.LogWave("הציג את חלון סיבת עצירה");
                stopReason.ShowDialog();
                if (!stopReason.CloseWindowWithOkButton) return;//אם לחץ על כפתור סגירה לא יקרה כלום
                if (stopReason.DescriptionStop != "" && stopReason.CodeStop != "95")
                {
                    Btn_StopReason.Text = stopReason.DescriptionStop +" "+ DateTime.Now.ToLocalTime() ;
                    Btn_StopReason.BackColor = Color.OrangeRed;
                }
                if (stopReason.CodeStop == "95")//הפסיקה התקלה
                {
                    Btn_StopReason.Text = "דיווח סיבת עצירה";
                    Btn_StopReason.BackColor = Color.Gainsboro;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                LogWaveClass.ErrorLogWave("Error: " + ex.Message);
            }
        }

        private void lbl_WrappingId_TextChanged(object sender, EventArgs e)
        {

        }

        /// <summary>
        /// מרכיבים וטופס בקרה עצמית 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btn_SelfInspection_Click(object sender, EventArgs e)
        {
            LogWaveClass.LogWave("משתמש לחץ על כפתור בקרה עצמית");
            selfInspection = new SelfInspection(tireSpecifications.CatalogNumberUsedNow, Cbo_Level.Text, CBox_WorkPlan.Text, Managers, int.Parse(Txt_Emp.Text), workPlan.Shift, PreviousEmployeeSelfInspection, PreviousSpecificationSelfInspection, machine.AS400User, machine.AS400Pass);//שולח לטופס בקרה גם את השלב בו אנו עובדים
            selfInspection.ShowDialog();
            PreviousEmployeeSelfInspection = ((SelfInspection)selfInspection).PreviousEmployee;//איזה עובד עשה עכשיו את הבקרה עצמית
            PreviousSpecificationSelfInspection = ((SelfInspection)selfInspection).PreviousSpecification;//על איזה מפרט היה עכשיו בקרה עצמית
            //בדיקת צבע סטטוס בקרה עצמית
            switch ((int)(selfInspection.colorSelfInspection))
            {
                case 0:
                    btn_SelfInspection.BackColor = Color.Crimson;
                    break;

                case 1:
                    btn_SelfInspection.BackColor = Color.Yellow;
                    break;

                case 2:
                    btn_SelfInspection.BackColor = Color.GreenYellow;
                    break;
            }
        }


        /// <summary>
        /// תצוגת לייבלים של שקילה בהתאם למצב עבודה
        /// </summary>
        private void Cbo_Machine_TextChanged(object sender, EventArgs e)
        {
            VisibleWeightFields();
        }


        /// <summary>
        /// תצוגת לייבלים של שקילה בהתאם למצב עבודה
        /// </summary>
        private void VisibleWeightFields()
        {
            switch (Cbo_Level.SelectedItem.ToString())
            {
                case "שלב א":
                    lA.Visible = true;
                    lbl_WeightA.Visible = true;
                    lbl_DeviationA.Visible = true;
                    lB.Visible = false;
                    lbl_WeightB.Visible = false;
                    lbl_DeviationB.Visible = false;
                    lFull.Visible = false;
                    lbl_WeightFull.Visible = false;
                    lbl_DeviationFull.Visible = false;

                    break;

                case "שלב ב":
                    lA.Visible = true;
                    lbl_WeightA.Visible = true;
                    lbl_DeviationA.Visible = true;
                    lB.Visible = true;
                    lbl_WeightB.Visible = true;
                    lbl_DeviationB.Visible = true;
                    lFull.Visible = true;
                    lbl_WeightFull.Visible = true;
                    lbl_DeviationFull.Visible = true;

                    break;

                case "צמיג מלא":
                    lA.Visible = false;
                    lbl_WeightA.Visible = false;
                    lbl_DeviationA.Visible = false;
                    lB.Visible = false;
                    lbl_WeightB.Visible = false;
                    lbl_DeviationB.Visible = false;
                    lFull.Visible = true;
                    lbl_WeightFull.Visible = true;
                    lbl_DeviationFull.Visible = true;

                    break;

            }
        }

        private void timer_RecSave_Tick(object sender, EventArgs e)
        {
            this.lbl_CurTime.Text = DateTime.Now.ToString("HH:mm:ss");
            try
            {
                if (actualTire.WeighingSave)//אם השדה שמירת שקילה שוחרר ישדר שקילה אחרי דקה
                {
                    actualTire.timeUntilSaveWeight--;
                    if (actualTire.timeUntilSaveWeight == 0)
                    {
                        timer_RecSave.Enabled = false;
                        timerConfigureShift.Enabled = false;//טיימר להחלפת משמרות
                        LogWaveClass.LogWave("לפני שידור שקילה");
                        LogWaveClass.LogWave("לפני שמירת משקל לאס400");
                        actualTire.SaveWeightToAS400(tireSpecifications, workPlan.Shift);//שמירת משקל לאס400
                        LogWaveClass.LogWave("לפני שמירת מרכיבי צמיג");
                        actualTire.SaveTireComponents(selfInspection);//שמירת מרכיבי צמיג
                        LogWaveClass.LogWave("לפני העלאת קאונטר ב1");
                        if (actualTire.DnaMangerConfirm == 0)
                        {
                            workPlan.AddOneTire();//העלאת קאונטר של צמיג ב1
                            LogWaveClass.LogWave("לפני סבב חדש ואיפוס הקודמים");
                            NewRound();//סבב חדש של שקילות ואיפוס הקודמים                          
                        }
                        reminderBandages.Close();//סגירת תזכורת הנחת תחבושת
                        actualTire.DnaMangerConfirm = 0;
                        timer_RecSave.Enabled = true;
                        timerConfigureShift.Enabled = true;

                    }
                }

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                LogWaveClass.ErrorLogWave("Error: " + ex.Message);
            }

        }

        /// <summary>
        /// למחיקת קבצי לוג שלפני חודש
        /// </summary>
        private void timerDeleteLog_Tick(object sender, EventArgs e)
        {
            
            if(DateTime.Now.Hour==17)
            {
                LogWaveClass.DeleteLog();
            }

            
        }


        private void בדיקתשקילהToolStripMenuItem_Click_1(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start(@"\\nas84bk\pub_dep\Application\C#\Weighing Indicator\WeighingIndicator.exe");
        }

        private void דוחשקילהToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                WeightReport weightReport = new WeightReport();
                weightReport.ShowDialog();
                StreamWriter SW;
                string LogName = "דוח שקילות עבור " + (weightReport).EmployeeNum;

                if (!File.Exists(@"C:\\c#\" + LogName + ".txt"))
                {
                    SW = File.CreateText(@"C:\\c#\" + LogName + ".txt");
                    SW.Close();

                }
                else
                {
                    File.Delete(@"C:\\c#\" + LogName + ".txt");
                }
                SW = File.AppendText(@"C:\\c#\" + LogName + ".txt");
                string str = "מוצר";
                SW.Write(str.PadRight(40, ' '));
                str = "סריאלי";
                SW.Write(str.PadRight(40, ' '));
                str = "מזהה";
                SW.Write(str.PadRight(40, ' '));
                str = "תאריך";
                SW.Write(str.PadRight(40, ' '));
                str = "שעת ייצור";
                SW.Write(str.PadRight(40, ' '));
                str = "ממפרט";
                SW.Write(str.PadRight(40, ' '));
                str = "משקל בפועל";
                SW.Write(str.PadRight(40, ' '));
                str = "משקל מפרט";
                SW.Write(str.PadRight(40, ' '));
                str = "סטייה";
                SW.Write(str.PadRight(40, ' '));
                SW.WriteLine("");
                SW.WriteLine("=======================================================================================================================================================================================           ");


                DataTable dataTable = new DataTable();
                dataTable = actualTire.MakeReportForEmployee((weightReport).EmployeeNum, (weightReport).dateTime);
                foreach (DataRow row in dataTable.Rows)
                {
                    SW.WriteLine($@"{String.Format("{0:P2}.", double.Parse(row["Deviation"].ToString())).PadRight(15,' ')}{row["WeightSpec"].ToString().PadRight(15, ' ')}{row["WeightActual"].ToString().PadRight(10, ' ')}{row["Specification"].ToString().PadRight(25, ' ')}{row["Time"].ToString().PadRight(15, ' ')}{row["Date"].ToString().PadRight(20, ' ')}{row["Dna"].ToString().PadRight(15, ' ')}{row["SerialNum"].ToString().PadRight(15, ' ')}{row["CatalogNum"].ToString().PadRight(15, ' ')} "); //double.Parse(row["Deviation"].ToString()).ToString("##.##%")
                }
                Process.Start(@"C:\\c#\" + LogName + ".txt");
                SW.Close();
            }
            catch
            {
                MessageBox.Show("שגיאה בהפקת דוח שקילות", "הודעת שגיאה", MessageBoxButtons.OK, MessageBoxIcon.Exclamation, MessageBoxDefaultButton.Button1);
            }

        }

        private void החלףעובדToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Txt_Emp.Enabled = true;
            Txt_Emp.Focus();
        }

        private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            AboutBox1 aboutBox1 = new AboutBox1();
            aboutBox1.Show();

        }

        private void Txt_Emp_Leave(object sender, EventArgs e)
        {
            try
            {
                LogWaveClass.LogWave("עובד רוצה להחליף מספר עובד");
                if (Txt_Emp.Text == "") return;
                if (Txt_Emp.Text.Length != 5) throw new Exception("מספר עובד לא תקין");
                int.TryParse(Txt_Emp.Text, out int EmpId);//
                bool ExistEmp = CheckEmployeeId(EmpId);
                if (ExistEmp && (Btn_Load.Enabled==false||Btn_Load.Text=="הפסק"))
                {               
                   workPlan.CheckIfTheSameEmployee(CBox_WorkPlan.Text, actualTire.CatalogNum);//בודק אם יש צורך לרשומה חדשה בכרטיס עבודה בתוכנית עבודה אם מספר עובד השתנה                  
                }
                if(ExistEmp)
                    Txt_Emp.Enabled = false;
                else
                {
                    MessageBox.Show("לא קיים מספר עובד זה", "הודעת שגיאה", MessageBoxButtons.OK, MessageBoxIcon.Exclamation, MessageBoxDefaultButton.Button1);
                    Txt_Emp.Text = "";
                    Txt_Emp.Enabled = true;
                    Txt_Emp.Focus();
                }

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                LogWaveClass.ErrorLogWave("Error: " + ex.Message);
            }
        }

        private void Cbo_Level_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void Cbo_Level_SelectedValueChanged(object sender, EventArgs e)
        {
            DataView DVdataTableWorkPlan = new DataView(dataTableWorkPlan);
            if (Cbo_Level.Text == "שלב א")
                DVdataTableWorkPlan.RowFilter = "Class='L'"; // query example = "id = 10"
            else
                DVdataTableWorkPlan.RowFilter = "Class<>'L'";
            CBox_WorkPlan.DataSource = DVdataTableWorkPlan;
            CBox_WorkPlan.DisplayMember = "IDRAW";//יציג רק מפרטים
        }


        private void Txt_Emp_TextChanged(object sender, EventArgs e)
        {
            if (Txt_Emp.Text.Length >= 5)
            {
                Txt_Cart.Focus();
                Txt_Emp.Enabled = false;
            }

        }

        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            LogWaveClass.LogWave("תוכנית נסגרה");
            Application.Exit();
        }


        /// <summary>
        /// כשחוצים משמרת משיג תוכנית עבודה מחדש 7.11.2019
        /// </summary>
        private void timerConfigureShift_Tick(object sender, EventArgs e)
        {

            if(DateTime.Now.ToShortTimeString() == "06:30" || DateTime.Now.ToShortTimeString() == "15:00"|| DateTime.Now.ToShortTimeString() == "23:30")
            {
                workPlan.GetWorkingPlane(false);
                workPlan.ReadyTires(CBox_WorkPlan.SelectedIndex, CBox_WorkPlan.Text);//בודק איזה צמיג בסדרה עכשיו. אמור להיות פלוס 1 מהסבב הקודם
                actualTire.TireNumber = workPlan.HowManyReady + 1;//איזה צמיג כרגע בסדרת ייצור
            }

            //בדיקה אם רוצים לעדכן גרסה-אם כן יסגור תוך דקות את התוכנית
            if (!beforeReleaseVersionExist)//רק אם לא פתוח חלון התראת סגירת תוכנית
            {
                DbServiceSQL DBnewVesion = new DbServiceSQL();
                DataTable dataTable = new DataTable();
                var Name = typeof(Form1).Namespace;
                string qry = $@"SELECT Change
                            FROM AppsForUpdate
                            WHERE Apps='{Name.ToString()}'";
                dataTable = DBnewVesion.executeSelectQueryNoParam(qry);
                if (dataTable.Rows[0][0].ToString() == "True")
                {
                    beforeReleaseVersionExist = true;
                    beforeReleaseVersion.Show();
                    beforeReleaseVersion.BringToFront();
                }
            }
   

        }

    }
}
