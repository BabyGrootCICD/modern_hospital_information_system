insert into clinical.patient_charts (patient_id, visit_date, department, doctor_id, summary)
values
  ('P001', current_date - 1, 'OPD', 'D001', 'Follow-up visit with stable status'),
  ('P002', current_date - 2, 'EMG', 'D007', 'Emergency intake resolved and discharged')
on conflict do nothing;
