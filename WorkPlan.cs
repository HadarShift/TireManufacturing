using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TireManufacturing
{
    /// <summary>
    /// סידור עבודה-יורש ממחלקה כללית
    /// </summary>
    class WorkPlan :General 
    {
        public int Shift { get; set; }
        public int PreviousShift { get; set; }//משמרת קודמת שנדע אם לקרוא שוב תוכנית עבודה אם השתנתה המשמרת
        public string Date { get; set; }
        //public string MachineId { get; set; }
        //public string DepartmentId { get; set; }
        public int HowManyPlanned  { get; set; }//כמה צמיגים תוכננו
        public int HowManyReady { get; set; }//כמה צמיגים מוכנים
        public string SpecificationChosen { get; set; }//מספר הצמיג בסדרה
        public string CatalogNum { get; set; }
        public string timeStart  { get; set; }//זמן בלחיצת כפתור טען מפרט
        public string timeWeighing { get; set; }//זמן לאחר שידור שקילה
        public double Oplan { get; set; }
        public double Omade{ get; set; }
        public int WorkCenter { get; set; }
        //שליפת תוכנית עבודה
        DBService DBS = new DBService();
        DataTable dataTableemployee = new DataTable();
        public WorkPlan()
        {
            LogWaveClass.LogWave("יצר אובייקט WORKPLAN");
        }

        /// <summary>
        /// מחזיר מפרטים בתוכנית עבודה
        /// </summary>
        /// <returns></returns>
        public DataTable GetWorkingPlane(bool SelectedIndex)
        {
            ConfigureShift(false);//הגדרת משמרת 
            string query =
                $@"SELECT right(TRIM(IDRAW),9) AS IDRAW,  SUM(OPLAN) AS OPLAN, SUM(OMADE) AS OMADE,substring(ICLAS,1,1) as Class
                   FROM  RZPADALI.MCOVIP JOIN BPCSFV30.IIML01 ON OPRIT = IPROD   
                   WHERE ODATE={Date} AND OMACH = '{this.MachineID}' AND ODEPW ={this.DepratmentId} AND OSHIFT = {Shift} 
                   GROUP BY OPRIT, IDRAW,ICLAS";//מפרט,מק"ט,כמה מתוכנן,כמה בוצע , TRIM(IDRAW) AS IDRAW, TRIM(OPRIT) AS OPRIT, SUM(OPLAN) AS OPLAN, SUM(OMADE) AS OMADE שדות נוספים אם תצטרך
            LogWaveClass.LogWave("קבלת תוכנית עבודה: " + query);
            dataTableemployee = DBS.executeSelectQueryNoParam(query);
            return dataTableemployee;
        }

        /// <summary>
        /// כמה מוכנים מתוך המפרטים
        /// </summary>
        /// <returns></returns>
        public void ReadyTires(int SelectedIndex,string SpecificationChosen)// SelectedIndex בודק את מיקום המפרט שבחרנו בשביל לדעת מה לשלוף 
        {
            if (dataTableemployee.Rows.Count != 0)
            {
                LogWaveClass.LogWave("על איזה צמיג עובדים עכשיו וכמה מתוכננים");
                DataRow[] dataRows = dataTableemployee.Select($@"IDRAW='{SpecificationChosen}' ");
                if (dataRows.Count() > 0)
                {
                    if (double.Parse(dataRows[0]["OPLAN"].ToString()) < 10)//בדיקה אם מספר דו ספרתי או חד ספרתי
                        HowManyPlanned = int.Parse(dataRows[0]["OPLAN"].ToString().Substring(0, 1));//כמה מתוכננים
                    else
                        HowManyPlanned = int.Parse(dataRows[0]["OPLAN"].ToString().Substring(0, 2));//כמה מתוכננים
                    if (double.Parse(dataRows[0]["OMADE"].ToString()) < 10)//בדיקה אם מספר דו ספרתי או חד ספרתי
                        HowManyReady = int.Parse(dataRows[0]["OMADE"].ToString().Substring(0, 1));//כמה מוכנים
                    else
                        HowManyReady = int.Parse(dataRows[0]["OMADE"].ToString().Substring(0, 2));
                }
                else
                {

                }
       
            }
        }

        /// <summary>
        /// בודק אם מדובר באותו מספר עובד כמו ברשומה הקיימת או צריך ליצור רשומה חדשה
        //מגיע לפונקציה או לפני טעינת מפרט או תוך כדי טעינת מפרט דרך החלפת עובד
         /// </summary>
        internal void CheckIfTheSameEmployee(string SpecificationChosen,string CatalogNum)
        {       
            ConfigureShift(true);
            LogWaveClass.LogWave("תחילת בדיקה האם מדובר באותו מספר עובד");
            double HowManyMadeTillNow = 0;
            this.SpecificationChosen = SpecificationChosen;
            
            this.CatalogNum = CatalogNum;
            //תמיד תהיה רשומה עם תכנון עבודה שמנהלי משמרת יוצרים עם שיבוץ עובד. על הרשומה הזאת יתבצע עדכון של כמות צמיגים שעשה
            //אם עובד יתחלף תהיה רשומה חדשה עם כמות מתוכננת פחות הכמות שעשה הקודם
            string qry = $@"SELECT  OVLNUM    as idRecord,oemp as EmpolyeeID,OTIMF as FromHour,OWRKC as WorkCenter ,OPLAN,OMADE
                          FROM RZPADALI.MCOVIP JOIN BPCSFV30.IIML01 ON OPRIT = IPROD   
                          WHERE ODATE={Date} AND OMACH = '{MachineID}' AND ODEPW ={this.DepratmentId} AND OSHIFT = '{Shift}'  and  right(TRIM(IDRAW),9)='{SpecificationChosen}'";
            LogWaveClass.LogWave("שליפת כרטיס עבודה " + qry);
            DataTable dataTableemployee = DBS.executeSelectQueryNoParam(qry);
            LogWaveClass.LogWave("עבר קריאת כרטיס עבודה");
            List<int> EmployeeNumbers = new List<int>();//רשימה של מספרי העובדים ששייכים לתוכנית העבודה
            if(dataTableemployee.Rows.Count!=0)//קיימת רשומה בכרטיס עבודה
            {
                int OADIF=CheckOderOADIFfield();//עדיפיות
                double MovlpIDrecord = checkMovlp();
                LogWaveClass.LogWave("עבר קריאת כרטיס עבודה");
                int.TryParse(dataTableemployee.Rows[0]["WorkCenter"].ToString(), out int WorkCenter);
                this.WorkCenter = WorkCenter;
                timeStart = DateTime.Now.ToString("HHmm");
                if ((DateTime.Now.Hour >= 00 && DateTime.Now.Hour < 6) || (DateTime.Now.Hour == 6 && DateTime.Now.Minute <= 30))//אם בין 12 ל6 וחצי צריך שזה יתייחס ליום לפני אז מוסיפים 24
                    timeStart = (double.Parse(timeStart) + 2400).ToString();


                //הוספה לרשימה של כל מספרי העובדים הרלוונטים 
                for (int i = 0; i < dataTableemployee.Rows.Count; i++)
                {
                    int.TryParse(dataTableemployee.Rows[i]["EmpolyeeID"].ToString(), out int EmpNum);
                    EmployeeNumbers.Add(EmpNum);
                    HowManyMadeTillNow += double.Parse(dataTableemployee.Rows[i]["OMADE"].ToString());//כמות מצטברת של צמיגים שנעשתה עד עכשיו
                }

                //אם יש רשומה שמספר עובד שלה 0 נעשה רק עדכון של מספר עובד
                if (EmployeeNumbers.Contains(0))
                {
                    qry = $@"UPDATE RZPADALI.MCOVIP set oemp='000{EmployeeId}'
                           WHERE ODATE={Date} AND OMACH = '{MachineID}' AND ODEPW ={this.DepratmentId} AND OSHIFT = '{Shift}' and OPRIT='{CatalogNum}'  and oemp=''";//and  right(TRIM(IDRAW),9)  ='{SpecificationChosen}'
                    LogWaveClass.LogWave("עדכון מספר עובד אם לא רשום בכלל" + qry);
                    DBS.executeInsertQuery(qry);
                    LogWaveClass.LogWave("עודכן מספר עובד");
                }
           
                //אם זה  מספר עובד חדש-רשומה חדשה
                else if (!EmployeeNumbers.Contains(EmployeeId))
                {
                    Oplan = double.Parse(dataTableemployee.Rows[0]["OPLAN"].ToString()) - double.Parse(dataTableemployee.Rows[0]["OMADE"].ToString());//כמות מתוכננת פחות כמה שעשו עובדים קודמים
                    if (Oplan < 0) Oplan = 0;
                    Omade = double.Parse(dataTableemployee.Rows[0]["omade"].ToString());
                    //סיום רשומת עובד אחרון בכרטיס העבודה-שדה כמות תוכננה שווה לשדה כמות שבוצעה
                    qry= $@"UPDATE RZPADALI.MCOVIP set oplan={Omade}
                           WHERE ODATE={Date} AND OMACH = '{MachineID}' AND ODEPW ={this.DepratmentId} AND OSHIFT = '{Shift}' and OPRIT='{CatalogNum}' and oemp='000{EmployeeNumbers[0]}'";
                    LogWaveClass.LogWave("סיום רשומת עובד אחרון בכרטיס העבודה " + qry);
                    DBS.executeInsertQuery(qry);
                    //רשומה חדשה לעובד אחר במספר עובד אני רושם 0,אחרי שקילת צמיג ראשונה אני מעדכן מספר עובד במחלקת actualtire
                    qry = $@"INSERT INTO RZPADALI.MCOVIP values({Date},{DepratmentId},{WorkCenter},'000{EmployeeId}',{DepratmentId},'{MachineID}','{CatalogNum}','{Shift}',{Oplan},{OADIF},'',0,'',0,0,{timeStart},0,'',0,0,0,'','','','','',0,0,0,'','',{DateTime.Now.ToString("HHmmss") + DateTime.Now.ToString("ddMMyy")},'{MachineID}-C#','{System.Environment.MachineName}',{MovlpIDrecord})";
                    LogWaveClass.LogWave("רשומה חדשה לעובד אחר " + qry);
                    DBS.executeSelectQueryNoParam(qry);

                    Form1.CreatedWorkTicket = false;
                    LogWaveClass.LogWave("סיום checkIfTheSameEmployee "+qry);
                }
            }
        }

        /// <summary>
        /// פנייה לקובץ MOVLP  -לוקח משם את הרשומה הבודדת ומקדם אותה ב1
        /// </summary>
        /// <returns></returns>
        private double checkMovlp()
        {
            string qry = $@"select *
                          from RZPALI.movlp";
            LogWaveClass.LogWave("checkmovlp "+qry);
            DataTable dataTableMovlp = new DataTable();
            dataTableMovlp = DBS.executeSelectQueryNoParam(qry);
            double.TryParse(dataTableMovlp.Rows[0]["MOVL1#"].ToString(), out double MovlpIDrecord);
            MovlpIDrecord += 1;
            qry = $@"UPDATE RZPALI.movlp set MOVL1#={MovlpIDrecord}";
            LogWaveClass.LogWave(qry);
            DBS.executeInsertQuery(qry);
            return MovlpIDrecord;//מקדם ב1 לזיהוי רשומה
        }

        /// <summary>
        /// רק בשביל שדה במידה ונפתחת רשומה חדשה בתוכנית העבודה OADIF. השדה אומר מה העדיפיות של המכונה-לא קריטי לעשות בסדר כרונולוגי
        /// </summary>
        public int CheckOderOADIFfield()
        {           
            string qry = $@"SELECT max(OADIF+1) as OADIF
                          FROM RZPADALI.MCOVIP
                           WHERE ODATE={Date} AND OMACH = '{MachineID}' AND ODEPW ={this.DepratmentId} and  OSHIFT='{this.Shift}'";
            LogWaveClass.LogWave(qry);
            DataTable OadifTable = new DataTable();
            OadifTable = DBS.executeSelectQueryNoParam(qry);
            int.TryParse(OadifTable.Rows[0][0].ToString(), out int Oadif);
            return Oadif;
        }


        /// <summary>
        /// מוסיף צמיג אחד לצמיגים המוכנים לאחר שהיו שידורי שקילה
        /// </summary>
        internal void AddOneTire(string SpecificationNow)
        {
            try
            {
                timeWeighing = DateTime.Now.ToString("HHmm");
                if ((DateTime.Now.Hour >= 00 && DateTime.Now.Hour < 6) || (DateTime.Now.Hour == 6 && DateTime.Now.Minute <= 30))//אם בין 12 ל6 וחצי צריך שזה יתייחס ליום לפני אז מוסיפים 24
                    timeWeighing = (double.Parse(timeWeighing) + 2400).ToString();
                LogWaveClass.LogWave("12");
                string qry = "";
                if (PreviousShift != Shift && SpecificationChosen== SpecificationNow)//אם המשיך עובד לעבוד אחרי שסיים משמרת על אותו מפרט
                {
                    AddTireAfterNewShift();
                }
                else //שאילתה דיפולטיבית להעלאת קאונטר ב1
                {
                    qry = $@"UPDATE RZPADALI.MCOVIP set OMADE=omade+1,OTIMF={timeStart},OTIMT={timeWeighing} 
                     WHERE ODATE={Date} AND OMACH = '{MachineID}' AND ODEPW ={this.DepratmentId} AND OSHIFT = '{Shift}' and OPRIT='{CatalogNum}'  and oemp='000{EmployeeId}'";

                    LogWaveClass.LogWave("העלאת קאונטר ב1 " + qry);
                    DBS.executeInsertQuery(qry);
                }

            }

            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                LogWaveClass.ErrorLogWave("Error: " + ex.Message);
            }



        }

        /// אם אותו עובד ממשיך לעבוד על אותו מפרט אחרי שהתחלפה המשמרת 
        private void AddTireAfterNewShift()
        {
            try
            {
                string qry = $@"SELECT  OVLNUM    as idRecord,oemp as EmpolyeeID,OTIMF as FromHour,OWRKC as WorkCenter ,OPLAN,OMADE
                          FROM RZPADALI.MCOVIP JOIN BPCSFV30.IIML01 ON OPRIT = IPROD   
                          WHERE ODATE={Date} AND OMACH = '{MachineID}' AND ODEPW ={this.DepratmentId} AND OSHIFT = '{Shift}'  and  right(TRIM(IDRAW),9)='{SpecificationChosen}' and OEMP='000{EmployeeId}";
                LogWaveClass.LogWave("שליפת כרטיס עבודה " + qry);
                DataTable dataTableemployee = DBS.executeSelectQueryNoParam(qry);
                if(dataTableemployee.Rows.Count==0)
                {
                    int OADIF = CheckOderOADIFfield();//עדיפיות
                    double MovlpIDrecord = checkMovlp();
                    LogWaveClass.LogWave("אותו עובד ממשיך לעבוד על אותו מפרט אחרי שהתחלפה המשמרת");
                    qry = $@"INSERT INTO RZPALI.MCOVIP values({Date},{DepratmentId},{WorkCenter},'000{EmployeeId}',{DepratmentId},'{MachineID}','{CatalogNum}','{Shift}',0,{OADIF},'',0,'',1,0,{timeStart},0,'',0,0,0,'','','','','',0,0,0,'','',{DateTime.Now.ToString("HHmmss") + DateTime.Now.ToString("ddMMyy")},'{MachineID}-C#','{System.Environment.MachineName}',{MovlpIDrecord+1})";//oplan=0 omade=1  oplan between shift and oadif omade-.2 fields before timestart
                    LogWaveClass.LogWave("רשומה חדשה לעובד אחר " + qry);
                    DBS.executeSelectQueryNoParam(qry);
                    Form1.CreatedWorkTicket = true;
                }
                else
                {
                    qry = $@"UPDATE RZPADALI.MCOVIP set OMADE=omade+1,OTIMF={timeStart},OTIMT={timeWeighing} 
                     WHERE ODATE={Date} AND OMACH = '{MachineID}' AND ODEPW ={this.DepratmentId} AND OSHIFT = '{Shift}' and OPRIT='{CatalogNum}'  and oemp='000{EmployeeId}'";
                    LogWaveClass.LogWave("רשומה חדשה לעובד אחר עדכון" + qry);
                    DBS.executeSelectQueryNoParam(qry);
                }
            }
             catch(Exception ex)
            {
                LogWaveClass.LogWave(ex.Message);
                //MessageBox.Show(ex.Message);
            }
          
        }

        /// <summary>
        /// הגדרת משמרת 
        /// </summary>
        public void ConfigureShift(bool FromSameEmpFunc)
        {
            LogWaveClass.LogWave("התחיל להגדיר משמרת");
            if (PreviousShift != Shift && FromSameEmpFunc)//רק אם בא מתוך פונקציית החלפת עובד
                PreviousShift = Shift;//לשימוש שידור העלאת קאונטר צמיג אם המשיך לעבוד אחרי סיום המשמרת

            Date = DateTime.Now.ToString("1yyMMdd");
            TimeSpan time = DateTime.Now.TimeOfDay;
            if (DateTime.Now.TimeOfDay >= Convert.ToDateTime("06:30:00").TimeOfDay && DateTime.Now.TimeOfDay <= Convert.ToDateTime("14:59:59").TimeOfDay)
            {
                Shift = 2;
            }
             
            else if (DateTime.Now.TimeOfDay >= Convert.ToDateTime("15:00:00").TimeOfDay && DateTime.Now.TimeOfDay <= Convert.ToDateTime("23:29:59").TimeOfDay)
            {
                Shift = 3;
            }
            else
            {
                Shift = 1;
                if (DateTime.Now.TimeOfDay >= Convert.ToDateTime("23:29:59").TimeOfDay && DateTime.Now.TimeOfDay <= Convert.ToDateTime("23:59:59").TimeOfDay)
                {
                    Date = DateTime.Now.AddDays(1).ToString("1yyMMdd");
                    //Labelgp_date = DateTime.Now.AddDays(1).ToString("yyyyMMdd");
                }
            }
            LogWaveClass.LogWave("סיים להגדיר משמרת");
        }

    }
}
