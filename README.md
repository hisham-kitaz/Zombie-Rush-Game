# Zombie-Rush-Game
#  Project Overview


This project is a game inspired by Candy Crush Saga, designed as part of the COMP 376 Fall 2024 Assignment 2. The game consists of two levels, each with unique mechanics related to tile matching and cascading effects. The goal is to clear the board by matching colored tiles in rows or columns of three or more. Points are awarded based on matches, and additional points can be earned through cascading chain reactions.

Game Characteristics
Main Behaviors
Tile Matching Mechanics:

Players swap adjacent candies. If the swap results in three or more candies of the same color in a row or column, the candies disappear, and points are awarded.
Matching occurs both horizontally and vertically, and the player receives points for every valid match.
Cascading Mechanic:

After candies disappear due to a match, new candies fall from above to fill the empty spaces.
This can result in additional matches without player input, which is referred to as the cascading mechanic.
Level-Specific Probability Distributions:

Level 1: Tile colors are generated based on simple probability rules:
The first tile has a random color, and subsequent tiles have a higher probability of matching the color of the tile below them (60%).
Level 2: Color generation is influenced by neighboring tiles. The more neighboring tiles of the same color, the higher the probability of generating a tile with that color. This behavior simulates a non-uniform probability distribution for matching tiles.
End-Game Conditions:

The game ends when the player completes the matching goal within the given time and move limits.
The player's performance is evaluated based on their score, and they are awarded between 1 to 3 stars depending on how many points they’ve earned.
If the player fails to reach the goal, they can either retry the level or return to the main menu.
Scoring System:

Players are rewarded points for each match they create, with additional points given for cascading matches. The multiplier increases with consecutive chain reactions, allowing for higher scores as the player successfully creates chain reactions.
Special Features
Pause and Resume:

The game can be paused at any time, freezing both the gameplay and the timer. Players can resume the game from where they left off or return to the main menu.
Dynamic Color Probability:

In both levels, dynamic color generation is implemented with varying probabilities based on the surrounding tiles (in Level 2), adding complexity to the matching mechanics and enhancing the challenge for the player.
Star Reward System:

A star system evaluates player performance based on their score. This system provides feedback to the player, motivating them to replay levels for higher rewards.
