using Dapper;
using Medicenter.Data;
using Medicenter.Models;

namespace Medicenter.Repositories;

public static class MedicoRepository
{
    private const string SelectBase = @"
        SELECT m.*, e.nome AS especialidade, u.login
          FROM medico m
          JOIN especialidade e ON e.id_especialidade = m.id_especialidade
          JOIN usuario u       ON u.id_usuario = m.id_usuario";

    public static List<Medico> Listar(string filtro, bool apenasAtivos)
    {
        using var c = Db.Abrir();
        return c.Query<Medico>(SelectBase + @"
            WHERE (m.nome LIKE @nome OR m.crm LIKE @nome)
              AND (@apenasAtivos = 0 OR m.ativo = 1)
            ORDER BY m.nome",
            new { nome = $"%{filtro?.Trim()}%", apenasAtivos = apenasAtivos ? 1 : 0 }).ToList();
    }

    public static Medico ObterPorId(int id)
    {
        using var c = Db.Abrir();
        return c.QueryFirstOrDefault<Medico>(SelectBase + " WHERE m.id_medico = @id", new { id });
    }

    public static void Salvar(Medico m, string login, string senha)
    {
        using var c = Db.Abrir();
        using var tx = c.BeginTransaction();

        if (m.IdMedico == 0)
        {
            m.IdUsuario = UsuarioRepository.Inserir(c, tx, login, senha, Perfis.Medico);
            m.IdMedico = c.ExecuteScalar<int>(@"
                INSERT INTO medico (id_usuario, id_especialidade, nome, cpf, crm, uf_crm, telefone, email, ativo)
                VALUES (@IdUsuario, @IdEspecialidade, @Nome, @Cpf, @Crm, @UfCrm, @Telefone, @Email, @Ativo);
                SELECT LAST_INSERT_ID();", m, tx);
        }
        else
        {
            UsuarioRepository.Atualizar(c, tx, m.IdUsuario, login, senha, null, m.Ativo);
            c.Execute(@"
                UPDATE medico
                   SET id_especialidade = @IdEspecialidade, nome = @Nome, cpf = @Cpf, crm = @Crm,
                       uf_crm = @UfCrm, telefone = @Telefone, email = @Email, ativo = @Ativo
                 WHERE id_medico = @IdMedico", m, tx);
        }

        tx.Commit();
    }

    //Não exclui: inativa o médico e o login, preservando o histórico.
    public static void Inativar(int id)
    {
        using var c = Db.Abrir();
        c.Execute(@"
            UPDATE medico m
              JOIN usuario u ON u.id_usuario = m.id_usuario
               SET m.ativo = 0, u.ativo = 0
             WHERE m.id_medico = @id", new { id });
    }
}
