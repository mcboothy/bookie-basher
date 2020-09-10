$workingDir = $PSScriptRoot;
$username = "root";
$password = "admin";
$jobPassword = "";
$databaseName = "bookie-basher";
$databaseHost = "25.54.206.254";
$port = 3306;
$contextName = "BBDBContext";

Write-Host "Project dir= $workingDir";
Write-Host "Creating temp project to run scaffold on";

Remove-Item "$workingDir/BookieBasher.Core/Database" -Force -Recurse;

MKDIR -Force temp/BookieBasher.Core
CD temp/BookieBasher.Core

dotnet new web
dotnet add package Microsoft.EntityFrameworkCore --version 3.1.1
dotnet add package Microsoft.EntityFrameworkCore.Tools --version 3.1.1
dotnet add package Microsoft.EntityFrameworkCore.Design --version 3.1.1
dotnet add package Pomelo.EntityFrameworkCore.MySql --version 3.1.1

Write-Host "Done creating temp project";

Write-Host "Scaffolding dbcontext into project";

dotnet ef dbcontext scaffold "server=$($databaseHost);port=$($port);user=$($username);password=$($password);database=$($databaseName);TreatTinyAsBoolean=true;" "Pomelo.EntityFrameworkCore.MySql" --output-dir Database --context $contextName --project "$workingDir\BookieBasher.Core\BookieBasher.Core.csproj"

Write-Host "Scaffold completed! Starting clean-up";

CD $workingDir

Remove-Item "$workingDir/temp" -force -Recurse;

Write-Host "Clean-up completed!";