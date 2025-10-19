namespace TodoApi.Entidades;

public class Tarefa
{
    public int Id { get; private set; }
    public string Titulo { get; private set; }
    public string Descricao { get; private set; }
    public string Observacao { get; private set; }
    public int Status { get; private set; }
    public int IdUsuario { get; private set; }
    public DateTime DataAbertura { get; private set; }
    public DateTime? DataInicio { get; private set; }
    public DateTime? DataFim { get; private set; }
    public Tarefa() {}

    public Tarefa(string titulo, string descricao, int status, int idUsuario, DateTime dataAbertura)
    {
        Titulo = titulo;
        Descricao = descricao;
        Status = status;
        IdUsuario = idUsuario;
        DataAbertura = dataAbertura;
    }

    public void AtualizarDados(string titulo, string descricao)
    {
        Titulo = titulo;
        Descricao = descricao;
    }

    public void Iniciar()
    {
        Status = 1;
        DataInicio = DateTime.UtcNow;
    }

    public void Finalizar(string observacao)
    {
        Status = 2;
        DataFim = DateTime.UtcNow;
        Observacao = observacao;
    }
}