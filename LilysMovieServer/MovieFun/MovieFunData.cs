using TMDbLib.Client;
using TMDbLib.Objects.General;
using TMDbLib.Objects.Movies;
using TMDbLib.Objects.People;
using TMDbLib.Objects.Search;

namespace LilysMovieServer
{
    /// <summary>
    /// Class responsible for interacting with the TMDb API and managing movie-related data.
    /// </summary>
    public class MovieFunData
    {
        /// <summary>
        /// List of actors used in the application.
        /// </summary>
        public readonly List<Actor> ActorLib = [];

        /// <summary>
        /// Scraper instance used for fetching additional movie data.
        /// </summary>
        public readonly Cine2Scraper Scraper;

        /// <summary>
        /// TMDb client used for making API requests.
        /// </summary>
        public TMDbClient Client { get; set; }

        /// <summary>
        /// Initializes a new instance of the MovieFunData class.
        /// </summary>
        public MovieFunData()
        {
            // Initialize the TMDb client with the provided API key.
            Client = new TMDbClient("59f3108db8a14fca6a228a8d7c10d611");
            ActorLib = [];
            Scraper = new Cine2Scraper();
        }

        /// <summary>
        /// Fetches a list of movies that include any of the actors in the provided film's cast.
        /// </summary>
        /// <param name="film">The film whose cast members are used to search for related movies.</param>
        /// <returns>A SearchContainer containing the search results.</returns>
        public SearchContainer<SearchMovie> FetchLinks(Film film)
        {
            // Use the TMDb client to discover movies that include any of the cast members from the provided film.
            return Client.DiscoverMoviesAsync()
                         .IncludeWithAnyOfPeople(film?.Cast?.Select(member => member.Id))
                         .IncludeVideoMovies(false)
                         .Query(1)
                         .Result;
        }

        /// <summary>
        /// Fetches detailed information about a movie by its ID.
        /// </summary>
        /// <param name="id">The ID of the movie to fetch.</param>
        /// <param name="methods">Optional methods to include additional data such as credits or images.</param>
        /// <returns>A Movie object containing the movie details.</returns>
        public Movie FetchMovie(int id, MovieMethods methods = MovieMethods.Undefined)
        {
            // Get images like this... MovieMethods.Credits | MovieMethods.Images

            // Use the TMDb client to fetch detailed information about the movie with the specified ID.
            return Client.GetMovieAsync(id, methods).Result;
        }

        /// <summary>
        /// Searches for movies by name and year.
        /// </summary>
        /// <param name="name">The name of the movie to search for.</param>
        /// <param name="year">The release year of the movie to search for.</param>
        /// <returns>A SearchContainer containing the search results.</returns>
        public SearchContainer<SearchMovie> FetchMovieByName(string name, int year)
        {
            // Use the TMDb client to search for movies by name and year.
            return Client.SearchMovieAsync(name, 0, false, 0, null, year).Result;
        }

        /// <summary>
        /// Fetches detailed information about an actor by their ID.
        /// </summary>
        /// <param name="id">The ID of the actor to fetch.</param>
        /// <returns>A Person object containing the actor's details.</returns>
        public Person FetchActor(int id)
        {
            // Use the TMDb client to fetch detailed information about the actor with the specified ID.
            return Client.GetPersonAsync(id, PersonMethods.MovieCredits).Result;
        }
    }
}