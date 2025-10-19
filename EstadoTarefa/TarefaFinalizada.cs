using TodoApi.Entidades;
using TodoApi.Exceptions;

namespace TodoApi.EstadoTarefa;

public class TarefaFinalizada : IEstadoTarefa
{
    public void Iniciar(Tarefa tarefa)
    {
        throw new DomainException("Tarefa já foi finalizada");
    }

    public void Finalizar(Tarefa tarefa)
    {
        throw new DomainException("Tarefa já foi finalizada");
    }
}