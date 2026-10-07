using System.Text.Json;
using MySqlConnector;

namespace Medicenter.Data;

public static class Db
{
    private static string _connectionString;

    public static string ConnectionString
    {
        get
        {
            if (_connectionString == null)
            {
                var caminho = Path.Combine(AppContext.BaseDirectory, "appsettings.json");
                if (!File.Exists(caminho))
                    throw new FileNotFoundException("Arquivo appsettings.json não encontrado.", caminho);

                using var json = JsonDocument.Parse(File.ReadAllText(caminho));
                _connectionString = json.RootElement.GetProperty("ConnectionString").GetString();
            }
            return _connectionString;
        }
    }

    public static MySqlConnection Abrir()
    {
        var conexao = new MySqlConnection(ConnectionString);
        conexao.Open();
        return conexao;
    }
}
