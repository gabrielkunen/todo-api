using TodoApi.Entidades;
using TodoApi.Exceptions;

namespace TodoApi.EstadoTarefa;

public class TarefaIniciada : IEstadoTarefa
{
    public void Iniciar(Tarefa tarefa)
    {
        throw new DomainException("Tarefa já foi iniciada");
    }

    public void Finalizar(Tarefa tarefa)
    {
        tarefa.SetEstado(new TarefaFinalizada());
    }
}