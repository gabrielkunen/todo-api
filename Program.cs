using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
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

app.UseAuthentication();
app.UseAuthorization();

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

    var connectionString = "Host=localhost;Port=10400;Username=user;Password=senha123;Database=todoapi";
    var sql = "INSERT INTO USUARIOS(EMAIL, SENHA, NOME) VALUES (@email, @senha, @nome)";
    using var con = new NpgsqlConnection(connectionString);
    
    con.Execute(sql, new {
        request.Email,
        senha = senhaCriptografa,
        request.Nome
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
    
    var senhaValida = BCrypt.Net.BCrypt.Verify(request.Senha, usuario.Senha);
    if (!senhaValida)
        return Results.BadRequest("Senha incorreta.");
    
    var handler = new JwtSecurityTokenHandler();
    var key = "e7477fbd-29f1-4698-8aed-a35276bfe197"u8.ToArray();
    var credentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature);
    
    var ci = new ClaimsIdentity();
    ci.AddClaim(new Claim("UserId", usuario.Id.ToString()));
    
    var tokenDescriptor = new SecurityTokenDescriptor
    {
        Subject = ci,
        Expires = DateTime.UtcNow.AddHours(2),
        SigningCredentials = credentials
    };
    var token = handler.CreateToken(tokenDescriptor);
    var tokenFinal = handler.WriteToken(token);
    
    return Results.Ok(tokenFinal);
});

app.MapPost("/tarefas", (CriarTarefaRequest request, HttpContext httpContext) =>
{
    if (string.IsNullOrWhiteSpace(request.Titulo)
        || string.IsNullOrWhiteSpace(request.Descricao))
        return Results.BadRequest("Tarefa inválida.");

    var userId = httpContext.User.FindFirst("UserId")?.Value;
    
    var connectionString = "Host=localhost;Port=10400;Username=user;Password=senha123;Database=todoapi";
    var sql = "INSERT INTO TAREFAS(TITULO, DESCRICAO, STATUS, IDUSUARIO, DATAABERTURA) " +
              "VALUES (@titulo, @descricao, @status, @idUsuario, @dataAbertura)";
    using var con = new NpgsqlConnection(connectionString);
    
    con.Execute(sql, new
    {
        request.Titulo,
        request.Descricao,
        status = 0,
        idUsuario = int.Parse(userId),
        dataAbertura = DateTime.UtcNow
    });

    return Results.Created();
}).RequireAuthorization();

app.MapPatch("/tarefas/{id}", (int id, AtualizarTarefaRequest request, HttpContext httpContext) =>
{
    if (string.IsNullOrWhiteSpace(request.Titulo)
        || string.IsNullOrWhiteSpace(request.Descricao))
        return Results.BadRequest("Tarefa inválida.");

    var userId = httpContext.User.FindFirst("UserId")?.Value;
    
    
    var connectionString = "Host=localhost;Port=10400;Username=user;Password=senha123;Database=todoapi";
    var sqlBuscarTarefa = "SELECT * FROM TAREFAS WHERE ID = @id AND IDUSUARIO = @idUsuario";
    
    using var con = new NpgsqlConnection(connectionString);
    
    var tarefa = con.QueryFirstOrDefault<Tarefa>(sqlBuscarTarefa, new
    {
        id,
        idUsuario = int.Parse(userId)
    });
    
    if (tarefa == null)
        return Results.NotFound("Tarefa não cadastrada.");
    
    var sql = "UPDATE TAREFAS SET TITULO = @titulo, DESCRICAO = @descricao WHERE ID = @idTarefa";
    con.Execute(sql, new
    {
        request.Titulo,
        request.Descricao,
        idTarefa = tarefa.Id
    });

    return Results.Ok();
}).RequireAuthorization();

app.MapDelete("/tarefas/{id}", (int id, HttpContext httpContext) =>
{
    var userId = httpContext.User.FindFirst("UserId")?.Value;
    
    var connectionString = "Host=localhost;Port=10400;Username=user;Password=senha123;Database=todoapi";
    var sqlBuscarTarefa = "SELECT * FROM TAREFAS WHERE ID = @id AND IDUSUARIO = @idUsuario";
    
    using var con = new NpgsqlConnection(connectionString);
    
    var tarefa = con.QueryFirstOrDefault<Tarefa>(sqlBuscarTarefa, new
    {
        id,
        idUsuario = int.Parse(userId)
    });
    
    if (tarefa == null)
        return Results.NotFound("Tarefa não cadastrada.");
    
    var sql = "DELETE FROM TAREFAS WHERE ID = @idTarefa";
    con.Execute(sql, new
    {
        idTarefa = tarefa.Id
    });

    return Results.NoContent();
}).RequireAuthorization();

app.MapPatch("/tarefas/{id}/iniciar", (int id, HttpContext httpContext) =>
{
    var userId = httpContext.User.FindFirst("UserId")?.Value;
    
    var connectionString = "Host=localhost;Port=10400;Username=user;Password=senha123;Database=todoapi";
    var sqlBuscarTarefa = "SELECT * FROM TAREFAS WHERE ID = @id AND IDUSUARIO = @idUsuario";
    
    using var con = new NpgsqlConnection(connectionString);
    
    var tarefa = con.QueryFirstOrDefault<Tarefa>(sqlBuscarTarefa, new
    {
        id,
        idUsuario = int.Parse(userId)
    });
    
    if (tarefa == null)
        return Results.NotFound("Tarefa não cadastrada.");
    
    var sql = "UPDATE TAREFAS SET STATUS = @status, DATAINICIO = @datainicio WHERE ID = @idTarefa";
    con.Execute(sql, new
    {
        status = 1,
        datainicio = DateTime.UtcNow,
        idTarefa = tarefa.Id
    });

    return Results.NoContent();
}).RequireAuthorization();

app.MapPatch("/tarefas/{id}/finalizar", (int id, FinalizarTarefaRequest request, HttpContext httpContext) =>
{
    var userId = httpContext.User.FindFirst("UserId")?.Value;
    
    var connectionString = "Host=localhost;Port=10400;Username=user;Password=senha123;Database=todoapi";
    var sqlBuscarTarefa = "SELECT * FROM TAREFAS WHERE ID = @id AND IDUSUARIO = @idUsuario";
    
    using var con = new NpgsqlConnection(connectionString);
    
    var tarefa = con.QueryFirstOrDefault<Tarefa>(sqlBuscarTarefa, new
    {
        id,
        idUsuario = int.Parse(userId)
    });
    
    if (tarefa == null)
        return Results.NotFound("Tarefa não cadastrada.");
    
    var sql = "UPDATE TAREFAS SET STATUS = @status, DATAFIM = @datafim, OBSERVACAO = @observacao WHERE ID = @idTarefa";
    con.Execute(sql, new
    {
        status = 2,
        datafim = DateTime.UtcNow,
        idTarefa = tarefa.Id,
        observacao = request.Observacao
    });

    return Results.NoContent();
}).RequireAuthorization();

app.MapGet("/tarefas", (int? status, HttpContext httpContext) =>
{
    var userId = httpContext.User.FindFirst("UserId")?.Value;

    var connectionString = "Host=localhost;Port=10400;Username=user;Password=senha123;Database=todoapi";
    var sqlBuscarTarefa = "SELECT * FROM TAREFAS WHERE IDUSUARIO = @idUsuario";

    var parameters = new DynamicParameters();
    parameters.Add("idUsuario", int.Parse(userId));

    if (status != null)
    {
        sqlBuscarTarefa += " AND  STATUS = @status";
        parameters.Add("status", status);
    }

    using var con = new NpgsqlConnection(connectionString);
    var tarefas = con.Query<Tarefa>(sqlBuscarTarefa, parameters).ToList();
    
    return Results.Ok(tarefas);
}).RequireAuthorization();

app.Run();