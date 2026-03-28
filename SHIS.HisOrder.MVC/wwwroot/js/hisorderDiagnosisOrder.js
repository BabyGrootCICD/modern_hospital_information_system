
let Diagnosisinput = document.querySelector('input[name=Diagnosis-drag-sort]'),
    tagify = new Tagify(Diagnosisinput, { // Top Most to inital tagify
        userInput: false, // Cancel Input to add tag
    });

function init_Diagnosis() {
    reloadOrderDiagnosis();
}

function reloadOrderDiagnosis() {

    tagify.removeAllTags();

    let ResultOrderData = [];

    $.ajax({
        type: 'POST',
        url: Root + "Diagnosis/getICDHisorderplanData",
        data: {},
        async: false,
        dataType: 'json',
        success: function (result) {
            ResultOrderData = result;
        },
        error: function (xhr, ajaxOptions, thrownError) { },
        complete: function (XMLHttpRequest, textStatus) { }
    });

    let IcdItemList = [];

    if (ResultOrderData != null) {
        for (let i = 0; i < ResultOrderData.length; i++) {

            let MedicalItem = {
                orderplanid: ResultOrderData[i].orderplanid,
                plancode: ResultOrderData[i].planCode,
                plandes: ResultOrderData[i].planDes,
                qty: 1,
            };

            IcdItemList.push(MedicalItem);
        };
    }

    fn_addIcdOrder(IcdItemList);
}

function fn_addIcdOrder(DataList) {

    //let DataList = {
    //    orderplanid: $OrderplanId,
    //    plancode: $PlanCode,
    //    plandes: $PlanDes,
    //    qty: $Qty,
    //};

    if (DataList !== 'undefined' && DataList.length > 0) {
        for (let i = 0; i < DataList.length; i++) {
            let TagName = DataList[i].plancode + ' ' + DataList[i].plandes;
            tagify.addTags([{ value: TagName, orderplanid: DataList[i].orderplanid, plancode: DataList[i].plancode, plandes: DataList[i].plandes }]);
        }
    }
}

/* 儲存診斷前的檢核 */
function DiagnosisCheckBeforeSend() {
    let DiagnosisOrderlist = tagify.value;

    if (DiagnosisOrderlist.length == 0) {
        layer.alert("Order need at least one Diagnosis.", { icon: 2, title: "Error" });
        return false;
    }
}

function fn_SaveDiagnosis(dfd1, status) {
    // tagify.value[0].value    Main Diagnosis Value
    let DiagnosisOrderlist = tagify.value;

    let Orderlist = [];

    if (DiagnosisOrderlist.length > 0) {

        for (let i = 0; i < DiagnosisOrderlist.length; i++) {
            let obj = {
                Orderplanid: DiagnosisOrderlist[i].orderplanid,
                PlanCode: DiagnosisOrderlist[i].plancode,
                PlanDes: DiagnosisOrderlist[i].plandes,
                SeqNo: i,
            };
            Orderlist.push(obj);
        }
    }

    // ajax
    let DiagnosisResult = {};

    $.ajax({
        type: 'POST',
        url: Root + "Diagnosis/ModifyICDOrder",
        data: {
            inOrder: Orderlist,
            inStatus: status
        },
        async: false,
        dataType: 'json',
        success: function (result) {
            DiagnosisResult = result;
        },
        error: function (xhr, ajaxOptions, thrownError) { },
        complete: function (XMLHttpRequest, textStatus) { }
    });

    reloadOrderDiagnosis();
    dfd1.resolve(DiagnosisResult);

    return dfd1.promise();
}

// Dragdown update DOM
function onDragEnd(elm) {
    tagify.updateValueByDOMTags()
}

$(document).ready(function () {

    init_Diagnosis();

    // DragSort.js
    let dragsort = new DragSort(tagify.DOM.scope, {
        selector: '.' + tagify.settings.classNames.tag,
        callbacks: {
            dragEnd: onDragEnd
        }
    })
})
