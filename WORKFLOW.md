# Feature Implementation Workflow
This file defines how to work in this project.

The project is split into components. 
Each component has its own subfolder (Api, Core, Database etc.) and 
is either its own C# Project or a different technology (Frontend)

# General rules
Use an iterative approach. Start each feature small and gather feedback from the user before you continue.

1) (Can be skipped if the user chooses the item for you) Choose a requirement item (defined in PLANNING.md) that fits to this change. Every change should belong to one such item. If there is no fitting one, ask the user to create one that fits (give a suggestion)
2) Plan the implementation of the next iteration. Identify which components need to be changed. Show the plan in a high-level description to the user for approval before continueing.
3) After the iteration, the project should compile and be runnable and the change should be visible/testable. If successful, make a commit.
4) Gather feedback from the user (they will try it out). Either the requirement is fulfilled or go back to step 2 for the next iteration.
5) Review the change and ask the user if the requirement is fulfilled and update the status in PLANNING.md accordingly.