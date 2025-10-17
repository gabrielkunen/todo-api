CREATE TABLE USUARIOS (
    id SERIAL PRIMARY KEY,
    email VARCHAR (500) NOT NULL UNIQUE,
    senha VARCHAR (500) NOT NULL,
    nome VARCHAR(150) NOT NULL
);

CREATE TABLE TAREFAS (
     id SERIAL PRIMARY KEY,
     titulo VARCHAR (100) NOT NULL,
     descricao VARCHAR (500) NOT NULL,
     observacao VARCHAR (500) NULL,
     status INT NOT NULL,
     idusuario INT NOT NULL,
     dataabertura TIMESTAMP NOT NULL,
     datainicio TIMESTAMP NULL,
     datafim TIMESTAMP NULL,
     CONSTRAINT fk_usuario
         FOREIGN KEY (idusuario)
             REFERENCES USUARIOS(id)
             ON DELETE RESTRICT
);