namespace TodoApi.Dto;

public class CriarUsuarioResponse
{
    public string Email { get; set; }
    public string Nome { get; set; }

    public CriarUsuarioResponse(string email, string nome)
    {
        Email = email;
        Nome = nome;
    }
}