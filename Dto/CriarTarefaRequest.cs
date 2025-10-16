namespace TodoApi.Dto;

public class CriarTarefaRequest
{
    public string Titulo { get; set; }
    public string Descricao { get; set; }
    public string Observacao { get; set; }
}