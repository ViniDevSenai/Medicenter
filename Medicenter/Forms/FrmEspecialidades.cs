using Medicenter.Models;
using Medicenter.Repositories;
using Medicenter.UI;

namespace Medicenter.Forms;

public class FrmEspecialidades : FrmLista
{
    public FrmEspecialidades() : base("Especialidades")
    {
        Size = new Size(600, 480);
        TxtBusca.PlaceholderText = "Nome da especialidade";
        Coluna(nameof(Especialidade.Nome), "Especialidade");

        Botao("Nova", () => Abrir(null));
        Botao("Editar", () =>
        {
            var item = Selecionado<Especialidade>();
            if (item != null) Abrir(item);
        });
        Botao("Excluir", Excluir);
    }

    protected override void Carregar() =>
        Grid.DataSource = EspecialidadeRepository.Listar(TxtBusca.Text);

    protected override void AoDuploClique()
    {
        var item = Selecionado<Especialidade>();
        if (item != null) Abrir(item);
    }

    private void Abrir(Especialidade item)
    {
        using var form = new FrmEspecialidadeEdit(item);
        if (form.ShowDialog(this) == DialogResult.OK) Recarregar();
    }

    private void Excluir()
    {
        var item = Selecionado<Especialidade>();
        if (item == null || !Ui.Confirmar($"Excluir a especialidade {item.Nome}?")) return;

        EspecialidadeRepository.Excluir(item.IdEspecialidade);
        Recarregar();
    }
}

public class FrmEspecialidadeEdit : FrmEdicao
{
    private readonly Especialidade _especialidade;
    private readonly TextBox _txtNome;

    public FrmEspecialidadeEdit(Especialidade especialidade)
        : base(especialidade == null ? "Nova especialidade" : "Editar especialidade", 420)
    {
        _especialidade = especialidade ?? new Especialidade();
        _txtNome = Campo("Nome *", new TextBox { MaxLength = 80, Text = _especialidade.Nome });
    }

    protected override bool Salvar()
    {
        if (string.IsNullOrWhiteSpace(_txtNome.Text))
        {
            Ui.Aviso("Informe o nome.");
            return false;
        }

        _especialidade.Nome = _txtNome.Text.Trim();
        EspecialidadeRepository.Salvar(_especialidade);
        return true;
    }
}
