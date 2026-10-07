using Medicenter.Models;
using Medicenter.Repositories;
using Medicenter.Services;
using Medicenter.UI;

namespace Medicenter.Forms;

public class FrmPacientes : FrmLista
{
    private readonly bool _modoProntuario;

    /// name="modoProntuario">true = tela do médico, só para abrir prontuários.
    public FrmPacientes(bool modoProntuario) : base(modoProntuario ? "Prontuários — pacientes" : "Pacientes")
    {
        _modoProntuario = modoProntuario;
        TxtBusca.PlaceholderText = "Nome ou CPF";

        Coluna(nameof(Paciente.Nome), "Nome", peso: 200);
        Coluna(nameof(Paciente.CpfFormatado), "CPF");
        Coluna(nameof(Paciente.DataNascimento), "Nascimento", "dd/MM/yyyy");
        Coluna(nameof(Paciente.Telefone), "Telefone");
        Coluna(nameof(Paciente.Convenio), "Convênio");
        if (!modoProntuario) Coluna(nameof(Paciente.Login), "Login");

        if (!modoProntuario)
        {
            Botao("Novo", Novo);
            Botao("Editar", Editar);
            Botao("Excluir", Excluir);
        }

        // Funcionário não vê dados clínicos
        if (Sessao.Eh(Perfis.Admin, Perfis.Medico))
            Botao("Prontuário", AbrirProntuario);
    }

    protected override void Carregar() =>
        Grid.DataSource = PacienteRepository.Listar(TxtBusca.Text);

    protected override void AoDuploClique()
    {
        if (_modoProntuario) AbrirProntuario();
        else Editar();
    }

    private void Novo()
    {
        using var form = new FrmPacienteEdit(null, ModoPaciente.Recepcao);
        if (form.ShowDialog(this) == DialogResult.OK) Recarregar();
    }

    private void Editar()
    {
        var item = Selecionado<Paciente>();
        if (item == null) return;

        using var form = new FrmPacienteEdit(PacienteRepository.ObterPorId(item.IdPaciente), ModoPaciente.Recepcao);
        if (form.ShowDialog(this) == DialogResult.OK) Recarregar();
    }

    private void Excluir()
    {
        var item = Selecionado<Paciente>();
        if (item == null) return;
        if (!Ui.Confirmar($"Excluir o paciente {item.Nome}?\nO prontuário e o login dele também serão apagados.")) return;

        PacienteRepository.Excluir(item.IdPaciente);
        Recarregar();
    }

    private void AbrirProntuario()
    {
        var item = Selecionado<Paciente>();
        if (item == null) return;

        using var form = new FrmProntuario(item.IdPaciente, item.Nome, Sessao.Eh(Perfis.Medico));
        form.ShowDialog(this);
    }
}
