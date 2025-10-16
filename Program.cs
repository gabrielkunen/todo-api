using Npgsql;
using TodoApi.Dto;
using TodoApi.Entidades;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

app.MapPost("/usuarios", (CriarUsuarioRequest request) =>
    {
        if (string.IsNullOrWhiteSpace(request.Email) 
            || string.IsNullOrWhiteSpace(request.Senha)
            || string.IsNullOrWhiteSpace(request.SenhaRepetida) 
            || string.IsNullOrWhiteSpace(request.Nome))
            return Results.BadRequest("Usuário inválido.");
        
        if (request.Senha != request.SenhaRepetida)
            return Results.BadRequest("Senhas precisam ser iguais.");
        
        var senhaCriptografa = BCrypt.Net.BCrypt.HashPassword(request.Senha);
        var usuario = new Usuario(request.Email, senhaCriptografa, request.Nome);

        var connectionString = "Host=localhost;Port=10400;Username=user;Password=senha123;Database=todoapi";
        using var con = new NpgsqlConnection(connectionString);
        
        var sql = "INSERT INTO USUARIOS(EMAIL, SENHA, NOME) VALUES (@email, @senha, @nome)";
        
        NpgsqlCommand command = new(sql, con);
        command.Parameters.AddWithValue("@email", usuario.Email);
        command.Parameters.AddWithValue("@senha", usuario.Senha);
        command.Parameters.AddWithValue("@nome", usuario.Nome);
        
        con.Open();
        command.ExecuteNonQuery();

        return Results.Created();
    })
    .WithName("usuarios");

app.Run();