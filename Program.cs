using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using TodoApi.Data;
using TodoApi.Dto;
using TodoApi.Entidades;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddScoped<ITarefaRepository, TarefaRepository>();
builder.Services.AddScoped<IUsuarioRepository, UsuarioRepository>();
builder.Services.AddSingleton<TodoApiContext>();

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

app.MapPost("/usuarios", (CriarUsuarioRequest request, IUsuarioRepository usuarioRepository) =>
{
    if (string.IsNullOrWhiteSpace(request.Email)
        || string.IsNullOrWhiteSpace(request.Senha)
        || string.IsNullOrWhiteSpace(request.SenhaRepetida)
        || string.IsNullOrWhiteSpace(request.Nome))
        return Results.BadRequest("Usuário inválido.");

    if (request.Senha != request.SenhaRepetida)
        return Results.BadRequest("Senhas precisam ser iguais.");

    var senhaCriptografa = BCrypt.Net.BCrypt.HashPassword(request.Senha);

    usuarioRepository.Adicionar(new Usuario(request.Email, senhaCriptografa, request.Nome));
    
    return Results.Created();
});

app.MapPost("/autenticacoes", (LoginRequest request, IUsuarioRepository usuarioRepository) =>
{
    if (string.IsNullOrWhiteSpace(request.Email)
        || string.IsNullOrWhiteSpace(request.Senha))
        return Results.BadRequest("Senha incorreta.");

    var usuario = usuarioRepository.BuscarPorEmail(request.Email);
    
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

app.MapPost("/tarefas", (CriarTarefaRequest request, HttpContext httpContext, ITarefaRepository tarefaRepository) =>
{
    if (string.IsNullOrWhiteSpace(request.Titulo)
        || string.IsNullOrWhiteSpace(request.Descricao))
        return Results.BadRequest("Tarefa inválida.");

    var userId = int.Parse(httpContext.User.FindFirst("UserId")?.Value);
    
    tarefaRepository.Adicionar(new Tarefa(request.Titulo, request.Descricao, 0, userId, DateTime.UtcNow));
    
    return Results.Created();
}).RequireAuthorization();

app.MapPatch("/tarefas/{id}", (int id, AtualizarTarefaRequest request, HttpContext httpContext, ITarefaRepository  tarefaRepository) =>
{
    if (string.IsNullOrWhiteSpace(request.Titulo)
        || string.IsNullOrWhiteSpace(request.Descricao))
        return Results.BadRequest("Tarefa inválida.");

    var userId = int.Parse(httpContext.User.FindFirst("UserId")?.Value);
    
    var tarefa = tarefaRepository.Buscar(id, userId);
    
    if (tarefa == null)
        return Results.NotFound("Tarefa não cadastrada.");

    tarefa.AtualizarDados(request.Titulo, request.Descricao);
    
    tarefaRepository.Atualizar(tarefa);
    
    return Results.Ok();
}).RequireAuthorization();

app.MapDelete("/tarefas/{id}", (int id, HttpContext httpContext, ITarefaRepository  tarefaRepository) =>
{
    var userId = int.Parse(httpContext.User.FindFirst("UserId")?.Value);
    
    var tarefa = tarefaRepository.Buscar(id, userId);
    
    if (tarefa == null)
        return Results.NotFound("Tarefa não cadastrada.");
    
    tarefaRepository.Deletar(tarefa.Id);

    return Results.NoContent();
}).RequireAuthorization();

app.MapPatch("/tarefas/{id}/iniciar", (int id, HttpContext httpContext, ITarefaRepository  tarefaRepository) =>
{
    var userId = int.Parse(httpContext.User.FindFirst("UserId")?.Value);
    
    
    var tarefa = tarefaRepository.Buscar(id, userId);
    
    if (tarefa == null)
        return Results.NotFound("Tarefa não cadastrada.");

    tarefa.Iniciar();
    
    tarefaRepository.Atualizar(tarefa);
    
    return Results.NoContent();
}).RequireAuthorization();

app.MapPatch("/tarefas/{id}/finalizar", (int id, FinalizarTarefaRequest request, HttpContext httpContext, ITarefaRepository tarefaRepository) =>
{
    var userId = int.Parse(httpContext.User.FindFirst("UserId")?.Value);

    var tarefa = tarefaRepository.Buscar(id, userId);
    
    if (tarefa == null)
        return Results.NotFound("Tarefa não cadastrada.");

    tarefa.Finalizar(request.Observacao);
    
    tarefaRepository.Atualizar(tarefa);

    return Results.NoContent();
}).RequireAuthorization();

app.MapGet("/tarefas", (int? status, HttpContext httpContext, ITarefaRepository tarefaRepository) =>
{
    var userId = int.Parse(httpContext.User.FindFirst("UserId")?.Value);

    var tarefas = tarefaRepository.Buscar(userId, status);
    
    return Results.Ok(tarefas);
}).RequireAuthorization();

app.Run();