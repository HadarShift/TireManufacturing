using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TireManufacturing
{
    class StopReasonClass:General
    {
        public List<string> StopReasons { get; set; }//רשימת סיבות עצירה
        public Dictionary<int,string> StopCodeIndex { get; set; }//קוד תקלה בהתאם לאינדקס רשימת STOPREASON
        public Dictionary<int, string> DescriptionStop { get; set; }//תיאורי תקלה

        public string CatalogNum { get; set; }
        public int Shift { get; set; }
        public string TimeStartStop { get; set; }//זמן התחלת תקלה
        public string WorkCenter { get; set; }
        public StopReasonClass()
        {
            LogWaveClass.LogWave("פתח אובייקט StopReasonClass");
            StopReasons = new List<string>();//סיבות עצירה
            StopCodeIndex = new Dictionary<int, string>();
            DescriptionStop = new Dictionary<int, string>();
            LogWaveClass.LogWave("לפני בדיקה האם ישנן סיבות עצירה");

            ShowStopReasons();
            WorkCenter = "0";
        }

        /// <summary>
        /// סיבות עצירה 
        /// </summary>
        private void ShowStopReasons()
        {
            try
            {
                LogWaveClass.LogWave("before open sql server");

                //בדיקה קודי תקלה רלוונטים מול sql serever
                DbServiceSQL dbServiceSQL = new DbServiceSQL();
                LogWaveClass.LogWave("after open sql server");

                DataTable CodesStop = new DataTable();//מקבלת שורה אחת בה כל הנתונים על המכונה עליה אנחנו עובדים
                string str =
                    $@"  SELECT *
                     FROM [dbo].[tbl_Malfunction]";
                LogWaveClass.LogWave(str);
                CodesStop = dbServiceSQL.executeSelectQueryNoParam(str);

                string WhereIn = "";//תוספת לשאילתה 
                for (int i = 0; i < CodesStop.Rows.Count; i++)
                {
                    if (i == CodesStop.Rows.Count - 1)
                        WhereIn += "'" + CodesStop.Rows[i]["Malfunction_Code"].ToString() + "'";
                    else WhereIn += "'" + CodesStop.Rows[i]["Malfunction_Code"].ToString() + "',";
                }
                LogWaveClass.LogWave("before open s400 server");

                DBService DBS = new DBService();
                DataTable dataTable = new DataTable();
                string query =
                $@"SELECT right(trim(IPROD),2) as Code, IDESC as Description  
               FROM  BPCSFV30.IIML01 
               WHERE  left(trim(IPROD),6)='TAK-71' and substring(IPROD,7,9) in ({WhereIn})
               ORDER BY right(trim(IPROD),2)";//בדיקה מול הsql  AND substring(IPROD,7,9) in ('04','06','13','18','22','29','30','31','33','35','40','55','95','51','52','26')"
                LogWaveClass.LogWave(query + "   stop reason");
                dataTable = DBS.executeSelectQueryNoParam(query);

                for (int i = 0; i < dataTable.Rows.Count; i++)
                {
                    StopReasons.Add($@"Tak-{DepratmentId}{dataTable.Rows[i]["Code"].ToString()} - {dataTable.Rows[i]["Description"].ToString()}");
                    DescriptionStop.Add(i, dataTable.Rows[i]["Description"].ToString());
                    StopCodeIndex.Add(i, dataTable.Rows[i]["Code"].ToString());
                }
            }
            catch (Exception ex)
            {
                LogWaveClass.ErrorLogWave("Error: " + ex.Message);
            }

        }
    }
}
