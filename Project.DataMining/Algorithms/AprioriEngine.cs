using Project.DataMining.Models;

namespace Project.DataMining
{
    public class AprioriEngine
    {
        public List<AssociationRule> GenerateRules(
            List<TransactionBasket> baskets, 
            double minSupport, 
            double minConfidence)
        {
            int totalBaskets = baskets.Count;
            int minSupportCount = (int)(minSupport * totalBaskets);

            // 1. Đếm tần suất của từng sản phẩm riêng lẻ (1-itemsets)
            var itemCounts = new Dictionary<string, int>();
            foreach (var basket in baskets)
            {
                foreach (var item in basket.Items)
                {
                    if (!itemCounts.ContainsKey(item)) itemCounts[item] = 0;
                    itemCounts[item]++;
                }
            }

            // Lọc ra các sản phẩm thỏa mãn minSupport
            var frequentItems = itemCounts
                .Where(x => x.Value >= minSupportCount)
                .Select(x => x.Key)
                .ToList();

            // 2. Tìm các cặp sản phẩm (2-itemsets) và đếm tần suất xuất hiện cùng nhau
            var pairCounts = new Dictionary<string, int>();
            
            foreach (var basket in baskets)
            {
                // Chỉ xét các sản phẩm đã phổ biến để giảm tải tính toán
                var itemsInBasket = basket.Items.Intersect(frequentItems).OrderBy(x => x).ToList();
                
                for (int i = 0; i < itemsInBasket.Count; i++)
                {
                    for (int j = i + 1; j < itemsInBasket.Count; j++)
                    {
                        string pairKey = $"{itemsInBasket[i]}|{itemsInBasket[j]}";
                        if (!pairCounts.ContainsKey(pairKey)) pairCounts[pairKey] = 0;
                        pairCounts[pairKey]++;
                    }
                }
            }

            // 3. Tạo các luật kết hợp (Association Rules) và tính toán các chỉ số
            var rules = new List<AssociationRule>();

            foreach (var pair in pairCounts)
            {
                // Bỏ qua nếu cặp sản phẩm không đạt minSupport
                if (pair.Value < minSupportCount) continue;

                var items = pair.Key.Split('|');
                string itemA = items[0];
                string itemB = items[1];

                double supportBoth = (double)pair.Value / totalBaskets;
                double supportA = (double)itemCounts[itemA] / totalBaskets;
                double supportB = (double)itemCounts[itemB] / totalBaskets;

                // Luật: Mua A -> Gợi ý B
                double confidenceAtoB = supportBoth / supportA;
                if (confidenceAtoB >= minConfidence)
                {
                    rules.Add(new AssociationRule
                    {
                        ProductA = itemA,
                        ProductB = itemB,
                        Support = supportBoth,
                        Confidence = confidenceAtoB,
                        Lift = confidenceAtoB / supportB
                    });
                }

                // Luật: Mua B -> Gợi ý A
                double confidenceBtoA = supportBoth / supportB;
                if (confidenceBtoA >= minConfidence)
                {
                    rules.Add(new AssociationRule
                    {
                        ProductA = itemB,
                        ProductB = itemA,
                        Support = supportBoth,
                        Confidence = confidenceBtoA,
                        Lift = confidenceBtoA / supportA
                    });
                }
            }

            // Sắp xếp các luật ưu tiên theo Lift (độ liên quan cao nhất) và Confidence
            return rules
                .Where(r => r.Lift > 1.0) // Bắt buộc Lift > 1 (có ý nghĩa tương quan dương)
                .OrderByDescending(r => r.Lift)
                .ThenByDescending(r => r.Confidence)
                .ToList();
        }
    }
}