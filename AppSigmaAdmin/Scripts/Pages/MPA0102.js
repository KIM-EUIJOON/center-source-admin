function updateshoplist() {
    // 表示/非表示の切替
    var facilityId = $('select[id=layer1] option:selected').val();
    if (facilityId == "") {
        $('#shoplist').prop('disabled', true);
    }
    else {
        $('#shoplist').prop('disabled', false);
    }
    // ShopListの動的取得
    var URL = $('#SelectShopList').val();
    var str = $('#language').val();
    $.getJSON(URL + '/?id=' + $('select[id=layer1] option:selected').val() + '&language=' + str, function (data) {
        var items = "";
        $.each(data, function (i, shop) {
            if (shop.Selected == true) {
                items += "<option selected='" + shop.Selected + "' value='" + shop.Value + "'>" + shop.Text + "</option>";
            }
            else {
                items += "<option value='" + shop.Value + "'>" + shop.Text + "</option>";
            }
        });
        $('#shoplist').html(items);
    });
};