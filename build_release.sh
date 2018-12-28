#!/bin/bash
set -euo pipefail
IFS=$'\n\t'

name="willitgocd"
origin=$(pwd)
version=0.0.1

echo "---=== origin: ${origin} ===---"

echo "---=== building release ===---"
dotnet build willitgocd/willitgocd.csproj -c release -p:Version=${version}

echo "---=== publishing linux-x64 ===---"
dotnet publish willitgocd -c release -r linux-x64 -p:Version=${version}
cd willitgocd/bin/release/netcoreapp2.0/linux-x64/publish/
echo "---=== now in $(pwd) ===---"
tar czf "${origin}/${name}-linux-x64.tgz" .
cd $origin

echo "---=== publishing osx.10.12-x64 ===---"
dotnet publish willitgocd -c release -r osx.10.12-x64 -p:Version=${version}
cd willitgocd/bin/release/netcoreapp2.0/osx.10.12-x64/publish/
echo "---=== now in $(pwd) ===---"
tar czf "${origin}/${name}-osx.10.12-x64.tgz" .
cd $origin

echo "---=== publishing win7-x64 ===---"
dotnet publish willitgocd -c release -r win7-x64 -p:Version=${version}
cd willitgocd/bin/release/netcoreapp2.0/win7-x64/publish/
echo "---=== now in $(pwd) ===---"
zip -r "${origin}/${name}-win7-x64.zip" .
cd $origin
