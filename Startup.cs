using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.OpenApi.Models;
using Shop.Data;

public class Startup
{
    public Startup(IConfiguration configuration)
    {
        Configuration = configuration;
    }

    public IConfiguration Configuration { get; }

    /*
        Banco de dados em memória: Um banco de dados em memória é armazenado na RAM e é útil para testes e prototipagem. Ele não persiste dados em disco, então os dados são perdidos quando a aplicação é encerrada.
    */
    public void ConfigureServices(IServiceCollection services)
    {   
        services.AddCors();
        // Realiza a compressão dos dados json
        services.AddResponseCompression(options => 
        {
            options.Providers.Add<GzipCompressionProvider>();
            options.MimeTypes = ResponseCompressionDefaults.MimeTypes.Concat(new[] { "application/json" });
        });
        /*
        Como funciona o CACHE
        Quando um dado é solicitado, o sistema verifica se ele está disponível no cache. Se estiver, o dado é recuperado rapidamente do cache, evitando a necessidade de buscar no sistema de origem.
        */
        //services.AddResponseCashing;
        services.AddControllers();

        // Autenticação
        var key = Encoding.ASCII.GetBytes(Settings.Secret);
        services.AddAuthentication(x =>
        {
            x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        }).AddJwtBearer(x => 
        {
            x.RequireHttpsMetadata = false;
            x.SaveToken = true;
            x.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true, // Valida se tem uma chave
                IssuerSigningKey = new SymmetricSecurityKey(key), // Validação da chave do front com o backend
                ValidateIssuer = false, 
                ValidateAudience = false
            };
        });

        //services.AddDbContext<DataContext>(opt => opt.UseSqlServer(
            //Configuration.GetConnectionString("connectionString")
        //));
        services.AddDbContext<DataContext>(opt => opt.UseInMemoryDatabase("Database")); // Adiciona o DbContext criado e o tipo do banco de dados

        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(c => 
        {
            c.SwaggerDoc("v1", new OpenApiInfo{ Title = "Shop API", Version = "v1"});
        });
    }

    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        /*if (env.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI(c => 
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "Shop API V1")
            });
        }*/
        app.UseSwagger();
        app.UseSwaggerUI(c => 
        {
            c.SwaggerEndpoint("/swagger/v1/swagger.json", "Shop API V1");
        });

        app.UseHttpsRedirection();
        app.UseRouting();

        app.UseCors(x => x
            .AllowAnyOrigin()
            .AllowAnyMethod()
            .AllowAnyHeader());
        app.UseAuthentication();
        app.UseAuthorization();
        
        app.UseEndpoints(endpoints =>
        {
            endpoints.MapControllers();
        });
    }
}
