using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Jellyfin.Api
{
    /// <summary>
    /// Constants related to The Movie Database (TMDB) API.
    /// </summary>
    public static class TmdbApiConstants
    {
        /// <summary>
        /// Gets the list of fields that can be appended to TMDB API movie detail requests.
        /// </summary>
        /// <remarks>
        /// These values are used with the 'append_to_response' query parameter.
        /// </remarks>
        public static readonly ReadOnlyCollection<string> AppendToResponseMovieFields = new ReadOnlyCollection<string>(new List<string>
        {
            "videos",
            "images",
            "credits",
            "releases",
            "keywords",
            "similar",
            "recommendations"
        });

        // Future constants for TV shows or other TMDB features can be added here.
    }
}
