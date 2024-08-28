using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Shop.Data;
using Shop.Models;
using Shop.Services;
using System;
using Microsoft.AspNetCore.Authorization;

namespace Shop.Controllers
{    
    [Route("v1/users")]
    public class UsersController : Controller
    {

      [HttpGet]
      [Route("")]
      [Authorize(Roles = "manager")]
      public async Task<ActionResult<List<User>>> get(
         [FromServices]DataContext context
      )
      {   
        var users = await context
            .Users //.Include(x => x.Category)
            .AsNoTracking()
            .ToListAsync();
        return Ok(users);
      }

      [HttpPost]
      [Route("")]
      [AllowAnonymous]
      // [Authorize(Roles = "manager")]
      public async Task<ActionResult<List<User>>> get(
        [FromServices]DataContext context,
        [FromBody]User model
      )
      {   
          // Verifica se os dados são válidos
          if (!ModelState.IsValid)
              return BadRequest(ModelState);
          
          try
          {   
              // Força o usuário a ser sempre "funcionário"
              model.Role = "employee";

              context.Users.Add(model);
              await context.SaveChangesAsync();

              // Esconde a senha
              model.Password = "";
              return Ok(model);
          }
          catch (Exception)
          {
              return BadRequest(new { message = "Não foi possível criar o usuário" });
          }
      }

      [HttpPost]
      [Route("login")]
      public async Task<ActionResult<dynamic>> Authenticate(
                  [FromBody]User model,
                  [FromServices]DataContext context
      )
      {
            if (model == null || string.IsNullOrEmpty(model.Username) || string.IsNullOrEmpty(model.Password))
                return BadRequest("Invalid login request");

            try
            {
                var user = await context.Users
                                .AsNoTracking()
                                .Where(x => x.Username == model.Username && x.Password == model.Password)
                                .FirstOrDefaultAsync();

                if (user == null)
                    return Unauthorized("Invalid credentials");

                var token = TokenService.GenerateToken(user);
                return Ok(new { 
                    name = user.Username, // Esconde a senha
                    token = token 
                });
            }
            catch (Exception ex)
            {
                // Log do erro para depuração
                Console.WriteLine(ex.Message);
                return StatusCode(500, "Internal server error");
            }
      }

      [HttpPut]
      [Route("{id:int}")]
      [Authorize(Roles = "manager")]
      public async Task<ActionResult<dynamic>> put(
          int id, 
          [FromBody]User model,
          [FromServices]DataContext context
          )
      {   
          // Verifica se o ID informado é o mesmo do modelo
          if( id != model.Id ) 
              return NotFound( new { message = "Usuário não encontrado "});
          
          // Verifica se os dados são válidos
          if( !ModelState.IsValid )
              return BadRequest(ModelState);

          try
          {
              context.Entry<User>(model).State = EntityState.Modified;
              await context.SaveChangesAsync(); // Faz persistir no banco
              return Ok(model);
          }
          catch (DbUpdateConcurrencyException)
          {
              return BadRequest(new { message = "Este registro já foi atualizado"});
          }
          catch (Exception)
          {
              return BadRequest(new {message = "Não foi possível atualizar o usuário"});
          }
      }

      [HttpDelete]
      [Route("{id:int}")]
      [Authorize(Roles = "manager")]
      public async Task<ActionResult<dynamic>> delete(
          int id,
          [FromServices]DataContext context
      )
      {   
          var user = await context.Users.FirstOrDefaultAsync(x => x.Id == id);
          
          if( user == null )
              return NotFound(new { message = "Usuário não encontrada" });

          try
          {
              context.Users.Remove(user);
              await context.SaveChangesAsync();
              return Ok(new { message = "Usuário removido com sucesso"});
          }
          catch (Exception)
          {
              return BadRequest(new { message = "Não foi possível remover o usuário"});
          }
      } 
    }

}