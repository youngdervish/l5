import { logOut, triggerAlert, fetchTokenExpiry, getRemainingSeconds, startCountdown, renewToken, printToken, initialize } from './dashboard.js';

let remainingSeconds;
//let initialValues = {};

document.addEventListener("DOMContentLoaded", () => {
    // Placeholder function to load users (replace with API call)
    printToken();
    initialize();
    loadUsers();

    document.getElementById("add-user").addEventListener("click", openAddUserOverlay);
    document.getElementById("cancel-add-user").addEventListener("click", closeAddUserOverlay);
    document.getElementById("add-user-confirm").addEventListener("click", addUser);
    document.getElementById("cancel-update-user").addEventListener("click", closeUpdateUserOverlay);
    document.getElementById("update-user-confirm").addEventListener("click", updateUser);

    // Function to open the add user overlay
    async function openAddUserOverlay() {
        remainingSeconds = getRemainingSeconds();
        console.log(remainingSeconds);
        if (remainingSeconds < 30) {
            document.getElementById("add-user-overlay").style.zIndex = 500;
            return;
        }
        document.getElementById("add-user-overlay").style.display = "flex"; // Show overlay
    }


    // Function to close the add user overlay
    function closeAddUserOverlay() {
        document.getElementById("add-user-overlay").style.display = "none"; // Hide overlay
    }

    // Function to add user (validate input and make API call)
    async function addUser() {
        const username = document.getElementById("username").value;
        const password = document.getElementById("password").value;
        const role = document.getElementById("role").value;
        const email = document.getElementById("email").value;
        const phone = document.getElementById("phone").value;

        if (!username || !password || !role) {
            alert("Please fill in the required fields.");
            return;
        }

        try {
            const response = await fetch(`/api/users/add-user?password=${encodeURIComponent(password)}`, {
                method: "POST",
                headers: { "Content-Type": "application/json" },
                body: JSON.stringify({
                    userName: username, // Ensure property names match the backend model
                    email: email,
                    phoneNumber: phone,
                    role: role
                }),
                credentials: "include"
            });

            if (!response.ok) {
                const errorText = await response.text();
                throw new Error(errorText);
            }

            closeAddUserOverlay();
            loadUsers(); // Refresh the user list
        } catch (error) {
            console.error("Error adding user:", error);
            alert(`Error adding user: ${error.message}`);
        }
    }

    async function openUpdateUserOverlay(username, role, email = "", phone = "") {
        remainingSeconds = await getRemainingSeconds();
        console.log(remainingSeconds);
        if (remainingSeconds < 30) {
            document.getElementById("update-user-overlay").style.zIndex = 500;
            return;
        }

        //initialValues = {
        //    username: username, 
        //    role: role,
        //    email: email,
        //    phone: phone
        //};

        document.getElementById("update-username").value = username;
        document.getElementById("update-role").value = role;
        document.getElementById("update-email").value = email;
        document.getElementById("update-phoneNumber").value = phone;

        document.getElementById("update-user-overlay").style.display = "flex";
    }

    // Function to close the update user overlay
    function closeUpdateUserOverlay() {
        document.getElementById("update-user-overlay").style.display = "none";
    }

    // Function to update user (simulate API call)
    async function updateUser(user) {
        const fields = ["username", "role", "email", "phoneNumber"];
        let updatedUser = {};
        let modifiedFields = [];
        const uName = document.getElementById(`update-${fields[0]}`);
        console.log("UserName: ", document.getElementById(`update-${fields[0]}`).value);
        console.log("Role: ", document.getElementById(`update-${fields[1]}`).value);
        console.log("Email: ", document.getElementById(`update-${fields[2]}`).value);

        console.log("Fields array:", fields);
        console.log("Phone field key:", fields[1]);

        console.log("Phone: ", document.getElementById(`update-${fields[3]}`).value);
        // Iterate through each field and check for changes
        fields.forEach(field => {
            let input = document.getElementById(`update-${field}`);
            let newValue = input.value.trim();
            updatedUser[field] = newValue;
            modifiedFields.push(field);
        });

        const passwordInput = document.getElementById("update-password");
        const newPassword = passwordInput.value.trim();

        if (newPassword) {
            updatedUser.PasswordHash = encodeURIComponent(newPassword); 
            modifiedFields.push("password");
        }

        try {
            console.log("Updated user: ", updatedUser);
            const response = await fetch(`/api/users/${uName.value}`, {
                method: "PUT",
                headers: { "Content-Type": "application/json" },
                body: JSON.stringify(updatedUser)
            });

            if (response.status === 204) {
                alert("Changes applied");
                closeUpdateUserOverlay();
                loadUsers();
            } else if (response.status === 304) {
                alert("No changes detected.");
            } else {
                const errorData = await response.json();
                alert("Error updating user: " + JSON.stringify(errorData));
                console.log(JSON.stringify(errorData));
            }
        } catch (error) {
            console.error("Update failed:", error);
            alert("Failed to update user.");
        }

        closeUpdateUserOverlay();
        loadUsers();
    }


    document.getElementById("remove-user").addEventListener("click", removeSelectedUsers);
    document.getElementById("search-user").addEventListener("click", searchUser);
    document.getElementById("back").addEventListener("click", () => {
        window.location.href = "/Dashboard"; // Redirect to dashboard
    });

    document.getElementById("logoutBtn").addEventListener("click", logOut);

    // Select all checkboxes
    document.getElementById("select-all").addEventListener("change", toggleSelectAll);

    // Function to simulate loading users (replace with API call later)
    async function loadUsers() {
        try {
            console.log("Load users called");
            const response = await fetch('/api/users/get-users', {
                method: 'GET',
                credentials: 'include' // Make sure cookies are sent with the request
            });

            const users = await response.json();
           
            users.sort((a, b) => a.username.localeCompare(b.username));

            const tbody = document.getElementById("user-table-body");
            tbody.innerHTML = ""; // Clear current rows

            users.forEach(user => {
                const row = document.createElement("tr");
                row.innerHTML = `
                <td><input type="checkbox" class="select-user"></td>
                <td><a href="#" class="update-user-link" data-username="${user.username}" 
                    data-role="${user.role}" data-email="${user.email}" data-phone="${user.phoneNumber}">
                    ${user.username}
                </a></td>
                <td>${user.role}</td>
            `;
                tbody.appendChild(row);
            });

            // Add event listeners for update-user links after dynamically generating the table rows
            document.querySelectorAll(".update-user-link").forEach(link => {
                link.addEventListener("click", function (event) {
                    event.preventDefault();
                    const username = this.getAttribute("data-username");
                    const role = this.getAttribute("data-role");
                    const email = this.getAttribute("data-email");
                    const phone = this.getAttribute("data-phone");

                    openUpdateUserOverlay(username, role, email, phone); // Call your overlay function
                });
            });
        } catch (error) {
            console.error('Error loading users:', error);
        }
    }

    // Function to remove selected users
    async function removeSelectedUsers() {
        const checkboxes = document.querySelectorAll(".select-user:checked");
        if (checkboxes.length === 0) {
            alert("No users selected for removal.");
            return;
        }

        const usernames = Array.from(checkboxes).map(checkbox =>
            checkbox.closest("tr").querySelector("td:nth-child(2)").textContent.trim()
        );

        try {
            const response = await fetch("/api/users", { 
                method: "DELETE",
                headers: { "Content-Type": "application/json" },
                body: JSON.stringify(usernames), // Directly sending the array of usernames
                credentials: "same-origin"
            });

            if (!response.ok) throw new Error("Failed to remove users");

            loadUsers(); // Refresh list after deletion
        } catch (error) {
            console.error("Error removing users:", error);
            alert("Error removing users. Please try again.");
        }
    }

    // Function to search for a user
    async function searchUser() {
        const searchInput = document.getElementById("search-input").value.trim();
        if (!searchInput) {
            alert("Please enter a username to search.");
            loadUsers();
            return;
        }

        try {
            const response = await fetch(`api/users/${encodeURIComponent(searchInput)}`, {
                method: 'GET',
                credentials: 'include' // Ensures cookies are sent for authentication
            });

            if (!response.ok) {
                if (response.status === 404) {
                    alert("User not found.");
                } else {
                    alert(`Error: ${response.statusText}`);
                }
                return;
            }

            const user = await response.json();
            console.log("Searched User: ", user); // Log the response
            displaySearchedUser(user);

        } catch (error) {
            console.error("Error fetching user:", error);
            alert("An error occurred while fetching the user.");
        }
    }

    function displaySearchedUser(user) {
        const tableBody = document.getElementById("user-table-body");

        // Clear existing rows (optional, depends on desired behavior)
        tableBody.innerHTML = "";

        // Create a new row
        const row = document.createElement("tr");

        row.innerHTML = `
            <td><input type="checkbox" class="select-user" data-username="${user.username}"></td>
            <td>
                <a href="#" class="update-user-link" data-username="${user.username}" 
                   data-role="${user.role}" data-email="${user.email}" data-phone="${user.phoneNumber}">
                   ${user.username}
                </a>
            </td>
            <td>${user.role || "N/A"}</td>
            <td>${user.email || "N/A"}</td>
            <td>${user.phoneNumber || "N/A"}</td>`;                     

        tableBody.appendChild(row);

        // Add event listeners for the update-user link
        document.querySelectorAll(".update-user-link").forEach(link => {
            link.addEventListener("click", function (event) {
                event.preventDefault();
                const username = this.getAttribute("data-username");
                const role = this.getAttribute("data-role");
                const email = this.getAttribute("data-email");
                const phone = this.getAttribute("data-phone");

                openUpdateUserOverlay(username, role, email, phone); // Call your overlay function
            });
        });
    }

    // Toggle select all checkboxes
    function toggleSelectAll() {
        const selectAllCheckbox = document.getElementById("select-all");
        const userCheckboxes = document.querySelectorAll(".select-user");

        userCheckboxes.forEach(checkbox => {
            checkbox.checked = selectAllCheckbox.checked;
        });
    }
});

