using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DesignTechPLMIntegrationPro.Persistence.Data
{
    public static class DatabaseInitializer
    {
        private static readonly string scheman = "PLM1";

        private static readonly Dictionary<string, string> tablesAndCommands = new Dictionary<string, string>
        {
            { "Table1", $@"
                CREATE TABLE {scheman}.Table1 (
                    TransferID varchar(MAX),
                    idA2A2 varchar(50),
                    idA3masterReference varchar(MAX),
                    statestate varchar(MAX),
                    name varchar(MAX),
                    WTPartNumber varchar(MAX),
                    updateStampA2 datetime,
                    ProcessTimestamp datetime,
                    Version varchar(MAX),
                    VersionID varchar(MAX),
                    ReviseDate datetime
                )" },

            { "Table2", $@"
                CREATE TABLE {scheman}.Table2 (
                    AnaParcaTransferID varchar(MAX),
                    AnaParcaID varchar(200),
                    AnaParcaNumber varchar(MAX),
                    AnaParcaName varchar(MAX),
                    TransferID varchar(MAX),
                    ID varchar(200),
                    ObjectType varchar(MAX),
                    Name varchar(MAX),
                    Number varchar(MAX),
                    updateStampA2 datetime,
                    modifyStampA2 datetime,
                    ProcessTimestamp datetime,
                    state varchar(MAX)
                )" },

            { "Table3", $@"
                CREATE TABLE {scheman}.Table3 (
                    TransferID varchar(MAX),
                    AdministrativeLockIsNull tinyint,
                    TypeAdministrativeLock varchar(MAX),
                    ClassNameKeyDomainRef varchar(MAX),
                    IdA3DomainRef bigint,
                    InheritedDomain tinyint,
                    ReplacementType varchar(MAX),
                    ClassNameKeyRoleAObjectRef varchar(MAX),
                    IdA3A5 bigint,
                    ClassNameKeyRoleBObjectRef varchar(MAX),
                    IdA3B5 bigint,
                    SecurityLabels varchar(MAX),
                    CreateStampA2 datetime,
                    MarkForDeleteA2 bigint,
                    ModifyStampA2 datetime,
                    ClassNameA2A2 varchar(MAX),
                    IdA2A2 bigint,
                    UpdateCountA2 int,
                    UpdateStampA2 datetime
                )" },

            { "Table4", $@"
                CREATE TABLE {scheman}.[Table4](
                    [Ent_ID] [bigint] IDENTITY(1,1) NOT NULL,
                    [EPMDocID] [bigint] NULL,
                      NULL,
                    CONSTRAINT [PK_Ent_EPMDocState] PRIMARY KEY CLUSTERED (
                        [Ent_ID] ASC
                    ) WITH (
                        PAD_INDEX = OFF,
                        STATISTICS_NORECOMPUTE = OFF,
                        IGNORE_DUP_KEY = OFF,
                        ALLOW_ROW_LOCKS = ON,
                        ALLOW_PAGE_LOCKS = ON,
                        FILLFACTOR = 80,
                        OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF
                    ) ON [PRIMARY]
                ) ON [PRIMARY];
                
                CREATE TRIGGER {scheman}.[Table4Trigger]
                ON {scheman}.[EPMDocument] 
                AFTER UPDATE
                AS 
                BEGIN
                    DECLARE @EPMDocumentID BIGINT,
                            @StateDegeri NVARCHAR(200);
                
                    SELECT @EPMDocumentID = idA2A2, @StateDegeri = statestate FROM inserted;
                
                    IF @StateDegeri = 'RELEASED'
                    BEGIN
                        IF EXISTS (SELECT 1 FROM {scheman}.EPMReferenceLink WHERE idA3A5 = @EPMDocumentID AND referenceType = 'DRAWING')
                        BEGIN
                            INSERT INTO {scheman}.Ent_EPMDocState (EPMDocID, StateDegeri) VALUES (@EPMDocumentID, @StateDegeri);
                        END
                    END
                END;
                
                ALTER TABLE {scheman}.[EPMDocument] ENABLE TRIGGER [EPMDokumanState];" },

            { "Table5", $@"
                CREATE TABLE {scheman}.[Table5](
                    [Ent_ID] [bigint] IDENTITY(1,1) NOT NULL,
                    [EPMDocID] [bigint] NULL,
                      NULL,
                    CONSTRAINT [PK_Ent_EPMDocState_CANCELLED] PRIMARY KEY CLUSTERED (
                        [Ent_ID] ASC
                    ) WITH (
                        PAD_INDEX = OFF,
                        STATISTICS_NORECOMPUTE = OFF,
                        IGNORE_DUP_KEY = OFF,
                        ALLOW_ROW_LOCKS = ON,
                        ALLOW_PAGE_LOCKS = ON,
                        FILLFACTOR = 80,
                        OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF
                    ) ON [PRIMARY]
                ) ON [PRIMARY];
                
                CREATE TRIGGER {scheman}.[Table5Trigger]
                ON {scheman}.[EPMDocument] 
                AFTER UPDATE
                AS 
                BEGIN
                    DECLARE @EPMDocumentID BIGINT,
                            @StateDegeri NVARCHAR(200);
                
                    SELECT @EPMDocumentID = idA2A2, @StateDegeri = statestate FROM inserted;
                
                    IF @StateDegeri = 'CANCELLED'
                    BEGIN
                        IF EXISTS (SELECT 1 FROM {scheman}.EPMReferenceLink WHERE idA3A5 = @EPMDocumentID AND referenceType = 'DRAWING')
                        BEGIN
                            INSERT INTO {scheman}.Ent_EPMDocState_CANCELLED (EPMDocID, StateDegeri) VALUES (@EPMDocumentID, @StateDegeri);
                        END
                    END
                END;
                
                ALTER TABLE {scheman}.[EPMDocument] ENABLE TRIGGER [EPMDokumanState_CANCELLED];" }
        };

        public static Dictionary<string, string> GetTablesAndCommands()
        {
            return tablesAndCommands;
        }

        public static async Task InitializeTablesAsync(string connectionString, string tableName)
        {
            if (tablesAndCommands.TryGetValue(tableName, out var createTableSql))
            {
                using (var connection = new SqlConnection(connectionString))
                {
                    await connection.OpenAsync();
                    using (var command = new SqlCommand(createTableSql, connection))
                    {
                        try
                        {
                            await command.ExecuteNonQueryAsync();
                        }
                        catch (Exception)
                        {
                            // Handle exception if needed
                        }
                    }
                }
            }
        }
    }
}
