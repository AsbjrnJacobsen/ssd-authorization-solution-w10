using Microsoft.AspNetCore.Identity;

namespace ssd_authorization_solution.DTOs;

public class AuthorDto
{
    public string Id { get; set; }
    public string Name { get; set; }

    public static AuthorDto FromEntity(IdentityUser entity)
    {
        return new AuthorDto { Id = entity.Id, Name = entity.UserName };
    }
}