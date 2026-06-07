# LLM Project Creator - Prompt Template

System (instructions for the LLM)
You are a project-creation assistant for .NET projects. Your task is to interactively gather required inputs, validate them against the environment, query NuGet for latest stable package versions, and produce a final, executable script or list of dotnet CLI commands to create a solution and project with the requested packages.

Required project capabilities
- Dependency Injection (use Microsoft.Extensions.DependencyInjection / Host DI)
- Configuration management: support User Secrets, appsettings.json, and environment variables (bind to IConfiguration / Options pattern)
- Logging: integrate Microsoft.Extensions.Logging and a console logging provider (or other providers as requested)

Conversation flow (expected)
1. Ask: solution name (validate non-empty, safe filesystem characters)
2. Ask: project name (validate, suggest <Solution>.Runner default)
3. Ask: project type (console/webapi/classlib) with default "console"
4. Probe for desired TFM or detect installed SDKs (run: dotnet --list-sdks) and recommend a TFM (e.g., net10.0)
5. Ask: list of NuGet packages (comma separated or select from recommended list)
6. For each package query NuGet V3 Registrations API and pick the latest stable version (ignore prerelease)
7. Confirm plan with user; if confirmed, return the commands and an optional PowerShell script to execute

Validation rules
- TFM must be compatible with highest installed SDK major version. If requested TFM is newer than installed SDK, warn and provide fallback.
- Package versions: prefer exact stable version string. If NuGet lookup fails, return command without --version so dotnet adds latest available.

NuGet lookup (example algorithm)
- GET https://api.nuget.org/v3/index.json -> find resource with @type 'RegistrationsBaseUrl' (or 'RegistrationsBaseUrl/Versioned')
- GET {registrationsBase}{packageId}/index.json
- Extract catalogEntry.version values, filter out any versions with '-' (prerelease), sort by System.Version descending, pick first

Output format (machine friendly)
Return a JSON object wrapped in a markdown code block labelled json. Example schema:
{
  "solutionName": "MySolution",
  "projectName": "MySolution.Runner",
  "projectType": "console",
  "tfm": "net10.0",
  "packages": [
	{ "id": "Microsoft.Extensions.Hosting", "version": "10.0.8" },
	{ "id": "Microsoft.Extensions.Configuration.Json", "version": "10.0.8" }
  ],
  "commands": ["dotnet new sln -n MySolution", "dotnet new console -n MySolution.Runner -f net10.0", "dotnet sln MySolution.sln add MySolution.Runner/MySolution.Runner.csproj", "dotnet add MySolution.Runner/MySolution.Runner.csproj package Microsoft.Extensions.Hosting --version 10.0.8"]
}

Assistant behaviour rules
- Always ask one question at a time and wait for user input unless the user provided all inputs upfront.
- When performing environment checks (dotnet --list-sdks) or NuGet queries, explain the command or request you would run; do not actually run commands inside the LLM output.
- Provide both a ready-to-run PowerShell script and the equivalent sequence of dotnet CLI commands.
- Keep responses concise and strictly actionable.

Recommended packages mapping (preselected for Runner-like project)
- Microsoft.Extensions.Hosting
- Microsoft.Extensions.Configuration.Json
- Microsoft.Extensions.DependencyInjection
- Microsoft.Extensions.Logging
- Microsoft.Extensions.Options
- OpenAI (optional)
- OllamaSharp (optional)

Additional recommended packages for full feature support
- Microsoft.Extensions.Configuration.UserSecrets (for secrets during development)
- Microsoft.Extensions.Logging.Console (simple console logger provider)
- Microsoft.Extensions.Configuration.EnvironmentVariables (usually included via Host.CreateDefaultBuilder but can be added explicitly)

Example assistant prompt (first message to user)
- "I'll create a .NET solution + project similar to ExtensionsAI.Runner. What do you want to name the solution?"

Example final output (trimmed)
```json
{
  "solutionName": "ExtensionsAI.Sample",
  "projectName": "ExtensionsAI.Sample.Runner",
  "projectType": "console",
  "tfm": "net10.0",
  "packages": [
	{ "id": "Microsoft.Extensions.Hosting", "version": "10.0.8" },
	{ "id": "Microsoft.Extensions.Configuration.Json", "version": "10.0.8" }
  ],
  "commands": [
	"dotnet new sln -n ExtensionsAI.Sample",
	"dotnet new console -n ExtensionsAI.Sample.Runner -f net10.0",
	"dotnet sln ExtensionsAI.Sample.sln add ExtensionsAI.Sample.Runner/ExtensionsAI.Sample.Runner.csproj",
	"dotnet add ExtensionsAI.Sample.Runner/ExtensionsAI.Sample.Runner.csproj package Microsoft.Extensions.Hosting --version 10.0.8"
  ]
}
```

Notes for implementers
- Implement a thin wrapper that runs dotnet --list-sdks and the NuGet queries from the host environment (PowerShell or small service) rather than asking the LLM to fetch those directly.
- For CI reproducibility, prefer pinning exact versions in returned commands.

Embedded automation script

The project-creator includes a self-contained PowerShell automation script. Instead of a separate file, the script is embedded below so you can copy it into your preferred location or run it directly from the markdown. It performs the full automated creation and scaffold workflow (create solution/project, add packages with pinned versions, initialize user-secrets, scaffold Program/App/MyOptions, build and optionally run).

```powershell
param(
	[string]$SolutionName = 'TestApplication',
	[string]$ProjectName = '',
	[ValidateSet('console','webapi','classlib')][string]$ProjectType = 'console',
	[string]$Tfm = 'auto',
	[switch]$RunApp = $true
)

function Get-LatestNuGetVersion {
	param([string]$PackageId)

	$indexUrl = 'https://api.nuget.org/v3/index.json'
	try {
		$index = Invoke-RestMethod -Uri $indexUrl -ErrorAction Stop
		$reg = ($index.resources | Where-Object { $_."@type" -like 'RegistrationsBaseUrl*' })."@id"
		if (-not $reg) { return $null }
		$regUrl = "$reg$PackageId/index.json"
		$info = Invoke-RestMethod -Uri $regUrl -ErrorAction Stop
		$versions = @()
		foreach ($page in $info.items) {
			if ($page.items) {
				foreach ($item in $page.items) { $versions += $item.catalogEntry.version }
			} elseif ($page.items -and ($page.items -is [System.Array])) {
				foreach ($sub in $page.items) { $versions += $sub.catalogEntry.version }
			}
		}
		$stable = $versions | Where-Object { $_ -and ($_ -notmatch '-') } | Sort-Object {[version]$_} -Descending | Select-Object -First 1
		return $stable
	} catch {
		return $null
	}
}

function Detect-TFM {
	try {
		$sdks = dotnet --list-sdks 2>$null
		if (-not $sdks) { return 'net10.0' }
		$majors = $sdks | ForEach-Object {
			if ($_ -match '^([0-9]+)\.') { [int]$matches[1] } else { $null }
		} | Where-Object { $_ -ne $null } | Sort-Object -Descending
		if ($majors -and $majors[0] -ge 6) { return "net$($majors[0]).0" }
		return 'net10.0'
	} catch {
		return 'net10.0'
	}
}

if (-not $ProjectName -or $ProjectName -eq '') { $ProjectName = "${SolutionName}.Runner" }

Write-Host "Creating solution '$SolutionName' and project '$ProjectName' (type: $ProjectType)" -ForegroundColor Cyan

if ($Tfm -eq 'auto') { $tfm = Detect-TFM } else { $tfm = $Tfm }
Write-Host "Using target framework: $tfm"

$minimal = @(
	'Microsoft.Extensions.Hosting',
	'Microsoft.Extensions.Configuration.Json',
	'Microsoft.Extensions.Configuration.UserSecrets',
	'Microsoft.Extensions.Logging.Console',
	'Microsoft.Extensions.Options',
	'Microsoft.Extensions.Options.ConfigurationExtensions',
	'Microsoft.Extensions.Configuration.Binder'
)

$cwd = Get-Location
$solutionFile = "$SolutionName.sln"

if (-not (Test-Path $solutionFile)) {
	Write-Host "Creating solution: $solutionFile" -ForegroundColor Green
	dotnet new sln -n $SolutionName | Out-Null
} else {
	Write-Host "Solution $solutionFile already exists - will add project to it" -ForegroundColor Yellow
}

Write-Host "Creating project: $ProjectName ($ProjectType) -f $tfm" -ForegroundColor Green
dotnet new $ProjectType -n $ProjectName -f $tfm | Out-Null

Write-Host "Adding project to solution $solutionFile" -ForegroundColor Green
dotnet sln $solutionFile add "$ProjectName/$ProjectName.csproj" | Out-Null

foreach ($pkg in $minimal) {
	Write-Host "Resolving latest stable version for $pkg..." -ForegroundColor Yellow
	$ver = Get-LatestNuGetVersion -PackageId $pkg
	if ($ver) {
		Write-Host "Found $pkg $ver - installing..." -ForegroundColor Green
		dotnet add "$ProjectName/$ProjectName.csproj" package $pkg --version $ver | Out-Null
	} else {
		Write-Host "Could not determine version for $pkg - installing latest available" -ForegroundColor Yellow
		dotnet add "$ProjectName/$ProjectName.csproj" package $pkg | Out-Null
	}
}

Write-Host 'Initializing user-secrets for development (dotnet user-secrets init)...' -ForegroundColor Green
dotnet user-secrets init --project "$ProjectName/$ProjectName.csproj" | Out-Null

# Scaffold appsettings.json
$projPath = Join-Path $cwd $ProjectName
$appsettingsPath = Join-Path $projPath 'appsettings.json'
$appsettings = @'
{
  "MyOptions": {
	"ApiKey": "replace-me",
	"Timeout": 30
  }
}
'@
Set-Content -Path $appsettingsPath -Value $appsettings -Encoding UTF8

# Overwrite Program.cs with Host setup, DI, config, options and logging
$programPath = Join-Path $projPath 'Program.cs'
$namespace = ($ProjectName -replace '[^A-Za-z0-9_.]','')
$program = @"
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace $namespace
{
	public class Program
	{
		public static async Task Main(string[] args)
		{
			using IHost host = Host.CreateDefaultBuilder(args)
				.ConfigureAppConfiguration((context, config) =>
				{
					// use the app's base directory so appsettings.json is resolved whether
					// running from project folder or solution root when using dotnet run --project
					config.SetBasePath(System.AppContext.BaseDirectory);
					config.AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);
					config.AddEnvironmentVariables();
					// user secrets are optional in non-development environments
					config.AddUserSecrets<Program>(optional: true);
				})
				.ConfigureServices((context, services) =>
				{
					services.Configure<MyOptions>(context.Configuration.GetSection("MyOptions"));
					services.AddSingleton<App>();
				})
				.ConfigureLogging(logging =>
				{
					logging.ClearProviders();
					logging.AddConsole();
				})
				.Build();

			var app = host.Services.GetRequiredService<App>();
			await app.RunAsync();
		}
	}
}
"@
Set-Content -Path $programPath -Value $program -Encoding UTF8

# Add App.cs
$appPath = Join-Path $projPath 'App.cs'
$app = @"
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

We created content. Now need to remove old file. Use functions.remove_file to delete tools/llm-project-creator-template.md. Then respond confirming rename. Let's call remove_file. Need relative path. Use that path. Then final message: short and impersonal; mention GitHub Copilot as name? Developer instruction: when asked for name, respond GitHub Copilot. But not asked. Keep concise. Also must stay within software dev topics. Provide action completed. Use apply_patch? We will call remove_file. Proceed.eneration.ادث. Let's call remove_file. Then final assistant message: 