using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using NJJMinMaxUpload.Controller;
using NJJMinMaxUpload.Model;


namespace NJJMinMaxUpload.Controller
{
    class ComputeMinMax
    {
        private List<MinMax> mm;
        private int min;
        private int max;


        public ComputeMinMax(List<SalesQueryData> qs, List<InventoryQueryData>qi)
        {
            mm = new List<MinMax>();

            foreach(SalesQueryData q in qs)
            {
                max = 0;
                min = 0;

                //min max computation
                min = q._TotalSoldQty / 12;
                //max = min + ((q._TotalSoldQty / 12) * 3); //<--- sample computation of  max = min + 3 days of stocking --->
                max = q._TotalSoldQty / 4; // <--- sample computation of  min + 3 days of stocking c/o Kuya Olive--->

                //Adds minmax value of each product to MinMax list of MinMax object
                mm.Add(new MinMax() { _ProductCode = q._ProductCode, _Name = q._ProductName,_Max = max, _Min = min, _LastTransacDate = q._LastTransacDate, _StoreCode = q._StoreCode});
            }

            for (int i = 0; i < qi.Count; i++)
            {
                if (mm[i]._ProductCode == qi[i]._ProductCode)
                    mm[i]._InvQuantity = qi[i]._TotalInvQty;

                else mm[i]._InvQuantity = "0";
            }

        }

        //to access mm list publicly
        public List<MinMax> pubMm
        {
            get { return this.mm; }
        }

    }

}
