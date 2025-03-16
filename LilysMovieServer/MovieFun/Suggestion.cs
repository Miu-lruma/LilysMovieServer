namespace LilysMovieServer
{
    // The Suggestion class represents a movie suggestion with properties for movie ID, title, and popularity rank
    public class Suggestion
    {
        // Default constructor for the Suggestion class
        public Suggestion()
        { }

        // Property to store the ID of the movie
        // Nullable integer to allow for cases where the movie ID might not be set
        public int? Movie_id { get; set; }

        // Property to store the title of the movie
        // Nullable string to allow for cases where the title might not be set
        public string? Title { get; set; }

        // Property to store the popularity rank of the movie
        // Nullable integer to allow for cases where the popularity rank might not be set
        public int? Popularity_rank { get; set; }
    }
}