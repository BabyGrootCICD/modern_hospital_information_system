*** Settings ***
Resource    ../resources/common.resource

*** Test Cases ***
All Services Healthcheck
    Create Session For    identity    ${IDENTITY_URL}
    Create Session For    lab    ${LAB_URL}
    Create Session For    sync    ${SYNC_URL}
    Create Session For    audit    ${AUDIT_URL}
    Create Session For    proof    ${PROOF_URL}

    ${resp}=    GET On Session    identity    /healthz
    Assert Status    ${resp}    200
    ${resp}=    GET On Session    lab    /healthz
    Assert Status    ${resp}    200
    ${resp}=    GET On Session    sync    /healthz
    Assert Status    ${resp}    200
    ${resp}=    GET On Session    audit    /healthz
    Assert Status    ${resp}    200
    ${resp}=    GET On Session    proof    /healthz
    Assert Status    ${resp}    200
