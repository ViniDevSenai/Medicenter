using Dapper;
using Medicenter.Data;
using Medicenter.Models;
using MySqlConnector;

namespace Medicenter.Repositories;

public static class EspecialidadeRepository
{
    public static List<Especialidade> Listar(string filtro = "")
    {
        using var c = Db.Abrir();
        return c.Query<Especialidade>(
            "SELECT * FROM especialidade WHERE nome LIKE @nome ORDER BY nome",
            new { nome = $"%{filtro?.Trim()}%" }).ToList();
    }

    public static void Salvar(Especialidade e)
    {
        using var c = Db.Abrir();
        if (e.IdEspecialidade == 0)
            e.IdEspecialidade = c.ExecuteScalar<int>(
                "INSERT INTO especialidade (nome) VALUES (@Nome); SELECT LAST_INSERT_ID();", e);
        else
            c.Execute("UPDATE especialidade SET nome = @Nome WHERE id_especialidade = @IdEspecialidade", e);
    }

    public static void Excluir(int id)
    {
        try
        {
            using var c = Db.Abrir();
            c.Execute("DELETE FROM especialidade WHERE id_especialidade = @id", new { id });
        }
        catch (MySqlException ex) when (ex.Number == 1451)
        {
            throw new InvalidOperationException("Existem médicos com essa especialidade. Altere o cadastro deles antes de excluir.");
        }
    }
}
