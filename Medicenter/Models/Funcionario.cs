using Medicenter.Services;

namespace Medicenter.Models;

public class Funcionario
{
    public int IdFuncionario { get; set; }
    public int IdUsuario { get; set; }
    public string Nome { get; set; }
    public string Cpf { get; set; }
    public string Cargo { get; set; }
    public string Telefone { get; set; }
    public string Email { get; set; }
    public DateTime DataAdmissao { get; set; }
    public bool Ativo { get; set; } = true;

    // Vêm do JOIN com usuario
    public string Login { get; set; }
    public string Perfil { get; set; } = Perfis.Funcionario;

    public string CpfFormatado => Texto.FormatarCpf(Cpf);
    public string AtivoTexto => Ativo ? "Sim" : "Não";
}
