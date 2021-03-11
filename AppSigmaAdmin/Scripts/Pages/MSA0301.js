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

function IPListEdit(NetAddress, IPAddressName) {
    $("#dialog").dialog("open");
    //更新処理のため追加ボタンを押下不可に設定する
    $(".ui-dialog-buttonpane button:contains('追加')").button("disable");
    $(".ui-dialog-buttonpane button:contains('更新')").button("enable");
    $("#NetAdd").val(NetAddress);
    $("#IPAddName").val(IPAddressName);
    $("#BeforeNetAdd").val(NetAddress);
};

function IPListSet() {
    $("#dialog").dialog("open");
    //新規追加処理のため更新ボタンを押下不可に設定する
    $(".ui-dialog-buttonpane button:contains('更新')").button("disable");
    $(".ui-dialog-buttonpane button:contains('追加')").button("enable");
    //ダイアログ入力内容を初期化
    $("#NetAdd").val("");
    $("#IPAddName").val("");
}