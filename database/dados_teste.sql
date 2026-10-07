-- Dados de teste. Senha de todos os usuários: 123456
USE medicenter;

INSERT INTO especialidade (nome) VALUES
  ('Clínica Geral'), ('Cardiologia'), ('Pediatria'), ('Dermatologia');

INSERT INTO usuario (login, senha_hash, perfil) VALUES
  ('admin',          SHA2('123456', 256), 'ADMIN'),
  ('carlos.mendes',  SHA2('123456', 256), 'MEDICO'),
  ('ana.ribeiro',    SHA2('123456', 256), 'MEDICO'),
  ('juliana.souza',  SHA2('123456', 256), 'FUNCIONARIO'),
  ('pedro.alves',    SHA2('123456', 256), 'PACIENTE'),
  ('maria.lima',     SHA2('123456', 256), 'PACIENTE');

INSERT INTO medico (id_usuario, id_especialidade, nome, cpf, crm, uf_crm, telefone, email) VALUES
  (2, 1, 'Carlos Mendes', '11122233344', '45871', 'MG', '31988887777', 'carlos@medicenter.com'),
  (3, 2, 'Ana Ribeiro',   '22233344455', '51209', 'MG', '31977776666', 'ana@medicenter.com');

INSERT INTO funcionario (id_usuario, nome, cpf, cargo, telefone, email, data_admissao) VALUES
  (4, 'Juliana Souza', '33344455566', 'Recepcionista', '31966665555', 'juliana@medicenter.com', '2025-03-10');

-- O trigger cria um prontuário para cada paciente inserido
INSERT INTO paciente (id_usuario, nome, cpf, data_nascimento, sexo, telefone, email, cidade, uf, convenio) VALUES
  (5,    'Pedro Alves',  '44455566677', '1990-05-14', 'M', '31955554444', 'pedro@email.com', 'Belo Horizonte', 'MG', 'Unimed'),
  (6,    'Maria Lima',   '55566677788', '1985-11-02', 'F', '31944443333', 'maria@email.com', 'Contagem',       'MG', NULL),
  (NULL, 'João Pereira', '66677788899', '2018-08-20', 'M', '31933332222', NULL,              'Belo Horizonte', 'MG', 'Hapvida');

UPDATE prontuario
   SET tipo_sanguineo = 'O+', alergias = 'Dipirona'
 WHERE id_paciente = 1;

INSERT INTO consulta (id_paciente, id_medico, id_funcionario, data_hora, status) VALUES
  (1, 1, 1, '2026-10-06 09:00:00', 'REALIZADA'),
  (2, 2, 1, '2026-10-06 10:00:00', 'AGENDADA'),
  (3, 1, 1, '2026-10-07 14:00:00', 'CONFIRMADA');

INSERT INTO prontuario_registro (id_prontuario, id_consulta, id_medico, queixa, diagnostico, prescricao) VALUES
  (1, 1, 1, 'Dor de garganta há 3 dias', 'Faringite viral', 'Paracetamol 750 mg de 8/8h por 3 dias');
