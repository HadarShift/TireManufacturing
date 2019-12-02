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
    /// נתוני מכונה-יורשת ממחלקת כללי
    /// </summary>
    class Machine:General //מה המכונה עצמה עושה יורשת פרטים ממחלקת גנרל
    {
        //MACHINE_ID ,departmentId יורשים ממחלקת גנרל

        public int MaxWeightingCap { get; set; }//מקסימום של ראש משקל
        public int TireBuildType { get; set; }//מה מייצרת המכונה כרגע
        public string ComPort1 { get; set; }//חיבור לראש שקילה 1
        public string ComPort2 { get; set; }//חיבור לראש שקילה 2
        public bool IsLoadingSpec { get; set; }//כרגע לא
        public bool IsLoadingId { get; set; }//כרגע לא
        public bool IsLoadingTechSpec { get; set; }//כרגע לא
        public string BatteryName { get; set; }
        public bool Karkas { get; set; }//מייצר קרקס?
        public bool LevelB { get; set; }//שלב ב'?
        public bool Tire { get; set; }//צמיג שלם
        public string AS400User { get; set; }//יוזר עבור המכונה לs400,שימוש לקבלת תקלות וכתיבה לdb 
        public string AS400Pass { get; set; }//סיסמא עבור היוזר של המכונה
        public string OpcServer { get; set; }//בקרים
        public string OpcIdFolder { get; set; }
        public string OpcMachFolder { get; set; }
        public float MaxWtTol { get; set; }//מקסימום סטיית משקל בפלוס
        public float MinWtTol { get; set; }//מינימום סטיית משקל במינוס
        public float ChangeWraps { get; set; }
        public float ChangeDist { get; set; }
        public float ChangeSSN { get; set; }
        public string EmailServer  { get; set; }
        public string IdDataKey { get; set; }
        public bool MustCaracasWeight { get; set; }//האם בשלב ב' מחייב שקילת קרקס לפני 
        public Machine():base()
        {
            LogWaveClass.LogWave("פעולת כתיבה ראשונה לקובץ לוג");
            GetDataMachine();
        }

        /// <summary>
        /// פרטים על המכונה 
        /// </summary>
        public void GetDataMachine()
        {
            try
            {
                LogWaveClass.LogWave("מקבל פרטי מכונה");
                DbServiceSQL dbServiceSQL = new DbServiceSQL();
                DataTable DataMachineSql = new DataTable();//מקבלת שורה אחת בה כל הנתונים על המכונה עליה אנחנו עובדים
                string str =
                    $@"SELECT *
                       FROM dbo.tbl_Mach_Set M
                       WHERE M.Machine_ID='{MachineID}' and M.Department_ID={DepratmentId}" ;
                LogWaveClass.LogWave("מקבל פרטי מכונה מתוך SQL "+str);
                DataMachineSql = dbServiceSQL.executeSelectQueryNoParam(str);
             

                if (DataMachineSql.Rows.Count>0) //רק אם יש רשומה כזאת
                {
                    LogWaveClass.LogWave("לפני השמת שדות במחלקת MACHINE");
                    MaxWeightingCap = int.Parse(DataMachineSql.Rows[0]["Max_Weighting_Cap"].ToString());
                    TireBuildType = int.Parse(DataMachineSql.Rows[0]["Tire_Build_Type"].ToString());
                    if (!string.IsNullOrEmpty(DataMachineSql.Rows[0]["Com_Port_1"].ToString()))
                        ComPort1 = DataMachineSql.Rows[0]["Com_Port_1"].ToString();
                    if (!string.IsNullOrEmpty(DataMachineSql.Rows[0]["Com_Port_2"].ToString()))
                        ComPort2 = DataMachineSql.Rows[0]["Com_Port_2"].ToString();
                    IsLoadingSpec =  bool.Parse(DataMachineSql.Rows[0]["Is_Loading_Spec"].ToString());
                    IsLoadingId = bool.Parse(DataMachineSql.Rows[0]["Is_Loading_ID"].ToString());
                    IsLoadingTechSpec=bool.Parse(DataMachineSql.Rows[0]["Is_Loading_Tech_Spec"].ToString());
                    if(!string.IsNullOrEmpty(DataMachineSql.Rows[0]["Battery_Name"].ToString()))
                        BatteryName = DataMachineSql.Rows[0]["Battery_Name"].ToString();
                    if (!string.IsNullOrEmpty(DataMachineSql.Rows[0]["Stage_A"].ToString()))
                        Karkas = bool.Parse(DataMachineSql.Rows[0]["Stage_A"].ToString());
                    if (!string.IsNullOrEmpty(DataMachineSql.Rows[0]["Stage_B"].ToString()))
                        LevelB = bool.Parse(DataMachineSql.Rows[0]["Stage_B"].ToString());
                    if (!string.IsNullOrEmpty(DataMachineSql.Rows[0]["Complete_Tire"].ToString()))
                        Tire = bool.Parse(DataMachineSql.Rows[0]["Complete_Tire"].ToString());
                    if (!string.IsNullOrEmpty(DataMachineSql.Rows[0]["AS400_User"].ToString()))
                    {
                        AS400User = DataMachineSql.Rows[0]["AS400_User"].ToString();
                        GlobalVariables.AS400User = AS400User;
                    }
                    
                    if (!string.IsNullOrEmpty(DataMachineSql.Rows[0]["AS400_Pass"].ToString()))
                    {
                        AS400Pass = DataMachineSql.Rows[0]["AS400_Pass"].ToString();
                        GlobalVariables.AS400Pass = AS400Pass;
                    }
                    if (!string.IsNullOrEmpty(DataMachineSql.Rows[0]["OPC_Server"].ToString()))
                        OpcServer = DataMachineSql.Rows[0]["OPC_Server"].ToString();
                    if (!string.IsNullOrEmpty(DataMachineSql.Rows[0]["OPC_ID_Folder"].ToString()))
                        OpcIdFolder = DataMachineSql.Rows[0]["OPC_ID_Folder"].ToString();
                    if (!string.IsNullOrEmpty(DataMachineSql.Rows[0]["OPC_Mach_Folder"].ToString()))
                        OpcMachFolder = DataMachineSql.Rows[0]["OPC_Mach_Folder"].ToString();
                    if (!string.IsNullOrEmpty(DataMachineSql.Rows[0]["Max_Wt_Tol"].ToString()))
                        MaxWtTol = float.Parse(DataMachineSql.Rows[0]["Max_Wt_Tol"].ToString());
                    if (!string.IsNullOrEmpty(DataMachineSql.Rows[0]["Min_Wt_Tol"].ToString()))
                        MinWtTol = float.Parse(DataMachineSql.Rows[0]["Min_Wt_Tol"].ToString());
                    if (!string.IsNullOrEmpty(DataMachineSql.Rows[0]["Change_Wraps"].ToString()))
                        ChangeWraps = float.Parse(DataMachineSql.Rows[0]["Change_Wraps"].ToString());
                    if (!string.IsNullOrEmpty(DataMachineSql.Rows[0]["Change_Dist"].ToString()))
                        ChangeDist = float.Parse(DataMachineSql.Rows[0]["Change_Dist"].ToString());
                    if (!string.IsNullOrEmpty(DataMachineSql.Rows[0]["Change_SSN"].ToString()))
                        ChangeSSN = float.Parse(DataMachineSql.Rows[0]["Change_SSN"].ToString());
                    if (!string.IsNullOrEmpty(DataMachineSql.Rows[0]["Email_Server"].ToString()))
                        EmailServer = DataMachineSql.Rows[0]["Email_Server"].ToString();
                    if (!string.IsNullOrEmpty(DataMachineSql.Rows[0]["ID_Data_Key"].ToString()))
                        IdDataKey = DataMachineSql.Rows[0]["ID_Data_Key"].ToString();
                    if (!string.IsNullOrEmpty(DataMachineSql.Rows[0]["Must_Carcass_Wt"].ToString()))
                        MustCaracasWeight = bool.Parse( DataMachineSql.Rows[0]["Must_Carcass_Wt"].ToString());
                }
                LogWaveClass.LogWave("סיים לקבל פרטי מכונה");

            }

            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }


        }

    }
}
