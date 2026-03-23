const BASE = '/api/group';

async function request(url, options = {}) {
  const res = await fetch(url, options);
  if (!res.ok) {
    const body = await res.json().catch(() => ({}));
    const err = new Error(body.error || `HTTP ${res.status}`);
    err.status = res.status;
    throw err;
  }
  return res.status === 204 ? null : res.json();
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

export function fetchBirthdays(groupId) {
  return request(`${BASE}/${groupId}/birthdays`);
}

export function personImageUrl(groupId, personId) {
  return `${BASE}/${groupId}/person/${personId}/image`;
}
