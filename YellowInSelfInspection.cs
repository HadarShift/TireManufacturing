using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TireManufacturing
{
    //יוצר אובייקטים עבור תאים שצריך למלא בטופס בקרה עצמית(ייצבע בצהוב)
    class YellowInSelfInspection
    {
        public int Row { get; set; }//השורה שתבצע בצהוב
        public string Column { get; set; }//הטור שייצבע בצהוב

        public YellowInSelfInspection(int Row,string Column)
        {
            this.Row = Row;
            this.Column = Column;
        }
    }
}
