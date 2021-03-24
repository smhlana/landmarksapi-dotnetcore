# .Net Core API - Search For Landmarks
This repository contains an ASP .NET Core API that searches for landmarks in a specified location and the images associated with the landmarks.

# Setup
## Prerequisites
- Dotnet Core 3.1 SDK must be installed.
- Visual Studio 2019 must be installed.
- Postman must be installed.
- Git must be installed.

Use a command line interface (cmd, PowerShell etc.), follow the steps below:

## Step 1. Clone or download this repository
git clone https://github.com/smhlana/landmarksapi-dotnetcore.git

## Step 2. Install .NET Core API dependencies
    cd landmarksapi-dotnetcore/LandmarksAPI
    dotnet restore
    
## Step 3. Trust development certificates ***
    dotnet dev-certs https --clean
    dotnet dev-certs https --trust
    
## Step 4. Save Key Vault credentials
    This project uses Azure key vault to store application keys. A service principal is used for authentication
    to the key vault. 
    
    Add environment variables to store the key vault credentials. Add the following environment variables under 
    System Variables:
    AZURE_CLIENT_ID = <clientId-of-your-service-principal>
    AZURE_CLIENT_SECRET = <clientSecret-of-your-service-principal>
    AZURE_TENANT_ID = <tenantId-of-your-service-principal>
    
    Add the key vault uri in the appsettings.json file (landmarksapi-dotnetcore/LandmarksAPI/appsettings.json) 
    under KeyVault > Uri.
    
    If you had Visual Studio open, restart it at this point for the environment variables to take effect.

## Step 4. Run the API
    Open LandmarksAPI.sln in Visual Studio 2019 (landmarksapi-dotnetcore\LandmarksAPI\LandmarksAPI.sln). Build and run
    the solution.
    This should open up a browser window to the url "https://localhost:<port>/api/landmarks" with the message 
    "{"message":"Unauthorized. Please login."}"
![image](https://user-images.githubusercontent.com/11193045/111881743-a29c3280-89ba-11eb-99a0-9925902eae43.png)

## Step 5. Register
    You have to register before you can use the API.
    To register an account using Postman follow these steps:
    
    1. Open a new request tab by clicking the plus (+) button at the end of the tabs.
    2. Change the http request method to "POST" with the dropdown selector on the left of the URL input field.
    3. In the URL field enter the address "http://localhost:<port>/api/landmarks/users/register" to point to the 
        register endpoint.
    5. Select the "Body" tab below the URL field, change the body type radio button to "raw", and change the 
        format dropdown selector to "JSON".
    7. Enter a JSON object containing the required details in the "Body" textarea as follows and fill in the 
        values as required (password has to be 6 characters or more):
            {
                "title": "Mr",
                "firstName": "Sesethu",
                "lastName": "Mhlana",
                "username": "smhlana",
                "password": "your-secret-password-here",
                "confirmPassword": "your-secret-password-here"
            }
![image](https://user-images.githubusercontent.com/11193045/111899076-d9ae2a80-8a32-11eb-83aa-bcaf959e8299.png)

    6. Click the "Send" button, you should receive a "200 OK" response with a "registration successful" message 
        in the response body. Postman might return a message saying "Unable to verify first certificate" and 
        prompt you to disable verification, select yes.
![image](https://user-images.githubusercontent.com/11193045/111867185-1b7b9a00-897b-11eb-9f7a-d85c665fe213.png)

## Step 6. Login
    You have to login to get an authentication token.
    To login using Postman follow these steps:
    
    1. In the URL field change the address to "https://localhost:<port>/api/landmarks/users/login" to point to 
        the login endpoint.
    3. Enter a JSON object containing the usename and password as follows:
            {
                "username": "smhlana",
                "password": "your-secret-password-here"
            }
    3. Click the "Send" button, you should receive a "200 OK" response with the user details and a token. You 
        will use this token to make requests.
![image](https://user-images.githubusercontent.com/11193045/111898987-25ac9f80-8a32-11eb-8868-752524218374.png)

## Step 7. Make requests
    To make a request using Postman follow these steps:
    
    1. Open a new request tab by clicking the plus (+) button at the end of the tabs.
    2. Change the http request method to "GET" with the dropdown selector on the left of the URL input field.
    3. In the URL field enter the address "https://localhost:<port>/api/landmarks/<endpoint>" to point to the 
        register endpoint. Find the relevent endpoint in the Endpoints section below
    5. Select the "Authorization" tab below the URL field, change the type to "Bearer Token" in the type 
        dropdown selector, and paste the token from the Login step into the "Token" field.
    7. Click the "Send" button, you should receive a "200 OK" response with the requested information.
    
### Endpoints:
#### 1. List all locations
    api/landmarks
    
#### 2. List images for a location
    api/landmarks/locationimages/{locationName}
    
#### 3. Get details for a specific image
    api/landmarks/imagedetailsbyurl?url=<url>
    api/landmarks/imagedetailsbyid?id=<id>
    
#### 4. Search for landmarks around a location
    api/landmarks/searchbyname/{name}
    api/landmarks/searchbylatlong/{latitude}/{longitude}
    
