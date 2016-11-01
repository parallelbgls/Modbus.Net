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
                $('#' + datas[index].Id).html(datas[index].Value);
            });
        },
        error: function () {
        }
    });
}