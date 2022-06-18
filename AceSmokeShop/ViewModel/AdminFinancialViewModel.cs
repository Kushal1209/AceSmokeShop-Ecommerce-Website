using AceSmokeShop.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace AceSmokeShop.ViewModel
{
    public class AdminFinancialViewModel
    {
        public AdminFinancialViewModel()
        {
            currentBalance = 0;
            TotalRevenue = 0;
            VendorRevenue = 0;
            UserRevenue = 0;
            TaxCollected = 0;

            ListofTransactions = new List<Transactions>();

            TotalTransactions = 0;
            CurrentPage = 1;
            TotalPages = 0;
            ItemsPerPage = 10;
            RowPerPage = new List<int> { 10, 20, 50, 100 };

            Period = 0;

            CardPayment = 0;
            InStorePayment = 0;
        }

        public double currentBalance { get; set; }
        public double TotalRevenue { get; set; }
        public double VendorRevenue { get; set; }
        public double UserRevenue { get; set; }
        public double TaxCollected { get; set; }

        public List<Transactions> ListofTransactions { get; set; }
        public int TotalTransactions { get; set; }
        public int CurrentPage { get; set; }
        public int TotalPages { get; set; }
        public int ItemsPerPage { get; set; }
        public List<int> RowPerPage { get; set; }

        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:dd'/'MM'/'yyyy}", ApplyFormatInEditMode = true)]
        public DateTime DateFrom { get; set; }

        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:dd'/'MM'/'yyyy}", ApplyFormatInEditMode = true)]
        public DateTime DateTo { get; set; }

        public int Period { get; set; }

        public double CardPayment { get; set; }
        public double InStorePayment { get; set; }

        public string yValues { get; set; }
        public string xValues { get; set; }
    }
}
