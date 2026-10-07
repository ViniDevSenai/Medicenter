using Medicenter.Models;
using Medicenter.Repositories;
using Medicenter.Services;
using Medicenter.UI;

namespace Medicenter.Forms;

public class FrmProntuario : Form
{
    private readonly int _idPaciente;
    private readonly bool _podeEditar;
    private Prontuario _prontuario;

    private readonly ComboBox _cboTipoSanguineo;
    private readonly TextBox _txtAlergias, _txtDoencas, _txtMedicamentos, _txtDetalhe;
    private readonly DataGridView _grid;

    // name="podeEditar">Só o médico altera os dados clínicos.
    public FrmProntuario(int idPaciente, string nomePaciente, bool podeEditar)
    {
        _idPaciente = idPaciente;
        _podeEditar = podeEditar;

        Text = $"Prontuário — {nomePaciente}";
        Font = Ui.Fonte;
        StartPosition = FormStartPosition.CenterParent;
        Size = new Size(920, 720);
        MinimumSize = new Size(720, 560);
        ShowInTaskbar = false;

        // Dados clínicos
        var grupo = new GroupBox { Text = "Dados clínicos", Dock = DockStyle.Top, Height = 230, Padding = new Padding(10) };
        var tabela = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 4 };
        tabela.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 130));
        tabela.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
        tabela.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 130));
        tabela.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));

        _cboTipoSanguineo = Ui.Combo("", "A+", "A-", "B+", "B-", "AB+", "AB-", "O+", "O-");
        _cboTipoSanguineo.Width = 100;
        _txtAlergias = Ui.Multilinha(64);
        _txtDoencas = Ui.Multilinha(64);
        _txtMedicamentos = Ui.Multilinha(64);

        tabela.Controls.Add(Rotulo("Tipo sanguíneo"), 0, 0);
        tabela.Controls.Add(_cboTipoSanguineo, 1, 0);
        tabela.Controls.Add(Rotulo("Alergias"), 0, 1);
        tabela.Controls.Add(Esticar(_txtAlergias), 1, 1);
        tabela.Controls.Add(Rotulo("Doenças crônicas"), 2, 1);
        tabela.Controls.Add(Esticar(_txtDoencas), 3, 1);
        tabela.Controls.Add(Rotulo("Medicamentos em uso"), 0, 2);
        tabela.Controls.Add(Esticar(_txtMedicamentos), 1, 2);

        var btnSalvar = new Button { Text = "Salvar dados clínicos", AutoSize = true, MinimumSize = new Size(160, 30), Visible = podeEditar };
        btnSalvar.Click += (s, e) => SalvarDadosClinicos();
        tabela.Controls.Add(btnSalvar, 3, 2);
        btnSalvar.Anchor = AnchorStyles.Right | AnchorStyles.Bottom;

        grupo.Controls.Add(tabela);

        if (!podeEditar)
        {
            _cboTipoSanguineo.Enabled = false;
            _txtAlergias.ReadOnly = _txtDoencas.ReadOnly = _txtMedicamentos.ReadOnly = true;
        }

        // Histórico de atendimentos
        var titulo = new Label
        {
            Text = "Atendimentos",
            Dock = DockStyle.Top,
            Height = 30,
            Font = new Font(Ui.Fonte, FontStyle.Bold),
            Padding = new Padding(8, 8, 0, 0)
        };

        _grid = new DataGridView { Dock = DockStyle.Fill };
        Ui.ConfigurarGrid(_grid);
        Ui.Coluna(_grid, nameof(ProntuarioRegistro.DataConsulta), "Consulta", "dd/MM/yyyy HH:mm", 80);
        Ui.Coluna(_grid, nameof(ProntuarioRegistro.Medico), "Médico", peso: 100);
        Ui.Coluna(_grid, nameof(ProntuarioRegistro.Queixa), "Queixa", peso: 160);
        Ui.Coluna(_grid, nameof(ProntuarioRegistro.Diagnostico), "Diagnóstico", peso: 160);
        _grid.SelectionChanged += (s, e) => MostrarDetalhe();

        _txtDetalhe = new TextBox
        {
            Dock = DockStyle.Bottom,
            Height = 180,
            Multiline = true,
            ReadOnly = true,
            ScrollBars = ScrollBars.Vertical,
            BackColor = SystemColors.Window
        };

        Controls.Add(_grid);
        Controls.Add(titulo);
        Controls.Add(_txtDetalhe);
        Controls.Add(grupo);
    }

    private static Label Rotulo(string texto) =>
        new() { Text = texto, AutoSize = true, Margin = new Padding(3, 8, 3, 3) };

    private static Control Esticar(Control controle)
    {
        controle.Anchor = AnchorStyles.Left | AnchorStyles.Right;
        return controle;
    }

    protected override void OnLoad(EventArgs e)
    {
        base.OnLoad(e);
        try
        {
            _prontuario = ProntuarioRepository.ObterPorPaciente(_idPaciente);
            _cboTipoSanguineo.SelectedItem = _prontuario.TipoSanguineo ?? "";
            _txtAlergias.Text = _prontuario.Alergias;
            _txtDoencas.Text = _prontuario.DoencasCronicas;
            _txtMedicamentos.Text = _prontuario.MedicamentosUso;

            _grid.DataSource = ProntuarioRepository.ListarRegistros(_prontuario.IdProntuario);
            MostrarDetalhe();
        }
        catch (Exception ex)
        {
            Ui.Erro(ex);
        }
    }

    private void SalvarDadosClinicos()
    {
        if (!_podeEditar || _prontuario == null) return;
        try
        {
            _prontuario.TipoSanguineo = Texto.Nulo(_cboTipoSanguineo.SelectedItem as string);
            _prontuario.Alergias = Texto.Nulo(_txtAlergias.Text);
            _prontuario.DoencasCronicas = Texto.Nulo(_txtDoencas.Text);
            _prontuario.MedicamentosUso = Texto.Nulo(_txtMedicamentos.Text);
            ProntuarioRepository.SalvarDadosClinicos(_prontuario);
            Ui.Info("Dados clínicos salvos.");
        }
        catch (Exception ex)
        {
            Ui.Erro(ex);
        }
    }

    private void MostrarDetalhe()
    {
        if (_grid.CurrentRow?.DataBoundItem is not ProntuarioRegistro r)
        {
            _txtDetalhe.Text = _grid.Rows.Count == 0 ? "Nenhum atendimento registrado." : "";
            return;
        }

        var nl = Environment.NewLine;
        _txtDetalhe.Text =
            $"Consulta: {r.DataConsulta:dd/MM/yyyy HH:mm}    Médico: {r.Medico}{nl}{nl}" +
            $"QUEIXA{nl}{r.Queixa}{nl}{nl}" +
            $"EXAME FÍSICO{nl}{r.ExameFisico ?? "-"}{nl}{nl}" +
            $"DIAGNÓSTICO{nl}{r.Diagnostico ?? "-"}{nl}{nl}" +
            $"PRESCRIÇÃO{nl}{r.Prescricao ?? "-"}";
    }
}
