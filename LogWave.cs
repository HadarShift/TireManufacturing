using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TireManufacturing
{
    class LogWaveClass
    {
        public static void LogWave(string LogStr)
        {
            StreamWriter SW;
            string LogName = DateTime.Now.Date.ToString("yyyy-MM-dd") + " LogFile";

            if (!File.Exists(@"C:\\c#\" + LogName + ".txt"))
            {
                SW = File.CreateText(@"C:\\c#\" + LogName + ".txt");
                SW.Close();
            }

            SW = File.AppendText(@"C:\\c#\" + LogName + ".txt");
            SW.WriteLine(DateTime.Now.ToString() + " - " + LogStr);
            SW.Close();
        }

        public static void ErrorLogWave(string LogStr)
        {
            StreamWriter SW;
            string LogName = DateTime.Now.Date.ToString("yyyy-MM-dd") + "Error LogFile";

            if (!File.Exists(@"C:\\c#\" + LogName + ".txt"))
            {
                SW = File.CreateText(@"C:\\c#\" + LogName + ".txt");
                SW.Close();
            }

            SW = File.AppendText(@"C:\\c#\" + LogName + ".txt");
            SW.WriteLine(DateTime.Now.ToString() + " - " + LogStr);
            SW.Close();
        }

        /// <summary>
        /// מוחק קבצי לוג מלפני 3 או 4 ימים
        /// </summary>
        public static void DeleteLog()
        {
            try
            {
                string LogName = ""; 
                for (int i = 30; i < 60; i++)
                {
                    LogName= DateTime.Now.AddDays(-i).Date.ToString("yyyy-MM-dd") + " LogFile";
                    if (File.Exists(@"C:\\c#\" + LogName + ".txt"))
                    {
                        File.Delete(@"C:\\c#\" + LogName + ".txt");
                    }
                }
                         
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message);
                ErrorLogWave("Error:בעיה במחיקת קבצי לוג " );
            }
        }
    }
}
