
    function Reload(data) {
        $('#add1').val(data.Add1);
        $('#add2').val(data.Add2);
        $('#add3').val(data.Add3);
        $('#ans').val(data.Ans);
    }

    function DisplayError(data) {
        alert(data);
    }