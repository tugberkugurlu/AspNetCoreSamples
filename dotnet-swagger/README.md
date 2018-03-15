# Swagger.json generator at build time

## How to Run

```
dotnet msbuild TurquoiseSoftware.DotNetTools.Swagger.TestApp/TurquoiseSoftware.DotNetTools.Swagger.TestApp.csproj /t:GenerateSwaggerJson /property:configuration=RELEASE
```

Expected output:

```
➜  dotnet-swagger git:(master) ✗ dotnet msbuild TurquoiseSoftware.DotNetTools.Swagger.TestApp/TurquoiseSoftware.DotNetTools.Swagger.TestApp.csproj /t:GenerateSwaggerJson /property:configuration=RELEASE
Microsoft (R) Build Engine version 15.5.180.51428 for .NET Core
Copyright (C) Microsoft Corporation. All rights reserved.

  {"swagger":"2.0","info":{"title":"Swagger TestApp","description":"lorem foo impsum","contact":{"name":"Foo bar","url":"http://tugberkugurlu.com","email":"foo@bar.com"}},"paths":{},"definitions":{}}
```