using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AssociationRule
{
    public class Sale
    {
        public string TransactionID { get; set; } // ID giao dịch
        public int ProductID { get; set; } // Mã sản phẩm
        public DateTime TransactionDate { get; set; } // Ngày giao dịch
    }

}
