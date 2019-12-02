using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TireManufacturing
{
    /// <summary>
    /// נתונים בסיסיים מחלקה ומכונה
    /// </summary>
    class General //
    {
        public string MachineID { get; set; }//ת.ז מכונה
        public string DepratmentId { get; set; }//מחלקה
        public int EmployeeId { get; set; }//מספר עובד
        public string Specifications { get; set; }//מפרט
        
        public General()
        {
            ExeConfigurationFileMap fileMap =
       new ExeConfigurationFileMap();
            fileMap.ExeConfigFilename =
        @"C:\\c#\\TireManufacturing.exe.config";
            System.Configuration.Configuration config =
                ConfigurationManager.OpenMappedExeConfiguration(fileMap,
                ConfigurationUserLevel.None);
      
            MachineID = config.AppSettings.Settings["MachineID"].Value;
            DepratmentId = config.AppSettings.Settings["DepartmentID"].Value;

            //MachineID = System.Configuration.ConfigurationManager.AppSettings["MachineID"];
            //DepratmentId = System.Configuration.ConfigurationManager.AppSettings["DepartmentID"];
        }



    }
}
