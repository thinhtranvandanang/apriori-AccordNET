using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Accord.MachineLearning.Rules;
using System.Data.SqlClient;
using AssociationRule;
using System.Data;
using System.Diagnostics; // Đo thời gian xử lý của Apriori
 
namespace AccordNET
{
    internal class Program
    {
        static string connectionString = @"Data Source=.;Initial Catalog=CSDLBanHangWsmart;Integrated Security=True";
        static string tableFullData = "SalesMonth"; // 16267, 10%=16267, 20%=3255
        static string tableTestData = "Sales"; // 274, 10%=27, 20%=55
        static void Main(string[] args)
        {
            List<Sale> sales = new List<Sale>(); // danh sách - bản sao từ CSDL
            sales=ReadFromSQLServerToList(tableTestData); // Bản sao của table trong CSDL
            var dataset = ConvertToAprioriFormat(sales);  // Xây dựng mô hình Apriori
                Stopwatch stopwatch = new Stopwatch(); // Khởi tạo Stopwatch
                stopwatch.Start();  // Bắt đầu đồng hồ
            BuildAprioriModel(dataset);
                stopwatch.Stop(); // Dừng đồng hồ
                Console.WriteLine($"Thời gian thực hiện: {stopwatch.Elapsed.TotalSeconds} giây"); // Hiển thị thời gian thực hiện
            Console.ReadKey();
        }
        public static void BuildAprioriModel(int[][] dataset)
        {
            var apriori = new Accord.MachineLearning.Rules.Apriori<int>(threshold: 2, confidence: 0.5);
            //var apriori = new Accord.MachineLearning.Rules.Apriori<int>(threshold: 10, confidence: 0.7);
            var classifier = apriori.Learn(dataset);
            // classifier sẽ có kiểu AssociationRuleMatcher<int>.
            // Đối tượng này chứa các quy tắc kết hợp đã được học từ tập dữ liệu.
            // có thể sử dụng classifier này để tìm các quy tắc hoặc
            // đưa ra dự đoán dựa trên dữ liệu mới.
            var rules = classifier.Rules;
            foreach (var rule in rules)
            {
                // Hiển thị các mục trong phần điều kiện của quy tắc
                var antecedentNames = rule.X.Select(item => productNames(item)).ToArray();
                // Hiển thị các mục trong phần kết quả của quy tắc
                var consequentNames = rule.Y.Select(item => productNames(item)).ToArray();

                Console.WriteLine($"Nếu mua: {string.Join(", ", antecedentNames)} thì cũng mua ==> : {string.Join(", ", consequentNames)} với độ tin cậy: {rule.Confidence}");
            }
        }
        private static List<Sale> ReadFromSQLServerToList(string NameTableCSDL)
        { // kết nối đến csdl, đọc toàn bộ table vào 1 danh sách sale kiểu - list
          // sale này, giống y bảng trong csdl
            List<Sale> tmpKQ = new List<Sale>(); // danh sách - bản sao từ CSDL
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                string query = "SELECT TransactionID, ProductID, TransactionDate FROM " + NameTableCSDL; // Thay đổi tên bảng và cột nếu cần
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    DateTime GDNgay;
                    string GDMa;
                    int GDMaHH;
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            GDNgay = Convert.ToDateTime(reader["TransactionDate"]);
                            GDMa = Convert.ToString(reader["TransactionID"]);
                            GDMaHH = Convert.ToInt32(reader["ProductID"]);
                            tmpKQ.Add(new Sale
                            {
                                TransactionID = GDMa,
                                ProductID = GDMaHH,
                                TransactionDate = GDNgay
                            });
                        }
                    }
                }
            }
            return tmpKQ;
        }
        public static int[][] ConvertToAprioriFormat(List<Sale> transactions)
        {
            var groupedTransactions = new Dictionary<string, List<int>>();
            // nhóm theo các đơn hàng dựa trên mã đơn hàng
            foreach (var transaction in transactions)
            {
                if (!groupedTransactions.ContainsKey(transaction.TransactionID))
                {
                    groupedTransactions[transaction.TransactionID] = new List<int>();
                }
                groupedTransactions[transaction.TransactionID].Add(transaction.ProductID);
            }

            var dataset = new List<int[]>(); // các hàng, mổi hàng là một danh sách các cột là các mã hàng hóa của đơn hàng
            foreach (var pt in groupedTransactions.Values)
            {
                dataset.Add(pt.ToArray()); // chuyển danh sách list int thành mảng int
            }

            return dataset.ToArray(); // chuyển danh sách list thành mảng 
        }
       
        public static string productNames(int maHH)
        { // luật không nhiều, nên không cần đọc tất cả vào bộ nhớ.
          // truy xuất csdl để lấy tên hàng hóa khi cần
            string tenHangHoa = string.Empty;

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                string query = "SELECT TENHH FROM HangHoa WHERE ProductID = @MaHangHoa";

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@MaHangHoa", maHH);

                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            tenHangHoa = reader["TENHH"].ToString();
                        }
                    }
                }
            }
            return tenHangHoa;
        }

    

    }
}
