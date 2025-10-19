using TodoApi.Entidades;

namespace TodoApi.Data;

public interface ITarefaRepository
{
    int Adicionar(Tarefa tarefa);
    Tarefa? Buscar(int id, int idUsuario);
    List<Tarefa> Buscar(int idUsuario, string? status);
    void Atualizar(Tarefa tarefa);
    void Deletar(int id);
}