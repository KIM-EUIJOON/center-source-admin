if (self === top) {
    var antiClickjack = document.getElementById("antiClickjack");
    antiClickjack.parentNode.removeChild(antiClickjack);
} else {
    top.location = self.location;
}
$(document).ready(function () {
    if ($('#DebugFlag').val() != $('#Debug').val()) {
        $('table tr a, tfoot a, td a').on('click', function () {
            //sigma付与
            location.href = "/sigma" + $(this).attr('href');

            return false;
        });
    }
    $('#AllClear').on('click', function () {
        window.location.href = $('#CreateUrl').val();
    });
});
//日付入力補助カレンダー(抽出開始日)
function SetcalenderFrom() {
    $('#datetimepickerFrom').datepicker(
        {
            maxDate: '0y',
            dateFormat: 'yy/mm/dd',
            changeYear: true,
            changeMonth: true,
            showMonthAfterYear: true,
            showButtonPanel: true,
        }
    );
    $('#datetimepickerFrom').datepicker("show");
}

//日付入力補助カレンダー(抽出終了日)
function SetcalenderTo() {
    $('#datetimepickerTo').datepicker({
        maxDate: '0y',
        dateFormat: 'yy/mm/dd',
        changeYear: true,
        changeMonth: true,
        showMonthAfterYear: true,
        showButtonPanel: true,
    });
    $('#datetimepickerTo').datepicker("show");
};
//Today押下で今日の日付を入力する
$.datepicker._gotoToday = function (id) {
    $(id).datepicker('setDate', new Date()).datepicker('hide').blur().change();
};
//ユーザーID検索からページ読み込み時にアプリ種別を設定する
$(window).on('load', function () {
    var modelval = $('#ModelAplType').val();
    var selectval = $('#AplTypeSelect').val();
    if (modelval != null && modelval != selectval) {
        if (selectval == "1") {
            $('#AplTypeSelect').val("-");
        }
    }
});