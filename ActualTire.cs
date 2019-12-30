using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;

namespace TireManufacturing
{
    /// <summary>
    ///  מתאר נתוני גלגל בפועל ונתוני ברקוד
    /// </summary>
    class ActualTire:General //מפרט נוכחי  
    {
        public int KarkasCode { get; set; }//סריקת תווית קרקס
        public int TireCode { get; set; }//סריקת תווית צמיג ירוק
        public  int Dna { get; set; }//dna
        public string DnaWithLetter { get; set; }//חדש-dna שלילי
        public string CatalogNum { get; set; }//מק"ט מתוך מפרט
        public int SerialNumber { get; set; }//מספר סידורי-מכיל 6 ספרות
        public string LevelWork { get; set; }//שלב עבודה
        public double WeightKarkas { get; set; }//משקל קרקס-שלב א
        public double WeightKarkasFromPast { get; set; }//משקל קרקס שנלקח מתוך נתוני שקילה. בשימוש כאשר בשלב ב' לא מחייבים שקילת קרקס לפני
        public double DeviationKarkas { get; set; }//סטייה קרקס
        public double WeightGreenTire { get; set; }//משקל צמיג ירוק
        public double DeviationGreenTire { get; set; }//סטייה צמיג ירוק
        public double WeightLevelB { get; set; }//משקל שלב ב
        public double DeviationLevelB { get; set; }//סטייה שלב ב

        public int TireNumber { get; set; }//31.7.2019 מספר צמיג בסדרת הייצור-נלקח משדה כמה מוכנים ממחלקת WORKPLAN
        public string WorkCenter { get; set; }//מרכז עבודה
        public int Shift { get; set; }
        //אישור מנהל
        public string WhichCase { get; set; }//איזה שלב אנחנו שוקלים עכשיו
        public List<int> Managers { get; set; }//רשימת מנהלים מאשרים
        public int DnaMangerConfirm { get; set; }//ניקח מתוך פורם מנהל מאשר
        public string DnaMangerConfirmWithLetter { get; set; }//ניקח מתוך פורם מנהל מאשר

        public int ManagerID { get; set; }//מי המנהל שאישר כרכס בלי שקילה
        public string CatalogNumMangerConfirm { get; set; }//ניקח מתוך פורם מנהל מאשר
        public int SerialNumMangerConfirm { get; set; }//ניקח מתוך פורם מנהל מאשר

        public int timeUntilSaveWeight { get; set; }//כמה שניות צריך לחכות עד שידור השקילה
        public bool WeighingSave { get; set; }//האם לשדר שקילה או לא
        public bool BlockWeightPrint { get; set; }//ימנע שוב לחיצה על המשקל אחרי שמתחיל שידור שקילה
        public bool SendSms  { get; set; }//יש הודעה על חריגה במשקל?
        public TireSpecifications tireSpecifications { get; set; }
        DBService DBS = new DBService();

        public ActualTire()
        {
            LogWaveClass.LogWave("יצר אובייקט ActualTire");
            timeUntilSaveWeight = 60;//אחרי דקה ישדר שקילה
            WeighingSave = false;//חוסם שידור שקילה     
            BlockWeightPrint = false;//ימנע שוב לחיצה על המשקל אחרי שמתחיל שידור שקילה
        }

        /// <summary>
        /// בנאי של סבב חדש לפני שקילה חדשה
        /// </summary>
        public ActualTire(int EmpNo,string Specification, List<int> Managers,string CatalogNum,string LevelWork):this()
        {
            this.EmployeeId = EmpNo;
            this.Specifications = Specification;
            this.Managers = Managers;
            this.CatalogNum = CatalogNum;
            this.LevelWork = LevelWork;
        }





        /// <summary>
        /// מקבל את נתוני השקילה של הקרקס עבור שלב ב'
        /// </summary>
        public bool CheckIfLevelBDnaUsed(string CaracasCatalogNum)
        {
  
            DataTable dataTable = new DataTable();
            CatalogNumMangerConfirm = "";
            string query =
                $@"SELECT A.LPROD,A.LYYWW,A.LLBLNO,A.LSTT,A.LMACH,A.LDEPT,A.LWRKC,A.LSHIFT,right(trim(A.LSPEC),9) as LSPEC,A.LKGMF,A.LVLNUM,A.LDATE,A.LTIME,A.LICLAS,B.LACTAN,LADESC,B.LKGAC      
                   FROM TAPIALI.LABELL1 A left join TAPIALI.LABELGP B on A.LPROD=B.LPROD  and A.LLBLNO=B.LLBLNO
                   WHERE  A.LLBLNA={Dna} and B.LACTAN in (3, 103) order by B.LACTAN desc";//בדיקת dna ו103-ירוק 3-קרקס
            LogWaveClass.LogWave(query);
            dataTable = DBS.executeSelectQueryNoParam(query);
            //כרכס ללא שקילה בשלב ב-אישור מנהל
            if(dataTable.Rows.Count==0)
            {
                ManagerConfirm managerConfirm = new ManagerConfirm(Managers, CatalogNum,DnaWithLetter,Specifications);//אישור מנהל שהתווית לא עברה שקילה
                managerConfirm.ShowDialog();
                if (((ManagerConfirm)managerConfirm).DnaAndSrialOk == true)//רק אם היה אישור בדיקה של דיאנאיי וסריאלי
                {
                    DnaMangerConfirm = ((ManagerConfirm)managerConfirm).Dna;
                    DnaMangerConfirmWithLetter = ((ManagerConfirm)managerConfirm).DnaWithLetter;
                    CatalogNumMangerConfirm = ((ManagerConfirm)managerConfirm).CatalogNum;
                    SerialNumMangerConfirm = ((ManagerConfirm)managerConfirm).SerialNum;
                    ManagerID = ((ManagerConfirm)managerConfirm).ManagerId;
                }
        
                if (DnaMangerConfirm == 0) return false;//סגר את החלון אישור מנהל ואז לא נרשם כלום
                return true;//המנהל אישר אי שקילת קרקס,ניתן להמשיך ולשקול את הקרקס
            }
            else if(dataTable.Rows.Count==1)
            {
                LogWaveClass.LogWave("אם הרשומה הבודדה היא קרקס המצב תקין ואפשר לעבור לשלב ב 3 זה תקין 103 לא'       " + dataTable.Rows[0]["LACTAN"].ToString());
                if(int.Parse(dataTable.Rows[0]["LACTAN"].ToString())==3)//אם הרשומה הבודדה היא קרקס המצב תקין ואפשר לעבור לשלב ב'
                {
              
                    if (!(dataTable.Rows[0]["LPROD"].ToString() == CaracasCatalogNum && dataTable.Rows[0]["LSPEC"].ToString() == Specifications))//וידוא שהמק"ט והדינאיי תואמים
                    {
                        MessageBox.Show("מפרט לא תואם את הנוכחי שייך למפרט " + Environment.NewLine + dataTable.Rows[0]["LSPEC"].ToString(), "הודעת שגיאה", MessageBoxButtons.OK, MessageBoxIcon.Exclamation, MessageBoxDefaultButton.Button1);
                        return false;
                    }
                   
                    double.TryParse(dataTable.Rows[0]["LKGAC"].ToString(), out double w);//לוקח משקל קרקס מהשקילה שבוצעה כבר-רק במקרה שלא מחייב שקילת קרקס לפני המשך שלב ב
                    WeightKarkasFromPast = w* 2.205;
                }
            }
            else if (dataTable.Rows.Count == 2)
            {
                LogWaveClass.LogWave("יש שתי רשומות בבדיקת דיאנאיי -דווח הדיאנאיי" + dataTable.Rows[0]["LACTAN"].ToString());
                if (int.Parse(dataTable.Rows[0]["LACTAN"].ToString()) == 103 && int.Parse(dataTable.Rows[1]["LACTAN"].ToString()) == 3)//במידה ודווח כבר על הצמיג-תמיד יהיה בסדר הזה בעקבות השאילתה
                    {
                         MessageBox.Show("DNA מסוג זה דווח בעבר" + Environment.NewLine + dataTable.Rows[0]["LSPEC"].ToString(), "הודעת שגיאה", MessageBoxButtons.OK, MessageBoxIcon.Exclamation, MessageBoxDefaultButton.Button1);
                         return false;
                    }
            }
            return true;         
           

        }


        /// <summary>
        /// בדיקות המתבצעות בסריקת ברקוד של קרקס-תוויות
        /// </summary>
        public bool CheckLevelSerialNumber(string LevelWorkSerial)
        {
            //this.LevelWork = LevelWork;
            bool ContinueNext = true;
            DataTable dataTable = new DataTable();
            string query = "";

            if (LevelWorkSerial == "שלב א")
                query =
                    $@"SELECT A.LPROD,A.LYYWW,A.LLBLNO,A.LSTT,A.LMACH,A.LDEPT,A.LWRKC,A.LSHIFT,right(trim(A.LSPEC),9) as LSPEC,A.LKGMF,A.LVLNUM,A.LDATE,A.LTIME,A.LICLAS, B.LLBLNA
                   FROM TAPIALI.LABELL1 A left join TAPIALI.LABELGP B on A.LPROD=B.LPROD  and A.LLBLNO=B.LLBLNO and B.LACTAN = 3
                   WHERE A.LPROD = '{CatalogNum}' AND A.LLBLNO = {SerialNumber}";

            else if (LevelWorkSerial == "צמיג מלא" || LevelWorkSerial=="שלב ב")
                query =
                    $@"SELECT A.LPROD,A.LYYWW,A.LLBLNO,A.LSTT,A.LMACH,A.LDEPT,A.LWRKC,A.LSHIFT,right(trim(A.LSPEC),9) as LSPEC,A.LKGMF,A.LVLNUM,A.LDATE,A.LTIME,A.LICLAS, B.LLBLNA
                   FROM TAPIALI.LABELL1 A left join TAPIALI.LABELGP B on A.LPROD=B.LPROD  and A.LLBLNO=B.LLBLNO and B.LACTAN = 103
                   WHERE A.LPROD = '{CatalogNum}' AND A.LLBLNO = {SerialNumber}";


            LogWaveClass.LogWave(query);
            dataTable = DBS.executeSelectQueryNoParam(query);
            if (dataTable.Rows.Count > 0)
            {
                if (dataTable.Rows[0]["LLBLNA"].ToString() == "")
                {
                    if (dataTable.Rows[0]["LSPEC"].ToString() != Specifications)
                    {
                        MessageBox.Show("מפרט לא תואם את הנוכחי שייך למפרט " + Environment.NewLine + dataTable.Rows[0]["LSPEC"].ToString(), "הודעת שגיאה", MessageBoxButtons.OK, MessageBoxIcon.Exclamation, MessageBoxDefaultButton.Button1);
                        ContinueNext = false;//אם מפרט לא שווה לשלנו
                    }
                }
                else
                {
                    MessageBox.Show("היה שימוש בתווית זו בעבר" + Environment.NewLine + dataTable.Rows[0]["LSPEC"].ToString(), "הודעת שגיאה", MessageBoxButtons.OK, MessageBoxIcon.Exclamation, MessageBoxDefaultButton.Button1);
                    ContinueNext = false;//אם לסידורי llblna יש ערך נעשה בו שימוש
                }

            }
            else
            {
                ContinueNext = false;// אם אין דטה טייבל אין שימוש במק"ט בכלל
                MessageBox.Show("תווית לא מזוהה" , "הודעת שגיאה", MessageBoxButtons.OK, MessageBoxIcon.Exclamation, MessageBoxDefaultButton.Button1);
            }
                return ContinueNext;

        }


        /// <summary>
        /// בודק אם נעשה שימוש בעבר בדיאנאיי
        /// </summary>
        public bool CheckIfDnaUsed()
        {
            DataTable dataTable = new DataTable();
            string query =
                $@"SELECT COUNT(LPROD) as Count FROM TAPIALI.LABELGP WHERE LLBLNA ={Dna}";
            dataTable = DBS.executeSelectQueryNoParam(query);
          //  if (dataTable.Rows.Count == 0)
          //      return false;

            if (dataTable.Rows[0]["Count"].ToString() == "0") return true;
            else
            {
                MessageBox.Show($@"ב{Dna}" + Environment.NewLine + "זה היה שימוש בעבר,נא הכנס חדש", "הודעת שגיאה", MessageBoxButtons.OK, MessageBoxIcon.Exclamation, MessageBoxDefaultButton.Button1);
                return false;
            }
            
        }



        /// <summary>
        /// בדיקת התראות למנהלים אחרי שקילה
        /// </summary>
        public void CheckAlert(string WhichCase, double Deviation, double ActualWeight)
        {
            if ((Deviation < -3.5) || 
                (LevelWork == "שלב ב" && Deviation > 5.5 && ActualWeight <= 50) ||
                (LevelWork == "שלב ב" && Deviation > 4 && ActualWeight > 50 && ActualWeight <= 150) ||
                (LevelWork == "שלב ב" && Deviation > 3.5 && ActualWeight > 150) ||
                (LevelWork == "צמיג מלא" && Deviation > 4 && ActualWeight > 50 && ActualWeight <= 150)||
                (LevelWork == "צמיג מלא" && Deviation > 3 && ActualWeight > 150))
            {
                SendSmsAlert(Deviation);
            }

        }


        /// <summary>
        /// רשימת מקבלי התראות במקרה של חריגה במשקל
        /// </summary>
        public void SendSmsAlert(double Deviation)
        {
            DataTable dataTable = new DataTable();
            string Param = "Deviation in tire weight - T-" + MachineID + " Dept " + DepratmentId + ". Cat. " + CatalogNum + ", DNA " + Dna + ", Deviation " + Deviation + ", Tire No. " /*+ */;
            string Sqlstr ="" /*"CALL SYS.SNDMSGPC('" + Rpad("TIREBULDT", 10, " ") + "','" + Format(Len(Trim(Param)), "000") + "','" & Param & "')"*/;
            DBS.executeInsertQuery(Sqlstr);
        }

        /// <summary>
        /// שם לתוך שדה dna מספרים בלבד
        /// </summary>
        internal bool PutDnaField(string TxtDna)
        {
            DnaWithLetter = TxtDna;
            switch (DnaWithLetter.Substring(5, 1))
            {
                case "J":
                TxtDna = "-" + DnaWithLetter.Substring(0, 5) + "1";
                    break;

                case "K":
                    TxtDna = "-" + DnaWithLetter.Substring(0, 5) + "2";
                    break;

                case "L":
                    TxtDna = "-" + DnaWithLetter.Substring(0, 5) + "3";
                    break;

                case "M":
                    TxtDna = "-" + DnaWithLetter.Substring(0, 5) + "4";
                    break;

                case "N":
                    TxtDna = "-" + DnaWithLetter.Substring(0, 5) + "5";
                    break;

                case "O":
                    TxtDna = TxtDna = "-" + DnaWithLetter.Substring(0, 5) + "6";
                    break;

                case "P":
                    TxtDna = "-" + DnaWithLetter.Substring(0, 5) + "7";
                    break;

                case "Q":
                    TxtDna = "-" + DnaWithLetter.Substring(0, 5) + "8";
                    break;

                case "R":
                    TxtDna = "-" + DnaWithLetter.Substring(0, 5) + "9";
                    break;
            }
            try
            {
                Dna = int.Parse(TxtDna);
                return true;
            }
            catch (FormatException e)
            {
                MessageBox.Show("שגיאה בהכנסת dna");
                return false;
            }

        }

        /// <summary>
        /// בדיקה בצמיג שלם ושלב ב האם לשלוח מייל למנהל כתוצאה מסטיית משקל
        /// </summary>
        public void CheckIFSmsDeviation(double ActualWeight, double deviation, string WhichCase)
        {
            string SMS = $@"Deviation in tire weight - T- {MachineID} Department {DepratmentId} .Cat. {CatalogNum} ,DNA {Dna} deviation {deviation} tire No. {TireCode}";
            SendSms = false;
            //אלה התנאים מתי שתשלח הודעה למנהלים
            if (deviation < -3.5)
                SendSms = true;
            else if(WhichCase=="שלב ב")
            {
                if ((deviation >= 5.5 && ActualWeight <= 50) || (deviation >= 4 &&
                    ActualWeight <= 150 && ActualWeight > 50) || (deviation >= 3.5 && ActualWeight > 150))
                    SendSms = true;
            }
            else if(WhichCase=="צמיג מלא")
            {
                if ((deviation >= 4 && ActualWeight <= 150 && ActualWeight > 50) || (deviation >= 3 && ActualWeight > 150))
                    SendSms = true;
            }
            string Sqlstr = "";
            if (SendSms)
            {
                Sqlstr = $@"CALL SYS.SNDMSGPC('TIREBULD  ','{SMS.Length}','{SMS}')";//to check in ALLTAB.SNDMSGP ,  lenght 10 of first word          
                DBS.executeInsertQuery(Sqlstr);
            }
            

        }

        /// <summary>
        /// שמירת משקל לאס400
        /// </summary>
        internal void SaveWeightToAS400(TireSpecifications tireSpecifications,int Shift)
        {
            try
            {
                LogWaveClass.LogWave("לפני שידור שקילה ראשון");
                string InsertQry = "";
                DataTable dataTable = new DataTable();
                this.Shift = Shift;
                this.tireSpecifications = tireSpecifications;
                timeUntilSaveWeight = 60;
                WeighingSave = false;//חוסם שידור שקילה
                BlockWeightPrint = true;//חוסם כפתור פרינט מהראש שקילה כי מתחיל שידור שקילה כבר
                string Date = DateTime.Now.ToString("yyyyMMdd");
                string time = DateTime.Now.ToString("HHmmss");
                double Weight = WeightGreenTire;//שמתי בדיפולט צמיג שלם, אם יהיה שלב א נשנה
                int LevelNum = 103;//או 103-צמיג שלם או 3-כרכס
                double WeightSpecification = tireSpecifications.TireWeight;
                WorkCenter = "0";
                string WeekOfTheYear = "0", Description = "", LVLNUM = "0", FatherProduct = "", PrefixSpecification = "", SpecificationFromAS400 = "", ItemType = "";
                long NumRun = 0;
                //שדות עבור אישור מנהל
                int LWSID = 0;
                char LERRDS = ' ', LERRCD = ' ';
                //בשליחת הודעה על חריגת משקל
                char LACCPT = ' ';
                string LACCTM = time;// מספר צמיג בסדרה כשאין חריגה אז השדה המשודר הוא הזמן בשעות
                LogWaveClass.LogWave("2");

                if (WhichCase == "שלב א")
                {
                    LevelNum = 3;
                    Weight = WeightKarkas;
                    WeightSpecification = tireSpecifications.CarcasWeight;


                    //אישור מנהל-כרכס בלי שקילה
                    if (ManagerID != 0 && LevelWork == "שלב ב")
                    {
                        LWSID = ManagerID;
                        LERRDS = '1';
                    }
                }

                //אם הייתה שליחת הודעה על חריגה במשקל
                if (SendSms)
                {
                    LACCPT = '@';
                    LACCTM = TireNumber.ToString();
                }

                //PRRP
                try
                {
                    //שידור שקילה בלוג שקילות            
                    InsertQry = $@"INSERT INTO STWIND.PRRP values('{MachineID}','{DateTime.Now.ToString("yyyy-MM-dd-HH.mm.ss.000000")}','00{EmployeeId}0','{CatalogNum}','{tireSpecifications.Size}','{SerialNumber}',{tireSpecifications.TireWeight},{WeightGreenTire},{tireSpecifications.CarcasWeight},{WeightKarkas},0,0,0,'','','','','{Specifications}')";
                    LogWaveClass.LogWave(InsertQry);
                    if (DnaMangerConfirm == 0)//אם זה בא מהמנהל אישור קרקס ללא שקילה לא משדר לPRRP
                        DBS.executeInsertQuery(InsertQry);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                    LogWaveClass.ErrorLogWave("Error: " + ex.Message);
                }

                //LABELP
                try
                {
                    //עדכון טבלת כותרות תוויות רשומת LNSTT ל3 
                    //קישור DNA לתווית            
                    InsertQry = $@"UPDATE  TAPIALI.LABELP set LSTT=3,LLBLNA={Dna},LKGAC={Math.Round(Weight / 2.2046, 3)},LSTTDT={Date},LSTTTM={time} Where LPROD='{CatalogNum}' and LLBLNO={SerialNumber} ";
                    DBS.executeInsertQuery(InsertQry);
                    LogWaveClass.LogWave("4" + InsertQry);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                    LogWaveClass.ErrorLogWave("Error: " + ex.Message);
                }

                //LABELGP
                try
                {
                    //שידור לקובץ תוויות לוג
                    //יש יותר פרטים בקובץ לוג בגלל זה מעדכנים גם בלוג וגם בPRRP
                    InsertQry = $@"SELECT MAX(LLOGNO) as max FROM TAPIALI.LABELGP";//שליפת מספר רץ אחרון 
                    
                    LogWaveClass.LogWave(" שליפת מספר רץ אחרון עבור LABELGPלפני  " + InsertQry);
                    dataTable = DBS.executeSelectQueryNoParam(InsertQry);
                    LogWaveClass.LogWave(" קיבל דטה טייבל של שליפת מספר אחרון  " + InsertQry);
                    NumRun = long.Parse(dataTable.Rows[0]["max"].ToString())+1;//מספר רץ עוקב לאחרון 
                    LogWaveClass.LogWave(" המיר מספר NUMRUN");
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                    LogWaveClass.ErrorLogWave("Error: " + ex.Message);
                }

                try
                {
                    //לפני הכנסה לטבלת לוג תוויות ללוקח שדות רלוונטים מתוך טבלת כותרות תוויות
                    InsertQry = $@"SELECT  LYYWW as WeekYear ,LVLNUM,LWRKC as WorkCenter,LPRODA as FatherProduct,left(TRIM(LSPEC),1) as PrefixSpecification,LICLAS as ItemType,LSPEC as Specification
                         FROM TAPIALI.LABELP
                         Where LPROD='{CatalogNum}' and LLBLNO={SerialNumber}";
                    LogWaveClass.LogWave("לפני שליפת נתונים עבור LABELGP   " + InsertQry);

                    dataTable = DBS.executeSelectQueryNoParam(InsertQry);
                    LogWaveClass.LogWave(InsertQry);

                    if (dataTable.Rows.Count != 0)
                    {
                        WeekOfTheYear = dataTable.Rows[0]["WeekYear"].ToString();
                        Description = ""; /*dataTable.Rows[0]["description"].ToString();*/
                        LVLNUM = dataTable.Rows[0]["LVLNUM"].ToString();
                        WorkCenter = dataTable.Rows[0]["WorkCenter"].ToString();
                        SpecificationFromAS400 = dataTable.Rows[0]["Specification"].ToString();//המפרט כולו
                        PrefixSpecification = dataTable.Rows[0]["PrefixSpecification"].ToString();//המקדים של המפרט
                        ItemType = dataTable.Rows[0]["ItemType"].ToString();
                    }

                    //מקרי קצה
                    //בצמיג שלם אם מופיע גם מפרט עבור כרכס-נשדר רשומה פיקטיבית כאילו בוצע גם שלב א
                    int SerialNumberForLogLabel = SerialNumber;//בהתחלה זה המקורי במקרי קצה זה משתנה
                    if (WhichCase == "צמיג שלם")
                    {
                        LogWaveClass.LogWave("לפני שידור רשומה פיקטיבית של קרקס במקרה של צמיג שלם-מקרה קצה");
                        if (tireSpecifications.CarcasWeightKg != 0)
                        {
                            LERRCD = '1';
                            time = (int.Parse(time) - 60).ToString();//יהיה דקה לפני הרשומה
                            SerialNumberForLogLabel = 0;
                            LevelNum = 3;
                            InsertToLogLabel();//הכנסת רשומה פיקטיבית
                                               //עדכון חזרה
                            LERRCD = ' ';
                            time = (int.Parse(time) + 60).ToString();//יהיה דקה לפני הרשומה
                            SerialNumberForLogLabel = SerialNumber;
                            LevelNum = 103;
                        }
                    }

                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                    LogWaveClass.ErrorLogWave("Error: " + ex.Message);
                }





                LogWaveClass.LogWave("לפני שידור LABELGP");

                InsertToLogLabel();
                //הכנסה לקובץ לוג תוויות
                void InsertToLogLabel()
                {
                    InsertQry = $@"Insert into TAPIALI.LABELGP values('{CatalogNum}',{WeekOfTheYear},{SerialNumber},'{MachineID}',{Date},{time},{NumRun},{LevelNum},'','','','{Description}',{DepratmentId}
                           ,{WorkCenter},{Shift},'{SpecificationFromAS400}','','{PrefixSpecification}','{tireSpecifications.SubstringsSpecifications[0]}','{tireSpecifications.SubstringsSpecifications[1]}','{tireSpecifications.SubstringsSpecifications[2]}',
                            '   {EmployeeId}','{ItemType}',{WeightSpecification / 2.2046},{Weight / 2.2046},'{LERRCD}','{LERRDS}','2',{LVLNUM},{Date},{time},'{tireSpecifications.FatherProduct}',0,{Dna},{Date},{time},'{MachineID}',
                            '{LWSID}','{DnaWithLetter}','',{Date},{time},'{DateTime.Now.ToString("yyyy-MM-dd-HH.mm.ss.000000")}','{LACCPT}')";
                    DBS.executeInsertQuery(InsertQry);
                }
                LogWaveClass.LogWave(InsertQry);
            }

            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                LogWaveClass.ErrorLogWave("Error: " + ex.Message);
            }


        }

        /// <summary>
        /// שלב ב בשמירת נתוני שקילה-שמירת רכיבי צמיג
        /// </summary>
        /// <param name="selfInspection"></param>
        internal void SaveTireComponents(SelfInspection selfInspection)
        {
            try
            {
                string qry = "";
                string SerialNumberSon = "", CatalogNumSon = "";
                double Quantity = 0;

                //שידור כרכס כרשומת מרכיב צמיג בשלב ב
                if (LevelWork == "שלב ב")
                {
                    Quantity = 1;
                    SerialNumberSon = DnaWithLetter;//מספר סריאלי של כרכס ברשומה זו יהיה הדיאנאיי
                    CatalogNumSon = tireSpecifications.CaracasCatalogNum;
                    LogWaveClass.LogWave("שידור כרכס כרשומת מרכיב צמיג בשלב ב " + qry);
                    InsertComponent("שידור כרכס כרשומת מרכיב צמיג בשלב ב");
                }

                //לולאה על כל רשומה בטבלת מרכיבים
                for (int i = 0; i < selfInspection.ForDataGrid.Rows.Count; i++)
                {
                    CatalogNumSon = selfInspection.ForDataGrid.Rows[i]["BCHLD"].ToString();
                    SerialNumberSon = selfInspection.ForDataGrid.Rows[i]["סידורי"].ToString();
                    if (SerialNumberSon == "999999")
                        SerialNumberSon = "";
                    Quantity = double.Parse(selfInspection.ForDataGrid.Rows[i]["bqreq"].ToString());
                    InsertComponent("לולאה מרכיבים");
                }

                //הכנסת רשומת רכיב
                void InsertComponent(string FromWhere)
                {
                    qry = $@"INSERT into BPCSFALI.MBMG values('{CatalogNum}','{DnaWithLetter}',1,'{CatalogNumSon}','{SerialNumberSon}',{Quantity},'{MachineID}',{WorkCenter},{DepratmentId},'000{EmployeeId}',{DateTime.Now.ToString("yyMMdd")},{DateTime.Now.ToString("HHmmss")},
                        '{Shift}','{DateTime.Now.ToString("yyyy-MM-dd-HH.mm.ss.000000")}','C#','{System.Environment.MachineName}','','','',0,0,0)";
                    LogWaveClass.LogWave(FromWhere + " " + qry);
                    DBS.executeInsertQuery(qry);
                }


            }

            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                LogWaveClass.ErrorLogWave("Error: " + ex.Message);
            }


        }

        /// <summary>
        /// דוח שקילות עבור עובד מסוים
        /// </summary>
        public DataTable MakeReportForEmployee(int EmployeeNumReport, DateTime dateTime)
        {
            string qry = $@"SELECT LPROD as CatalogNum ,SUBSTR(CHAR(LACNDT), 7, 2) Concat '/' Concat SUBSTR(CHAR(LACNDT), 5, 2) Concat '/' Concat SUBSTR(CHAR(LACNDT), 1, 4) as Date,SUBSTR(CHAR(RIGHT('0000000'||LACNTM,6)), 1, 2) Concat ':' Concat SUBSTR(CHAR(RIGHT('0000000'||LACNTM,6)), 3, 2) Concat ':' Concat SUBSTR(CHAR(RIGHT('0000000'||LACNTM,6)), 5, 2) as Time ,LSPEC as Specification,LKGAC as WeightActual,LKGMF as WeightSpec,(LKGAC/LKGMF -1) as Deviation,LMNGID as Dna,LLBLNO as SerialNum
                          FROM TAPIALI.LABELGP
                          WHERE trim(LOVED)='{EmployeeNumReport}' and LACNDT||RIGHT('000000'||LACNTM,6) between {dateTime.AddDays(-1).ToString("yyyyMMdd233000")} and {dateTime.ToString("yyyyMMdd232959")} and LACTAN in (3,103)";
            DataTable dataTable = new DataTable();
            dataTable = DBS.executeSelectQueryNoParam(qry);
            return dataTable;
        }


    }
}


