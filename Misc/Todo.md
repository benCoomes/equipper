# Todo
* Record processing stats & Activity Count
  * Store details of each processing event in Cosmos.
  * Cannot store activity data, raw or derived.
  * May store algorithm stats such as std dev, variance, confidence, etc
  * Plan: 
    * ~~Create new 'Activity' data model which has Strava activity ID, athlete ID, and stats ID (guid). This model will have TTL of 7 days, which is the acceptable limit per Strava API agreement.~~
    * ~~Store 'ClassificationStats' model, which has a guid and cross-validation results for each algorithm.~~
    * In SetGear: 
      * Check for existing Activity with matching Strava ID. If one found, do nothing else and return.
      * ~~Run as usual. Update activity with best-match gear.~~
      * ~~Set the ClassificationStats guid on the activity model. Persist the Activity and ClassificationStats items.~~
        * ~~If this fails, log warning and continue. One cause of failure could be a concurrently processed event for the same activity, in which case first-in-wins is acceptable.~~
    * Create new endpoint to return count of ClassificationStats as 'about' how many activities Equipper has processed. Errors and duplicate events more than 7 days apart prevent this from being exact.
  * With this plan, we should accomplish a few things: don't reprocess activities (within 7 days of initial event), get approximate count of number of unique activities processed, and persist data about algorithms. And, all without storing any Strava data longer than 7 days!
* User Counter
  * Display count of users on the website
* UI Framework
  * Use a UI framework so that the header and html boilerplate are not duplicated between the auth and index page.
* Set Gear - different activity types:
  * run classification only on activities of the same type (run, ride, etc). 
  * This ensures chosen gear matches activity type
  * Interestingly, I see no exceptions when the app processes a run and attempts to set a bike as the gear. The bike does not end up set on the run in Strava though.
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
* Store predictions & revisions
  * Store activity IDs and predicted bike IDs. 
  * Equipper can subscribe to activity updates and check if an athlete has set the bike to a non-predicted value
  * This data can be used to evaluate accuracy.
* Prediction logic vX
  * Would be interesting to develop different algorithms and assign users an algorithm at random upon signing up
    * Could also base this on granted permissions, if algorithms need different read-level access
  * Compare accuracy of each algorithm (requires knowledge of bad predictions)