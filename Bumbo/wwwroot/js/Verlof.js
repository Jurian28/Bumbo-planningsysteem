document.querySelectorAll('.status-box').forEach(box => {
    box.addEventListener('click', function() {
        if (this.classList.contains('approved')) {
            this.classList.remove('approved');
            this.classList.add('rejected');
            this.style.backgroundColor = 'red'; // Change to red for rejected
        } else if (this.classList.contains('rejected')) {
            this.classList.remove('rejected');
            this.classList.add('pending');
            this.style.backgroundColor = 'grey'; // Change to yellow for pending
        } else {
            this.classList.remove('pending');
            this.classList.add('approved');
            this.style.backgroundColor = 'green'; // Change to green for approved
        } 
    });
});

document.addEventListener("DOMContentLoaded", function () {
    const form = document.querySelector("form[asp-action='Create']");
    form.addEventListener("submit", function (event) {
        const validationErrors = document.querySelectorAll(".text-danger");
        let hasErrors = false;

        validationErrors.forEach(error => {
            if (error.textContent.trim() !== "") {
                hasErrors = true;
                showPopup(error.textContent, false);
            }
        });

        if (hasErrors) {
            event.preventDefault();
        }
    });
});

function showPopup(message, isSuccess) {
    const popup = document.getElementById("popup-message");
    if (!popup) {
        console.error("Popup element not found");
        return;
    }
    popup.textContent = message;
    popup.className = `popup ${isSuccess ? "success" : "error"}`;
    popup.classList.remove("hidden");
    setTimeout(() => {
        popup.classList.add("hidden");
    }, 3000);
}