# Script de validación de comandos peligrosos para GitHub Copilot Hooks
# Lee stdin JSON y bloquea comandos que coincidan con patrones peligrosos

$inputJson = [Console]::In.ReadToEnd()
$input = $inputJson | ConvertFrom-Json

$toolName = $input.tool_name
$toolInput = ""

if ($input.tool_input) {
    if ($input.tool_input -is [string]) {
        $toolInput = $input.tool_input
    }
    else {
        $toolInput = $input.tool_input | ConvertTo-Json -Compress
    }
}

# Solo validar herramientas de ejecución de terminal
$terminalTools = @("run_in_terminal", "terminal", "execute", "shell")
$isTerminalTool = $false
foreach ($t in $terminalTools) {
    if ($toolName -match $t) {
        $isTerminalTool = $true
        break
    }
}

if (-not $isTerminalTool) {
    Write-Output "{}"
    exit 0
}

# Patrones peligrosos
$dangerousPatterns = @(
    @{ Pattern = 'rm\s+-rf\s+/';           Label = 'rm -rf /' },
    @{ Pattern = 'DROP\s+(TABLE|DATABASE)'; Label = 'DROP TABLE/DATABASE' },
    @{ Pattern = 'TRUNCATE\s+TABLE';        Label = 'TRUNCATE TABLE' },
    @{ Pattern = 'DELETE\s+FROM\s+(?!.*WHERE)'; Label = 'DELETE FROM sin WHERE' },
    @{ Pattern = '(password|secret|apikey)\s*='; Label = 'credencial expuesta' },
    @{ Pattern = 'curl\s+.*\|\s*bash';      Label = 'curl | bash' },
    @{ Pattern = 'git\s+push\s+--force(?!\s+--force-with-lease)'; Label = 'git push --force sin --force-with-lease' },
    @{ Pattern = 'del\s+/s';                Label = 'del /s' },
    @{ Pattern = 'Format-Volume';            Label = 'Format-Volume' },
    @{ Pattern = 'Remove-Item\s+-Recurse\s+.*\s+-Force'; Label = 'Remove-Item recursivo forzado' }
)

foreach ($dp in $dangerousPatterns) {
    if ($toolInput -match $dp.Pattern) {
        $reason = [char]0x26D4 + " Comando bloqueado por politica de seguridad: $($dp.Label)"
        $response = @{
            hookSpecificOutput = @{
                hookEventName          = "PreToolUse"
                permissionDecision     = "deny"
                permissionDecisionReason = $reason
            }
        }
        $response | ConvertTo-Json -Compress
        exit 0
    }
}

Write-Output "{}"
