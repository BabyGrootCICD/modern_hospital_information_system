if (!window['ajax']) {
    window['ajax'] = function (inUrl, inData, inSuccessFunc, inErrorFunc, inTimeout) {

        var timeoutPeriod = inTimeout || 2000000;
        timeoutPeriod = timeoutPeriod <= 0 ? 2000000 : timeoutPeriod;
        var relativeUrl = APPLICATION_ROOT + inUrl;
        var ajaxexec =
            $.ajax({
                cache: false,
                url: relativeUrl,
                data: inData,
                type: "POST",
                async: true,
                dataType: 'text',
                timeout: timeoutPeriod,
                success: function (msg) {
                    if (inSuccessFunc != null)
                        inSuccessFunc(msg);
                },
                error: function (xhr, ajaxOptions, thrownError) {
                    if (xhr.status == "401") {
                        window.location.reload();
                    }
                    else {
                        if (inErrorFunc != null)
                            inErrorFunc();
                        setTimeout(function () {
                            window.location.href = "/Login/logout";
                            layer.alert("Server Error,Please reflesh and try again!!" + "(Error:" + xhr.status + " - " + thrownError + "!!)", {
                                skin: 'layui-layer-lan',
                                closebtn: 1,
                                anim: 5,
                                icon: 2,
                                btn: ['Yes', 'cancel'],
                                title: 'Edit Message'
                            }, function (index) {
                                layer.close(index);
                            });
                        }, 0);
                    }
                }
            });
        return ajaxexec;
    };
}

if (!window['ajaxGet']) {
    window['ajaxGet'] = function (inUrl, inData, inSuccessFunc, inErrorFunc, inTimeout) {

        var timeoutPeriod = inTimeout || 20000;
        timeoutPeriod = timeoutPeriod <= 0 ? 20000 : timeoutPeriod;
        var relativeUrl = APPLICATION_ROOT + inUrl;
        var ajaxexec =
            $.ajax({
                cache: false,
                url: relativeUrl,
                data: inData,
                type: "GET",
                async: true,
                dataType: 'text',
                timeout: timeoutPeriod,
                success: inSuccessFunc,
                error: function (xhr, ajaxOptions, thrownError) {
                    if (inErrorFunc != null)
                        inErrorFunc();
                    setTimeout(function () {
                        window.location.href = "/Login/logout";
                        layer.alert("Server Error,Please reflesh and try again!!" + "(Error:" + xhr.status + " - " + thrownError + "!!)", {
                            skin: 'layui-layer-lan',
                            closebtn: 1,
                            anim: 5,
                            icon: 2,
                            btn: ['Yes', 'cancel'],
                            title: 'Edit Message'
                        }, function (index) {
                            layer.close(index);
                        });
                    }, 0);
                }
            });
        return ajaxexec;
    };
}