/*
Este trecho de código é uma parte de uma classe que herda de DbContext, a classe base para trabalhar com o Entity Framework Core, um ORM (Object-Relational Mapping) que facilita o acesso e a manipulação de dados em um banco de dados usando objetos .NET. 
*/

using Microsoft.EntityFrameworkCore;
using Shop.Models;

namespace Shop.Data
{
    public class DataContext : DbContext
    {
        public DataContext(DbContextOptions<DataContext> options) : base(options)
        {}

        /*
        DbSet<T> é uma propriedade que representa uma coleção de entidades do tipo T (como Product, Category, User) que será mapeada para uma tabela correspondente no banco de dados.
        */
        public DbSet<Product> Products { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<User> Users { get; set; }
        // Permite o CRUD (Creat, Read, Update, Delete)
    }
}