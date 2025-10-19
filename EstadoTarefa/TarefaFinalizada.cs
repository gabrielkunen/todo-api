using TodoApi.Entidades;

namespace TodoApi.EstadoTarefa;

public class TarefaFinalizada : IEstadoTarefa
{
    public void Iniciar(Tarefa tarefa)
    {
        throw new Exception("Tarefa já foi finalizada");
    }

    public void Finalizar(Tarefa tarefa)
    {
        throw new Exception("Tarefa já foi finalizada");
    }
}