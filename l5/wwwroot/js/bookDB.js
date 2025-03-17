import { logOut, triggerAlert, fetchTokenExpiry, getRemainingSeconds, startCountdown, renewToken, printToken, initialize } from './dashboard.js';

let remainingSeconds;
let oldTitle;
let oldAuthor;

document.addEventListener("DOMContentLoaded", async () => {
    printToken();
    initialize();
    loadBooks();
    //const userRole = await getUserRole();
    const userRole = await getUserRoleFromToken();
    adjustButtonAvailability(userRole);

    document.getElementById("add-book").addEventListener("click", openAddBookOverlay);
    document.getElementById("cancel-add-book").addEventListener("click", closeAddBookOverlay);
    document.getElementById("add-book-confirm").addEventListener("click", addBook);
    document.getElementById("cancel-update-book").addEventListener("click", closeUpdateBookOverlay);
    document.getElementById("update-book-confirm").addEventListener("click", updateBook);
    document.getElementById("borrow-book").addEventListener("click", borrowSelectedBooks);
    document.getElementById("return-book").addEventListener("click", returnSelectedBooks);

    async function getUserRoleFromToken() {
        //try {
        //    const response = await fetch('/api/jwt/get-user-role-via-token', { method: 'GET', credentials: 'include' });
        //    const data = await response.json();
        //    console.log(`Get User Role from Token Data: ${data}`);
        //    console.log(`Get User Role From Token: ${data.role}`);

        //    return data.role || 'Guest';
        //} catch (error) {
        //    console.error("Error fetching Get User Role From Token:", error);
        //    return 'Error Guest';
        //}

        try {
            const response = await fetch('/api/jwt/get-user-role-via-token', {
                method: 'GET',
                credentials: 'include'
            });

            if (!response.ok) {
                console.error("Failed to fetch role, status:", response.status);
                return 'Guest';
            }

            const data = await response.json();
            return data.role || 'Guest';
        } catch (error) {
            console.error("Error fetching user role from token:", error);
            return 'Guest';
        }
    }

    async function getUserRole() {
        try {
            const response = await fetch('/api/users/getUserRole', { method: 'GET', credentials: 'include' });
            const data = await response.json();
            
            return data.role || 'Guest';  // Default to 'Guest' if no role is found
        } catch (error) {
            console.error("Error fetching user role:", error);
            return 'Guest';
        }
    }

    function adjustButtonAvailability(role) {
        console.log(role);
        const addBookButton = document.getElementById("add-book");
        const removeBookButton = document.getElementById("remove-book");

        if (role == "Student") {
            addBookButton.disabled = true;
            removeBookButton.disabled = true;
            addBookButton.style.display = 'none';
            removeBookButton.style.display= 'none';
        } else {
            addBookButton.disabled = false;
            removeBookButton.disabled = false;
        }
    }

    // Open the add book overlay
    async function openAddBookOverlay() {
        remainingSeconds = getRemainingSeconds();
        console.log(remainingSeconds);
        if (remainingSeconds < 30) {
            document.getElementById("add-book-overlay").style.zIndex = 500;
            return;
        }
        document.getElementById("add-book-overlay").style.display = "flex";
    }

    // Close the add book overlay
    function closeAddBookOverlay() {
        document.getElementById("add-book-overlay").style.display = "none";
    }

    // Add a new book
    async function addBook() {
        const title = document.getElementById("book-title").value;
        const author = document.getElementById("book-author").value;
        const year = document.getElementById("book-year").value;  // Year field
        const quantity = document.getElementById("book-quantity").value; // Quantity field

        if (!title || !author || !year || !quantity) {
            alert("Please fill in all required fields.");
            return;
        }

        try {
            const response = await fetch(`/api/books/add-book`, {
                method: "POST",
                headers: { "Content-Type": "application/json" },
                body: JSON.stringify({
                    title: title,
                    author: author,
                    year: year,
                    quantity: quantity,
                    borrowedBy: null,
                    borrowedDate: null
                }),
                credentials: "include"
            });

            if (!response.ok) {
                const errorText = await response.text();
                throw new Error(errorText);
            }

            closeAddBookOverlay();
            loadBooks(); // Refresh the book list
        } catch (error) {
            console.error("Error adding book:", error);
            alert(`Error adding book: ${error.message}`);
        }
    }

    // Open the update book overlay
    async function openUpdateBookOverlay(title, author, year = "", quantity = "") {
        remainingSeconds = await getRemainingSeconds();
        console.log(remainingSeconds);
        if (remainingSeconds < 30) {
            document.getElementById("update-book-overlay").style.zIndex = 500;
            return;
        }

        document.getElementById("update-book-title").value = title;
        document.getElementById("update-book-author").value = author;
        document.getElementById("update-book-year").value = year;
        document.getElementById("update-book-quantity").value = quantity;

        document.getElementById("update-book-overlay").style.display = "flex";

        oldTitle = title;
        oldAuthor = author;
    }

    // Close the update book overlay
    function closeUpdateBookOverlay() {
        document.getElementById("update-book-overlay").style.display = "none";
    }

    // Update book (simulate API call)
    async function updateBook() {
        const title = document.getElementById("update-book-title").value;
        const author = document.getElementById("update-book-author").value;
        const year = document.getElementById("update-book-year").value;
        const quantity = document.getElementById("update-book-quantity").value;

        const updatedBook = {
            title: title,
            author: author,
            year: year,
            quantity: quantity
        };

        try {
            const response = await fetch(`/api/books/update-book/${oldTitle}/${oldAuthor}`, {
                method: "PUT",
                headers: { "Content-Type": "application/json" },
                body: JSON.stringify(updatedBook)
            });

            const text = await response.text(); // Read response as text first
            console.log("Response Text:", text);

            if (!response.ok) {
                throw new Error(text); // Show error message
            }

            if (response.status === 204) {
                alert("Changes applied");
                closeUpdateBookOverlay();
                loadBooks();
            } else if (response.status === 304) {
                alert("No changes detected.");
            } else {
                const errorData = await response.json();
                alert("Error updating book: " + JSON.stringify(errorData));
                console.log(JSON.stringify(errorData));
            }
        } catch (error) {
            console.error("Update failed:", error);
            alert("Failed to update book.");
        }

        closeUpdateBookOverlay();
        loadBooks();
    }

    // Remove selected books
    document.getElementById("remove-book").addEventListener("click", removeSelectedBooks);
    document.getElementById("search-book").addEventListener("click", searchBook);
    document.getElementById("back").addEventListener("click", () => {
        window.location.href = "/Dashboard"; // Redirect to dashboard
    });

    document.getElementById("logoutBtn").addEventListener("click", logOut);

    async function loadBooks() {
        try {
            console.log("Load books called");
            const response = await fetch('/api/books/get-borrowed-books', {
                method: 'GET',
                credentials: 'include' // Make sure cookies are sent with the request
            });

            const books = await response.json();
            console.log("Books::: ", books);

            books.sort((a, b) => a.title.localeCompare(b.title));

            const tbody = document.getElementById("book-table-body");
            tbody.innerHTML = ""; // Clear current rows

            books.forEach(book => {
                const row = document.createElement("tr");
                row.innerHTML = `
                <td><input type="checkbox" class="select-book"></td>
                <td><a href="#" class="update-book-link" data-title="${book.title}" data-author="${book.author}" data-year="${book.year}" data-quantity="${book.quantity}">
                    ${book.title}
                </a></td>
                <td style="display: none">${book.id}</td>
                <td>${book.author}</td>
                <td>${book.year}</td>
                <td>${book.quantity}</td>
            `;

                // Highlight borrowed books
                if (book.isBorrowed) {
                    row.style.backgroundColor = 'orange'; // Highlight borrowed books with a different color
                }

                tbody.appendChild(row);
            });

            // Add event listeners for update-book links after dynamically generating the table rows
            document.querySelectorAll(".update-book-link").forEach(link => {
                link.addEventListener("click", function (event) {
                    event.preventDefault();
                    const title = this.getAttribute("data-title");
                    const author = this.getAttribute("data-author");
                    const year = this.getAttribute("data-year");
                    const quantity = this.getAttribute("data-quantity");

                    openUpdateBookOverlay(title, author, year, quantity); // Call your overlay function
                });
            });
        } catch (error) {
            console.error('Error loading books:', error);
        }
    }
    //async function loadBooks() {
    //    try {
    //        console.log("Load books called");
    //        const response = await fetch('/api/books/get-books', {
    //            method: 'GET',
    //            credentials: 'include' // Make sure cookies are sent with the request
    //        });

            

    //        const books = await response.json();
    //        console.log("Books::: ", books);

    //        books.sort((a, b) => a.title.localeCompare(b.title));

    //        const tbody = document.getElementById("book-table-body");
    //        tbody.innerHTML = ""; // Clear current rows

    //        books.forEach(book => {
    //            const row = document.createElement("tr");
    //            row.innerHTML = `
    //                <td><input type="checkbox" class="select-book"></td>
    //                <td><a href="#" class="update-book-link" data-title="${book.title}" data-author="${book.author}" data-year="${book.year}" data-quantity="${book.quantity}">
    //                    ${book.title}
    //                </a></td>
    //                <td style="display: none">${book.id}</td>
    //                <td>${book.author}</td>
    //                <td>${book.year}</td>
    //                <td>${book.quantity}</td>
    //            `;
    //            tbody.appendChild(row);
    //        });

    //        // Add event listeners for update-book links after dynamically generating the table rows
    //        document.querySelectorAll(".update-book-link").forEach(link => {
    //            link.addEventListener("click", function (event) {
    //                event.preventDefault();
    //                const title = this.getAttribute("data-title");
    //                //const id = this.getAttribute("data-id");
    //                const author = this.getAttribute("data-author");
    //                const year = this.getAttribute("data-year");
    //                const quantity = this.getAttribute("data-quantity");



    //                openUpdateBookOverlay(title, author, year, quantity); // Call your overlay function
    //            });
    //        });
    //    } catch (error) {
    //        console.error('Error loading books:', error);
    //    }
    //}

    // Function to remove selected books
    async function removeSelectedBooks() {
        const checkboxes = document.querySelectorAll(".select-book:checked");
        if (checkboxes.length === 0) {
            alert("No books selected for removal.");
            return;
        }

        const titles = Array.from(checkboxes).map(checkbox =>
            checkbox.closest("tr").querySelector("td:nth-child(3)").textContent.trim()
        );

        try {
            const response = await fetch("/api/books", {
                method: "DELETE",
                headers: { "Content-Type": "application/json" },
                body: JSON.stringify(titles), // Directly sending the array of titles
                credentials: "same-origin"
            });

            if (!response.ok) throw new Error("Failed to remove books");

            loadBooks(); // Refresh list after deletion
        } catch (error) {
            console.error("Error removing books:", error);
            alert("Error removing books. Please try again.");
        }
    }

    // Function to search for a book
    async function searchBook() {
        const searchInput = document.getElementById("search-input").value.trim();
        if (!searchInput) {
            alert("Please enter a title to search.");
            loadBooks();  // Assuming this loads all books when no search is made
            return;
        }

        try {
            const response = await fetch(`api/books/${encodeURIComponent(searchInput)}`, {
                method: 'GET',
                credentials: 'include' // Ensures cookies are sent for authentication
            });

            if (!response.ok) {
                if (response.status === 404) {
                    alert("No books found matching the title.");
                } else {
                    alert(`Error: ${response.statusText}`);
                }
                return;
            }

            const books = await response.json();  // Now expecting an array of books
            console.log("Searched Books: ", books); // Log the response for debugging

            if (books.length === 0) {
                alert("No books found matching the title.");
                return;
            }

            displaySearchedBooks(books);  // Assuming this function can display multiple books

        } catch (error) {
            console.error("Error fetching books:", error);
            alert("An error occurred while fetching the books.");
        }
    }

    // Borrow function
    async function borrowSelectedBooks() {
        const checkboxes = document.querySelectorAll(".select-book:checked");

        if (checkboxes.length === 0) {
            alert("No books selected for borrowing.");
            return;
        }

        const booksToBorrow = Array.from(checkboxes).map(checkbox =>
            checkbox.closest("tr").querySelector("td:nth-child(3)").textContent.trim()
        );

        try {
            const response = await fetch("/api/books/borrow", {
                method: "POST",
                headers: { "Content-Type": "application/json" },
                body: JSON.stringify(booksToBorrow),
                credentials: "include"
            });

            if (!response.ok) {
                const errorText = await response.text();
                throw new Error(errorText);
            }

            alert("Books borrowed successfully.");
            
        } catch (error) {
            console.error("Error borrowing books:", error);
            alert(`Error borrowing books: ${error.message}`);
        }
        loadBooks(); // Refresh book list
    }

    async function returnSelectedBooks() {
        const checkboxes = document.querySelectorAll(".select-book:checked");

        if (checkboxes.length === 0) {
            alert("No books selected for borrowing.");
            return;
        }

        const booksToReturn = Array.from(checkboxes).map(checkbox =>
            checkbox.closest("tr").querySelector("td:nth-child(3)").textContent.trim()
        );

        console.log(booksToReturn);

        try {
            const response = await fetch("/api/books/return", {
                method: "POST",
                headers: { "Content-Type": "application/json" },
                body: JSON.stringify(booksToReturn),
                credentials: "include"
            });

            if (!response.ok) {
                const errorText = await response.text();
                throw new Error(errorText);
            }

            alert("Books returned successfully.");
            loadBooks();
        } catch (error) {
            console.error("Error returning books:", error);
            alert(`Error returning books: ${error.message}`);
        }

        
    }

    function displaySearchedBooks(books) {
        const tableBody = document.getElementById("book-table-body");

        // Clear existing rows (optional, depends on desired behavior)
        tableBody.innerHTML = "";

        console.log("Loading book quantity: ", books.length);

        books.forEach(book => {
            const row = document.createElement("tr");
            row.innerHTML = `
                <td><input type="checkbox" class="select-book"></td>
                <td><a href="#" class="update-book-link" data-title="${book.title}" data-author="${book.author}" data-year="${book.year}" data-quantity="${book.quantity}">
                    ${book.title}
                </a></td>
                <td style="display: none">${book.id}</td>
                <td>${book.author}</td>
                <td>${book.year}</td>
                <td>${book.quantity}</td>
            `;

            // Highlight borrowed books
            if (book.isBorrowed) {
                row.style.backgroundColor = 'orange'; // Highlight borrowed books with a different color
            }

            tableBody.appendChild(row);
        });
    }
});
