using System.Net.Http.Headers;

namespace LilysMovieServer
{
    /// <summary>
    /// Class responsible for scraping movie suggestions from the Cine2 API.
    /// </summary>
    public class Cine2Scraper
    {
        // HttpClient instance used to send HTTP requests.
        private readonly HttpClient Client;

        // Base URL for the Cine2 API.
        private readonly string Cine2URL;

        // Query parameters for the Cine2 API.
        private readonly string Cine2Params;

        /// <summary>
        /// Initializes a new instance of the Cine2Scraper class.
        /// Sets up the HttpClient and configures the base address and headers.
        /// </summary>
        public Cine2Scraper()
        {
            // Initialize the base URL for the Cine2 API.
            Cine2URL = "https://cinenerdle-suggestion-server-e031df6af7d2.herokuapp.com/suggestion-list/movies";

            // Initialize the query parameters for the Cine2 API.
            Cine2Params = "?normalizedInput=";

            // Create a new HttpClient instance and set the base address.
            Client = new HttpClient
            {
                BaseAddress = new Uri(Cine2URL)
            };

            // Add an Accept header to the HttpClient for JSON format.
            Client.DefaultRequestHeaders.Accept.Add(
            new MediaTypeWithQualityHeaderValue("application/json"));
        }

        /// <summary>
        /// Finds movie suggestions based on the search string provided.
        /// </summary>
        /// <param name="search">The search string to query the Cine2 API with.</param>
        /// <returns>A Cine2Response object containing the suggestions.</returns>
        public Cine2Response FindSuggestions(string search)
        {
            // Send a GET request to the Cine2 API with the search string.
            HttpResponseMessage response = Client.GetAsync(Cine2Params + search).Result;

            // Check if the response status code indicates success.
            if (response.IsSuccessStatusCode)
            {
                // Parse the response body into a Cine2Response object.
                Cine2Response cineResponse = response.Content.ReadAsAsync<Cine2Response>().Result;

                // Remove any failed suggestions from the response.
                cineResponse.PurgeFailures();

                // Return the parsed and cleaned response.
                return cineResponse;
            }
            else
            {
                // Return an empty Cine2Response object if the request failed.
                return new Cine2Response();
            }
        }
    }
}