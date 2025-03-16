using TMDbLib.Objects.Movies;
using TMDbLib.Objects.People;
using TMDbLib.Objects.Search;

namespace LilysMovieServer
{
    /// <summary>
    /// Represents a film with its details and cast information.
    /// </summary>
    public class Film
    {
        /// <summary>
        /// Default constructor initializing default values.
        /// </summary>
        public Film()
        {
            Id = 0;
            Name = "";
            ReleaseDate = DateTime.UnixEpoch;
            ReleaseYear = 0;
        }

        /// <summary>
        /// Constructor initializing a Film object from a Movie object.
        /// </summary>
        /// <param name="movie">The Movie object to initialize from.</param>
        public Film(Movie movie) : this(movie.Id, movie.Title, movie.PosterPath, movie.ReleaseDate, movie.Credits)
        { }

        /// <summary>
        /// Constructor initializing a Film object from a MovieJob object.
        /// </summary>
        /// <param name="movie">The MovieJob object to initialize from.</param>
        public Film(MovieJob movie) : this(movie.Id, movie.Title, movie.PosterPath, movie.ReleaseDate)
        { }

        /// <summary>
        /// Constructor initializing a Film object from a MovieRole object.
        /// </summary>
        /// <param name="movie">The MovieRole object to initialize from.</param>
        public Film(MovieRole movie) : this(movie.Id, movie.Title, movie.PosterPath, movie.ReleaseDate)
        { }

        /// <summary>
        /// Constructor initializing a Film object from a SearchMovie object.
        /// </summary>
        /// <param name="movie">The SearchMovie object to initialize from.</param>
        public Film(SearchMovie movie) : this(movie.Id, movie.Title, movie.PosterPath, movie.ReleaseDate)
        { }

        /// <summary>
        /// Constructor initializing a Film object with basic details.
        /// </summary>
        /// <param name="id">The ID of the film.</param>
        /// <param name="name">The name of the film.</param>
        /// <param name="posterPath">The path to the film's poster image.</param>
        /// <param name="releaseDate">The release date of the film.</param>
        public Film(int id, string name, string posterPath, DateTime? releaseDate) : this(id, name, posterPath, releaseDate, null)
        { }

        /// <summary>
        /// Constructor initializing a Film object with detailed information.
        /// </summary>
        /// <param name="id">The ID of the film.</param>
        /// <param name="name">The name of the film.</param>
        /// <param name="posterPath">The path to the film's poster image.</param>
        /// <param name="releaseDate">The release date of the film.</param>
        /// <param name="credits">The credits information of the film.</param>
        public Film(int id, string name, string posterPath, DateTime? releaseDate, Credits? credits)
        {
            Id = id;
            Name = name;
            ReleaseDate = releaseDate;
            ReleaseYear = releaseDate?.Year ?? 0;

            // Set the image URL if the poster path is provided
            if (posterPath != null)
            {
                Image = "https://image.tmdb.org/t/p/w154" + posterPath;
            }

            // If credits are not provided, return early
            if (credits == null)
            {
                return;
            }

            // Filter and order crew members by specific jobs and popularity
            Cast = credits.Crew
                .Where(member =>
                    (member.Job == "Director") ||
                    (member.Job == "Screenplay") ||
                    (member.Job == "Original Music Composer") ||
                    (member.Job == "Director of Photography") ||
                    (member.Job == "Novel"))
                .OrderByDescending(member => member.Popularity)
                .Select(member => new Actor(member))

            // Concatenate the cast members, order by popularity, and select distinct members by ID
            .Concat(
               credits.Cast

                //.Where(member =>
                //    !member.Character.Contains("(uncredited)"))
                .OrderByDescending(member => member.Popularity)
                .Select(member => new Actor(member)))
            .DistinctBy(member => member.Id)

            // Take the top 45 members and convert to a list
            //.Take(45)
            .ToList();
        }

        /// <summary>
        /// Gets or sets the ID of the film.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets the name of the film.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the image URL of the film.
        /// </summary>
        public string? Image { get; set; }

        /// <summary>
        /// Gets or sets the release date of the film.
        /// </summary>
        public DateTime? ReleaseDate { get; set; }

        /// <summary>
        /// Gets or sets the release year of the film.
        /// </summary>
        public int ReleaseYear { get; set; }

        /// <summary>
        /// Gets or sets the cast of the film.
        /// </summary>
        public List<Actor>? Cast { get; set; }
    }
}