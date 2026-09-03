#!/usr/bin/env bash
# Script de validación de comandos peligrosos para GitHub Copilot Hooks
# Lee stdin JSON y bloquea comandos que coincidan con patrones peligrosos

set -euo pipefail

INPUT=$(cat)
TOOL_NAME=$(echo "$INPUT" | python3 -c "import sys,json; print(json.load(sys.stdin).get('tool_name',''))" 2>/dev/null || echo "")
TOOL_INPUT=$(echo "$INPUT" | python3 -c "import sys,json; d=json.load(sys.stdin).get('tool_input',''); print(d if isinstance(d,str) else json.dumps(d))" 2>/dev/null || echo "")

# Solo validar herramientas de ejecución de terminal
case "$TOOL_NAME" in
    *terminal*|*execute*|*shell*|*run_in_terminal*)
        ;;
    *)
        echo "{}"
        exit 0
        ;;
esac

# Patrones peligrosos
check_pattern() {
    local pattern="$1"
    local label="$2"
    if echo "$TOOL_INPUT" | grep -qiE "$pattern"; then
        echo "{\"hookSpecificOutput\":{\"hookEventName\":\"PreToolUse\",\"permissionDecision\":\"deny\",\"permissionDecisionReason\":\"⛔ Comando bloqueado por política de seguridad: $label\"}}"
        exit 0
    fi
}

check_pattern 'rm\s+-rf\s+/' 'rm -rf /'
check_pattern 'DROP\s+(TABLE|DATABASE)' 'DROP TABLE/DATABASE'
check_pattern 'TRUNCATE\s+TABLE' 'TRUNCATE TABLE'
check_pattern '(password|secret|apikey)\s*=' 'credencial expuesta'
check_pattern 'curl\s+.*\|\s*bash' 'curl | bash'
check_pattern 'del\s+/s' 'del /s'
check_pattern 'Format-Volume' 'Format-Volume'

echo "{}"
