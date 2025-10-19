using TodoApi.EstadoTarefa;

namespace TodoApi.Entidades;

public class Tarefa
{
    public int Id { get; private set; }
    public string Titulo { get; private set; }
    public string Descricao { get; private set; }
    public string Observacao { get; private set; }
    public IEstadoTarefa Status { get; private set; }
    public string StatusNome => Status.GetType().Name;
    public int IdUsuario { get; private set; }
    public DateTime DataAbertura { get; private set; }
    public DateTime? DataInicio { get; private set; }
    public DateTime? DataFim { get; private set; }

    public Tarefa(string titulo, string descricao, int idUsuario, DateTime dataAbertura)
    {
        Titulo = titulo;
        Descricao = descricao;
        Status = new TarefaCriada();
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
        Status.Iniciar(this);
        DataInicio = DateTime.UtcNow;
    }

    public void Finalizar(string observacao)
    {
        Status.Finalizar(this);
        DataFim = DateTime.UtcNow;
        Observacao = observacao;
    }

    public void SetEstado(IEstadoTarefa estado)
    {
        Status = estado;
    }

    // Dapper
    public Tarefa(int id, string titulo, string descricao, string observacao, IEstadoTarefa status, int idUsuario, DateTime dataAbertura, DateTime? dataInicio, DateTime? dataFim)
    {
        Id = id;
        Titulo = titulo;
        Descricao = descricao;
        Observacao = observacao;
        Status = status;
        IdUsuario = idUsuario;
        DataAbertura = dataAbertura;
        DataInicio = dataInicio;
        DataFim = dataFim;
    }
}