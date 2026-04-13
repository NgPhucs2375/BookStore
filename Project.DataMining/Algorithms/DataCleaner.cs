using CsvHelper;
using CsvHelper.Configuration;
using System.Globalization;
using Project.DataMining.Models;

namespace Project.DataMining
{
    public class DataCleaner
    {
        public List<TransactionBasket> ProcessRetailData(string csvFilePath)
        {
            var rawData = new List<RawRetailItem>();

            // 1. Đọc dữ liệu từ file CSV
            var config = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                HasHeaderRecord = true,
                MissingFieldFound = null,
                BadDataFound = null // Bỏ qua các dòng lỗi format thay vì crash app
            };

            using (var reader = new StreamReader(csvFilePath))
            using (var csv = new CsvReader(reader, config))
            {
                rawData = csv.GetRecords<RawRetailItem>().ToList();
            }

            // 2. Làm sạch dữ liệu (Data Cleaning)
            var cleanedData = rawData
                .Where(x => !string.IsNullOrWhiteSpace(x.InvoiceNo))
                .Where(x => !x.InvoiceNo.StartsWith("C")) // Bỏ hóa đơn bị hủy
                .Where(x => x.Quantity > 0)               // Số lượng phải > 0
                .Where(x => x.UnitPrice > 0)              // Giá phải > 0
                .Where(x => !string.IsNullOrWhiteSpace(x.Description)) // Bỏ tên rỗng
                .Where(x => x.StockCode != "POST" && x.StockCode != "M") // Bỏ phí ship
                .Select(x => new
                {
                    InvoiceNo = x.InvoiceNo.Trim(),
                    // Làm sạch tên sản phẩm: Bỏ khoảng trắng thừa, in hoa để tránh phân biệt
                    Product = x.Description.Trim().ToUpper() 
                })
                .Distinct() // Loại bỏ các dòng sản phẩm bị lặp lại trong cùng 1 hóa đơn
                .ToList();

            // 3. Gom nhóm dữ liệu thành các "Giỏ hàng" (Baskets)
            // Nhóm tất cả các sản phẩm có cùng chung 1 mã InvoiceNo
            var transactions = cleanedData
                .GroupBy(x => x.InvoiceNo)
                .Select(g => new TransactionBasket
                {
                    InvoiceNo = g.Key,
                    Items = g.Select(x => x.Product).ToList()
                })
                .Where(b => b.Items.Count > 1) // CHÚ Ý: Chỉ lấy hóa đơn có từ 2 mặt hàng trở lên mới tạo được quy tắc mua kèm
                .ToList();

            return transactions;
        }
    }
}