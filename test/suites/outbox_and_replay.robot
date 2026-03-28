*** Settings ***
Resource    ../resources/common.resource

*** Test Cases ***
Lab Outbox Status Endpoint
    Create Session For    lab    ${LAB_URL}
    ${resp}=    GET On Session    lab    /v1/outbox/status
    Assert Status    ${resp}    200

Sync Outbox And Consumer Replay Endpoints
    Create Session For    sync    ${SYNC_URL}
    ${headers}=    Auth Headers
    ${resp}=    POST On Session    sync    /v1/outbox/replay-deadletters    headers=${headers}
    Run Keyword If    '${INTERNAL_TOKEN}' == ''    Assert Status    ${resp}    200
    Run Keyword If    '${INTERNAL_TOKEN}' != ''    Assert Status    ${resp}    200

    ${resp}=    POST On Session    sync    /v1/consumer/replay-deadletters    headers=${headers}
    Run Keyword If    '${INTERNAL_TOKEN}' == ''    Assert Status    ${resp}    200
    Run Keyword If    '${INTERNAL_TOKEN}' != ''    Assert Status    ${resp}    200
