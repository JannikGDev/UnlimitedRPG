# Planning
The project is split into "milestones". 
The milestone are a list of "items" that must be fulfilled for that milestone to be reached.
An item is a requirement in the form of acceptance criteria or a user story.
An item can have these states:
- Open: Not working on, not implemented yet
- In Progress: Partly implemented
- Done: The requirement is fulfilled (must be approved by the user)

# General Project Description
The UnlimitedRPG Project aims to provide a webapp 
that users can visit to play pen-and-paper like sessions on their own.
Using LLMs and ImageGeneration to provide Content dynamically as the player explores 
the world and experiences adventures. 
The webapp also allows people to create their own worlds for other players to play in.

# Definitions
User = A human person using the webapp over the browser
Character = A fictional person that can be played as
World = A fictional setting
Session = A running instance of game of a player with a character in a world

# Personas
The following personas are stand-ins for different roles of users:
- RPGPlayer: Someone who wants to play the game, creates characters, creates sessions and plays in them
- WorldBuilder: Someone who wants to design an rpg experience, creates worlds and their content

# Milestone 1 - MVP
[Done][R1]RPGPlayer can open the website without an error.

[Done][R2]RPGPlayer can create and save a character by giving a name and a short free-text description.

[Done][R3]RPGPlayer can see all created characters in a list and open them to see the details

[Done][R4]RPGPlayer can can edit the characters after saving them.

[Open][R5]RPGPlayer can start a session with one of their characters and is taken to the session page.

[Open][R6]The session page shows a chat-style message history and an input area with a mode selector ("Say something" / "Do something").

[Open][R7]RPGPlayer can type a message, select a mode, and send it — the message appears in the chat history with its mode indicated.

[Open][R8]After the player sends a message, the game generates a text response and displays it in the chat.