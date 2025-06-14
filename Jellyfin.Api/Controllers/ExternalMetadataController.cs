// Jellyfin.Api/Controllers/ExternalMetadataController.cs
// This file is part of the Jellyfin API.
// It provides an API controller for fetching external metadata, specifically for movies ans series using TMDb.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Jellyfin.Api.Models.ExternalMetadataDtos;
using Jellyfin.Data.Enums;
using MediaBrowser.Providers.Plugins.Tmdb;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using TMDbLib.Objects.Collections;
using TMDbLib.Objects.General;
using TMDbLib.Objects.Movies;

namespace Jellyfin.Api.Controllers
{
    /// <summary>
    /// Controller for fetching external metadata.
    /// </summary>
    [ApiController]
    [Route("ExternalMetadata")]
    public class ExternalMetadataController : ControllerBase
    {
        private readonly TmdbClientManager _tmdbClientManager;
        private readonly ILogger<ExternalMetadataController> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="ExternalMetadataController"/> class.
        /// </summary>
        /// <param name="tmdbClientManager">The TMDb client manager.</param>
        /// <param name="logger">The logger.</param>
        public ExternalMetadataController(TmdbClientManager tmdbClientManager, ILogger<ExternalMetadataController> logger)
        {
            _tmdbClientManager = tmdbClientManager;
            _logger = logger;
        }

        /// <summary>
        /// Gets remote movie details by TMDb ID.
        /// </summary>
        /// <param name="movieId">The TMDb ID of the movie.</param>
        /// <param name="language">The language for localization (e.g., "en-US"). Defaults to "en-US".</param>
        /// <returns>An <see cref="ActionResult"/> containing the <see cref="ExternalMovieDetailsDto"/> or an error response.</returns>
        [HttpGet("Movie")] // Changed from "Movie/{tmdbId}"
        public async Task<ActionResult<ExternalMovieDetailsDto>> GetRemoteMovieDetails([FromQuery] int movieId, [FromQuery] string? language = "en-US") // Changed tmdbId to movieId and added [FromQuery]
        {
            if (movieId <= 0) // Changed from tmdbId
            {
                return BadRequest("Valid TMDb ID (movieId) is required."); // Changed from tmdbId
            }

            try
            {
                string requestedLanguageNonNull = language ?? "en-US";
                string imageLanguages = TmdbUtils.GetImageLanguagesParam(requestedLanguageNonNull);

                Movie? tmdbMovie = await _tmdbClientManager.GetMovieAsync(movieId, requestedLanguageNonNull, imageLanguages, HttpContext.RequestAborted) // Changed from tmdbId
                    .ConfigureAwait(false);

                if (tmdbMovie is null)
                {
                    _logger.LogInformation("Movie with TMDb ID {MovieId} not found.", movieId);
                    return NotFound();
                }

                var dto = MapToExternalMovieDetailsDto(tmdbMovie!, requestedLanguageNonNull);

                if (tmdbMovie.Similar?.Results != null && tmdbMovie.Similar.Results.Count > 0)
                {
                    dto.SimilarMovies = tmdbMovie.Similar.Results.Select(s => new ExternalMovieDto
                    {
                        Id = s.Id,
                        Title = s.Title,
                        PosterPath = s.PosterPath,
                        ReleaseDate = s.ReleaseDate,
                        CommunityRating = s.VoteAverage
                    }).ToList();
                }

                if (tmdbMovie.BelongsToCollection?.Id != null)
                {
                    string collectionImageLanguages = TmdbUtils.GetImageLanguagesParam(requestedLanguageNonNull);
                    Collection? collection = await _tmdbClientManager.GetCollectionAsync(tmdbMovie.BelongsToCollection.Id, requestedLanguageNonNull, collectionImageLanguages, HttpContext.RequestAborted)
                        .ConfigureAwait(false);
                    if (collection is not null)
                    {
                        dto.BelongsToCollection = new ExternalCollectionDto
                        {
                            Id = collection.Id,
                            Name = collection.Name,
                            PosterPath = collection.PosterPath,
                            BackdropPath = collection.BackdropPath
                        };
                    }
                }

                return Ok(dto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching remote movie details for TMDb ID {MovieId}", movieId);
                return StatusCode(500, "An error occurred while fetching movie details.");
            }
        }

        private ExternalMovieDetailsDto MapToExternalMovieDetailsDto(Movie tmdbMovie, string requestedLanguage)
        {
            return new ExternalMovieDetailsDto
            {
                TmdbId = tmdbMovie.Id,
                ImdbId = tmdbMovie.ImdbId,
                Title = tmdbMovie.Title,
                OriginalTitle = tmdbMovie.OriginalTitle,
                Overview = tmdbMovie.Overview,
                Tagline = tmdbMovie.Tagline,
                ReleaseDate = tmdbMovie.ReleaseDate,
                RuntimeMinutes = tmdbMovie.Runtime,
                OfficialRating = DetermineOfficialRating(tmdbMovie.Releases, requestedLanguage),
                CommunityRating = tmdbMovie.VoteAverage,
                VoteCount = tmdbMovie.VoteCount,
                Genres = tmdbMovie.Genres?.Select(g => g.Name).ToList() ?? new List<string>(),
                ProductionCompanies = tmdbMovie.ProductionCompanies?.Select(pc => new ExternalProductionCompanyDto
                {
                    Id = pc.Id,
                    Name = pc.Name,
                    LogoPath = pc.LogoPath,
                    OriginCountry = pc.OriginCountry
                }).ToList() ?? new List<ExternalProductionCompanyDto>(),
                Homepage = tmdbMovie.Homepage,
                PosterPath = tmdbMovie.PosterPath,
                BackdropPath = tmdbMovie.BackdropPath,
                Images = MapImages(tmdbMovie.Images, requestedLanguage),
                Cast = tmdbMovie.Credits?.Cast?.Select(c => new ExternalPersonDto
                {
                    Id = c.Id,
                    Name = c.Name,
                    Character = c.Character,
                    ProfilePath = c.ProfilePath,
                    PersonType = PersonKind.Actor
                }).ToList() ?? new List<ExternalPersonDto>(),
                Crew = tmdbMovie.Credits?.Crew?.Select(c => new ExternalPersonDto
                {
                    Id = c.Id,
                    Name = c.Name,
                    Job = c.Job,
                    Department = c.Department,
                    ProfilePath = c.ProfilePath,
                    PersonType = MapCrewToPersonKind(c)
                }).ToList() ?? new List<ExternalPersonDto>(),
                Trailers = tmdbMovie.Videos?.Results?
                    .Where(v =>
                        (v.Type?.Equals("Trailer", StringComparison.OrdinalIgnoreCase) == true || v.Type?.Equals("Teaser", StringComparison.OrdinalIgnoreCase) == true) &&
                        v.Site?.Equals("YouTube", StringComparison.OrdinalIgnoreCase) == true)
                    .Select(v => new ExternalVideoDto
                    {
                        Id = v.Id,
                        Key = v.Key,
                        Name = v.Name,
                        Site = v.Site,
                        Type = v.Type,
                        Size = v.Size
                    }).ToList() ?? new List<ExternalVideoDto>(),
            };
        }

        private List<ExternalImageDto>? MapImages(TMDbLib.Objects.General.Images? images, string requestedLanguage)
        {
            if (images is null)
            {
                return null;
            }

            var allImages = new List<ExternalImageDto>();

            if (images.Backdrops != null && images.Backdrops.Count > 0)
            {
                allImages.AddRange(images.Backdrops.Select(img => new ExternalImageDto
                {
                    FilePath = img.FilePath,
                    Width = img.Width,
                    Height = img.Height,
                    Iso6391 = TmdbUtils.AdjustImageLanguage(img.Iso_639_1, requestedLanguage),
                    VoteAverage = img.VoteAverage,
                    VoteCount = img.VoteCount,
                    ImageType = "Backdrop"
                }));
            }

            if (images.Logos != null && images.Logos.Count > 0)
            {
                allImages.AddRange(images.Logos.Select(img => new ExternalImageDto
                {
                    FilePath = img.FilePath,
                    Width = img.Width,
                    Height = img.Height,
                    Iso6391 = TmdbUtils.AdjustImageLanguage(img.Iso_639_1, requestedLanguage),
                    VoteAverage = img.VoteAverage,
                    VoteCount = img.VoteCount,
                    ImageType = "Logo"
                }));
            }

            if (images.Posters != null && images.Posters.Count > 0)
            {
                allImages.AddRange(images.Posters.Select(img => new ExternalImageDto
                {
                    FilePath = img.FilePath,
                    Width = img.Width,
                    Height = img.Height,
                    Iso6391 = TmdbUtils.AdjustImageLanguage(img.Iso_639_1, requestedLanguage),
                    VoteAverage = img.VoteAverage,
                    VoteCount = img.VoteCount,
                    ImageType = "Poster"
                }));
            }

            return allImages.Count > 0 ? allImages : null;
        }

        private string? DetermineOfficialRating(TMDbLib.Objects.Movies.Releases? releases, string requestedLanguage)
        {
            if (releases?.Countries == null || releases.Countries.Count == 0)
            {
                return null;
            }

            string countryCode = requestedLanguage.Split('-').LastOrDefault()?.ToUpperInvariant() ?? string.Empty;

            var countryRelease = releases.Countries.FirstOrDefault(c => c.Iso_3166_1.Equals(countryCode, StringComparison.OrdinalIgnoreCase) && !string.IsNullOrEmpty(c.Certification));
            if (countryRelease is not null)
            {
                return countryRelease.Certification;
            }

            // Fallback
            if (string.IsNullOrEmpty(countryCode) || requestedLanguage.StartsWith("en", StringComparison.OrdinalIgnoreCase) || countryCode == "US")
            {
                countryRelease = releases.Countries.FirstOrDefault(c => c.Iso_3166_1.Equals("US", StringComparison.OrdinalIgnoreCase) && !string.IsNullOrEmpty(c.Certification));
                if (countryRelease is not null)
                {
                    return countryRelease.Certification;
                }
            }

            // Fallback: Any country's rating
            countryRelease = releases.Countries.FirstOrDefault(c => !string.IsNullOrEmpty(c.Certification));
            return countryRelease?.Certification;
        }

        private PersonKind MapCrewToPersonKind(TMDbLib.Objects.General.Crew crewMember)
        {
            if (crewMember.Job?.Equals("Director", StringComparison.OrdinalIgnoreCase) == true)
            {
                return PersonKind.Director;
            }

            if (crewMember.Department?.Equals("Writing", StringComparison.OrdinalIgnoreCase) == true)
            {
                return PersonKind.Writer;
            }

            if (crewMember.Department?.Equals("Production", StringComparison.OrdinalIgnoreCase) == true &&
                (crewMember.Job?.IndexOf("Producer", StringComparison.OrdinalIgnoreCase) >= 0))
            {
                return PersonKind.Producer;
            }

            return PersonKind.Unknown; // Default for unmapped roles
        }
    }
}
