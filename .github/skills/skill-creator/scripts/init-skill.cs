#!/usr/bin/env dotnet
/*
Skill Initializer - Creates a new skill from template

Usage:
    dotnet run init-skill.cs <skill-name> --path <path>

Examples:
    dotnet run init-skill.cs my-new-skill --path skills/public
    dotnet run init-skill.cs my-api-helper --path skills/private
    dotnet run init-skill.cs custom-skill --path /custom/location

.NET 10 File-Based App (FBA)
https://learn.microsoft.com/en-us/dotnet/core/sdk/file-based-apps
*/

using System;
using System.IO;
using System.Linq;
using System.Text;

const string SKILL_TEMPLATE = """
---
name: {0}
description: [TODO: 이 스킬이 무엇을 하고 언제 사용해야 하는지 완전하고 상세한 설명을 작성하세요. 이 스킬을 사용해야 하는 구체적인 시나리오, 파일 유형, 또는 작업을 포함하세요.]
---

# {1}

## 개요

[TODO: 이 스킬이 무엇을 가능하게 하는지 1-2문장으로 설명하세요]

## 스킬 구조화 가이드

[TODO: 이 스킬의 목적에 가장 적합한 구조를 선택하세요. 일반적인 패턴:

**1. 워크플로우 기반** (순차적 프로세스에 적합)
- 명확한 단계별 절차가 있을 때 효과적
- 예시: DOCX 스킬의 "워크플로우 결정 트리" → "읽기" → "생성" → "편집"
- 구조: ## 개요 → ## 워크플로우 결정 트리 → ## 1단계 → ## 2단계...

**2. 작업 기반** (도구 모음에 적합)
- 스킬이 다양한 작업/기능을 제공할 때 효과적
- 예시: PDF 스킬의 "빠른 시작" → "PDF 병합" → "PDF 분할" → "텍스트 추출"
- 구조: ## 개요 → ## 빠른 시작 → ## 작업 카테고리 1 → ## 작업 카테고리 2...

**3. 참조/가이드라인** (표준 또는 명세에 적합)
- 브랜드 가이드라인, 코딩 표준, 요구사항에 효과적
- 예시: 브랜드 스타일링의 "브랜드 가이드라인" → "색상" → "타이포그래피" → "기능"
- 구조: ## 개요 → ## 가이드라인 → ## 명세 → ## 사용법...

**4. 기능 기반** (통합 시스템에 적합)
- 스킬이 여러 상호 연관된 기능을 제공할 때 효과적
- 예시: 제품 관리의 "핵심 기능" → 번호가 매겨진 기능 목록
- 구조: ## 개요 → ## 핵심 기능 → ### 1. 기능 → ### 2. 기능...

패턴은 필요에 따라 혼합하여 사용할 수 있습니다. 대부분의 스킬은 패턴을 결합합니다 (예: 작업 기반으로 시작하고, 복잡한 작업에는 워크플로우 추가).

완료 후 이 "스킬 구조화 가이드" 섹션 전체를 삭제하세요 - 이것은 단지 안내일 뿐입니다.]

## [TODO: 선택한 구조에 따라 첫 번째 주요 섹션으로 교체하세요]

[TODO: 여기에 내용을 추가하세요. 기존 스킬의 예시 참조:
- 기술 스킬을 위한 코드 샘플
- 복잡한 워크플로우를 위한 결정 트리
- 현실적인 사용자 요청이 포함된 구체적인 예시
- 필요시 scripts/templates/references 참조]

## 리소스

이 스킬에는 다양한 유형의 번들 리소스를 구성하는 방법을 보여주는 예제 리소스 디렉토리가 포함되어 있습니다:

### scripts/
특정 작업을 수행하기 위해 직접 실행할 수 있는 실행 가능한 코드 (.NET FBA/Bash/등).

**다른 스킬의 예시:**
- PDF 스킬: `FillFormFields.cs`, `ExtractFormFieldInfo.cs` - PDF 조작 유틸리티
- DOCX 스킬: `Document.cs`, `Utilities.cs` - 문서 처리 모듈

**적합한 용도:** .NET 스크립트, 셸 스크립트, 또는 자동화, 데이터 처리, 특정 작업을 수행하는 모든 실행 가능한 코드.

**참고:** 스크립트는 컨텍스트에 로드하지 않고 실행될 수 있지만, Claude가 패치나 환경 조정을 위해 읽을 수 있습니다.

### references/
Claude의 프로세스와 사고에 정보를 제공하기 위해 컨텍스트에 로드되도록 의도된 문서 및 참조 자료.

**다른 스킬의 예시:**
- 제품 관리: `communication.md`, `context_building.md` - 상세 워크플로우 가이드
- BigQuery: API 참조 문서 및 쿼리 예제
- 재무: 스키마 문서, 회사 정책

**적합한 용도:** 심층 문서, API 참조, 데이터베이스 스키마, 종합 가이드, 또는 Claude가 작업 중 참조해야 하는 모든 상세 정보.

### assets/
컨텍스트에 로드되도록 의도되지 않고, Claude가 생성하는 출력 내에서 사용되도록 의도된 파일.

**다른 스킬의 예시:**
- 브랜드 스타일링: PowerPoint 템플릿 파일 (.pptx), 로고 파일
- 프론트엔드 빌더: HTML/React 보일러플레이트 프로젝트 디렉토리
- 타이포그래피: 폰트 파일 (.ttf, .woff2)

**적합한 용도:** 템플릿, 보일러플레이트 코드, 문서 템플릿, 이미지, 아이콘, 폰트, 또는 최종 출력에 복사하거나 사용할 모든 파일.

---

**필요하지 않은 디렉토리는 삭제할 수 있습니다.** 모든 스킬이 세 가지 유형의 리소스를 모두 필요로 하지는 않습니다.
""";

const string EXAMPLE_SCRIPT = """
#!/usr/bin/env dotnet run
/*
Example helper script for {0}

This is a placeholder script that can be executed directly.
Replace with actual implementation or delete if not needed.

Example real scripts from other skills:
- pdf/scripts/FillFormFields.cs - Fills PDF form fields
- pdf/scripts/ConvertPdfToImages.cs - Converts PDF pages to images
*/

Console.WriteLine("This is an example script for {0}");
// TODO: Add actual script logic here
// This could be data processing, file conversion, API calls, etc.
""";

const string EXAMPLE_REFERENCE = """
# Reference Documentation for {0}

This is a placeholder for detailed reference documentation.
Replace with actual reference content or delete if not needed.

Example real reference docs from other skills:
- product-management/references/communication.md - Comprehensive guide for status updates
- product-management/references/context_building.md - Deep-dive on gathering context
- bigquery/references/ - API references and query examples

## When Reference Docs Are Useful

Reference docs are ideal for:
- Comprehensive API documentation
- Detailed workflow guides
- Complex multi-step processes
- Information too lengthy for main SKILL.md
- Content that's only needed for specific use cases

## Structure Suggestions

### API Reference Example
- Overview
- Authentication
- Endpoints with examples
- Error codes
- Rate limits

### Workflow Guide Example
- Prerequisites
- Step-by-step instructions
- Common patterns
- Troubleshooting
- Best practices
""";

const string EXAMPLE_ASSET = """
# Example Asset File

This placeholder represents where asset files would be stored.
Replace with actual asset files (templates, images, fonts, etc.) or delete if not needed.

Asset files are NOT intended to be loaded into context, but rather used within
the output Claude produces.

Example asset files from other skills:
- Brand guidelines: logo.png, slides_template.pptx
- Frontend builder: hello-world/ directory with HTML/React boilerplate
- Typography: custom-font.ttf, font-family.woff2
- Data: sample_data.csv, test_dataset.json

## Common Asset Types

- Templates: .pptx, .docx, boilerplate directories
- Images: .png, .jpg, .svg, .gif
- Fonts: .ttf, .otf, .woff, .woff2
- Boilerplate code: Project directories, starter files
- Icons: .ico, .svg
- Data files: .csv, .json, .xml, .yaml

Note: This is a text placeholder. Actual assets can be any file type.
""";

string TitleCaseSkillName(string skillName)
{
    return string.Join(' ', skillName.Split('-').Select(word =>
        char.ToUpper(word[0]) + word.Substring(1)));
}

string? InitSkill(string skillName, string path)
{
    var skillDir = Path.GetFullPath(Path.Combine(path, skillName));

    if (Directory.Exists(skillDir))
    {
        Console.WriteLine($"❌ Error: Skill directory already exists: {skillDir}");
        return null;
    }

    try
    {
        Directory.CreateDirectory(skillDir);
        Console.WriteLine($"✅ Created skill directory: {skillDir}");
    }
    catch (Exception e)
    {
        Console.WriteLine($"❌ Error creating directory: {e.Message}");
        return null;
    }

    var skillTitle = TitleCaseSkillName(skillName);
    var skillContent = string.Format(SKILL_TEMPLATE, skillName, skillTitle);

    var skillMdPath = Path.Combine(skillDir, "SKILL.md");
    try
    {
        File.WriteAllText(skillMdPath, skillContent);
        Console.WriteLine("✅ Created SKILL.md");
    }
    catch (Exception e)
    {
        Console.WriteLine($"❌ Error creating SKILL.md: {e.Message}");
        return null;
    }

    try
    {
        // Create scripts/ directory with example script
        var scriptsDir = Path.Combine(skillDir, "scripts");
        Directory.CreateDirectory(scriptsDir);
        var exampleScript = Path.Combine(scriptsDir, "Example.cs");
        File.WriteAllText(exampleScript, string.Format(EXAMPLE_SCRIPT, skillName));
        Console.WriteLine("✅ Created scripts/Example.cs");

        // Create references/ directory with example reference doc
        var referencesDir = Path.Combine(skillDir, "references");
        Directory.CreateDirectory(referencesDir);
        var exampleReference = Path.Combine(referencesDir, "api_reference.md");
        File.WriteAllText(exampleReference, string.Format(EXAMPLE_REFERENCE, skillTitle));
        Console.WriteLine("✅ Created references/api_reference.md");

        // Create assets/ directory with example asset placeholder
        var assetsDir = Path.Combine(skillDir, "assets");
        Directory.CreateDirectory(assetsDir);
        var exampleAsset = Path.Combine(assetsDir, "example_asset.txt");
        File.WriteAllText(exampleAsset, EXAMPLE_ASSET);
        Console.WriteLine("✅ Created assets/example_asset.txt");
    }
    catch (Exception e)
    {
        Console.WriteLine($"❌ Error creating resource directories: {e.Message}");
        return null;
    }

    Console.WriteLine($"\n✅ Skill '{skillName}' initialized successfully at {skillDir}");
    Console.WriteLine("\nNext steps:");
    Console.WriteLine("1. Edit SKILL.md to complete the TODO items and update the description");
    Console.WriteLine("2. Customize or delete the example files in scripts/, references/, and assets/");
    Console.WriteLine("3. Run the validator when ready to check the skill structure");

    return skillDir;
}

// Main
if (args.Length < 3 || args[1] != "--path")
{
    Console.WriteLine("Usage: dotnet run init-skill.cs <skill-name> --path <path>");
    Console.WriteLine();
    Console.WriteLine("Skill name requirements:");
    Console.WriteLine("  - Hyphen-case identifier (e.g., 'data-analyzer')");
    Console.WriteLine("  - Lowercase letters, digits, and hyphens only");
    Console.WriteLine("  - Max 40 characters");
    Console.WriteLine("  - Must match directory name exactly");
    Console.WriteLine();
    Console.WriteLine("Examples:");
    Console.WriteLine("  dotnet run init-skill.cs my-new-skill --path skills/public");
    Console.WriteLine("  dotnet run init-skill.cs my-api-helper --path skills/private");
    Console.WriteLine("  dotnet run init-skill.cs custom-skill --path /custom/location");
    return 1;
}

var skillName = args[0];
var targetPath = args[2];

Console.WriteLine($"🚀 Initializing skill: {skillName}");
Console.WriteLine($"   Location: {targetPath}");
Console.WriteLine();

var result = InitSkill(skillName, targetPath);

return result != null ? 0 : 1;
