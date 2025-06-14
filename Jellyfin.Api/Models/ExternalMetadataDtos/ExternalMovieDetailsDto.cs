#pragma warning disable CS1591, CS8603, SA1402 // Suppress warnings
using System.Collections.Generic;
using System.Collections.ObjectModel; // Added for Collection<T>
using System.Text.Json.Serialization; // Added for JsonPropertyName

namespace Jellyfin.Api.Models.ExternalMetadataDtos
{
    /// <summary>
    /// Represents detailed movie information from an external source like TMDB.
    /// </summary>
    public class ExternalMovieDetailsDto
    {
        public string? BackdropPath { get; set; }

        public BelongsToCollectionDto? BelongsToCollection { get; set; }

        public long Budget { get; set; }

        public Collection<GenreDto>? Genres { get; }

        public string? Homepage { get; set; }

        public int Id { get; set; }

        public Collection<string>? OriginCountry { get; }

        public string? OriginalLanguage { get; set; }

        public string? Overview { get; set; }

        public string? PosterPath { get; set; }

        public Collection<ProductionCompanyDto>? ProductionCompanies { get; }

        public Collection<ProductionCountryDto>? ProductionCountries { get; }

        public string? ReleaseDate { get; set; } // Consider converting to DateTime during processing

        public long Revenue { get; set; }

        public int? Runtime { get; set; }

        public string? Status { get; set; }

        public string? Tagline { get; set; }

        public string? Title { get; set; }

        public double VoteAverage { get; set; }

        public int VoteCount { get; set; }

        public VideoCollectionDto? Videos { get; set; }

        public ImageCollectionDto? Images { get; set; }

        public CreditsDto? Credits { get; set; }

        public TmdbMovieReleasesDto? Releases { get; set; }

        public TmdbKeywordsDto? Keywords { get; set; }

        public PaginatedMovieResultDto? Similar { get; set; }

        public PaginatedMovieResultDto? Recommendations { get; set; }

        public bool Adult { get; set; }

        public string? ImdbId { get; set; }

        public string? OriginalTitle { get; set; }

        public double Popularity { get; set; }

        public bool Video { get; set; }

        public Collection<SpokenLanguageDto>? SpokenLanguages { get; }
    }

    public class GenreDto
    {
        public int Id { get; set; }

        public string? Name { get; set; }
    }

    public class ProductionCompanyDto
    {
        public int Id { get; set; }

        public string? LogoPath { get; set; }

        public string? Name { get; set; }

        public string? OriginCountry { get; set; }
    }

    public class ProductionCountryDto
    {
        [JsonPropertyName("iso_3166_1")]
        public string? Iso31661 { get; set; }

        public string? Name { get; set; }
    }

    public class SpokenLanguageDto
    {
        public string? EnglishName { get; set; }

        [JsonPropertyName("iso_639_1")]
        public string? Iso6391 { get; set; }

        public string? Name { get; set; }
    }

    public class VideoCollectionDto
    {
        public Collection<VideoResultDto>? Results { get; }
    }

    public class VideoResultDto
    {
        public string? Id { get; set; } // TMDB's own video ID

        [JsonPropertyName("iso_639_1")]
        public string? Iso6391 { get; set; } // Language code

        [JsonPropertyName("iso_3166_1")]
        public string? Iso31661 { get; set; } // Country code

        public string? Name { get; set; }

        public string? Key { get; set; } // e.g., YouTube key

        public string? Site { get; set; } // e.g., "YouTube"

        public int Size { get; set; } // e.g., 1080, 720

        public string? Type { get; set; } // e.g., "Trailer", "Teaser", "Clip"

        public bool Official { get; set; }

        public string? PublishedAt { get; set; } // ISO 8601 date-time string
    }

    public class ImageCollectionDto
    {
        public Collection<ImageDetailsDto>? Backdrops { get; }

        public Collection<ImageDetailsDto>? Logos { get; }

        public Collection<ImageDetailsDto>? Posters { get; }
    }

    public class ImageDetailsDto
    {
        public double AspectRatio { get; set; }

        public int Height { get; set; }

        [JsonPropertyName("iso_639_1")]
        public string? Iso6391 { get; set; } // Language of the image (e.g., for posters with text)

        public string? FilePath { get; set; }

        public double VoteAverage { get; set; }

        public int VoteCount { get; set; }

        public int Width { get; set; }
    }

    public class CreditsDto
    {
        public Collection<CastMemberDto>? Cast { get; }

        public Collection<CrewMemberDto>? Crew { get; }
    }

    public class CastMemberDto
    {
        public bool Adult { get; set; }

        public int? Gender { get; set; } // 0: Not set, 1: Female, 2: Male, 3: Non-binary

        public int Id { get; set; }

        public string? KnownForDepartment { get; set; }

        public string? Name { get; set; }

        public string? OriginalName { get; set; }

        public double Popularity { get; set; }

        public string? ProfilePath { get; set; }

        public int? CastId { get; set; } // Specific TMDB cast entry ID

        public string? Character { get; set; }

        public string? CreditId { get; set; } // TMDB credit ID

        public int? Order { get; set; }
    }

    public class CrewMemberDto
    {
        public bool Adult { get; set; }

        public int? Gender { get; set; }

        public int Id { get; set; }

        public string? KnownForDepartment { get; set; }

        public string? Name { get; set; }

        public string? OriginalName { get; set; }

        public double Popularity { get; set; }

        public string? ProfilePath { get; set; }

        public string? CreditId { get; set; }

        public string? Department { get; set; }

        public string? Job { get; set; }
    }

    public class TmdbMovieReleasesDto // Corresponds to the 'releases' field in TMDB movie details
    {
        public Collection<CountryReleaseCertificationDto>? Countries { get; }
    }

    public class CountryReleaseCertificationDto
    {
        public string? Certification { get; set; }

        [JsonPropertyName("iso_3166_1")]
        public string? Iso31661 { get; set; } // Country code

        public bool Primary { get; set; }

        public string? ReleaseDate { get; set; } // Date string
        // TMDB might also include 'descriptors' (List<string>) here
    }

    public class TmdbKeywordsDto // Corresponds to the 'keywords' field in TMDB movie details
    {
        // In TMDB, the movie details 'keywords' field might be structured as { "keywords": [...] } or { "id": ..., "keywords": [...] }
        // This DTO assumes the direct list if 'keywords' is the top-level object fetched,
        // or it's the inner list if part of a larger keyword response.
        // Based on tmdb_test.json: "keywords": { "keywords": [] }, this DTO represents the inner list's container.
        public Collection<KeywordDto>? Keywords { get; }
    }

    public class KeywordDto
    {
        public int Id { get; set; }

        public string? Name { get; set; }
    }

    public class PaginatedMovieResultDto // For similar and recommendations
    {
        public int Page { get; set; }

        public Collection<SimplifiedMovieDto>? Results { get; }

        public int TotalPages { get; set; }

        public int TotalResults { get; set; }
    }

    public class SimplifiedMovieDto
    {
        public string? PosterPath { get; set; }

        public bool Adult { get; set; }

        public string? Overview { get; set; }

        public string? ReleaseDate { get; set; }

        public Collection<int>? GenreIds { get; }

        public int Id { get; set; }

        public string? OriginalTitle { get; set; }

        public string? OriginalLanguage { get; set; }

        public string? Title { get; set; }

        public string? BackdropPath { get; set; }

        public double Popularity { get; set; }

        public int VoteCount { get; set; }

        public bool Video { get; set; }

        public double VoteAverage { get; set; }
    }

    public class BelongsToCollectionDto
    {
        public int Id { get; set; }

        public string? Name { get; set; }

        public string? PosterPath { get; set; }

        public string? BackdropPath { get; set; }
    }
}
#pragma warning restore CS1591, CS8603 // Restore warnings
