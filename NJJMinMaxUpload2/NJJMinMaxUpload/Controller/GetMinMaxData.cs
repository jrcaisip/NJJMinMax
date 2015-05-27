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
            connetionString = "Server=localhost;Database=AralcoBOS_Dummy;Integrated Security=SSPI";
                
                /*"Data Source= DUMMYSERVER"+
            "Initial Catalog=AralcoBOS_Dummy;" +
            "User id=eti;" +
            "Password=enigmatech1_2015;";*/

            cnn = new SqlConnection(connetionString);
            dataStorage = new DataSet();
            adp = new SqlDataAdapter();

            salesQuery = @"SELECT b.ProductCode,
	            b.Name,
				ISNULL( CAST(e.Code AS varchar), '103') AS SoldFrom,
				ISNULL(CAST(ABS(SUM(a.Quantity))AS INT),0) AS TotalSoldQty,
                ISNULL (CAST(MAX(c.TransacDate) AS varchar),'2015-06-02') AS LastTransacDate
              FROM [AralcOBOS_Dummy].[dbo].[vwInvProduct] AS b
              LEFT OUTER JOIN [AralcOBOS_Dummy].[dbo].[vwSaleTransaction] AS a
              ON a.ProductId = b.ProductID
              LEFT OUTER JOIN [AralcOBOS_Dummy].[dbo].[vwSaleTransactions] AS c
              ON a.POSTransItemID = c.POSTransItemID
              LEFT OUTER JOIN [AralcOBOS_Dummy].[dbo].Store AS e
              ON a.StoreID = e.StoreID
              GROUP BY b.ProductCode, b.ProductID, b.name, e.code
			  ORDER BY ProductCode";

            invQuery = @"SELECT a1.ProductCode,
		                        a1.Name,
                        CAST(SUM(b1.Quantity) AS int) AS TotalInvQty
                        FROM [AralcoBOS_Dummy].[dbo].[vwInvProduct] AS a1
                        LEFT OUTER JOIN [AralcoBOS_Dummy].[dbo].[vwInventoryTotals] AS b1
                        ON a1.ProductID = b1.ProductID
                        LEFT OUTER JOIN [AralcoBOS_Dummy].[dbo].[Store] AS c1
                        ON b1.StoreID = c1.StoreID
                        WHERE c1.code IN ('101','102','103')
                        GROUP BY a1.ProductCode, a1.name
                        ORDER BY ProductCode";

            //Gives the date 90 days before today
            dateDiff = DateTime.Now.Subtract(TimeSpan.FromDays(90));

            try
            {
                cnn.Open();
                Console.Write("Connection to Local DatabaseOpen ! \n");
            }
            catch (Exception ex)
            {
                Console.Write(ex);
            }

            adp.SelectCommand = new SqlCommand(salesQuery, cnn);

            //Assign SQL data to C# Data Table
            adp.Fill(dataStorage, "data");
            dt = dataStorage.Tables["data"];

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

            dataStorage = new DataSet();
            adp = new SqlDataAdapter();

            adp.SelectCommand = new SqlCommand(invQuery, cnn);

            //Assign SQL data to C# Data Table
            adp.Fill(dataStorage, "data");
            dt = dataStorage.Tables["data"];

            qi = new List<InventoryQueryData>();
            qi = (from DataRow row in dt.Rows
                  select new InventoryQueryData
                  {
                      _ProductCode = row["ProductCode"].ToString(),
                      _ProductName = row["Name"].ToString(),
                      _TotalInvQty = row["TotalInvQty"].ToString(),
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
