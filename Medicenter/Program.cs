using Dapper;
using Medicenter.Forms;
using Medicenter.UI;

namespace Medicenter;

internal static class Program
{
    [STAThread]
    private static void Main()
    {
        ApplicationConfiguration.Initialize();

        // Permite mapear colunas snake_case (id_paciente) para propriedades PascalCase (IdPaciente)
        DefaultTypeMap.MatchNamesWithUnderscores = true;

        Application.ThreadException += (s, e) => Ui.Erro(e.Exception);
        Application.Run(new FrmLogin());
    }
}
