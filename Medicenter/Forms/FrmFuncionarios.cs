using Medicenter.Models;
using Medicenter.Repositories;
using Medicenter.Services;
using Medicenter.UI;

namespace Medicenter.Forms;

public class FrmFuncionarios : FrmLista
{
    public FrmFuncionarios() : base("Funcionários")
    {
        TxtBusca.PlaceholderText = "Nome ou cargo";

        Coluna(nameof(Funcionario.Nome), "Nome", peso: 180);
        Coluna(nameof(Funcionario.Cargo), "Cargo");
        Coluna(nameof(Funcionario.CpfFormatado), "CPF");
        Coluna(nameof(Funcionario.Telefone), "Telefone");
        Coluna(nameof(Funcionario.Login), "Login");
        Coluna(nameof(Funcionario.Perfil), "Perfil");
        Coluna(nameof(Funcionario.AtivoTexto), "Ativo", peso: 50);

        Botao("Novo", Novo);
        Botao("Editar", Editar);
        Botao("Inativar", Inativar);
    }

    protected override void Carregar() =>
        Grid.DataSource = FuncionarioRepository.Listar(TxtBusca.Text);

    protected override void AoDuploClique() => Editar();

    private void Novo()
    {
        using var form = new FrmFuncionarioEdit(null);
        if (form.ShowDialog(this) == DialogResult.OK) Recarregar();
    }

    private void Editar()
    {
        var item = Selecionado<Funcionario>();
        if (item == null) return;

        using var form = new FrmFuncionarioEdit(FuncionarioRepository.ObterPorId(item.IdFuncionario));
        if (form.ShowDialog(this) == DialogResult.OK) Recarregar();
    }

    private void Inativar()
    {
        var item = Selecionado<Funcionario>();
        if (item == null) return;
        if (item.IdFuncionario == Sessao.IdFuncionario)
        {
            Ui.Aviso("Você não pode inativar o próprio acesso.");
            return;
        }
        if (!item.Ativo)
        {
            Ui.Aviso("Este funcionário já está inativo. Para reativar, use Editar.");
            return;
        }
        if (!Ui.Confirmar($"Inativar {item.Nome}?\nO login será bloqueado.")) return;

        FuncionarioRepository.Inativar(item.IdFuncionario);
        Recarregar();
    }
}
