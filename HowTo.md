## Build API Client
```bash 
./kiota.exe generate --language=CSharp --openapi=/e/repos/TraXile/TraXile/Resources/apiDefinition.yml
```

## Import Kiota Dependencies
```
dotnet add package Microsoft.Kiota.Abstractions
dotnet add package Microsoft.Kiota.Http.HttpClientLibrary
dotnet add package Microsoft.Kiota.Serialization.Form
dotnet add package Microsoft.Kiota.Serialization.Json
dotnet add package Microsoft.Kiota.Serialization.Text
dotnet add package Microsoft.Kiota.Serialization.Multipart
```