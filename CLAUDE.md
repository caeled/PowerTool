# PowellTec IT Support Tools — Claude Instructions

## Git workflow
After every commit, always:
1. `git push origin master`
2. `gh run list --repo caeled/PowerTool --limit=3` to find the relevant run
3. Post the direct link: `https://github.com/caeled/PowerTool/actions/runs/<id>`

Never ask for permission to push. Never stop at the commit and wait.

## Consistency
Steve has ADHD/spectrum — consistency across sessions is critical. If a behavior changes after context compaction, it is a regression. When Steve flags it, add it here and to memory so it doesn't happen again.
