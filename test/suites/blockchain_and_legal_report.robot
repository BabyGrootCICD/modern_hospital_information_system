*** Settings ***
Resource    ../resources/common.resource

*** Test Cases ***
Proof Anchor And Verify
    Create Session For    proof    ${PROOF_URL}
    ${payload}=    Create Dictionary    merkle_root=abc123merkle    network=polygon
    ${resp}=    POST On Session    proof    /v1/proofs/anchor    json=${payload}
    Assert Status    ${resp}    200
    ${body}=    Set Variable    ${resp.json()}
    Dictionary Should Contain Key    ${body}    anchor
    ${anchor}=    Get From Dictionary    ${body}    anchor
    ${anchor_id}=    Get From Dictionary    ${anchor}    anchor_id
    ${verify}=    GET On Session    proof    /v1/proofs/verify/${anchor_id}
    Assert Status    ${verify}    200

Audit Event And Legal Report
    Create Session For    audit    ${AUDIT_URL}
    ${payload}=    Create Dictionary    aggregate_id=agg-robot-1    aggregate_type=order    payload={\"sample\":true}
    ${resp}=    POST On Session    audit    /v1/integrity/events    json=${payload}
    Assert Status    ${resp}    200
    ${report}=    GET On Session    audit    /v1/integrity/legal-report/agg-robot-1
    Assert Status    ${report}    200
    ${body}=    Set Variable    ${report.json()}
    Dictionary Should Contain Key    ${body}    report_signature
