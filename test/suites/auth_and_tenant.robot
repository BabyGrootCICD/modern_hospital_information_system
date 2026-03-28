*** Settings ***
Resource    ../resources/common.resource

*** Test Cases ***
Identity Login And Validate
    Create Session For    identity    ${IDENTITY_URL}
    ${payload}=    Create Dictionary    username=robot-user    role=admin    tenant=${TENANT_ID}
    ${resp}=    POST On Session    identity    /v1/auth/login    json=${payload}
    Assert Status    ${resp}    200
    ${body}=    Set Variable    ${resp.json()}
    Dictionary Should Contain Key    ${body}    token

Lab Transfer Create With Tenant
    Create Session For    lab    ${LAB_URL}
    ${headers}=    Auth Headers
    ${payload}=    Create Dictionary    patient_id=P-001    source_site=A    target_site=B    payload_ref=ref://robot
    ${resp}=    POST On Session    lab    /v1/transfers    json=${payload}    headers=${headers}
    Run Keyword If    '${INTERNAL_TOKEN}' == ''    Assert Status    ${resp}    201
    Run Keyword If    '${INTERNAL_TOKEN}' != ''    Assert Status    ${resp}    201

Sync Job Create With Tenant
    Create Session For    sync    ${SYNC_URL}
    ${headers}=    Auth Headers
    ${payload}=    Create Dictionary    mode=export    hospital=HOSP-A    payload_ref=blob://robot
    ${resp}=    POST On Session    sync    /v1/sync/jobs    json=${payload}    headers=${headers}
    Assert Status    ${resp}    201
