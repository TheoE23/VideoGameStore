namespace VideoGameStore.Models.Rawg
{
    public class RawgGameDetails
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;

        public List<RawgNamedEntity> Genres { get; set; } = new();
        public List<RawgNamedEntity> Developers { get; set; } = new();
    }
}
