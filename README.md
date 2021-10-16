# Equipper
A Strava Developer Application that automatically picks a bike for each activity based on previous rides and bike selections. Under development.

## Install

### Environment Variables
Set your client ID and client secret as environment variables under the User that will run the application.
```
[Environment]::SetEnvironmentVariable('CoomesEquipper_StravaApi__ClientId', '12345', 'User')
[Environment]::SetEnvironmentVariable('CoomesEquipper_StravaApi__ClientSecret', '**********', 'User')
```

### Runtime
This app is hosted as an Azure Functions app. To run it locally, use the [Azure Functions Core Tools](https://docs.microsoft.com/en-us/azure/azure-functions/functions-run-local?tabs=v3%2Cwindows%2Ccsharp%2Cportal%2Cbash%2Ckeda). Once the tools are instlled, run the following inside of the 'FunctionApp' directory: 
```
func start
```