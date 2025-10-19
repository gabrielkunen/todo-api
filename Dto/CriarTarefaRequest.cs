using FluentValidation;

namespace TodoApi.Dto;

public class CriarTarefaRequest
{
    public string Titulo { get; set; }
    public string Descricao { get; set; }
}

public class CriarTarefaRequestValidator : AbstractValidator<CriarTarefaRequest>
{
    public CriarTarefaRequestValidator()
    {
        RuleFor(x => x.Titulo)
            .NotEmpty()
            .WithMessage("Titulo é obrigatório");
        
        RuleFor(x => x.Descricao)
            .NotEmpty()
            .WithMessage("Descricao é obrigatória");
    }
}