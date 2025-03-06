using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ssd_authorization_solution.DTOs;
using ssd_authorization_solution.Entities;
using Microsoft.AspNetCore.Authorization;

namespace ssd_authorization_solution.Controllers;

[Route("api/[controller]")]
[ApiController]
public class ArticleController : ControllerBase
{
    private readonly AppDbContext db;

    public ArticleController(AppDbContext ctx)
    {
        this.db = ctx;
    }

    [AllowAnonymous]
    [HttpGet]
    public IEnumerable<ArticleDto> Get()
    {
        return db.Articles.Include(x => x.Author).Select(ArticleDto.FromEntity);
    }

    [AllowAnonymous]
    [HttpGet("{id}")]
    public ArticleDto? GetById(int id)
    {
        return db
            .Articles.Include(x => x.Author)
            .Where(x => x.Id == id)
            .Select(ArticleDto.FromEntity)
            .SingleOrDefault();
    }

    [Authorize(Roles = "Editor, Writer")]
    [HttpPost]
    public ArticleDto Post([FromBody] ArticleFormDto dto)
    {
        var userName = HttpContext.User.Identity?.Name;
        var author = db.Users.Single(x => x.UserName == userName);
        var entity = new Article
        {
            Title = dto.Title,
            Content = dto.Content,
            Author = author,
            CreatedAt = DateTime.Now
        };
        var created = db.Articles.Add(entity).Entity;
        db.SaveChanges();
        return ArticleDto.FromEntity(created);
    }

    [Authorize(Roles = "Editor, Writer")]
    [HttpPut("{id}")]
    public ActionResult<ArticleDto> Put(int id, [FromBody] ArticleFormDto dto)
    {
        var userName = HttpContext.User.Identity?.Name;
        var entity = db
            .Articles
            .Include(x => x.Author)
            .Single(x => x.Id == id);

        if (HttpContext.User.IsInRole("Writer") && !HttpContext.User.IsInRole("Editor"))
        {
            if(entity.Author.UserName != userName)
                return  Forbid("You are not authorized to edit this article");
        }
        
        entity.Title = dto.Title;
        entity.Content = dto.Content;
        var updated = db.Articles.Update(entity).Entity;
        db.SaveChanges();
        return ArticleDto.FromEntity(updated);
    }

    [Authorize(Roles = "Editor")]
    [HttpDelete("{id}")]
    public ActionResult<ArticleDto> Delete(int id)
    {
        var entity = db.Articles.Include(a => a.Author).Single(x => x.Id == id);
        if (entity == null)
        {
            return NotFound();
        }
        db.Articles.Remove(entity);
        db.SaveChanges();
        
        return ArticleDto.FromEntity(entity);
    }
}
