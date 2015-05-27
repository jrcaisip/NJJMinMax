using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NJJMinMaxUpload.Model;
using System.Transactions;
using System.Data.Odbc;
using Microsoft.Win32;

namespace NJJMinMaxUpload.Controller
{
    class UploadMinMax
    {
        private ComputeMinMax mm;
        private RegistryKey rk = Registry.CurrentUser.CreateSubKey("indexForUpload");

        private OdbcConnection cnn;
        private OdbcCommand insCmd;
        private OdbcCommand delCmd;
        private OdbcCommand preCmd;

        //ODBC connection to the WebSales
        private String connetionString = @"Provider=MSDASQL;Driver={MySQL ODBC 5.1 Driver};
                    Server=aetuser.db.8363057.hostedresource.com;
                    Database=aetuser;User=aetuser;Password=Enigmatech2;Option=3;";

        public UploadMinMax()
        {
            String dq;
            String iq;
            var gd = new GetMinMaxData();
            
            //gets the data from the database and computes the minmax
            mm = new ComputeMinMax(gd.getSalesList,gd.getInvList);
            cnn = new OdbcConnection(connetionString);

            Connect();

            try
            {
                using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required, TimeSpan.FromSeconds(3600)))
                {

                    if ((int)rk.GetValue("index") == 0)
                    {
                        //Delete query
                        dq = @"DELETE FROM njjminmax";
                        delCmd = new OdbcCommand(dq, cnn);
                        Console.WriteLine("Records Deleted! Total Rows Affected " + delCmd.ExecuteNonQuery() + "\n");
                    }

                    //Prepare Table
                    preCmd = new OdbcCommand("ALTER TABLE njjminmax AUTO_INCREMENT = 1", cnn);
                    preCmd.ExecuteNonQuery();
                    //Insert Query
                    iq = @"INSERT INTO njjminmax (Product_Code, Last_Trans_Date, Current_Inventory, Min, Max,Store_Code)
                    VALUES (?,?,?,?,?,?)";

                    insCmd = new OdbcCommand(iq, cnn);
                    insCmd.CommandTimeout = 30;

                    Console.WriteLine("Uploading Data to web DB \n");

                    //if the registry key is equal to zero it will start a new upload otherwise it will call the continue upload
                    //passing the last index saved in the registry
                    if ((int)rk.GetValue("index") == 0)
                        NewUpload();
                    else
                        ContinueUpload((int)rk.GetValue("index"));

                    scope.Complete();
                }

            }catch (OdbcException ex)
            {
                Console.WriteLine("Test aborted: " + ex);
            }

            rk.SetValue("index", 0);
            cnn.Close();
            Console.Write("\n Done!");
        }

        //Upload method where if the index in the registry is equal to 0
        private void NewUpload()
        {
            try
            {
                for (int i = 0; i != mm.pubMm.Count; i++)
                {
                    insCmd.Parameters.AddWithValue("Product_Code", mm.pubMm[i]._ProductCode);
                    insCmd.Parameters.AddWithValue("Last_Trans_date", mm.pubMm[i]._LastTransacDate);
                    insCmd.Parameters.AddWithValue("Current_Inventory", mm.pubMm[i]._InvQuantity);
                    insCmd.Parameters.AddWithValue("Min", mm.pubMm[i]._Min);
                    insCmd.Parameters.AddWithValue("Max", mm.pubMm[i]._Max);
                    insCmd.Parameters.AddWithValue("Store_Code", mm.pubMm[i]._StoreCode);
                    insCmd.ExecuteNonQuery();
                    insCmd.Parameters.Clear();
                    rk.SetValue("index", i);
                    Console.Clear();
                    Console.WriteLine("Records update! Total rows Updated " + (i+1) + " out of " + mm.pubMm.Count + "\n");
                }
            }catch (Exception ex)
            {
                if (ex is OdbcException || ex is InvalidOperationException)
                {
                    Connect();
                    ContinueUpload((int)rk.GetValue("index"));
                }
            }
        }

        //Continue Upload method, will start from the last index saved in the registry subkey
        private void ContinueUpload(int index)
        {
            try
            {
                for (int i = index; i != mm.pubMm.Count; i++)
                {
                    insCmd.Parameters.AddWithValue("Product_Code", mm.pubMm[i]._ProductCode);
                    insCmd.Parameters.AddWithValue("Last_Trans_date", mm.pubMm[i]._LastTransacDate);
                    insCmd.Parameters.AddWithValue("Current_Inventory", mm.pubMm[i]._InvQuantity);
                    insCmd.Parameters.AddWithValue("Min", mm.pubMm[i]._Min);
                    insCmd.Parameters.AddWithValue("Max", mm.pubMm[i]._Max);
                    insCmd.Parameters.AddWithValue("Store_Code", mm.pubMm[i]._StoreCode);
                    insCmd.ExecuteNonQuery();
                    insCmd.Parameters.Clear();
                    rk.SetValue("index", i);
                    Console.Clear();
                    Console.WriteLine("Records update! Total rows Updated " + (i + 1) + " out of " + mm.pubMm.Count + "\n");
                }
            }
            catch (Exception ex)
            {
                
                if (ex is OdbcException || ex is InvalidOperationException)
                {
                    Connect();
                    ContinueUpload((int)rk.GetValue("index"));
                }   
            }
        }

        //Connect / Reconnect method
        public void Connect()
        {
            int flag = 0;

            while (flag == 0)
            {
                try
                {
                    cnn.Open();
                    Console.Write("Connecting!");
                    flag = 1;
                }
                catch (Exception ex)
                {
                    flag = 0;
                }
            }
        }
    }
}
