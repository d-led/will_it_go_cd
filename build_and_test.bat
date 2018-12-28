dotnet test wigc_test\wigc_test.csproj
IF %ERRORLEVEL% NEQ 0 exit /b %ERRORLEVEL%

dotnet run --project willitgocd\willitgocd.csproj -- help
dotnet run --project willitgocd\willitgocd.csproj -- xml -f wigc_test\data\with_environments.xml
dotnet run --project willitgocd\willitgocd.csproj -- xml -f wigc_test\data\without_environments.xml
dotnet run --project willitgocd\willitgocd.csproj -- xml -f wigc_test\data\with_templates_and_parameters.xml
dotnet run --project willitgocd\willitgocd.csproj -- xml -f wigc_test\data\no_agents.xml
