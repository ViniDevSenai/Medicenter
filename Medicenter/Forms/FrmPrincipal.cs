using Medicenter.Models;
using Medicenter.Repositories;
using Medicenter.Services;
using Medicenter.UI;

namespace Medicenter.Forms;

public class FrmPrincipal : Form
{
    private readonly MenuStrip _menu = new();
    private readonly FlowLayoutPanel _atalhos;

    /// true quando o usuário escolheu "Trocar usuário" (volta para o login).
    public bool TrocarUsuario { get; private set; }

    public FrmPrincipal()
    {
        Text = $"Medicenter — {Sessao.Nome} ({Sessao.Perfil})";
        Font = Ui.Fonte;
        StartPosition = FormStartPosition.CenterScreen;
        Size = new Size(1000, 640);
        MinimumSize = new Size(800, 500);

        var boasVindas = new Label
        {
            Text = $"Olá, {Sessao.Nome}",
            Font = new Font("Segoe UI", 18f, FontStyle.Bold),
            ForeColor = Color.FromArgb(30, 60, 120),
            Dock = DockStyle.Top,
            Height = 60,
            Padding = new Padding(24, 18, 0, 0)
        };
        var dica = new Label
        {
            Text = "Escolha uma opção abaixo ou no menu.",
            ForeColor = Color.DimGray,
            Dock = DockStyle.Top,
            Height = 30,
            Padding = new Padding(26, 4, 0, 0)
        };

        _atalhos = new FlowLayoutPanel { Dock = DockStyle.Fill, Padding = new Padding(20), AutoScroll = true };

        var status = new StatusStrip();
        status.Items.Add($"Usuário: {Sessao.Usuario.Login}   |   Perfil: {Sessao.Perfil}   |   {DateTime.Now:dd/MM/yyyy}");

        MontarMenu();

        Controls.Add(_atalhos);
        Controls.Add(dica);
        Controls.Add(boasVindas);
        Controls.Add(_menu);
        Controls.Add(status);
        MainMenuStrip = _menu;
    }

    private void MontarMenu()
    {
        if (Sessao.Eh(Perfis.Admin, Perfis.Funcionario))
        {
            var cadastros = Grupo("Cadastros");
            Acao(cadastros, "Pacientes", () => new FrmPacientes(false));
            if (Sessao.Eh(Perfis.Admin))
            {
                Acao(cadastros, "Médicos", () => new FrmMedicos());
                Acao(cadastros, "Funcionários", () => new FrmFuncionarios());
                Acao(cadastros, "Especialidades", () => new FrmEspecialidades());
            }

            var agenda = Grupo("Agenda");
            Acao(agenda, "Consultas", () => new FrmConsultas(ModoAgenda.Geral));
        }

        if (Sessao.Eh(Perfis.Medico))
        {
            var atendimento = Grupo("Atendimento");
            Acao(atendimento, "Minha agenda", () => new FrmConsultas(ModoAgenda.Medico));
            Acao(atendimento, "Prontuários", () => new FrmPacientes(true));
        }

        if (Sessao.Eh(Perfis.Paciente))
        {
            var paciente = Grupo("Minha área");
            Acao(paciente, "Minhas consultas", () => new FrmConsultas(ModoAgenda.Paciente));
            Acao(paciente, "Meus dados", () =>
                new FrmPacienteEdit(PacienteRepository.ObterPorId(Sessao.IdPaciente.Value), ModoPaciente.MeusDados));
        }

        var conta = Grupo("Conta");
        Acao(conta, "Alterar senha", () => new FrmAlterarSenha(), atalho: false);
        conta.DropDownItems.Add(new ToolStripSeparator());
        conta.DropDownItems.Add("Trocar usuário", null, (s, e) => { TrocarUsuario = true; Close(); });
        conta.DropDownItems.Add("Sair do sistema", null, (s, e) => Close());
    }

    private ToolStripMenuItem Grupo(string texto)
    {
        var item = new ToolStripMenuItem(texto);
        _menu.Items.Add(item);
        return item;
    }

    // Cria o item de menu e, se pedido, um botão de atalho na tela inicial.
    private void Acao(ToolStripMenuItem grupo, string texto, Func<Form> criar, bool atalho = true)
    {
        void Abrir()
        {
            try
            {
                using var form = criar();
                form.ShowDialog(this);
            }
            catch (Exception ex)
            {
                Ui.Erro(ex);
            }
        }

        grupo.DropDownItems.Add(texto, null, (s, e) => Abrir());

        if (atalho)
        {
            var botao = new Button
            {
                Text = texto,
                Size = new Size(180, 80),
                Margin = new Padding(8),
                Font = new Font("Segoe UI", 11f),
                BackColor = Color.FromArgb(235, 241, 250),
                FlatStyle = FlatStyle.Flat
            };
            botao.FlatAppearance.BorderColor = Color.FromArgb(180, 196, 222);
            botao.Click += (s, e) => Abrir();
            _atalhos.Controls.Add(botao);
        }
    }
}
