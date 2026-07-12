<#
.SYNOPSIS
    Scanner script to detect Clean Architecture violations across presentation layer (.cshtml, .cshtml.cs, Controllers, .razor, .razor.cs).

.DESCRIPTION
    Scans files matching pattern and outputs structured JSON and/or Markdown reports categorized by Rule IDs:
    - CA-ERR-01: Direct AppDbContext / DbSet injection or usage in UI/Presentation
    - CA-ERR-02: Direct EF Core / LINQ queries (.Where, .Include, .FirstOrDefaultAsync) on Repositories/DB in UI
    - CA-ERR-03: Business logic branching / constraints check (if/else on StartTime, EndTime, Capacity, Status) in UI
    - CA-WARN-01: Field-level exception mapping (catch ArgumentException / InvalidOperationException) in UI
    - CA-WARN-02: UI badge / color status switch/case logic inside View HTML (.cshtml / .razor)
    - CA-CLEAN-01: Low-level mechanical code cleanup (unused DAL/EF using statements, empty catch blocks, dead commented context code)

.PARAMETER TargetDir
    Directory path to scan. Defaults to current directory.
.PARAMETER OutputFormat
    Report format: 'Markdown', 'JSON', or 'Both'. Defaults to 'Both'.
.PARAMETER OutputPath
    Optional explicit path for report output. Defaults to .agents/docs/reports/clean-arch-scan-{dir}-{timestamp}.md
#>

param(
    [string]$TargetDir = ".",
    [ValidateSet("Markdown", "JSON", "Both")]
    [string]$OutputFormat = "Both",
    [string]$OutputPath = ""
)

$ErrorActionPreference = "Stop"

# Ensure output directory exists
$timestamp = Get-Date -Format "yyyyMMdd-HHmm"
$dirName = (Get-Item -Path $TargetDir).Name
if (-not $dirName -or $dirName -eq ".") { $dirName = "project" }

$reportsDir = ".agents/docs/reports"
if (-not (Test-Path -Path $reportsDir)) {
    New-Item -ItemType Directory -Path $reportsDir -Force | Out-Null
}

if (-not $OutputPath) {
    $OutputPath = Join-Path -Path $reportsDir -ChildPath "clean-arch-scan-$dirName-$timestamp.md"
}
$jsonOutputPath = [System.IO.Path]::ChangeExtension($OutputPath, ".json")

# Excluded folders
$excludeDirs = @("bin", "obj", "Migrations", ".git", ".agents", "node_modules")

# Rules Definition
$rules = @{
    "CA-ERR-01" = @{ Severity = "Error"; Description = "Tiêm trực tiếp AppDbContext / IDbContext / DbSet vào tầng Presentation (.cshtml.cs, Controller, .razor)" }
    "CA-ERR-02" = @{ Severity = "Error"; Description = "Gọi trực tiếp LINQ/EF Core query (.Where, .Include, .FirstOrDefaultAsync, .ToListAsync) trong Presentation" }
    "CA-ERR-03" = @{ Severity = "Error"; Description = "Chứa logic nghiệp vụ rẽ nhánh (if/else kiểm tra StartTime, EndTime, MaxCapacity, Status) tại tầng UI" }
    "CA-WARN-01" = @{ Severity = "Warning"; Description = "Bắt ngoại lệ nghiệp vụ riêng lẻ (catch ArgumentException / InvalidOperationException) tự ánh xạ vào form field" }
    "CA-WARN-02" = @{ Severity = "Warning"; Description = "Chứa switch/case ánh xạ màu sắc UI (badgeClass, bg-) hoặc trạng thái nằm trực tiếp bên trong View HTML" }
    "CA-CLEAN-01" = @{ Severity = "Notice"; Description = "Code thừa cơ học: using DAL/EF Core khi không dùng DB, khối catch rỗng, hoặc comment code chết" }
}

$violations = @()

# Get target files
$files = Get-ChildItem -Path $TargetDir -Recurse -File | Where-Object {
    $ext = $_.Extension.ToLower()
    ($ext -eq ".cs" -or $ext -eq ".cshtml" -or $ext -eq ".razor") -and
    -not ($excludeDirs | Where-Object { $_.FullName -like "*\$_\\*" -or $_.FullName -like "*\$_/*" })
}

foreach ($file in $files) {
    $relPath = $file.FullName.Replace((Get-Location).Path + "\", "").Replace((Get-Location).Path + "/", "")
    
    # Filter only Presentation files (Controllers, Pages, Views, Blazor components)
    # Skip DAL, BLL, BusinessObjects unless explicitly scanned
    if ($relPath -like "*DAL\*" -or $relPath -like "*BLL\*" -or $relPath -like "*BusinessObjects\*") {
        continue
    }

    $lines = [System.IO.File]::ReadAllLines($file.FullName, [System.Text.Encoding]::UTF8)
    
    for ($i = 0; $i -lt $lines.Count; $i++) {
        $lineNum = $i + 1
        $lineText = $lines[$i]

        # Check clean-arch-ignore
        if ($lineText -like "*// clean-arch-ignore*" -or $lineText -like "*clean-arch-ignore*") {
            continue
        }
        if ($i -gt 0 -and ($lines[$i-1] -like "*// clean-arch-ignore*" -or $lines[$i-1] -like "*clean-arch-ignore*")) {
            continue
        }

        # Rule CA-ERR-01: Direct AppDbContext / DbSet
        if ($lineText -match "\b(AppDbContext|IDbContext|DbSet<|DbContext)\b" -and $relPath -notlike "*Program.cs*" -and $relPath -notlike "*Startup.cs*") {
            $violations += [PSCustomObject]@{
                RuleId = "CA-ERR-01"
                Severity = $rules["CA-ERR-01"].Severity
                Description = $rules["CA-ERR-01"].Description
                File = $relPath
                Line = $lineNum
                CodeSnippet = $lineText.Trim()
            }
        }

        # Rule CA-ERR-02: Direct EF Core / LINQ Queries
        if ($lineText -match "\.(Where|FirstOrDefaultAsync|FirstOrDefault|Include|ThenInclude|ToListAsync|AnyAsync|CountAsync)\(" -and $file.Extension -eq ".cs") {
            # Ensure it's not just a collection filter on a ViewModel/DTO in UI if benign, but flag for review
            $violations += [PSCustomObject]@{
                RuleId = "CA-ERR-02"
                Severity = $rules["CA-ERR-02"].Severity
                Description = $rules["CA-ERR-02"].Description
                File = $relPath
                Line = $lineNum
                CodeSnippet = $lineText.Trim()
            }
        }

        # Rule CA-ERR-03: Business logic check in UI (StartTime, EndTime, Capacity, Status comparisons)
        if ($lineText -match "if\s*\(.*(StartTime|EndTime|Capacity|MaxCapacity|Price|Quantity|VenueId).*\)") {
            $violations += [PSCustomObject]@{
                RuleId = "CA-ERR-03"
                Severity = $rules["CA-ERR-03"].Severity
                Description = $rules["CA-ERR-03"].Description
                File = $relPath
                Line = $lineNum
                CodeSnippet = $lineText.Trim()
            }
        }

        # Rule CA-WARN-01: Field-level specific exception catch in UI
        if ($lineText -match "catch\s*\(\s*(System\.)?(ArgumentException|InvalidOperationException|DbUpdateConcurrencyException)") {
            $violations += [PSCustomObject]@{
                RuleId = "CA-WARN-01"
                Severity = $rules["CA-WARN-01"].Severity
                Description = $rules["CA-WARN-01"].Description
                File = $relPath
                Line = $lineNum
                CodeSnippet = $lineText.Trim()
            }
        }

        # Rule CA-WARN-02: UI Badge / Status switch/case inside View HTML or UI methods
        if ($lineText -match "(switch\s*\(|switch\s*\{|=>\s*.*bg-)" -and ($file.Extension -eq ".cshtml" -or $file.Extension -eq ".razor" -or $lineText -match "(bg-success|bg-danger|bg-warning|bg-info|bg-secondary)")) {
            $violations += [PSCustomObject]@{
                RuleId = "CA-WARN-02"
                Severity = $rules["CA-WARN-02"].Severity
                Description = $rules["CA-WARN-02"].Description
                File = $relPath
                Line = $lineNum
                CodeSnippet = $lineText.Trim()
            }
        }

        # Rule CA-CLEAN-01: Mechanical cleanups (unused DAL/EF using in PageModel, empty catch, commented context calls)
        if (($lineText -match "using\s+(DAL|Microsoft\.EntityFrameworkCore|System\.Data);" -and $file.Extension -eq ".cs" -and ($lines -join "`n") -notmatch "\b(_context|DbSet|AppDbContext)\b") -or
            ($lineText -match "catch\s*\([^\)]*\)\s*\{\s*\}") -or
            ($lineText -match "^\s*//\s*(await\s+_context\.|var\s+.*\s*=\s*_context\.)")) {
            $violations += [PSCustomObject]@{
                RuleId = "CA-CLEAN-01"
                Severity = $rules["CA-CLEAN-01"].Severity
                Description = $rules["CA-CLEAN-01"].Description
                File = $relPath
                Line = $lineNum
                CodeSnippet = $lineText.Trim()
            }
        }
    }
}

# Output Generation
if ($OutputFormat -eq "JSON" -or $OutputFormat -eq "Both") {
    $jsonContent = $violations | ConvertTo-Json -Depth 4
    [System.IO.File]::WriteAllText($jsonOutputPath, $jsonContent, [System.Text.Encoding]::UTF8)
    Write-Host "Report exported to JSON: $jsonOutputPath" -ForegroundColor Cyan
}

if ($OutputFormat -eq "Markdown" -or $OutputFormat -eq "Both") {
    $sb = [System.Text.StringBuilder]::new()
    [void]$sb.AppendLine("# Clean Architecture Scan Report: $dirName")
    [void]$sb.AppendLine("**Scan Timestamp**: $(Get-Date -Format 'yyyy-MM-dd HH:mm:ss')  ")
    [void]$sb.AppendLine("**Target Directory**: ${TargetDir}  ")
    [void]$sb.AppendLine("**Total Violations Found**: $($violations.Count)")
    [void]$sb.AppendLine("")

    # Summary table
    [void]$sb.AppendLine("## Summary by Rule ID")
    [void]$sb.AppendLine("| Rule ID | Severity | Description | Count | Tier Level |")
    [void]$sb.AppendLine("| :--- | :--- | :--- | :--- | :--- |")
    
    $grouped = $violations | Group-Object RuleId
    foreach ($ruleId in $rules.Keys) {
        $g = $grouped | Where-Object { $_.Name -eq $ruleId }
        $cnt = if ($g) { $g.Count } else { 0 }
        $tier = switch ($ruleId) {
            "CA-CLEAN-01" { "Level 1: Safe Auto-fix" }
            "CA-ERR-01"   { "Level 2: Review Required" }
            "CA-ERR-02"   { "Level 2: Review Required" }
            "CA-ERR-03"   { "Level 2: Review Required" }
            "CA-WARN-01"  { "Level 2: Review Required" }
            "CA-WARN-02"  { "Level 2: Review Required" }
            default       { "Level 3: Report Only" }
        }
        $sb.AppendLine("| ``" + $ruleId + "`` | **" + $rules[$ruleId].Severity + "** | " + $rules[$ruleId].Description + " | **" + $cnt + "** | " + $tier + " |") | Out-Null
    }
    [void]$sb.AppendLine("")

    [void]$sb.AppendLine("## Detailed Violations List")
    if ($violations.Count -eq 0) {
        [void]$sb.AppendLine("*No Clean Architecture violations detected in ${TargetDir}!*")
    } else {
        [void]$sb.AppendLine("| Rule ID | Severity | File : Line | Code Snippet | Recommended Action |")
        [void]$sb.AppendLine("| :--- | :--- | :--- | :--- | :--- |")
        foreach ($v in $violations) {
            $action = switch ($v.RuleId) {
                "CA-CLEAN-01" { "Safe Auto-fix (Xóa using/catch rỗng/comment chết)" }
                "CA-ERR-01"   { "Review Required (Chuyển sang tiêm Service/Repository qua IUnitOfWork)" }
                "CA-ERR-02"   { "Review Required (Chuyển LINQ/EF query xuống Service method)" }
                "CA-ERR-03"   { "Review Required (Đóng gói điều kiện kiểm tra vào Service/FluentValidation)" }
                "CA-WARN-01"  { "Review Required (Thu gọn catch hoặc dùng ModelState/Validation)" }
                "CA-WARN-02"  { "Review Required (Chuyển switch/case màu sắc sang DTO hoặc PageModel)" }
                default       { "Report Only (Kiểm tra lại nghiệp vụ)" }
            }
            $rid = $v.RuleId
            $sev = $v.Severity
            $f = $v.File
            $l = $v.Line
            $snippet = $v.CodeSnippet.Replace("|", "\|")
            if ($snippet.Length -gt 60) { $snippet = $snippet.Substring(0, 57) + "..." }
            $sb.AppendLine("| ``" + $rid + "`` | **" + $sev + "** | [" + $f + ":" + $l + "](file:///" + $f + "#L" + $l + ") | `` " + $snippet + " `` | " + $action + " |") | Out-Null
        }
    }

    [System.IO.File]::WriteAllText($OutputPath, $sb.ToString(), [System.Text.Encoding]::UTF8)
    Write-Host "Report exported to Markdown: $OutputPath" -ForegroundColor Green
}

exit 0
