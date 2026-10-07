using Medicenter.Models;

namespace Medicenter.Services;

//Usuário logado no momento.
public static class Sessao
{
    public static Usuario Usuario { get; private set; }
    public static string Nome { get; private set; }
    public static int? IdMedico { get; private set; }
    public static int? IdFuncionario { get; private set; }
    public static int? IdPaciente { get; private set; }

    public static string Perfil => Usuario?.Perfil;

    public static bool Eh(params string[] perfis) =>
        Usuario != null && perfis.Contains(Usuario.Perfil);

    public static void Iniciar(Usuario usuario, string nome, int? idMedico, int? idFuncionario, int? idPaciente)
    {
        Usuario = usuario;
        Nome = nome;
        IdMedico = idMedico;
        IdFuncionario = idFuncionario;
        IdPaciente = idPaciente;
    }

    public static void Encerrar()
    {
        Usuario = null;
        Nome = null;
        IdMedico = IdFuncionario = IdPaciente = null;
    }
}
