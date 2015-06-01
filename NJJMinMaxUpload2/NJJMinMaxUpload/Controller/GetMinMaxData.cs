using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SqlClient;
using NJJMinMaxUpload.Model;
using System.Data;

namespace NJJMinMaxUpload.Controller
{
    public class GetMinMaxData
    {
        private List<SalesQueryData> qs;
        private List<InventoryQueryData> qi;
        DateTime dateDiff;
        private String salesQuery;
        private String invQuery;
        private String scode = "01"; //Change for actual store data

        public GetMinMaxData()
        {
            SqlConnection cnn;
            SqlDataAdapter adp;
            DataSet dataStorage;
            DataTable dt;
            
            //Connection String
            string connetionString = null;
            connetionString = "Server=localhost;Database=AralcoBOS;Integrated Security=SSPI";
                
               /* "Data Source= 10.0.1.6"+
            "Initial Catalog=AralcoBOS;" +
            "User id=eti;" +
            "Password=enigmatech1_2015;";*/
                
            cnn = new SqlConnection(connetionString);
            dataStorage = new DataSet();
            adp = new SqlDataAdapter();

            salesQuery = @"SELECT b.ProductCode,
	            b.Name,
				ISNULL(CAST(e.Code AS varchar), '01') AS SoldFrom,
				ISNULL(CAST(ABS(SUM(a.Quantity))AS INT),0) AS TotalSoldQty,
                ISNULL (CAST(MAX(c.TransacDate) AS varchar),'2015-06-02') AS LastTransacDate
              FROM [AralcoBOS].[dbo].[vwInvProduct] AS b
              LEFT OUTER JOIN [AralcoBOS].[dbo].[vwSaleTransaction] AS a
              ON a.ProductId = b.ProductID
              LEFT OUTER JOIN [AralcoBOS].[dbo].[vwSaleTransactions] AS c
              ON a.POSTransItemID = c.POSTransItemID
              LEFT OUTER JOIN [AralcoBOS].[dbo].Store AS e
              ON a.StoreID = e.StoreID
              GROUP BY b.ProductCode, b.ProductID, b.name, e.code
			  ORDER BY ProductCode";

            invQuery = @"SELECT a1.ProductCode,
		                        CAST(a1.Name AS varchar) AS Name,
                        CAST(SUM(b1.Quantity) AS int) AS TotalInvQty
                        FROM [AralcoBOS].[dbo].[vwInvProduct] AS a1
                        LEFT OUTER JOIN [AralcoBOS].[dbo].[vwInventoryTotals] AS b1
                        ON a1.ProductID = b1.ProductID
                        LEFT OUTER JOIN [AralcoBOS].[dbo].[Store] AS c1
                        ON b1.StoreID = c1.StoreID
                        WHERE c1.code IN ('0001','01')
                        GROUP BY a1.ProductCode, a1.name
                        ORDER BY ProductCode";

            //Gives the date 90 days before today
            dateDiff = DateTime.Now.Subtract(TimeSpan.FromDays(90));

            try
            {
                cnn.Open();
                Console.Write("Connection to Local DatabaseOpen!... \n");
            }
            catch (Exception ex)
            {
                Console.Write(ex);
            }

            adp.SelectCommand = new SqlCommand(salesQuery, cnn);

            Console.Write(" Copying Sales Data from Database!... \n");
            //Assign SQL data to C# Data Table
            adp.Fill(dataStorage, "data");
            dt = dataStorage.Tables["data"];
            try
            {
                //Data Table to List for access
                qs = new List<SalesQueryData>();
                qs = (from DataRow row in dt.Rows
                      select new SalesQueryData
                      {
                          _ProductCode = row["ProductCode"].ToString(),
                          _ProductName = row["Name"].ToString(),
                          _StoreCode = row["SoldFrom"].ToString(),
                          _TotalSoldQty = (int)row["TotalSoldQTY"],
                          _LastTransacDate = row["LastTransacDate"].ToString(),
                      }).ToList();
            }catch(Exception ex)
            {
                Console.Write(ex);
            }

            dataStorage = new DataSet();
            adp = new SqlDataAdapter();

            adp.SelectCommand = new SqlCommand(invQuery, cnn);

            //Assign SQL data to C# Data Table
            adp.Fill(dataStorage, "data");
            dt = dataStorage.Tables["data"];


            Console.Write(" Copying Inventory Data from Database!... \n");
            qi = new List<InventoryQueryData>();
            qi = (from DataRow row in dt.Rows
                  select new InventoryQueryData
                  {
                      _ProductCode = row["ProductCode"].ToString(),
                      _ProductName = row["Name"].ToString(),
                      _TotalInvQty = (int)row["TotalInvQty"],
                  }).ToList();

            cnn.Close();
        }

        //to access qd list publicly
        public List<SalesQueryData> getSalesList
        {
            get { return this.qs; }
        }

        public string getStoreCode
        {
            get {return this.scode;}
        }

        public List<InventoryQueryData> getInvList
        {
            get { return this.qi; }
        }

    }
}
