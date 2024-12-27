namespace splitQuickNotes
{
    public class QuickNote (Guid? guid, DateTime utc, string? title, string content)
    {
        public Guid? Guid { get; set; } = guid;

        public DateTime Utc { get; set; } = utc;

        public string? Title { get; set; } = title;

        public string Content { get; set; } = content;
    }
}
