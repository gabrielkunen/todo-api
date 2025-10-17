namespace TodoApi.Entidades;

public class Usuario
{
    public int Id { get; private set; }
    public string Email { get; private set; }
    public string Senha { get; private set; }
    public string Nome { get; private set; }

    public Usuario() {}
    public Usuario(string email, string senha, string nome)
    {
        Email = email;
        Senha = senha;
        Nome = nome;
    }
}