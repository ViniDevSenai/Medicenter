using Medicenter.Models;
using Medicenter.Repositories;
using Medicenter.Services;
using Medicenter.UI;

namespace Medicenter.Forms;

public enum ModoAgenda
{
    //Recepção/admin: todas as consultas.
    Geral,
    //Médico logado: só a própria agenda.
    Medico,
    //Paciente logado: só as próprias consultas.
    Paciente
}

public class FrmConsultas : FrmLista
{
    private readonly ModoAgenda _modo;
    private readonly DateTimePicker _dtpDe, _dtpAte;
    private readonly ComboBox _cboMedico, _cboStatus;

    public FrmConsultas(ModoAgenda modo) : base(TituloPara(modo))
    {
        _modo = modo;

        // Troca a busca por texto pelos filtros de agenda
        PainelFiltro.Controls.Clear();

        var hoje = DateTime.Today;
        _dtpDe = new DateTimePicker
        {
            Format = DateTimePickerFormat.Short,
            Width = 115,
            Value = modo == ModoAgenda.Paciente ? hoje.AddMonths(-6) : hoje
        };
        _dtpAte = new DateTimePicker
        {
            Format = DateTimePickerFormat.Short,
            Width = 115,
            Value = modo == ModoAgenda.Paciente ? hoje.AddMonths(6) : hoje.AddDays(30)
        };
        PainelFiltro.Controls.Add(Rotulo("De:"));
        PainelFiltro.Controls.Add(_dtpDe);
        PainelFiltro.Controls.Add(Rotulo("Até:"));
        PainelFiltro.Controls.Add(_dtpAte);

        if (modo == ModoAgenda.Geral)
        {
            _cboMedico = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Width = 220 };
            PainelFiltro.Controls.Add(Rotulo("Médico:"));
            PainelFiltro.Controls.Add(_cboMedico);
        }

        _cboStatus = Ui.Combo(new[] { "Todos" }.Concat(StatusConsulta.Todos).ToArray());
        _cboStatus.Width = 120;
        _cboStatus.SelectedIndex = 0;
        PainelFiltro.Controls.Add(Rotulo("Status:"));
        PainelFiltro.Controls.Add(_cboStatus);
        PainelFiltro.Controls.Add(BtnBuscar);

        Coluna(nameof(AgendaItem.DataHora), "Data/hora", "dd/MM/yyyy HH:mm");
        if (modo != ModoAgenda.Paciente)
        {
            Coluna(nameof(AgendaItem.Paciente), "Paciente", peso: 160);
            Coluna(nameof(AgendaItem.TelefonePaciente), "Telefone");
        }
        if (modo != ModoAgenda.Medico)
            Coluna(nameof(AgendaItem.Medico), "Médico", peso: 140);
        Coluna(nameof(AgendaItem.Especialidade), "Especialidade");
        Coluna(nameof(AgendaItem.DuracaoMin), "Min", peso: 40);
        Coluna(nameof(AgendaItem.Status), "Status", peso: 80);
        Coluna(nameof(AgendaItem.Observacoes), "Observações", peso: 140);

        switch (modo)
        {
            case ModoAgenda.Geral:
                Botao("Agendar", Novo);
                Botao("Editar", Editar);
                Botao("Confirmar", () => MudarStatus(StatusConsulta.Confirmada, "Confirmar a consulta"));
                Botao("Cancelar consulta", () => MudarStatus(StatusConsulta.Cancelada, "Cancelar a consulta"));
                Botao("Faltou", () => MudarStatus(StatusConsulta.Faltou, "Marcar falta do paciente"));
                if (Sessao.Eh(Perfis.Admin)) Botao("Excluir", Excluir);
                break;

            case ModoAgenda.Medico:
                Botao("Atender", Atender);
                Botao("Prontuário", AbrirProntuario);
                Botao("Faltou", () => MudarStatus(StatusConsulta.Faltou, "Marcar falta do paciente"));
                break;

            case ModoAgenda.Paciente:
                Botao("Agendar", Novo);
                Botao("Cancelar consulta", () => MudarStatus(StatusConsulta.Cancelada, "Cancelar a consulta"));
                break;
        }
    }

    private static string TituloPara(ModoAgenda modo) => modo switch
    {
        ModoAgenda.Medico => "Minha agenda",
        ModoAgenda.Paciente => "Minhas consultas",
        _ => "Consultas"
    };

    protected override void OnLoad(EventArgs e)
    {
        if (_cboMedico != null)
        {
            try
            {
                var medicos = new List<Medico> { new() { IdMedico = 0, Nome = "(Todos)" } };
                medicos.AddRange(MedicoRepository.Listar("", apenasAtivos: false));
                _cboMedico.DisplayMember = nameof(Medico.Nome);
                _cboMedico.ValueMember = nameof(Medico.IdMedico);
                _cboMedico.DataSource = medicos;
            }
            catch (Exception ex)
            {
                Ui.Erro(ex);
            }
        }
        base.OnLoad(e);
    }

    protected override void Carregar()
    {
        int? idMedico = null, idPaciente = null;

        if (_modo == ModoAgenda.Medico) idMedico = Sessao.IdMedico;
        else if (_modo == ModoAgenda.Paciente) idPaciente = Sessao.IdPaciente;
        else if (_cboMedico.SelectedValue is int id && id > 0) idMedico = id;

        var status = _cboStatus.SelectedIndex > 0 ? (string)_cboStatus.SelectedItem : null;

        Grid.DataSource = ConsultaRepository.Listar(_dtpDe.Value, _dtpAte.Value, idMedico, idPaciente, status);
    }

    protected override void AoDuploClique()
    {
        if (_modo == ModoAgenda.Geral) Editar();
        else if (_modo == ModoAgenda.Medico) Atender();
    }

    private void Novo()
    {
        using var form = new FrmConsultaEdit(null, _modo);
        if (form.ShowDialog(this) == DialogResult.OK) Recarregar();
    }

    private void Editar()
    {
        var item = Selecionado<AgendaItem>();
        if (item == null) return;
        if (item.Status == StatusConsulta.Realizada)
        {
            Ui.Aviso("Consultas realizadas não podem ser alteradas.");
            return;
        }

        using var form = new FrmConsultaEdit(ConsultaRepository.ObterPorId(item.IdConsulta), _modo);
        if (form.ShowDialog(this) == DialogResult.OK) Recarregar();
    }

    private void MudarStatus(string novoStatus, string pergunta)
    {
        var item = Selecionado<AgendaItem>();
        if (item == null) return;

        if (item.Status == StatusConsulta.Realizada)
        {
            Ui.Aviso("Esta consulta já foi realizada.");
            return;
        }
        if (item.Status == novoStatus)
        {
            Ui.Aviso($"A consulta já está como {novoStatus}.");
            return;
        }
        if (StatusConsulta.Encerrada(item.Status) && novoStatus == StatusConsulta.Confirmada)
        {
            Ui.Aviso("Consulta cancelada ou com falta não pode ser confirmada. Faça um novo agendamento.");
            return;
        }
        if (_modo == ModoAgenda.Paciente && item.DataHora <= DateTime.Now)
        {
            Ui.Aviso("Só é possível cancelar consultas futuras.");
            return;
        }
        if (!Ui.Confirmar($"{pergunta} de {item.DataHora:dd/MM/yyyy HH:mm}?")) return;

        ConsultaRepository.AlterarStatus(item.IdConsulta, novoStatus);
        Recarregar();
    }

    private void Excluir()
    {
        var item = Selecionado<AgendaItem>();
        if (item == null) return;
        if (!Ui.Confirmar("Excluir esta consulta definitivamente?\nPrefira cancelar para manter o histórico.")) return;

        ConsultaRepository.Excluir(item.IdConsulta);
        Recarregar();
    }

    private void Atender()
    {
        var item = Selecionado<AgendaItem>();
        if (item == null) return;

        if (StatusConsulta.Encerrada(item.Status))
        {
            Ui.Aviso("Consulta cancelada ou com falta não pode ser atendida.");
            return;
        }
        if (ProntuarioRepository.ConsultaTemRegistro(item.IdConsulta))
        {
            Ui.Info("O atendimento desta consulta já foi registrado. Abrindo o prontuário.");
            AbrirProntuario();
            return;
        }

        using var form = new FrmAtendimento(item);
        if (form.ShowDialog(this) == DialogResult.OK) Recarregar();
    }

    private void AbrirProntuario()
    {
        var item = Selecionado<AgendaItem>();
        if (item == null) return;

        using var form = new FrmProntuario(item.IdPaciente, item.Paciente, podeEditar: true);
        form.ShowDialog(this);
    }
}
