# Medicenter

Sistema de gestão da clínica Medicenter: cadastro de pacientes, médicos e funcionários, agenda de consultas e prontuário eletrônico. Desktop em C# WinForms (.NET 8) com MySQL.

## Requisitos

- Windows 10 ou 11
- Visual Studio 2022 com a carga de trabalho "Desenvolvimento para desktop com .NET"
- MySQL Server 8.0 ou superior

## Como rodar

1. No MySQL Workbench (ou terminal), execute `database/medicenter.sql` e depois `database/dados_teste.sql`.
2. Abra `Medicenter/appsettings.json` e ajuste usuário e senha do MySQL na connection string.
3. Abra `Medicenter.sln` no Visual Studio. Os pacotes NuGet (Dapper e MySqlConnector) são baixados no primeiro build.
4. Pressione F5.

## Usuários de teste

Senha de todos: `123456`

| Login | Perfil | O que acessa |
| --- | --- | --- |
| admin | ADMIN | Tudo: pacientes, médicos, funcionários, especialidades, consultas |
| juliana.souza | FUNCIONARIO | Pacientes e agenda de consultas |
| carlos.mendes | MEDICO | Própria agenda, atendimento e prontuários |
| ana.ribeiro | MEDICO | Própria agenda, atendimento e prontuários |
| pedro.alves | PACIENTE | Próprias consultas (agendar e cancelar) e dados pessoais |
| maria.lima | PACIENTE | Próprias consultas (agendar e cancelar) e dados pessoais |

O paciente João Pereira (CPF 666.777.888-99) está cadastrado sem login. Use "Não tenho acesso — quero me cadastrar" na tela de login com esse CPF para testar o vínculo do acesso a um cadastro já existente.

## Regras implementadas

- Só entra no sistema quem tem login ativo. Paciente sem acesso se cadastra pela tela de login; médicos e funcionários são cadastrados pelo administrador.
- Senhas gravadas como hash SHA-256 (mesmo resultado do `SHA2(senha, 256)` do MySQL).
- Agendamento bloqueia conflito de horário do médico e do paciente, considerando a duração da consulta. No banco, a coluna `horario_ativo` impede dois agendamentos ativos no mesmo horário; consultas canceladas ou com falta liberam o horário.
- O prontuário é criado automaticamente (trigger) quando o paciente é cadastrado.
- Ao finalizar o atendimento, o registro vai para o prontuário e a consulta passa para REALIZADA na mesma transação.
- Médicos e funcionários não são excluídos, são inativados, para manter o histórico. Paciente com consultas também não pode ser excluído.
- Funcionário não tem acesso a dados clínicos; só médicos editam o prontuário.

## Estrutura

```
database/        scripts SQL (criação do banco e dados de teste)
docs/            diagrama de casos de uso (PlantUML)
Medicenter/
  Data/          conexão com o MySQL
  Models/        classes das tabelas
  Repositories/  acesso a dados com Dapper (CRUD)
  Services/      login, sessão do usuário, utilitários
  UI/            telas base reutilizadas (lista e edição)
  Forms/         telas do sistema
```

## Diagrama de casos de uso

O arquivo `docs/casos-de-uso.puml` pode ser aberto no [PlantText](https://www.planttext.com) ou na extensão PlantUML do VS Code para gerar a imagem.

## Publicar no GitHub

```
git init
git add .
git commit -m "Medicenter: versão inicial"
git branch -M main
git remote add origin https://github.com/SEU_USUARIO/medicenter.git
git push -u origin main
```
