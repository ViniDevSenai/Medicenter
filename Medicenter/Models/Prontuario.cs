namespace Medicenter.Models;

public class Prontuario
{
    public int IdProntuario { get; set; }
    public int IdPaciente { get; set; }
    public string TipoSanguineo { get; set; }
    public string Alergias { get; set; }
    public string DoencasCronicas { get; set; }
    public string MedicamentosUso { get; set; }
}

public class ProntuarioRegistro
{
    public int IdRegistro { get; set; }
    public int IdProntuario { get; set; }
    public int IdConsulta { get; set; }
    public int IdMedico { get; set; }
    public string Queixa { get; set; }
    public string ExameFisico { get; set; }
    public string Diagnostico { get; set; }
    public string Prescricao { get; set; }
    public DateTime DataRegistro { get; set; }

    // Vêm dos JOINs
    public string Medico { get; set; }
    public DateTime DataConsulta { get; set; }
}
