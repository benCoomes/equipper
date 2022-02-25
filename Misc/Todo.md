# Todo
* User Counter
  * Display count of users on the website
* UI Framework
  * Use a UI framework so that the header and html boilerplate are not duplicated between the auth and index page.
* Set Gear - different activity types:
  * run classification only on activities of the same type (run, ride, etc). 
  * This ensures chosen gear matches activity type
  * Interestingly, I see no exceptions when the app processes a run and attempts to set a bike as the gear. The bike does not end up set on the run in Strava though.
* User Authentication
  * To implement data access requests, we need a way for users to authenticate with equipper. 
  * Azure SWAs have built in support for authentication. This seems like a good option.
    * Strava does not implement OIDC, so it can't be an identity provider, unfortunately.
  * Could **maybe** use auth tokens as authentication mechanism: 
    * It isn't advised: https://oauth.net/articles/authentication/#access-tokens
    * However, the main criticism above does not apply here, as the response from Strava when exchanging an auth code includes the athlete ID. Equipper would not have to parse the access token.
* Data request
  * requires authentication to be in place.
  * necessary when caching activities
  * support request of data
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
  * On activity updates, check if an athlete has set the bike to a non-predicted value
  * This data can be used to asses accuracy.
* Prediction logic vX
  * Would be interesting to develop different algorithms and assign users an algorithm at random upon signing up
    * Could also base this on granted permissions, if algorithms need different read-level access
  * Compare accuracy of each algorithm (requires knowledge of bad predictions)