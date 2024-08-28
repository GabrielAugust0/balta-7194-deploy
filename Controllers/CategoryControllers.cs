using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using Shop.Data;
using Shop.Models;

/* 
  Endpoint => URL
 https://localhost:5001
 http://localhost:5000
*/

/*
async: Indica que o método é assíncrono, permitindo operações que podem ser realizadas em paralelo, como acessar uma base de dados ou chamar um serviço externo, sem bloquear a execução do programa.

Task<ActionResult<List<Category>>>: Define o tipo de retorno do método. Task indica que o método é assíncrono. ActionResult<List<Category>> significa que o método retorna uma resposta HTTP que pode incluir uma lista de objetos do tipo Category.

post: Nome do método.

[FromBody]Category model: O parâmetro model é do tipo Category, que provavelmente é uma classe representando uma categoria. O atributo [FromBody] indica que o model será passado no corpo da requisição (JSON, XML, etc.).
*/

[Route("v1/categories")]
public class CategoryController : ControllerBase
{     
      [HttpGet]
      [Route("")]
      [AllowAnonymous]
      [ResponseCache(VaryByHeader = "User-Agent", Location = ResponseCacheLocation.Any, Duration = 30)]
      public async Task<ActionResult<List<Category>>> get(
        [FromServices]DataContext context
      )
      {   
          /*
          AsNoTracking():

            Esse método indica ao Entity Framework para não rastrear as entidades retornadas pela consulta. O rastreamento normalmente é utilizado para monitorar as mudanças feitas nas entidades e permitir que essas mudanças sejam salvas no banco de dados. Quando você sabe que não vai alterar as entidades, usar AsNoTracking() pode melhorar a performance, já que o Entity Framework não precisa manter essa informação em memória.
          
          ToListAsync():

            Este método converte o resultado da consulta em uma lista (List<Category>) de forma assíncrona. Como a consulta é executada de forma assíncrona, o await é utilizado para esperar até que todos os dados sejam carregados do banco de dados.
          */
          var categories = await context.Categories.AsNoTracking().ToListAsync();
          return Ok(categories);
      }

      [HttpGet]
      [Route("{id:int}")]
      [AllowAnonymous]
      public async Task<ActionResult<Category>> getById(
          int id,
          [FromServices]DataContext context)
      {   
          var category = await context.Categories.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);
          return Ok(category);
      }

      [HttpPost]
      [Route("")]
      [Authorize(Roles = "employee")]
      public async Task<ActionResult<List<Category>>> post(
        [FromBody]Category model,
        [FromServices]DataContext context
        )
      {
          // Verificando se o 'model' é válido
          /*
          ModelState é específico para aplicações que usam o ASP.NET Core. Ele é usado para validar dados recebidos em requisições HTTP e verificar se eles estão de acordo com as regras de validação definidas nos modelos.
          */
          if( !ModelState.IsValid )
                return BadRequest(ModelState);  

          try
          {
                // Coloca a informação no banco e trata possível erro com o try/catch
                context.Categories.Add(model);
                await context.SaveChangesAsync();
                return Ok(model); // já retorna o ID correto
          }
          catch
          {
                return BadRequest(new { message = "Não foi possível criar a categoria" });
          }
          
      }

      [HttpPut]
      [Route("{id:int}")]
      [Authorize(Roles = "employee")]
      public async Task<ActionResult<List<Category>>> put(
          int id, 
          [FromBody]Category model,
          [FromServices]DataContext context
          )
      {   
          // Verifica se o ID informado é o mesmo do modelo
          if( id != model.Id ) 
              return NotFound( new { message = "Categoria não encontrada "});
          
          // Verifica se os dados são válidos
          if( !ModelState.IsValid )
              return BadRequest(ModelState);

          try
          {
              context.Entry<Category>(model).State = EntityState.Modified;
              await context.SaveChangesAsync(); // Faz persistir no banco
              return Ok(model);
          }
          catch (DbUpdateConcurrencyException)
          {
              return BadRequest(new { message = "Este registro já foi atualizado"});
          }
          catch (Exception)
          {
              return BadRequest(new {message = "Não foi possível atualizar a categoria"});
          }
      }

      [HttpDelete]
      [Route("{id:int}")]
      [Authorize(Roles = "employee")]
      public async Task<ActionResult<List<Category>>> delete(
          int id,
          [FromServices]DataContext context
      )
      {   
          var category = await context.Categories.FirstOrDefaultAsync(x => x.Id == id);
          if( category == null )
              return NotFound(new { message = "Categoria não encontrada" });

          try
          {
              context.Categories.Remove(category);
              await context.SaveChangesAsync();
              return Ok(new { message = "Categoria removida com sucesso"});
          }
          catch (Exception)
          {
              return BadRequest(new { message = "Não foi possível remover a categoria"});
          }
      }
}