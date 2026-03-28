alter table identity.users enable row level security;
alter table clinical.patient_charts enable row level security;
alter table transfer.transfer_jobs enable row level security;
alter table sync.sync_jobs enable row level security;
alter table audit.integrity_events enable row level security;

-- Baseline policies: authenticated users can read/write only through service role in current phase.
drop policy if exists users_service_role on identity.users;
create policy users_service_role on identity.users
  for all
  to service_role
  using (true)
  with check (true);

drop policy if exists charts_service_role on clinical.patient_charts;
create policy charts_service_role on clinical.patient_charts
  for all
  to service_role
  using (true)
  with check (true);

drop policy if exists transfers_service_role on transfer.transfer_jobs;
create policy transfers_service_role on transfer.transfer_jobs
  for all
  to service_role
  using (true)
  with check (true);

drop policy if exists sync_service_role on sync.sync_jobs;
create policy sync_service_role on sync.sync_jobs
  for all
  to service_role
  using (true)
  with check (true);

drop policy if exists integrity_service_role on audit.integrity_events;
create policy integrity_service_role on audit.integrity_events
  for all
  to service_role
  using (true)
  with check (true);
