<#
.SYNOPSIS
  Deploys Solicitor Lead Finder to a free (F1) Azure App Service as a single artifact
  (the API serving the bundled Angular SPA).

.DESCRIPTION
  Run `az login` first. The script creates a resource group, a free Linux App Service plan
  and a web app, publishes the app (which builds the Angular SPA into wwwroot), and zip-deploys
  it. The hosted app runs in "fixture" scraper mode so the demo always shows data.

.EXAMPLE
  az login
  ./deploy/azure-deploy.ps1 -AppName my-solicitor-demo
#>
param(
  [string]$AppName = "infotrack-solicitor-finder-$(Get-Random -Maximum 99999)",
  [string]$ResourceGroup = "rg-solicitor-finder",
  [string]$Location = "uksouth",
  [string]$Plan = "plan-solicitor-finder"
)

$ErrorActionPreference = "Stop"
$root = Split-Path $PSScriptRoot -Parent
Set-Location $root

Write-Host "Creating Azure resources (free F1 tier)..." -ForegroundColor Cyan
az group create -n $ResourceGroup -l $Location | Out-Null
az appservice plan create -g $ResourceGroup -n $Plan --sku F1 --is-linux | Out-Null
az webapp create -g $ResourceGroup -p $Plan -n $AppName --runtime "DOTNETCORE:10.0" | Out-Null

Write-Host "Configuring app settings..." -ForegroundColor Cyan
az webapp config appsettings set -g $ResourceGroup -n $AppName --settings `
  ASPNETCORE_ENVIRONMENT=Production Scraper__Mode=fixture | Out-Null

Write-Host "Publishing (builds the SPA into wwwroot) and packaging..." -ForegroundColor Cyan
if (Test-Path publish) { Remove-Item -Recurse -Force publish }
if (Test-Path app.zip) { Remove-Item -Force app.zip }
dotnet publish src/InfoTrack.SolicitorFinder.Api -c Release -o publish
Compress-Archive -Path publish/* -DestinationPath app.zip -Force

Write-Host "Deploying..." -ForegroundColor Cyan
az webapp deploy -g $ResourceGroup -n $AppName --src-path app.zip --type zip | Out-Null

Write-Host "Done. Live at: https://$AppName.azurewebsites.net" -ForegroundColor Green
