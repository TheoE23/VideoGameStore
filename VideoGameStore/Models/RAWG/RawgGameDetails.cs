namespace VideoGameStore.Models.Rawg
{
    public class RawgGameDetails
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;

        public string? Background_Image { get; set; }

        public List<RawgNamedEntity> Genres { get; set; } = new();
        public List<RawgNamedEntity> Developers { get; set; } = new();
    }
}
