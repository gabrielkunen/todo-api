using FluentValidation;

namespace TodoApi.Dto;

public class LoginRequest
{
    public string Email { get; set; }
    public string Senha { get; set; }
}

public class LoginRequestValidator : AbstractValidator<LoginRequest>
{
    public LoginRequestValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty()
            .WithMessage("Email é obrigatório");
        
        RuleFor(x => x.Senha)
            .NotEmpty()
            .WithMessage("Senha é obrigatória");
    }
}