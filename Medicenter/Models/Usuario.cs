namespace Medicenter.Models;

public class Usuario
{
    public int IdUsuario { get; set; }
    public string Login { get; set; }
    public string SenhaHash { get; set; }
    public string Perfil { get; set; }
    public bool Ativo { get; set; }
    public DateTime? UltimoAcesso { get; set; }
}
