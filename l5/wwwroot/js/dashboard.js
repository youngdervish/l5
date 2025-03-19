let accessTokenExpiry;
let refreshTokenExpiry;
let countdownInterval;
let tokenRefreshCount = 0;
const logoutBtn = document.getElementById("logoutBtn");
const countdownElement = document.getElementById("countdownTimer");
let isRefreshing = false;
let remainingTimeForAccess;

export async function logOut() {
    console.log("Log OUT function has been summoned!!!");
    try {
        const response = await fetch('api/auth/logout', { method: 'POST', credentials: 'same-origin' });
        const respText = await response.text();

        console.log("Response text: ", respText);
        console.log("Attempting to clear the tokens");

        document.cookie = "accessToken=; Max-Age=0; path=/; domain=" + document.domain;
        document.cookie = "refreshToken=; Max-Age=0; path=/; domain=" + document.domain;

        setTimeout(printToken, 3000);
        setTimeout(console.log("Back to login..."), 3000);
        window.location.href = 'index.html';
    }
    catch (error) { console.error("Logout failed:", error); }
}

export function printToken()
{
    fetch("api/jwt/print-access-token").then(res => res.json()).then(data => console.log("Access Token:", data.accessToken)).catch(err => console.error(err));
    fetch("/api/jwt/print-refresh-token").then(res => res.json()).then(data => console.log("Refresh Token:", data.refreshToken)).catch(err => console.error(err));
}

export async function fetchTokenExpiry() {
    try {
        fetch("api/jwt/token-expiry").then(res => res.json()).then(data => {
            console.log("Access Token in Expiry:", data.accessTokenExpiry),
                accessTokenExpiry = new Date(data.accessTokenExpiry).getTime(), 
                console.log("Refresh Token in Expiry:", data.refreshTokenExpiry),
                refreshTokenExpiry = new Date(data.refreshTokenExpiry);
        }).catch(err => console.error(err));
        //setTimeout(3000);
    } catch (error) {
        console.error("Error fetching token expiry: ", error);
    }
}
export function startCountdown() {
    clearInterval(countdownInterval);
    countdownInterval = setInterval(() => {
        const now = Date.now();
        //let remainingTimeForAccess = Math.floor((accessTokenExpiry - now) / 1000);
        remainingTimeForAccess = Math.floor((accessTokenExpiry - now) / 1000);
        let remainingTimeForRefresh = Math.floor((refreshTokenExpiry - now) / 1000);

        countdownElement.innerText = `Access Token expires in ${remainingTimeForAccess} seconds.`;

        if (remainingTimeForAccess === 30) {
            triggerAlert();
        }

        if (remainingTimeForAccess <= 0) {
            clearInterval(countdownInterval);
            console.log("Remaining Time for Access is 0");
            logOut();
        }
    }, 1000);
}

export function getRemainingSeconds() {
    return remainingTimeForAccess;
}

export async function initialize() {
    //printToken();
    await fetchTokenExpiry();
    startCountdown();
}

export async function renewToken() {
    if (isRefreshing) return;
    isRefreshing = true;

    console.log("Tokens prior to refresh");
    printToken();

    try {
        const response = await fetch("/api/jwt/rotate-tokens", { method: 'POST', credentials: 'include' });
        tokenRefreshCount++;
        if (response.ok) {
            console.log("Token refreshed successfully");
            console.log("Refresh Token Count = ", tokenRefreshCount);
            await initialize();
        } else {
            setTimeout(console.error("Token refresh failed"), 3000);
            logOut();
        }
    } catch (error) {
        console.error("Renew token error: ", error);
    } finally {
        isRefreshing = false;
    }
    console.log("Tokens after to refresh");
    printToken();
}

export async function triggerAlert() {
    //console.log("Trigger Alerted");
    //const refreshOverlay = document.getElementById("refresh-overlay");
    //refreshOverlay.style.display = "flex";

    //document.getElementById("refreshBtn").addEventListener("click", async () => {
    //    await renewToken();
    //    refreshOverlay.style.display = "none";
    //});

    //document.getElementById("cancelBtn").addEventListener("click", () => {
    //    refreshOverlay.style.display = "none";
    //});

    console.log("Trigger Alerted");

    const refreshOverlay = document.getElementById("refresh-overlay");
    refreshOverlay.style.display = "flex";  // Show the modal

    // Ensure buttons are available before attaching event listeners
    const refreshBtn = document.getElementById("refreshBtn");
    const cancelBtn = document.getElementById("cancelBtn");

    if (refreshBtn && cancelBtn) {
        refreshBtn.addEventListener("click", async () => {
            await renewToken();  // Call token renewal
            refreshOverlay.style.display = "none";  // Hide modal after action
        });

        cancelBtn.addEventListener("click", () => {
            refreshOverlay.style.display = "none";  // Close the modal without action
        });
    } else {
        console.error("refreshBtn or cancelBtn not found");
    }
}

//async function getUserRole() {
//    try {
//        const response = await fetch('/api/users/getUserRole', { method: 'GET', credentials: 'include' });
//        const data = await response.json();
//        return data.role || 'Guest';  // Default to 'Guest' if no role is found
//    } catch (error) {
//        console.error("Error fetching user role:", error);
//        return 'Guest';
//    }
//}

document.addEventListener("DOMContentLoaded", async function () {
    printToken();
    await initialize();

    //const userRole = await getUserRole();

    //console.log(`User role is ${userRole}`);

    //if (userRole != 'Administrator') {
    //    const userDbButton = document.querySelector('.db-button:nth-child(1)');
    //    userDbButton.style.display = 'none';  // Hide the UserDB button for non-admin users
    //}

    logoutBtn.addEventListener("click", logOut);
});