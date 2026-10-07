-- =========================================================
-- MEDICENTER - Script de criação do banco
-- MySQL 8.0+
-- =========================================================
DROP DATABASE IF EXISTS medicenter;
CREATE DATABASE medicenter
  CHARACTER SET utf8mb4
  COLLATE utf8mb4_unicode_ci;
USE medicenter;

-- 1. USUARIO: login de acesso de qualquer perfil
CREATE TABLE usuario (
  id_usuario     INT AUTO_INCREMENT PRIMARY KEY,
  login          VARCHAR(100) NOT NULL,
  senha_hash     VARCHAR(255) NOT NULL,
  perfil         ENUM('ADMIN','MEDICO','FUNCIONARIO','PACIENTE') NOT NULL,
  ativo          TINYINT(1)   NOT NULL DEFAULT 1,
  ultimo_acesso  DATETIME     NULL,
  criado_em      DATETIME     NOT NULL DEFAULT CURRENT_TIMESTAMP,
  CONSTRAINT uq_usuario_login UNIQUE (login)
) ENGINE=InnoDB;

-- 2. ESPECIALIDADE
CREATE TABLE especialidade (
  id_especialidade INT AUTO_INCREMENT PRIMARY KEY,
  nome             VARCHAR(80) NOT NULL,
  CONSTRAINT uq_especialidade_nome UNIQUE (nome)
) ENGINE=InnoDB;

-- 3. PACIENTE
CREATE TABLE paciente (
  id_paciente      INT AUTO_INCREMENT PRIMARY KEY,
  id_usuario       INT          NULL,
  nome             VARCHAR(120) NOT NULL,
  cpf              CHAR(11)     NOT NULL,
  data_nascimento  DATE         NOT NULL,
  sexo             ENUM('M','F','O') NULL,
  telefone         VARCHAR(15)  NULL,
  email            VARCHAR(100) NULL,
  cep              CHAR(8)      NULL,
  logradouro       VARCHAR(120) NULL,
  numero           VARCHAR(10)  NULL,
  bairro           VARCHAR(60)  NULL,
  cidade           VARCHAR(60)  NULL,
  uf               CHAR(2)      NULL,
  convenio         VARCHAR(60)  NULL,
  criado_em        DATETIME     NOT NULL DEFAULT CURRENT_TIMESTAMP,
  CONSTRAINT uq_paciente_cpf     UNIQUE (cpf),
  CONSTRAINT uq_paciente_usuario UNIQUE (id_usuario),
  CONSTRAINT fk_paciente_usuario FOREIGN KEY (id_usuario)
    REFERENCES usuario (id_usuario) ON DELETE SET NULL
) ENGINE=InnoDB;

-- 4. MEDICO
CREATE TABLE medico (
  id_medico         INT AUTO_INCREMENT PRIMARY KEY,
  id_usuario        INT          NOT NULL,
  id_especialidade  INT          NOT NULL,
  nome              VARCHAR(120) NOT NULL,
  cpf               CHAR(11)     NOT NULL,
  crm               VARCHAR(10)  NOT NULL,
  uf_crm            CHAR(2)      NOT NULL,
  telefone          VARCHAR(15)  NULL,
  email             VARCHAR(100) NULL,
  ativo             TINYINT(1)   NOT NULL DEFAULT 1,
  criado_em         DATETIME     NOT NULL DEFAULT CURRENT_TIMESTAMP,
  CONSTRAINT uq_medico_cpf     UNIQUE (cpf),
  CONSTRAINT uq_medico_crm     UNIQUE (crm, uf_crm),
  CONSTRAINT uq_medico_usuario UNIQUE (id_usuario),
  CONSTRAINT fk_medico_usuario FOREIGN KEY (id_usuario)
    REFERENCES usuario (id_usuario),
  CONSTRAINT fk_medico_especialidade FOREIGN KEY (id_especialidade)
    REFERENCES especialidade (id_especialidade)
) ENGINE=InnoDB;

-- 5. FUNCIONARIO
CREATE TABLE funcionario (
  id_funcionario  INT AUTO_INCREMENT PRIMARY KEY,
  id_usuario      INT          NOT NULL,
  nome            VARCHAR(120) NOT NULL,
  cpf             CHAR(11)     NOT NULL,
  cargo           VARCHAR(60)  NOT NULL,
  telefone        VARCHAR(15)  NULL,
  email           VARCHAR(100) NULL,
  data_admissao   DATE         NOT NULL,
  ativo           TINYINT(1)   NOT NULL DEFAULT 1,
  criado_em       DATETIME     NOT NULL DEFAULT CURRENT_TIMESTAMP,
  CONSTRAINT uq_funcionario_cpf     UNIQUE (cpf),
  CONSTRAINT uq_funcionario_usuario UNIQUE (id_usuario),
  CONSTRAINT fk_funcionario_usuario FOREIGN KEY (id_usuario)
    REFERENCES usuario (id_usuario)
) ENGINE=InnoDB;

-- 6. CONSULTA (agendamento)
--    horario_ativo fica NULL quando a consulta é cancelada ou o paciente falta,
--    liberando o horário para um novo agendamento.
CREATE TABLE consulta (
  id_consulta     INT AUTO_INCREMENT PRIMARY KEY,
  id_paciente     INT      NOT NULL,
  id_medico       INT      NOT NULL,
  id_funcionario  INT      NULL,  -- quem agendou (NULL = o próprio paciente ou o admin)
  data_hora       DATETIME NOT NULL,
  duracao_min     SMALLINT NOT NULL DEFAULT 30,
  status          ENUM('AGENDADA','CONFIRMADA','REALIZADA','CANCELADA','FALTOU')
                  NOT NULL DEFAULT 'AGENDADA',
  observacoes     VARCHAR(255) NULL,
  criado_em       DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
  horario_ativo   DATETIME AS (IF(status IN ('CANCELADA','FALTOU'), NULL, data_hora)) STORED,
  CONSTRAINT uq_consulta_medico_horario UNIQUE (id_medico, horario_ativo),
  CONSTRAINT fk_consulta_paciente FOREIGN KEY (id_paciente)
    REFERENCES paciente (id_paciente),
  CONSTRAINT fk_consulta_medico FOREIGN KEY (id_medico)
    REFERENCES medico (id_medico),
  CONSTRAINT fk_consulta_funcionario FOREIGN KEY (id_funcionario)
    REFERENCES funcionario (id_funcionario) ON DELETE SET NULL
) ENGINE=InnoDB;

CREATE INDEX idx_consulta_data     ON consulta (data_hora);
CREATE INDEX idx_consulta_paciente ON consulta (id_paciente);

-- 7. PRONTUARIO (um por paciente)
CREATE TABLE prontuario (
  id_prontuario    INT AUTO_INCREMENT PRIMARY KEY,
  id_paciente      INT      NOT NULL,
  tipo_sanguineo   ENUM('A+','A-','B+','B-','AB+','AB-','O+','O-') NULL,
  alergias         TEXT     NULL,
  doencas_cronicas TEXT     NULL,
  medicamentos_uso TEXT     NULL,
  criado_em        DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
  CONSTRAINT uq_prontuario_paciente UNIQUE (id_paciente),
  CONSTRAINT fk_prontuario_paciente FOREIGN KEY (id_paciente)
    REFERENCES paciente (id_paciente) ON DELETE CASCADE
) ENGINE=InnoDB;

-- 8. PRONTUARIO_REGISTRO (evolução de cada consulta)
CREATE TABLE prontuario_registro (
  id_registro     INT AUTO_INCREMENT PRIMARY KEY,
  id_prontuario   INT      NOT NULL,
  id_consulta     INT      NOT NULL,
  id_medico       INT      NOT NULL,
  queixa          TEXT     NOT NULL,
  exame_fisico    TEXT     NULL,
  diagnostico     TEXT     NULL,
  prescricao      TEXT     NULL,
  data_registro   DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
  CONSTRAINT uq_registro_consulta UNIQUE (id_consulta),
  CONSTRAINT fk_registro_prontuario FOREIGN KEY (id_prontuario)
    REFERENCES prontuario (id_prontuario) ON DELETE CASCADE,
  CONSTRAINT fk_registro_consulta FOREIGN KEY (id_consulta)
    REFERENCES consulta (id_consulta),
  CONSTRAINT fk_registro_medico FOREIGN KEY (id_medico)
    REFERENCES medico (id_medico)
) ENGINE=InnoDB;

-- TRIGGER: cria o prontuário automaticamente ao cadastrar paciente
DELIMITER $$
CREATE TRIGGER trg_paciente_cria_prontuario
AFTER INSERT ON paciente
FOR EACH ROW
BEGIN
  INSERT INTO prontuario (id_paciente) VALUES (NEW.id_paciente);
END$$
DELIMITER ;

-- VIEW: agenda legível
CREATE VIEW vw_agenda AS
SELECT c.id_consulta,
       c.data_hora,
       c.duracao_min,
       c.status,
       p.id_paciente, p.nome AS paciente, p.telefone AS telefone_paciente,
       m.id_medico,   m.nome AS medico,
       e.nome AS especialidade
FROM consulta c
JOIN paciente p      ON p.id_paciente = c.id_paciente
JOIN medico m        ON m.id_medico = c.id_medico
JOIN especialidade e ON e.id_especialidade = m.id_especialidade;
