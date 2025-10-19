using TodoApi.Entidades;

namespace TodoApi.EstadoTarefa;

public class TarefaIniciada : IEstadoTarefa
{
    public void Iniciar(Tarefa tarefa)
    {
        throw new Exception("Tarefa já foi iniciada");
    }

    public void Finalizar(Tarefa tarefa)
    {
        tarefa.SetEstado(new TarefaFinalizada());
    }
}