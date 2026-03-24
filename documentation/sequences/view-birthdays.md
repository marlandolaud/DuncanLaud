# View Upcoming Birthdays

Paste this at [sequencediagram.org](https://sequencediagram.org) to render.

```
title View Upcoming Birthdays

participant User
participant React
participant API
participant DB

User->React: navigates to /mygroup/{groupId}
React->React: isValidGuid check — passes
React->API: GET /api/group/{groupId}
API->DB: SELECT FROM Groups WHERE Id = ?
alt Group found
  DB->API: Group row { id, name, createdAtUtc, memberCount }
  API->React: 200 { id, name, createdAtUtc, memberCount }
  React->API: GET /api/group/{groupId}/birthdays
  API->DB: SELECT FROM Persons WHERE GroupId = ? (IX_Person_GroupId)
  DB->API: Person rows
  API->API: BirthdayCalculator.GetUpcoming(members, daysAhead=60, today)
  API->API: compute DaysUntilBirthday for each member
  API->API: filter where DaysUntil <= 60
  API->API: sort ascending by DaysUntil
  API->React: 200 [ { personId, displayName, birthDateDisplay, daysUntil, imageThumbnail } ]
  React->React: setState(LANDING) — render BirthdayList
  React->User: show upcoming birthdays sorted by proximity
else Group not found (404)
  API->React: 404 Not Found
  React->React: check sessionStorage for pending group name
  note over React: If name present → create group flow\nIf not present → show error message
end
```
