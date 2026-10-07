using Medicenter.Models;
using Medicenter.Repositories;
using Medicenter.Services;
using Medicenter.UI;

namespace Medicenter.Forms;

public class FrmFuncionarioEdit : FrmEdicao
{
    private readonly Funcionario _funcionario;
    private readonly bool _novo;

    private readonly TextBox _txtNome, _txtCargo, _txtTelefone, _txtEmail, _txtLogin, _txtSenha, _txtConfirma;
    private readonly MaskedTextBox _mtbCpf;
    private readonly DateTimePicker _dtpAdmissao;
    private readonly ComboBox _cboPerfil;
    private readonly CheckBox _chkAtivo;

    public FrmFuncionarioEdit(Funcionario funcionario) : base(funcionario == null ? "Novo funcionário" : "Editar funcionário")
    {
        _novo = funcionario == null;
        _funcionario = funcionario ?? new Funcionario();

        _txtNome = Campo("Nome *", new TextBox { MaxLength = 120 });
        _mtbCpf = Campo("CPF *", new MaskedTextBox("000.000.000-00"));
        _txtCargo = Campo("Cargo *", new TextBox { MaxLength = 60 });
        _txtTelefone = Campo("Telefone", new TextBox { MaxLength = 15 });
        _txtEmail = Campo("E-mail", new TextBox { MaxLength = 100 });
        _dtpAdmissao = Campo("Admissão *", new DateTimePicker { Format = DateTimePickerFormat.Short, Value = DateTime.Today });
        _cboPerfil = Campo("Perfil *", Ui.Combo(Perfis.Funcionario, Perfis.Admin));
        _chkAtivo = Campo("Ativo", new CheckBox { Checked = true, Text = "Pode acessar o sistema" });
        _txtLogin = Campo("Login *", new TextBox { MaxLength = 100 });
        _txtSenha = Campo(_novo ? "Senha *" : "Nova senha", new TextBox { UseSystemPasswordChar = true });
        _txtConfirma = Campo("Confirmar senha", new TextBox { UseSystemPasswordChar = true });

        Nota("FUNCIONARIO: pacientes e agenda. ADMIN: acesso total, incluindo médicos e funcionários."
             + (_novo ? "" : " Deixe a senha em branco para manter a atual."));

        // Evita que o admin tire o próprio acesso
        if (!_novo && _funcionario.IdFuncionario == Sessao.IdFuncionario)
        {
            _cboPerfil.Enabled = false;
            _chkAtivo.Enabled = false;
        }
    }

    protected override void CarregarDados()
    {
        if (_novo)
        {
            _cboPerfil.SelectedItem = Perfis.Funcionario;
            return;
        }

        _txtNome.Text = _funcionario.Nome;
        _mtbCpf.Text = _funcionario.Cpf;
        _txtCargo.Text = _funcionario.Cargo;
        _txtTelefone.Text = _funcionario.Telefone;
        _txtEmail.Text = _funcionario.Email;
        _dtpAdmissao.Value = _funcionario.DataAdmissao;
        _cboPerfil.SelectedItem = _funcionario.Perfil;
        _chkAtivo.Checked = _funcionario.Ativo;
        _txtLogin.Text = _funcionario.Login;
    }

    protected override bool Salvar()
    {
        var cpf = Texto.Digitos(_mtbCpf.Text);
        var login = Texto.Nulo(_txtLogin.Text);
        var senha = _txtSenha.Text == "" ? null : _txtSenha.Text;

        if (_txtNome.Text.Trim().Length < 3) { Ui.Aviso("Informe o nome."); return false; }
        if (cpf.Length != 11) { Ui.Aviso("O CPF deve ter 11 dígitos."); return false; }
        if (string.IsNullOrWhiteSpace(_txtCargo.Text)) { Ui.Aviso("Informe o cargo."); return false; }
        if (_cboPerfil.SelectedItem is not string perfil) { Ui.Aviso("Selecione o perfil."); return false; }
        if (login == null) { Ui.Aviso("Informe o login."); return false; }
        if (_novo && senha == null) { Ui.Aviso("Informe a senha."); return false; }
        if (senha != null && senha.Length < 6) { Ui.Aviso("A senha deve ter pelo menos 6 caracteres."); return false; }
        if (_txtSenha.Text != _txtConfirma.Text) { Ui.Aviso("A confirmação não confere com a senha."); return false; }

        _funcionario.Nome = _txtNome.Text.Trim();
        _funcionario.Cpf = cpf;
        _funcionario.Cargo = _txtCargo.Text.Trim();
        _funcionario.Telefone = Texto.Nulo(_txtTelefone.Text);
        _funcionario.Email = Texto.Nulo(_txtEmail.Text);
        _funcionario.DataAdmissao = _dtpAdmissao.Value.Date;
        _funcionario.Perfil = perfil;
        _funcionario.Ativo = _chkAtivo.Checked;

        FuncionarioRepository.Salvar(_funcionario, login, senha);
        return true;
    }
}
