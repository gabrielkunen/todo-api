using Dapper;
using TodoApi.Entidades;

namespace TodoApi.Data;

public class UsuarioRepository : IUsuarioRepository
{
    private readonly TodoApiContext _context;
    private readonly string _tableName;

    public UsuarioRepository(TodoApiContext context)
    {
        _context = context;
        _tableName = "USUARIOS";
    }

    public void Adicionar(Usuario usuario)
    {
        var sql = $"INSERT INTO {_tableName}(EMAIL, SENHA, NOME) VALUES (@email, @senha, @nome)";
        
        var parameters = new DynamicParameters();
        parameters.Add("email", usuario.Email);
        parameters.Add("senha", usuario.Senha);
        parameters.Add("nome", usuario.Nome);
        
        using var connection = _context.CreateConnection();
        connection.Execute(sql, parameters);
    }
    
    public Usuario? BuscarPorEmail(string email)
    {
        var sql =  $"SELECT * FROM {_tableName} WHERE EMAIL = @email";
        
        var parameters = new DynamicParameters();
        parameters.Add("email", email);

        using var connection = _context.CreateConnection();
        return connection.QueryFirstOrDefault<Usuario>(sql, parameters);
    }
}