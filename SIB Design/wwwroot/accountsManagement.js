// https://auth0.com/docs/secure/tokens/access-tokens/get-access-tokens#example-post-to-token-url
// Create a webAuth instance of Auth0 credentials for the logout function
const webAuth = new auth0.WebAuth({
    domain: 'dev-gmprewiciwvaauxn.us.auth0.com',
    clientID: 'eNIZK1CtP2TTXt6NUGF6AW4ghRfx6VKA',
    redirectUri: `${window.location.origin}/accountsMainPage.html`
});

// Button the user can click to log them out and redirect to welcome page
document.getElementById("signOutBtn").addEventListener("click", async () => {
    // try {
    //     // Redirect the user to initiate the logout process on the server
    //     window.location.href = 'http://localhost:3000/logout';
    // } catch (error) {
    //     console.error("Login error:", error);
    // }

    try {
        await webAuth.logout({
            logoutParams: {
                // URL to send them back to after sign out button is clicked
                returnTo: `${window.location.origin}/index.html`
            }
        });
    } catch (error) {
        console.error("Logout error:", error);
    }
});

// Send a delete request that gets handled by the server on the backend
document.getElementById("deleteAccountBtn").addEventListener("click", async () => {
    const sendDeleteRequest = {
        method: 'POST',
        url: 'http://localhost:3000/delete',
        headers: { 'Content-Type': 'application/json' },
    };

    // Check if the delete request was received by the server
    // This will be commented out after testing is done locally
    axios(sendDeleteRequest).then(response => {
        console.log("Received the account deletion request and deleted account");
    })
        .catch(error => {
            console.error(error);
        });
});

