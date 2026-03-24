const BASE = '/api/group';

async function request(url, options = {}) {
  const res = await fetch(url, options);
  if (!res.ok) {
    const body = await res.json().catch(() => ({}));
    const err = new Error(body.error || `HTTP ${res.status}`);
    err.status = res.status;
    throw err;
  }
  if (res.status === 204) return null;
  const ct = res.headers.get('content-type') || '';
  if (!ct.includes('application/json')) {
    throw new Error(`Unexpected response (${res.status}): expected JSON`);
  }
  return res.json();
}

export function fetchGroup(groupId) {
  return request(`${BASE}/${groupId}`);
}

export function createGroup(groupId, name) {
  return request(BASE, {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({ groupId, name }),
  });
}

export function addPerson(groupId, { firstName, lastName, preferredName, birthDate, photoFile }) {
  const form = new FormData();
  form.append('firstName', firstName);
  form.append('lastName', lastName);
  if (preferredName) form.append('preferredName', preferredName);
  form.append('birthDate', birthDate);
  if (photoFile) form.append('image', photoFile);

  return request(`${BASE}/${groupId}/person`, { method: 'POST', body: form });
}

export function fetchPersons(groupId) {
  return request(`${BASE}/${groupId}/persons`);
}

export function fetchPerson(groupId, personId) {
  return request(`${BASE}/${groupId}/person/${personId}`);
}

export function updatePerson(groupId, personId, { firstName, lastName, preferredName, birthDate, photoFile, removeImage }) {
  const form = new FormData();
  form.append('firstName', firstName);
  form.append('lastName', lastName);
  if (preferredName) form.append('preferredName', preferredName);
  form.append('birthDate', birthDate);
  form.append('removeImage', removeImage ? 'true' : 'false');
  if (photoFile) form.append('image', photoFile);

  return request(`${BASE}/${groupId}/person/${personId}`, { method: 'PUT', body: form });
}

export function fetchBirthdays(groupId) {
  return request(`${BASE}/${groupId}/birthdays`);
}

export function personImageUrl(groupId, personId) {
  return `${BASE}/${groupId}/person/${personId}/image`;
}
