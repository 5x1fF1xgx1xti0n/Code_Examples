namespace TVC.ImageServer.ImageManipulation.Models
{
    public class MergeModel
    {
        public byte[] Left { get; set; }
        public byte[] Right { get; set; }
        public byte[] Background { get; set; }
        public string Text { get; set; }
    }
}
