using Dapper;
using Medicenter.Data;
using Medicenter.Models;

namespace Medicenter.Repositories;

public static class ProntuarioRepository
{
    //Busca o prontuário do paciente. Cria se ainda não existir.
    public static Prontuario ObterPorPaciente(int idPaciente)
    {
        using var c = Db.Abrir();
        c.Execute("INSERT IGNORE INTO prontuario (id_paciente) VALUES (@idPaciente)", new { idPaciente });
        return c.QueryFirst<Prontuario>("SELECT * FROM prontuario WHERE id_paciente = @idPaciente", new { idPaciente });
    }

    public static void SalvarDadosClinicos(Prontuario p)
    {
        using var c = Db.Abrir();
        c.Execute(@"
            UPDATE prontuario
               SET tipo_sanguineo = @TipoSanguineo, alergias = @Alergias,
                   doencas_cronicas = @DoencasCronicas, medicamentos_uso = @MedicamentosUso
             WHERE id_prontuario = @IdProntuario", p);
    }

    public static List<ProntuarioRegistro> ListarRegistros(int idProntuario)
    {
        using var c = Db.Abrir();
        return c.Query<ProntuarioRegistro>(@"
            SELECT r.*, m.nome AS medico, c.data_hora AS data_consulta
              FROM prontuario_registro r
              JOIN medico m   ON m.id_medico = r.id_medico
              JOIN consulta c ON c.id_consulta = r.id_consulta
             WHERE r.id_prontuario = @idProntuario
             ORDER BY c.data_hora DESC", new { idProntuario }).ToList();
    }

    public static bool ConsultaTemRegistro(int idConsulta)
    {
        using var c = Db.Abrir();
        return c.ExecuteScalar<int>(
            "SELECT COUNT(*) FROM prontuario_registro WHERE id_consulta = @idConsulta", new { idConsulta }) > 0;
    }

    //Grava o atendimento e marca a consulta como REALIZADA na mesma transação.
    public static void RegistrarAtendimento(ProntuarioRegistro r)
    {
        using var c = Db.Abrir();
        using var tx = c.BeginTransaction();

        var status = c.ExecuteScalar<string>(
            "SELECT status FROM consulta WHERE id_consulta = @IdConsulta FOR UPDATE", r, tx);

        if (StatusConsulta.Encerrada(status))
            throw new InvalidOperationException("Não é possível registrar atendimento de consulta cancelada ou com falta.");

        c.Execute(@"
            INSERT INTO prontuario_registro (id_prontuario, id_consulta, id_medico, queixa, exame_fisico, diagnostico, prescricao)
            VALUES (@IdProntuario, @IdConsulta, @IdMedico, @Queixa, @ExameFisico, @Diagnostico, @Prescricao)", r, tx);

        c.Execute("UPDATE consulta SET status = 'REALIZADA' WHERE id_consulta = @IdConsulta", r, tx);

        tx.Commit();
    }
}
