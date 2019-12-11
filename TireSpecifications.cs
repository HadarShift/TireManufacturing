using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Net.Mail;

namespace TireManufacturing
{
    /// <summary>
    /// מחלקת פרטי מפרט
    /// </summary>
    class TireSpecifications:General
    {
        public double Circumference { get; set; } // היקף תוף חגורה
        public string Size { get; set; }//גודל
        public int CatalogNumber8 { get; set; }// 8 מק"ט
        public string FatherProduct { get; set; }//מק"ט תוצרת גמורה-AL-פריט אב
        public string CaracasCatalogNum { get; set; }//מק"ט קרקס -100
        public string GreenCatalogNum { get; set; }//000-מק"ט צמיג ירוק
        public int Model { get; set; }//דגם תבנית
        public double TireWeight { get; set; }//משקל צמיג ירוק
        public double CarcasWeight { get; set; }//משקל כרכס בליברות
        public double CarcasWeightKg { get; set; }//משקל קרקס בקילו
        public double WeightLevelB { get; set; }//משקל שלב ב-חיסור ירוק מקרקס
        public string Pr { get; set; }//חוזק צמיג
        public double Deviation { get; set; }//סטיית משקל
        public int Status { get; set; }//סטטוס מפרט
        public int Unit { get; set; }//מספר מחלקה
        public double Width { get; set; }//רוחב תוף
        public string NameReporter { get; set; }//שם מעדכן
        public string CodeLock { get; set; }//קוד נעילת חישוק
        public double BaseCircumference { get; set; }//היקף תושבת
        public string Ring { get; set; }//שלב ב' טבעת ניפוח
        public string WrappingId { get; set; }//ת.ז ליפוף
        public string Updater { get; set; }//שם עורך
        public int CodeUpdater { get; set; }//קוד עורך
        public string MailUpdater { get; set; }//מייל עורך
        public string Mixture { get; set; }//תערובת
        public string Test { get; set; }//מפרט נסיוני dp
        public int NumBreakers { get; set; }//מספר ברקרים במפרט
        public string[] SubstringsSpecifications { get; set; }//פיצול סטרינג מפרט לכמה סטרינגים
        public List<double> WidthBreakers { get; set; }//רוחב ברקרים


        public string CatalogNumberUsedNow { get; set; }//מק"ט רלוונטי בהתאם לשלב עבודה שלנו



        public TireSpecifications()
        {
            LogWaveClass.LogWave("יצר אובייקט TireSpecifications");
        }

        

        public void GetSpecificationBaseData(string[] SubstringsSpecifications)
        {
            this.SubstringsSpecifications = SubstringsSpecifications;
            DBService DBS = new DBService();
            DataTable dataTable = new DataTable();
            string query =
                $@"SELECT MF2RTH, INSIZ, MFPRD, MFTVDG, MFWT, MFSTT, MFDPT, MFTOFW, INPR, MFUSER, MFCSCD, MFTSHA, MF2RNG , MRKOD, ZMUSCD, MFCDO1, XTMIXD, WOSDADD1 as emailEng ,MFKOD5||MFTST as Test
                   FROM MFRT.MFRTP 
                   LEFT JOIN BPCSDALI.IIMNL01 ON INPROD = MFPRD 
                   LEFT JOIN MFRT.MFRTVP ON MFSZ = MRSZ AND MFNO = MRNO and MFVRN = MRVRN 
                   LEFT JOIN MFRT.ZMFRTP ON ZMCODE = MFCDO1 
                   LEFT JOIN MFRT.XTRUP ON XTCODE = MRKOD 
                   LEFT JOIN ALLTAB.USRMAILP ON ZMUSCD = WOSDDEN 
                   WHERE MFSZ = '{SubstringsSpecifications[0]}' AND MFNO = {SubstringsSpecifications[1]} AND MFVRN = {SubstringsSpecifications[2]} AND MRSUG IN ('SUO','CAP','SUL','BAS') 
                   GROUP BY MF2RTH, MFPRD, MFTVDG, MFWT, MFSTT, MFDPT, MFTOFW, INSIZ, INPR, MFUSER, MFCSCD, MFTSHA, MF2RNG, MRKOD, ZMUSCD, MFCDO1, XTMIXD, WOSDADD1,MFKOD5||MFTST";
            LogWaveClass.LogWave("קבלת נתוני מפרט " + query);
            dataTable = DBS.executeSelectQueryNoParam(query);
            if (!(dataTable is null))
            {
                Circumference = double.Parse(dataTable.Rows[0]["MF2RTH"].ToString());
                Size = dataTable.Rows[0]["INSIZ"].ToString();
                CatalogNumber8 = int.Parse(dataTable.Rows[0]["MFPRD"].ToString());
                Model = int.Parse(dataTable.Rows[0]["MFTVDG"].ToString());
                TireWeight = double.Parse(dataTable.Rows[0]["MFWT"].ToString());
                Status = int.Parse(dataTable.Rows[0]["MFSTT"].ToString());
                Unit = int.Parse(dataTable.Rows[0]["MFSTT"].ToString());
                Width = double.Parse(dataTable.Rows[0]["MFTOFW"].ToString());
                Pr = dataTable.Rows[0]["INPR"].ToString();
                NameReporter = dataTable.Rows[0]["MFUSER"].ToString();
                CodeLock = dataTable.Rows[0]["MFCSCD"].ToString();
                BaseCircumference = double.Parse(dataTable.Rows[0]["MFTSHA"].ToString());
                Ring = dataTable.Rows[0]["MF2RNG"].ToString();
                WrappingId = dataTable.Rows[0]["MRKOD"].ToString().Substring(dataTable.Rows[0]["MRKOD"].ToString().LastIndexOf('.') + 1);
                if (WrappingId.ToLower().Contains('/'))
                    WrappingId = WrappingId.Substring(0, WrappingId.IndexOf("/"));
                Updater = dataTable.Rows[0]["ZMUSCD"].ToString();
                CodeUpdater = int.Parse(dataTable.Rows[0]["MFCDO1"].ToString());
                Mixture = dataTable.Rows[0]["XTMIXD"].ToString();
                MailUpdater = dataTable.Rows[0]["emailEng"].ToString();
                Test = dataTable.Rows[0]["Test"].ToString();
            }
            AddBreakers(SubstringsSpecifications);//מוסיף ברקרים במידה ויש למפרט מסוים
          


        }



        /// <summary>
        /// הוספת ברקרים למפרט במידה ויש
        /// </summary>
        private void AddBreakers(string[] SubstringsSpecifications)
        {
            DBService DBS = new DBService();
            DataTable dataTable = new DataTable();
            string query =
                $@"select MRNUM,MRWDT
                   from MFRT.MFRTVP 
                   where MRSZ = '{SubstringsSpecifications[0]}' AND MRNO = {SubstringsSpecifications[1]} AND MRVRN = {SubstringsSpecifications[2]} AND MRSUG IN ('BRR','BRD','BRB','SBL','SBG') ";
            LogWaveClass.LogWave("הוספת ברקרים למפרט אם יש " + query);
            dataTable = DBS.executeSelectQueryNoParam(query);
            NumBreakers = dataTable.Rows.Count;//מספר ברייקרים לפי מספר הרשומות,מסודר במספור סריאלי
            WidthBreakers = new List<double>();
            for (int i = 0; i < NumBreakers; i++)
            {
                WidthBreakers.Add(double.Parse(dataTable.Rows[i]["MRWDT"].ToString()));//מוסיף לרשימה את רחבי הברקרים בתיאום
            }
            

        }

        /// <summary>
        /// בודק תקינות מפרט ושליפת מק"טים
       ///מעץ מוצר
            /// </summary>
        public void CheckSpecificationAndGetCatalog(string Specification,string WhichCase,int Shift)
        {
            DBService DBS = new DBService();
            DataTable dataTable = new DataTable();
            //בדיקה שלמפרט אין יותר ממספר קטלוגי אחד
            string query = $@"SELECT distinct substring(iprod,1,8),  substr(IDRAW,7,15)
                            FROM bpcsfv30.iiml01
                            WHERE substr(IDRAW,7,15)='{Specification}'  and substring (iclas,1,1) in ('L','M','N')";
            LogWaveClass.LogWave("בדיקת תקינות מפרט-האם יש יותר ממספר קטלוגי אחד " + query);
            dataTable = DBS.executeSelectQueryNoParam(query);
            if(dataTable.Rows.Count>1)
            {
                MessageBox.Show("מפרט לא תקין,נא לפנות לתפי");
                return;
            }
            //שליפת נתונים ממפרט-עץ מוצר
            if(WhichCase=="שלב א")
            {
                query =
           //טוב לשלב א
           $@"SELECT distinct  G.BCHLD as GreenCatalogNum, C.BCHLD as CaracasCatalogNum, round(W.ICSCP1* 2.2046, 2  ) as WeightLevelLibrot,round(W.ICSCP1* 2.2046, 2  ) as WeightCaracasLibrot,round(W.ICSCP1, 2)as WeightCarcasKg
               FROM BPCSFV30.IIMl01 A join BPCSFV30.MBML01 F on IPROD=F.BPROD 
               left join BPCSFV30.MBML01 G on F.BPROD=G.BPROD and (G.BCLAC between'N1' and 'N9' or G.BCLAC between'M1' and 'M9')
               left join BPCSFV30.MBML01 C on G.BCHLD=C.BPROD and C.BCLAC ='L'
               left join BPCSFV30.cicl01 W on trim(C.BCHLD)=trim(W.ICPROD)
               left join  RZPALI.mcovip M on W.ICPROD=M.OPRIT and M.OMACH='{MachineID}'       
               WHERE substr(A.IPROD,1,8)='{CatalogNumber8}' and substr(A.IDRAW,7,15)='{Specification}' and F.BID='BM' and (A.ICLAS between'1D' and '9D' or A.ICLAS between'1R' and '9R') and M.ODATE={DateTime.Now.ToString("1yyMMdd")} ";
            }
           
            ////שונה ב26.11
            ////טוב לשלב ב
            ////query = $@"SELECT distinct F.BPROD, G.BCHLD as GreenNum, C.BCHLD as CaracasNum, round(W.ICSCP1 * 2.2046, 2) as WeightCarcas,round(W.ICSCP1, 2) as WeightCarcasKg
            ////       FROM BPCSFV30.IIMl01 A join BPCSFV30.MBML01 F on IPROD = F.BPROD
            ////       left join BPCSFV30.MBML01 G on F.BPROD = G.BPROD and(G.BCLAC between'N1' and 'N9' or G.BCLAC between'M1' and 'M9')
            ////       left join BPCSFV30.MBML01 C on G.BCHLD = C.BPROD and C.BCLAC = 'L'
            ////       left join BPCSFV30.cicl01 W on trim(G.BCHLD)=trim(W.ICPROD)
            ////       left join  RZPALI.mcovip M on W.ICPROD = M.OPRIT and M.OMACH = '{MachineID}'
            ////       WHERE substr(A.IPROD,1,8)= '{CatalogNumber8}' and substr(A.IDRAW,7,15)= '{Specification}' and F.BID = 'BM' and (A.ICLAS between'1D' and '9D' or A.ICLAS between'1R' and '9R') and M.ODATE ={DateTime.Now.ToString("1yyMMdd")} ";
            else
            {
                query = $@"SELECT distinct OPRIT as GreenCatalogNum, BCHLD as CaracasCatalogNum, round(G.ICSCP1* 2.2046, 2  ) as WeightLevelLibrot,round(C.ICSCP1* 2.2046, 2  ) as WeightCaracasLibrot,round(C.ICSCP1, 2)as WeightCarcasKg
               FROM RZPALI.mcovip 
               left join BPCSFV30.MBML01   on OPRIT=BPROD and BCLAC ='L'
               left join BPCSFV30.cicl01 G on OPRIT=G.ICPROD and G.ICFAC = 'F1'
               left join BPCSFV30.cicl01 C on BCHLD=C.ICPROD and C.ICFAC = 'F1'
	           left join BPCSFV30.IIMl01 A on oprit=iprod
               WHERE OMACH = '{MachineID}' and substr(OPRIT,1,8)= '{CatalogNumber8}'  and substr(A.IDRAW,7,15)='{Specification}' and odate={DateTime.Now.ToString("1yyMMdd")} and oshift='{Shift}' and bchld is not null ";
            }
           
      
            
            LogWaveClass.LogWave("שליפת נתונים מפרט מעץ מוצר " + query);
            dataTable = DBS.executeSelectQueryNoParam(query);
            if (dataTable is null)
            {
                MessageBox.Show("מפרט לא תקין,נא לפנות לתפי");
                return;
            }
            else if (dataTable.Rows.Count >= 1)
            {

                //FatherProduct = dataTable.Rows[0]["BPROD"].ToString();
                FatherProduct = "";
                GreenCatalogNum = dataTable.Rows[0]["GreenCatalogNum"].ToString();
                CaracasCatalogNum = dataTable.Rows[0]["CaracasCatalogNum"].ToString();
                if (!string.IsNullOrEmpty(dataTable.Rows[0]["WeightCaracasLibrot"].ToString() as string))
                {
                    CarcasWeight = double.Parse(dataTable.Rows[0]["WeightCaracasLibrot"].ToString());//משקל קרקס
                    CarcasWeightKg = double.Parse(dataTable.Rows[0]["WeightCarcasKg"].ToString());
                }
            }
       

            WeightLevelB = TireWeight - CarcasWeight;//חישוב משקל שלב ב'
            CheckIfDp(Specification);//בודק אם מדובר במפרט נסיוני
        }

        /// <summary>
        /// בודק אם מדובר במפרט נסיוני
        /// </summary>
        private void CheckIfDp(string Specification)
        {
            if (Test == "DP")//אם dp מדובר במפרט נסיוני וישלח מייל לעורך 1
            {
                LogWaveClass.LogWave("שליחת מייל עבור מפרט נסיוני");
                MailMessage mail = new MailMessage();
                SmtpClient SmtpServer = new SmtpClient("smtp.gmail.com");
                mail.From = new MailAddress("Test_Specification@atgtire.com");
                mail.To.Add(MailUpdater);//"hshiftan@atgtire.com"
                mail.Subject = $@"מפרט נסיוני-{Specification} ";
                mail.Body = $@"מפרט נסיוני {Specification} התבצע היום {DateTime.Now} 
                               המפרט שייך למכונה {MachineID} ";

                //if (mail_object.File_Name != null)
                //{
                //    System.Net.Mail.Attachment attachment;
                //    attachment = new System.Net.Mail.Attachment(mail_object.File_Name);
                //    mail.Attachments.Add(attachment);
                //}

                SmtpClient client = new SmtpClient();
                client.Host = "almail";// ServerIP;
                client.Send(mail);
                client.Dispose();
            }
        }
    
    }
}
