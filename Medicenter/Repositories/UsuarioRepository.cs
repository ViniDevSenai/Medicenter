using Dapper;
using Medicenter.Data;
using Medicenter.Services;
using MySqlConnector;

namespace Medicenter.Repositories;

public static class UsuarioRepository
{
    private static void ValidarLogin(MySqlConnection c, MySqlTransaction tx, string login, int idIgnorar)
    {
        var existe = c.ExecuteScalar<int>(
            "SELECT COUNT(*) FROM usuario WHERE login = @login AND id_usuario <> @idIgnorar",
            new { login, idIgnorar }, tx);

        if (existe > 0)
            throw new InvalidOperationException($"O login \"{login}\" já está em uso.");
    }

    public static int Inserir(MySqlConnection c, MySqlTransaction tx, string login, string senha, string perfil)
    {
        ValidarLogin(c, tx, login, 0);
        return c.ExecuteScalar<int>(
            "INSERT INTO usuario (login, senha_hash, perfil) VALUES (@login, @hash, @perfil); SELECT LAST_INSERT_ID();",
            new { login, hash = Texto.Hash(senha), perfil }, tx);
    }

    //Senha, perfil e ativo só são alterados quando informados.
    public static void Atualizar(MySqlConnection c, MySqlTransaction tx, int id, string login,
                                 string senha, string perfil = null, bool? ativo = null)
    {
        ValidarLogin(c, tx, login, id);

        var sql = "UPDATE usuario SET login = @login";
        if (!string.IsNullOrEmpty(senha)) sql += ", senha_hash = @hash";
        if (perfil != null) sql += ", perfil = @perfil";
        if (ativo.HasValue) sql += ", ativo = @ativo";
        sql += " WHERE id_usuario = @id";

        var hash = string.IsNullOrEmpty(senha) ? null : Texto.Hash(senha);
        c.Execute(sql, new { id, login, hash, perfil, ativo }, tx);
    }

    public static bool AlterarSenha(int id, string senhaAtual, string novaSenha)
    {
        using var c = Db.Abrir();
        var hashAtual = c.ExecuteScalar<string>("SELECT senha_hash FROM usuario WHERE id_usuario = @id", new { id });

        if (!string.Equals(hashAtual, Texto.Hash(senhaAtual), StringComparison.OrdinalIgnoreCase))
            return false;

        c.Execute("UPDATE usuario SET senha_hash = @hash WHERE id_usuario = @id",
                  new { id, hash = Texto.Hash(novaSenha) });
        return true;
    }
}
