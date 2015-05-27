using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NJJMinMaxUpload.Model
{
    public class MinMax
    {
        public String _ProductCode {get; set;}
        public int _Min {get; set;}
        public int _Max {get; set;}
        public String _LastTransacDate { get; set; }
        public String _StoreCode { get; set; }
        public String _InvQuantity { get; set; }

        public int CheckNegative(int inp)
        {
            if(inp >=0)
                return inp;
            else
                return 0;
        }
    }
}
