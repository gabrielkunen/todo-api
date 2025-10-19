using System.Security.Claims;

namespace TodoApi.Network;

public class HttpUserContext : IUserContext
{
    private readonly IHttpContextAccessor _httpAccessor;
    public HttpUserContext(IHttpContextAccessor httpAccessor)
    {
        _httpAccessor = httpAccessor;
    }

    public int IdUsuarioLogado
    {
        get
        {
            var idUsuarioLogado = _httpAccessor.HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrWhiteSpace(idUsuarioLogado))
                throw new UnauthorizedAccessException("Token inválido.");

            if (!int.TryParse(idUsuarioLogado, out var idUsuarioLogadoInt))
                throw new UnauthorizedAccessException("Token inválido.");

            return idUsuarioLogadoInt;
        }
    }
}