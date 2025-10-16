namespace TodoApi.Entidades;

public class Tarefa
{
    public int Id { get; private set; }
    public string Titulo { get; private set; }
    public string Descricao { get; private set; }
    public string Observacao { get; private set; }
    public int Status { get; private set; }
    public DateTime DataAbertura { get; private set; }
    public DateTime? DataInicio { get; private set; }
    public DateTime? DataFim { get; private set; }
}