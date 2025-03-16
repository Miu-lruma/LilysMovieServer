namespace LilysMovieServer
{
    /// <summary>
    /// Represents the response from the Cine2 service.
    /// </summary>
    public class Cine2Response
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Cine2Response"/> class.
        /// </summary>
        public Cine2Response()
        {
            // Initialize the Suggestions list to an empty list.
            Suggestions = [];

            // Initialize the Message property to an empty string.
            Message = "";
        }

        /// <summary>
        /// Removes all suggestions that have a null Movie_id.
        /// </summary>
        public void PurgeFailures()
        {
            // Remove all suggestions from the Suggestions list where the Movie_id is null.
            Suggestions.RemoveAll(suggestion => suggestion.Movie_id == null);
        }

        /// <summary>
        /// Gets or sets the list of suggestions.
        /// </summary>
        public List<Suggestion> Suggestions { get; set; }

        /// <summary>
        /// Gets or sets the message associated with the response.
        /// </summary>
        public string Message { get; set; }
    }
}