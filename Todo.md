* Store crendentials for user on token exchange
  * find cheap option - maybe use azure blob storage as simple document db?
* Logging activity webhook
  * Implement subscription endpoint that sets up a subscription successfully and then logs activity ID for completed activities.
  * Subscribe to new activities for registered athletes. 
  * POST is made to registered callback. Application must respond within 2 seconds.
  * Will need to make an API call to get details of activity.
  * One 'Save' action by the athlete can cause multiple webhook events.
  * If app has 'read' scope, then it will recieve notifications when activity is changed to private and when it is changed to public/followers. App must respect privacy.
  * App can only have single subscription.
  * See: https://developers.strava.com/docs/webhooks/
  * use ngrok for local testing
* Deauthorize Webhook: Application must implement a webhook to know when an athelete deauthorizes it.
  * Should remove all athlete data and tokens when deauthorized
  * See: https://developers.strava.com/docs/webhooks/
* Useful activity webhook:
  * Use simple classificiation to set gearID on all new activities
* Set up deployment pipeline for app (github?).
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
  * What data needs to be stored for the model?
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