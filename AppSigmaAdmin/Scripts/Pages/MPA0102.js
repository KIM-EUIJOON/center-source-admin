
function updateshoplist() {

    var facilityId = $('select[id=layer1] option:selected').val();
    if (facilityId == "1") {
        $('#shoplist1').hide();
        $('#shoplist2').show();
    }
    else {
        //$('#shoplist1').hidden = false
        //$('#shoplist2').hidden = true;
    }
    //var count = $('select[id="layer2"]').children().length;
    //for (var cnt = 0; cnt < count; cnt++) {
    //    var a = $('select[id="layer2"] option:eq(' + cnt + ')');
    //    if (a.val() == facilityId) {
    //        //a.show();
            //a[0].hidden = false;

            //$('select[id="layer2"]').children()[cnt].hidden = true;
        //}
        //else {
        //    a[0].hidden = true;
            //a.hide();
            //alert('test');
            //a.prop('selected', true);
            //$('select[id="layer2"]').children()[cnt].hidden = false;
    //    }
    //}
};
