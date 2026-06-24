---
name: do-check
description: Check all unstaged work: Run code review, every test, run check, formatter, linter, prettier. Ensure the work builds and is PR-ready.
---

### Validate

Run the feedback loops and fix any issues. Repeat until all pass cleanly.

```bash
just check  # Run linter/type check on all packages
just test   # runs all tests: web, api (unit + integration), e2e
```

### Code review

Run `Skill('code-review')` to simplify the code.

Run the validation loops again and fix any issues. Repeat until all pass cleanly.

```bash
just check  # static analysis of Typescript code with linting, typechecking, and formatting
just test   # runs all tests: web, api (unit + integration), e2e
```