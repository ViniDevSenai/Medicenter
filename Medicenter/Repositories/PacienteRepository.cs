using Dapper;
using Medicenter.Data;
using Medicenter.Models;
using Medicenter.Services;
using MySqlConnector;

namespace Medicenter.Repositories;

public static class PacienteRepository
{
    private const string SelectBase = @"
        SELECT p.*, u.login
          FROM paciente p
          LEFT JOIN usuario u ON u.id_usuario = p.id_usuario";

    private const string Insert = @"
        INSERT INTO paciente (id_usuario, nome, cpf, data_nascimento, sexo, telefone, email,
                              cep, logradouro, numero, bairro, cidade, uf, convenio)
        VALUES (@IdUsuario, @Nome, @Cpf, @DataNascimento, @Sexo, @Telefone, @Email,
                @Cep, @Logradouro, @Numero, @Bairro, @Cidade, @Uf, @Convenio);
        SELECT LAST_INSERT_ID();";

    private const string Update = @"
        UPDATE paciente
           SET id_usuario = @IdUsuario, nome = @Nome, cpf = @Cpf, data_nascimento = @DataNascimento,
               sexo = @Sexo, telefone = @Telefone, email = @Email, cep = @Cep,
               logradouro = @Logradouro, numero = @Numero, bairro = @Bairro, cidade = @Cidade,
               uf = @Uf, convenio = @Convenio
         WHERE id_paciente = @IdPaciente";

    public static List<Paciente> Listar(string filtro)
    {
        filtro = (filtro ?? "").Trim();
        var cpf = Texto.Digitos(filtro);

        using var c = Db.Abrir();
        return c.Query<Paciente>(SelectBase + @"
            WHERE p.nome LIKE @nome OR (@cpf <> '' AND p.cpf LIKE @cpfLike)
            ORDER BY p.nome",
            new { nome = $"%{filtro}%", cpf, cpfLike = $"%{cpf}%" }).ToList();
    }

    public static Paciente ObterPorId(int id)
    {
        using var c = Db.Abrir();
        return c.QueryFirstOrDefault<Paciente>(SelectBase + " WHERE p.id_paciente = @id", new { id });
    }

    //Cadastro feito pela recepção/admin. Login e senha são opcionais.
    public static void Salvar(Paciente p, string login, string senha)
    {
        using var c = Db.Abrir();
        using var tx = c.BeginTransaction();

        if (!string.IsNullOrWhiteSpace(login))
        {
            if (p.IdUsuario == null)
            {
                if (string.IsNullOrEmpty(senha))
                    throw new InvalidOperationException("Informe a senha para criar o acesso do paciente.");
                p.IdUsuario = UsuarioRepository.Inserir(c, tx, login, senha, Perfis.Paciente);
            }
            else
            {
                UsuarioRepository.Atualizar(c, tx, p.IdUsuario.Value, login, senha);
            }
        }

        if (p.IdPaciente == 0)
            p.IdPaciente = c.ExecuteScalar<int>(Insert, p, tx);
        else
            c.Execute(Update, p, tx);

        tx.Commit();
    }

    
    /// Autocadastro pela tela de login. Se o CPF já existe sem acesso (cadastrado na recepção),
    /// só cria o usuário e vincula. Retorna true quando vinculou a um cadastro existente.

    public static bool AutoCadastrar(Paciente p, string login, string senha)
    {
        using var c = Db.Abrir();
        using var tx = c.BeginTransaction();

        var existente = c.QueryFirstOrDefault<Paciente>(
            "SELECT * FROM paciente WHERE cpf = @Cpf", p, tx);

        if (existente != null && existente.IdUsuario != null)
            throw new InvalidOperationException("Este CPF já possui acesso. Use a tela de login.");

        var idUsuario = UsuarioRepository.Inserir(c, tx, login, senha, Perfis.Paciente);

        if (existente != null)
        {
            c.Execute("UPDATE paciente SET id_usuario = @idUsuario WHERE id_paciente = @idPaciente",
                      new { idUsuario, idPaciente = existente.IdPaciente }, tx);
        }
        else
        {
            p.IdUsuario = idUsuario;
            p.IdPaciente = c.ExecuteScalar<int>(Insert, p, tx);
        }

        tx.Commit();
        return existente != null;
    }

    public static void Excluir(int id)
    {
        try
        {
            using var c = Db.Abrir();
            using var tx = c.BeginTransaction();

            var idUsuario = c.ExecuteScalar<int?>(
                "SELECT id_usuario FROM paciente WHERE id_paciente = @id", new { id }, tx);

            // O prontuário é apagado junto (ON DELETE CASCADE)
            c.Execute("DELETE FROM paciente WHERE id_paciente = @id", new { id }, tx);

            if (idUsuario != null)
                c.Execute("DELETE FROM usuario WHERE id_usuario = @idUsuario", new { idUsuario }, tx);

            tx.Commit();
        }
        catch (MySqlException ex) when (ex.Number == 1451)
        {
            throw new InvalidOperationException("O paciente tem consultas registradas e não pode ser excluído.");
        }
    }
}
