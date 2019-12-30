using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TireManufacturing
{

    /// <summary>
    /// טופס בקרה עצמית
    /// </summary>
    public partial class SelfInspection : Form
    {
        public string LevelWork { get; set; }
        public string CatalogNum { get; set; }
        public string ComponentBarCode { get; set; }//בר קוד מרכיב
        public int SerialNum { get; set; }
        public int PreviousEmployee { get; set; }//פעם הבאה שיפתח את הבקרה עצמית יבדוק אם זה אותו עובד
        public string PreviousSpecification { get; set; }//פעם הבאה שיפתח את הבקרה עצמית יבדוק אם זה אותו מפרט
        public DataTable TireComponenetsdataTable { get; set; }//טבלת מרכיבי צמיג
        public DataTable ForDataGrid { get; set; }//טבלת מרכיבי צמיג מקוצרת עבור הדטה גריד
        public int Shift { get; set; }
        public string Department { get; set; }

        List<int> Managers = new List<int>();//רשימת מנהלים מאשרים
        public enum ColorSelfInspection { red,yellow,green};//צבע נוכחי עבור סטטוס בקרה עצמית
        public ColorSelfInspection colorSelfInspection { get; set; }

        General GeneralDetails = new General();

        WorkPlan workPlan = new WorkPlan();

        List<string> MachineCheck = new List<string>();//בקרת ציוד מכונה

        List<string> Safety = new List<string>();//בקרת בטיחות

        DataTable DatatableSql;//טבלת שליפה מתוך sql server
        //DataTable CheckBoxChecked;//לדעת איזה צ'ק בוקס סומנו על מנת שנשמור בדטה בייס 
        Dictionary<int,string> CellsPaintYellow;
        List<YellowInSelfInspection> yellowInSelfInspections;//רשימה לדעת איזה תאים לצבוע כי חייבים למלא רוחב ואורך בדטה גריד
        bool SameEmpoloyeeOrSpecification=true;//בודק אם זה אותו עובד ביצע את טופס הבקרה-אם כן רשאי לשחזר ולשמור את מה שעשה אם לא אז עושה הכל מחדש
        bool ManagerConfirm=false;//בדיקת אישור מנהל
        bool IfUpdatedTable = false;//לבדוק אם צריך לעשות insert or update לרשומות בsql
        bool IfUpdateCheckbox = false;//לבדוק אם צריך לעשות insert or delete לרשומות בsql
        bool IfUpdateManger = false;//בדיקה אם הוכנס אישור מנהל בעבר
        double PreviousValue;//ערך קודם בתא
        DBService DBSs;

        SqlParameter MachineID;
        SqlParameter DepartmentID;
        SqlParameter Spec_No;
        SqlParameter Emp_No;
        SqlParameter Building_Stage;

        public SelfInspection(string CatalogNum, string LevelWork,string Specification, List<int> Managers,int EmpolyeeId,int Shift,int PreviousEmployeeSelfInspection,string PreviousSpecificationSelfInspection, string AS400User, string AS400Pass )
        {
            try
            {
                LogWaveClass.LogWave("עולה חלון בקרה עצמית");
                InitializeComponent();
                DBSs  = new DBService();
                colorSelfInspection = ColorSelfInspection.red;//בהתחלה סטטוס אדום
                this.LevelWork = LevelWork;
                this.Shift = Shift;
                this.Department = Department;
                if (LevelWork == "שלב א")
                    lbl_ConstForQA.Text = "QA-431/1";
                else
                    lbl_ConstForQA.Text = "QA-432/1";
                this.CatalogNum = CatalogNum;
                GeneralDetails.Specifications = Specification;
                this.Managers = Managers;
                GeneralDetails.EmployeeId = EmpolyeeId;

                //עבור פרמטרי שמירה לאסקיואל סרבר
                MachineID = new SqlParameter("a", GeneralDetails.MachineID);
                DepartmentID = new SqlParameter("b", GeneralDetails.DepratmentId);
                Spec_No = new SqlParameter("c", GeneralDetails.Specifications);
                Emp_No = new SqlParameter("d", GeneralDetails.EmployeeId);
                Building_Stage = new SqlParameter("e", LevelWork);//or LevelWork next

                //DbServiceSQL sqlcombo = /*new*/ DbServiceSQL();
                //שליפה אם המנהל אישר כבר

                //if ((EmpolyeeId!=PreviousEmployeeSelfInspection && PreviousEmployeeSelfInspection != 0) ||(Specification!= PreviousSpecificationSelfInspection && PreviousSpecificationSelfInspection != null))//התחלף מספר עובד או מפרט
                //{
                //    SameEmpoloyeeOrSpecification = false;
                //}
                lbl_upperline.SendToBack();
                lbl_lowerline.SendToBack();

                lbl_catalognum.Text = CatalogNum;
                lbl_date.Text = DateTime.Now.ToString();
                lbl_machine.Text = GeneralDetails.MachineID;
                lbl_shift.Text = Shift.ToString();
                CheckWhichEmployee();
                TireComponenets();//שליפת מרכיבי צמיג
                CheckColorStatus();
            }
          

            catch(Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }


        /// <summary>
        /// בודק סטטוס טופס בקרה-צהוב הכל נבדק בלי אישור מנהל או ירוק הכל נבדק. נועד במקרה שטוענים שוב מפרט
        /// </summary>
        private void CheckColorStatus()
        {
            //DbServiceSQL sqlcombo = new DbServiceSQL();
            ////שליפה אם המנהל אישר כבר
            //string qry = $@"SELECT m.Manager_ID,m.Manager_Remark
            //       FROM tbl_Mach_Doc_Manager m
            //       WHERE m.Spec_No='{GeneralDetails.Specifications}'";
            //DataTable MangerConfirmTBl = new DataTable();
            //MangerConfirmTBl = sqlcombo.executeSelectQueryNoParam(qry);
            //if (MangerConfirmTBl.Rows.Count != 0)
            //{
            //    IfUpdateManger = true;
            //    txt_Manager.Text = MangerConfirmTBl.Rows[0]["Manager_ID"].ToString();
            //    txt_comment.Text = MangerConfirmTBl.Rows[0]["Manager_Remark"].ToString();
            //}


            ManagerConfirm = CheckManger(txt_Manager.Text);//בדיקה אם מנהל אישר

            bool AllChecked = true;//בודק אם הכל סומן לפני מעבר צבע סטטוס לצהוב

            //בדיקה אם לצבוע צבע טופס לצהוב
            //לולאה אם כל הסידורי מלא
            for (int i = 0; i < dataGridViewTireComponent.RowCount; i++)
            {
                if (string.IsNullOrEmpty(dataGridViewTireComponent.Rows[i].Cells["סידורי"].Value.ToString() as string) || dataGridViewTireComponent.Rows[i].Cells["סידורי"].Value.ToString()=="0")
                {
                    AllChecked = false;
                }
            }

            // אם כל הסידורי מלא בודק אם סומן כל הצ'ק בוקסים
            foreach (var control in this.Controls)
            {
                if (control is CheckBox)
                {
                    if (!((CheckBox)control).Checked)
                    {
                        AllChecked = false;
                    }
                }
            }

            //בדיקה אם כל התאים שצבועים בצהוב מולאו 
            for (int i = 0; i < yellowInSelfInspections.Count; i++)//עובר על כל התאים שצריך לצבוע בצהוב
            {
                if (dataGridViewTireComponent.Rows[yellowInSelfInspections[i].Row].Cells[yellowInSelfInspections[i].Column].Value.ToString() == "0")
                {
                    AllChecked = false;
                }
            }

            LogWaveClass.LogWave("סיים בדיקות בקרה עצמית לפני שמירה לAS400");

            if (AllChecked)
            {
                if ((txt_Manager.Text == "" || ManagerConfirm == false))//רק אם הכל סומן בבדיקה ומנהל לא אישר ייצבע לצהוב
                {
                    lbl_catalognum.BackColor = Color.Yellow;
                    colorSelfInspection = ColorSelfInspection.yellow;
                }
                else if (ManagerConfirm)
                {
                    lbl_catalognum.BackColor = Color.GreenYellow;
                    colorSelfInspection = ColorSelfInspection.green;
                }
            }
        }

        public void CheckWhichEmployee()
        {
            DbServiceSQL sqlcombo = new DbServiceSQL();
            //שליפה אם המנהל אישר כבר
            string qry = $@"SELECT *
                   FROM tbl_Mach_Doc_Manager m
                   WHERE m.Spec_No='{GeneralDetails.Specifications}'";
            LogWaveClass.LogWave(qry);
            DataTable MangerConfirmTBl = new DataTable();
            MangerConfirmTBl = sqlcombo.executeSelectQueryNoParam(qry);
            if (MangerConfirmTBl.Rows.Count != 0)
            {
                if (MangerConfirmTBl.Rows[0]["Emp_No"].ToString() != Emp_No.Value.ToString())//אם מספר עובד שונה מהמספר ששמור במערכת נצטרך שוב בקרה עצמית
                    SameEmpoloyeeOrSpecification = false;

                if (!string.IsNullOrEmpty(MangerConfirmTBl.Rows[0]["Manager_ID"].ToString()))//אם מספר מנהל לא ריק ברשומה
                {
                    IfUpdateManger = true;
                    txt_Manager.Text = MangerConfirmTBl.Rows[0]["Manager_ID"].ToString();
                    txt_Manager.PasswordChar = '*';
                    txt_comment.Text = MangerConfirmTBl.Rows[0]["Manager_Remark"].ToString();
                }
            }
            else//יצור רשומה שיש בה רק את העובד בינתיים
            {
                string ForInsert = $@"INSERT tbl_Mach_Doc_Manager
                               VALUES (?,?,?,?,?,?,?)";

                SqlParameter Manager_ID = new SqlParameter("M", txt_Manager.Text);
                SqlParameter Manager_Remark;
                if (!string.IsNullOrEmpty(txt_comment.Text))
                    Manager_Remark = new SqlParameter("r", txt_comment.Text);
                else
                    Manager_Remark = new SqlParameter("r", DBNull.Value);
                LogWaveClass.LogWave(ForInsert);
                sqlcombo.ExecuteQuery(ForInsert, CommandType.Text, MachineID, DepartmentID, Spec_No, Emp_No, Building_Stage, Manager_ID, Manager_Remark);
            }
        }

        private void SelfInspection_Load(object sender, EventArgs e)
        {
            try
            {
                GetControlList();
                CreateControlCheckBox();
                CheckStatusInspection();//שולף בדיקות מהדטה בייס שבוצעו כבר בעבר עבור אותו מפרט
                CellPainting();
            }

            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                LogWaveClass.ErrorLogWave("Error: " + ex.Message);
            }

        }


        /// <summary>
        /// שליפת מרכיבי צמיג 
        /// </summary>
        private void TireComponenets()
        {
            LogWaveClass.LogWave("TireComponenets start");
            TireComponenetsdataTable = new DataTable();
            //string query =
            //    $@"SELECT bvlnum,bseq,bprod,bchld,IDESC,INTEGER(round(bqreq,0)) as bqreq,bcums,a.ICLAS,ICDES,ICALP2 As sidra,trim(ICALC2) As SHKILA,case when iums='GL' then ' ' else b.ICPPGL end as Param,
            //       case when substring(b.ICPPGL,1,1)=1 then substring(b.ICPPGL,1,1)  else ' ' end as lenght
            //       ,case when substring(b.ICPPGL,1,2)=1 then substring(b.ICPPGL,2,1)  else ' ' end as width,
            //       case when substring(b.ICPPGL,1,3)=1 then substring(b.ICPPGL,3,1) else ' ' end as WidthCombined
            //       FROM TAPIALI.LABMBMP left outer join BPCSFV30.IIML01 a on (bchld = IPROD) left outer join BPCSFV30.IICL01 b on (a.ICLAS = b.ICLAS) 
            //       WHERE BVLNUM = (select max(bvlnum) from TAPIALI.LABMBMP where BPROD = '{CatalogNum}' And BQREQ <> 0) And BQREQ <> 0 and trim(b.ICTAX)='1' 
            //       ORDER BY bseq";
            string query = $@"select '' as bvlnum,bseq,bprod,bchld,trim(IDESC) as IDESC,round(bqreq,1) as bqreq,IUMS,a.ICLAS,trim(ICDES) as ICDES,ICALP2 As sidra,trim(ICALC2) As SHKILA,case when iums='GL' then ' ' else b.ICPPGL end as Param,
                              case when substring(b.ICPPGL,1,1)=1 then substring(b.ICPPGL,1,1)  else ' ' end as lenght
                             ,case when substring(b.ICPPGL,1,2)=1 then substring(b.ICPPGL,2,1)  else ' ' end as width,
                             case when substring(b.ICPPGL,1,3)=1 then substring(b.ICPPGL,3,1) else ' ' end as WidthCombined
                             from BPCSFV30.MBML01
                                 left outer join BPCSFV30.IIML01 a on (bchld = IPROD)
                                 left outer join BPCSFV30.IICL01 b on (a.ICLAS = b.ICLAS)
                             Where BPROD = '{CatalogNum}' And BQREQ <> 0 and trim(b.ICTAX)='1'
                             order by bseq";
            LogWaveClass.LogWave("TireComponenets "+ query);
            TireComponenetsdataTable = DBSs.executeSelectQueryNoParam(query);
            
            TireComponenetsdataTable.Columns.Add("מס", typeof(System.Int32));//עמודת מספר רץ
            TireComponenetsdataTable.Columns.Add("סידורי", typeof(System.Int32));//מספר סידורי
            int num = 1;//
            for (int i = 0; i < TireComponenetsdataTable.Rows.Count; i++)
            {
                TireComponenetsdataTable.Rows[i]["מס"] = num;
                num++;
                string s = TireComponenetsdataTable.Rows[i]["Param"].ToString();                
            }

            //CellsPaintYellow = new Dictionary<int, string>();
            yellowInSelfInspections = new List<YellowInSelfInspection>();
            YellowInSelfInspection Yellow;
            //איזה תאים לצבוע בצהוב על מנת לחייב מילוי אורך/רוחב
            for (int i = 0; i < TireComponenetsdataTable.Rows.Count; i++)
            {
                if (!string.IsNullOrEmpty(TireComponenetsdataTable.Rows[i]["lenght"].ToString() as string))//אם קיים מספר 1 זה אומר שחייבים למלא את התא
                {
                    Yellow = new YellowInSelfInspection(i, "lenght");
                    yellowInSelfInspections.Add(Yellow);
                    //CellsPaintYellow.Add(i, "lenght");//יצבע את התא הנוכחי
                    if (TireComponenetsdataTable.Rows[i]["lenght"].ToString() == "1")
                        TireComponenetsdataTable.Rows[i]["lenght"] = "0";
                }
                if (!string.IsNullOrEmpty(TireComponenetsdataTable.Rows[i]["width"].ToString() as string))
                {
                    Yellow = new YellowInSelfInspection(i, "width");
                    yellowInSelfInspections.Add(Yellow);
                    //CellsPaintYellow.Add(i, "width");
                    if (TireComponenetsdataTable.Rows[i]["width"].ToString() == "1")
                        TireComponenetsdataTable.Rows[i]["width"] = "0";
                }
                if (!string.IsNullOrEmpty(TireComponenetsdataTable.Rows[i]["WidthCombined"].ToString() as string))
                {
                    Yellow = new YellowInSelfInspection(i, "WidthCombined");
                    yellowInSelfInspections.Add(Yellow);
                    //CellsPaintYellow.Add(i, "WidthCombined");
                    if (TireComponenetsdataTable.Rows[i]["WidthCombined"].ToString() == "1")
                        TireComponenetsdataTable.Rows[i]["WidthCombined"] = "0";
                }
            }



            DataView view = new DataView(TireComponenetsdataTable);
            ForDataGrid = view.ToTable(false,"מס", "bchld", "IDESC", "ICDES", "bqreq", "IUMS", "סידורי", "lenght", "width", "WidthCombined");
            if(!SameEmpoloyeeOrSpecification)//אם עובד אחר עושה את הטופס מוחק מאסקיואל את נתוני העבר
            {
                DeleteComoponentInserted();
            }          
            CheckComoponentInserted();//שליפה אם כברנרשמו נתונים בטבלה בעבר
           

            LogWaveClass.LogWave("שחזור נתונים מבקרה עצמית");

      
            ForDataGrid.Columns.Add("דיווח פסולות");
            dataGridViewTireComponent.DataSource = ForDataGrid;
            
            dataGridViewTireComponent.Columns["מס"].ReadOnly = true;
            dataGridViewTireComponent.Columns["bchld"].ReadOnly = true;
            dataGridViewTireComponent.Columns["IDESC"].ReadOnly = true;
            dataGridViewTireComponent.Columns["ICDES"].ReadOnly = true;
            dataGridViewTireComponent.Columns["bqreq"].ReadOnly = true;
            dataGridViewTireComponent.Columns["IUMS"].ReadOnly = true;
            dataGridViewTireComponent.Columns["סידורי"].ReadOnly = true;
            dataGridViewTireComponent.Columns["width"].ReadOnly = true;
            dataGridViewTireComponent.Columns["lenght"].ReadOnly = true;
            dataGridViewTireComponent.Columns["WidthCombined"].ReadOnly = true;


            //dataGridViewTireComponent.Columns["IDESC"].Width = (int)(dataGridViewTireComponent.Width * 0.15);
            //dataGridViewTireComponent.Columns["bchld"].Width = (int)(dataGridViewTireComponent.Width * 0.15);
            //dataGridViewTireComponent.Columns["ICDES"].Width = (int)(dataGridViewTireComponent.Width * 0.15);
            //dataGridViewTireComponent.Columns["מס"].Width = (int)(dataGridViewTireComponent.Width * 0.07);
            //dataGridViewTireComponent.Columns["bqreq"].Width = (int)(dataGridViewTireComponent.Width * 0.07);
            //dataGridViewTireComponent.Columns["IUMS"].Width = (int)(dataGridViewTireComponent.Width * 0.07);
            //dataGridViewTireComponent.Columns["lenght"].Width = (int)(dataGridViewTireComponent.Width * 0.07);
            //dataGridViewTireComponent.Columns["width"].Width = (int)(dataGridViewTireComponent.Width * 0.07);
            //dataGridViewTireComponent.Columns["WidthCombined"].Width = (int)(dataGridViewTireComponent.Width * 0.07);
            //dataGridViewTireComponent.Columns["סידורי"].Width = (int)(dataGridViewTireComponent.Width * 0.13);



            dataGridViewTireComponent.Columns["bchld"].HeaderText = "מק\"ט רכיב";
            dataGridViewTireComponent.Columns["bchld"].Visible = false;
            dataGridViewTireComponent.Columns["IDESC"].HeaderText = "תאור";
            //DataGridViewColumn column = dataGridViewTireComponent.Columns[0];
            //column.Width = 150;
            dataGridViewTireComponent.Columns["ICDES"].HeaderText = "תאור קבוצת פריט";
            dataGridViewTireComponent.Columns["bqreq"].HeaderText = "כמות";
            dataGridViewTireComponent.Columns["IUMS"].HeaderText = "יח מידה";
            dataGridViewTireComponent.Columns["lenght"].HeaderText = "אורך נמדד";
            dataGridViewTireComponent.Columns["width"].HeaderText = "רוחב נמדד";
            dataGridViewTireComponent.Columns["WidthCombined"].HeaderText = "רוחב משולב נמדד";

            LogWaveClass.LogWave("TireComponenets end");
        }

  



        /// <summary>
        /// רשימת בקרת ציוד מכונה בהתאם לשלב עבודה
        /// </summary>
        private void GetControlList()
        {
            LogWaveClass.LogWave("selfInspection.GetControlList start");
            DbServiceSQL dbServiceSQL = new DbServiceSQL();
            DatatableSql = new DataTable();
            string query = "";
            if (LevelWork == "שלב א")
                query = $@"SELECT Section,Mac_Key_ID,Mac_Key_Heb
                         FROM tbl_Mach_Doc_Set
                         WHERE Stage_A=1";
            else if (LevelWork == "שלב ב")
                query = $@"SELECT Section,Mac_Key_ID,Mac_Key_Heb
                        FROM tbl_Mach_Doc_Set
                        WHERE Stage_B=1";
            else if (LevelWork == "צמיג מלא")
                query = $@"SELECT Section,Mac_Key_ID,Mac_Key_Heb
                        FROM tbl_Mach_Doc_Set
                        WHERE Complete_Tire=1";

            DatatableSql = dbServiceSQL.executeSelectQueryNoParam(query);
            for (int i = 0; i < DatatableSql.Rows.Count; i++)
            {
                if (DatatableSql.Rows[i]["Section"].ToString() == "Safety")
                    Safety.Add(DatatableSql.Rows[i]["Mac_Key_Heb"].ToString());                    
                else if (DatatableSql.Rows[i]["Section"].ToString() == "MachineCheck")
                    MachineCheck.Add(DatatableSql.Rows[i]["Mac_Key_Heb"].ToString());
            }
            LogWaveClass.LogWave("selfInspection.GetControlList end");
        }

        /// <summary>
        /// יוצר רשימות בקרה מיד לאחר קבלת רשימות מהדטה בייס
        /// </summary>
        private void CreateControlCheckBox()
        {
            LogWaveClass.LogWave("selfInspection.CreateControlCheckBox start");
            CheckBox box;
            int y= (int) (this.Size.Height *0.17);
            int x= (int)( this.Size.Width *0.8);
            int maxLenghtWord = 0;
            int LenghtWord;
            for (int i = 0; i < MachineCheck.Count; i++)//לספור כמה אותיות במילה-בשביל מיקום מדויק עבור צק בוקס
            {
                if (MachineCheck[i].Length > maxLenghtWord)
                    maxLenghtWord = MachineCheck[i].Length;
            }

                for (int i = 0; i < MachineCheck.Count; i++)
            {
                box = new CheckBox();
                //box.RightToLeft = new RightToLeft();
                //box.RightToLeft = RightToLeft.Yes;
                box.Tag = i.ToString();
                box.Text = MachineCheck[i]; //שם mach_key_heb       
                box.Font = new Font("arial", 12F, FontStyle.Bold);
                box.AutoSize = true;
                LenghtWord = box.Text.Length;
                //box.Location = gnew Point(x+(maxLenghtWord*5)-(LenghtWord*5), y);//בגלל רייט טו לפרט שיחקתי עם המיקומים שיהיו באותו קו
                box.Location = new Point(x, y);
                y = y + 25;
                this.Controls.Add(box);
                box.BringToFront();
            }

            maxLenghtWord = 0;
            for (int i = 0; i < Safety.Count; i++)//לספור כמה אותיות במילה-בשביל מיקום מדויק עבור צק בוקס
            {
                if (Safety[i].Length > maxLenghtWord)
                    maxLenghtWord = Safety[i].Length;
            }

            y = (int)(this.Size.Height * 0.17);
            x = (int)(this.Size.Width * 0.47);
            for (int i = 0; i < Safety.Count; i++)
            {
                box = new CheckBox();
                //box.RightToLeft = new RightToLeft();
                //box.RightToLeft = RightToLeft.Yes;
                box.Tag = i.ToString();
                box.Text = Safety[i];
                box.Font = new Font("arial", 12F, FontStyle.Bold);
                box.AutoSize = true;
                LenghtWord = box.Text.Length;
                //box.Location = new Point(x + (maxLenghtWord * 5) - (LenghtWord * 5), y);
                box.Location = new Point(x, y);
                y = y + 25;
                this.Controls.Add(box);
                box.BringToFront();
            }
            LogWaveClass.LogWave("selfInspection.CreateControlCheckBox end");
        }




        /// <summary>
        /// עיצובי קווים
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void panel1_Paint(object sender, PaintEventArgs e)
        {
            Graphics graphics = panel1.CreateGraphics();
            //Pen border=new Pen()
        }

        private void txt_barcode_KeyPress(object sender, KeyPressEventArgs e)
        {
            try
            {
                LogWaveClass.LogWave("selfInspection-txt_barcode_KeyPress start");
                if (e.KeyChar == (char)13)
                {
                    if (txt_barcode.Text == null)
                    {
                        MessageBox.Show(
                            "Specification Not Loaded",
                            "Alliance Tire Manufacturing",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Error,
                            MessageBoxDefaultButton.Button1,
                            (MessageBoxOptions)0x40000);
                        this.txt_barcode.Text = string.Empty;
                        return;
                    }
                    bool Check = CheckStickerSerialNumBeoforeContinue();//בדיקה שמספר סריאלי הוכנס כמו שצריך
                    if (Check)
                    {
                        InsertSerial();
                    }
                    txt_barcode.Text = "";
                    txt_barcode.Focus();
                }
                LogWaveClass.LogWave("selfInspection-txt_barcode_KeyPress end");
            }

            catch(Exception ex)
            {
                MessageBox.Show(ex.Message);
                LogWaveClass.ErrorLogWave("Error: " + ex.Message);
            }
        }



        /// <summary>
        /// אחרי קריאת ברקוד מרכיב מכניס מספר סריאלי לשורה המתאימה בדטה גריד
        /// </summary>
        private void InsertSerial()
        {
            bool CheckMixures = true;//משתנה בדיקת תערובות
            LogWaveClass.LogWave("selfInspection.InsertSerial start");
            for (int i = 0; i < dataGridViewTireComponent.RowCount; i++)
            {
                if (ComponentBarCode == dataGridViewTireComponent.Rows[i].Cells["bchld"].Value.ToString().Split(' ').First())//השוואת מקטים
                {
                    if (TireComponenetsdataTable.Rows[i]["ICLAS"].ToString().Substring(0, 1) == "B")  //אם מדובר בתערובת יש לערוך מספר בדיקות לפני הכנסת סריאלי
                    {
                        CheckMixures=CheckBMixture();
                    }
                    if (CheckMixures)//אם היו תערובות אז כולן מאושרות לפני הכנסת מספר סריאלי
                    {
                        dataGridViewTireComponent.Rows[i].Cells["סידורי"].Value = SerialNum;
                        TireComponenetsdataTable.Rows[i]["סידורי"] = SerialNum;//26.8.19 עבור שאילתת שמירת מרכיבים                    
                    }
                }
            }
            LogWaveClass.LogWave("selfInspection.InsertSerial start");
        }



        /// <summary>
        /// בדיקת מרכיב תערובת
        private bool CheckBMixture()
        {
            LogWaveClass.LogWave("selfInspection.CheckBMixture start");
            DataTable CheckBTable = new DataTable();
            string qry = $@"SELECT LNID,LNSTT,LNEXDT from BPCSFALI.ILNF where LNPROD='{ComponentBarCode}' and LNLOT='{SerialNum}'";
            CheckBTable = DBSs.executeSelectQueryNoParam(qry);
            if(CheckBTable.Rows.Count==0)
            {
                MessageBox.Show("שגיאה: משטח תערובת לא קיים במערכת");
                return false;
            }
            if(CheckBTable.Rows[0]["LNID"].ToString()== "LZ")
            {
                MessageBox.Show("שגיאה: משטח תערובת לא עבר לייצור במערכת ");
                return false;
            }
            if (CheckBTable.Rows[0]["LNSTT"].ToString() != "A")
            {
                MessageBox.Show("שגיאה: משטח תערובת לא משוחרר ");
                return false;
            }
            if (int.Parse(CheckBTable.Rows[0]["LNEXDT"].ToString())< int.Parse(DateTime.Now.ToString("yyyyMMdd")))
            {
                MessageBox.Show("שגיאה: משטח תערובת פג תוקף ");
                return false;
            }
            LogWaveClass.LogWave("selfInspection.CheckBMixture end");
            return true;

        }

        /// <summary>
        /// בודק אם הוקלד מספר מנהל קיים
        /// </summary>
        private bool CheckManger(string text)
        {
            bool Check = false;
            bool isNumeric = int.TryParse(text, out int n);
            if (isNumeric)
            {
                if (Managers.Contains(n))//מתוך טבלת מנהלים
                    Check = true;
                else
                    MessageBox.Show("מספר מנהל לא קיים", "הודעת שגיאה", MessageBoxButtons.OK, MessageBoxIcon.Exclamation, MessageBoxDefaultButton.Button1);
            }
            return Check;
        }

        /// <summary>
        /// כפתור שמירת נתונים
        /// </summary>
        private void button1_Click(object sender, EventArgs e)
        {
            try
            {
                LogWaveClass.LogWave("לחץ על כפתור שמירת נתונים בבקרה עצמית");
                SaveDataSql();//שמירת נתונים עד עכשיו
                LogWaveClass.LogWave("סיים לשמור נתוני SQL בבקרה עצמית");
                bool AllChecked = true;//בודק אם הכל סומן לפני מעבר צבע סטטוס לצהוב

                //בדיקה אם לצבוע צבע טופס לצהוב
                //לולאה אם כל הסידורי מלא
                for (int i = 0; i < dataGridViewTireComponent.RowCount; i++)
                {
                    if (string.IsNullOrEmpty(dataGridViewTireComponent.Rows[i].Cells["סידורי"].Value.ToString() as string))
                    {
                        AllChecked = false;
                    }
                }

                // אם כל הסידורי מלא בודק אם סומן כל הצ'ק בוקסים
                foreach (var control in this.Controls)
                {
                    if (control is CheckBox)
                    {
                        if (!((CheckBox)control).Checked)
                        {
                            AllChecked = false;
                        }
                    }
                }

                //בדיקה אם כל התאים שצבועים בצהוב מולאו 
                for (int i = 0; i < yellowInSelfInspections.Count; i++)//עובר על כל התאים שצריך לצבוע בצהוב
                {
                    if (dataGridViewTireComponent.Rows[yellowInSelfInspections[i].Row].Cells[yellowInSelfInspections[i].Column].Value.ToString() == "0")
                    {
                        AllChecked = false;
                    }
                }

                LogWaveClass.LogWave("סיים בדיקות בקרה עצמית לפני שמירה לAS400");

                if (AllChecked == true)//אם הכל סומן בבדיקה יכנס לפה ויבדוק אם ייצבע בירוק או צהוב
                {
                    //שמירת מרכיבים לדטה בייס
                    SaveToAs400();
                    LogWaveClass.LogWave("אחרי שמירת נתונים ל AS400");

                    if ((txt_Manager.Text == "" || ManagerConfirm == false))//רק אם הכל סומן בבדיקה ומנהל לא אישר ייצבע לצהוב
                    {
                        lbl_catalognum.BackColor = Color.Yellow;
                        colorSelfInspection = ColorSelfInspection.yellow;
                    }
                    else if (ManagerConfirm)
                    {
                        lbl_catalognum.BackColor = Color.GreenYellow;
                        colorSelfInspection = ColorSelfInspection.green;
                    }
                }
                else
                {
                    MessageBox.Show("לא ביצעת את כל הבדיקות הדרושות", "הודעת שגיאה", MessageBoxButtons.OK, MessageBoxIcon.Exclamation, MessageBoxDefaultButton.Button1);
                }
                //SaveStatusSqlServer(colorSelfInspection);
                this.Close();
                PreviousEmployee = GeneralDetails.EmployeeId;//פעם הבאה שיפתח בקרה עצמית יבדוק אם זה אותו עובד
                PreviousSpecification = GeneralDetails.Specifications;
            }
           
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message);
                LogWaveClass.ErrorLogWave("Error: " + ex.Message);
            }
       
        }
  

        /// <summary>
        /// שמירת מרכיבים לדטה בייס רק במידה והטופס צהוב או ירוק
        /// </summary>
        private void SaveToAs400()
        {
            try
            {
                string Date = DateTime.Now.ToString("yyyyMMdd"), ManagerId = "";
                double Lenght = 0, Width = 0, WidthCombined = 0;
                if (Shift == 1)//אם המשמרת חוצה יום אז לדאוג שאחרי 12 בלילה מדווח על יום לפני
                {
                    if (DateTime.Now.Hour < 23 || (DateTime.Now.Hour >= 23 && DateTime.Now.Minute <= 30))//קטן מ11 או בין 11 וחצי ל12
                        Date = DateTime.Now.AddDays(-1).ToString("yyyyMMdd");
                }
                DataTable dataTable = new DataTable();
                //בודק אם קיימת רשומה בטבלת הצ'ק בוקס כי יש שם רשומה אחת בלבד לכל טופס בקרה
                string qry = $@"SELECT *
                           FROM TAPIALI.BLDCHKH 
                           WHERE  BHDTE={Date} and BHSHF='{Shift}' and BHDEP={GeneralDetails.DepratmentId} and BHEMP='000{GeneralDetails.EmployeeId}' and BHMAC='{GeneralDetails.MachineID}'  and BHBLD='{CatalogNum}' ";
                dataTable = DBSs.executeSelectQueryNoParam(qry);
                //יצירת רשומה חדשה 
                if (dataTable.Rows.Count == 0)
                {
                    if (ManagerConfirm)
                        ManagerId = txt_Manager.Text;
                    //עדכון שעשינו בקרה עם כל הצ'ק בוקסים
                    string ForQueryByLevel = "'1','1','1','1','1','1','1','1'";//דיפולט צמיג שלם-מגדיר את ה1 לפי הצ'ק בוקסים בכל שלב עבור שאילתה הבאה 
                    //לפי הצ'ק בוקסים של השלבים-מופיע בSQL SERVER
                    switch (LevelWork)
                    {
                        case "שלב א":
                            ForQueryByLevel = "'0','1','1','1','1','1','1','1'";
                            break;

                        case "שלב ב":
                            ForQueryByLevel = "'1','0','0','0','0','0','1','1'";
                            break;
                    }
                    qry = $@"INSERT into TAPIALI.BLDCHKH values({Date},{DateTime.Now.ToString("HHmmss")},'{Shift}',{GeneralDetails.DepratmentId},'{GeneralDetails.MachineID}','000{GeneralDetails.EmployeeId}','{CatalogNum}',{ForQueryByLevel},'{ManagerId}','{txt_comment.Text}')";
                    DBSs.executeInsertQuery(qry);
                    LogWaveClass.LogWave("אחרי שאילתת בקרה עם צ'ק בוקסים");
                    //הכנסה של מדידות רוחב/אורך של רשומות בנים לטבלה השניה
                    for (int i = 0; i < dataGridViewTireComponent.Rows.Count; i++)
                    {
                        Lenght = 0;
                        Width = 0;
                        WidthCombined = 0;
                        if (!string.IsNullOrEmpty(dataGridViewTireComponent.Rows[i].Cells["lenght"].Value.ToString()))
                            double.TryParse(dataGridViewTireComponent.Rows[i].Cells["lenght"].Value.ToString(), out Lenght);
                        if (!string.IsNullOrEmpty(dataGridViewTireComponent.Rows[i].Cells["width"].Value.ToString()))
                            double.TryParse(dataGridViewTireComponent.Rows[i].Cells["width"].Value.ToString(), out Width);
                        if (!string.IsNullOrEmpty(dataGridViewTireComponent.Rows[i].Cells["WidthCombined"].Value.ToString()))
                            double.TryParse(dataGridViewTireComponent.Rows[i].Cells["WidthCombined"].Value.ToString(), out WidthCombined);
                        qry = $@"INSERT into TAPIALI.BLDCHKD  values({Date},{DateTime.Now.ToString("HHmmss")},'{Shift}',{GeneralDetails.DepratmentId},'{GeneralDetails.MachineID}','000{GeneralDetails.EmployeeId}','{CatalogNum}',
                          '{dataGridViewTireComponent.Rows[i].Cells["bchld"].Value.ToString()}',{dataGridViewTireComponent.Rows[i].Cells["סידורי"].Value.ToString()},{Lenght},{Width},{WidthCombined}) ";
                        DBSs.executeInsertQuery(qry);
                    }
                    LogWaveClass.LogWave("הכניס פרטי בנים לדטה בייס");

                }
                //אם קיימת רשומה ומנהל אישר
                if (dataTable.Rows.Count >= 1)
                {
                    if (dataTable.Rows[0]["BHMNG"].ToString() == "" && ManagerConfirm)//אם מספר מנהל ריק
                    {
                        if (ManagerConfirm)
                            ManagerId = txt_Manager.Text;
                        qry = $@"UPDATE TAPIALI.BLDCHKH set BHMNG='{ManagerId}' , BHCMT='{txt_comment.Text}' WHERE  BHDTE={Date} and BHSHF ='{Shift}' and BHDEP={GeneralDetails.DepratmentId} and BHEMP='000{GeneralDetails.EmployeeId}' and BHMAC='{GeneralDetails.MachineID}'  and BHBLD='{CatalogNum}'";
                        DBSs.executeInsertQuery(qry);
                    }
                    LogWaveClass.LogWave(" עדכן רשומה כאשר מנהל אישר");
                }
            }

            catch(Exception ex)
            {
                MessageBox.Show(ex.Message);
                LogWaveClass.ErrorLogWave("Error: " + ex.Message);
            }
   
        }

        /// <summary>
        /// שמירת נתונים בדטה בייס
        /// </summary>
        private void SaveDataSql()
        {

            DbServiceSQL sqlcombo = new DbServiceSQL();
        

            //שמירת בקרת ציוד ובקרת בטיחות  בטבלה tbl_ Mach_Doc_check
            string ForInsert = $@"INSERT tbl_Mach_Doc_Check
                                VALUES (?,?,?,?,?,?,?) ";//

            if (IfUpdateCheckbox == true)//אם סימנו בעבר צ'ק בוקסים נמחק את כל הרשומות ונעדכן חדשים
            {
                string ForDelete = $@"DELETE
                                     FROM tbl_Mach_Doc_Check
                                     WHERE tbl_Mach_Doc_Check.Spec_No='{GeneralDetails.Specifications}' ";
                sqlcombo.ExecuteQuery(ForDelete);
            }

            foreach (var control in this.Controls) // אם כל הסידורי מלא בודק אם סומן כל הצ'ק בוקסים
            {
                if (control is CheckBox)
                {
                    if (((CheckBox)control).Checked)
                    {
                        string Mac_Key_Heb = ((CheckBox)control).Text;//שם הבדיקה
                        for (int i = 0; i < DatatableSql.Rows.Count; i++)//השוואה עם כל הטבלה בשביל שנוכל לקחת את כל הערכים עבור הצ'ק בוקס שסומן
                        {
                            if (Mac_Key_Heb == DatatableSql.Rows[i]["Mac_Key_Heb"].ToString())//אחרי שסומנה בדיקה מסוימת נשלוף פרטים נוספים
                            {
                                SqlParameter Section = new SqlParameter("f", DatatableSql.Rows[i]["Section"].ToString());
                                SqlParameter Mac_Key_ID = new SqlParameter("g", DatatableSql.Rows[i]["Mac_Key_ID"].ToString());
                                sqlcombo.ExecuteQuery(ForInsert, CommandType.Text, MachineID, DepartmentID, Spec_No, Emp_No, Building_Stage, Section, Mac_Key_ID);
                            }
                        }
                    }
                }
            }

            //שמירת נתוני מרכיבים בטבלה 
            for (int i = 0; i < dataGridViewTireComponent.RowCount; i++)
            {
                ForInsert = $@"INSERT tbl_Spec_Components
                               VALUES (?,?,?,?,?,?,?,?,?,?)";//שאילתת הכנסת נתונים

           

                SqlParameter Catalog_No = new SqlParameter("c", dataGridViewTireComponent.Rows[i].Cells["bchld"].Value.ToString());
                string ForUpdate = $@"UPDATE tbl_Spec_Components
                                      SET Serial_No=(?),Act_Length=(?),Act_Width=(?),Act_Combined_Width=(?)
                                      WHERE Spec_No='{GeneralDetails.Specifications}' and Catalog_No='{Catalog_No.Value}'" ;//שאילתת עדכון
                SqlParameter Serial_No = new SqlParameter("s", dataGridViewTireComponent.Rows[i].Cells["סידורי"].Value.ToString());

                SqlParameter Act_Length;
                if (!string.IsNullOrEmpty(dataGridViewTireComponent.Rows[i].Cells["lenght"].Value.ToString()))
                    Act_Length = new SqlParameter("s", dataGridViewTireComponent.Rows[i].Cells["lenght"].Value.ToString());
                else
                    Act_Length = new SqlParameter("s", DBNull.Value);

                SqlParameter Act_Width;
                if (!string.IsNullOrEmpty(dataGridViewTireComponent.Rows[i].Cells["width"].Value.ToString()))
                    Act_Width = new SqlParameter("w", dataGridViewTireComponent.Rows[i].Cells["width"].Value.ToString());
                else
                    Act_Width= new SqlParameter("w", DBNull.Value);


                SqlParameter Act_Combined_Width;
                if (!string.IsNullOrEmpty(dataGridViewTireComponent.Rows[i].Cells["WidthCombined"].Value.ToString()))
                    Act_Combined_Width = new SqlParameter("c", dataGridViewTireComponent.Rows[i].Cells["WidthCombined"].Value.ToString());
                else
                    Act_Combined_Width= new SqlParameter("c", DBNull.Value);


                if(IfUpdatedTable==false)//הכנסת נתונים ראשונית
                    sqlcombo.ExecuteQuery(ForInsert, CommandType.Text, MachineID, DepartmentID, Spec_No, Emp_No,Building_Stage,Catalog_No,Serial_No,Act_Length,Act_Width,Act_Combined_Width);
                else //עדכון
                    sqlcombo.ExecuteQuery(ForUpdate, CommandType.Text, Serial_No, Act_Length, Act_Width, Act_Combined_Width);

            }

            //שמירת מנהל מאשר
            if(ManagerConfirm)//אם מנהל אישר ישמור בדטה בייס
            {
                string ForUpdate1 = $@"UPDATE tbl_Mach_Doc_Manager
                                      SET Manager_ID=(?),Manager_Remark=(?)
                                      WHERE Spec_No='{GeneralDetails.Specifications}'";
                SqlParameter Manager_ID = new SqlParameter("M", txt_Manager.Text);
                SqlParameter Manager_Remark;
                if (!string.IsNullOrEmpty(txt_comment.Text))
                    Manager_Remark = new SqlParameter("r", txt_comment.Text);
                else
                    Manager_Remark = new SqlParameter("r", DBNull.Value);

                //if (IfUpdateManger == false)//הכנסה ראשונית של מנהל מאשר
                //    sqlcombo.ExecuteQuery(ForInsert, CommandType.Text, MachineID, DepartmentID, Spec_No, Emp_No, Building_Stage, Manager_ID, Manager_Remark);
                //else//עדכון
                    sqlcombo.ExecuteQuery(ForUpdate1, CommandType.Text, Manager_ID, Manager_Remark);

            }

        }

        /// <summary>
        /// שומר סטטוס בקרה עצמית אדום/צהוב/ירוק
        /// </summary>
        private void SaveStatusSqlServer(ColorSelfInspection colorSelfInspection)
        {
            //SqlParameter colorSelfInspectionParam = new SqlParameter("c", colorSelfInspection.ToString());
            //string qry = $@"SELECT *
            //              FROM tbl_Spec_Status
            //              WHERE Spec_No='{GeneralDetails.Specifications}' and Machine_ID='{MachineID}' and Emp_No='{Emp_No}' and Department_ID={Department}";
            //DbServiceSQL sqlcombo = new DbServiceSQL();
            //DataTable DTCheckStatus = new DataTable();
            //if (DTCheckStatus.Rows.Count == 0)//אם הטבלה ריקה ניצור רשומה חדשה 
            //{
            //    qry= $@"INSERT tbl_Spec_Status
            //            VALUES (?,?,?,?,?)";
            //    sqlcombo.ExecuteQuery(qry, CommandType.Text, Spec_No,DepartmentID,MachineID,Emp_No,colorSelfInspectionParam);

            //}
            //else //קיימת רשומה רק נעדכן צבע סטטוס
            //{
            //    qry=   $@"UPDATE tbl_Spec_Status
            //              SET Status=(?)
            //              WHERE Spec_No='{GeneralDetails.Specifications}' and Machine_ID='{MachineID}' and Emp_No='{Emp_No}' and Department_ID={Department}";
            //    sqlcombo.ExecuteQuery(qry, CommandType.Text,colorSelfInspectionParam);
            //}
        }


        /// <summary>
        /// שולף בדיקות מהדטה בייס שבוצעו כבר בעבר עבור אותו מפרט
        /// </summary>
        private void CheckStatusInspection()
        {
            LogWaveClass.LogWave("selfInspection.CheckStatusInspection start");
            //שליפה מטבלת בקרת ציוד ובטיחות
            DbServiceSQL sqlcombo = new DbServiceSQL();
            DataTable DTCheckbox = new DataTable();
            string qry = $@"SELECT M.Mac_Key_ID,d.Mac_Key_Heb
                            FROM tbl_Mach_Doc_Check M inner join tbl_Mach_Doc_Set D on M.Mac_Key_ID=D.Mac_Key_ID
                            WHERE M.Spec_No='{GeneralDetails.Specifications}'";//לוקח את כל צ'ק בוקסים שסומנו עבור המפרט הנוכחי
            DTCheckbox = sqlcombo.executeSelectQueryNoParam(qry);
            if (DTCheckbox.Rows.Count != 0) IfUpdateCheckbox = true;//סימנו בעבר כבר חלק מהצ'ק בוקסים ולכן נמחק רשומה אחרונה
            for (int i = 0; i < DTCheckbox.Rows.Count; i++)
            {
                foreach (var control in this.Controls) // מעבר על הצ'ק בוקסים לבדוק אלו סומנו כבר בעבר
                {
                    if (control is CheckBox)
                    {
                        if (((CheckBox)control).Text == DTCheckbox.Rows[i]["Mac_Key_Heb"].ToString())
                            ((CheckBox)control).Checked = true;
                    }
                }
            }

    

            LogWaveClass.LogWave("selfInspection.CheckStatusInspection end");
        }

        /// <summary>
        /// שם את הנתונים שהוכנסו כבר בטבלה בעבר-(רוחב,אורך,סידורי)
        /// </summary>
        private void CheckComoponentInserted()
        {
            try
            {
                //שליפה מטבלת נתוני מרכיבים
                DbServiceSQL sqlcombo = new DbServiceSQL();
                DataTable DTComponents = new DataTable();
                string qry = $@"SELECT s.Catalog_No,s.Serial_No,s.Act_Length,s.Act_Width,s.Act_Combined_Width
                    FROM tbl_Spec_Components S
                    WHERE s.Spec_No='{GeneralDetails.Specifications}'";
                LogWaveClass.LogWave(qry);
                DTComponents = sqlcombo.executeSelectQueryNoParam(qry);
                if (DTComponents.Rows.Count != 0) IfUpdatedTable = true;//חיווי שיהיה צריך לעדכן רשומות ולא להכניס חדש

                for (int j = 0; j < DTComponents.Rows.Count; j++)
                {
                    for (int k = 0; k < ForDataGrid.Rows.Count; k++)
                    {
                        if (DTComponents.Rows[j]["Catalog_No"].ToString() == ForDataGrid.Rows[k]["bchld"].ToString())
                        {
                            if (!string.IsNullOrEmpty(DTComponents.Rows[j]["Serial_No"].ToString()) && DTComponents.Rows[j]["Serial_No"].ToString().Trim() != "0")
                                ForDataGrid.Rows[k]["סידורי"] = DTComponents.Rows[j]["Serial_No"].ToString();
                            if (!string.IsNullOrEmpty(DTComponents.Rows[j]["Act_Length"].ToString()))
                                ForDataGrid.Rows[k]["lenght"] = DTComponents.Rows[j]["Act_Length"].ToString();
                            if (!string.IsNullOrEmpty(DTComponents.Rows[j]["Act_Width"].ToString()))
                                ForDataGrid.Rows[k]["width"] = DTComponents.Rows[j]["Act_Width"].ToString();
                            if (!string.IsNullOrEmpty(DTComponents.Rows[j]["Act_Combined_Width"].ToString()))
                                ForDataGrid.Rows[k]["WidthCombined"] = DTComponents.Rows[j]["Act_Combined_Width"].ToString();
                        }
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
        /// מוחק מאסקיואל אם מדובר בשינוי מפרט או שינוי מספר עובד
        /// </summary>
        private void DeleteComoponentInserted()
        {
            DbServiceSQL sqlcombo = new DbServiceSQL();
            string qry = $@"DELETE
                           FROM tbl_Spec_Components";//מחיקת טבלת מרכיבים
            LogWaveClass.LogWave(qry);
            sqlcombo.executeSelectQueryForDelete(qry);
            qry = $@" DELETE
                    FROM tbl_Mach_Doc_Check";
            LogWaveClass.LogWave(qry);
            sqlcombo.executeSelectQueryForDelete(qry);//מוחק צ'ק בוקסים
            qry = $@"DELETE
                   FROM tbl_Mach_Doc_Manager";//מחיקת מספר מנהל 
            LogWaveClass.LogWave(qry);
            sqlcombo.executeSelectQueryForDelete(qry);//מוחק צ'ק בוקסים
            txt_Manager.Text = "";
        }


        private void SelfInspection_Shown(object sender, EventArgs e)
        {
            txt_barcode.Focus();
        }

        /// <summary>
        ///-(אותן בדיקות כמו במסך הראשי)בדיקות מספר סריאלי לפני המשך תהליך
        /// </summary>
        /// <returns></returns>
        private bool CheckStickerSerialNumBeoforeContinue()
        {
            txt_barcode.CharacterCasing = CharacterCasing.Upper;//מחליף הכל לאותיות גדולות
            bool ContinueNext = CheckSerialNumBarCode(txt_barcode.Text);
            if (!ContinueNext)
            {
                txt_barcode.Focus();
                return false;
            }
            ComponentBarCode = txt_barcode.Text.Split(' ').First();//הכנסת מספר קטלוגי-של קריאת מרכיב                     
            string value="";
            var regex = new Regex(@"\s");
            if (regex.IsMatch(txt_barcode.Text))//אם יש רווח אחרי מספר קטלוגי יש גם מספר סריאלי
            {
                if (!string.IsNullOrEmpty(txt_barcode.Text.Split(' ').Last() as string))//בודק אם הרווח לא סתמי ובאמת יש משהו אחר כך
                {
                    value = txt_barcode.Text.Split(' ').Last();
                    if(value=="S")//לא הכניס סריאלי אבל הכניס קטלוגי
                        SerialNum = 999999;//מספר סריאלי בדוי
                    else
                    {
                        value = value.Substring(value.Length - 6);
                        int.TryParse(value, out int Num);//הכנסת מספר סריאלי במידה ויש
                        if (Num != 0)
                            SerialNum = Num;
                        else
                            MessageBox.Show("הכנס מספר סריאלי עם ספרות בלבד", "הודעת שגיאה", MessageBoxButtons.OK, MessageBoxIcon.Exclamation, MessageBoxDefaultButton.Button1);
                    }  
                }
                else
                    SerialNum = 999999;//מספר סריאלי בדוי
            }
            else
            {
                SerialNum = 999999;
            }
                return true;//עבר את הבדיקות בהצלחה

        }

        /// <summary>
        /// בדיקה שסידורי הוכנס כמו שצריך
        /// </summary>
        private bool CheckSerialNumBarCode(string txtSerial)
        {


            if (txtSerial == "")
            {
                MessageBox.Show($@"הכנס {lbl_Product.Text}", "הודעת שגיאה", MessageBoxButtons.OK, MessageBoxIcon.Exclamation, MessageBoxDefaultButton.Button1);
                return false;
            }

            //if (txtSerial.Length != 21)
            //{
            //    MessageBox.Show("מקט לא הוכנס בהתאם", "הודעת שגיאה", MessageBoxButtons.OK, MessageBoxIcon.Exclamation, MessageBoxDefaultButton.Button1);
            //    return false;
            //}


            return true;
        }


        public void CellPainting()
        {
            dataGridViewTireComponent.Columns["lenght"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            dataGridViewTireComponent.Columns["width"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            dataGridViewTireComponent.Columns["WidthCombined"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            dataGridViewTireComponent.Columns["דיווח פסולות"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;

            for (int i = 0; i < yellowInSelfInspections.Count; i++)//עובר על כל התאים שצריך לצבוע בצהוב
            {
                dataGridViewTireComponent.Rows[yellowInSelfInspections[i].Row].Cells[yellowInSelfInspections[i].Column].Style.BackColor = Color.Yellow;
            }
        }

        private void lbl_upperline_Click(object sender, EventArgs e)
        {

        }

        private void dataGridViewTireComponent_CellEndEdit(object sender, DataGridViewCellEventArgs e)
        {
            try
            {
                if (!double.TryParse(dataGridViewTireComponent.Rows[e.RowIndex].Cells[e.ColumnIndex].Value.ToString(), out double num))
                {
                    MessageBox.Show("הקלד מספר בלבד", "הודעת שגיאה", MessageBoxButtons.OK, MessageBoxIcon.Exclamation, MessageBoxDefaultButton.Button1);
                    if (PreviousValue != 0)
                        dataGridViewTireComponent.Rows[e.RowIndex].Cells[e.ColumnIndex].Value = PreviousValue;
                    else
                        dataGridViewTireComponent.Rows[e.RowIndex].Cells[e.ColumnIndex].Value = "";
                }
                if (num % 1 != 0)//אם מספר דצימלי מגביל אותו לספרה אחת אחרי הנקודה העשרונית
                {
                    num = Math.Round(num, 1);
                    dataGridViewTireComponent.Rows[e.RowIndex].Cells[e.ColumnIndex].Value = num;
                }
            }

            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                LogWaveClass.ErrorLogWave("Error: " + ex.Message);
            }


        }

        private void dataGridViewTireComponent_CellBeginEdit(object sender, DataGridViewCellCancelEventArgs e)
        {
            try
            {
                if (!string.IsNullOrEmpty(dataGridViewTireComponent.Rows[e.RowIndex].Cells[e.ColumnIndex].Value.ToString() as string))
                {
                    PreviousValue = double.Parse(dataGridViewTireComponent.Rows[e.RowIndex].Cells[e.ColumnIndex].Value.ToString());
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                LogWaveClass.ErrorLogWave("Error: " + ex.Message);
            }

        }

        private void txt_Manager_Leave(object sender, EventArgs e)
        {
            try
            {
                ManagerConfirm = CheckManger(txt_Manager.Text);//בדיקה אם מנהל אישר
                if (ManagerConfirm)
                {
                    for (int i = 0; i < dataGridViewTireComponent.RowCount; i++)
                    {
                        if (TireComponenetsdataTable.Rows[i]["ICLAS"].ToString().Substring(0, 1) == "B")//אם יש תערובת לא מאושרת ידלג על לשים אוטומטי מספר סידרורי בעקבות אישור מנהל
                        {
                            bool Check=CheckBMixture();
                            if (!Check)
                                continue;
                        }
                         
                        if (dataGridViewTireComponent.Rows[i].Cells["סידורי"].Value.ToString() == "" || dataGridViewTireComponent.Rows[i].Cells["סידורי"].Value.ToString() == "0")
                            dataGridViewTireComponent.Rows[i].Cells["סידורי"].Value = 999999;
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
        /// דיווח פסולות
        /// </summary>
        private void button2_Click(object sender, EventArgs e)
        {
            try
            {
                LogWaveClass.LogWave("תחילת דיווח פסולות");
                //בדיקה אם המחסן בספירה-אם כן לא ניתן לדווח פסולות 
                string Sqlstr = "select count(DATA)  from BPCSFV30.ZPAL01  where PKEY in ('SFIRA#AL','SFIRA#PS') and substring(DATA,1,1)='Y'";
                DataTable dataTable = new DataTable();
                dataTable = DBSs.executeSelectQueryNoParam(Sqlstr);
                if (dataTable.Rows.Count == 0)
                {
                    MessageBox.Show("שגיאה בבדיקת ספירת מחסן");
                    return;
                }
                if (int.Parse(dataTable.Rows[0][0].ToString()) > 0)
                {
                    MessageBox.Show("מחסן בספירה לא ניתן לדווח מרכיבים פסולים");
                    for (int i = 0; i < dataGridViewTireComponent.RowCount; i++)//מאפס את כל עמודת דיווח פסולת
                    {
                        dataGridViewTireComponent.Rows[i].Cells["דיווח פסולות"].Value = "";
                    }
                    return;

                }
                LogWaveClass.LogWave("אחרי בדיקה ולפני דיווח פסולות");

                string WasteQry = "";
                double value;
                for (int i = 0; i < dataGridViewTireComponent.RowCount; i++)
                {
                    if (dataGridViewTireComponent.Rows[i].Cells["דיווח פסולות"].Value.ToString() != "0" && !string.IsNullOrEmpty(dataGridViewTireComponent.Rows[i].Cells["דיווח פסולות"].Value.ToString()))
                    {
                        value = double.Parse(dataGridViewTireComponent.Rows[i].Cells["דיווח פסולות"].Value.ToString());
                        //אם מדובר בסוג גליל-עושים המרת יחידות לפי תוכנית בS400
                        if (dataGridViewTireComponent.Rows[i].Cells["IUMS"].Value.ToString().Trim() == "GL")
                        {
                            Sqlstr = $@"select  {value}/IUMCN*UMCONV from BPCSFV30.IIML01 JOIN BPCSFV30.IUML01 on IUMP=UMFUM and UMTUM='MT' where IPROD='{CatalogNum}'";
                            dataTable = DBSs.executeSelectQueryNoParam(Sqlstr);
                            if (dataTable.Rows.Count != 0)
                                value = double.Parse(dataTable.Rows[0][0].ToString());//מכניס את הערך המומר אם זה גליל
                        }
                        //Call bpcsoali.in@430c('58024950-311  S','000000100')
                        ///לסיים את הדיווח פסולות
                        string temp = (value * 100).ToString("000000000"); ; //פורמט מספר עם אפסים מובילים
                        Sqlstr = $@"Call bpcsoali.in@430c('" + (CatalogNum).PadRight(15, ' ') + "','" + temp + "')";
                        //DBSs.executeInsertQuery(Sqlstr);           
                    }
                }
                LogWaveClass.LogWave("סיים דיווח פסולות");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                LogWaveClass.ErrorLogWave("Error: " + ex.Message);
            }
        }

        private void dataGridViewTireComponent_CellEnter(object sender, DataGridViewCellEventArgs e)
        {
            if (dataGridViewTireComponent.Rows[e.RowIndex].Cells[e.ColumnIndex].Style.BackColor == Color.Yellow)//מתחיל לערוך כמות של תא מפורט כבר
            {
                dataGridViewTireComponent.Rows[e.RowIndex].Cells[e.ColumnIndex].ReadOnly = false;
                return;
            }
        }

        private void SelfInspection_FormClosing(object sender, FormClosingEventArgs e)
        {
            PreviousEmployee = GeneralDetails.EmployeeId;//פעם הבאה שיפתח בקרה עצמית יבדוק אם זה אותו עובד
            PreviousSpecification = GeneralDetails.Specifications;
        }

        private void dataGridViewTireComponent_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            foreach (DataGridViewColumn column in dataGridViewTireComponent.Columns)
            {
                column.SortMode = DataGridViewColumnSortMode.NotSortable;
            }
        }

        private void txt_Manager_KeyPress(object sender, KeyPressEventArgs e)
        {
            txt_Manager.PasswordChar = '*';
        }
    }
}
