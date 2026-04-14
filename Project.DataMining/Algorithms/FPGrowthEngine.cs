using System;
using System.Collections.Generic;
using System.Linq;
using Project.DataMining.Models;

namespace Project.DataMining.Algorithms
{
    // Cấu trúc Node đặc thù của cây FP-Tree
    public class FPTreeNode
    {
        public string Item { get; set; }
        public int Count { get; set; }
        public FPTreeNode Parent { get; set; }
        public Dictionary<string, FPTreeNode> Children { get; set; } = new Dictionary<string, FPTreeNode>();

        public FPTreeNode(string item, FPTreeNode parent)
        {
            Item = item;
            Count = 1;
            Parent = parent;
        }
    }

    public class FPGrowthEngine
    {
        private double _minSupport;

        public FPGrowthEngine(double minSupport)
        {
            _minSupport = minSupport;
        }

        public void Run(List<TransactionBasket> transactions)
        {
            Console.WriteLine("\n===========================================");
            Console.WriteLine("[*] KHỞI CHẠY THUẬT TOÁN FP-GROWTH...");
            Console.WriteLine("===========================================\n");

            // Tính số lượng giao dịch tối thiểu để đạt Min Support
            int minSupportCount = (int)Math.Ceiling(_minSupport * transactions.Count);
            Console.WriteLine($"[+] Hệ số Min Support: {_minSupport * 100}% (Yêu cầu xuất hiện trong ít nhất {minSupportCount} đơn hàng)");

            // BƯỚC 1: Đếm tần suất các mặt hàng (Tạo Header Table)
            var itemFrequencies = new Dictionary<string, int>();
            foreach (var basket in transactions)
            {
                foreach (var item in basket.Items)
                {
                    if (!itemFrequencies.ContainsKey(item))
                        itemFrequencies[item] = 0;
                    itemFrequencies[item]++;
                }
            }

            // BƯỚC 2: Lọc bỏ các item không phổ biến và sắp xếp giảm dần theo tần suất
            var frequentItems = itemFrequencies
                .Where(x => x.Value >= minSupportCount)
                .OrderByDescending(x => x.Value)
                .Select(x => x.Key)
                .ToList();

            Console.WriteLine($"[+] Tìm thấy {frequentItems.Count} mặt hàng phổ biến.");

            // BƯỚC 3: Xây dựng cấu trúc cây FP-Tree
            Console.WriteLine("[+] Đang xây dựng cấu trúc FP-Tree trong bộ nhớ (Memory)...");
            FPTreeNode root = new FPTreeNode("Null", null); // Gốc của cây

            foreach (var basket in transactions)
            {
                // Lọc và sắp xếp item trong giỏ hàng theo thứ tự xuất hiện nhiều nhất
                var orderedItems = basket.Items
                    .Where(item => frequentItems.Contains(item))
                    .OrderByDescending(item => itemFrequencies[item])
                    .ToList();

                // Chèn vào cây
                if (orderedItems.Count > 0)
                {
                    InsertTree(orderedItems, root);
                }
            }

            Console.WriteLine("[+] Xây dựng FP-Tree hoàn tất! Tốc độ tối ưu hơn Apriori (Không sinh tập ứng viên).");
            Console.WriteLine("[+] Sẵn sàng khai thác luật kết hợp từ FP-Tree...\n");
        }

        // Hàm đệ quy để chèn các mặt hàng vào nhánh cây
        private void InsertTree(List<string> items, FPTreeNode node)
        {
            string firstItem = items.First();
            
            // Nếu nhánh đã tồn tại, tăng biến đếm
            if (node.Children.ContainsKey(firstItem))
            {
                node.Children[firstItem].Count++;
            }
            // Nếu chưa có, tạo node mới
            else
            {
                node.Children[firstItem] = new FPTreeNode(firstItem, node);
            }

            // Đệ quy chèn các item còn lại
            if (items.Count > 1)
            {
                InsertTree(items.Skip(1).ToList(), node.Children[firstItem]);
            }
        }
    }
}