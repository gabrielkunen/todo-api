namespace TodoApi.Dto;

public class CriarUsuarioRequest
{
    public string Email { get; set; }
    public string Senha { get; set; }
    public string SenhaRepetida { get; set; }
    public string Nome { get; set; }
}