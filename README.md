# CronoCord
Discord bot for managing schedules

## Features
### Schedules
- Users can create availaiblity slots for when they are available
- Users can edit and delete existing availability slots they own
- Users can genereate a schedule that shows the availability of who ever they specify


## Setup
Before starting the bot, make sure to go to system environment variables, and add a new user variable with the name CRONOCORD_BOT_TOKEN and value is your bot token.
Also make sure to sync your computers clock.
Steps on windows:
1. open cmd in admin mode
2. make sure windows time service is running with this command: net start w32time
3. check if the time is synced with this command: w32tm /query /status
4. if not synced then use this to resync: w32tm /resync
