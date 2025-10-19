using TodoApi.Entidades;
using TodoApi.Exceptions;

namespace TodoApi.EstadoTarefa;

public class TarefaCriada : IEstadoTarefa
{
    public void Iniciar(Tarefa tarefa)
    {
        tarefa.SetEstado(new TarefaIniciada());
    }

    public void Finalizar(Tarefa tarefa)
    {
        throw new DomainException("Tarefa não foi iniciada");
    }
}