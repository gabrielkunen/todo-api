using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using FluentValidation;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using TodoApi.Data;
using TodoApi.Dto;
using TodoApi.Entidades;
using TodoApi.Filters;
using TodoApi.Middlewares;
using TodoApi.Network;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddScoped<ITarefaRepository, TarefaRepository>();
builder.Services.AddScoped<IUsuarioRepository, UsuarioRepository>();
builder.Services.AddSingleton<TodoApiContext>();
builder.Services.AddScoped<IUserContext, HttpUserContext>();
builder.Services.AddValidatorsFromAssemblyContaining<Program>();

builder.Services.AddHttpContextAccessor();

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
app.UseMiddleware<ExceptionMiddleware>();

app.MapPost("/usuarios", (CriarUsuarioRequest request, IUsuarioRepository usuarioRepository) =>
{
    if (request.Senha != request.SenhaRepetida)
        return Results.BadRequest(new PadraoErroResponse("Senhas precisam ser iguais."));

    var usuarioExistente = usuarioRepository.BuscarPorEmail(request.Email);
    if (usuarioExistente != null)
        return Results.BadRequest(new PadraoErroResponse($"Usuário com email {request.Email} já cadastrado."));
    
    var senhaCriptografa = BCrypt.Net.BCrypt.HashPassword(request.Senha);

    var usuario = new Usuario(request.Email, senhaCriptografa, request.Nome);
    
    usuarioRepository.Adicionar(usuario);

    return Results.Created(
        string.Empty, 
        new CriarUsuarioResponse(usuario.Email, usuario.Nome));
}).AddEndpointFilter<ValidationFilter<CriarUsuarioRequest>>();

app.MapPost("/autenticacoes", (LoginRequest request, IUsuarioRepository usuarioRepository) =>
{
    var usuario = usuarioRepository.BuscarPorEmail(request.Email);
    
    if (usuario == null)
        return Results.BadRequest(new PadraoErroResponse("Senha incorreta."));
    
    var senhaValida = BCrypt.Net.BCrypt.Verify(request.Senha, usuario.Senha);
    if (!senhaValida)
        return Results.BadRequest(new PadraoErroResponse("Senha incorreta."));
    
    var handler = new JwtSecurityTokenHandler();
    var key = "e7477fbd-29f1-4698-8aed-a35276bfe197"u8.ToArray();
    var credentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature);
    
    var ci = new ClaimsIdentity();
    ci.AddClaim(new Claim(ClaimTypes.NameIdentifier, usuario.Id.ToString()));
    
    var tokenDescriptor = new SecurityTokenDescriptor
    {
        Subject = ci,
        Expires = DateTime.UtcNow.AddHours(2),
        SigningCredentials = credentials
    };
    var token = handler.CreateToken(tokenDescriptor);
    var tokenFinal = handler.WriteToken(token);

    return Results.Ok(new LoginResponse { Token = tokenFinal });
}).AddEndpointFilter<ValidationFilter<LoginRequest>>();

app.MapPost("/tarefas", (CriarTarefaRequest request, IUserContext userContext, ITarefaRepository tarefaRepository) =>
{
    var tarefa = new Tarefa(request.Titulo, request.Descricao, userContext.IdUsuarioLogado, DateTime.UtcNow);
    var id = tarefaRepository.Adicionar(tarefa);
    return Results.Created(
        string.Empty, 
        new CriarTarefaResponse(id, tarefa.Titulo, tarefa.Descricao, tarefa.StatusNome, tarefa.DataAbertura.ToString("yyyy-MM-dd HH:mm:ss")));
    
}).RequireAuthorization().AddEndpointFilter<ValidationFilter<CriarTarefaRequest>>();

app.MapPatch("/tarefas/{id}", (int id, AtualizarTarefaRequest request, IUserContext userContext, ITarefaRepository  tarefaRepository) =>
{
    var tarefa = tarefaRepository.Buscar(id, userContext.IdUsuarioLogado);
    
    if (tarefa == null)
        return Results.NotFound(new PadraoErroResponse("Tarefa não cadastrada."));

    tarefa.AtualizarDados(request.Titulo, request.Descricao);
    
    tarefaRepository.Atualizar(tarefa);
    
    return Results.NoContent();
}).RequireAuthorization().AddEndpointFilter<ValidationFilter<AtualizarTarefaRequest>>();

app.MapDelete("/tarefas/{id}", (int id, IUserContext userContext, ITarefaRepository  tarefaRepository) =>
{
    var tarefa = tarefaRepository.Buscar(id, userContext.IdUsuarioLogado);
    
    if (tarefa == null)
        return Results.NotFound(new PadraoErroResponse("Tarefa não cadastrada."));
    
    tarefaRepository.Deletar(tarefa.Id);

    return Results.NoContent();
}).RequireAuthorization();

app.MapPatch("/tarefas/{id}/iniciar", (int id, IUserContext userContext, ITarefaRepository  tarefaRepository) =>
{
    var tarefa = tarefaRepository.Buscar(id, userContext.IdUsuarioLogado);
    
    if (tarefa == null)
        return Results.NotFound(new PadraoErroResponse("Tarefa não cadastrada."));

    tarefa.Iniciar();
    
    tarefaRepository.Atualizar(tarefa);
    
    return Results.NoContent();
}).RequireAuthorization();

app.MapPatch("/tarefas/{id}/finalizar", (int id, FinalizarTarefaRequest request, IUserContext userContext, ITarefaRepository tarefaRepository) =>
{
    var tarefa = tarefaRepository.Buscar(id, userContext.IdUsuarioLogado);
    
    if (tarefa == null)
        return Results.NotFound(new PadraoErroResponse("Tarefa não cadastrada."));

    tarefa.Finalizar(request.Observacao);
    
    tarefaRepository.Atualizar(tarefa);

    return Results.NoContent();
}).RequireAuthorization();

app.MapGet("/tarefas", (string? status, IUserContext userContext, ITarefaRepository tarefaRepository) =>
{
    var tarefas = tarefaRepository.Buscar(userContext.IdUsuarioLogado, status);
    return Results.Ok(tarefas);
}).RequireAuthorization();

app.Run();