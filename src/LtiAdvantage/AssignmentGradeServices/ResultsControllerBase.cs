﻿using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace LtiAdvantage.AssignmentGradeServices
{
    /// <inheritdoc />
    /// <summary>
    /// Implements the Assignment and Grade Services results endpoint.
    /// </summary>
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Policy = Constants.LtiScopes.AgsResultReadonly)]
    [Route("context/{contextId}/lineitems/{lineItemId}/results", Name = Constants.ServiceEndpoints.AgsResultsService)]
    [Route("context/{contextId}/lineitems/{lineItemId}/results.{format}")]
    [ApiController]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public abstract class ResultsControllerBase : ControllerBase
    {
        /// <summary>
        /// </summary>
        protected readonly ILogger<ResultsControllerBase> Logger;

        /// <summary>
        /// </summary>
        protected ResultsControllerBase(ILogger<ResultsControllerBase> logger)
        {
            Logger = logger;
        }
                
        /// <summary>
        /// Returns the results for a line item.
        /// </summary>
        /// <param name="request">The request parameters.</param>
        /// <returns>The results.</returns>
        protected abstract Task<ActionResult<ResultContainer>> OnGetResultsAsync(GetResultsRequest request);

        /// <summary>
        /// Returns the results for a lineitem.
        /// </summary>
        /// <param name="contextId">The context id.</param>
        /// <param name="lineItemId">The line item id.</param>
        /// <param name="userId">Optional user id filter.</param>
        /// <param name="limit">Optional limit to the number of results returned.</param>
        /// <returns>The results.</returns>
        [HttpGet]
        [Produces(Constants.MediaTypes.ResultContainer)]
        [ProducesResponseType(typeof(ResultContainer), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ResultContainer>> GetAsync(string contextId, string lineItemId, 
            [FromQuery(Name = "userId")] string userId = null, 
            [FromQuery] int? limit = null)
        {
            try
            {
                Logger.LogDebug($"Entering {nameof(GetAsync)}.");
                
                if (string.IsNullOrWhiteSpace(contextId))
                {
                    Logger.LogError($"{nameof(contextId)} is missing.");
                    return BadRequest(new ProblemDetails { Title = $"{nameof(contextId)} is required." });
                }
            
                if (string.IsNullOrWhiteSpace(lineItemId))
                {
                    Logger.LogError($"{nameof(lineItemId)} is missing.");
                    return BadRequest(new ProblemDetails { Title = $"{nameof(lineItemId)} is required." });
                }

                try
                {
                    var request = new GetResultsRequest(contextId, lineItemId, userId, limit);
                    return await OnGetResultsAsync(request).ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    Logger.LogError(ex, "Error processing get results request.");
                    return StatusCode(StatusCodes.Status500InternalServerError, new ProblemDetails
                    {
                        Title = ex.Message,
                        Detail = ex.StackTrace
                    });
                }
            }
            finally
            {
                Logger.LogDebug($"Exiting {nameof(GetAsync)}.");
            }
        }
    }
}

