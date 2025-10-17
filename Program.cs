using System.IdentityModel.Tokens.Jwt;
using System.Text;
using Dapper;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Npgsql;
using TodoApi.Dto;
using TodoApi.Entidades;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        options.SaveToken = true;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            IssuerSigningKey = new SymmetricSecurityKey("e7477fbd-29f1-4698-8aed-a35276bfe197"u8.ToArray()),
            ValidateIssuer = false,
            ValidateAudience = false
        };
    });

builder.Services.AddAuthorization();

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
    var sql = "INSERT INTO USUARIOS(EMAIL, SENHA, NOME) VALUES (@email, @senha, @nome)";
    using var con = new NpgsqlConnection(connectionString);
    
    con.Execute(sql, new {
        usuario.Email,
        usuario.Senha,
        usuario.Nome
    });
    
    return Results.Created();
});

app.MapPost("/autenticacoes", (LoginRequest request) =>
{
    if (string.IsNullOrWhiteSpace(request.Email)
        || string.IsNullOrWhiteSpace(request.Senha))
        return Results.BadRequest("Senha incorreta.");
    
    var connectionString = "Host=localhost;Port=10400;Username=user;Password=senha123;Database=todoapi";
    var sqlBuscarUsuario = "SELECT * FROM USUARIOS WHERE EMAIL = @email";
    
    using var con = new NpgsqlConnection(connectionString);
    
    var usuario = con.QueryFirstOrDefault<Usuario>(sqlBuscarUsuario, new
    {
        request.Email
    });
    
    if (usuario == null)
        return Results.BadRequest("Senha incorreta.");
    
    var senhaValida =  BCrypt.Net.BCrypt.Verify(request.Senha, usuario.Senha);
    if (!senhaValida)
        return Results.BadRequest("Senha incorreta.");
    
    var handler = new JwtSecurityTokenHandler();
    var key = "e7477fbd-29f1-4698-8aed-a35276bfe197"u8.ToArray();
    var credentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature);
    
    var tokenDescriptor = new SecurityTokenDescriptor
    {
        Expires = DateTime.UtcNow.AddHours(2),
        SigningCredentials = credentials
    };
    var token = handler.CreateToken(tokenDescriptor);
    var tokenFinal = handler.WriteToken(token);
    
    return Results.Ok(tokenFinal);
});

app.MapGet("/teste-auth", () =>
{
    return Results.Ok("sucesso");
}).RequireAuthorization();

app.Run();