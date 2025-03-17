document.addEventListener("DOMContentLoaded", function () {
    const panelContentDiv = document.getElementById("panelContent");
    const logoutBtn = document.getElementById("logoutBtn");
    const token = localStorage.getItem("token");

    if (token) {
        try {
            const decodedToken = JSON.parse(atob(token.split(".")[1]));
            const username = decodedToken["http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name"];
            const role = decodedToken["http://schemas.microsoft.com/ws/2008/06/identity/claims/role"];

            // Display username and role
            panelContentDiv.innerHTML = `<h2>Welcome, ${username}!</h2><p>You are logged in as ${role}.</p>`;

            // Display content based on role
            if (role === "Administrator") {
                panelContentDiv.innerHTML += `
                    <h3>Admin Actions:</h3>
                    <ul>
                        <li><button id="editUsersBtn">Edit Users</button></li>
                        <li><button id="viewBooksBtn">View Books Database</button></li>
                        <li><button id="addBooksBtn">Add New Book</button></li>
                        <li><button id="updateBooksBtn">Update Book</button></li>
                        <li><button id="removeBooksBtn">Remove Book</button></li>
                    </ul>
                `;
                // Add event listeners for Admin actions
                document.getElementById("editUsersBtn").addEventListener("click", function () {
                    window.location.href = "/admin/edit-users"; // Admin page for user management
                });
                document.getElementById("viewBooksBtn").addEventListener("click", function () {
                    window.location.href = "/admin/view-books"; // Admin page to view books
                });
                document.getElementById("addBooksBtn").addEventListener("click", function () {
                    window.location.href = "/admin/add-book"; // Admin page to add new book
                });
                document.getElementById("updateBooksBtn").addEventListener("click", function () {
                    window.location.href = "/admin/update-book"; // Admin page to update book
                });
                document.getElementById("removeBooksBtn").addEventListener("click", function () {
                    window.location.href = "/admin/remove-book"; // Admin page to remove book
                });
            } else {
                panelContentDiv.innerHTML += `
                    <h3>User Actions:</h3>
                    <ul>
                        <li><button id="viewBooksBtn">View Books</button></li>
                    </ul>
                `;
                // Add event listener for User action
                document.getElementById("viewBooksBtn").addEventListener("click", function () {
                    window.location.href = "/user/view-books"; // User page to view books
                });
            }

            // Logout action
            logoutBtn.addEventListener("click", function () {
                localStorage.removeItem("token"); // Remove the token from localStorage
                window.location.href = "/login";  // Redirect to login page after logout
            });

        } catch (err) {
            panelContentDiv.textContent = "Invalid token. Please login again.";
        }
    } else {
        panelContentDiv.textContent = "No valid token found. Please login.";
    }
});
