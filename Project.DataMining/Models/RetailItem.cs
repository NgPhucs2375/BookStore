// use to store the retail item data from the csv file
using CsvHelper.Configuration.Attributes;

namespace Project.DataMining.Models
{
    // Class dung de map du lieu tho tu file csv
    public class RawRetailItem
    {
        [Name("InvoiceNo")] public string InvoiceNo {get;set;} = string.Empty;
        [Name("StockCode")] public string StockCode {get;set;} = string.Empty;
        [Name("Description")] public string Description {get;set;} = string.Empty;
        [Name("Quantity")] public int Quantity {get;set;}
        [Name("InvoiceDate")] public string InvoiceDate {get;set;} = string.Empty;
        [Name("UnitPrice")] public double UnitPrice {get;set;}
        [Name("CustomerID")] public string CustomerID {get;set;} = string.Empty;
        [Name("Country")] public string Country {get;set;} = string.Empty;

    }

    // class dai dien cho 1 hoa don sau khi da duoc gom nhoms (Basket)
    public class TransactionBasket
    {
        public string InvoiceNo {get;set;} = string.Empty;
        public List<string> Items {get;set;} = new List<string>();
    }
}