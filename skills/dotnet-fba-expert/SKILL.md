---
name: dotnet-fba-expert
description: .NET 10 File-Based Apps (FBA) 전문가. 프로젝트 파일 없이 단일 .cs 파일로 .NET 애플리케이션을 빌드, 실행, 게시하는 경량 방식. 다음 상황에서 사용: (1) 단일 파일 C# 스크립트/유틸리티 작성, (2) NuGet 패키지를 참조하는 간단한 도구 생성, (3) Native AOT로 빠른 CLI 도구 빌드, (4) .csproj 없이 빠른 프로토타이핑, (5) FBA 지시문(#:package, #:property, #:sdk, #:project) 사용법 질문, (6) FBA를 일반 프로젝트로 변환
---

# .NET 10 File-Based Apps (FBA) 전문가

## 개요

File-Based Apps는 .NET 10에서 도입된 기능으로, 단일 C# 파일에서 프로젝트 파일(.csproj) 없이 .NET 애플리케이션을 빌드, 실행, 게시할 수 있게 해준다.

## 빠른 시작

### 기본 실행

```csharp
// hello.cs
Console.WriteLine("Hello, FBA!");
```

```bash
dotnet run hello.cs
# 또는 단축 형식
dotnet hello.cs
```

### NuGet 패키지 사용

```csharp
#!/usr/bin/env dotnet
#:package Newtonsoft.Json

using Newtonsoft.Json;

var obj = new { Name = "Example", Value = 42 };
Console.WriteLine(JsonConvert.SerializeObject(obj));
```

## 지시문 (Directives)

FBA는 `#:` 접두사가 붙은 지시문으로 빌드 설정을 구성한다. 파일 상단에 배치한다.

| 지시문 | 용도 | 예시 |
|--------|------|------|
| `#:package` | NuGet 패키지 참조 | `#:package Newtonsoft.Json` |
| `#:package ... version="x.x"` | 특정 버전 패키지 | `#:package Serilog version="3.1.1"` |
| `#:property` | MSBuild 속성 설정 | `#:property TargetFramework=net10.0` |
| `#:sdk` | SDK 지정 (기본: Microsoft.NET.Sdk) | `#:sdk Microsoft.NET.Sdk.Web` |
| `#:project` | 다른 프로젝트 참조 | `#:project ../Lib/Lib.csproj` |

## CLI 명령어

| 명령어 | 설명 |
|--------|------|
| `dotnet run file.cs` | 실행 |
| `dotnet file.cs` | 실행 (단축) |
| `dotnet run file.cs -- arg1 arg2` | 인수 전달 |
| `dotnet build file.cs` | 빌드 |
| `dotnet publish file.cs` | 게시 (Native AOT 기본 활성화) |
| `dotnet pack file.cs` | .NET 도구로 패키징 |
| `dotnet clean file.cs` | 빌드 출력 정리 |
| `dotnet restore file.cs` | NuGet 복원 |
| `dotnet project convert file.cs` | .csproj 프로젝트로 변환 |

## 일반적인 패턴

### Web API 서버

```csharp
#!/usr/bin/env dotnet
#:sdk Microsoft.NET.Sdk.Web

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

app.MapGet("/", () => "Hello from FBA!");
app.MapGet("/api/data", () => new { Message = "JSON response", Time = DateTime.Now });

app.Run();
```

### HTTP 클라이언트

```csharp
#!/usr/bin/env dotnet
#:package System.Net.Http.Json

using System.Net.Http.Json;

using var client = new HttpClient();
var data = await client.GetFromJsonAsync<JsonElement>("https://api.example.com/data");
Console.WriteLine(data);
```

### JSON 처리

```csharp
#!/usr/bin/env dotnet
#:package System.Text.Json

using System.Text.Json;

var options = new JsonSerializerOptions { WriteIndented = true };
var json = JsonSerializer.Serialize(new { Name = "Test", Values = new[] { 1, 2, 3 } }, options);
Console.WriteLine(json);
```

### 파일 처리

```csharp
#!/usr/bin/env dotnet

var inputPath = args.Length > 0 ? args[0] : throw new ArgumentException("파일 경로 필요");
var content = await File.ReadAllTextAsync(inputPath);
Console.WriteLine($"파일 크기: {content.Length} 문자");
```

### Native AOT 비활성화

Native AOT가 필요 없거나 호환성 문제가 있는 경우:

```csharp
#:property PublishAot=false
```

## 주의사항

### 폴더 레이아웃

- ❌ `.csproj` 프로젝트 디렉토리 내부에 FBA 파일 배치 금지
- ✅ FBA 파일은 별도 디렉토리에 배치
- `Directory.Build.props` 등 상위 빌드 파일의 영향에 주의

### 캐시 문제 해결

```bash
# 캐시 없이 빌드
dotnet build file.cs --no-cache

# 클린 후 빌드
dotnet clean file.cs
dotnet build file.cs
```

### 기본 포함 항목

- 단일 .cs 파일 자체
- 동일 디렉토리의 ResX 리소스 파일
- Web SDK 사용 시 `*.json` 구성 파일 포함

## Launch Profile (선택)

개발 시 실행 구성이 필요하면 `[파일명].run.json` 생성:

```json
// app.run.json
{
  "profiles": {
    "dev": {
      "commandName": "Project",
      "environmentVariables": {
        "ASPNETCORE_ENVIRONMENT": "Development"
      }
    }
  }
}
```

```bash
dotnet run app.cs --launch-profile dev
```