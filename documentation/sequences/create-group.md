# Create New Birthday Group

Paste this at [sequencediagram.org](https://sequencediagram.org) to render.

```
title Create New Birthday Group

participant User
participant React
participant API
participant DB

User->React: visits /mygroup (no groupId)
React->User: render CreateGroupView (name input + button)
User->React: enters group name "Smith Family"
User->React: clicks "Create My Group"
React->React: generate UUID v7 (e.g. 019xxx...)
React->React: store name in sessionStorage[group_name_{uuid}]
React->React: navigate to /mygroup/{uuid}
React->API: GET /api/group/{uuid}
API->DB: SELECT FROM Groups WHERE Id = ?
DB->API: (empty result set)
API->React: 404 Not Found
React->API: POST /api/group { groupId: uuid, name: "Smith Family" }
API->DB: INSERT INTO Groups (Id, Name, CreatedAtUtc)
DB->API: 1 row affected
API->React: 201 Created { id, name, createdAtUtc, memberCount: 0 }
React->React: clear sessionStorage entry
React->React: setState(NEW_GROUP) → show AddPersonForm (isFirstMember=true)
```
