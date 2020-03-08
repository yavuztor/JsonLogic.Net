#! /bin/bash
rm JsonLogic.Net/bin/Release/*.nupkg
dotnet pack -c Release --no-build
PKGNAME=`ls JsonLogic.Net/bin/Release/*.nupkg`
echo PKGNAME is $PKGNAME
dotnet nuget push $PKGNAME -k "$NUGET_KEY" -s "https://api.nuget.org/v3/index.json" 2>&1 || true