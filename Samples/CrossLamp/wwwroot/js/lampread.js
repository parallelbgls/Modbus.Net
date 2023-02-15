var colorArr = ["#f05f2a", "#DDFF00", "#01a252"],
    liNodes = document.getElementsByClassName("light"),
    index = 0,
    index2 = 3;

$(document).ready(function () {
    setInterval(readLamp, 1000);
});

function readLamp() {
    var url = "/Home/GetLamp";
    $.ajax({
        type: "get",
        async: true,
        url: url,
        timeout: 1000,
        success: function (datas) {
            [].forEach.call(liNodes, function (v) {
                v.style.backgroundColor = "lightgray";
            });
            switch (datas.mainLamp) {
            case "Red":
            {
                liNodes[index].style.backgroundColor = colorArr[index];
                break;
            }
            case "Yellow":
            {
                liNodes[index + 1].style.backgroundColor = colorArr[index + 1];
                break;
            }
            case "Green":
            {
                liNodes[index + 2].style.backgroundColor = colorArr[index + 2];
                break;
            }
            }
            switch (datas.subLamp) {
            case "Red":
            {
                liNodes[index2].style.backgroundColor = colorArr[index];
                break;
            }
            case "Yellow":
            {
                liNodes[index2 + 1].style.backgroundColor = colorArr[index + 1];
                break;
            }
            case "Green":
            {
                liNodes[index2 + 2].style.backgroundColor = colorArr[index + 2];
                break;
            }
            }
        },
        error: function() {
        }
    });
}