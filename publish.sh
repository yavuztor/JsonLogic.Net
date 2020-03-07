#! /bin/bash
rm JsonLogic.Net/bin/Debug/*.nupkg
dotnet pack
PKGNAME=`ls JsonLogic.Net/bin/Debug/*.nupkg`
echo PKGNAME is $PKGNAME
dotnet nuget push $PKGNAME -k "$NUGET_KEY" -s "https://api.nuget.org/v3/index.json" 2>&1 || true