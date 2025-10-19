using TodoApi.Entidades;

namespace TodoApi.Data;

public interface IUsuarioRepository
{
    void Adicionar(Usuario usuario);
    Usuario? BuscarPorEmail(string email);
}