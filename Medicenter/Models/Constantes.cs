namespace Medicenter.Models;

public static class Perfis
{
    public const string Admin = "ADMIN";
    public const string Medico = "MEDICO";
    public const string Funcionario = "FUNCIONARIO";
    public const string Paciente = "PACIENTE";
}

public static class StatusConsulta
{
    public const string Agendada = "AGENDADA";
    public const string Confirmada = "CONFIRMADA";
    public const string Realizada = "REALIZADA";
    public const string Cancelada = "CANCELADA";
    public const string Faltou = "FALTOU";

    public static readonly string[] Todos = { Agendada, Confirmada, Realizada, Cancelada, Faltou };

    /// <summary>Status que liberam o horário do médico.</summary>
    public static bool Encerrada(string status) => status == Cancelada || status == Faltou;
}
