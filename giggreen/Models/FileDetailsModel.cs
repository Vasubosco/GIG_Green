namespace giggreen.Models
{
    public class FileDetailsModel
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
