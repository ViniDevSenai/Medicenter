using Medicenter.Models;
using Medicenter.Repositories;
using Medicenter.UI;

namespace Medicenter.Forms;

public class FrmMedicos : FrmLista
{
    public FrmMedicos() : base("Médicos")
    {
        TxtBusca.PlaceholderText = "Nome ou CRM";

        Coluna(nameof(Medico.Nome), "Nome", peso: 180);
        Coluna(nameof(Medico.Especialidade), "Especialidade");
        Coluna(nameof(Medico.CrmCompleto), "CRM");
        Coluna(nameof(Medico.Telefone), "Telefone");
        Coluna(nameof(Medico.Login), "Login");
        Coluna(nameof(Medico.AtivoTexto), "Ativo", peso: 50);

        Botao("Novo", Novo);
        Botao("Editar", Editar);
        Botao("Inativar", Inativar);
    }

    protected override void Carregar() =>
        Grid.DataSource = MedicoRepository.Listar(TxtBusca.Text, apenasAtivos: false);

    protected override void AoDuploClique() => Editar();

    private void Novo()
    {
        using var form = new FrmMedicoEdit(null);
        if (form.ShowDialog(this) == DialogResult.OK) Recarregar();
    }

    private void Editar()
    {
        var item = Selecionado<Medico>();
        if (item == null) return;

        using var form = new FrmMedicoEdit(MedicoRepository.ObterPorId(item.IdMedico));
        if (form.ShowDialog(this) == DialogResult.OK) Recarregar();
    }

    private void Inativar()
    {
        var item = Selecionado<Medico>();
        if (item == null) return;
        if (!item.Ativo)
        {
            Ui.Aviso("Este médico já está inativo. Para reativar, use Editar.");
            return;
        }
        if (!Ui.Confirmar($"Inativar {item.Nome}?\nO login será bloqueado e o histórico de consultas será mantido.")) return;

        MedicoRepository.Inativar(item.IdMedico);
        Recarregar();
    }
}
