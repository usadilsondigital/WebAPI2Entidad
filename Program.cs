
using Microsoft.EntityFrameworkCore;
using System.Globalization;
using System.Text;
using System.Collections.Generic;
using System.Security.Cryptography;

namespace WebAPI2Entidad
{
    public class Program
    {
        /*    public static void Main(string[] args)
            {
                var builder = WebApplication.CreateBuilder(args);

                // Add services to the container.

                builder.Services.AddControllers();
                // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
                builder.Services.AddEndpointsApiExplorer();
                builder.Services.AddSwaggerGen();

                var app = builder.Build();

                // Configure the HTTP request pipeline.
                if (app.Environment.IsDevelopment())
                {
                    app.UseSwagger();
                    app.UseSwaggerUI();
                }

                app.UseAuthorization();


                app.MapControllers();

                app.Run();
            }
      */


        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            builder.Services.AddDbContext<TodoDb>(opt => opt.UseInMemoryDatabase("TodoList"));
            builder.Services.AddDatabaseDeveloperPageExceptionFilter();
            var app = builder.Build();

            var todoItems = app.MapGroup("/todoitems");

            todoItems.MapGet("/", GetAllTodos);
            todoItems.MapGet("/complete", GetCompleteTodos);
            todoItems.MapGet("/{id}", GetTodo);
            todoItems.MapPost("/", CreateTodo);
            todoItems.MapPut("/{id}", UpdateTodo);
            todoItems.MapDelete("/{id}", DeleteTodo);
            todoItems.MapGet("/encrypt/{id}/{key}", EncryptAdi);
            todoItems.MapGet("/encryptAdi/{plainText}/{key}", EncryptAdi);

            app.Run();

            static async Task<IResult> GetAllTodos(TodoDb db)
            {
                return TypedResults.Ok(await db.Todos.ToArrayAsync());
            }

            static async Task<IResult> GetCompleteTodos(TodoDb db)
            {
                return TypedResults.Ok(await db.Todos.Where(t => t.IsComplete).ToListAsync());
            }

            static async Task<IResult> GetTodo(int id, TodoDb db)
            {
                return await db.Todos.FindAsync(id)
                    is Todo todo
                        ? TypedResults.Ok(todo)
                        : TypedResults.NotFound();
            }

            static async Task<IResult> CreateTodo(Todo todo, TodoDb db)
            {
                db.Todos.Add(todo);
                await db.SaveChangesAsync();

                return TypedResults.Created($"/todoitems/{todo.Id}", todo);
            }

            static async Task<IResult> UpdateTodo(int id, Todo inputTodo, TodoDb db)
            {
                var todo = await db.Todos.FindAsync(id);

                if (todo is null) return TypedResults.NotFound();

                todo.Name = inputTodo.Name;
                todo.IsComplete = inputTodo.IsComplete;

                await db.SaveChangesAsync();

                return TypedResults.NoContent();
            }

            static async Task<IResult> DeleteTodo(int id, TodoDb db)
            {
                if (await db.Todos.FindAsync(id) is Todo todo)
                {
                    db.Todos.Remove(todo);
                    await db.SaveChangesAsync();
                    return TypedResults.NoContent();
                }

                return TypedResults.NotFound();
            }
            //encrypt

            /*http://localhost:5068/todoitems/encryptAdi/HellowrldThisismyfirstmessage/keynumber1123456*/

            static async Task<IResult> EncryptAdi(string plainText, string key)
            {
                byte[] iv = new byte[16];
                byte[] buffer = Encoding.UTF8.GetBytes(plainText);
                AesCryptoServiceProvider aes = new AesCryptoServiceProvider();
                aes.Key = Encoding.UTF8.GetBytes(key);//give error if the key is not 16 bytes
                aes.IV = iv;
                string aux1 = Convert.ToBase64String(buffer, 0, 16);//gives error when plain text is small
                string aux2 = Convert.ToBase64String(aes.CreateEncryptor().TransformFinalBlock(buffer, 0, buffer.Length));
                if (string.IsNullOrEmpty(aux2))
                {
                    return TypedResults.NotFound();
                }
                else
                {
                    return TypedResults.Ok(aux2);

                }
            }

            static bool StringsAreEqual(string a, string b)
            {
                if (string.IsNullOrEmpty(a))
                {
                    return string.IsNullOrEmpty(b);
                }
                else
                {
                    return string.Equals(a, b);
                }
            }



        }

    }
}
