using Medicenter.Services;
using Medicenter.UI;

namespace Medicenter.Forms;

public class FrmLogin : Form
{
    private readonly TextBox _txtLogin;
    private readonly TextBox _txtSenha;

    public FrmLogin()
    {
        Text = "Medicenter — Acesso";
        Font = Ui.Fonte;
        StartPosition = FormStartPosition.CenterScreen;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        ClientSize = new Size(380, 330);

        var titulo = new Label
        {
            Text = "Medicenter",
            Font = new Font("Segoe UI", 20f, FontStyle.Bold),
            ForeColor = Color.FromArgb(30, 60, 120),
            AutoSize = true,
            Location = new Point(30, 20)
        };
        var subtitulo = new Label
        {
            Text = "Gestão da clínica — entre com seu login",
            ForeColor = Color.DimGray,
            AutoSize = true,
            Location = new Point(33, 62)
        };

        var lblLogin = new Label { Text = "Login", AutoSize = true, Location = new Point(32, 100) };
        _txtLogin = new TextBox { Location = new Point(32, 120), Width = 316 };

        var lblSenha = new Label { Text = "Senha", AutoSize = true, Location = new Point(32, 155) };
        _txtSenha = new TextBox { Location = new Point(32, 175), Width = 316, UseSystemPasswordChar = true };

        var btnEntrar = new Button { Text = "Entrar", Location = new Point(32, 215), Size = new Size(150, 34) };
        var btnSair = new Button { Text = "Sair", Location = new Point(198, 215), Size = new Size(150, 34) };
        btnEntrar.Click += (s, e) => Entrar();
        btnSair.Click += (s, e) => Close();

        var lnkCadastro = new LinkLabel
        {
            Text = "Não tenho acesso — quero me cadastrar como paciente",
            AutoSize = true,
            Location = new Point(32, 265)
        };
        lnkCadastro.LinkClicked += (s, e) => AutoCadastro();

        var lblInfo = new Label
        {
            Text = "Médicos e funcionários: peça seu acesso ao administrador.",
            ForeColor = Color.DimGray,
            AutoSize = true,
            Location = new Point(32, 290)
        };

        Controls.AddRange(new Control[]
        {
            titulo, subtitulo, lblLogin, _txtLogin, lblSenha, _txtSenha, btnEntrar, btnSair, lnkCadastro, lblInfo
        });

        AcceptButton = btnEntrar;
    }

    private void Entrar()
    {
        var login = _txtLogin.Text.Trim();
        if (login == "" || _txtSenha.Text == "")
        {
            Ui.Aviso("Informe login e senha.");
            return;
        }

        try
        {
            var erro = AuthService.Entrar(login, _txtSenha.Text);
            if (erro != null)
            {
                Ui.Aviso(erro);
                _txtSenha.Clear();
                _txtSenha.Focus();
                return;
            }
        }
        catch (Exception ex)
        {
            Ui.Erro(ex);
            return;
        }

        Hide();
        bool trocarUsuario;
        using (var principal = new FrmPrincipal())
        {
            principal.ShowDialog();
            trocarUsuario = principal.TrocarUsuario;
        }
        Sessao.Encerrar();

        if (!trocarUsuario)
        {
            Close();
            return;
        }

        _txtSenha.Clear();
        Show();
        _txtLogin.Focus();
    }

    private void AutoCadastro()
    {
        using var form = new FrmPacienteEdit(null, ModoPaciente.AutoCadastro);
        if (form.ShowDialog(this) == DialogResult.OK)
        {
            _txtLogin.Text = form.LoginInformado;
            _txtSenha.Clear();
            _txtSenha.Focus();
        }
    }
}
