using Medicenter.Models;
using Medicenter.Repositories;
using Medicenter.Services;
using Medicenter.UI;

namespace Medicenter.Forms;

public enum ModoPaciente
{
    //Recepção ou admin cadastrando/editando. Login opcional.
    Recepcao,
    //Paciente se cadastrando pela tela de login. Login obrigatório.
    AutoCadastro,
    //Paciente logado editando os próprios dados. Sem campos de login.
    MeusDados
}

public class FrmPacienteEdit : FrmEdicao
{
    private readonly Paciente _paciente;
    private readonly ModoPaciente _modo;

    private readonly TextBox _txtNome, _txtTelefone, _txtEmail, _txtLogradouro, _txtNumero,
                             _txtBairro, _txtCidade, _txtConvenio;
    private readonly MaskedTextBox _mtbCpf, _mtbCep;
    private readonly DateTimePicker _dtpNascimento;
    private readonly ComboBox _cboSexo, _cboUf;
    private readonly TextBox _txtLogin, _txtSenha, _txtConfirma;

    public string LoginInformado => _txtLogin?.Text.Trim();

    public FrmPacienteEdit(Paciente paciente, ModoPaciente modo) : base(TituloPara(paciente, modo), 560)
    {
        _paciente = paciente ?? new Paciente();
        _modo = modo;

        _txtNome = Campo("Nome *", new TextBox { MaxLength = 120 });
        _mtbCpf = Campo("CPF *", new MaskedTextBox("000.000.000-00"));
        _dtpNascimento = Campo("Nascimento *", new DateTimePicker
        {
            Format = DateTimePickerFormat.Short,
            MaxDate = DateTime.Today,
            Value = DateTime.Today.AddYears(-30)
        });
        _cboSexo = Campo("Sexo", Ui.Combo("", "M", "F", "O"));
        _txtTelefone = Campo("Telefone", new TextBox { MaxLength = 15 });
        _txtEmail = Campo("E-mail", new TextBox { MaxLength = 100 });
        _mtbCep = Campo("CEP", new MaskedTextBox("00000-000"));
        _txtLogradouro = Campo("Logradouro", new TextBox { MaxLength = 120 });
        _txtNumero = Campo("Número", new TextBox { MaxLength = 10 });
        _txtBairro = Campo("Bairro", new TextBox { MaxLength = 60 });
        _txtCidade = Campo("Cidade", new TextBox { MaxLength = 60 });
        _cboUf = Campo("UF", Ui.Combo(Ui.Ufs));
        _txtConvenio = Campo("Convênio", new TextBox { MaxLength = 60 });

        if (modo != ModoPaciente.MeusDados)
        {
            var obrigatorio = modo == ModoPaciente.AutoCadastro ? " *" : "";
            _txtLogin = Campo("Login" + obrigatorio, new TextBox { MaxLength = 100 });
            _txtSenha = Campo("Senha" + obrigatorio, new TextBox { UseSystemPasswordChar = true });
            _txtConfirma = Campo("Confirmar senha" + obrigatorio, new TextBox { UseSystemPasswordChar = true });
        }

        if (modo == ModoPaciente.Recepcao)
            Nota("Login e senha são opcionais: preencha para dar acesso ao paciente. Na edição, deixe a senha em branco para manter a atual.");
        if (modo == ModoPaciente.AutoCadastro)
        {
            Nota("Se você já foi atendido na clínica, informe o mesmo CPF: o acesso será ligado ao seu cadastro.");
            BtnSalvar.Text = "Cadastrar";
        }
        if (modo == ModoPaciente.MeusDados)
            _mtbCpf.ReadOnly = true;
    }

    private static string TituloPara(Paciente p, ModoPaciente modo) => modo switch
    {
        ModoPaciente.AutoCadastro => "Cadastro de paciente",
        ModoPaciente.MeusDados => "Meus dados",
        _ => p == null ? "Novo paciente" : "Editar paciente"
    };

    protected override void CarregarDados()
    {
        if (_paciente.IdPaciente == 0) return;

        _txtNome.Text = _paciente.Nome;
        _mtbCpf.Text = _paciente.Cpf;
        _dtpNascimento.Value = _paciente.DataNascimento;
        _cboSexo.SelectedItem = _paciente.Sexo ?? "";
        _txtTelefone.Text = _paciente.Telefone;
        _txtEmail.Text = _paciente.Email;
        _mtbCep.Text = _paciente.Cep;
        _txtLogradouro.Text = _paciente.Logradouro;
        _txtNumero.Text = _paciente.Numero;
        _txtBairro.Text = _paciente.Bairro;
        _txtCidade.Text = _paciente.Cidade;
        _cboUf.SelectedItem = _paciente.Uf ?? "";
        _txtConvenio.Text = _paciente.Convenio;
        if (_txtLogin != null) _txtLogin.Text = _paciente.Login;
    }

    protected override bool Salvar()
    {
        var nome = _txtNome.Text.Trim();
        var cpf = Texto.Digitos(_mtbCpf.Text);

        if (nome.Length < 3)
        {
            Ui.Aviso("Informe o nome completo.");
            _txtNome.Focus();
            return false;
        }
        if (cpf.Length != 11)
        {
            Ui.Aviso("O CPF deve ter 11 dígitos.");
            _mtbCpf.Focus();
            return false;
        }

        string login = null, senha = null;
        if (_txtLogin != null)
        {
            login = Texto.Nulo(_txtLogin.Text);
            senha = _txtSenha.Text == "" ? null : _txtSenha.Text;

            if (_modo == ModoPaciente.AutoCadastro && (login == null || senha == null))
            {
                Ui.Aviso("Informe login e senha.");
                return false;
            }
            if (senha != null && login == null)
            {
                Ui.Aviso("Informe o login.");
                return false;
            }
            if (senha != null && senha.Length < 6)
            {
                Ui.Aviso("A senha deve ter pelo menos 6 caracteres.");
                return false;
            }
            if (_txtSenha.Text != _txtConfirma.Text)
            {
                Ui.Aviso("A confirmação não confere com a senha.");
                return false;
            }
        }

        _paciente.Nome = nome;
        _paciente.Cpf = cpf;
        _paciente.DataNascimento = _dtpNascimento.Value.Date;
        _paciente.Sexo = Texto.Nulo(_cboSexo.SelectedItem as string);
        _paciente.Telefone = Texto.Nulo(_txtTelefone.Text);
        _paciente.Email = Texto.Nulo(_txtEmail.Text);
        _paciente.Cep = Texto.Nulo(Texto.Digitos(_mtbCep.Text));
        _paciente.Logradouro = Texto.Nulo(_txtLogradouro.Text);
        _paciente.Numero = Texto.Nulo(_txtNumero.Text);
        _paciente.Bairro = Texto.Nulo(_txtBairro.Text);
        _paciente.Cidade = Texto.Nulo(_txtCidade.Text);
        _paciente.Uf = Texto.Nulo(_cboUf.SelectedItem as string);
        _paciente.Convenio = Texto.Nulo(_txtConvenio.Text);

        if (_modo == ModoPaciente.AutoCadastro)
        {
            var vinculado = PacienteRepository.AutoCadastrar(_paciente, login, senha);
            Ui.Info(vinculado
                ? "Encontramos seu cadastro na clínica. O acesso foi criado e ligado a ele."
                : "Cadastro realizado. Agora entre com seu login e senha.");
        }
        else
        {
            PacienteRepository.Salvar(_paciente, login, senha);
        }
        return true;
    }
}
