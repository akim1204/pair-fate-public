# Pair Fate

Pair Fate is a multiplayer co-op puzzle game built using Godot 4 and C#. The project focuses on real-time player synchronization, interactive puzzle mechanics, and responsive multiplayer gameplay.

## Overview

This project was designed as a networked multiplayer game where players must cooperate to solve environment-based challenges. The system emphasizes smooth state synchronization between players while maintaining responsive movement and interaction.

## Presentation

Project presentation slides:

https://docs.google.com/presentation/d/1tCCCkmR-M7ka1J3-UTmy-YjkM3RYJhBmPO6waozwzKY

## Features

- Real-time multiplayer gameplay  
- Cooperative puzzle-solving mechanics  
- Player movement and interaction synchronization  
- Event-driven game state updates  
- Scene-based level architecture in Godot 4  

## Tech Stack

- **Engine:** Godot 4  
- **Language:** C#  
- **Networking:** Godot multiplayer networking framework  
- **Architecture:** Event-driven game object synchronization  

## Technical Highlights

- Implemented multiplayer synchronization for player state and object interaction  
- Designed game logic to support concurrent player actions across shared scenes  
- Reduced local multiplayer latency through efficient update handling  
- Structured gameplay systems using modular scene composition in Godot  

## Project Structure

- `Scripts/` — gameplay logic and networking code  
- `Scenes/` — game scenes and level definitions  
- `Assets/` — sprites, audio, and visual assets  

## Challenges Solved

One of the main technical challenges was maintaining consistent multiplayer behavior while multiple players interacted with shared game objects. To address this, state updates were structured to minimize desynchronization and preserve responsiveness.

## Future Improvements

- Dedicated lobby / matchmaking support  
- Improved network reconciliation  
- Additional puzzle mechanics and level expansion  

## Running the Project

1. Open the project in Godot 4  
2. Load the main scene  
3. Run multiplayer instances for testing  

## Author

Alexander Kim