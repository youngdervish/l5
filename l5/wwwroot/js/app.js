document.addEventListener("DOMContentLoaded", function () {
    const loginForm = document.getElementById("loginForm");
    const responseDiv = document.getElementById("response");

    loginForm.addEventListener("submit", async function (event) {
        event.preventDefault(); // Prevent the default form submission behavior

        const formData = new FormData(loginForm);
        const payload = new URLSearchParams(formData);

        try {
            // Send POST request to the server
            const response = await fetch("/api/auth/login?" + new Date().getTime(), {
                method: "POST",
                body: payload,
            });

            if (response.ok) {
                const data = await response.json();

                window.location.href = 'welcome.html';
            } else {
                const error = await response.json();
                responseDiv.textContent = `Error: ${error.message}`;
            }
        } catch (err) {
            responseDiv.textContent = `Error: ${err.message}`;
        }
    });
});


/*
this:
fetch("api/auth/token-expiry").then(res => res.json()).then(data => console.log("Access Token in Expiry:", data.accessTokenExpiry)).catch(err => console.error(err));
returns this:
Access Token in Expiry: 2025-02-07T06:49:36Z
this:
fetch("/api/auth/token-expiry", { method: "GET", credentials: "include" }).then(res => res.json()).then(data => {
    const accessTokenExpiry = new Date(data.accessTokenExpiry).getTime();
 
    console.log("Access Token: ", accessTokenExpiry);
 
})
returns this:
Access Token:  1738910976000
how do these represent the same thing but look so different?
*/