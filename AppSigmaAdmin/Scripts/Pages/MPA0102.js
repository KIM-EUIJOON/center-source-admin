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
    var URL = '/MPA0102/SelectShopList';
    $.getJSON(URL + '/' + $('select[id=layer1] option:selected').val(), function (data) {
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