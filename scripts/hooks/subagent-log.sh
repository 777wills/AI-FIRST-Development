#!/usr/bin/env bash
# Script de logging de sub-agentes para GitHub Copilot Hooks
# Lee stdin JSON y retorna systemMessage con nombre y timestamp del sub-agente

set -euo pipefail

INPUT=$(cat)
TIMESTAMP=$(date +"%H:%M:%S")
AGENT_TYPE=$(echo "$INPUT" | python3 -c "import sys,json; print(json.load(sys.stdin).get('agent_type','unknown'))" 2>/dev/null || echo "unknown")
EVENT_NAME=$(echo "$INPUT" | python3 -c "import sys,json; print(json.load(sys.stdin).get('hookEventName','SubagentStart'))" 2>/dev/null || echo "SubagentStart")

if [ "$EVENT_NAME" = "SubagentStart" ]; then
    echo "{\"hookSpecificOutput\":{\"hookEventName\":\"SubagentStart\",\"additionalContext\":\"\"},\"systemMessage\":\"🚀 [$TIMESTAMP] Sub-agente iniciado: $AGENT_TYPE\"}"
elif [ "$EVENT_NAME" = "SubagentStop" ]; then
    echo "{\"systemMessage\":\"✅ [$TIMESTAMP] Sub-agente finalizado: $AGENT_TYPE\"}"
else
    echo "{}"
fi
