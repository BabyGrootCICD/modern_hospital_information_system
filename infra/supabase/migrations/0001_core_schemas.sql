create schema if not exists identity;
create schema if not exists clinical;
create schema if not exists transfer;
create schema if not exists sync;
create schema if not exists audit;

create extension if not exists pgcrypto;

create table if not exists identity.users (
  user_id uuid primary key default gen_random_uuid(),
  username text not null unique,
  role text not null,
  tenant text not null,
  created_at timestamptz not null default now()
);

create table if not exists clinical.patient_charts (
  chart_id uuid primary key default gen_random_uuid(),
  patient_id text not null,
  visit_date date not null,
  department text not null,
  doctor_id text not null,
  summary text not null,
  created_at timestamptz not null default now()
);

create table if not exists transfer.transfer_jobs (
  transfer_id uuid primary key default gen_random_uuid(),
  patient_id text not null,
  source_site text not null,
  target_site text not null,
  payload_ref text,
  status text not null,
  created_at timestamptz not null default now(),
  updated_at timestamptz not null default now()
);

create table if not exists sync.sync_jobs (
  sync_id uuid primary key default gen_random_uuid(),
  mode text not null check (mode in ('export', 'import')),
  hospital text not null,
  payload_ref text,
  status text not null,
  created_at timestamptz not null default now(),
  updated_at timestamptz not null default now()
);

create table if not exists audit.integrity_events (
  event_id uuid primary key default gen_random_uuid(),
  aggregate_id text not null,
  aggregate_type text not null,
  digest_sha256 text not null,
  merkle_root text,
  chain_tx_hash text,
  created_at timestamptz not null default now()
);

create index if not exists idx_patient_charts_patient on clinical.patient_charts (patient_id);
create index if not exists idx_transfer_jobs_status on transfer.transfer_jobs (status);
create index if not exists idx_sync_jobs_status on sync.sync_jobs (status);
create index if not exists idx_integrity_events_aggregate on audit.integrity_events (aggregate_type, aggregate_id);
