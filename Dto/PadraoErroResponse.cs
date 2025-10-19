namespace TodoApi.Dto;

public class PadraoErroResponse
{
    public string MensagemErro { get; set; }

    public PadraoErroResponse(string mensagemErro)
    {
        MensagemErro = mensagemErro;
    }
}