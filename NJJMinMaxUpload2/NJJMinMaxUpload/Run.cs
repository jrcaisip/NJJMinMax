using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using NJJMinMaxUpload.Controller;
using NJJMinMaxUpload.Model;
using System.Data.SqlClient;
using System.Data.Odbc;
using Microsoft.Win32;

namespace NJJMinMaxUpload
{
    class Run
    {

        public static void Main()
        {
            RegistryKey rk = Registry.CurrentUser.CreateSubKey("indexForUpload");

            SpinAnimation.Start(50);
            if (rk == null)
            {
                rk.SetValue("index", 0);
            }
            UploadMinMax um = new UploadMinMax();
            SpinAnimation.Stop();
        }
    }

}
    

