using Microsoft.AspNetCore.Mvc;
using TMDbLib.Objects.General;
using TMDbLib.Objects.Movies;
using TMDbLib.Objects.People;
using TMDbLib.Objects.Search;

namespace LilysMovieServer.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class MovieFunController : ControllerBase
    {
        private readonly ILogger<MovieFunController> _logger;
        private readonly MovieFunData data = new MovieFunData();

        public MovieFunController(ILogger<MovieFunController> logger)
        {
            _logger = logger;
        }

        // ***************************************
        /// <summary>
        /// Retrieves a movie by its name and optional year.
        /// </summary>
        /// <param name="nameYear">The name of the movie and optionally the year in the format "Name (Year)".</param>
        /// <returns>A Film object containing movie details.</returns>
        [HttpGet("GetMovieByName", Name = "GetMovieByName")]
        public ActionResult<Film> GetMovieByName(string nameYear)
        {
            // Extract the year from the nameYear string if it exists
            int pClose = nameYear.LastIndexOf(')');
            string name = nameYear;
            string year = "";

            if (pClose >= 0)
            {
                int pOpen = nameYear.LastIndexOf('(', pClose);

                if (pOpen >= 0)
                {
                    year = nameYear.Substring(pOpen + 1, pClose - pOpen - 1);
                    name = nameYear[..(pOpen - 1)];
                }
            }

            // Parse the year to an integer, default to 0 if not provided
            int intYear = year == "" ? 0 : Int32.Parse(year);

            // Fetch movie details by name and year
            SearchContainer<SearchMovie> results = data.FetchMovieByName(name, intYear);

            // Return an empty Film object if no results are found
            if (results.TotalResults < 1)
            { return new Film(); }

            // Return the first movie result with its credits
            return new Film(data.FetchMovie(results.Results[0].Id, MovieMethods.Credits));
        }

        // ***************************************
        /// <summary>
        /// Retrieves links for an actor excluding a specific movie.
        /// </summary>
        /// <param name="actorId">The ID of the actor.</param>
        /// <param name="movieId">The ID of the movie to exclude.</param>
        /// <returns>An Actor object with movie links.</returns>
        [HttpGet("GetLinksForActor", Name = "GetLinksForActor")]
        public ActionResult<Actor> GetLinksForActor(int actorId, int movieId)
        {
            // Create an Actor object and add movies to it
            Actor actor = new(actorId);
            actor.AddMovies(data);

            // Remove the specified movie from the actor's movie list
            actor.Movies.RemoveAll(x => x.Id == movieId);

            return actor;
        }

        /// <summary>
        /// Retrieves links for a movie by its name and optional year.
        /// </summary>
        /// <param name="nameYear">The name of the movie and optionally the year in the format "Name (Year)".</param>
        /// <returns>A Film object containing movie details and links.</returns>
        [HttpGet("GetLinksByName", Name = "GetLinksByName")]
        public ActionResult<Film> GetLinksByName(string nameYear)
        {
            // Extract the year from the nameYear string if it exists
            int pClose = nameYear.LastIndexOf(')');
            string name = nameYear;
            string year = "";

            if (pClose >= 0)
            {
                int pOpen = nameYear.LastIndexOf('(', pClose);

                if (pOpen >= 0)
                {
                    year = nameYear.Substring(pOpen + 1, pClose - pOpen - 1);
                    name = nameYear[..(pOpen - 1)];
                }
            }

            // Parse the year to an integer, default to 0 if not provided
            int intYear = year == "" ? 0 : Int32.Parse(year);

            // Fetch movie details by name and year
            SearchContainer<SearchMovie> results = data.FetchMovieByName(name, intYear);

            // Return an empty Film object if no results are found
            if (results.TotalResults < 1)
            { return new Film(); }

            // Return the movie links for the first result
            return GetLinks(results.Results[0].Id);
        }

        /// <summary>
        /// Retrieves links for a movie by its ID.
        /// </summary>
        /// <param name="id">The ID of the movie.</param>
        /// <returns>A Film object containing movie details and links.</returns>
        [HttpGet("GetLinks", Name = "GetLinksById")]
        public ActionResult<Film> GetLinks(int id)
        {
            // Fetch movie details and credits
            Film film = new(data.FetchMovie(id, MovieMethods.Credits));
            if (film.Cast != null)
            {
                // Add movies to each actor in the cast asynchronously
                Task[] tasks = film.Cast.Select(actor => actor.AddMoviesAsync(data, id)).ToArray();
                tasks = [.. tasks, .. film.Cast.Select(actor => actor.AddMoviesAsync(data, id))];
                Task.WaitAll(tasks);

                // Remove actors with no movies
                film.Cast.RemoveAll(actor => actor.Movies.Count < 1);
            }

            return film;
        }

        // ***************************************
        /// <summary>
        /// Retrieves a movie by its ID.
        /// </summary>
        /// <param name="movieId">The ID of the movie.</param>
        /// <returns>A Film object containing movie details.</returns>
        [HttpGet("GetMovie", Name = "GetMovieById")]
        public ActionResult<Film> GetMovie(int movieId, int actorId)
        {
            // Fetch and return movie details and credits
            Film film = new Film(data.FetchMovie(movieId, MovieMethods.Credits));
            film.Cast?.RemoveAll(x => x.Id == actorId);
            return film;
        }

        // ***************************************
        /// <summary>
        /// Retrieves movie suggestions based on a search query.
        /// </summary>
        /// <param name="search">The search query.</param>
        /// <returns>A Cine2Response object containing suggestions.</returns>
        [HttpGet("GetSuggestions", Name = "GetSuggestions")]
        public ActionResult<Cine2Response> GetSuggestions(string search)
        {
            // Fetch and return movie suggestions
            return data.Scraper.FindSuggestions(search);
        }

        /// <summary>
        /// Debug method to retrieve a specific movie.
        /// </summary>
        /// <returns>A Movie object containing movie details.</returns>
        [HttpGet("DebugMovie", Name = "DebugMovie")]
        public ActionResult<Movie> DebugMovie()
        {
            // Fetch and return a specific movie for debugging
            return data.FetchMovie(10603, MovieMethods.Credits);
        }

        /// <summary>
        /// Retrieves an actor by their ID.
        /// </summary>
        /// <param name="id">The ID of the actor.</param>
        /// <returns>An Actor object containing actor details.</returns>
        [HttpGet("GetActor", Name = "GetActorById")]
        public ActionResult<Actor> GetActor(int id)
        {
            // Fetch and return actor details
            return new Actor(data.FetchActor(id));
        }

        /// <summary>
        /// Debug method to retrieve a specific actor.
        /// </summary>
        /// <returns>A Person object containing actor details.</returns>
        [HttpGet("DebugPerson", Name = "DebugPerson")]
        public ActionResult<Person> DebugPerson()
        {
            // Fetch and return a specific actor for debugging
            return data.FetchActor(10980);
        }
    }
}