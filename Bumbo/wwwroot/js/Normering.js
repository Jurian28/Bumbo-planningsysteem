$(document).ready(function () {
    // AJAX voor het bewerken van Norms
    $('.norm-form').on('submit', function (event) {
        event.preventDefault();
        var form = $(this);
        $.ajax({
            url: '@Url.Action("EditNorm", "Normering")',
            type: 'POST',
            data: form.serialize(),
            success: function (response) {
                if (response.success) {
                    form.find('.save-button').text('@Resource.OpslaanButton');
                } else {
                    console.log('Fout bij het opslaan van norm.');
                }
            },
            error: function () {
                console.log('Er is een fout opgetreden.');
            }
        });
    });

    // AJAX voor het bewerken van Templates
    $('.template-form').on('submit', function (event) {
        event.preventDefault();
        var form = $(this);
        $.ajax({
            url: '@Url.Action("EditTemplate", "Normering")',
            type: 'POST',
            data: form.serialize(),
            success: function (response) {
                if (response.success) {
                    form.find('.save-button').text('@Resource.OpslaanButton');
                } else {
                    console.log('Fout bij het opslaan van template.');
                }
            },
            error: function () {
                console.log('Er is een fout opgetreden.');
            }
        });
    });
});