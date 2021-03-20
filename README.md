# dotnet-landmarks-api
This repository contains an ASP .NET Core API that searches for landmarks in a specified location and the images associated with the landmarks.

# Setup
## Prerequisites
- Dotnet Core 3.1 SDK must be installed.
- Visual Studio 2019 must be installed.
- Postman must be installed.

Use a command line interface (cmd, PowerShell etc.), follow the steps below:

## Step 1. Clone or download this repository
git clone https://github.com/smhlana/landmarksapi-dotnetcore.git

## Step 2. Install .NET Core API dependencies
    cd landmarksapi-dotnetcore
    dotnet restore
    dotnet build
    
## Step 3. Trust development certificates ***
    dotnet dev-certs https --clean
    dotnet dev-certs https --trust
    
## Step 4. Save Key Vault credentials
    This project uses Azure key vaul to store application keys. A service principal is used for authentication to the key vault. 
    
    Add environment variables to store the key vault credentials. Add the following environment variables unnder **System Variables**:
    AZURE_CLIENT_ID = <clientId-of-your-service-principal>
    AZURE_CLIENT_SECRET = <clientSecret-of-your-service-principal>
    AZURE_TENANT_ID = <tenantId-of-your-service-principal>

## Step 4. Run the API
    Open LandmarksAPI.sln in Visual Studio 2019 (landmarksapi-dotnetcore\LandmarksAPI\LandmarksAPI.sln) and run
    This should open up a browser window to the url "https://localhost:<port>/api/landmarks" with the message "{"message":"Unauthorized. Please login."}"
    (https://user-images.githubusercontent.com/11193045/111866737-41536f80-8978-11eb-8043-d53b7b993e89.png)

## Step 5. Register
    You have to register before you can use the API.
    To register an account using Postman follw these steps:
    
    1. Open a new request tab by clicking the plus (+) button at the end of the tabs.
    2. Change the http request method to "POST" with the dropdown selector on the left of the URL input field.
    3. In the URL field enter the address "http://localhost:<port>/api/landmarks/users/register" to point to the register endpoint.
    4. Select the "Body" tab below the URL field, change the body type radio button to "raw", and change the format dropdown selector to "JSON".
    5. Enter a JSON object containing the required details in the "Body" textarea as follows and fill in the values as required:
            {
                "title": "Mr",
                "firstName": "Sesethu",
                "lastName": "Mhlana",
                "username": "smhlana",
                "password": "5621547",
                "confirmPassword": "5621547"
            }
      (https://user-images.githubusercontent.com/11193045/111867097-8f697280-897a-11eb-9f68-b8e4713c6e05.png) 
    6. Click the "Send" button, you should receive a "200 OK" response with a "registration successful" message in the response body.
      (https://user-images.githubusercontent.com/11193045/111867185-1b7b9a00-897b-11eb-9f7a-d85c665fe213.png)

## Step 6. Login
    You have to login to get an authentication token.
    To login using Postman follow these steps:
    
    1. In the URL field change the address to "http://localhost:<port>/api/landmarks/users/login" to point to the login endpoint.
    2. Enter a JSON object containing the usename and password as follows:
            {
                "username": "smhlana",
                "password": "5621547"
            }
    3. Click the "Send" button, you should receive a "200 OK" response with the user details and a token. You will use this token to make requests.
      (https://user-images.githubusercontent.com/11193045/111867533-5383dc80-897d-11eb-8a1e-2e763b1f567a.png)

## Step 7. Make requests
    To make a request using Postman follow these steps:
    
    1. Open a new request tab by clicking the plus (+) button at the end of the tabs.
    2. Change the http request method to "GET" with the dropdown selector on the left of the URL input field.
    3. In the URL field enter the address "http://localhost:<port>/api/landmarks/<endpoint>" to point to the register endpoint. Find the relevent endpoint in the **Endpints** section below
    4. Select the "Authorization" tab below the URL field, change the type to "Bearer Token" in the type dropdown selector, and paste the token from the **Login** step into the "Token" field.
    5. Click the "Send" button, you should receive a "200 OK" response with the requested information.
    
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


    
    
