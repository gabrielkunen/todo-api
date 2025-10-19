using System.Text;
using Dapper;
using TodoApi.Entidades;

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
        parameters.Add("status", tarefa.Status);
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
        return connection.QueryFirstOrDefault<Tarefa>(sql, parameters);
    }

    public List<Tarefa> Buscar(int idUsuario, int? status)
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
        return connection.Query<Tarefa>(sql.ToString(), parameters).ToList();
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
        parameters.Add("observacao", tarefa.Descricao);
        parameters.Add("status", tarefa.Status);
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
}