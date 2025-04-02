var selectedWeek = getCurrentWeekNumber();
var currentWeek = getCurrentWeekNumber();

var selectedYear = new Date().getFullYear();
var currentYear = new Date().getFullYear();

function getCurrentWeekNumber() {
    var currentDate = new Date();
    var oneJan = new Date(currentDate.getFullYear(), 0, 1);
    var numberOfDays = Math.floor((currentDate - oneJan) / (24 * 60 * 60 * 1000));
    var currentWeekNumber = Math.ceil((currentDate.getDay() + 1 + numberOfDays) / 7);
    return currentWeekNumber;
}

function previousWeek() {

    if (selectedWeek > currentWeek || selectedYear > currentYear) {
        selectedWeek--;
    }


    
    if (selectedWeek < 1) {
        selectedWeek = 52;  // Terug naar week 52 als week 1 wordt overschreden
        selectedYear--;    // Jaar met 1 omlaag
    }
    updateWeekAndYear();
}

function nextWeek() {
    // Bereken het maximale weeknummer, inclusief mogelijke jaarovergang
    var maxFutureWeek = currentWeek + 10;
    var maxFutureYear = currentYear;

    // Als het maximum aantal weken verder dan week 52 gaat, moeten we het jaar verhogen
    if (maxFutureWeek > 52) {
        maxFutureWeek -= 52;
        maxFutureYear++;
    }

    // Verhoog het geselecteerde weeknummer alleen als het niet meer dan 10 weken in de toekomst is
    if (selectedYear < maxFutureYear || (selectedYear === maxFutureYear && selectedWeek < maxFutureWeek)) {
        selectedWeek++;
    }

    // Controleer of het weeknummer overschrijdt en update het jaar indien nodig
    if (selectedWeek > 52) {
        selectedWeek = 1;
        selectedYear++;
    }

    updateWeekAndYear();
}


function updateWeekAndYear() {
    var weekNumberElement = document.getElementById("week-number");
    var yearElement = document.getElementById("year");

    weekNumberElement.innerText = "Week " + selectedWeek;
    yearElement.innerText = selectedYear;

    document.getElementById('weekForm').value = selectedWeek;
    document.getElementById('yearForm').value = selectedYear;
}



function getDateOfIsoWeek() {
    week = selectedWeek;
    year = selectedYear;

    if (week < 1 || week > 53) {
        throw new RangeError("ISO 8601 weeks are numbered from 1 to 53");
    } else if (!Number.isInteger(week)) {
        throw new TypeError("Week must be an integer");
    } else if (!Number.isInteger(year)) {
        throw new TypeError("Year must be an integer");
    }

    const simple = new Date(year, 0, 1 + (week - 1) * 7);
    const dayOfWeek = simple.getDay();
    const isoWeekStart = simple;

    // Get the Monday past, and add a week if the day was
    // Friday, Saturday or Sunday.

    isoWeekStart.setDate(simple.getDate() - dayOfWeek + 1);
    if (dayOfWeek > 4) {
        isoWeekStart.setDate(isoWeekStart.getDate() + 7);
    }

    // The latest possible ISO week starts on December 28 of the current year.
    if (isoWeekStart.getFullYear() > year ||
        (isoWeekStart.getFullYear() == year &&
            isoWeekStart.getMonth() == 11 &&
            isoWeekStart.getDate() > 28)) {
        throw new RangeError(`${year} has no ISO week ${week}`);
    }
    return isoWeekStart;
}

// Set the initial week number and year
updateWeekAndYear();