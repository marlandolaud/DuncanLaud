# Replace date input with three dropdown selects (Year, Month, Day)

## Context
The native `<input type="date">` for birthday is inconsistent across browsers/devices and not the best UX. Replace it with three `<select>` dropdowns (Year, Month name, Day) displayed in a single row. This is frontend-only — the backend still receives `YYYY-MM-DD` string via FormData.

## Files to modify
1. **`src/duncanlaud-react/src/pages/group/AddPersonForm.jsx`**
2. **`src/duncanlaud-react/src/pages/group/EditPersonForm.jsx`**
3. **`src/duncanlaud-react/src/pages/group/MyGroupPage.css`**

## Implementation

### 1. Form state change (both forms)
Replace `birthDate: ''` with three separate fields:
```js
birthYear: '', birthMonth: '', birthDay: ''
```
In EditPersonForm, parse the loaded `person.birthDate` (`'YYYY-MM-DD'`) into the three parts on load.

### 2. Constants (inline in each form or a shared util)
- `MONTHS` array: `['January', 'February', ..., 'December']` (full names, index+1 = month number)
- Year options: range from current year down to 1900 (descending so recent years are first)
- Day options: dynamically computed 1–28/29/30/31 based on selected year+month

### 3. Dropdown JSX (both forms)
Replace the single `<input type="date">` with:
```jsx
<div className="add-person-form__date-row">
  <select id="birthYear" value={form.birthYear} onChange={set('birthYear')}>
    <option value="">Year</option>
    {years.map(y => <option key={y} value={y}>{y}</option>)}
  </select>
  <select id="birthMonth" value={form.birthMonth} onChange={set('birthMonth')}>
    <option value="">Month</option>
    {MONTHS.map((m, i) => <option key={i} value={i+1}>{m}</option>)}
  </select>
  <select id="birthDay" value={form.birthDay} onChange={set('birthDay')}>
    <option value="">Day</option>
    {days.map(d => <option key={d} value={d}>{d}</option>)}
  </select>
</div>
```

### 4. Day count logic
Compute `days` array using `new Date(year, month, 0).getDate()` to get correct day count (handles Feb leap years). If year or month not yet selected, default to 31. If currently selected day exceeds new max, clear day.

### 5. Assemble date for submission
In `handleSubmit`, combine the three fields back into ISO format before sending:
```js
const birthDate = `${form.birthYear}-${String(form.birthMonth).padStart(2,'0')}-${String(form.birthDay).padStart(2,'0')}`;
```

### 6. Validation update
Replace the current `new Date(form.birthDate)` validation with:
- All three selects are required (year, month, day)
- Construct the date and check: must be in the past, year ≥ 1900
- Single error message displayed below the row

### 7. CSS (MyGroupPage.css)
Add new rule for the row layout:
```css
.add-person-form__date-row {
  display: flex;
  gap: 0.5rem;
}
.add-person-form__date-row select {
  flex: 1;
  padding: 0.65rem 0.85rem;
  border: 1.5px solid #ccc;
  border-radius: 6px;
  font-size: 1rem;
  transition: border-color 0.2s;
  background: #fff;
  appearance: auto;
}
.add-person-form__date-row select:focus {
  outline: none;
  border-color: #8b6b9e;
  box-shadow: 0 0 0 3px rgba(139,107,158,0.15);
}
```
Year select gets slightly more flex space since 4-digit years are wider. Month gets the most since full names are longest.

## Verification
1. `npx vite build` — no errors
2. `dotnet test` — all 183 tests still pass (no backend changes)
3. Manual test: select a date, submit form, verify `YYYY-MM-DD` arrives at API
4. Verify day dropdown updates when changing month (e.g. Feb shows 28/29 days)
5. Verify EditPersonForm pre-populates the three dropdowns from existing data
