# Robot Framework Full Test Suite

## Install

```bash
python3 -m venv .venv
source .venv/bin/activate
pip install -r test/requirements.txt
```

## Run

```bash
robot -d test/results test/suites
```

## Environment Variables

- `IDENTITY_URL` (default `http://localhost:8081`)
- `LAB_URL` (default `http://localhost:8084`)
- `SYNC_URL` (default `http://localhost:8085`)
- `AUDIT_URL` (default `http://localhost:8091`)
- `PROOF_URL` (default `http://localhost:8092`)
- `INTERNAL_TOKEN` (optional bearer token for write endpoints protected by internal JWT middleware)
