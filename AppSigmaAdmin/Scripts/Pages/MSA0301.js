$(document).ready(function () {
    //ダイアログ作成
    $("#dialog").dialog({
        autoOpen: false,
        modal: true,
        width: 450,
        model: true,
        buttons: {
            "更新": function () {
                $("#Edit").submit();
            },
            "追加": function () {
                $('form').attr('action', $('#IpAdd').val());
                $('form').submit();
            },
            "キャンセル": function () {
                $(this).dialog("close");
            }
        }
    });
});

$(window).load(function () {
    //IPアドレスリスト更新判定(画面表示後に通知)
    if ($('#UpdateFlag').val() == 1) {
        window.alert('リストの更新に成功しました。');
    }
    else if ($('#UpdateFlag').val() == 2) {
        window.alert('リストの更新に失敗しました。');
    };
});

function IPListDelete(NetAddress) {
    if (confirm('IPアドレス「' + NetAddress + '」を認証用IPアドレスリストから削除します。よろしいですか?')) {
        //メッセージボックス「はい」選択時の処理
        $.ajax({
            type: 'POST',
            url: $('#IpDelete').val(),
            data: NetAddress,
            dataType: 'text',
            success: function (data) {
                window.alert('リストの更新に成功しました。');
                window.location.href = $('#CreateUrl').val();
            }
        });
    }
    else {
        //メッセージボックス「いいえ」選択時の処理
        window.alert('削除処理をキャンセルしました。');
    }
};

function IPListEdit(NetAddress, IPAddressName, EnvDev, EnvPre, EnvProd) {
    $("#dialog").dialog("open");
    //更新処理のため追加ボタンを押下不可に設定する
    $(".ui-dialog-buttonpane button:contains('追加')").button("disable");
    $(".ui-dialog-buttonpane button:contains('更新')").button("enable");
    $("#NetAdd").attr('readonly', true);
    $("#NetAdd").val(NetAddress);
    $("#IPAddName").val(IPAddressName);
    var dev = EnvDev;
    if (dev == "○") { $("#EnvDev").prop('checked', true); }
    else { $("#EnvDev").prop('checked', false); }
    var pre = EnvPre;
    if (pre == "○") { $("#EnvPre").prop('checked', true); }
    else { $("#EnvPre").prop('checked', false); }
    var prod = EnvProd;
    if (prod == "○") { $("#EnvProd").prop('checked', true); }
    else { $("#EnvProd").prop('checked', false); }
};

function IPListSet() {
    $("#dialog").dialog("open");
    //新規追加処理のため更新ボタンを押下不可に設定する
    $(".ui-dialog-buttonpane button:contains('更新')").button("disable");
    $(".ui-dialog-buttonpane button:contains('追加')").button("enable");
    //ダイアログ入力内容を初期化
    $("#NetAdd").attr('readonly', false);
    $("#NetAdd").val("");
    $("#IPAddName").val("");
    $("#EnvDev").prop('checked', false);
    $("#EnvPre").prop('checked', false);
    $("#EnvProd").prop('checked', false);
}