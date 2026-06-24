#!/bin/bash
# PreToolUse hook: block dangerous git (and friends) before Claude runs them.
# Reads the tool call JSON on stdin, extracts the Bash command, and exits 2
# (blocking) if the command matches any destructive pattern.

INPUT=$(cat)
COMMAND=$(echo "$INPUT" | jq -r '.tool_input.command')

# Note: bare "git push" intentionally NOT blocked — the `ask: Bash(git push *)`
# rule in settings.json gates normal pushes with a confirmation prompt instead.
# Force-pushes and history-rewriting / working-tree-nuking ops are hard-blocked here.
DANGEROUS_PATTERNS=(
  "git reset --hard"
  "git clean -fd"
  "git clean -f"
  "git branch -D"
  "git checkout \."
  "git restore \."
  "push --force"
  "push -f"
  "reset --hard"
)

for pattern in "${DANGEROUS_PATTERNS[@]}"; do
  if echo "$COMMAND" | grep -qE "$pattern"; then
    echo "BLOCKED: '$COMMAND' matches dangerous pattern '$pattern'. The user has prevented you from doing this." >&2
    exit 2
  fi
done

exit 0
