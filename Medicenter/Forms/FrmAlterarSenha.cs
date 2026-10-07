using Medicenter.Repositories;
using Medicenter.Services;
using Medicenter.UI;

namespace Medicenter.Forms;

public class FrmAlterarSenha : FrmEdicao
{
    private readonly TextBox _txtAtual;
    private readonly TextBox _txtNova;
    private readonly TextBox _txtConfirma;

    public FrmAlterarSenha() : base("Alterar senha", 420)
    {
        _txtAtual = Campo("Senha atual", new TextBox { UseSystemPasswordChar = true });
        _txtNova = Campo("Nova senha", new TextBox { UseSystemPasswordChar = true });
        _txtConfirma = Campo("Confirmar", new TextBox { UseSystemPasswordChar = true });
        Nota("A senha deve ter pelo menos 6 caracteres.");
    }

    protected override bool Salvar()
    {
        if (_txtNova.Text.Length < 6)
        {
            Ui.Aviso("A nova senha deve ter pelo menos 6 caracteres.");
            return false;
        }
        if (_txtNova.Text != _txtConfirma.Text)
        {
            Ui.Aviso("A confirmação não confere com a nova senha.");
            return false;
        }
        if (!UsuarioRepository.AlterarSenha(Sessao.Usuario.IdUsuario, _txtAtual.Text, _txtNova.Text))
        {
            Ui.Aviso("Senha atual incorreta.");
            return false;
        }

        Ui.Info("Senha alterada.");
        return true;
    }
}
