import { useState, useRef, useMemo } from 'react';
import { addPerson } from '../../services/groupApi';
import { sanitizeTextInput } from '../../utils/sanitize';
import compressImage from '../../utils/compressImage';
import defaultAvatar from '../../assets/default-avatar.svg';

const ALLOWED_TYPES = ['image/jpeg', 'image/png', 'image/webp', 'image/gif'];
const MONTHS = [
  'January','February','March','April','May','June',
  'July','August','September','October','November','December',
];
const CURRENT_YEAR = new Date().getFullYear();
const YEARS = Array.from({ length: CURRENT_YEAR - 1900 + 1 }, (_, i) => CURRENT_YEAR - i);
const SENTINEL_BIRTH_YEAR = 1904;

export default function AddPersonForm({ groupId, onSuccess, onCancel, isFirstMember }) {
  const [form, setForm] = useState({
    firstName: '',
    lastName: '',
    preferredName: '',
    birthYear: '',
    birthMonth: '',
    birthDay: '',
    email: '',
  });
  const [photoFile, setPhotoFile] = useState(null);
  const [photoPreview, setPhotoPreview] = useState(null);
  const [errors, setErrors] = useState({});
  const [submitting, setSubmitting] = useState(false);
  const [submitError, setSubmitError] = useState('');
  const fileInputRef = useRef(null);
  const cameraInputRef = useRef(null);

  /** For text name fields: sanitize to A-Za-z0-9 only on every keystroke. */
  function setName(field) {
    return (e) => {
      setForm((prev) => ({ ...prev, [field]: sanitizeTextInput(e.target.value) }));
      setErrors((prev) => ({ ...prev, [field]: '' }));
    };
  }

  /** For non-sanitized fields (e.g. email, date selects). */
  function set(field) {
    return (e) => {
      const value = e.target.value;
      setForm((prev) => {
        const next = { ...prev, [field]: value };
        // If year or month changed, clamp or clear the day if it exceeds new max
        if (field === 'birthYear' || field === 'birthMonth') {
          const y = field === 'birthYear' ? Number(value) : Number(prev.birthYear);
          const m = field === 'birthMonth' ? Number(value) : Number(prev.birthMonth);
          if (y && m && prev.birthDay) {
            const maxDay = new Date(y, m, 0).getDate();
            if (Number(prev.birthDay) > maxDay) next.birthDay = '';
          }
        }
        return next;
      });
      setErrors((prev) => ({ ...prev, [field]: '', birthDate: '' }));
    };
  }

  const daysInMonth = useMemo(() => {
    const y = Number(form.birthYear) || SENTINEL_BIRTH_YEAR;
    const m = Number(form.birthMonth);
    const max = m ? new Date(y, m, 0).getDate() : 31;
    return Array.from({ length: max }, (_, i) => i + 1);
  }, [form.birthYear, form.birthMonth]);

  async function handlePhotoChange(e) {
    const file = e.target.files?.[0];
    if (!file) return;

    if (!ALLOWED_TYPES.includes(file.type)) {
      setErrors((prev) => ({ ...prev, photo: 'Please select a JPEG, PNG, WebP, or GIF image.' }));
      return;
    }
    if (file.size > 5 * 1024 * 1024) {
      setErrors((prev) => ({ ...prev, photo: 'Image must be under 5 MB.' }));
      return;
    }

    try {
      const compressed = await compressImage(file);
      setPhotoFile(compressed);
      setErrors((prev) => ({ ...prev, photo: '' }));
      const reader = new FileReader();
      reader.onload = (ev) => setPhotoPreview(ev.target.result);
      reader.readAsDataURL(compressed);
    } catch {
      setErrors((prev) => ({ ...prev, photo: 'Could not process image. Please try another.' }));
    }
  }

  function validate() {
    const errs = {};
    if (form.firstName.length < 2) errs.firstName = 'First name must be at least 2 characters (letters and numbers only).';
    if (form.firstName.length > 100) errs.firstName = 'First name must be 100 characters or fewer.';
    if (form.lastName.length < 2) errs.lastName = 'Last name must be at least 2 characters (letters and numbers only).';
    if (form.lastName.length > 100) errs.lastName = 'Last name must be 100 characters or fewer.';
    if (form.preferredName && form.preferredName.length < 2)
      errs.preferredName = 'Preferred name must be at least 2 characters (letters and numbers only).';
    if (!form.birthMonth || !form.birthDay) {
      errs.birthDate = 'Birthday month and day are required.';
    } else if (form.birthYear) {
      const bd = new Date(Number(form.birthYear), Number(form.birthMonth) - 1, Number(form.birthDay));
      const today = new Date();
      if (bd >= today) errs.birthDate = 'Birthday must be in the past.';
      if (bd.getFullYear() < 1900) errs.birthDate = 'Birthday must be after 1900.';
    }
    if (form.email && !/^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(form.email))
      errs.email = 'Please enter a valid email address.';
    return errs;
  }

  async function handleSubmit(e) {
    e.preventDefault();
    const errs = validate();
    if (Object.keys(errs).length > 0) { setErrors(errs); return; }

    setSubmitting(true);
    setSubmitError('');

    try {
      const year = form.birthYear || SENTINEL_BIRTH_YEAR;
      const birthDate = `${year}-${String(form.birthMonth).padStart(2, '0')}-${String(form.birthDay).padStart(2, '0')}`;
      await addPerson(groupId, {
        firstName: form.firstName,
        lastName: form.lastName,
        preferredName: form.preferredName || null,
        birthDate,
        email: form.email || null,
        photoFile: photoFile || null,
      });

      onSuccess();
    } catch (err) {
      setSubmitError(err.message || 'Something went wrong. Please try again.');
    } finally {
      setSubmitting(false);
    }
  }

  return (
    <form className="add-person-form" onSubmit={handleSubmit} noValidate>
      <h2 className="add-person-form__title">
        {isFirstMember ? 'Add Your First Member' : 'Add a Member'}
      </h2>

      <div className="add-person-form__photo-section">
        <button
          type="button"
          className="add-person-form__photo-btn"
          onClick={() => fileInputRef.current?.click()}
          aria-label="Choose photo"
        >
          <img
            className="add-person-form__photo-preview"
            src={photoPreview || defaultAvatar}
            alt="Photo preview"
          />
          <span className="add-person-form__photo-label">Choose Photo</span>
        </button>
        <input
          ref={fileInputRef}
          type="file"
          accept="image/jpeg,image/png,image/webp,image/gif"
          onChange={handlePhotoChange}
          className="add-person-form__file-input"
          aria-label="Upload photo"
        />
        <input
          ref={cameraInputRef}
          type="file"
          accept="image/*"
          capture="user"
          onChange={handlePhotoChange}
          className="add-person-form__file-input"
          aria-label="Take photo with camera"
        />
        <button
          type="button"
          className="add-person-form__camera-btn"
          onClick={() => cameraInputRef.current?.click()}
          aria-label="Take photo with camera"
        >
          <svg width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round" aria-hidden="true">
            <path d="M23 19a2 2 0 0 1-2 2H3a2 2 0 0 1-2-2V8a2 2 0 0 1 2-2h4l2-3h6l2 3h4a2 2 0 0 1 2 2z"/>
            <circle cx="12" cy="13" r="4"/>
          </svg>
          Take Photo
        </button>
        {errors.photo && <p className="add-person-form__error" role="alert">{errors.photo}</p>}
      </div>

      <div className="add-person-form__fields">
        <div className="add-person-form__field">
          <label htmlFor="firstName">First Name <span aria-hidden="true">*</span></label>
          <input id="firstName" type="text" value={form.firstName} onChange={setName('firstName')}
            maxLength={100} pattern="[A-Za-z0-9]+" required autoComplete="given-name" />
          {errors.firstName && <span className="add-person-form__error" role="alert">{errors.firstName}</span>}
        </div>

        <div className="add-person-form__field">
          <label htmlFor="lastName">Last Name <span aria-hidden="true">*</span></label>
          <input id="lastName" type="text" value={form.lastName} onChange={setName('lastName')}
            maxLength={100} pattern="[A-Za-z0-9]+" required autoComplete="family-name" />
          {errors.lastName && <span className="add-person-form__error" role="alert">{errors.lastName}</span>}
        </div>

        <div className="add-person-form__field">
          <label htmlFor="preferredName">Preferred Name</label>
          <input id="preferredName" type="text" value={form.preferredName} onChange={setName('preferredName')}
            maxLength={100} pattern="[A-Za-z0-9]*" autoComplete="nickname" />
          {errors.preferredName && <span className="add-person-form__error" role="alert">{errors.preferredName}</span>}
        </div>

        <div className="add-person-form__field">
          <label>
            Birthday <span aria-hidden="true">*</span>
            <span className="add-person-form__field-hint"> (year optional)</span>
          </label>
          <div className="add-person-form__date-row">
            <select id="birthYear" value={form.birthYear} onChange={set('birthYear')} aria-label="Birth year">
              <option value="">Year</option>
              {YEARS.map(y => <option key={y} value={y}>{y}</option>)}
            </select>
            <select id="birthMonth" value={form.birthMonth} onChange={set('birthMonth')} aria-label="Birth month">
              <option value="">Month</option>
              {MONTHS.map((m, i) => <option key={i} value={i + 1}>{m}</option>)}
            </select>
            <select id="birthDay" value={form.birthDay} onChange={set('birthDay')} aria-label="Birth day">
              <option value="">Day</option>
              {daysInMonth.map(d => <option key={d} value={d}>{d}</option>)}
            </select>
          </div>
          {errors.birthDate && <span className="add-person-form__error" role="alert">{errors.birthDate}</span>}
        </div>

        <div className="add-person-form__field">
          <label htmlFor="email">Email</label>
          <input id="email" type="email" value={form.email} onChange={set('email')}
            maxLength={254} autoComplete="email" placeholder="name@example.com" />
          {errors.email && <span className="add-person-form__error" role="alert">{errors.email}</span>}
        </div>
      </div>


      {submitError && <p className="add-person-form__submit-error" role="alert">{submitError}</p>}

      <div className="add-person-form__actions">
        <button type="submit" className="add-person-form__submit" disabled={submitting}>
          {submitting && <span className="add-person-form__spinner" aria-hidden="true" />}
          {submitting ? 'Adding…' : 'Add Member'}
        </button>
        {onCancel && (
          <button type="button" className="add-person-form__cancel" onClick={onCancel} disabled={submitting}>
            Cancel
          </button>
        )}
      </div>
    </form>
  );
}
