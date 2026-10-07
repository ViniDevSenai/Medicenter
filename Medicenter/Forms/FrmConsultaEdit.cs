using Medicenter.Models;
using Medicenter.Repositories;
using Medicenter.Services;
using Medicenter.UI;

namespace Medicenter.Forms;

public class FrmConsultaEdit : FrmEdicao
{
    private readonly Consulta _consulta;
    private readonly ModoAgenda _modo;
    private readonly bool _nova;

    private readonly ComboBox _cboPaciente, _cboMedico, _cboStatus;
    private readonly DateTimePicker _dtpData, _dtpHora;
    private readonly NumericUpDown _nudDuracao;
    private readonly TextBox _txtObservacoes;

    public FrmConsultaEdit(Consulta consulta, ModoAgenda modo)
        : base(consulta == null ? "Agendar consulta" : "Editar consulta", 520)
    {
        _nova = consulta == null;
        _consulta = consulta ?? new Consulta();
        _modo = modo;

        if (modo == ModoAgenda.Paciente)
        {
            Nota($"Paciente: {Sessao.Nome}", destaque: true);
        }
        else
        {
            _cboPaciente = Campo("Paciente *", new ComboBox
            {
                DropDownStyle = ComboBoxStyle.DropDown,
                AutoCompleteMode = AutoCompleteMode.SuggestAppend,
                AutoCompleteSource = AutoCompleteSource.ListItems
            });
        }

        _cboMedico = Campo("Médico *", new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList });
        _dtpData = Campo("Data *", new DateTimePicker { Format = DateTimePickerFormat.Short });
        _dtpHora = Campo("Hora *", new DateTimePicker
        {
            Format = DateTimePickerFormat.Custom,
            CustomFormat = "HH:mm",
            ShowUpDown = true
        });
        _nudDuracao = Campo("Duração (min)", new NumericUpDown { Minimum = 15, Maximum = 240, Increment = 15, Value = 30 });

        if (modo == ModoAgenda.Geral && !_nova)
            _cboStatus = Campo("Status", Ui.Combo(StatusConsulta.Agendada, StatusConsulta.Confirmada,
                                                  StatusConsulta.Cancelada, StatusConsulta.Faltou));

        _txtObservacoes = Campo("Observações", Ui.Multilinha(60));
        _txtObservacoes.MaxLength = 255;

        if (_nova) BtnSalvar.Text = "Agendar";
    }

    protected override void CarregarDados()
    {
        if (_cboPaciente != null)
        {
            _cboPaciente.DisplayMember = nameof(Paciente.Descricao);
            _cboPaciente.ValueMember = nameof(Paciente.IdPaciente);
            _cboPaciente.DataSource = PacienteRepository.Listar("");
            _cboPaciente.SelectedIndex = -1;
        }

        // Médicos ativos + o médico atual da consulta (caso tenha sido inativado depois)
        var medicos = MedicoRepository.Listar("", apenasAtivos: false)
            .Where(m => m.Ativo || m.IdMedico == _consulta.IdMedico)
            .ToList();
        _cboMedico.DisplayMember = nameof(Medico.Descricao);
        _cboMedico.ValueMember = nameof(Medico.IdMedico);
        _cboMedico.DataSource = medicos;

        if (_nova)
        {
            _cboMedico.SelectedIndex = -1;
            _dtpData.Value = DateTime.Today;
            _dtpHora.Value = DateTime.Today.AddHours(8);
            return;
        }

        if (_cboPaciente != null) _cboPaciente.SelectedValue = _consulta.IdPaciente;
        _cboMedico.SelectedValue = _consulta.IdMedico;
        _dtpData.Value = _consulta.DataHora.Date;
        _dtpHora.Value = DateTime.Today.Add(_consulta.DataHora.TimeOfDay);
        _nudDuracao.Value = Math.Clamp(_consulta.DuracaoMin, (short)15, (short)240);
        if (_cboStatus != null) _cboStatus.SelectedItem = _consulta.Status;
        _txtObservacoes.Text = _consulta.Observacoes;
    }

    protected override bool Salvar()
    {
        int idPaciente;
        if (_cboPaciente == null)
        {
            idPaciente = Sessao.IdPaciente.Value;
        }
        else if (_cboPaciente.SelectedValue is int id)
        {
            idPaciente = id;
        }
        else
        {
            Ui.Aviso("Selecione o paciente na lista.");
            return false;
        }

        if (_cboMedico.SelectedValue is not int idMedico)
        {
            Ui.Aviso("Selecione o médico.");
            return false;
        }

        var d = _dtpData.Value;
        var h = _dtpHora.Value;
        var inicio = new DateTime(d.Year, d.Month, d.Day, h.Hour, h.Minute, 0);
        var duracao = (short)_nudDuracao.Value;
        var status = _cboStatus?.SelectedItem as string ?? _consulta.Status;

        if (_nova && inicio < DateTime.Now)
        {
            Ui.Aviso("Não é possível agendar para uma data ou hora que já passou.");
            return false;
        }

        if (!StatusConsulta.Encerrada(status))
        {
            if (ConsultaRepository.MedicoOcupado(idMedico, inicio, duracao, _consulta.IdConsulta))
            {
                Ui.Aviso("O médico já tem consulta nesse horário. Escolha outro horário.");
                return false;
            }
            if (ConsultaRepository.PacienteOcupado(idPaciente, inicio, duracao, _consulta.IdConsulta))
            {
                Ui.Aviso("O paciente já tem consulta nesse horário.");
                return false;
            }
        }

        _consulta.IdPaciente = idPaciente;
        _consulta.IdMedico = idMedico;
        _consulta.DataHora = inicio;
        _consulta.DuracaoMin = duracao;
        _consulta.Status = status;
        _consulta.Observacoes = Texto.Nulo(_txtObservacoes.Text);
        if (_nova) _consulta.IdFuncionario = Sessao.IdFuncionario;

        ConsultaRepository.Salvar(_consulta);
        if (_nova) Ui.Info($"Consulta agendada para {inicio:dd/MM/yyyy} às {inicio:HH:mm}.");
        return true;
    }
}
