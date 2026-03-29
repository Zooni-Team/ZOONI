-- Tabla de eventos públicos de la comunidad
CREATE TABLE EventoPublico (
    Id_Evento       INT IDENTITY(1,1) PRIMARY KEY,
    Titulo          NVARCHAR(200)   NOT NULL,
    Descripcion     NVARCHAR(MAX)   NULL,
    Organizador     NVARCHAR(200)   NULL,
    Fecha           DATETIME        NOT NULL,
    Hora            NVARCHAR(10)    NULL,
    Lugar           NVARCHAR(300)   NULL,
    Imagen          NVARCHAR(500)   NULL,
    Especie         NVARCHAR(100)   NULL,
    Raza            NVARCHAR(100)   NULL,
    Activo          BIT             NOT NULL DEFAULT 1,
    FechaCreacion   DATETIME        NOT NULL DEFAULT GETDATE()
);

-- Evento de ejemplo: vacunación Golden Retrievers
INSERT INTO EventoPublico (Titulo, Descripcion, Organizador, Fecha, Hora, Lugar, Imagen, Especie, Raza)
VALUES (
    'Jornada de Vacunación Gratuita para Golden Retrievers',
    'El Gobierno de la Ciudad Autónoma de Buenos Aires organiza una jornada de vacunación gratuita para Golden Retrievers. Se aplicarán vacunas antirrábicas, séxtuple y contra la leptospirosis. Traé el carnet sanitario de tu mascota.',
    'Gobierno Ciudad Autónoma de Buenos Aires',
    '20260415 09:00:00',
    '09:00',
    'Parque Centenario, Av. Díaz Vélez 4900, CABA',
    '/img/eventos/vacunacion-golden-retriever.jpg',
    'Perro',
    'Golden Retriever'
);
