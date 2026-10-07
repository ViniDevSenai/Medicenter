using Dapper;
using Medicenter.Data;
using Medicenter.Models;

namespace Medicenter.Services;

public static class AuthService
{
    //Tenta entrar. Retorna null se deu certo ou a mensagem de erro.
    public static string Entrar(string login, string senha)
    {
        using var c = Db.Abrir();

        var usuario = c.QueryFirstOrDefault<Usuario>(
            "SELECT * FROM usuario WHERE login = @login", new { login });

        if (usuario == null || !string.Equals(usuario.SenhaHash, Texto.Hash(senha), StringComparison.OrdinalIgnoreCase))
            return "Login ou senha inválidos.";

        if (!usuario.Ativo)
            return "Este acesso está inativo. Procure a administração da clínica.";

        var id = usuario.IdUsuario;
        var idMedico = c.ExecuteScalar<int?>("SELECT id_medico FROM medico WHERE id_usuario = @id", new { id });
        var idFuncionario = c.ExecuteScalar<int?>("SELECT id_funcionario FROM funcionario WHERE id_usuario = @id", new { id });
        var idPaciente = c.ExecuteScalar<int?>("SELECT id_paciente FROM paciente WHERE id_usuario = @id", new { id });

        if (usuario.Perfil == Perfis.Medico && idMedico == null)
            return "Cadastro de médico não encontrado para este login.";
        if (usuario.Perfil == Perfis.Paciente && idPaciente == null)
            return "Cadastro de paciente não encontrado para este login.";

        var nome = c.ExecuteScalar<string>(@"
            SELECT COALESCE(
                (SELECT nome FROM medico      WHERE id_usuario = @id),
                (SELECT nome FROM funcionario WHERE id_usuario = @id),
                (SELECT nome FROM paciente    WHERE id_usuario = @id))", new { id }) ?? usuario.Login;

        c.Execute("UPDATE usuario SET ultimo_acesso = NOW() WHERE id_usuario = @id", new { id });

        Sessao.Iniciar(usuario, nome, idMedico, idFuncionario, idPaciente);
        return null;
    }
}
