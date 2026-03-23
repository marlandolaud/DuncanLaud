import { useState, useRef } from 'react';
import { addPerson } from '../../services/groupApi';
import defaultAvatar from '../../assets/default-avatar.svg';

const ALLOWED_TYPES = ['image/jpeg', 'image/png', 'image/webp', 'image/gif'];

export default function AddPersonForm({ groupId, onSuccess, onCancel, isFirstMember }) {
  const [form, setForm] = useState({
    firstName: '',
    lastName: '',
    preferredName: '',
    birthDate: '',
  });
  const [photoFile, setPhotoFile] = useState(null);
  const [photoPreview, setPhotoPreview] = useState(null);
  const [errors, setErrors] = useState({});
  const [submitting, setSubmitting] = useState(false);
  const [submitError, setSubmitError] = useState('');
  const fileInputRef = useRef(null);

  function set(field) {
    return (e) => {
      setForm((prev) => ({ ...prev, [field]: e.target.value }));
      setErrors((prev) => ({ ...prev, [field]: '' }));
    };
  }

  function handlePhotoChange(e) {
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

    setPhotoFile(file);
    setErrors((prev) => ({ ...prev, photo: '' }));
    const reader = new FileReader();
    reader.onload = (ev) => setPhotoPreview(ev.target.result);
    reader.readAsDataURL(file);
  }

  function validate() {
    const errs = {};
    if (form.firstName.trim().length < 2) errs.firstName = 'First name must be at least 2 characters.';
    if (form.firstName.trim().length > 100) errs.firstName = 'First name must be 100 characters or fewer.';
    if (form.lastName.trim().length < 2) errs.lastName = 'Last name must be at least 2 characters.';
    if (form.lastName.trim().length > 100) errs.lastName = 'Last name must be 100 characters or fewer.';
    if (form.preferredName.trim() && form.preferredName.trim().length < 2)
      errs.preferredName = 'Preferred name must be at least 2 characters.';
    if (!form.birthDate) errs.birthDate = 'Birthday is required.';
    else {
      const bd = new Date(form.birthDate);
      const today = new Date();
      if (bd >= today) errs.birthDate = 'Birthday must be in the past.';
      if (bd.getFullYear() < 1900) errs.birthDate = 'Birthday must be after 1900.';
    }
    return errs;
  }

  async function handleSubmit(e) {
    e.preventDefault();
    const errs = validate();
    if (Object.keys(errs).length > 0) { setErrors(errs); return; }

    setSubmitting(true);
    setSubmitError('');

    try {
      await addPerson(groupId, {
        firstName: form.firstName.trim(),
        lastName: form.lastName.trim(),
        preferredName: form.preferredName.trim() || null,
        birthDate: form.birthDate,
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
        {errors.photo && <p className="add-person-form__error" role="alert">{errors.photo}</p>}
      </div>

      <div className="add-person-form__fields">
        <div className="add-person-form__field">
          <label htmlFor="firstName">First Name <span aria-hidden="true">*</span></label>
          <input id="firstName" type="text" value={form.firstName} onChange={set('firstName')}
            maxLength={100} required autoComplete="given-name" />
          {errors.firstName && <span className="add-person-form__error" role="alert">{errors.firstName}</span>}
        </div>

        <div className="add-person-form__field">
          <label htmlFor="lastName">Last Name <span aria-hidden="true">*</span></label>
          <input id="lastName" type="text" value={form.lastName} onChange={set('lastName')}
            maxLength={100} required autoComplete="family-name" />
          {errors.lastName && <span className="add-person-form__error" role="alert">{errors.lastName}</span>}
        </div>

        <div className="add-person-form__field">
          <label htmlFor="preferredName">Preferred Name</label>
          <input id="preferredName" type="text" value={form.preferredName} onChange={set('preferredName')}
            maxLength={100} autoComplete="nickname" />
          {errors.preferredName && <span className="add-person-form__error" role="alert">{errors.preferredName}</span>}
        </div>

        <div className="add-person-form__field">
          <label htmlFor="birthDate">Birthday <span aria-hidden="true">*</span></label>
          <input id="birthDate" type="date" value={form.birthDate} onChange={set('birthDate')}
            max={new Date().toISOString().split('T')[0]} min="1900-01-01" required />
          {errors.birthDate && <span className="add-person-form__error" role="alert">{errors.birthDate}</span>}
        </div>
      </div>

      {submitError && <p className="add-person-form__submit-error" role="alert">{submitError}</p>}

      <div className="add-person-form__actions">
        <button type="submit" className="add-person-form__submit" disabled={submitting}>
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
