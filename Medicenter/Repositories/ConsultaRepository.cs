using Dapper;
using Medicenter.Data;
using Medicenter.Models;
using MySqlConnector;

namespace Medicenter.Repositories;

public static class ConsultaRepository
{
    private const string SelectAgenda = @"
        SELECT c.id_consulta, c.data_hora, c.duracao_min, c.status, c.observacoes,
               p.id_paciente, p.nome AS paciente, p.telefone AS telefone_paciente,
               m.id_medico, m.nome AS medico, e.nome AS especialidade
          FROM consulta c
          JOIN paciente p      ON p.id_paciente = c.id_paciente
          JOIN medico m        ON m.id_medico = c.id_medico
          JOIN especialidade e ON e.id_especialidade = m.id_especialidade";

    public static List<AgendaItem> Listar(DateTime de, DateTime ate, int? idMedico, int? idPaciente, string status)
    {
        using var c = Db.Abrir();
        return c.Query<AgendaItem>(SelectAgenda + @"
            WHERE c.data_hora >= @de AND c.data_hora < @ateFim
              AND (@idMedico IS NULL OR c.id_medico = @idMedico)
              AND (@idPaciente IS NULL OR c.id_paciente = @idPaciente)
              AND (@status IS NULL OR c.status = @status)
            ORDER BY c.data_hora",
            new { de = de.Date, ateFim = ate.Date.AddDays(1), idMedico, idPaciente, status }).ToList();
    }

    public static Consulta ObterPorId(int id)
    {
        using var c = Db.Abrir();
        return c.QueryFirstOrDefault<Consulta>("SELECT * FROM consulta WHERE id_consulta = @id", new { id });
    }

    public static bool MedicoOcupado(int idMedico, DateTime inicio, short duracao, int ignorarId) =>
        TemConflito("id_medico", idMedico, inicio, duracao, ignorarId);

    public static bool PacienteOcupado(int idPaciente, DateTime inicio, short duracao, int ignorarId) =>
        TemConflito("id_paciente", idPaciente, inicio, duracao, ignorarId);

    // coluna recebe só as constantes acima, nunca texto digitado
    private static bool TemConflito(string coluna, int id, DateTime inicio, short duracao, int ignorarId)
    {
        using var c = Db.Abrir();
        var total = c.ExecuteScalar<int>($@"
            SELECT COUNT(*)
              FROM consulta
             WHERE {coluna} = @id
               AND id_consulta <> @ignorarId
               AND status NOT IN ('CANCELADA','FALTOU')
               AND data_hora < @fim
               AND DATE_ADD(data_hora, INTERVAL duracao_min MINUTE) > @inicio",
            new { id, ignorarId, inicio, fim = inicio.AddMinutes(duracao) });
        return total > 0;
    }

    public static void Salvar(Consulta consulta)
    {
        using var c = Db.Abrir();
        if (consulta.IdConsulta == 0)
            consulta.IdConsulta = c.ExecuteScalar<int>(@"
                INSERT INTO consulta (id_paciente, id_medico, id_funcionario, data_hora, duracao_min, status, observacoes)
                VALUES (@IdPaciente, @IdMedico, @IdFuncionario, @DataHora, @DuracaoMin, @Status, @Observacoes);
                SELECT LAST_INSERT_ID();", consulta);
        else
            c.Execute(@"
                UPDATE consulta
                   SET id_paciente = @IdPaciente, id_medico = @IdMedico, data_hora = @DataHora,
                       duracao_min = @DuracaoMin, status = @Status, observacoes = @Observacoes
                 WHERE id_consulta = @IdConsulta", consulta);
    }

    public static void AlterarStatus(int id, string status)
    {
        using var c = Db.Abrir();
        c.Execute("UPDATE consulta SET status = @status WHERE id_consulta = @id", new { id, status });
    }

    public static void Excluir(int id)
    {
        try
        {
            using var c = Db.Abrir();
            c.Execute("DELETE FROM consulta WHERE id_consulta = @id", new { id });
        }
        catch (MySqlException ex) when (ex.Number == 1451)
        {
            throw new InvalidOperationException("Esta consulta já tem atendimento no prontuário e não pode ser excluída.");
        }
    }
}
