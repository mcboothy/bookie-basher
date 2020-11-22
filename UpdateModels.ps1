$workingDir = $PSScriptRoot;
$username = "bookie-basher-user";
$password = "P@ssword12";
$databaseName = "Bookie-Basher";
$databaseHost = "192.168.1.210";
$port = 3306;
$contextName = "BBDBContext";

Write-Host "Project dir= $workingDir";
Write-Host "Creating temp project to run scaffold on";

Remove-Item "$workingDir/Core/Database" -Force -Recurse;

MKDIR -Force temp/BookieBasher.Core
CD temp/BookieBasher.Core

dotnet new web
dotnet add package Microsoft.EntityFrameworkCore --version 5.0.0
dotnet add package Microsoft.EntityFrameworkCore.Tools --version 5.0.0
dotnet add package Microsoft.EntityFrameworkCore.Design --version 5.0.0
dotnet add package Pomelo.EntityFrameworkCore.MySql --version 5.0.0-alpha.2

Write-Host "Done creating temp project";

Write-Host "Scaffolding dbcontext into project";

dotnet ef dbcontext scaffold "server=$($databaseHost);port=$($port);user=$($username);password=$($password);database=$($databaseName);TreatTinyAsBoolean=true;" "Pomelo.EntityFrameworkCore.MySql" --output-dir Database --context $contextName 

Write-Host "Scaffold completed! Starting clean-up";

CD $workingDir

Copy-Item -Path "$workingDir\temp\BookieBasher.Core\Database" -Destination "$workingDir\Core\Database" -Recurse

Remove-Item "$workingDir/temp" -force -Recurse;

Write-Host "Clean-up completed!";