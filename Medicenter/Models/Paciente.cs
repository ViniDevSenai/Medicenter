using Medicenter.Services;

namespace Medicenter.Models;

public class Paciente
{
    public int IdPaciente { get; set; }
    public int? IdUsuario { get; set; }
    public string Nome { get; set; }
    public string Cpf { get; set; }
    public DateTime DataNascimento { get; set; }
    public string Sexo { get; set; }
    public string Telefone { get; set; }
    public string Email { get; set; }
    public string Cep { get; set; }
    public string Logradouro { get; set; }
    public string Numero { get; set; }
    public string Bairro { get; set; }
    public string Cidade { get; set; }
    public string Uf { get; set; }
    public string Convenio { get; set; }
    public DateTime CriadoEm { get; set; }

    // Vem do JOIN com usuario
    public string Login { get; set; }

    public string CpfFormatado => Texto.FormatarCpf(Cpf);
    public string Descricao => $"{Nome} ({CpfFormatado})";
}
