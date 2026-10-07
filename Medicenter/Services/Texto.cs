using System.Security.Cryptography;
using System.Text;

namespace Medicenter.Services;

public static class Texto
{
    public static string Digitos(string valor) =>
        new string((valor ?? "").Where(char.IsDigit).ToArray());

    //Retorna null para texto vazio, para gravar NULL no banco.
    public static string Nulo(string valor) =>
        string.IsNullOrWhiteSpace(valor) ? null : valor.Trim();

    public static string FormatarCpf(string cpf) =>
        cpf != null && cpf.Length == 11
            ? $"{cpf[..3]}.{cpf.Substring(3, 3)}.{cpf.Substring(6, 3)}-{cpf[9..]}"
            : cpf;

    //Mesmo resultado de SHA2('senha', 256) no MySQL.
    public static string Hash(string senha) =>
        Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(senha))).ToLowerInvariant();
}
