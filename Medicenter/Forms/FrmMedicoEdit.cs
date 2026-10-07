using Medicenter.Models;
using Medicenter.Repositories;
using Medicenter.Services;
using Medicenter.UI;

namespace Medicenter.Forms;

public class FrmMedicoEdit : FrmEdicao
{
    private readonly Medico _medico;
    private readonly bool _novo;

    private readonly TextBox _txtNome, _txtCrm, _txtTelefone, _txtEmail, _txtLogin, _txtSenha, _txtConfirma;
    private readonly MaskedTextBox _mtbCpf;
    private readonly ComboBox _cboUfCrm, _cboEspecialidade;
    private readonly CheckBox _chkAtivo;

    public FrmMedicoEdit(Medico medico) : base(medico == null ? "Novo médico" : "Editar médico")
    {
        _novo = medico == null;
        _medico = medico ?? new Medico();

        _txtNome = Campo("Nome *", new TextBox { MaxLength = 120 });
        _mtbCpf = Campo("CPF *", new MaskedTextBox("000.000.000-00"));
        _txtCrm = Campo("CRM *", new TextBox { MaxLength = 10 });
        _cboUfCrm = Campo("UF do CRM *", Ui.Combo(Ui.Ufs));
        _cboEspecialidade = Campo("Especialidade *", new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList });
        _txtTelefone = Campo("Telefone", new TextBox { MaxLength = 15 });
        _txtEmail = Campo("E-mail", new TextBox { MaxLength = 100 });
        _chkAtivo = Campo("Ativo", new CheckBox { Checked = true, Text = "Pode acessar o sistema e receber consultas" });
        _txtLogin = Campo("Login *", new TextBox { MaxLength = 100 });
        _txtSenha = Campo(_novo ? "Senha *" : "Nova senha", new TextBox { UseSystemPasswordChar = true });
        _txtConfirma = Campo("Confirmar senha", new TextBox { UseSystemPasswordChar = true });

        if (!_novo) Nota("Deixe a senha em branco para manter a atual.");
    }

    protected override void CarregarDados()
    {
        _cboEspecialidade.DisplayMember = nameof(Especialidade.Nome);
        _cboEspecialidade.ValueMember = nameof(Especialidade.IdEspecialidade);
        _cboEspecialidade.DataSource = EspecialidadeRepository.Listar();

        if (_novo)
        {
            _cboUfCrm.SelectedItem = "MG";
            return;
        }

        _txtNome.Text = _medico.Nome;
        _mtbCpf.Text = _medico.Cpf;
        _txtCrm.Text = _medico.Crm;
        _cboUfCrm.SelectedItem = _medico.UfCrm;
        _cboEspecialidade.SelectedValue = _medico.IdEspecialidade;
        _txtTelefone.Text = _medico.Telefone;
        _txtEmail.Text = _medico.Email;
        _chkAtivo.Checked = _medico.Ativo;
        _txtLogin.Text = _medico.Login;
    }

    protected override bool Salvar()
    {
        var cpf = Texto.Digitos(_mtbCpf.Text);
        var login = Texto.Nulo(_txtLogin.Text);
        var senha = _txtSenha.Text == "" ? null : _txtSenha.Text;
        var uf = _cboUfCrm.SelectedItem as string;

        if (_txtNome.Text.Trim().Length < 3) { Ui.Aviso("Informe o nome."); return false; }
        if (cpf.Length != 11) { Ui.Aviso("O CPF deve ter 11 dígitos."); return false; }
        if (string.IsNullOrWhiteSpace(_txtCrm.Text) || string.IsNullOrEmpty(uf)) { Ui.Aviso("Informe CRM e UF do CRM."); return false; }
        if (_cboEspecialidade.SelectedValue is not int idEspecialidade)
        {
            Ui.Aviso("Selecione a especialidade. Se a lista estiver vazia, cadastre em Cadastros > Especialidades.");
            return false;
        }
        if (login == null) { Ui.Aviso("Informe o login."); return false; }
        if (_novo && senha == null) { Ui.Aviso("Informe a senha."); return false; }
        if (senha != null && senha.Length < 6) { Ui.Aviso("A senha deve ter pelo menos 6 caracteres."); return false; }
        if (_txtSenha.Text != _txtConfirma.Text) { Ui.Aviso("A confirmação não confere com a senha."); return false; }

        _medico.Nome = _txtNome.Text.Trim();
        _medico.Cpf = cpf;
        _medico.Crm = _txtCrm.Text.Trim();
        _medico.UfCrm = uf;
        _medico.IdEspecialidade = idEspecialidade;
        _medico.Telefone = Texto.Nulo(_txtTelefone.Text);
        _medico.Email = Texto.Nulo(_txtEmail.Text);
        _medico.Ativo = _chkAtivo.Checked;

        MedicoRepository.Salvar(_medico, login, senha);
        return true;
    }
}
