/// <reference path="..\lib\jquery\dist\jquery.js" />

var oldNonMed = [];

function getRandomData(x) {

    var index = Math.floor(Math.random() * x);

    var list = [
        { orderplaid: '', plancode: '303456', plandes: 'CBC-1', qty: 1 },
        { orderplaid: '', plancode: '111111', plandes: 'Chest-Pa', qty: 1 },
        { orderplaid: '', plancode: '222222', plandes: 'Ca', qty: 1 },
        { orderplaid: '', plancode: '333333', plandes: 'Na', qty: 1 },
        { orderplaid: '', plancode: '777777', plandes: 'MRI', qty: 1 },
        { orderplaid: '', plancode: '33072B', plandes: 'CT', qty: 1 }
    ];

    var newList = [];
    newList.push(list.at(index));

    return newList;
};



//初始化
function init_NonMedOrder() {

    //table
    var table = $("#nonmedorder_table");

    //changeFlag重製
    changeObj.NonMed = false;


    //fn_addNonMedorder(list)
    //顯示筆數
    fn_showNonMedOrderCount();
    //設定icheck屬性
    fn_seticheck(table);
    //置底
    /*  table.scrollTop(table.height());*/


    $(".add_nonmedorders").click(function () {
        fn_addNonMedorder(getRandomData(5))
    });


    $(".delete_nonmedorders").click(function () {
        fn_deleteNonMedOrders(table)
    });

    $(".completed_nonmedorders").click(function () {
    });


    $("#NonMedPrint").click(function () {
        //取得tr
        var checklist = table.find("input:checked:not(.all):not(.nonmedorder_tr_templete)");
        var idlist = [];
        if (checklist.length > 0) {
            checklist.each(function (idx, val) {

                var orderplaid = $(val).closest('tr').attr("data-orderplan-id");
                var modifytype = $(val).closest('tr').attr('data-modify-type');


                if (orderplaid == "-1" || modifytype == "U") {

                    layer.alert('Only archived orders may be reprinted', {
                        skin: 'layui-layer-lan',
                        closebtn: 1,
                        anim: 5,
                        icon: 2,
                        btn: ['OK'],
                        title: 'Message'
                    }, function (index) {
                        layer.close(index);

                    });
                    idlist.length = 0;
                    return false;

                } else {
                    idlist.push(orderplaid);
                    /* $(val).closest('tr:not(.nonmedorder_tr_templete)').attr("hidden", true).attr("data-modify-type", "D");*/
                }
            });


            if (idlist != undefined && idlist.length > 0) {
                fn_RePrint(idlist);
            }

            //changeObj.NonMed = true;
            //fn_CheckBeforeUnload();
        } else
        {
            layer.alert('1. Select the item you want to reprint. </br> 2. Only archived orders may be reprinted.', {
                skin: 'layui-layer-lan',
                closebtn: 1,
                anim: 5,
                icon: 2,
                btn: ['OK'],
                title: 'Message'
            }, function (index) {
                layer.close(index);

            });
            
            
        }



        //取消checkbox
        table.find('input.all').iCheck('uncheck');
        //計數
        fn_showNonMedOrderCount();

    });


    //用不到了
    //$(".save_nonmedorders").click(function (event, status) {
    //    fn_SaveNonMedOrderByElements(status);
    //});

    table.find('input[data-plan-qty]').change(function () {

        $(this).attr('data-plan-qty', $(this).val());
        fn_checkNonMedOrderChange($(this));
    });

    table.find('input[data-remark]').change(function () {

        $(this).attr('data-remark', $(this).val());
        fn_checkNonMedOrderChange($(this));
    });

    table.find('td > select[data-location-code]').change(function () {
        $(this).attr('data-location-code', $(this).val());
        fn_checkNonMedOrderChange($(this));
    });

    table.find('input[data-plan-qty]').focusout(function (e) {
        checkInput($(this));
    });

    table.find('input[data-plan-qty]').keyup(function (e) {
        if (e.keyCode == 13) {

            var result = checkInput($(this));
            if (!result) {
                return;
            }

            //fn_checkMedOrderChange($(this));

            var tt = $(this).closest('tr').next();
            if ($(this).closest('tr').next().length > 0) {
                $(this).closest('tr').next().find('input[data-plan-qty]').focus().select();
            } else {
                $(this).closest('tr').siblings('tr:visible:first').find('input[data-plan-qty]').focus().select();
            }
        }
    });


    //正則
    function checkInput(e) {
        var input = $(e).val();
        var pattern = /^(\d{1,3}(\.\d{1,2})?|0(\.\d{1,2})?)?$/;
        if (pattern.test(input)) {
            if ($(e).hasClass('border-danger')) {
                $(e).removeClass('border-danger');
            }

            return true;

        } else {

            if ($(e).hasClass('border-danger') == false) {
                $(e).addClass('border-danger');
            }

            $(e).val(function (index, value) {
                return value.replace(value, '');
            });

            $(e).trigger('change');

            layer.tips('<div style="font-size:16px;">Input error: </div> <div style="font-size:14px;"> Please enter the correct value </div>', e, { tips: [2, "#dc3545"], time: 3000 });

            return false;
        }
    }


    fn_reload_old_nonmedorder_data();

    fn_CheckBeforeUnload();

}

function fn_checkNonMedOrderChange(element) {

    var changeFlag = false;

    var $tr = $(element).closest('tr');

    var oldData = oldNonMed.find(({ Orderplanid }) => Orderplanid == $tr.attr('data-orderplan-id'));

    var newOrder = {
        Orderplanid: $tr.attr("data-orderplan-id"),
        PlanCode: $tr.find("td[data-plan-code]").text(),
        PlanDes: $tr.find("td[data-plan-des]").text(),
        QtyDose: $tr.find("input[data-plan-qty]").attr('data-plan-qty'),
        LocationCode: $tr.find("td > select[data-location-code]").attr("data-location-code"),
        Remark: $tr.find("input[data-remark]").attr('data-remark'),
    };

    //欄位是否異動-擴充
    if (oldData != undefined &&
        ((oldData.QtyDose != newOrder.QtyDose) ||
        (oldData.LocationCode != newOrder.LocationCode) ||
        (oldData.Remark != newOrder.Remark))
    ) {
        changeFlag = true;
    }


    if (changeFlag) {
        var $span = $tr.find('td>span[data-status]');

        if ($span.attr('data-status') == '0' || $span.attr('data-status') == '2') {
            if ($span.attr('data-status') == '0') {
                $span.removeClass('badge-gray');
            } else {
                $span.removeClass('badge-success');
            }

            $span.addClass('badge-warning').text('Change');

            $tr.attr("data-modify-type", 'U');

        }
    } else {

        var $span = $tr.find('td>span[data-status]');

        if ($span.hasClass('badge-warning')) {
            $span.removeClass('badge-warning');
            if ($span.attr('data-status') == '0') {
                $span.addClass('badge-gray').text('Examining');
            } else {
                $span.addClass('badge-success').text('Cfm');
            }

            //還原，不異動
            $tr.attr("data-modify-type", '');
        }
    }

    changeObj.NonMed = changeFlag;
    fn_CheckBeforeUnload();

    return changeFlag;

}

//刪除
function fn_deleteNonMedOrders(table) {

    //取得tr
    var checklist = table.find("input:checked:not(.all):not(.nonmedorder_tr_templete)");

    if (checklist.length > 0) {
        checklist.each(function (idx, val) {

            var orderplaid = $(val).closest('tr').attr("data-orderplan-id");

            if (orderplaid == "") {
                $(val).closest('tr:not(.nonmedorder_tr_templete)').remove();
            } else {
                $(val).closest('tr:not(.nonmedorder_tr_templete)').attr("hidden", true).attr("data-modify-type", "D");
            }
        });

        changeObj.NonMed = true;
        fn_CheckBeforeUnload();
    }



    //取消checkbox
    table.find('input.all').iCheck('uncheck');
    //計數
    fn_showNonMedOrderCount();
}

function fn_seticheck(table) {
    // iCheck
    table.find("input").iCheck({
        labelHover: true,
        cursor: true,
        checkboxClass: "icheckbox_flat-pink",
        radioClass: "iradio_square-blue",
        increaseArea: "15%"
    });

    var checkAll = table.find('input.all:not(.nonmedorder_tr_templete)');
    var checkboxes = table.find('input.check');

    checkAll.on('ifChecked ifUnchecked', function (event) {
        if (event.type == 'ifChecked') {
            checkboxes.iCheck('check');
        } else {
            checkboxes.iCheck('uncheck');
        }
    });

    checkboxes.on('ifChanged', function (event) {

        //勾選變色
        var tr = $(this).closest('tr');
        const value = $(this).iCheck('update')[0].checked;
        if (value) {
            tr.css({ 'background-color': '#FFF0F5' });
        } else {
            tr.css({ 'background-color': '' });
        }


        if (checkboxes.filter(':checked').length == checkboxes.length) {
            checkAll.prop('checked', 'checked');
        } else {
            checkAll.removeProp('checked');
        }
        checkAll.iCheck('update');
    });
}

function fn_showNonMedOrderCount() {
    var dataCount = $("#nonmedorder_table").find("tbody > tr:visible").length
    var strCount = (dataCount == 0) ? "0" : dataCount.toString();
    //顯示筆數
    $("#nonmedorder_count").text(strCount);
}

function fn_reload_nonmedorder_partial_view() {

    var nonmedorder_section = $(".nonmedorder-section");
    nonmedorder_section.empty();
    var vURL = "/HisOrder/NonMed/ReloadPartialView";
    var vSuccessFunc = function (result) {
        /*layer.load();*/
        nonmedorder_section.html(result);
        //重新綁定事件
        init_NonMedOrder();
        /*layer.closeAll('loading');*/
    };
    var vErrorFunc = function () {
        nonmedorder_section.html("載入失敗");
        return this;
    };
    ajaxGet(vURL, null, vSuccessFunc, vErrorFunc);

}


function fn_reload_old_nonmedorder_data() {

    var inhospid = $('#patientInhospid').val();
    var vURL = "/HisOrder/NonMed/GetHisOrderPlan";
    var vData = {
        inhospid: inhospid
    };
    var vSuccessFunc = function (msg) {

        var result = JSON.parse(msg);
        if (result.isSuccess == true) {
            var data = JSON.parse(result.returnValue);
            if (data != null && data != undefined) {
                oldNonMed.length = 0;
                $.each(data, function (k, v) {
                    oldNonMed.push(v);
                });
            }
        }
    };
    var vErrorFunc = function () {

        return this;
    };
    ajaxGet(vURL, vData, vSuccessFunc, vErrorFunc);

}


//畫面add 
function fn_addNonMedorder(DataList) {

    if (DataList !== 'undefined' && DataList.length > 0) {

        var table = $("#nonmedorder_table");
        $.each(DataList, function (index, value) {

            console.log(value.orderplaid);
            console.log(value.plancode);
            console.log(value.plandes);

            var maxseq = table.find("tbody > tr:not(.nonmedorder_tr_templete):last > td[data-seq-no]").attr("data-seq-no");
            if (maxseq == undefined) {
                maxseq = 1;
            } else {
                maxseq++;
            }

            var $el = $(".nonmedorder_tr_templete:first")
                .clone(true, true)
                .removeClass("nonmedorder_tr_templete")
                .attr('hidden', false)
                .attr("data-modify-type", "I");

            $el.find("input.nonmedorder_tr_templete").removeClass("nonmedorder_tr_templete");


            //$el.attr("data-orderplan-id", value.orderplaid)
            //    .attr("data-status", "")
            //    .attr("data-plan-code", value.plancode)
            //    .attr("data-plan-des", value.plandes)
            //    .attr("data-plan-qty", value.qty)
            //    .attr("data-seq-no", "78");


            $el.find("td[data-orderplan-id]").attr('data-orderplan-id', value.orderplaid).text(value.orderplaid);
            $el.find("td[data-plan-code]").attr('data-plan-code', value.plancode).text(value.plancode);
            $el.find("td[data-plan-des]").attr('data-plan-des', value.plandes).text(value.plandes);
            $el.find("td>span[data-status]").attr('data-status', "").addClass("badge-pink").text("?");
            $el.find("input[data-plan-qty]").attr('data-plan-qty', value.qty).val(value.qty);     

            var hplanTxt = "";
            var hplanClassColor = ""
            switch (value.NonMedItemType)
            {
                case "Lab":
                    hplanTxt = "Laboratory";
                    hplanClassColor = "badge-purple"
                    break;
                case "Exam":
                    hplanTxt = "Radiology";
                    hplanClassColor = "badge-info"
                    break;
                case "Path":
                    hplanTxt = "Pathology";
                    hplanClassColor = "badge-master"
                    break;
                case "Blood":
                    hplanTxt = "Blood";
                    hplanClassColor = "badge-danger"
                    break;
                default:
                    hplanTxt = "Material";
                    hplanClassColor = "badge-warning"
                    break;
            }

            $el.find("td[data-hplan-type]").attr('data-hplan-type', value.NonMedItemType);

            $el.find("td[data-hplan-type] > span").text(hplanTxt).addClass(hplanClassColor);

            if (value.NonMedItemType == "Blood") {
                $el.find("td[data-hplan-Spec]").attr('data-hplan-Spec', "Route IV").text("Route IV");
            } else {
                $el.find("td[data-hplan-Spec]").attr('data-hplan-Spec', "N/A").text("N/A");
            }


            //hidden
            $el.find("td[data-seq-no]").attr('data-seq-no', maxseq);
            $el.find("td[data-plan-type]").attr('data-plan-type', value.NonMedItemType);
            console.log($el);

            table.find("tbody").append($el);

        });

        changeObj.NonMed = true;
        fn_CheckBeforeUnload();
        fn_seticheck(table);
        fn_showNonMedOrderCount();
        table.scrollTop(table.height());

    }

}

//取得前端資訊，將醫令寫入DB
function fn_SaveNonMedOrderByElements(dfd1, status) {

    //var dfd = $.Deferred();

    var trList = $("#nonmedorder_table").find("tbody > tr:not(.nonmedorder_tr_templete)");
    const orderAry = [];
    trList.each(function (idx, val) {

        var modifyType = $(val).attr("data-modify-type");
        var obj = {
            Orderplanid: $(val).attr("data-orderplan-id"),
            PlanCode: $(val).find("td[data-plan-code]").text(),
            PlanDes: $(val).find("td[data-plan-des]").text(),
            QtyDose: $(val).find("input[data-plan-qty]").attr('data-plan-qty'),
            Remark: $(val).find("input[data-remark]").val(),
            SeqNo: $(val).find("td[data-seq-no]").attr("data-seq-no"),
            LocationCode: $(val).find("td > select[data-location-code]").attr("data-location-code"),
            ModifyType: modifyType,
            HplanType: $(val).find("td[data-plan-type]").attr("data-plan-type")
        };
        orderAry.push(obj);
    });


    var vURL = "/HisOrder/NonMed/ModifyNonMedOrder";
    var vData = {
        inOrder: JSON.stringify(orderAry),
        inStatus: status
    };
    var vSuccessFunc = function (msg) {

        var objResult = JSON.parse(msg);
        /*fn_reload_nonmedorder_partial_view();*/
        //return objResult;
        console.log('nonmed ok');
        dfd1.resolve(objResult);
    };
    var vErrorFunc = function (xhr) {

        return xhr;
    };

    ajax(vURL, vData, vSuccessFunc, vErrorFunc);


    return dfd1.promise();
}


//醫令寫入DB
//function fn_SaveNonMedOrder(orderList, status) {

//    var vURL = "/HisOrder/NonMed/ModifyNonMedOrder";
//    var vData = {
//        inOrder: orderList,
//        inStatus: status
//    };
//    var vSuccessFunc = function (msg) {

//        var objResult = JSON.parse(msg);
//        return objResult;
//    };
//    var vErrorFunc = function (xhr) {

//        return objResult;
//    };
//    ajax(vURL, vData, vSuccessFunc, vErrorFunc);
//}