using System.Text;
using Dapper;
using TodoApi.Entidades;
using TodoApi.EstadoTarefa;

namespace TodoApi.Data;

public class TarefaRepository : ITarefaRepository
{
    private readonly TodoApiContext _context;
    private readonly string _tableName;

    public TarefaRepository(TodoApiContext context)
    {
        _context = context;
        _tableName = "TAREFAS";
    }
    
    public void Adicionar(Tarefa tarefa)
    {
        var sql = $"INSERT INTO {_tableName}(TITULO, DESCRICAO, STATUS, IDUSUARIO, DATAABERTURA) " +
                               "VALUES (@titulo, @descricao, @status, @idUsuario, @dataAbertura)";
        
        var parameters = new DynamicParameters();
        parameters.Add("titulo", tarefa.Titulo);
        parameters.Add("descricao", tarefa.Descricao);
        parameters.Add("status", tarefa.StatusNome);
        parameters.Add("idUsuario", tarefa.IdUsuario);
        parameters.Add("dataAbertura", tarefa.DataAbertura);
        
        using var connection = _context.CreateConnection();
        connection.Execute(sql, parameters);
    }

    public Tarefa? Buscar(int id, int idUsuario)
    {
        var sql = $"SELECT * FROM {_tableName} WHERE ID = @id AND IDUSUARIO = @idUsuario";
        
        var parameters = new DynamicParameters();
        parameters.Add("id", id);
        parameters.Add("idUsuario", idUsuario);
        
        using var connection = _context.CreateConnection();
        var retorno = connection.QueryFirstOrDefault(sql, parameters);

        if (retorno == null)
            return null;

        return MapearResultadoDbParaTarefa(retorno);
    }

    public List<Tarefa> Buscar(int idUsuario, string? status)
    {
        var sql = new StringBuilder($"SELECT * FROM {_tableName} WHERE IDUSUARIO = @idUsuario");
        
        var parameters = new DynamicParameters();
        parameters.Add("idUsuario", idUsuario);
        
        if (status != null)
        {
            sql.Append(" AND STATUS = @status");
            parameters.Add("status", status);
        }
        
        using var connection = _context.CreateConnection();
        var retorno = connection.Query(sql.ToString(), parameters);
        
        var listaRetorno = new List<Tarefa>();
        foreach (var tarefaDb in retorno)
            listaRetorno.Add(MapearResultadoDbParaTarefa(tarefaDb));

        return listaRetorno;
    }

    public void Atualizar(Tarefa tarefa)
    {
        var sql = $"UPDATE {_tableName} " +
                  $"SET TITULO = @titulo, " +
                  $"DESCRICAO = @descricao, " +
                  $"OBSERVACAO = @observacao, " +
                  $"STATUS = @status, " +
                  $"DATAINICIO = @dataInicio, " +
                  $"DATAFIM = @dataFim " +
                  $"WHERE ID = @id";
        
        var parameters = new DynamicParameters();
        parameters.Add("titulo", tarefa.Titulo);
        parameters.Add("descricao", tarefa.Descricao);
        parameters.Add("observacao", tarefa.Observacao);
        parameters.Add("status", tarefa.StatusNome);
        parameters.Add("dataInicio", tarefa.DataInicio);
        parameters.Add("dataFim", tarefa.DataFim);
        parameters.Add("id", tarefa.Id);
        
        using var connection = _context.CreateConnection();
        connection.Execute(sql, parameters);
    }

    public void Deletar(int id)
    {
        var sql = $"DELETE FROM {_tableName} WHERE ID = @id";
        
        var parameters = new DynamicParameters();
        parameters.Add("id", id);
        
        using var connection = _context.CreateConnection();
        connection.Execute(sql, parameters);
    }

    private Tarefa MapearResultadoDbParaTarefa(dynamic retorno)
    {
        return new Tarefa(
            retorno.id,
            retorno.titulo,
            retorno.descricao,
            retorno.observacao,
            MapearStringTarefaStatusParaClasse((string)retorno.status),
            retorno.idusuario,
            retorno.dataabertura,
            retorno.datainicio,
            retorno.datafim
        );
    }
    
    private IEstadoTarefa? MapearStringTarefaStatusParaClasse(string status)
    {
        return status switch
        {
            "TarefaCriada" => new TarefaCriada(),
            "TarefaIniciada" => new TarefaIniciada(),
            "TarefaFinalizada" => new TarefaFinalizada(),
            _ => throw new ArgumentException("Status da tarefa inválido")
        };
    }
}