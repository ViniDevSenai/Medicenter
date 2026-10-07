namespace Medicenter.UI;

//Base das telas de listagem: filtro no topo, grade no meio e botões embaixo.
public class FrmLista : Form
{
    protected readonly DataGridView Grid;
    protected readonly FlowLayoutPanel PainelFiltro;
    protected readonly FlowLayoutPanel PainelBotoes;
    protected readonly TextBox TxtBusca;
    protected readonly Button BtnBuscar;

    public FrmLista() : this("Lista") { }

    public FrmLista(string titulo)
    {
        Text = titulo;
        Font = Ui.Fonte;
        StartPosition = FormStartPosition.CenterParent;
        Size = new Size(1000, 600);
        MinimumSize = new Size(720, 400);
        ShowInTaskbar = false;

        PainelFiltro = new FlowLayoutPanel
        {
            Dock = DockStyle.Top,
            Height = 46,
            Padding = new Padding(8, 8, 8, 0),
            WrapContents = false
        };

        TxtBusca = new TextBox { Width = 280, PlaceholderText = "Digite para buscar" };
        TxtBusca.KeyDown += (s, e) =>
        {
            if (e.KeyCode == Keys.Enter)
            {
                e.SuppressKeyPress = true;
                Recarregar();
            }
        };

        BtnBuscar = new Button { Text = "Buscar", Size = new Size(90, 28) };
        BtnBuscar.Click += (s, e) => Recarregar();

        PainelFiltro.Controls.Add(Rotulo("Buscar:"));
        PainelFiltro.Controls.Add(TxtBusca);
        PainelFiltro.Controls.Add(BtnBuscar);

        Grid = new DataGridView { Dock = DockStyle.Fill };
        Ui.ConfigurarGrid(Grid);
        Grid.CellDoubleClick += (s, e) =>
        {
            if (e.RowIndex >= 0) Executar(AoDuploClique);
        };

        PainelBotoes = new FlowLayoutPanel
        {
            Dock = DockStyle.Bottom,
            Height = 50,
            Padding = new Padding(8)
        };

        Controls.Add(Grid);
        Controls.Add(PainelFiltro);
        Controls.Add(PainelBotoes);
    }

    protected static Label Rotulo(string texto) =>
        new() { Text = texto, AutoSize = true, Margin = new Padding(6, 7, 3, 3) };

    protected Button Botao(string texto, Action acao)
    {
        var botao = new Button { Text = texto, AutoSize = true, MinimumSize = new Size(100, 30) };
        botao.Click += (s, e) => Executar(acao);
        PainelBotoes.Controls.Add(botao);
        return botao;
    }

    protected void Coluna(string propriedade, string titulo, string formato = null, int peso = 100) =>
        Ui.Coluna(Grid, propriedade, titulo, formato, peso);

    //Item selecionado na grade ou null (com aviso).
    protected T Selecionado<T>() where T : class
    {
        var item = Grid.CurrentRow?.DataBoundItem as T;
        if (item == null) Ui.Aviso("Selecione um registro na lista.");
        return item;
    }

    protected virtual void Carregar() { }
    protected virtual void AoDuploClique() { }

    protected void Recarregar() => Executar(Carregar);

    protected static void Executar(Action acao)
    {
        try
        {
            acao();
        }
        catch (Exception ex)
        {
            Ui.Erro(ex);
        }
    }

    protected override void OnLoad(EventArgs e)
    {
        base.OnLoad(e);
        Botao("Fechar", Close);
        Recarregar();
    }
}
