using System.ComponentModel.DataAnnotations;

namespace giggreen.Models
{
    public class DhpoPageViewModel
    {
        public string Category { get; set; }
        public string DateRangeFrom { get; set; }
        public string DateRangeTo { get; set; }
        public string SearchFile { get; set; }
        public string SearchPartner { get; set; }
        public string TransactionStatus { get; set; }
        public string Direction { get; set; }
        public List<DHPOViewModel> TableData { get; set; }
    }
    public class DHPOViewModel
    {
        public string FileID { get; set; }
        public string FileName { get; set; }
        public string SenderID { get; set; }

        public string ReceiverID { get; set; }
        public string TransactionDate { get; set; }
        public string RecordCount { get; set; }
        public string IsDownloaded { get; set; }

        public string fromdate { get; set; }


    }

  
}
