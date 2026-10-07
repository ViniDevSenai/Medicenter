namespace Medicenter.UI;

//Base dos formulários de cadastro: campos em duas colunas e botões Salvar/Cancelar.
public class FrmEdicao : Form
{
    protected readonly TableLayoutPanel Campos;
    protected readonly FlowLayoutPanel PainelBotoes;
    protected readonly Button BtnSalvar;
    protected readonly Button BtnCancelar;

    public FrmEdicao() : this("Cadastro") { }

    public FrmEdicao(string titulo, int largura = 540)
    {
        Text = titulo;
        Font = Ui.Fonte;
        StartPosition = FormStartPosition.CenterParent;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        ShowInTaskbar = false;
        ClientSize = new Size(largura, 400);

        Campos = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            RowCount = 0,
            Padding = new Padding(12, 12, 16, 4),
            AutoScroll = true
        };
        Campos.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 140));
        Campos.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));

        PainelBotoes = new FlowLayoutPanel
        {
            Dock = DockStyle.Bottom,
            FlowDirection = FlowDirection.RightToLeft,
            Height = 50,
            Padding = new Padding(8)
        };

        BtnCancelar = new Button { Text = "Cancelar", Size = new Size(100, 30), DialogResult = DialogResult.Cancel };
        BtnSalvar = new Button { Text = "Salvar", Size = new Size(100, 30) };
        BtnSalvar.Click += (s, e) =>
        {
            try
            {
                if (Salvar())
                {
                    DialogResult = DialogResult.OK;
                    Close();
                }
            }
            catch (Exception ex)
            {
                Ui.Erro(ex);
            }
        };

        PainelBotoes.Controls.Add(BtnCancelar);
        PainelBotoes.Controls.Add(BtnSalvar);

        // A ordem importa para o Dock: o Fill é adicionado primeiro
        Controls.Add(Campos);
        Controls.Add(PainelBotoes);

        CancelButton = BtnCancelar;
    }

    //Adiciona uma linha "rótulo | controle".
    protected T Campo<T>(string rotulo, T controle) where T : Control
    {
        var label = new Label
        {
            Text = rotulo,
            AutoSize = true,
            Anchor = AnchorStyles.Left | AnchorStyles.Top,
            Margin = new Padding(3, 8, 3, 3)
        };

        controle.Anchor = AnchorStyles.Left | AnchorStyles.Right;
        controle.Margin = new Padding(3, 4, 3, 4);

        var linha = Campos.RowCount++;
        Campos.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        Campos.Controls.Add(label, 0, linha);
        Campos.Controls.Add(controle, 1, linha);
        return controle;
    }

    //Texto explicativo ocupando as duas colunas.
    protected Label Nota(string texto, bool destaque = false)
    {
        var label = new Label
        {
            Text = texto,
            AutoSize = true,
            MaximumSize = new Size(ClientSize.Width - 50, 0),
            ForeColor = destaque ? Color.FromArgb(30, 60, 120) : Color.DimGray,
            Font = destaque ? new Font(Ui.Fonte, FontStyle.Bold) : Ui.Fonte,
            Margin = new Padding(3, 6, 3, 8)
        };

        var linha = Campos.RowCount++;
        Campos.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        Campos.Controls.Add(label, 0, linha);
        Campos.SetColumnSpan(label, 2);
        return label;
    }

    //Valida e grava. Retorne false para manter o formulário aberto.
    protected virtual bool Salvar() => true;

    //reenche os campos (chamado no Load, quando os combos já aceitam SelectedValue).
    protected virtual void CarregarDados() { }

    protected override void OnLoad(EventArgs e)
    {
        base.OnLoad(e);
        try
        {
            CarregarDados();
        }
        catch (Exception ex)
        {
            Ui.Erro(ex);
        }

        var altura = Campos.GetPreferredSize(new Size(ClientSize.Width, 0)).Height;
        ClientSize = new Size(ClientSize.Width, Math.Min(altura + PainelBotoes.Height + 8, 720));
    }
}
