# Albion LootBot

Albion LootBot is a custom Discord bot built with **C# (.NET 8)** and **Discord.NET** to automate, log, and simplify lootsplit session management for Albion Online player groups.

---

## Overview & Purpose

### What is it?
Albion LootBot streamlines the process of managing group loot splits after content runs (such as ganking, fame farming, static dungeons, or zvz). It tracks participants, logs silver/item values, applies guild tax rates automatically, and outputs clear payout breakdowns.

### Why was it made?
Manual lootsplit calculations in spreadsheets are tedious, error-prone, and prone to miscommunication. LootBot brings the entire workflow directly into Discord slash commands, handling the math, tax calculations, and remainder distribution instantly and transparently.

### Who is it for?
* **Guild:** BlackDisciples
* **Alliance:** Congregation of Fools
* Designed specifically for officers, group leaders, and guild members operating within the alliance to handle party logistics seamlessly.

---

## Command Reference

All commands use Discord native Slash Commands (`/`).

### Session Management

| Command | Arguments | Description |
| :--- | :--- | :--- |
| `/create-split` | `splitName` *(Required)*<br>`participantsInput` *(Optional)* | Creates a new lootsplit. Automatically adds the creator and any space-separated `@mentions` passed into `participantsInput`. |
| `/delete-split` | `splitName` *(Required)* | Deletes a split session permanently. *(Note: Statistical tracking impact warning)* |
| `/view-active-splits` | *None* | Lists all active/open split sessions where you are registered as a participant. |
| `/view-all-active-splits` | *None* | **Admin Command:** Displays all active split sessions currently open across the server. |
| `/get-current-split` | `splitName` *(Required)* | Displays the current running total silver value stored in the session. |

### Participant Management

| Command | Arguments | Description |
| :--- | :--- | :--- |
| `/add-split-participant` | `newParticipant` *(Required)*<br>`splitName` *(Required)* | Adds a user (`@mention`) to an existing active split session. |
| `/remove-split-participant` | `userToBeRemoved` *(Required)*<br>`splitName` *(Required)* | Removes a user (`@mention`) from an active split session. |

### Loot Logging & Payouts

| Command | Arguments | Description |
| :--- | :--- | :--- |
| `/add-item-value` | `sessionName` *(Required)*<br>`entityType` *(Required)*<br>`itemValue` *(Required)* | Logs a silver or item value entry toward the split session pool. |
| `/calculate-split` | `splitName` *(Required)* | Finalizes the session, applies guild tax, calculates per-person payouts, closes the split, and posts a summary embed. |

---

## Alpha Testing Guide

1. **Create the Split:**  
   `/create-split splitName: "Roam-Static-01" participantsInput: "@Player1 @Player2"`
2. **Log Loot as You Go:**  
   `/add-item-value sessionName: "Roam-Static-01" entityType: Silver itemValue: 2500000`
3. **Verify Pool Total:**  
   `/get-current-split splitName: "Roam-Static-01"`
4. **Finalize & Distribute:**  
   `/calculate-split splitName: "Roam-Static-01"`
