namespace Project.DataMining.Models
{
    public class AssociationRule
    {
        public string ProductA { get; set; } = string.Empty; // Sản phẩm khách đang xem
        public string ProductB { get; set; } = string.Empty; // Sản phẩm gợi ý mua kèm
        
        // Các chỉ số đánh giá độ mạnh của luật
        public double Support { get; set; }    // Tỷ lệ xuất hiện của cả A và B trong toàn bộ hóa đơn
        public double Confidence { get; set; } // Xác suất khách mua B khi đã mua A
        public double Lift { get; set; }       // Độ cải thiện (Lift > 1 nghĩa là A và B thực sự liên quan)
    }
}