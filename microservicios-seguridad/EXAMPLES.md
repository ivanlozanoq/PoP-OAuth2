# OAuth2 PoC - Usage Examples

This document provides practical examples of how to use the OAuth2 PoC API.

## Prerequisites

1. Application is running on `https://localhost:7001`
2. GitHub OAuth2 application is configured
3. Configuration file contains valid GitHub Client ID and Secret

## Example 1: Complete Authentication Flow with cURL

### Step 1: Initiate Authentication

```bash
curl -X POST "https://localhost:7001/api/auth/login" \
  -H "Content-Type: application/json" \
  -d '{
    "provider": "GitHub",
    "codeChallenge": null
  }'
```

**Expected Response:**
```json
{
  "authorizationUrl": "https://github.com/login/oauth/authorize?client_id=your_client_id&redirect_uri=https%3A//localhost%3A7001/api/auth/callback&scope=user%3Aemail&state=550e8400-e29b-41d4-a716-446655440000&response_type=code",
  "state": "550e8400-e29b-41d4-a716-446655440000"
}
```

### Step 2: User Authorization
1. Copy the `authorizationUrl` from the response
2. Open it in a web browser
3. Log in to GitHub and authorize the application
4. GitHub will redirect to: `https://localhost:7001/api/auth/callback?code=...&state=...`

### Step 3: The callback is automatically processed
The API will return user information:

```json
{
  "id": "123e4567-e89b-12d3-a456-426614174000",
  "email": "user@example.com",
  "name": "John Doe",
  "provider": "GitHub",
  "accessToken": "gho_xxxxxxxxxxxxxxxxxxxxxxxxxxxx",
  "expiresAt": "2024-12-20T12:00:00Z"
}
```

## Example 2: Using Postman

### Collection Setup
1. Create a new Postman collection
2. Set base URL variable: `{{baseUrl}}` = `https://localhost:7001`

### Request 1: Login
- **Method**: POST
- **URL**: `{{baseUrl}}/api/auth/login`
- **Headers**: `Content-Type: application/json`
- **Body** (raw JSON):
```json
{
  "provider": "GitHub"
}
```

### Request 2: Manual Callback Test
- **Method**: GET  
- **URL**: `{{baseUrl}}/api/auth/callback`
- **Query Parameters**:
  - `code`: `authorization_code_from_github`
  - `state`: `state_from_previous_request`

## Example 3: JavaScript/Fetch API

```javascript
// Step 1: Initiate authentication
async function startOAuth2Login() {
    const response = await fetch('https://localhost:7001/api/auth/login', {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json',
        },
        body: JSON.stringify({
            provider: 'GitHub'
        })
    });
    
    const data = await response.json();
    
    // Store state for validation
    sessionStorage.setItem('oauth2_state', data.state);
    
    // Redirect user to authorization URL
    window.location.href = data.authorizationUrl;
}

// Step 2: Handle callback (on callback page)
async function handleOAuth2Callback() {
    const urlParams = new URLSearchParams(window.location.search);
    const code = urlParams.get('code');
    const state = urlParams.get('state');
    const error = urlParams.get('error');
    
    // Validate state
    const storedState = sessionStorage.getItem('oauth2_state');
    if (state !== storedState) {
        console.error('State mismatch - possible CSRF attack');
        return;
    }
    
    if (error) {
        console.error('OAuth2 Error:', error);
        return;
    }
    
    // The callback endpoint will be called automatically by the redirect
    // You can also manually process it:
    const response = await fetch(`https://localhost:7001/api/auth/callback?code=${code}&state=${state}`);
    const userData = await response.json();
    
    console.log('Authenticated user:', userData);
    
    // Store user info and access token
    sessionStorage.setItem('user_data', JSON.stringify(userData));
    sessionStorage.setItem('access_token', userData.accessToken);
}
```

## Example 4: Error Handling

### Invalid Provider
```bash
curl -X POST "https://localhost:7001/api/auth/login" \
  -H "Content-Type: application/json" \
  -d '{
    "provider": "InvalidProvider"
  }'
```

**Expected Response (400 Bad Request):**
```json
{
  "error": "Failed to initiate authentication",
  "message": "Unsupported OAuth2 provider"
}
```

### OAuth2 Error Callback
When user denies authorization, GitHub redirects with error parameters:
```
https://localhost:7001/api/auth/callback?error=access_denied&error_description=The+user+has+denied+your+application+access.&state=550e8400-e29b-41d4-a716-446655440000
```

**Expected Response (400 Bad Request):**
```json
{
  "error": "Authentication failed",
  "message": "OAuth2 error: access_denied - The user has denied your application access."
}
```

## Example 5: Refresh Token (Not Supported by GitHub)

```bash
curl -X POST "https://localhost:7001/api/auth/refresh" \
  -H "Content-Type: application/json" \
  -d '{
    "refreshToken": "refresh_token_here",
    "provider": "GitHub"
  }'
```

**Expected Response (400 Bad Request):**
```json
{
  "error": "Token refresh not supported",
  "message": "GitHub OAuth2 does not support refresh tokens in the basic flow"
}
```

## Testing Tips

1. **Use HTTPS**: GitHub requires HTTPS for OAuth2 callbacks in production
2. **State Validation**: Always validate the state parameter to prevent CSRF attacks
3. **Error Handling**: Implement proper error handling for all scenarios
4. **Token Storage**: Store tokens securely (consider HttpOnly cookies for production)
5. **Logging**: Check application logs for detailed error information

## Security Considerations for Production

- Implement PKCE (Proof Key for Code Exchange) for enhanced security
- Use secure, HttpOnly cookies for token storage
- Implement proper session management
- Add rate limiting to prevent abuse
- Validate all inputs and sanitize outputs
- Use HTTPS in production
- Implement proper CORS policies
- Add comprehensive logging and monitoring
