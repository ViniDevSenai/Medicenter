using MySqlConnector;

namespace Medicenter.UI;

public static class Ui
{
    public const string Titulo = "Medicenter";

    public static readonly Font Fonte = new("Segoe UI", 9.75f);

    public static readonly string[] Ufs =
    {
        "", "AC", "AL", "AP", "AM", "BA", "CE", "DF", "ES", "GO", "MA", "MT", "MS", "MG", "PA",
        "PB", "PR", "PE", "PI", "RJ", "RN", "RS", "RO", "RR", "SC", "SP", "SE", "TO"
    };

    public static void Info(string mensagem) =>
        MessageBox.Show(mensagem, Titulo, MessageBoxButtons.OK, MessageBoxIcon.Information);

    public static void Aviso(string mensagem) =>
        MessageBox.Show(mensagem, Titulo, MessageBoxButtons.OK, MessageBoxIcon.Warning);

    public static void Erro(Exception ex) =>
        MessageBox.Show(Traduzir(ex), Titulo, MessageBoxButtons.OK, MessageBoxIcon.Error);

    public static bool Confirmar(string mensagem) =>
        MessageBox.Show(mensagem, Titulo, MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes;

    public static string Traduzir(Exception ex)
    {
        if (ex is MySqlException m)
        {
            return m.Number switch
            {
                1062 => "Já existe um registro com esse valor (CPF, login, CRM ou horário já usado).",
                1451 => "Não é possível excluir: existem registros ligados a este item.",
                1042 or 2002 or 2003 => "Não foi possível conectar ao MySQL. Verifique se o servidor está ligado e o appsettings.json.",
                1045 => "Usuário ou senha do MySQL inválidos. Verifique o appsettings.json.",
                1049 => "Banco 'medicenter' não encontrado. Rode o script database/medicenter.sql.",
                _ => "Erro no banco de dados: " + m.Message
            };
        }
        return ex.Message;
    }

    public static ComboBox Combo(params string[] itens)
    {
        var combo = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList };
        combo.Items.AddRange(itens);
        return combo;
    }

    public static TextBox Multilinha(int altura) =>
        new() { Multiline = true, ScrollBars = ScrollBars.Vertical, Height = altura, AcceptsReturn = true };

    public static void ConfigurarGrid(DataGridView grid)
    {
        grid.ReadOnly = true;
        grid.AllowUserToAddRows = false;
        grid.AllowUserToDeleteRows = false;
        grid.AllowUserToResizeRows = false;
        grid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        grid.MultiSelect = false;
        grid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        grid.RowHeadersVisible = false;
        grid.BackgroundColor = SystemColors.Window;
        grid.BorderStyle = BorderStyle.None;
        grid.AutoGenerateColumns = false;
        grid.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(245, 247, 250);
    }

    public static void Coluna(DataGridView grid, string propriedade, string titulo, string formato = null, int peso = 100)
    {
        var coluna = new DataGridViewTextBoxColumn
        {
            DataPropertyName = propriedade,
            HeaderText = titulo,
            FillWeight = peso
        };
        if (formato != null) coluna.DefaultCellStyle.Format = formato;
        grid.Columns.Add(coluna);
    }
}
