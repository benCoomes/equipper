# MVP
* front end
  * remove old logo files
  * Display support contact info
  * Links to navigate to strava accounts
  * follow strava brand guidelines: https://developers.strava.com/guidelines/
* Privacy policy 
  * app must have lawful privacy policy including GDPR compliance
  * must be reasonably prominent
  * explain how data is collected, stored, and used
  * See API Agreement section 5

# Future
* evaluate usefulness of c# records
* Set Gear - different activity types:
  * run classification only on activities of the same type (run, ride, etc). 
  * This ensures chosen gear matches activity type
  * Interestingly, I see no exceptions when the app processes a run and attempts to set a bike as the gear. The bike does not end up set on the run in Strava though.
* Set Gear - store classifications:
  * Cannot store Strava data per agreement. 
  * Currently making a request on every activity event for historical data. 
  * Could this cause issues when all  historical data is affected by equipper?
  * Can use caching up to 7 days to reduce request rates
* Set Gear - don't change already-processed activities
  * Does it count as storing strava data to store activity IDs alone?
* Mitigate Abuse of Subscription Endpoint
  * check that subscription ID matches expected? 
  * only allow requests from known Strava IPs?
  * rate limiting
  * alerts on suspicious activity
* Prediction logic v2
  * Create model for predicting the bike used on an activity.
  * Use my data as a sample
  * What data is available per activity? 
    * Avg Speed
    * Distance
    * Start/end locations? Sensitive info. Privacy zone data for activity: read_all, no PZ data for activity: read.
    * Start time
    * Surface data? eg paved vs off-road
    * Power & cadence data. Likely present on some bikes but not others.
    * HR data.
  * What data is needed to build the model?
  * Can we do better than the v1 model?
* Model initialization
  * Will need to gather data to build model for new subscribers. 
  * Maybe expensive? May want to protect against unsubscribe/resubscribe cycles.
  * Consider strava api use terms.
* Store predictions & revisions
  * Store activity IDs and predicted bike IDs. 
  * Equipper can subscribe to activity updates and check if an athlete has set the bike to a non-predicted value
  * This data can be used to evaluate accuracy.
* Prediction logic vX
  * Would be interesting to develop different algorithms and assign users an algorithm at random upon signing up
    * Could also base this on granted permissions, if algorithms need different read-level access
  * Compare accuracy of each algorithm (requires knowledge of bad predictions)