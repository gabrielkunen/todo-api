using TodoApi.Entidades;

namespace TodoApi.EstadoTarefa;

public interface IEstadoTarefa
{
    public void Iniciar(Tarefa tarefa);
    public void Finalizar(Tarefa tarefa);
}