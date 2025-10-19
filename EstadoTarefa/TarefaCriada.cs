using TodoApi.Entidades;

namespace TodoApi.EstadoTarefa;

public class TarefaCriada : IEstadoTarefa
{
    public void Iniciar(Tarefa tarefa)
    {
        tarefa.SetEstado(new TarefaIniciada());
    }

    public void Finalizar(Tarefa tarefa)
    {
        throw new Exception("Tarefa não foi iniciada");
    }
}