#!/usr/bin/env bash
# Copies the Azure test database into the local SQL Server of docker-compose.yml (F11) and points the app's
# user-secrets at it. The source is only read. The copy (.bacpac) holds real data: it is kept in ~/.direnistakip,
# outside the repository.
#
#   docker compose up -d
#   SOURCE_DB='<connection string of the Azure test database>' tools/copy-test-db.sh
#
# Run again to refresh the local copy (it is replaced).
set -euo pipefail

: "${SOURCE_DB:?Set SOURCE_DB to the Azure test database's connection string (Azure portal: SQL database → Connection strings).}"
PASSWORD="${LOCAL_SQL_PASSWORD:-Local-dev-only-1}"
LOCAL_DB="ResistanceSurvey"
LOCAL="Server=localhost,1433;Database=${LOCAL_DB};User Id=sa;Password=${PASSWORD};TrustServerCertificate=True;Encrypt=False"
DIR="$HOME/.direnistakip"
BACPAC="$DIR/test-copy.bacpac"
export DOTNET_ROLL_FORWARD=Major # sqlpackage on newer .NET runtimes

command -v sqlpackage >/dev/null || { echo "sqlpackage is missing: dotnet tool install -g microsoft.sqlpackage"; exit 1; }
mkdir -p "$DIR"

echo "1/3 Exporting the test database (read only)…"
sqlpackage /Action:Export /SourceConnectionString:"$SOURCE_DB" /TargetFile:"$BACPAC" /p:VerifyExtraction=False >/dev/null

echo "2/3 Replacing the local database…"
docker compose exec -T sql /opt/mssql-tools18/bin/sqlcmd -C -S localhost -U sa -P "$PASSWORD" -Q \
  "IF DB_ID('${LOCAL_DB}') IS NOT NULL BEGIN ALTER DATABASE [${LOCAL_DB}] SET SINGLE_USER WITH ROLLBACK IMMEDIATE; DROP DATABASE [${LOCAL_DB}]; END" >/dev/null
sqlpackage /Action:Import /SourceFile:"$BACPAC" /TargetConnectionString:"$LOCAL" >/dev/null

echo "3/3 Pointing the app's user-secrets at the local copy…"
dotnet user-secrets --project "$(dirname "$0")/../RS.MVC" set "ConnectionStrings:RSConnectionString" "$LOCAL" >/dev/null

echo "Done. The app now uses the local copy (database ${LOCAL_DB} on localhost)."
