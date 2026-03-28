var changeObj = { Med: false, NonMed: false, ClinicRemark: false, Management: false, Diagnosis: false };



$(document).ready(function () {

    var connection = new signalR.HubConnectionBuilder().withUrl(encodeURI("/chatHub")).build();
    //與Server建立連線
    connection.start().then(function () {
        console.warn("Hub connection successful!");
    }).catch(function (err) {
        alert('connection error: ' + err.toString());
    });


    // Tagify
    $('[name=tags]').tagify();

    // iCheck
    $("input").iCheck({
        labelHover: true,
        cursor: true,
        checkboxClass: "icheckbox_flat-pink",
        radioClass: "iradio_square-blue",
        increaseArea: "15%",
    });


    $(".lock-btn").click(function () {

        $(this).closest('tr')
            .find('span:not([data-status])').toggleClass('badge-primary badge-success')
            .find('i').toggleClass('fa-lock fa-unlock')

        $(this).closest('tr').find('td>input,select')
            .prop('disabled', (i, v) => !v);

    });


    // Reload Menu Basic Data
    $('.Reload-btn').click(function () {
        fullscreenLoading(true);

        $('#ICDMenuSearch').val('');
        $('#MedMenuSearch').val('');
        $('#NonMedMenuSearch').val('');

        RenderIcdMenu();
        RenderMedMenu();
        RenderLabMenu();
        RenderExamMenu();
        RenderPathMenu();
        RenderSupplyMenu();

        // Auto Selected ICD Row 0
        $('#Categoryul > li').not('.headli').eq(0).click();

        // Scroll to the bottom 5px,Show the next 100 IcdNodes.
        $('#CategoryitemCol').on('scroll', function () {
            let viewH = $(this).height(); //可見高度
            let contentH = $(this).get(0).scrollHeight; //内容高度
            let scrollTop = $(this).scrollTop(); //滾動高度

            //到達底部 20% 時,往下長100筆內容
            if (contentH - viewH - scrollTop <= contentH * 0.2) {

                let ShowCount = 0;
                $('.SearchNode,.MenuNode > li').each(function () {
                    if ($(this).css('display') == 'none') {
                        $(this).show();
                        ShowCount++;
                    }

                    if (ShowCount == 100) { return false; }
                });
                ShowCount = 0;
            }
        });

        fullscreenLoading(false);
    });


    // 螢幕暫存、完成看診、取消
    $('.cancel-btn').click(function () {

        //var hasChagne = false;

        //$.each(changeObj, function (k, v) {
        //    if (v) {
        //        hasChagne = v;
        //    }
        //});

        //if (hasChagne) {
        //    $(window).bind('beforeunload', function () { return true; });
        //} else {
        //    $(window).unbind('beforeunload');
        //}

        fn_CheckBeforeUnload();

        var sourceType = $('#clinicSourceType').val();


        window.location.href = '/HisOrder/HisOrder/Index?sourceType=' + sourceType;
    });
    $('.save-btn,.examining-btn,.completed-btn,.observation-btn').click(function (e) {
        var cm_data_Context = CKEDITOR.instances.editor_clinic_remarks.getData();
        var ConvertIntoString = cm_data_Context.toString();
        var RemovingEntity = ConvertIntoString.replace(/<[^>]*(>|$)|&nbsp;|(\r\n|\n|\r)/g, '').trim();
        RemovingEntity = RemovingEntity.replace(/\s/g, '');

        var icdR = $("#icdR").text();

        ////alert(cm_data_Context);
       ////var cmdata = cm_data_Context.slice(5, cm_data_Context.length - 7);

        console.log("-------------- the value of the ICD ---------------");
        console.log(icdR);

        var cudur = $(".tagify__tag-text").text()
        if (cudur == "" && icdR == "Y") {
            layer.alert('Please select at least one diagnosis', {
                skin: 'layui-layer-lan',
                closebtn: 3,
                anim: 5,
                icon: 7,
                btn: ['OK'],
                title: 'Message',
                OK: function (index) {
                    layer.close(index);
                }
            });
        } 
        else if (RemovingEntity.replace(/\s/g, '').length <= 50) {
            layer.msg('Please write in the clinic remark over 50 letters', {
                skin: 'layui-layer-lan',
                closebtn: 3,
                anim: 5,
                icon: 7,
                btn: ['OK'],
                title: 'Message',
                OK: function (index) {
                    layer.close(index);
                }
            });//alert end
        } else {

            //2023.10.01 add by 1050325 Prevent Data Loss
            if (document.readyState !== 'complete') {
                alert('The page has not finished loading. Please try again later.');
                return;
            }

            fullscreenLoading(true);
            var order_status = "";
            var clinic_status = "";

            if ($(this).hasClass('completed-btn')) {
                order_status = "confirm";
                clinic_status = "Completed";
            }
            else if ($(this).hasClass('observation-btn')) {
                order_status = "confirm";
                clinic_status = "Observation";
            }


            var dfd_diagnosis = $.Deferred();
            var dfd_med = $.Deferred();
            var dfd_nonmed = $.Deferred();
            var dfd_clinicStatus = $.Deferred();
            var dfd_soap = $.Deferred();

            $.when(
                fn_ChangeClinicStatus(dfd_clinicStatus, clinic_status),
                fn_SaveDiagnosis(dfd_diagnosis, order_status),
                fn_SaveMedOrderByElements(dfd_med, order_status),
                fn_SaveNonMedOrderByElements(dfd_nonmed, order_status),
                fn_SaveSoapData(dfd_soap, order_status)
            )
                .done(
                    function (r_clinic, r_diagnosis, r_med, r_nonmed, r_soap) {
                        if (r_diagnosis.isSuccess == true) {
                            console.log('r_diagnosis saved');
                        }

                        if (r_med.isSuccess == true) {
                            fn_reload_medorder_partial_view();
                            console.log('r_med saved');
                        }

                        if (r_nonmed.isSuccess == true) {
                            fn_reload_nonmedorder_partial_view();
                            console.log('r_nonmed saved');
                        }

                        if (r_soap.isSuccess == true) {
                            var dfd = $.Deferred();
                            $.when(getSoapDataByVersion(dfd)).done(
                                function (r_version) {
                                    if (r_version.isSuccess == true) {
                                        var data = JSON.parse(r_version.returnValue);
                                        if (data != null && data != undefined && data.length != 0) {
                                            $(".version-select").empty();
                                            $.each(data, function (idx, val) {
                                                $(".version-select").append($('<option>', { value: val.VersionCode, text: val.Des }));
                                            });

                                            initSoapData($(".version-select option:selected").val());
                                        } else {
                                            initSoapData();
                                        }
                                    }
                                })
                            // getSoapDataByVersion();
                            /*                initSoapData($(".version-select option:selected").val());*/
                            console.log('r_soap saved');
                        }

                        if (r_clinic.isSuccess == true) {
                            //nonmed status

                            console.log('r_clinic saved');
                        }

                        if (r_diagnosis.isSuccess == true && r_med.isSuccess == true && r_nonmed.isSuccess == true && r_clinic.isSuccess == true && r_soap.isSuccess == true) {
                            fn_ResetChangeObj();
                            fn_reload_nonmedorder_partial_view();
                            fn_reload_medorder_partial_view();
                            fullscreenLoading(false);
                            layer.msg('successfully saved', { time: 1500, icon: 1 });

                            fn_OrderPrint(order_status);
                        } else {
                            if (r_diagnosis.isSuccess == false) {
                                layer.alert("Save diagnosis order Error：" + r_diagnosis.Message, { icon: 2, title: "Error" });
                            }
                        }
                    });
        }
    });

    //叫號
    $('.call-btn').click(function () {

        var inhospid = $('#patientInhospid').val();
        var vURL = "/HisOrder/Ajax/callLight";
        var vData = {
            inhospid: inhospid
        };

        var vSuccessFunc = function (msg) {
            var objResult = JSON.parse(msg);
            if (objResult.isSuccess == true) {
                layer.msg(objResult.Message);
                var regData = JSON.parse(objResult.returnValue);

                if (connection.state == "Disconnected") {
                    console.warn("Hub connection again!");
                }

                connection.invoke("SendMessage", regData.RegRoomNo, regData.RegSeqNo.toString()).catch(function (err) {
                    console.log('傳送錯誤: ' + err.toString());
                });
            }
            else {
                layer.msg("叫號失敗" + objResult.Message);
            }
        };
        var vErrorFunc = function () {
            layer.msg("叫號失敗");
        };

        ajax(vURL, vData, vSuccessFunc, vErrorFunc);
    });

});

//更改診間狀態
function fn_ChangeClinicStatus(dfd2, status) {
    var transfer = $("#checkbox_transfer:checked").length;
    var _vTransfer_des = $("#transfer_des").val();
    //var _vTransfer = "N";
    //if (transfer != undefined && transfer > 0) {
    //    _vTransfer = "Y";
    //}

    var transferCode = $(".radio_transfer:checked").attr("data-trans-code");

    var vURL = "";
    var vData = {
        /*transfer: _vTransfer,*/
        transfer: transferCode,
        transfer_des: _vTransfer_des
    };
    if (status == "Completed") {
        vURL = "/HisOrder/Ajax/EndVisitSave";
    }
    else if (status == "Observation") {

        vURL = "/HisOrder/Ajax/ObserveSave";
    }
    else {
        vURL = "/HisOrder/Ajax/ScreenSave";
    }

    var vSuccessFunc = function (msg) {
        var objResult = JSON.parse(msg);
        if (objResult.isSuccess == true) {
            dfd2.resolve(objResult);
        }
        else {
            layer.msg("螢幕暫存執行失敗" + objResult.Message);
        }
    };
    var vErrorFunc = function () {
        layer.msg("螢幕暫存執行失敗");
    };

    ajax(vURL, vData, vSuccessFunc, vErrorFunc);
    return dfd2.promise();
}

function fn_ResetChangeObj() {
    $.each(changeObj, function (k, v) {
        changeObj[k] = false;
    });

}


function fn_CheckBeforeUnload() {

    var hasChagne = false;

    $.each(changeObj, function (k, v) {
        if (v) {
            hasChagne = v;
        }
    });

    if (hasChagne) {
        $(window).unbind('beforeunload');
        $(window).bind('beforeunload', function () { return true; });
    } else {
        $(window).unbind('beforeunload');
    }
}

function fn_OrderPrint(order_status) {

    $.ajax({
        type: 'POST',
        url: Root + "Print/PrintForm",
        data: {
            inStatus: order_status,
        },
        async: false,
        dataType: 'text',
        success: function (result) {
            $('#PrintForm').html(result);
            $('#PrintMenu').modal('show');
        },
        error: function (xhr, ajaxOptions, thrownError) { },
        complete: function (XMLHttpRequest, textStatus) { }
    });
}