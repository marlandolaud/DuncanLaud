import { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { v7 as uuidv7 } from 'uuid';

export default function CreateGroupView() {
  const [name, setName] = useState('');
  const [error, setError] = useState('');
  const navigate = useNavigate();

  function handleCreate(e) {
    e.preventDefault();
    const trimmed = name.trim();
    if (trimmed.length < 2) {
      setError('Group name must be at least 2 characters.');
      return;
    }
    if (trimmed.length > 100) {
      setError('Group name must be 100 characters or fewer.');
      return;
    }
    const newId = uuidv7();
    // Store name in sessionStorage so GroupLandingView can use it on first creation
    sessionStorage.setItem(`group_name_${newId}`, trimmed);
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
            onChange={(e) => { setName(e.target.value); setError(''); }}
            placeholder="e.g. Smith Family"
            maxLength={100}
            required
            autoFocus
          />
          {error && <p className="group-create__error" role="alert">{error}</p>}

          <button className="group-create__btn" type="submit">
            Create My Group
          </button>
        </form>
      </div>
    </div>
  );
}
