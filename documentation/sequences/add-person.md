# Add Person to Group (with Image Upload)

Paste this at [sequencediagram.org](https://sequencediagram.org) to render.

```
title Add Person to Group

participant User
participant React
participant API
participant Storage
participant DB

note over React: User is on AddPersonForm

opt User picks a photo
  User->React: selects image file
  React->API: GET /api/image/upload-url?fileName=photo.jpg
  API->Storage: request pre-signed PUT URL (MinIO dev / CF Images prod)
  Storage->API: { uploadUrl, deliveryUrl }
  API->React: { uploadUrl, deliveryUrl }
  React->Storage: PUT file bytes to uploadUrl (direct, no .NET proxy)
  Storage->React: 200 OK
  React->React: store deliveryUrl in form state
end

User->React: fills form (firstName, lastName, birthDate, ...)
User->React: clicks "Add Member"
React->React: client-side validation (length, birthDate in past)
React->API: POST /api/group/{groupId}/person\n{ firstName, lastName, preferredName, birthDate, imageThumbnail }
API->API: DataAnnotations model validation
API->API: check ImageThumbnail domain allowlist
API->API: PersonService.AddPersonAsync(command)
API->API: PersonValidator — length rules (2-100 chars)
API->API: PersonValidator — birthDate in past, year >= 1900
API->API: ProfanityChecker — check firstName, lastName, preferredName
API->DB: INSERT INTO Persons
DB->API: 1 row affected
API->React: 201 Created { id, firstName, lastName, ... }
React->React: setState(LANDING) → reload birthday list
```
