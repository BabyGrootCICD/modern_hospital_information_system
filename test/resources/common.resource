*** Settings ***
Library    RequestsLibrary
Library    Collections
Library    String

*** Variables ***
${IDENTITY_URL}      %{IDENTITY_URL=http://localhost:8081}
${LAB_URL}           %{LAB_URL=http://localhost:8084}
${SYNC_URL}          %{SYNC_URL=http://localhost:8085}
${AUDIT_URL}         %{AUDIT_URL=http://localhost:8091}
${PROOF_URL}         %{PROOF_URL=http://localhost:8092}
${TENANT_ID}         t-hospital-a
${INTERNAL_TOKEN}    %{INTERNAL_TOKEN=}

*** Keywords ***
Create Session For
    [Arguments]    ${alias}    ${base_url}
    Create Session    ${alias}    ${base_url}

Auth Headers
    [Arguments]    ${content_type}=application/json
    ${headers}=    Create Dictionary    Content-Type=${content_type}    X-Tenant-ID=${TENANT_ID}
    IF    '${INTERNAL_TOKEN}' != ''
        Set To Dictionary    ${headers}    Authorization=Bearer ${INTERNAL_TOKEN}
    END
    [Return]    ${headers}

Assert Status
    [Arguments]    ${resp}    ${expected}
    Should Be Equal As Integers    ${resp.status_code}    ${expected}
