-- Add PesoDisplay column to Mascota table
IF NOT EXISTS (
    SELECT * FROM sys.columns 
    WHERE object_id = OBJECT_ID(N'[dbo].[Mascota]') 
    AND name = 'PesoDisplay'
)
BEGIN
    ALTER TABLE [dbo].[Mascota]
    ADD [PesoDisplay] NVARCHAR(20) NULL;

    -- Update existing records to copy Peso to PesoDisplay
    UPDATE [dbo].[Mascota]
    SET [PesoDisplay] = CAST([Peso] AS NVARCHAR(20));
END