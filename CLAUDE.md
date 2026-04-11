# General Style
Be direct, be honest, Don't try to be polite or nice
If a user suggestion is wrong or a bad choice, give direct feedback that is it bad
Ask for the intention of certain steps, if it is hard to judge
If multiple possible ways to implement something are possible, ask which one to take while listing their pros/cons

# Git Policy
Work on main, don't use PRs or create new branches.
If working on main is not possible because of the user, ask him to change it.
Commit whenever a working (as in it compiles and starts without errors) state is and the users has accepted the work
even when the feature is not done: Commit messages should be short, max 10 words.
Never change the git history (by using --force or --hard), if it is necessary, ask the user to do it

# Verifying compilation
When code changes were done, use "dotnet build" to build the whole solution.
There should be no errors, everything should build. If there are errors, try to fix them.
If fixing the errors does not work, tell the user there are errors and ask for help. 
Give them as much info as possible for the error and your suggestion for fixes, if you have any

# Smoke testing
Run smoke tests after each change that builds successfully. 
If the tests fail (when they passed before), try to fix them or ask for help.

# Documentation
Read the documentation files before making changes, their names are:
WORKFLOW.md: Includes the definition of the process making changes to the code
TECHNICAL.md: Technical implementation details, decisions and rules
PLANNING.md: This document details the (non-technical) requirements, milestones etc. of the project.
Changes to the documentation always requires user approval

# Scope discipline
Implement exactly what was asked. No extra features, no cleanup of unrelated code, no unsolicited refactoring.
