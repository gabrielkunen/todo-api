namespace TodoApi.Dto;

public class CriarTarefaResponse
{
    public int Id { get; set; }
    public string Titulo { get; set; }
    public string Descricao { get; set; }
    public string Status { get; set; }
    public string DataAbertura { get; set; }

    public CriarTarefaResponse(int id, string titulo, string descricao, string status, string dataAbertura)
    {
        Id = id;
        Titulo = titulo;
        Descricao = descricao;
        Status = status;
        DataAbertura = dataAbertura;
    }
}