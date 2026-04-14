using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using CsvHelper;
using CsvHelper.Configuration;
using Project.DataMining.Models; 

namespace Project.DataMining.Algorithms
{
    public class DataCleaner
    {
        public List<TransactionBasket> ProcessRetailData(string csvFilePath)
        {
            Console.WriteLine("===========================================");
            Console.WriteLine("[*] KHỞI ĐỘNG QUY TRÌNH LÀM SẠCH DỮ LIỆU...");
            Console.WriteLine("===========================================\n");

            var rawData = new List<RawRetailItem>();

            // 1. Đọc dữ liệu từ file CSV
            Console.WriteLine("[+] Bước 1: Đọc dữ liệu từ file CSV...");
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
            Console.WriteLine($"    -> Đã load thành công: {rawData.Count} dòng dữ liệu thô.\n");

            // 2. Làm sạch dữ liệu (Data Cleaning)
            Console.WriteLine("[+] Bước 2: Kích hoạt bộ lọc dữ liệu nhiễu/rác...");
            var cleanedData = rawData
                .Where(x => !string.IsNullOrWhiteSpace(x.InvoiceNo))
                // Lọc Business Rules
                .Where(x => !x.InvoiceNo.StartsWith("C")) // Bỏ hóa đơn bị hủy
                .Where(x => x.Quantity > 0)               // Số lượng phải > 0
                .Where(x => x.UnitPrice > 0)              // Giá phải > 0
                // Lọc Missing Values
                .Where(x => !string.IsNullOrWhiteSpace(x.Description)) // Bỏ tên rỗng
                // Lọc Noise (Rác) - Thêm BANK CHARGES vào để bắt cái dữ liệu ảo nãy mình thêm
                .Where(x => x.StockCode != "POST" && x.StockCode != "M" && x.StockCode != "BANK CHARGES") 
                .Select(x => new
                {
                    InvoiceNo = x.InvoiceNo.Trim(),
                    // Làm sạch tên sản phẩm: Bỏ khoảng trắng thừa, in hoa để đồng bộ
                    Product = x.Description.Trim().ToUpper() 
                })
                .Distinct() // Lọc Deduplication: Loại bỏ các dòng sản phẩm bị lặp lại trong cùng 1 hóa đơn
                .ToList();

            int totalCleanedRows = cleanedData.Count;
            Console.WriteLine($"    -> Đã quét và phát hiện/loại bỏ: {rawData.Count - totalCleanedRows} dòng rác/lỗi.");
            Console.WriteLine($"    -> Số dòng hợp lệ giữ lại: {totalCleanedRows}\n");

            // 3. Gom nhóm dữ liệu thành các "Giỏ hàng" (Baskets)
            Console.WriteLine("[+] Bước 3: Gom nhóm (Data Transformation)...");
            var transactions = cleanedData
                .GroupBy(x => x.InvoiceNo)
                .Select(g => new TransactionBasket
                {
                    InvoiceNo = g.Key,
                    Items = g.Select(x => x.Product).ToList()
                })
                .Where(b => b.Items.Count > 1) // CHÚ Ý: Chỉ lấy hóa đơn có từ 2 mặt hàng trở lên
                .ToList();

            Console.WriteLine($"    -> Đã chuyển đổi thành công {transactions.Count} giỏ hàng (Basket) hợp lệ (>1 item).");
            Console.WriteLine("\n===========================================");
            Console.WriteLine("[*] HOÀN TẤT TIỀN XỬ LÝ! SẴN SÀNG CHẠY APRIORI.");
            Console.WriteLine("===========================================\n");

            return transactions;
        }
    }
}