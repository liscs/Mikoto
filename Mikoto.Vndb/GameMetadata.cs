namespace Mikoto.Vndb
{
    public class GameMetadata
    {
        public string? VndbId { get; set; }

        public string? Title { get; set; }

        public string? NativeTitle { get; set; }

        public string? Description { get; set; }

        public float Score { get; set; }

        public string? CoverUrl { get; set; }

        public string? ReleaseDate { get; set; }

        public string? Developer { get; set; }

        public List<string>? Screenshots { get; set; }
    }
}