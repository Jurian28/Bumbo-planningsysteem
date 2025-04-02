// document.addEventListener("DOMContentLoaded", function () {
//     console.log("DOM is geladen");
//     const forms = document.querySelectorAll("form[asp-action='TakeOverShift']");
//     forms.forEach(form => {
//         form.addEventListener("submit", async function (event) {
//             event.preventDefault();
//             console.log("Formulier verstuurd:", form);
//             const formData = new FormData(form);
//             try {
//                 const response = await fetch(form.action, {
//                     method: "POST",
//                     body: formData,
//                     headers: {
//                         "X-Requested-With": "XMLHttpRequest"
//                     }
//                 });
//                 console.log("Fetch response ontvangen:", response);
//                 if (!response.ok) throw new Error("Serverfout");
//                 const result = await response.json();
//                 console.log("Serverrespons JSON:", result);
//                 showPopup(result.message, result.success);
//             } catch (error) {
//                 console.error("Fout tijdens fetch:", error);
//                 showPopup("Er is een fout opgetreden", false);
//             }
//         });
//     });
//
//     const ruilverzoekButton = document.getElementById("ruilverzoek-button");
//     if (ruilverzoekButton) {
//         ruilverzoekButton.addEventListener("click", function () {
//             showPopup("Ruilverzoek aangevraagd", true);
//         });
//     }
// // });
//
// function showPopup(message, isSuccess) {
//     console.log("Popup wordt aangeroepen:", message, isSuccess);
//     const popup = document.getElementById("popup-message");
//     if (!popup) {
//         console.error("Popup-element niet gevonden");
//         return;
//     }
//     popup.textContent = message;
//     popup.className = `popup ${isSuccess ? "success" : "error"}`;
//     popup.classList.remove("hidden");
//     setTimeout(() => {
//         popup.classList.add("hidden");
//     }, 3000);
// }