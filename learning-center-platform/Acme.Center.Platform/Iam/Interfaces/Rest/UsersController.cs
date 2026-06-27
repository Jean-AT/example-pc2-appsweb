using System.Net.Mime;
using Acme.Center.Platform.Iam.Application.QueryServices;
using Acme.Center.Platform.Iam.Domain.Model.Queries;
using Acme.Center.Platform.Iam.Infrastructure.Pipeline.Middleware.Attributes;
using Acme.Center.Platform.Iam.Interfaces.Rest.Resources;
using Acme.Center.Platform.Iam.Interfaces.Rest.Transform;
using Acme.Center.Platform.Shared.Interfaces.Rest.ProblemDetails;
using Acme.Center.Platform.Shared.Resources.Errors;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Swashbuckle.AspNetCore.Annotations;
// For ProblemDetailsFactory

// For IamError enum

namespace Acme.Center.Platform.Iam.Interfaces.Rest;

[Authorize]
[ApiController]
[Route("api/v1/[controller]")]
[Produces(MediaTypeNames.Application.Json)]
[SwaggerTag("Available User endpoints")]
public class UsersController(
    IUserQueryService userQueryService,
    IStringLocalizer<ErrorMessage> errorLocalizer, // Renamed for clarity
    ProblemDetailsFactory problemDetailsFactory) // Inject ProblemDetailsFactory
    : ControllerBase
{
    private readonly IStringLocalizer<ErrorMessage> _errorLocalizer = errorLocalizer;
    private readonly ProblemDetailsFactory _problemDetailsFactory = problemDetailsFactory;

    /**
     * <summary>
     *     Get user by id endpoint. It allows to get a user by id
     * </summary>
     * <param name="id">The user id</param>
     * <param name="cancellationToken">The cancellation token.</param>
     * <returns>The user resource</returns>
     */
    [HttpGet("{id}")]
    [SwaggerOperation(
        Summary = "Get a user by its id",
        Description = "Get a user by its id",
        OperationId = "GetUserById")]
    [SwaggerResponse(StatusCodes.Status200OK, "The user was found", typeof(UserResource))]
    [SwaggerResponse(StatusCodes.Status404NotFound, "The user was not found")]
    public async Task<IActionResult> GetUserById(int id, CancellationToken cancellationToken)
    {
        var getUserByIdQuery = new GetUserByIdQuery(id);
        var user = await userQueryService.Handle(getUserByIdQuery, cancellationToken);

        return IamActionResultAssembler.ToActionResultFromGetUserByIdResult(
            this,
            user,
            _errorLocalizer,
            _problemDetailsFactory,
            foundUser => Ok(UserResourceFromEntityAssembler.ToResourceFromEntity(foundUser))
        );
    }

    /**
     * <summary>
     *     Get all users' endpoint. It allows getting all users
     * </summary>
     * <returns>The user resources</returns>
     */
    [HttpGet]
    [SwaggerOperation(
        Summary = "Get all users",
        Description = "Get all users",
        OperationId = "GetAllUsers")]
    [SwaggerResponse(StatusCodes.Status200OK, "The users were found", typeof(IEnumerable<UserResource>))]
    public async Task<IActionResult> GetAllUsers(CancellationToken cancellationToken)
    {
        var getAllUsersQuery = new GetAllUsersQuery();
        var users = await userQueryService.Handle(getAllUsersQuery, cancellationToken);
        var userResources = users.Select(UserResourceFromEntityAssembler.ToResourceFromEntity);
        return Ok(userResources);
    }
}