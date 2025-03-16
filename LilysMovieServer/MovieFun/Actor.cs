using TMDbLib.Objects.General;
using TMDbLib.Objects.Movies;
using TMDbLib.Objects.People;

namespace LilysMovieServer
{
    /// <summary>
    /// Represents an actor with associated movies and scraping functionality.
    /// </summary>
    public class Actor
    {
        private const string imageUrl = "https://image.tmdb.org/t/p/w154";

        /// <summary>
        /// Initializes a new instance of the Actor class using a Person object.
        /// </summary>
        /// <param name="actor">The Person object representing the actor.</param>
        public Actor(Person actor) : this(actor.Id, actor.Name, actor.ProfilePath, null)
        { }

        /// <summary>
        /// Initializes a new instance of the Actor class using a Cast object.
        /// </summary>
        /// <param name="actor">The Cast object representing the actor.</param>
        public Actor(Cast actor) : this(actor.Id, actor.Name, actor.ProfilePath, actor.Character)
        { }

        /// <summary>
        /// Initializes a new instance of the Actor class using a Crew object.
        /// </summary>
        /// <param name="actor">The Crew object representing the actor.</param>
        public Actor(Crew actor) : this(actor.Id, actor.Name, actor.ProfilePath, actor.Job)
        { }

        /// <summary>
        /// Initializes a new instance of the Actor class by copying another Actor object.
        /// </summary>
        /// <param name="actor">The Actor object to copy.</param>
        public Actor(Actor actor)
        {
            Id = actor.Id;
            Name = actor.Name;
            Title = actor.Title;
            Image = actor.Image;

            // Copy the list of movies from the existing actor
            Movies = [.. actor.Movies];

            // Initialize the Scraped list as empty
            Scraped = [];
        }

        /// <summary>
        /// Initializes a new instance of the Actor class with a specified ID.
        /// </summary>
        /// <param name="id">The ID of the actor.</param>
        public Actor(int id) : this(id, null, null, null)
        { }

        /// <summary>
        /// Initializes a new instance of the Actor class with specified details.
        /// </summary>
        /// <param name="id">The ID of the actor.</param>
        /// <param name="name">The name of the actor.</param>
        /// <param name="profilePath">The profile image path of the actor.</param>
        /// <param name="title">The title or role of the actor.</param>
        public Actor(int id, string? name, string? profilePath, string? title)
        {
            Id = id;
            Name = name;
            Title = title;
            Movies = [];
            Scraped = [];

            // If a profile path is provided, construct the full image URL
            if (profilePath != null)
            {
                Image = imageUrl + profilePath;
            }
        }

        /// <summary>
        /// Asynchronously adds movies to the actor's movie list, excluding a specified movie ID.
        /// </summary>
        /// <param name="data">The MovieFunData object containing movie data.</param>
        /// <param name="id">The ID of the movie to exclude.</param>
        /// <returns>A Task representing the asynchronous operation.</returns>
        public Task AddMoviesAsync(MovieFunData data, int id)
        {
            return Task.Run(() =>
            {
                AddMovies(data);

                // Remove the movie with the specified ID from the list
                Movies.RemoveAll(x => x.Id == id);
            });
        }

        /// <summary>
        /// Adds movies to the actor's movie list.
        /// </summary>
        /// <param name="data">The MovieFunData object containing movie data.</param>
        public void AddMovies(MovieFunData data)
        {
            // Check if the actor already exists in the ActorLib
            Actor? foundActor = data.ActorLib.ToList().FirstOrDefault(a => a.Id == Id);
            if (foundActor != null)
            {
                // Copy the list of movies from the found actor
                Name = foundActor.Name;
                Image = foundActor.Image;
                Movies = [.. foundActor.Movies];
                return;
            }

            // Fetch the movie credits for the actor
            Person personObj = data.FetchActor(Id);

            MovieCredits credits = personObj.MovieCredits;
            Name = personObj.Name;
            Image = imageUrl + personObj.ProfilePath;

            // Create tasks to fetch links for cast and crew roles
            Task[] tasks = credits.Cast.Select(i => FetchLinks(data.Scraper, i)).ToArray();
            tasks = [.. tasks, .. credits.Crew.Select(i => FetchLinks(data.Scraper, i))];
            Task.WaitAll(tasks);

            // Group movies by ID, select the first of each group, and order by release date
            Movies.RemoveAll(x => x == null);
            Movies = [..
                    Movies
                    .GroupBy(x => x.Id)
                    .Select(x => x.First())
                    .OrderByDescending(member => member.ReleaseDate)];

            // Add the actor to the ActorLib
            data.ActorLib.Add(new Actor(this));
        }

        /// <summary>
        /// Asynchronously fetches links for a movie role.
        /// </summary>
        /// <param name="scraper">The Cine2Scraper object used for scraping.</param>
        /// <param name="movie">The MovieRole object representing the movie.</param>
        /// <returns>A Task representing the asynchronous operation.</returns>
        public Task FetchLinks(Cine2Scraper scraper, MovieRole movie)
        {
            return Task.Run(() => FetchLinks(scraper, movie.Id, movie.Title, movie.PosterPath, movie.ReleaseDate));
        }

        /// <summary>
        /// Asynchronously fetches links for a movie job.
        /// </summary>
        /// <param name="scraper">The Cine2Scraper object used for scraping.</param>
        /// <param name="movie">The MovieJob object representing the movie.</param>
        /// <returns>A Task representing the asynchronous operation.</returns>
        public Task FetchLinks(Cine2Scraper scraper, MovieJob movie)
        {
            return Task.Run(() => FetchLinks(scraper, movie.Id, movie.Title, movie.PosterPath, movie.ReleaseDate));
        }

        /// <summary>
        /// Fetches links for a movie and adds it to the actor's movie list if found.
        /// </summary>
        /// <param name="scraper">The Cine2Scraper object used for scraping.</param>
        /// <param name="movieId">The ID of the movie.</param>
        /// <param name="name">The name of the movie.</param>
        /// <param name="posterPath">The poster path of the movie.</param>
        /// <param name="releaseDate">The release date of the movie.</param>
        public void FetchLinks(Cine2Scraper scraper, int movieId, string name, string posterPath, DateTime? releaseDate)
        {
            // Check if the movie has already been scraped
            if (Scraped.Contains(movieId))
            { return; }

            // Add the movie ID to the scraped list
            Scraped = Scraped.Append(movieId).ToList();

            // Construct the movie literal for searching
            string movieLiteral = name + (releaseDate == null ? "" : " (" + releaseDate.Value.Year + ")");
            Cine2Response cine2Response = scraper.FindSuggestions(movieLiteral);
            foreach (var link in cine2Response.Suggestions)
            {
                // If a matching suggestion is found, add the movie to the list
                if (link.Title == movieLiteral)
                {
                    Movies.Add(new Film(movieId, name, posterPath, releaseDate));
                    return;
                }
            }
        }

        /// <summary>
        /// Gets or sets the ID of the actor.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets the name of the actor.
        /// </summary>
        public string? Name { get; set; }

        /// <summary>
        /// Gets or sets the title or role of the actor.
        /// </summary>
        public string? Title { get; set; }

        /// <summary>
        /// Gets or sets the profile image URL of the actor.
        /// </summary>
        public string? Image { get; set; }

        /// <summary>
        /// Gets or sets the list of movies associated with the actor.
        /// </summary>
        public List<Film> Movies { get; set; }

        /// <summary>
        /// Gets or sets the list of scraped movie IDs.
        /// </summary>
        public List<int> Scraped { get; set; }
    }
}