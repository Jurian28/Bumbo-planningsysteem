document.addEventListener('DOMContentLoaded', function () {
    const dayCards = document.querySelectorAll('.day-card');

    dayCards.forEach(card => {
        const toggle = card.querySelector('input[type="checkbox"]');
        const dayText = card.querySelector('h3');

        const dayValue = card.querySelector('input[name$=".Day"]').value;
        const availabilityId = card.querySelector('input[name$=".AvailabilityId"]').value;

        // Set initial color
        dayText.style.color = toggle.checked ? 'lightblue' : '#333';

        toggle.addEventListener('change', function () {
            dayText.style.color = toggle.checked ? 'lightblue' : '#333';

            fetch('/Beschikbaarheid/UpdateAvailability', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                    'X-CSRF-TOKEN': document.querySelector('input[name="__RequestVerificationToken"]').value
                },
                body: JSON.stringify({
                    AvailabilityId: availabilityId,
                    Day: dayValue,
                    IsAvailable: toggle.checked
                })
            }).then(response => {
                if (!response.ok) {
                    console.error('Failed to update availability');
                }
            }).catch(error => {
                console.error('Error:', error);
            });
        });
    });
});