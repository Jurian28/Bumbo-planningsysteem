document.addEventListener('DOMContentLoaded', function () {
    const dropdownButtons = document.querySelectorAll('.nav-item.dropdown');

    dropdownButtons.forEach(button => {
        const dropdownMenu = button.querySelector('.dropdown-menu');
        const dropdownArrow = button.querySelector('.dropdown-arrow');

        // Toggle dropdown on click
        button.addEventListener('click', function (event) {
            event.stopPropagation(); // Prevent event bubbling
            const isExpanded = button.getAttribute('aria-expanded') === 'true';

            // Close all dropdowns first
            closeAllDropdowns();

            // Toggle the clicked dropdown
            if (!isExpanded) {
                button.setAttribute('aria-expanded', 'true');
                dropdownMenu.style.display = 'block';
                dropdownArrow.style.transform = 'rotate(180deg)';
            }
        });
    });

    // Close dropdowns when clicking outside
    document.addEventListener('click', function () {
        closeAllDropdowns();
    });

    // Function to close all dropdowns
    function closeAllDropdowns() {
        dropdownButtons.forEach(button => {
            const dropdownMenu = button.querySelector('.dropdown-menu');
            const dropdownArrow = button.querySelector('.dropdown-arrow');
            button.setAttribute('aria-expanded', 'false');
            dropdownMenu.style.display = 'none';
            dropdownArrow.style.transform = 'rotate(0deg)';
        });
    }
});