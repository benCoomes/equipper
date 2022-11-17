* When authorizing with Strava
  * Check on other places where tokens can be updated to ensure they don't incorrectly change userID
  * does register athlete actually store tokens? strava token provider response doesn't include userID
    * Create 'strava only' version of AthleteToken that does not have UserID? 
  * Implement 'get by user' in cosmos storage