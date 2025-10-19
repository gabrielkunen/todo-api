using FluentValidation;

namespace TodoApi.Dto;

public class CriarUsuarioRequest
{
    public string Email { get; set; }
    public string Senha { get; set; }
    public string SenhaRepetida { get; set; }
    public string Nome { get; set; }
}

public class CriarUsuarioRequestValidator : AbstractValidator<CriarUsuarioRequest>
{
    public CriarUsuarioRequestValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty()
            .WithMessage("Email é obrigatório");
        
        RuleFor(x => x.Senha)
            .NotEmpty()
            .WithMessage("Senha é obrigatória");
        
        RuleFor(x => x.SenhaRepetida)
            .NotEmpty()
            .WithMessage("SenhaRepetida é obrigatória");
        
        RuleFor(x => x.Nome)
            .NotEmpty()
            .WithMessage("Nome é obrigatório");
    }
}