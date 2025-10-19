using FluentValidation;

namespace TodoApi.Dto;

public class AtualizarTarefaRequest
{
    public string Titulo { get; set; }
    public string Descricao { get; set; }
}

public class AtualizarTarefaRequestValidator : AbstractValidator<AtualizarTarefaRequest>
{
    public AtualizarTarefaRequestValidator()
    {
        RuleFor(x => x.Titulo)
            .NotEmpty()
            .WithMessage("Titulo é obrigatório");
        
        RuleFor(x => x.Descricao)
            .NotEmpty()
            .WithMessage("Descricao é obrigatória");
    }
}