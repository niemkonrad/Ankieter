// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.

function deleteQuestion(i) {
    $.ajax({
        url: deleteUrl,
        type: 'POST',
        data: {
            id: i
        },
        success: function () {
            window.location.reload();
        }
    });
}
function populateForm(i) {
    console.log("populateForm called with id:", i);
    $.ajax({
        url: populateFormUrl,
        type: 'GET',
        data: { id: i },
        dataType: 'json',
        success: function (response) {
            console.log(response); // debug
            $("#Question_Name").val(response.name);
            $("#Question_Id").val(response.id);
            $("#Question_Type").val(response.type);
            $("#Question_IsActive").prop("checked", response.isActive);
            $("#form-button").val("Update Question");
            $("#form-action").attr("action", updateUrl);
        },
        error: function (xhr, status, error) {
            alert("Błąd przy pobieraniu danych: " + error);
        }
    });
}