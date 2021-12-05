# Todo
* UI Framework
  * Use a UI framework so that the header and html boilerplate are not duplicated between the auth and index page.
* Activity processing counter
  * It would be cool to know how many activities Equipper has processed and display this on the website.
* User Counter
  * Display count of users on the website
* Record processing stats
  * Store details of each processing event in Cosmos.
  * Cannot store activity data, raw or derived.
  * May store algorithm stats such as std dev, variance, confidence, etc
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
  * Cannot store activity IDs longer than 7 days
  * Consider cosmos table with TTL of 7 days.
* Data request
  * necessary when caching activities
  * support request of data
  * use auth flow to get access token, then access token to get athlete ID. Then, return athlete data.
  * Call auth flow with 'read' scope only. Return to different endpoint.
    * Existing tokens should continue to work with privs granted at the time
    * do not force consent screen if already authorized.
  * do not return access tokens in response!
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