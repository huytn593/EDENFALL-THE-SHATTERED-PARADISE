<#
.SYNOPSIS
    EDENFALL: Phase 3.6.2 Master Art Restoration & Super-Resolution Pipeline.
.DESCRIPTION
    Autonomous Windows-first PowerShell orchestrator for Aurelia character art:
    - Step 1: Inventory & source discovery (source_manifest.json)
    - Step 2: Panel & character extraction (extraction_manifest.json)
    - Step 3: AI background removal & matting via BRIA RMBG 2.0
    - Step 4: Real-ESRGAN Anime 4x Super-Resolution via NCNN Vulkan (GTX 1650 4GB)
    - Step 5: Visual QA scoring, contact sheet & promotion (approved_asset_manifest.json, visual_qa_report.md)
.PARAMETER Step
    Which step to run: All, Inventory, Extract, Matting, Upscale, Score
.PARAMETER Force
    Force re-processing of already cached assets
#>

[CmdletBinding()]
param (
    [ValidateSet("All", "Inventory", "Extract", "Matting", "Upscale", "Score")]
    [string]$Step = "All",
    [switch]$Force
)

$ErrorActionPreference = "Stop"
$ScriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$ProjectRoot = Split-Path -Parent $ScriptDir

Write-Host "=================================================================" -ForegroundColor Cyan
Write-Host "  EDENFALL: THE SHATTERED PARADISE - AI ART RECOVERY PIPELINE    " -ForegroundColor Cyan
Write-Host "  Version: 3.6.2 (Phase 3.6.2 Production Asset Pipeline)         " -ForegroundColor Cyan
Write-Host "=================================================================" -ForegroundColor Cyan

# 1. Environment & Dependency Validation
Write-Host "[1/6] Validating Environment & Dependencies..." -ForegroundColor Yellow
$PythonCmd = Get-Command python -ErrorAction SilentlyContinue
if (-not $PythonCmd) {
    Write-Error "Python 3 is required but not found in PATH."
}
Write-Host "  Python: $($PythonCmd.Source)" -ForegroundColor Green

$ToolsDir = Join-Path $ProjectRoot "tools\realesrgan"
$RealESRGAN = Join-Path $ToolsDir "realesrgan-ncnn-vulkan.exe"
if (-not (Test-Path $RealESRGAN)) {
    Write-Host "  Real-ESRGAN binary missing, downloading..." -ForegroundColor Yellow
    # Trigger step 1 or tool setup
} else {
    Write-Host "  Real-ESRGAN: $RealESRGAN" -ForegroundColor Green
}

# 2. Step 1: Inventory Sources
if ($Step -eq "All" -or $Step -eq "Inventory") {
    Write-Host "[2/6] Running Step 1: Inventory Sources..." -ForegroundColor Yellow
    & python "$ScriptDir\art_pipeline\step1_inventory_sources.py"
    if ($LASTEXITCODE -ne 0) { throw "Step 1 failed with exit code $LASTEXITCODE" }
}

# 3. Step 2: Panel Extraction
if ($Step -eq "All" -or $Step -eq "Extract") {
    Write-Host "[3/6] Running Step 2: Panel & Character Extraction..." -ForegroundColor Yellow
    & python "$ScriptDir\art_pipeline\step2_extract_panels.py"
    if ($LASTEXITCODE -ne 0) { throw "Step 2 failed with exit code $LASTEXITCODE" }
}

# 4. Step 3: AI Matting & Restoration
if ($Step -eq "All" -or $Step -eq "Matting") {
    Write-Host "[4/6] Running Step 3: AI Background Removal (BRIA RMBG 2.0)..." -ForegroundColor Yellow
    & python "$ScriptDir\art_pipeline\step3_ai_matting_restore.py"
    if ($LASTEXITCODE -ne 0) { throw "Step 3 failed with exit code $LASTEXITCODE" }
}

# 5. Step 4: Real-ESRGAN Anime 4x Super-Resolution
if ($Step -eq "All" -or $Step -eq "Upscale") {
    Write-Host "[5/6] Running Step 4: Real-ESRGAN Anime 4x Super-Resolution..." -ForegroundColor Yellow
    & python "$ScriptDir\art_pipeline\step4_super_resolution.py"
    if ($LASTEXITCODE -ne 0) { throw "Step 4 failed with exit code $LASTEXITCODE" }
}

# 6. Step 5: Asset Quality Scoring & Promotion
if ($Step -eq "All" -or $Step -eq "Score") {
    Write-Host "[6/6] Running Step 5: Visual QA Scoring, Contact Sheet & Asset Promotion..." -ForegroundColor Yellow
    & python "$ScriptDir\art_pipeline\step5_quality_scoring_contact_sheet.py"
    if ($LASTEXITCODE -ne 0) { throw "Step 5 failed with exit code $LASTEXITCODE" }
}

Write-Host "=================================================================" -ForegroundColor Green
Write-Host "  PIPELINE EXECUTION COMPLETED SUCCESSFULLY!                     " -ForegroundColor Green
Write-Host "  Manifests:                                                     " -ForegroundColor Green
Write-Host "   - Assets/_Edenfall/Art/Characters/Aurelia/source_manifest.json" -ForegroundColor Green
Write-Host "   - Assets/_Edenfall/Art/Characters/Aurelia/extraction_manifest.json" -ForegroundColor Green
Write-Host "   - Assets/_Edenfall/Art/Characters/Aurelia/enhancement_manifest.json" -ForegroundColor Green
Write-Host "   - Assets/_Edenfall/Art/Characters/Aurelia/approved_asset_manifest.json" -ForegroundColor Green
Write-Host "   - Assets/_Edenfall/Art/Characters/Aurelia/visual_qa_report.md" -ForegroundColor Green
Write-Host "=================================================================" -ForegroundColor Green
