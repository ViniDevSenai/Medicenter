using Medicenter.Models;
using Medicenter.Repositories;
using Medicenter.Services;
using Medicenter.UI;

namespace Medicenter.Forms;

//Registro do atendimento no prontuário. Ao salvar, a consulta vira REALIZADA.
public class FrmAtendimento : FrmEdicao
{
    private readonly AgendaItem _consulta;
    private readonly Prontuario _prontuario;
    private readonly TextBox _txtQueixa, _txtExame, _txtDiagnostico, _txtPrescricao;

    public FrmAtendimento(AgendaItem consulta) : base("Atendimento", 640)
    {
        _consulta = consulta;
        _prontuario = ProntuarioRepository.ObterPorPaciente(consulta.IdPaciente);

        Nota($"Paciente: {consulta.Paciente}    •    Consulta: {consulta.DataHora:dd/MM/yyyy HH:mm}", destaque: true);
        if (!string.IsNullOrWhiteSpace(_prontuario.Alergias))
        {
            var alerta = Nota($"Alergias: {_prontuario.Alergias}");
            alerta.ForeColor = Color.Firebrick;
        }

        _txtQueixa = Campo("Queixa *", Ui.Multilinha(70));
        _txtExame = Campo("Exame físico", Ui.Multilinha(70));
        _txtDiagnostico = Campo("Diagnóstico", Ui.Multilinha(60));
        _txtPrescricao = Campo("Prescrição", Ui.Multilinha(80));

        BtnSalvar.Text = "Finalizar";
    }

    protected override bool Salvar()
    {
        if (string.IsNullOrWhiteSpace(_txtQueixa.Text))
        {
            Ui.Aviso("Informe a queixa do paciente.");
            _txtQueixa.Focus();
            return false;
        }
        if (!Ui.Confirmar("Finalizar o atendimento? A consulta será marcada como REALIZADA.")) return false;

        ProntuarioRepository.RegistrarAtendimento(new ProntuarioRegistro
        {
            IdProntuario = _prontuario.IdProntuario,
            IdConsulta = _consulta.IdConsulta,
            IdMedico = Sessao.IdMedico.Value,
            Queixa = _txtQueixa.Text.Trim(),
            ExameFisico = Texto.Nulo(_txtExame.Text),
            Diagnostico = Texto.Nulo(_txtDiagnostico.Text),
            Prescricao = Texto.Nulo(_txtPrescricao.Text)
        });
        return true;
    }
}
