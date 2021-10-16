* Store crendentials for user on token exchange
  * find cheap option - try cosmos serverless
* Deauthorize Webhook: Application must implement a webhook to know when an athelete deauthorizes it.
  * Should remove all athlete data and tokens when deauthorized
  * See: https://developers.strava.com/docs/webhooks/
  * Comes as event to existing webhook
  * Example: {"aspect_type":"update","event_time":unixseconds,"object_id":athleteid,"object_type":"athlete","owner_id":athleteid,"subscription_id":subid,"updates":{"authorized":"false"}​}
* Useful activity webhook:
  * Use simple classificiation to set gearID on all new activities
  * Cannot store Strava data per agreement. 
  * Start out making a request on every activity event for historical data. 
  * Can use caching up to 7 days to reduce request rates
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
* Store predictions & revisions
  * Store activity IDs and predicted bike IDs. 
  * Equipper can subscribe to activity updates and check if an athelete has set the bike to a non-predicted value
  * This data can be used to evaluate accuracy.
* Prediction logic vX
  * Would be interesting to develop different algorithms and assign users an algorithm at random upon signing up
    * Could also base this on granted permissions, if algorithms need different read-level access
  * Compare accuracy of each algorithm (requires knowledge of bad predictions)