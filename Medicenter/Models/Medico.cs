using Medicenter.Services;

namespace Medicenter.Models;

public class Medico
{
    public int IdMedico { get; set; }
    public int IdUsuario { get; set; }
    public int IdEspecialidade { get; set; }
    public string Nome { get; set; }
    public string Cpf { get; set; }
    public string Crm { get; set; }
    public string UfCrm { get; set; }
    public string Telefone { get; set; }
    public string Email { get; set; }
    public bool Ativo { get; set; } = true;

    // Vêm dos JOINs
    public string Especialidade { get; set; }
    public string Login { get; set; }

    public string CpfFormatado => Texto.FormatarCpf(Cpf);
    public string CrmCompleto => $"{Crm}/{UfCrm}";
    public string AtivoTexto => Ativo ? "Sim" : "Não";
    public string Descricao => string.IsNullOrEmpty(Especialidade) ? Nome : $"{Nome} — {Especialidade}";
}
