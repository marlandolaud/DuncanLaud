import { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { v7 as uuidv7 } from 'uuid';
import { sanitizeTextInput } from '../../utils/sanitize';

export default function CreateGroupView() {
  const [name, setName] = useState('');
  const [error, setError] = useState('');
  const navigate = useNavigate();

  function handleNameChange(e) {
    setName(sanitizeTextInput(e.target.value));
    setError('');
  }

  function handleCreate(e) {
    e.preventDefault();
    if (name.length < 2) {
      setError('Group name must be at least 2 characters (letters and numbers only).');
      return;
    }
    if (name.length > 100) {
      setError('Group name must be 100 characters or fewer.');
      return;
    }
    const newId = uuidv7();
    // Store name in sessionStorage so GroupLandingView can use it on first creation
    sessionStorage.setItem(`group_name_${newId}`, name);
    navigate(`/mygroup/${newId}`);
  }

  return (
    <div className="group-page">
      <div className="group-create">
        <h1 className="group-create__title">Birthday Groups</h1>
        <p className="group-create__subtitle">
          Create a shareable group to keep track of birthdays for family and friends.
          Anyone with the link can view upcoming birthdays and add new members.
        </p>

        <form className="group-create__form" onSubmit={handleCreate} noValidate>
          <label className="group-create__label" htmlFor="group-name">
            Group Name <span aria-hidden="true">*</span>
          </label>
          <input
            id="group-name"
            className={`group-create__input ${error ? 'group-create__input--error' : ''}`}
            type="text"
            value={name}
            onChange={handleNameChange}
            placeholder="e.g. SmithFamily"
            maxLength={100}
            pattern="[A-Za-z0-9]+"
            required
            autoFocus
          />
          <p className="group-create__hint">Letters and numbers only</p>
          {error && <p className="group-create__error" role="alert">{error}</p>}

          <button className="group-create__btn" type="submit">
            Create My Group
          </button>
        </form>
      </div>
    </div>
  );
}
