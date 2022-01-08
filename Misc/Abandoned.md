* Set Gear - store classifications:
  * Abandoned because logging cross validations seems sufficient for model evaluation. Was there a different point to this?
  * Cannot store Strava data per agreement. 
  * Currently making a request on every activity event for historical data. 
  * Could this cause issues when all  historical data is affected by equipper?
  * Can use caching up to 7 days to reduce request rates
* Model initialization
  * Abandoned because current models do not need to persist data and are cheap to initialize
  * Will need to gather data to build model for new subscribers. 
  * Maybe expensive? May want to protect against unsubscribe/resubscribe cycles.
  * Consider strava api use terms.
* Set Gear - don't change already-processed activities
  * Abandoned because ignoring 'update' events solves this problem.
  * Cannot store activity IDs longer than 7 days
  * Consider cosmos table with TTL of 7 days.