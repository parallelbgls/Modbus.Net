$(document).ready(function () {
    setInterval("startRequest()", 1000);
});

function startRequest() {
    var url = "/Home/Get";
    $.ajax({
        type: "get",
        async: true,
        url: url,
        timeout: 1000,
        success: function (datas) {
            $.each(datas, function(index) {
                $('#' + datas[index].id).html(datas[index].value);
            });
        },
        error: function () {
        }
    });
}