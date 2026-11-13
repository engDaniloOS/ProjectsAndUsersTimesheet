using Microsoft.EntityFrameworkCore.Migrations;

namespace TimeSheet.Migrations
{
    public partial class UserUniqueLoginEmail : Migration
    {
        private const string CaseInsensitive = "COLLATE SQL_Latin1_General_CP1_CI_AS";

        // Reatribui os registros dependentes dos usuarios duplicados (@merge) ao usuario mantido
        // e remove os duplicados com suas credenciais.
        private const string MergeTail = @"
UPDATE w SET w.[UserId] = m.[KeepId]
FROM [WorkTime] w JOIN @merge m ON m.[DupId] = w.[UserId];

INSERT INTO [ProjectUser] ([ProjectsId], [UsersId])
SELECT DISTINCT pu.[ProjectsId], m.[KeepId]
FROM [ProjectUser] pu JOIN @merge m ON m.[DupId] = pu.[UsersId]
WHERE NOT EXISTS (SELECT 1 FROM [ProjectUser] x
                  WHERE x.[UsersId] = m.[KeepId] AND x.[ProjectsId] = pu.[ProjectsId]);

DELETE pu FROM [ProjectUser] pu JOIN @merge m ON m.[DupId] = pu.[UsersId];
DELETE u FROM [User] u JOIN @merge m ON m.[DupId] = u.[Id];
DELETE c FROM [Credentials] c JOIN @merge m ON m.[DupCredentialId] = c.[Id];";

        /// <summary>
        /// Limpeza dos dados existentes antes de criar os indices unicos (login e e-mail sem diferenciar
        /// maiusculas/minusculas): preenche nulos, mescla duplicados mantendo o usuario de menor Id
        /// (horas e vinculos com projetos sao reatribuidos a ele) e remove credenciais orfas.
        /// </summary>
        private static void CleanUpExistingData(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
UPDATE [Credentials] SET [Login] = 'unknown-' + CAST([Id] AS nvarchar(20)) WHERE [Login] IS NULL;
UPDATE [User] SET [Email] = 'unknown-' + CAST([Id] AS nvarchar(20)) + '@invalid.local' WHERE [Email] IS NULL;
UPDATE [User] SET [Name] = 'unknown-' + CAST([Id] AS nvarchar(20)) WHERE [Name] IS NULL;");

            // Duplicados de login.
            migrationBuilder.Sql(@"
DECLARE @merge TABLE ([DupId] int NOT NULL, [KeepId] int NOT NULL, [DupCredentialId] int NULL);

INSERT INTO @merge ([DupId], [KeepId], [DupCredentialId])
SELECT u.[Id], k.[KeepId], u.[CredentialId]
FROM [User] u
JOIN [Credentials] c ON c.[Id] = u.[CredentialId]
JOIN (SELECT c2.[Login] " + CaseInsensitive + @" AS [Key], MIN(u2.[Id]) AS [KeepId]
      FROM [User] u2 JOIN [Credentials] c2 ON c2.[Id] = u2.[CredentialId]
      GROUP BY c2.[Login] " + CaseInsensitive + @") k
  ON k.[Key] = c.[Login] " + CaseInsensitive + @"
WHERE u.[Id] <> k.[KeepId];
" + MergeTail);

            // Duplicados de e-mail (usuarios com logins diferentes e o mesmo e-mail sao unificados).
            migrationBuilder.Sql(@"
DECLARE @merge TABLE ([DupId] int NOT NULL, [KeepId] int NOT NULL, [DupCredentialId] int NULL);

INSERT INTO @merge ([DupId], [KeepId], [DupCredentialId])
SELECT u.[Id], k.[KeepId], u.[CredentialId]
FROM [User] u
JOIN (SELECT u2.[Email] " + CaseInsensitive + @" AS [Key], MIN(u2.[Id]) AS [KeepId]
      FROM [User] u2
      GROUP BY u2.[Email] " + CaseInsensitive + @") k
  ON k.[Key] = u.[Email] " + CaseInsensitive + @"
WHERE u.[Id] <> k.[KeepId];
" + MergeTail);

            // Credenciais sem usuario nao autenticam ninguem e poderiam violar a unicidade do login.
            migrationBuilder.Sql(@"
DELETE c FROM [Credentials] c
WHERE NOT EXISTS (SELECT 1 FROM [User] u WHERE u.[CredentialId] = c.[Id]);");
        }

        protected override void Up(MigrationBuilder migrationBuilder)
        {
            CleanUpExistingData(migrationBuilder);

            migrationBuilder.DropIndex(
                name: "IX_User_Email",
                table: "User");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "User",
                type: "nvarchar(450)",
                maxLength: 450,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(450)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Email",
                table: "User",
                type: "nvarchar(450)",
                maxLength: 450,
                nullable: false,
                defaultValue: "",
                collation: "SQL_Latin1_General_CP1_CI_AS",
                oldClrType: typeof(string),
                oldType: "nvarchar(450)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Login",
                table: "Credentials",
                type: "nvarchar(450)",
                maxLength: 450,
                nullable: false,
                defaultValue: "",
                collation: "SQL_Latin1_General_CP1_CI_AS",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_User_Email",
                table: "User",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Credentials_Login",
                table: "Credentials",
                column: "Login",
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_User_Email",
                table: "User");

            migrationBuilder.DropIndex(
                name: "IX_Credentials_Login",
                table: "Credentials");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "User",
                type: "nvarchar(450)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)",
                oldMaxLength: 450);

            migrationBuilder.AlterColumn<string>(
                name: "Email",
                table: "User",
                type: "nvarchar(450)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)",
                oldMaxLength: 450,
                oldCollation: "SQL_Latin1_General_CP1_CI_AS");

            migrationBuilder.AlterColumn<string>(
                name: "Login",
                table: "Credentials",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)",
                oldMaxLength: 450,
                oldCollation: "SQL_Latin1_General_CP1_CI_AS");

            migrationBuilder.CreateIndex(
                name: "IX_User_Email",
                table: "User",
                column: "Email");
        }
    }
}
