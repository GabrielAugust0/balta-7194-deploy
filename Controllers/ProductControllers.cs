using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using Shop.Data;
using Shop.Models;

namespace Shop.Controllers
{
    [Route("v1/products")]
    public class ProductController : ControllerBase
    {
      [HttpGet]
      [Route("")]
      [AllowAnonymous]
      public async Task<ActionResult<List<Product>>> get(
        [FromServices]DataContext context
      )
      {   
          // Dessa forma, além dos produtos mostra também suas categorias (Fazendo um join no SQL server)
          var products = await context
                .Products
                .Include(x => x.Category)
                .AsNoTracking()
                .ToListAsync();
          return Ok(products);
      }

      [HttpGet]
      [Route("{id:int}")]
      [AllowAnonymous]
      public async Task<ActionResult<List<Product>>> getById(
        [FromServices]DataContext context,
        int id
      )
      {   
          // Dessa forma, além dos produtos mostra também suas categorias (Fazendo um join no SQL server)
          var products = await context
                .Products
                .Include(x => x.Category)
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == id);
          return Ok(products);
      }

      [HttpGet] // products/categories/1
      [Route("categories/{id:int}")]
      [AllowAnonymous]
      public async Task<ActionResult<List<Product>>> getByCategory(
        [FromServices]DataContext context,
        int id
      )
      {   
          // Dessa forma, além dos produtos mostra também suas categorias (Fazendo um join no SQL server)
          var products = await context
                .Products
                .Include(x => x.Category)
                .AsNoTracking()
                .Where(x => x.Category.Id == id)
                .ToListAsync();
                
          return Ok(products);
      }

      [HttpPut]
      [Route("{id:int}")]
      [Authorize(Roles = "manager")]
      public async Task<ActionResult<dynamic>> put(
          int id, 
          [FromBody]Product model,
          [FromServices]DataContext context
          )
      {   
          // Verifica se o ID informado é o mesmo do modelo
          if( id != model.Id ) 
              return NotFound( new { message = "Produto não encontrado "});
          
          // Verifica se os dados são válidos
          if( !ModelState.IsValid )
              return BadRequest(ModelState);

          try
          {
              context.Entry<Product>(model).State = EntityState.Modified;
              await context.SaveChangesAsync(); // Faz persistir no banco
              return Ok(model);
          }
          catch (DbUpdateConcurrencyException)
          {
              return BadRequest(new { message = "Este registro já foi atualizado"});
          }
          catch (Exception)
          {
              return BadRequest(new {message = "Não foi possível atualizar o produto"});
          }
      }

      [HttpPost]
      [Route("")]
      [Authorize(Roles = "employee")]
      public async Task<ActionResult<List<Product>>> post(
        [FromBody]Product model,
        [FromServices]DataContext context
        )
      {
          if( !ModelState.IsValid )
                return BadRequest(ModelState);  

          try
          {
                context.Products.Add(model);
                await context.SaveChangesAsync();
                return Ok(model);
          }
          catch
          {
                return BadRequest(new { message = "Não foi possível criar o produto" });
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
          var product = await context.Products.FirstOrDefaultAsync(x => x.Id == id);
          
          if( product == null )
              return NotFound(new { message = "Produto não encontrada" });

          try
          {
              context.Products.Remove(product);
              await context.SaveChangesAsync();
              return Ok(new { message = "Produto removido com sucesso"});
          }
          catch (Exception)
          {
              return BadRequest(new { message = "Não foi possível remover o usuário"});
          }
      }
    }  
}