namespace Medicenter.Models;

public class Consulta
{
    public int IdConsulta { get; set; }
    public int IdPaciente { get; set; }
    public int IdMedico { get; set; }
    public int? IdFuncionario { get; set; }
    public DateTime DataHora { get; set; }
    public short DuracaoMin { get; set; } = 30;
    public string Status { get; set; } = StatusConsulta.Agendada;
    public string Observacoes { get; set; }
}

//Linha da agenda (consulta + nomes de paciente, médico e especialidade).
public class AgendaItem
{
    public int IdConsulta { get; set; }
    public DateTime DataHora { get; set; }
    public short DuracaoMin { get; set; }
    public string Status { get; set; }
    public string Observacoes { get; set; }
    public int IdPaciente { get; set; }
    public string Paciente { get; set; }
    public string TelefonePaciente { get; set; }
    public int IdMedico { get; set; }
    public string Medico { get; set; }
    public string Especialidade { get; set; }
}
