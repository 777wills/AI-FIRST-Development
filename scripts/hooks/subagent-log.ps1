# Script de logging de sub-agentes para GitHub Copilot Hooks
# Lee stdin JSON y retorna systemMessage con nombre y timestamp del sub-agente

$inputJson = [Console]::In.ReadToEnd()
$input = $inputJson | ConvertFrom-Json

$timestamp = Get-Date -Format "HH:mm:ss"
$agentType = $input.agent_type

$eventName = $input.hookEventName
if (-not $eventName) {
    $eventName = "SubagentStart"
}

if ($eventName -eq "SubagentStart") {
    $response = @{
        hookSpecificOutput = @{
            hookEventName     = "SubagentStart"
            additionalContext = ""
        }
        systemMessage       = [char]0x1F680 + " [$timestamp] Sub-agente iniciado: $agentType"
    }
}
elseif ($eventName -eq "SubagentStop") {
    $response = @{
        systemMessage = [char]0x2705 + " [$timestamp] Sub-agente finalizado: $agentType"
    }
}
else {
    $response = @{}
}

$response | ConvertTo-Json -Compress
