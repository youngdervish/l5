//http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name
//http://schemas.microsoft.com/ws/2008/06/identity/claims/role

document.addEventListener("DOMContentLoaded", async function () {
    const welcomeMessageDiv = document.getElementById("welcomeMessage");

    const continueBtn = document.getElementById("continueBtn");
    if (continueBtn) {
        continueBtn.addEventListener("click", function () {
            window.location.href = "/Dashboard"; // Matches the @page directive
        });
    }

    // Fetch username from the backend
    try {
        const response = await fetch("/api/users/getUsername", { method: "GET", credentials: "include" });
        const data = await response.json();

        const username = data.username || "Guest";

        // Add the welcome message and styling
        welcomeMessageDiv.innerHTML = `
            <h3 style="font-size: 1.5rem;">Welcome ${username}</h3>
            <br>
        `;
    } catch (error) {
        console.error("Error fetching username:", error);
    }
});