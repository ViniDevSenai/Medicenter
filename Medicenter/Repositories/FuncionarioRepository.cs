using Dapper;
using Medicenter.Data;
using Medicenter.Models;

namespace Medicenter.Repositories;

public static class FuncionarioRepository
{
    private const string SelectBase = @"
        SELECT f.*, u.login, u.perfil
          FROM funcionario f
          JOIN usuario u ON u.id_usuario = f.id_usuario";

    public static List<Funcionario> Listar(string filtro)
    {
        using var c = Db.Abrir();
        return c.Query<Funcionario>(SelectBase + @"
            WHERE f.nome LIKE @nome OR f.cargo LIKE @nome
            ORDER BY f.nome",
            new { nome = $"%{filtro?.Trim()}%" }).ToList();
    }

    public static Funcionario ObterPorId(int id)
    {
        using var c = Db.Abrir();
        return c.QueryFirstOrDefault<Funcionario>(SelectBase + " WHERE f.id_funcionario = @id", new { id });
    }

    public static void Salvar(Funcionario f, string login, string senha)
    {
        using var c = Db.Abrir();
        using var tx = c.BeginTransaction();

        if (f.IdFuncionario == 0)
        {
            f.IdUsuario = UsuarioRepository.Inserir(c, tx, login, senha, f.Perfil);
            f.IdFuncionario = c.ExecuteScalar<int>(@"
                INSERT INTO funcionario (id_usuario, nome, cpf, cargo, telefone, email, data_admissao, ativo)
                VALUES (@IdUsuario, @Nome, @Cpf, @Cargo, @Telefone, @Email, @DataAdmissao, @Ativo);
                SELECT LAST_INSERT_ID();", f, tx);
        }
        else
        {
            UsuarioRepository.Atualizar(c, tx, f.IdUsuario, login, senha, f.Perfil, f.Ativo);
            c.Execute(@"
                UPDATE funcionario
                   SET nome = @Nome, cpf = @Cpf, cargo = @Cargo, telefone = @Telefone,
                       email = @Email, data_admissao = @DataAdmissao, ativo = @Ativo
                 WHERE id_funcionario = @IdFuncionario", f, tx);
        }

        tx.Commit();
    }

    /// Não exclui: inativa o funcionário e o login, preservando o histórico.
    public static void Inativar(int id)
    {
        using var c = Db.Abrir();
        c.Execute(@"
            UPDATE funcionario f
              JOIN usuario u ON u.id_usuario = f.id_usuario
               SET f.ativo = 0, u.ativo = 0
             WHERE f.id_funcionario = @id", new { id });
    }
}
