dotnet test wigc_test\wigc_test.csproj
IF %ERRORLEVEL% NEQ 0 exit /b %ERRORLEVEL%

dotnet run --project willitgocd\willitgocd.csproj -- help
dotnet run --project willitgocd\willitgocd.csproj -- xml -f wigc_test\data\with_environments.xml