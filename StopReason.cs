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
    public partial class FormStopReason : Form
    {
        StopReasonClass stopReasonClass { get; set; }//מחלקת סיבות עצירה
        public bool ExistStop { get; set; }//קוד תקלה קיים כבר?
        public string CodeStop { get; set; }
        public string CodeForStopp { get; set; }//דיווח עבור טבלת STOPP איזה קוד תקלה היה לפני שהסתיים
        public string DescriptionStop { get; set; }//תיאור תקלה
        DBService DBS = new DBService();
        public bool CloseWindowWithOkButton { get; set; }//סגר חלון אך ורק לפי כפתור אישור
        public FormStopReason(string AS400User, string AS400Pass)
        {
            LogWaveClass.LogWave("פתח אובייקט סיבת עצירה");
            InitializeComponent();
            stopReasonClass = new StopReasonClass();
            CloseWindowWithOkButton = false;
            LogWaveClass.LogWave("סיים לפתוח אובייקט סיבת עצירה");

        }


        public void ShowReasons(int EmployeeID,string CatalogNum,int Shift,string WorkCenter)
        {
            ListBox_StopReason.DataSource = stopReasonClass.StopReasons;
            stopReasonClass.EmployeeId = EmployeeID;
            stopReasonClass.CatalogNum = CatalogNum;
            stopReasonClass.Shift = Shift;
            if(WorkCenter!=null)
            stopReasonClass.WorkCenter = WorkCenter;
            //ListBox_StopReason.Items.Add(StopReasons);
        }

        /// <summary>
        /// אישר קוד תקלה
        /// </summary>
        private void btn_Stop_Click(object sender, EventArgs e)
        {
            LogWaveClass.LogWave("אישר סיבת עצירה");
            //stopReasonClass.TimeStartStop = DateTime.Now.ToString("HHmm");
            string StopReasonText = ListBox_StopReason.GetItemText(ListBox_StopReason.SelectedItem);
            stopReasonClass.StopCodeIndex.TryGetValue(ListBox_StopReason.SelectedIndex, out string CodeStop) ;//מקבל קוד תקלה מתוך הרשומה הנבחרת 
            stopReasonClass.DescriptionStop.TryGetValue(ListBox_StopReason.SelectedIndex, out string DescriptionStop);//מוציא את התיאור של התקלה-עבור הלייבל של הכפתור
            this.DescriptionStop = DescriptionStop.Trim();
            UpdateStopTables(CodeStop);
            LogWaveClass.LogWave("אישר סיבת עצירה");
            CloseWindowWithOkButton = true;//סגר את החלון 
            this.Close();
        }


        /// <summary>
        /// מדווח לטבלאות עצירה בהתאם לקוד תקלה שהוזן
        /// </summary>
        public void UpdateStopTables(string CodeStop)
        {
            this.CodeStop = CodeStop;
            string Numerator = "";
            string qry = "";
            CheckIFStopExist();//אם כבר מדווח עצירת תקלה אז לא צריך לדווח שוב 

            //עדכון לקובץ STT
            //קובץ שמחזיק רשומה אחת עבור כל מכונה ומעדכנים את המצב שלה-בעצירה או בהמשך
            qry = $@"UPDATE RZPADALI.HMIBMACSTT set STIMESTAMP='{DateTime.Now.ToString("yyyy-MM-dd-HH.mm.ss.000000")}',SEMP='{stopReasonClass.EmployeeId}',STAK='{CodeStop}',SUSER='{System.Environment.MachineName}' 
                   WHERE SDEPT={stopReasonClass.DepratmentId} and SMACHINE='{stopReasonClass.MachineID}'";
            LogWaveClass.LogWave(qry);
            DBS.executeInsertQuery(qry);

            if (CodeStop != "95")//אם באמת הוכנסה תקלה שהיא לא קוד תקלה 95 של סיום תקלה
            {
                    //הכנסה לקובץ לוג עצירות-קובץ תיעוד של כל היסטוריית העצירות
                    qry = $@"INSERT into RZPADALI.HMIBMACLOG values('{DateTime.Now.ToString("yyyy-MM-dd-HH.mm.ss.000000")}',{stopReasonClass.DepratmentId},'{stopReasonClass.MachineID}','{stopReasonClass.MachineID}','4','{stopReasonClass.EmployeeId}','{CodeStop}','{stopReasonClass.CatalogNum}',0,'{stopReasonClass.MachineID}')";
                    LogWaveClass.LogWave("מכניס לקובץ תקלות לוג " + qry);

                    DBS.executeInsertQuery(qry);
                    stopReasonClass.TimeStartStop = DateTime.Now.ToString("HHmm");//זמן עדכון תקלה
            }

            //לחץ על סיום תקלה ואז מעדכנים סיום תקלה בקובץ STOPP
            else
            {
                //CheckIFStopExist();//אם כבר מדווח עצירת תקלה אז לא צריך לדווח שוב 
                if (ExistStop)//רק אם יש באמת קוד תקלה אז נדווח עצירה בקובץ STOPP
                {
                    qry = $@"SELECT STVLNM
                             FROM RZPADALI.STVLP";
                    DataTable dataTable = new DataTable();
                    dataTable = DBS.executeSelectQueryNoParam(qry);
                    if (dataTable.Rows.Count != 0)
                        Numerator = (double.Parse(dataTable.Rows[0][0].ToString())+1).ToString();
                    qry = $@"UPDATE RZPADALI.STVLP set STVLNM={Numerator}";
                    DBS.executeInsertQuery(qry);
                    //דיווח סיום תקלה בקובץ STOPP 
                    //if (stopReasonClass.TimeStartStop == null)//אם לא שמור זמן תחילת תקלה אצלנו נפנה לדטה בייס-יכול להיות שהתקלה נפתחה מאפליקציה אחרת
                    //{
                    //    //qry = $@"select  SUBSTR(CHAR(STIMESTAMP), 12, 2) Concat ':' Concat SUBSTR(CHAR(STIMESTAMP), 15, 2)  as TIME
                    //    //        from RZPADALI.HMIBMACSTT 
                    //    //        where SMACHINE ='{stopReasonClass.MachineID}' and SDEPT='{stopReasonClass.DepratmentId}'";
                    //    //dataTable = new DataTable();
                    //    //dataTable = DBS.executeSelectQueryNoParam(qry);
                    //    //string d = dataTable.Rows[0][0].ToString();
                    //    //stopReasonClass.TimeStartStop = (DateTime.Parse(d)).ToString("HHmm");
                    //}
                    if (stopReasonClass.WorkCenter == "0")//אם לחץ סיבת עצירה אבל לא עשה שקילה לא יהיה מרכז עבודה ולכן נשלוף באופן יזוםכ
                    {
                        qry = $@"select RWRKC as WorkCenter
                         FROM bpcsfv30.frtl01 
                         Where RPROD = '{stopReasonClass.CatalogNum}' ";
                        DataTable data = new DataTable();
                        data = DBS.executeSelectQueryNoParam(qry);
                        if (data.Rows.Count == 1)
                        {
                            stopReasonClass.WorkCenter = data.Rows[0]["WorkCenter"].ToString();
                        }
                    }
                    qry = $@"INSERT into RZPADALI.STOPP values({DateTime.Now.ToString("1yyMMdd")},{stopReasonClass.DepratmentId},'000{stopReasonClass.EmployeeId}',{stopReasonClass.DepratmentId},'{stopReasonClass.MachineID}','TAK-{stopReasonClass.DepratmentId}{int.Parse(CodeForStopp).ToString("00")}','{stopReasonClass.Shift}',{stopReasonClass.TimeStartStop},{DateTime.Now.ToString("HHmm")},0,'{stopReasonClass.CatalogNum}',0,
                            {stopReasonClass.WorkCenter},0,0,'','','','',0,0,'{DateTime.Now.ToString("HHmmssddmmyy")}','C#','{System.Environment.MachineName}',{Numerator})";
                    LogWaveClass.LogWave("מכניס לטבלת עצירות " + qry);
                    DBS.executeInsertQuery(qry);
                    stopReasonClass.TimeStartStop = null;
                }
            }

        }

        /// <summary>
        /// בודק אם קיים כבר קוד תקלה פתוח
        /// </summary>
        public void CheckIFStopExist()
        {
            string qry = $@"SELECT *
                          FROM RZPADALI.HMIBMACSTT 
                          WHERE SDEPT={stopReasonClass.DepratmentId} and SMACHINE='{stopReasonClass.MachineID}' and STAK<>95";//95 -סיום העצירה
            LogWaveClass.LogWave("בדיקה אם קיימת סיבת עצירה "+qry);
            DataTable dataTable = new DataTable();
            dataTable = DBS.executeSelectQueryNoParam(qry);
            if (dataTable.Rows.Count == 0)//המכונה הנוכחית עובדת כרגיל ולא הוזנה לה סיבת עצירה
            {
                ExistStop = false;
            }
            else
            {
                ExistStop = true;
                var myKey = stopReasonClass.StopCodeIndex.FirstOrDefault(x => x.Value == dataTable.Rows[0]["STAK"].ToString().Trim()).Key;
                CodeForStopp = stopReasonClass.StopCodeIndex[myKey];//קוד תקלה עבור הSTOPP
                stopReasonClass.DescriptionStop.TryGetValue(myKey, out string DescriptionStopI);//אם קיים קוד תקלה כבר 
                DescriptionStop = DescriptionStopI.Trim();
                //אם לא שמור זמן תחילת תקלה אצלנו נפנה לדטה בייס-יכול להיות שהתקלה נפתחה מאפליקציה אחרת או התוכנית נסגרה
                qry = $@"select  SUBSTR(CHAR(STIMESTAMP), 12, 2) Concat ':' Concat SUBSTR(CHAR(STIMESTAMP), 15, 2)  as TIME
                                from RZPADALI.HMIBMACSTT 
                                where SMACHINE ='{stopReasonClass.MachineID}' and SDEPT='{stopReasonClass.DepratmentId}'";
                LogWaveClass.LogWave(qry);
                dataTable = new DataTable();
                dataTable = DBS.executeSelectQueryNoParam(qry);
                string d = dataTable.Rows[0][0].ToString();
                stopReasonClass.TimeStartStop = (DateTime.Parse(d)).ToString("HHmm");
            }
                          
        }

        private void FormStopReason_FormClosing(object sender, FormClosingEventArgs e)
        {
         
        }
    }
}
