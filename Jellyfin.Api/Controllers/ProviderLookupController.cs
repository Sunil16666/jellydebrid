using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Jellyfin.Api.Extensions;
using MediaBrowser.Controller.Dto;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Library;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging; // Added for ILogger

namespace Jellyfin.Api.Controllers
{
    /// <summary>
    /// Controller for provider-based item existence checks.
    /// </summary>
    [Route("Library/ProviderLookup")]
    [ApiController]
    public class ProviderLookupController : ControllerBase
    {
        private readonly ILibraryManager _libraryManager;
        private readonly IUserManager _userManager;
        private readonly ILogger<ProviderLookupController> _logger; // Added ILogger field

        /// <summary>
        /// Initializes a new instance of the <see cref="ProviderLookupController"/> class.
        /// </summary>
        /// <param name="libraryManager">The library manager to access the item list.</param>
        /// <param name="userManager">The user manager to access user information.</param>
        /// <param name="logger">The logger.</param>
        public ProviderLookupController(
            ILibraryManager libraryManager,
            IUserManager userManager,
            ILogger<ProviderLookupController> logger) // Injected ILogger
        {
            _libraryManager = libraryManager;
            _userManager = userManager;
            _logger = logger; // Assigned ILogger
        }

        /// <summary>
        /// Checks if an item exists in the library by provider and provider id for a specific user.
        /// </summary>
        /// <param name="provider">The provider name (e.g., 'Tmdb').</param>
        /// <param name="id">The provider id (e.g., TMDb numeric id as string).</param>
        /// <param name="userId">The id of the user to check against.</param>
        /// <returns>True if an item exists, false otherwise.</returns>
        [HttpGet("Exists")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public ActionResult<bool> ExistsByProviderId(
            [FromQuery][Required] string provider,
            [FromQuery][Required] string id,
            [FromQuery][Required] Guid userId)
        {
            _logger.LogInformation("Checking existence for provider: '{Provider}', id: '{Id}', userId: '{UserId}'", provider, id, userId);

            var user = _userManager.GetUserById(userId);
            if (user == null)
            {
                _logger.LogWarning("User with id '{UserId}' not found.", userId);
                return NotFound("User not found.");
            }

            int checkedCount = 0;
            // Pass the user to InternalItemsQuery
            foreach (var item in _libraryManager.GetItemList(new InternalItemsQuery(user)))
            {
                try
                {
                    checkedCount++;
                    if (item.ProviderIds != null && item.ProviderIds.TryGetValue(provider, out var pid))
                    {
                        _logger.LogDebug("Checking item '{ItemName}' (Id: {ItemId}) with provider id '{ProviderId}'", item.Name, item.Id, pid);
                        if (pid == id)
                        {
                            _logger.LogInformation("Found match: item '{ItemName}' (Id: {ItemId})", item.Name, item.Id);
                            return true;
                        }
                    }
                }
                catch (Exception ex)
                {
                    // Log and skip problematic items
                    _logger.LogWarning(ex, "Skipping item '{ItemId}' due to error.", item?.Id);
                }
            }

            _logger.LogInformation("No match found after scanning {CheckedCount} items for provider: '{Provider}', id: '{Id}', userId: '{UserId}'.", checkedCount, provider, id, userId);
            return false;
        }
    }
}
